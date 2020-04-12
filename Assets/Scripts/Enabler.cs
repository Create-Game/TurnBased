using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enabler: MonoBehaviour
{
	public static void Enable(MonoBehaviour enableMe)
	{
		enableMe.enabled = true;
	}

	public static void Disable(MonoBehaviour disableMe)
	{
		disableMe.enabled = false;
	}
}
