using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothSeeThrough : MonoBehaviour
{
	[SerializeField]
	Material seeThrough;
	[SerializeField]
	float smoothSpeed;
	[SerializeField]
	float defaultRadius = 3f;


	static float staticDefaultRadius = 0f;
	static float currentRadius = 0f;
	static float targetRadius = 0f;

	public static void SetTargetRadius(float radius)
	{
		targetRadius = radius;
	}

	public static void ResetRadius()
	{
		targetRadius = staticDefaultRadius;
	}

	private void Awake()
	{
		staticDefaultRadius = defaultRadius;
		ResetRadius();
	}

	void FixedUpdate()
	{
		if (Mathf.Abs(currentRadius - targetRadius) > Mathf.Epsilon)
		{
			StopAllCoroutines();
			StartCoroutine(UpdateRadius());
		}
	}

	IEnumerator UpdateRadius()
	{
		while (Mathf.Abs(currentRadius - targetRadius) > Mathf.Epsilon)
		{
			float delta = smoothSpeed * Time.fixedDeltaTime;

			if (Mathf.Abs(currentRadius - targetRadius) < delta)
			{
				currentRadius = targetRadius;
			}
			else
			{
				currentRadius = (currentRadius > targetRadius) ? currentRadius - delta : currentRadius + delta;
			}

			seeThrough.SetFloat("radius", currentRadius);

			yield return new WaitForFixedUpdate();
		}
	}
}
