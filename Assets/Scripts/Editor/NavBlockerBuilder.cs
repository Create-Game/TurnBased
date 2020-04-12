using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public class NavBlockerBuilder: EditorWindow
{
	[MenuItem("Game Tools/Build Nav Blocker")]
	static void Init()
	{
		// Get existing open window or if none, make a new one:
		NavBlockerBuilder window = (NavBlockerBuilder)EditorWindow.GetWindow(typeof(NavBlockerBuilder));
		window.Show();
	}

	Editor activeObjectEditor;
	GameObject previewTarget;

	void OnGUI()
	{
		GameObject obj = null;

		if (Selection.activeContext)
		{
			obj = Selection.activeContext as GameObject;

			if (GUILayout.Button("Select " + Selection.activeContext.name))
			{
				Selection.SetActiveObjectWithContext(Selection.activeContext, null);
				if (activeObjectEditor != null)
				{
					DestroyImmediate(activeObjectEditor);
					activeObjectEditor = null;
				}
			}
		}
		else
		{
			obj = Selection.activeObject as GameObject;
			GUILayout.Label(obj.name);
		}

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

			if (GUILayout.Button(showStr, GUILayout.Width(150f)))
			{
				EditorGUIUtility.PingObject(rend);
				Selection.SetActiveObjectWithContext(rend.gameObject, obj);
				if (activeObjectEditor != null)
				{
					DestroyImmediate(activeObjectEditor);
					activeObjectEditor = null;
				}
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

		if (previewTarget != Selection.activeObject)
		{
			if (activeObjectEditor != null)
			{
				DestroyImmediate(activeObjectEditor);
				activeObjectEditor = null;
			}

			if (Selection.activeObject != null)
			{
				previewTarget = Selection.activeObject as GameObject;
				activeObjectEditor = Editor.CreateEditor(Selection.activeObject);
			}
		}
		else if (activeObjectEditor == null)
		{
			if (Selection.activeObject != null)
			{
				previewTarget = Selection.activeObject as GameObject;
				activeObjectEditor = Editor.CreateEditor(Selection.activeObject);
			}
		}

		activeObjectEditor?.OnPreviewGUI(GUILayoutUtility.GetRect(100, 100), EditorStyles.whiteLabel);
	}
}
