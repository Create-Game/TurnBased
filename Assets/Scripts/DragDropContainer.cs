using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropContainer : MonoBehaviour, IDropHandler
{
	[SerializeField]
	Transform retarget;
	[SerializeField]
	bool centered;

	RectTransform rect;

	private void Awake()
	{
		if (!retarget)
			retarget = transform;

		rect = retarget.GetComponent<RectTransform>();
	}

	public void OnDrop(PointerEventData eventData)
	{
		DragDropItem item = eventData.pointerDrag.GetComponent<DragDropItem>();

		if (!item)
			return;

		item.DropHandeled();
		item.transform.SetParent(retarget);

		RectTransform itemRect = item.GetComponent<RectTransform>();

		if (centered)
		{
			itemRect.anchoredPosition = new Vector2(rect.sizeDelta.x / 2f, - rect.sizeDelta.y / 2f + itemRect.sizeDelta.y / 2f);
		}
		else
		{
			PlaceInside(itemRect);
		}
	}

	void PlaceInside(RectTransform itemRect)
	{
		float maxX = rect.sizeDelta.x - itemRect.sizeDelta.x / 2f;
		float minX = itemRect.sizeDelta.x / 2f;

		float maxY = 0f;
		float minY = -rect.sizeDelta.y + itemRect.sizeDelta.y;

		Vector2 newPosition = itemRect.anchoredPosition;

		if (itemRect.anchoredPosition.x < minX)
			newPosition.x = minX;
		else if (itemRect.anchoredPosition.x > maxX)
			newPosition.x = maxX;

		if (itemRect.anchoredPosition.y < minY)
			newPosition.y = minY;
		else if (itemRect.anchoredPosition.y > maxY)
			newPosition.y = maxY;

		itemRect.anchoredPosition = newPosition;
	}
}
