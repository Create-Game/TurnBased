using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyUI : MonoBehaviour, IDropAcceptor
{
	SkinnedMeshRenderer rend;
	Inventory inv;

	public bool ApplyDrop(Item item)
	{
		bool result = false;

		Equippable equip = item.prefab.GetComponent<Equippable>();

		if (equip)
		{
			inv.RemoveItem(item);

			Equippable equipInstance = Instantiate(item.prefab).GetComponent<Equippable>();
			equipInstance.Equip(rend);
			result = true;
		}

		return result;
	}

	public void InitUI(SkinnedMeshRenderer characterRend, Inventory inv)
	{
		rend = characterRend;
		this.inv = inv;
	}
}
