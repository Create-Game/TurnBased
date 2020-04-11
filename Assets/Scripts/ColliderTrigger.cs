using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColliderTrigger : MonoBehaviour
{
	public UnityEvent enter;
	public UnityEvent exit;

	private void OnTriggerEnter(Collider other)
	{
		enter?.Invoke();
	}

	private void OnTriggerExit(Collider other)
	{
		exit?.Invoke();
	}
}
