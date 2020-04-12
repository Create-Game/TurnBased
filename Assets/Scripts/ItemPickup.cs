using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemPickup: MonoBehaviour
{
	//public UnityEvent pickedUp;
	public float radius;
	public Item item;
	public Transform worldView;

	public void Pickup(Inventory inventory)
	{
		inventory.AddItem(item);
		Destroy(gameObject);
	}

	private void OnDrawGizmosSelected()
	{
		UnityEditor.Handles.color = Color.green;
		UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, radius);
	}
}
