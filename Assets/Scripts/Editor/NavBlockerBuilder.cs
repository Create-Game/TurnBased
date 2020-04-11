using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public class NavBlockerBuilder: EditorWindow
{
	string myString = "Hello World";
	bool groupEnabled;
	bool myBool = true;
	float myFloat = 1.23f;

	// Add menu named "My Window" to the Window menu
	[MenuItem("Game Tools/Build Nav Blocker")]
	static void Init()
	{
		// Get existing open window or if none, make a new one:
		NavBlockerBuilder window = (NavBlockerBuilder)EditorWindow.GetWindow(typeof(NavBlockerBuilder));
		window.Show();
	}

	const float timeToShow = 8f;

	private void Update()
	{
		for (int i = shownObjects.Count - 1; i >= 0; i--)
		{
			Shown shown = shownObjects[i];

			shown.time += Time.deltaTime;

			if (shown.time >= timeToShow)
			{
				Debug.Log(shown.time);
				shown.shown.SetActive(shown.wasActive);
				shownObjects.RemoveAt(i);
			}
			else
			{
				shownObjects[i] = shown;
			}
		}
	}

	private void OnDisable()
	{
		Debug.Log("disable");
		foreach (var shown in shownObjects)
			shown.shown.SetActive(shown.wasActive);
	}

	struct Shown
	{
		public GameObject shown;
		public bool wasActive;
		public float time;

		public Shown(GameObject shown, bool wasActive) : this()
		{
			this.shown = shown;
			this.wasActive = wasActive;
		}
	}

	List<Shown> shownObjects = new List<Shown>();

	void OnGUI()
	{
		//if (GUILayout.Button("Rebuild mesh"))
		//{
		//	NavMeshBuildSettings settings = new NavMeshBuildSettings();
		//
		//	settings.
		//	NavMeshBuilder.BuildNavMeshData()
		//	NavMeshBuilder.UpdateNavMeshDataAsync()
		//}

		GameObject obj = Selection.activeObject as GameObject;

		if (!obj)
		{
			GUILayout.Label("Select the game object you want to set up.", EditorStyles.boldLabel);
			return;
		}

		Renderer[] rends = obj.GetComponentsInChildren<Renderer>(true);

		foreach (var rend in rends)
		{
			GUILayout.BeginHorizontal();

			StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(rend.gameObject);
			bool hasFlag = flags.HasFlag(StaticEditorFlags.NavigationStatic);

			string showStr = string.Format("Make '{0}' nav static?", rend.name);

			if (shownObjects.Find(x => x.shown == rend.gameObject).shown == null)
			{
				if (GUILayout.Button(showStr, GUILayout.Width(150f)))
				{
					EditorGUIUtility.PingObject(rend);
					(SceneView.sceneViews[0] as SceneView)?.Frame(rend.bounds);
					shownObjects.Add(new Shown(rend.gameObject, rend.gameObject.activeSelf));
					rend.gameObject.SetActive(!rend.gameObject.activeSelf);
				}
			}
			else
			{
				GUILayout.Label(showStr, GUILayout.Width(150f));
			}

			bool flagSet = GUILayout.Toggle(hasFlag, GUIContent.none, GUILayout.Width(15f));

			if (flagSet != hasFlag)
			{
				if (flagSet)
				{
					flags |= StaticEditorFlags.NavigationStatic;
				}
				else
				{
					flags ^= (StaticEditorFlags.NavigationStatic);
				}

				GameObjectUtility.SetStaticEditorFlags(rend.gameObject, flags);
			}

			if (flagSet)
			{
				int area = GameObjectUtility.GetNavMeshArea(rend.gameObject);

				GUILayout.Label(string.Format("walkable?", rend.name), GUILayout.Width(60f));

				bool walk = GUILayout.Toggle(area == 0, GUIContent.none, GUILayout.Width(15f));
				if (walk != (area == 0))
				{
					if (walk)
					{
						area = 0;
					}
					else
					{
						area = 1;
					}

					GameObjectUtility.SetNavMeshArea(rend.gameObject, area);
				}
			}

			GUILayout.EndHorizontal();
		}

		//GUILayout.Label("Base Settings", EditorStyles.boldLabel);
		//myString = EditorGUILayout.TextField("Text Field", myString);
		//
		//groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
		//myBool = EditorGUILayout.Toggle("Toggle", myBool);
		//myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
		//EditorGUILayout.EndToggleGroup();
	}
}
