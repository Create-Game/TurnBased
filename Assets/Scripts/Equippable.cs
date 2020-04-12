using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Equippable: MonoBehaviour
{
	public Equipmentizer equip;
	public UnityEvent equipped;

	public void Equip(SkinnedMeshRenderer targetRenderer)
	{
		equip.TargetMeshRenderer = targetRenderer;
		transform.SetParent(targetRenderer.transform.parent);

		equipped?.Invoke();
	}
}
