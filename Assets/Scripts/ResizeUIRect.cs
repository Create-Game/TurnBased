using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResizeUIRect : MonoBehaviour, IPointerDownHandler, IDragHandler
{
	[SerializeField]
	RectTransform target;
	[SerializeField]
	Vector2 pivot;
	[SerializeField]
	Vector2 mult;
	UIRectSize rectSize;


	private void Awake()
	{
		rectSize = target.GetComponent<UIRectSize>();
	}

	public void OnDrag(PointerEventData eventData)
	{
		target.sizeDelta += eventData.delta * mult;
		rectSize?.Clamp();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		var deltaPivot = pivot - target.pivot;
		target.pivot = pivot;
		target.anchoredPosition += target.sizeDelta * deltaPivot;
}
}
