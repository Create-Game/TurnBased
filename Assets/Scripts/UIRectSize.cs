using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIRectSize : MonoBehaviour
{
	[SerializeField]
	Vector2 minSize;
	[SerializeField]
	Vector2 maxSize;

	RectTransform rect;

	private void Awake()
	{
		rect = GetComponent<RectTransform>();
	}

	public void Clamp()
	{
		rect.sizeDelta = new Vector2(
			Mathf.Clamp(rect.sizeDelta.x, minSize.x, maxSize.x),
			Mathf.Clamp(rect.sizeDelta.y, minSize.y, maxSize.y));
	}
}
