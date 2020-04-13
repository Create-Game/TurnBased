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

	const string startGamePrefabInfo = "You can:\n- select Model in scene/assets to create game prefab\n- select existing game prefab (object tagged GamePrefab) to edit";

	private void OnSelectionChange()
	{
		Repaint();
		if (Selection.activeObject)
			prefabName = Selection.activeObject.name;
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
		desc.DestroyEditor();
		equip.DestroyEditor();
		pickup.DestroyEditor();
	}

	string prefabName;

	void OnGUI()
	{
		var currentPrefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
		GameObject obj = currentPrefabStage != null ? currentPrefabStage.prefabContentsRoot : Selection.activeGameObject;

		if (!obj)
		{
			EditorGUILayout.HelpBox(startGamePrefabInfo, MessageType.Info);
			return;
		}

		if (!EditorSceneManager.IsPreviewSceneObject(obj))
		{
			// это не режим редактирования префаба (т.е. либо asset, либо на сцене)
			bool isGamePrefab = obj.transform.root.gameObject.tag == "GamePrefab";

			if (isGamePrefab)
			{
				// это игровой префаб (либо asset, либо на сцене)
				if (PrefabUtility.IsPartOfPrefabAsset(obj) || PrefabUtility.IsPartOfPrefabInstance(obj))
				{
					// проверяем, существует ли префаб ассет
					string path = AssetDatabase.GetAssetPath(obj);

					if (PrefabUtility.IsPartOfPrefabInstance(obj))
					{
						// этот префаб на сцене - находим его источник в ассетах (если он существует, т.к. редактировать надо именно его)
						path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
					}

					if (!string.IsNullOrEmpty(path))
					{
						// юнити префаб для этого игрового префаба существует в ассетах, будем редактировать его
						//obj = obj.transform.root.gameObject;
						if (GUILayout.Button("Edit game prefab"))
						{
							FlushEditors();

							// открываем префаб из ассетов на редактирование
							Type t = typeof(UnityEditor.Experimental.SceneManagement.PrefabStageUtility);
							System.Reflection.MethodInfo mi = t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
								.Single(m =>
									m.Name == "OpenPrefab"
									&& m.GetParameters().Length == 1
									&& m.GetParameters()[0].ParameterType == typeof(string)
							);
							mi.Invoke(null, new object[] { path });

							UnityEditor.Experimental.SceneManagement.PrefabStage.prefabStageClosing -= OnSceneClosing;
							UnityEditor.Experimental.SceneManagement.PrefabStage.prefabStageClosing += OnSceneClosing;
						}
					}
					else
					{
						EditorGUILayout.HelpBox("Selected object has no prefab asset file associated with it. Check if it should be a GamePrefab, and if it should - make proper Unity prefab out of this object.", MessageType.Warning);
					}
				}
				else
				{
					// этот игровой префаб не юнити префаб, т.е. связь разорвана была видимо
					// можно предупредить об этом
					EditorGUILayout.HelpBox("Selected object is GamePrefab, but not the Unity prefab. Check if GamePrefab tag on this object is requiered, and if it is - make proper Unity prefab out of this object.", MessageType.Warning);
				}
			}
			else
			{
				// этот объект - не игровой префаб, можно сделать его игровым префабом
				if (PrefabUtility.IsPartOfModelPrefab(obj))
				{
					if (GUILayout.Button("Make Generic Game Prefab"))
						MakePrefab(obj);

					GUILayout.BeginHorizontal();
					prefabName = GUILayout.TextField(prefabName);

					if (GUILayout.Button("Make Cloth Game Prefab"))
					{
						string prefabPath = AssetDatabase.GetAssetPath(MakePrefab(obj));

						GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

						// Создаем одежду - надо сначала проверить условия

						// Скрываем риг, создаем pickup и item для него
						GetRig(prefabRoot)?.SetActive(false);

						prefabRoot.AddComponent<BoxCollider>();

						var pickup = prefabRoot.AddComponent<ItemPickup>();
						pickup.item = PickupItemEditor.CreateItem(prefabRoot.name, prefabPath);
						pickup.item.icon = GetDefaultIcon();

						// радиус равен bounds сетки пополам
						// fit collider позже

						var equip = prefabRoot.AddComponent<Equippable>();
						// hidden не ищем, т.к. риг скрыли уже
						equip.equip = prefabRoot.GetComponentInChildren<SkinnedMeshRenderer>().gameObject.AddComponent<Equipmentizer>();
						equip.equip.gameObject.SetActive(false);

						var lying = prefabRoot.GetComponentsInChildren<Renderer>().First((x) => !x.GetComponent<Equipmentizer>());
						if (lying)
						{
							pickup.worldView = lying.transform;
						}

						EquippableEditor.SetupEvents(equip);

						ColliderFitter.FitCollider(prefabRoot, false);

						var bounds = pickup.worldView.GetComponent<Renderer>().bounds;
						if (bounds.extents.x > bounds.extents.z)
							pickup.radius = bounds.extents.x;
						else
							pickup.radius = bounds.extents.z;

						PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
						PrefabUtility.UnloadPrefabContents(prefabRoot);
					}

					GUILayout.EndHorizontal();
				}
				else
				{
					EditorGUILayout.HelpBox(startGamePrefabInfo, MessageType.Info);
				}
			}
		}
		else
		{
			// мы в режиме редактирования префаба - редактируем game prefab

			// rename game prefa (asset)
			GUILayout.BeginHorizontal();
			{
				obj.name = GUILayout.TextField(obj.name);
				string currentName = System.IO.Path.GetFileNameWithoutExtension(currentPrefabStage.prefabAssetPath);
				if (obj.name != currentName && GUILayout.Button("Apply", GUILayout.Width(150f)))
				{
					AssetDatabase.RenameAsset(currentPrefabStage.prefabAssetPath, obj.name);
				}
			}
			GUILayout.EndHorizontal();

			// Body-rig - для персонажей и одежды
			var rig = GetRig(obj);
			if (rig && (GUILayout.Button(rig.activeSelf ? disableRigButton : enableRigButton)))
			{
				rig.SetActive(!rig.activeSelf);
			}

			// Компоненты игрового префаба, которые могут быть на нем
			GetForcedComponentEditor(obj, ref desc)?.OnInspectorGUI();
			GetComponentEditor(obj, "It is equippable", "It is not equippable", ref equip)?.OnInspectorGUI();
			GetComponentEditor(obj, "Can pick it up", "Can not pick that up", ref pickup)?.OnInspectorGUI();
		}
	}

	GameObject GetRig(GameObject root)
	{
		return root.transform.GetChild(0).Find("Body-rig")?.gameObject;
	}

	void OnSceneClosing(UnityEditor.Experimental.SceneManagement.PrefabStage scene)
	{
		UnityEditor.Experimental.SceneManagement.PrefabStage.prefabStageClosing -= OnSceneClosing;
		var stage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
		PrefabUtility.SaveAsPrefabAsset(stage.prefabContentsRoot, stage.prefabAssetPath);
		FlushEditors();
	}

	Sprite GetDefaultIcon()
	{
		return AssetDatabase.LoadAssetAtPath(System.IO.Path.Combine("Assets", "Textures", "default_error_icon.png"), typeof(Sprite)) as Sprite;
	}

	GameObject MakePrefab(GameObject obj)
	{
		GameObject go = new GameObject(string.IsNullOrEmpty(prefabName) ? obj.name : prefabName);
		var desc = go.AddComponent<Description>();
		desc.caption = go.name;
		desc.text = string.Format("This is {0}. Nothing special.", go.name);

		Vector3 oldPosition = obj.transform.position;
		Vector3 oldRotation = obj.transform.rotation.eulerAngles;

		GameObject child = null;
		if (PrefabUtility.IsPartOfPrefabAsset(obj))
		{
			child = PrefabUtility.InstantiatePrefab(obj) as GameObject;
		}
		else
		{
			var loaded = AssetDatabase.LoadAssetAtPath(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj), typeof(GameObject));
			child = PrefabUtility.InstantiatePrefab(loaded) as GameObject;
		}

		float rotationFix = -90f;
		if (child.transform.childCount > 0)
			rotationFix = 0f;

		child.name = obj.name;

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

		return prefabObj;
	}
}
