using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ColliderFitter : MonoBehaviour
{
	[MenuItem("Game Tools/Collider Fitter")]
	static void FitCollider()
	{
		if (Selection.activeGameObject)
		{
			FitCollider(Selection.activeGameObject, true);
		}
	}

	public static void FitCollider(GameObject obj, bool includeHidden)
	{
		if (obj)
		{
			BoxCollider bc = obj.GetComponent<BoxCollider>();
			if (bc)
			{
				Renderer[] rends = bc.GetComponentsInChildren<Renderer>(includeHidden);

				Vector3 max = Vector3.negativeInfinity;
				Vector3 min = Vector3.positiveInfinity;
				foreach (var rend in rends)
				{
					Debug.Log(rend.bounds);
					Vector3 rendMin = rend.bounds.min;
					Vector3 rendMax = rend.bounds.max;

					if (min.x > rendMin.x)
						min.x = rendMin.x;
					if (min.y > rendMin.y)
						min.y = rendMin.y;
					if (min.z > rendMin.z)
						min.z = rendMin.z;
					if (max.x < rendMax.x)
						max.x = rendMax.x;
					if (max.y < rendMax.y)
						max.y = rendMax.y;
					if (max.z < rendMax.z)
						max.z = rendMax.z;
				}

				Undo.RecordObject(bc, "Fit collider");

				bc.center = (max + min) / 2f;
				bc.size = max - min;
				bc.center -= bc.transform.position;
			}
		}
	}
}
