using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveUIRect : MonoBehaviour, IDragHandler
{
	[SerializeField]
	RectTransform target;

	public void OnDrag(PointerEventData eventData)
	{
		target.anchoredPosition += eventData.delta;
	}
}
