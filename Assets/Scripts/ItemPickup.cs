using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemPickup: MonoBehaviour
{
	//public UnityEvent pickedUp;
	public float radius;

	[SerializeField]
	Item item;

	public void Pickup(Inventory inventory)
	{
		inventory.AddItem(item);
		Destroy(gameObject);
	}
}
