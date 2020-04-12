using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(ItemUI))]
public class DragDropItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
	[HideInInspector]
	public ItemUI dragItem;

	Vector3 offset = new Vector3(2f, 2f, 0f);
	Transform lastParent;
	int lastSibling;
	Vector3 startPosition;
	RectTransform rectT;

	private void Awake()
	{
		rectT = GetComponent<RectTransform>();
		dragItem = GetComponent<ItemUI>();
	}

	public void OnDrag(PointerEventData eventData)
	{
		rectT.position += (Vector3)eventData.delta;
		//throw new System.NotImplementedException();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		startPosition = rectT.position;
		rectT.position += offset;
		lastParent = transform.parent;
		lastSibling = transform.GetSiblingIndex();

		transform.SetParent(GetComponentInParent<Canvas>().transform);
		transform.SetAsLastSibling();

		Image[] images = GetComponentsInChildren<Image>();
		foreach (var image in images)
			image.raycastTarget = false;

		dropped = false;
	}

	bool dropped;

	public void DropHandeled()
	{
		dropped = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		rectT.position -= offset;

		Image[] images = GetComponentsInChildren<Image>();
		foreach (var image in images)
			image.raycastTarget = true;

		StopAllCoroutines();
		StartCoroutine(EndDrag());
	}

	IEnumerator EndDrag()
	{
		yield return new WaitForEndOfFrame();
		if (!dropped)
		{
			transform.SetParent(lastParent);
			transform.SetSiblingIndex(lastSibling);
			transform.position = startPosition;
		}
	}
}
