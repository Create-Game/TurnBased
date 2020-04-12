using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Item")]
public class Item : ScriptableObject
{
	public Sprite icon;
	public GameObject prefab;
}
