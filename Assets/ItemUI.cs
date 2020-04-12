using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
	Item item;

	public void SetItem(Item item)
	{
		this.item = item;
		if (item)
		{
			Image icon = transform.GetChild(0).GetComponent<Image>();
			icon.sprite = item.icon;
			icon.color = Color.white;
		}
	}

	public Item GetItem()
	{
		return item;
	}
}
