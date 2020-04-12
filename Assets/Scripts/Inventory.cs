using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	List<Item> items = new List<Item>();

	public System.Action changed;

	public void AddItem(Item item)
	{
		items.Add(item);
		changed?.Invoke();
	}

	public void RemoveItem(Item item)
	{
		items.Remove(item);
		changed?.Invoke();
	}

	public bool HasItem(Item item)
	{
		return items.Contains(item);
	}

	public Item[] GetItems()
	{
		return items.ToArray();
	}
}
