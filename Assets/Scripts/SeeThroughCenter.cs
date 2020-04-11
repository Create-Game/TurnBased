using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeeThroughCenter : MonoBehaviour
{
	[SerializeField]
	Material seeThrough;
	[SerializeField]
	string parameter;
	[SerializeField]
	LayerMask screenBlocker;

	void Update()
	{
		if (seeThrough)
		{
			Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

			int use = 0;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(screenPos), (Camera.main.transform.position - transform.position).magnitude, screenBlocker.value))
			{
				use = 1;
			}

			seeThrough.SetVector(parameter, transform.position);
			seeThrough.SetInt("use", use);

			//if (use > 0)
			//{
			//}
			//else
			//{
			//	SmoothSeeThrough.SetTargetRadius(0f);
			//}
		}
	}
}
