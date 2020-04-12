using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
	[SerializeField]
	Transform content;

	public GameObject itemUIPrefab;
	Inventory currentInventory;

	public void Show(SkinnedMeshRenderer characterRend, Inventory inv)
	{
		Debug.Assert(inv);
		currentInventory = inv;
		currentInventory.changed += OnInventoryChanged;

		OnInventoryChanged();

		IDropAcceptor[] acceptors = GetComponentsInChildren<IDropAcceptor>();
		foreach (var acceptor in acceptors)
			acceptor.InitUI(characterRend, inv);

		gameObject.SetActive(true);
	}

	public void Close()
	{
		Debug.Assert(currentInventory);
		currentInventory.changed -= OnInventoryChanged;
		currentInventory = null;

		OnInventoryChanged();

		gameObject.SetActive(false);
	}

	void OnInventoryChanged()
	{
		List<Item> itemsToAdd = new List<Item>();
		if (currentInventory)
			itemsToAdd.AddRange(currentInventory.GetItems());

		for (int i = content.childCount - 1; i >= 0; i--)
		{
			ItemUI ui = content.GetChild(i).GetComponent<ItemUI>();
			if (!itemsToAdd.Contains(ui.GetItem()))
			{
				Destroy(ui.gameObject);
			}
			else
			{
				itemsToAdd.Remove(ui.GetItem());
			}
		}

		foreach (Item item in itemsToAdd)
		{
			ItemUI ui = Instantiate(itemUIPrefab.gameObject).GetComponent<ItemUI>();
			ui.SetItem(item);
			ui.transform.SetParent(content);
			ui.GetComponent<RectTransform>().localScale = Vector3.one;
		}
	}
}
