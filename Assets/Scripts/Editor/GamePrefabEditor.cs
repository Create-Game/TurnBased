using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using System;

public class GamePrefabEditor: EditorWindow
{
	[MenuItem("Game Tools/Prefab Editor")]
	static void Init()
	{
		GamePrefabEditor window = (GamePrefabEditor)EditorWindow.GetWindow(typeof(GamePrefabEditor));
		window.Show();
	}

	// пока - структура префаба:
	// 1. родительский элемент - тут коллайдер и все скрипты взаимодействия
	// 2. графика отдельно (child 0)

	Editor activeObjectEditor;
	GameObject previewTarget;

	GUIContent enableRigButton;
	GUIContent disableRigButton;

	private void OnEnable()
	{
		enableRigButton = new GUIContent("Enable Body-rig (disabled)", "Should be DISABLED for any NON character prefab.");
		disableRigButton = new GUIContent("Disable Body-rig (enabled)", "Should be DISABLED for any NON character prefab.");
	}

	struct GamePrefabComponentEditor
	{
		public System.Type componentType;
		public Component component;
		public Editor editor;

		public GamePrefabComponentEditor(Type componentType): this()
		{
			this.componentType = componentType;
		}

		public void CreateEditor(Component comp)
		{
			component = comp;

			if (component != null)
			{
				editor = Editor.CreateEditor(component);
			}
		}

		public void DestroyEditor()
		{
			component = null;
			DestroyImmediate(editor);
			editor = null;
		}
	}

	Editor GetForcedComponentEditor(GameObject obj, ref GamePrefabComponentEditor componentEditor)
	{
		System.Type componentType = componentEditor.componentType;
		Component comp = obj.GetComponent(componentType);

		if (!comp)
		{
			comp = obj.AddComponent(componentType);
		}

		if (componentEditor.component != comp)
		{
			componentEditor.DestroyEditor();
			componentEditor.CreateEditor(comp);
		}

		return componentEditor.editor;
	}

	Editor GetComponentEditor(GameObject obj, string createText, string destroyText, ref GamePrefabComponentEditor componentEditor)
	{
		System.Type componentType = componentEditor.componentType;
		Component comp = obj.GetComponent(componentType);

		if (!comp)
		{
			if (GUILayout.Button(createText))
			{
				comp = obj.AddComponent(componentType);
			}
		}
		else
		{
			if (GUILayout.Button(destroyText))
			{
				DestroyImmediate(comp);
				componentEditor.DestroyEditor();
			}
		}

		if (componentEditor.component != comp)
		{
			componentEditor.DestroyEditor();
			componentEditor.CreateEditor(comp);
		}

		return componentEditor.editor;
	}

	GamePrefabComponentEditor desc = new GamePrefabComponentEditor(typeof(Description));
	GamePrefabComponentEditor equip = new GamePrefabComponentEditor(typeof(Equippable));
	GamePrefabComponentEditor pickup = new GamePrefabComponentEditor(typeof(ItemPickup));

	void OnGUI()
	{
		GameObject obj = Selection.activeGameObject;

		if (!obj)
		{
			GUILayout.Label("Select object graphics you want to make prefab of.", EditorStyles.boldLabel);
			return;
		}

		if (obj.tag == "GamePrefab")
		{
			// edit prefab

			obj.name = GUILayout.TextField(obj.name);

			var rig = obj.transform.GetChild(0).Find("Body-rig");
			if (rig && (GUILayout.Button(rig.gameObject.activeSelf ? disableRigButton : enableRigButton)))
			{
				rig.gameObject.SetActive(!rig.gameObject.activeSelf);
			}

			GetForcedComponentEditor(obj, ref desc)?.OnInspectorGUI();

			GetComponentEditor(obj, "It is equippable", "It is not equippable", ref equip)?.OnInspectorGUI();
			GetComponentEditor(obj, "Can pick it up", "Can not pick that up", ref pickup)?.OnInspectorGUI();

			/*
			Renderer[] rends = obj.GetComponentsInChildren<Renderer>();

			foreach (var rend in rends)
			{
				EditorGUILayout.ObjectField(rend, typeof(Renderer), true);
			}

			// preview prefab
			if ((activeObjectEditor == null) || (previewTarget != Selection.activeObject))
			{
				previewTarget = Selection.activeObject as GameObject;

				if (previewTarget != null)
					activeObjectEditor = Editor.CreateEditor(Selection.activeObject);
				else
					activeObjectEditor = null;
			}

			activeObjectEditor?.OnPreviewGUI(GUILayoutUtility.GetRect(100, 100), EditorStyles.whiteLabel);
			*/
		}

		else
		{
			// make prefab
			if (GUILayout.Button("Make Game Prefab", GUILayout.Width(150f)))
			{
				GameObject go = new GameObject(obj.name);

				GameObject child = Instantiate(obj);
				child.name = obj.name;

				child.transform.SetParent(go.transform);

				go.tag = "GamePrefab";

				Selection.SetActiveObjectWithContext(go, null);
				EditorGUIUtility.PingObject(go);
			}
		}
	}
}
