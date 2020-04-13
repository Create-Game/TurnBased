using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Linq;

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

	void FlushEditors()
	{
		desc.editor = null;
		desc.component = null;
		equip.editor = null;
		equip.component = null;
		pickup.editor = null;
		pickup.component = null;
	}

	void OnGUI()
	{
		GameObject obj = Selection.activeGameObject;

		if (!obj)
		{
			GUILayout.Label("Select object graphics you want to make prefab of.", EditorStyles.boldLabel);
			return;
		}

		if (!EditorSceneManager.IsPreviewSceneObject(obj))
		{
			bool isGamePrefab = obj.transform.root.gameObject.tag == "GamePrefab";

			if (isGamePrefab)
			{
				if (PrefabUtility.IsPartOfAnyPrefab(obj))
				{
					obj = obj.transform.root.gameObject;
					if (GUILayout.Button("Edit game prefab"))
					{
						FlushEditors();

						string path = AssetDatabase.GetAssetPath(obj);

						if (PrefabUtility.IsPartOfPrefabInstance(obj))
						{
							path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
						}

						Type t = typeof(UnityEditor.Experimental.SceneManagement.PrefabStageUtility);
						System.Reflection.MethodInfo mi = t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
							.Single(m =>
								m.Name == "OpenPrefab"
								&& m.GetParameters().Length == 1
								&& m.GetParameters()[0].ParameterType == typeof(string)
						);
						mi.Invoke(null, new object[] { path });
					}
				}
				else
				{
					Debug.Log("Not a game prefab object");
				}
			}
			else
			{
				// make prefab
				if (GUILayout.Button("Make Game Prefab"))
				{
					MakePrefab(obj);
				}
			}
		}
		else
		{
			obj = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;

			if (obj.tag == "GamePrefab")
			{
				// edit prefab

				// rename asset
				GUILayout.BeginHorizontal();
				obj.name = GUILayout.TextField(obj.name);
				{
					var stage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
					string currentName = System.IO.Path.GetFileNameWithoutExtension(stage.prefabAssetPath);
					if (obj.name != currentName && GUILayout.Button("Apply", GUILayout.Width(150f)))
					{
						AssetDatabase.RenameAsset(stage.prefabAssetPath, obj.name);
						Selection.SetActiveObjectWithContext(stage.scene.GetRootGameObjects().FirstOrDefault(), null);
						//Selection.SetActiveObjectWithContext(stage.prefabContentsRoot, null);
					}
				}
				GUILayout.EndHorizontal();

				var rig = obj.transform.GetChild(0).Find("Body-rig");
				if (rig && (GUILayout.Button(rig.gameObject.activeSelf ? disableRigButton : enableRigButton)))
				{
					rig.gameObject.SetActive(!rig.gameObject.activeSelf);
				}

				GetForcedComponentEditor(obj, ref desc)?.OnInspectorGUI();

				GetComponentEditor(obj, "It is equippable", "It is not equippable", ref equip)?.OnInspectorGUI();
				GetComponentEditor(obj, "Can pick it up", "Can not pick that up", ref pickup)?.OnInspectorGUI();

				if (GUILayout.Button("Save"))
				{
					var stage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();

					PrefabUtility.SaveAsPrefabAsset(obj, stage.prefabAssetPath);

					try
					{
						//	EditorSceneManager.ClosePreviewScene(stage.scene);
					}
					catch
					{

					}

					//PrefabUtility.ApplyPrefabInstance(obj, InteractionMode.AutomatedAction);
				}
			}
		}
	}

	void MakePrefab(GameObject obj)
	{
		GameObject go = new GameObject(obj.name);

		GameObject child = null;
		if (PrefabUtility.IsPartOfPrefabAsset(obj))
		{
			child = PrefabUtility.InstantiatePrefab(obj) as GameObject;
		}
		else if (PrefabUtility.IsPartOfPrefabInstance(obj))
		{
			child = obj;
		}
		else
		{
			child = obj;
		}

		float rotationFix = -90f;
		if (child.transform.childCount > 0)
			rotationFix = 0f;

		child.name = obj.name;

		Vector3 oldPosition = child.transform.position;
		Vector3 oldRotation = child.transform.rotation.eulerAngles;

		child.transform.SetParent(go.transform);
		child.transform.localPosition = Vector3.zero;
		child.transform.localRotation = Quaternion.identity;
		child.transform.Rotate(rotationFix, 0f, 0f);
		oldRotation.x -= rotationFix;

		go.tag = "GamePrefab";

		GameObject prefabObj = PrefabUtility.SaveAsPrefabAsset(go, System.IO.Path.Combine("Assets", "Prefabs", go.name + ".prefab"));

		DestroyImmediate(go);
		obj = PrefabUtility.InstantiatePrefab(prefabObj) as GameObject;

		obj.transform.position = oldPosition;
		obj.transform.Rotate(oldRotation);

		Selection.SetActiveObjectWithContext(obj, null);
		EditorGUIUtility.PingObject(prefabObj);

		//obj = prefabObj;
	}
}
