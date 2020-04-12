using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using System;

[CustomEditor(typeof(ItemPickup))]
public class PickupItemEditor: Editor
{
	bool defaultEditor;
	Editor colliderEditor;

	string itemName;

	public override void OnInspectorGUI()
	{
		//CUSTOM PROPERTY EDITOR
		ItemPickup pickup = (target as ItemPickup);

		// create or assign item

		GUILayout.BeginHorizontal();

		if (pickup.item)
			itemName = pickup.item.name;
		else if (string.IsNullOrEmpty(itemName))
			itemName = "New Item";
		itemName = EditorGUILayout.TextField(itemName);
		if (pickup.item)
			pickup.item.name = itemName;
		pickup.item = EditorGUILayout.ObjectField(pickup.item, typeof(Item), false) as Item;
		GUILayout.EndHorizontal();

		if (!pickup.item)
		{
			if (GUILayout.Button("Create new item"))
			{
				string path = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine("Assets", "Items", itemName + ".asset"));

				Item newItem = ScriptableObject.CreateInstance<Item>();
				// FIXME: временно, тестовый вариант
				newItem.prefab = pickup.gameObject;

				pickup.item = newItem;

				AssetDatabase.CreateAsset(newItem, path);
			}
		}

		pickup.radius = EditorGUILayout.FloatField("radius", pickup.radius);


		if (pickup.worldView == null)
		{
			var rends = pickup.GetComponentsInChildren<Renderer>(true);
			foreach (var rend in rends)
			{
				if (!rend.GetComponent<Equipmentizer>() && 
					GUILayout.Button("Make '" + rend.name + "' lay down"))
				{
					pickup.worldView = rend.transform;
					pickup.worldView.gameObject.SetActive(true);
				}
			}
		}
		else
		{
			GUILayout.BeginHorizontal();

			if (GUILayout.Button(pickup.worldView.gameObject.activeSelf ? "hide" : "show"))
			{
				pickup.worldView.gameObject.SetActive(!pickup.worldView.gameObject.activeSelf);
			}

			pickup.worldView = EditorGUILayout.ObjectField(pickup.worldView, typeof(Transform), true) as Transform;

			GUILayout.EndHorizontal();
		}

		BoxCollider collider = pickup.gameObject.GetComponent<BoxCollider>();
		if (collider == null)
		{
			collider = pickup.gameObject.AddComponent<BoxCollider>();
		}

		if (GUILayout.Button("Fit collider"))
			ColliderFitter.FitCollider(collider.gameObject, false);


		defaultEditor = EditorGUILayout.Foldout(defaultEditor, "-- default inspector --");

		if (defaultEditor)
		{
			base.OnInspectorGUI();

			if (colliderEditor == null)
				colliderEditor = Editor.CreateEditor(collider);
			colliderEditor.OnInspectorGUI();
		}
		else
		{
			if (colliderEditor == null)
				DestroyImmediate(colliderEditor);
		}
	}
}
