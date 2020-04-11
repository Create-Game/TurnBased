using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeeThroughBlocker : MonoBehaviour
{
	[SerializeField]
	float radius;

	public void SetRadius()
	{
		SmoothSeeThrough.SetTargetRadius(radius);
	}

	public void ResetRadius()
	{
		SmoothSeeThrough.ResetRadius();
	}
}
