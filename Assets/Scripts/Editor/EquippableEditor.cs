using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.Events;
using System;

[CustomEditor(typeof(Equippable))]
public class EquippableEditor: Editor
{
	bool defaultEditor;

	public override void OnInspectorGUI()
	{
		//CUSTOM PROPERTY EDITOR
		Equippable equip = (target as Equippable);

		if (equip.equip == null)
		{
			var skins = (target as Equippable).GetComponentsInChildren<SkinnedMeshRenderer>(true);
			foreach (var skin in skins)
			{
				if (GUILayout.Button("Equip " + skin.name))
				{
					Equipmentizer skinEquip = skin.gameObject.GetComponent<Equipmentizer>();
					if (!skinEquip)
						skinEquip = skin.gameObject.AddComponent<Equipmentizer>();

					equip.equip = skinEquip;
					equip.equip.gameObject.SetActive(false);
				}
			}
		}
		else
		{
			GUILayout.BeginHorizontal();

			if (GUILayout.Button(equip.equip.gameObject.activeSelf ? "hide" : "show"))
			{
				equip.equip.gameObject.SetActive(!equip.equip.gameObject.activeSelf);
			}

			equip.equip = EditorGUILayout.ObjectField(equip.equip, typeof(Equippable), true) as Equipmentizer;

			GUILayout.EndHorizontal();
		}

		ItemPickup pickup = equip.GetComponent<ItemPickup>();
		Collider collider = equip.GetComponent<Collider>();

		bool pickupHides = !pickup || !pickup.worldView;
		bool colliderHides = !collider;
		bool skinShows = !equip.equip;

		if (equip.equipped != null)
		{
			for (int i = 0; i < equip.equipped.GetPersistentEventCount(); i++)
			{
				string method = equip.equipped.GetPersistentMethodName(i);
				var targetObject = equip.equipped.GetPersistentTarget(i);

				if (!colliderHides && (method == "set_enabled") && (targetObject == collider))
				{
					colliderHides = true;
				}
				else if (!skinShows && (method == "SetActive") && (targetObject == equip.equip.gameObject))
				{
					skinShows = true;
				}
				else if (!pickupHides && (method == "SetActive") && (targetObject == pickup.worldView.gameObject))
				{
					pickupHides = true;
				}

			}
		}

		if (!skinShows)
			UnityEventTools.AddBoolPersistentListener(equip.equipped, new UnityEngine.Events.UnityAction<bool>(equip.equip.gameObject.SetActive), true);
		if (!pickupHides)
			UnityEventTools.AddBoolPersistentListener(equip.equipped, new UnityEngine.Events.UnityAction<bool>(pickup.worldView.gameObject.SetActive), false);
		if (!colliderHides)
		{
			var test = Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction<bool>), collider, "set_enabled") as
				UnityEngine.Events.UnityAction<bool>;
			UnityEventTools.AddBoolPersistentListener(equip.equipped, test, false);
		}

		defaultEditor = EditorGUILayout.Foldout(defaultEditor, "-- default inspector --");

		if (defaultEditor)
		{
			base.OnInspectorGUI();
		}
	}
}
