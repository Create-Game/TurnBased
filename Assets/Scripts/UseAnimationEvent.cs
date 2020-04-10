using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseAnimationEvent : MonoBehaviour
{
	public System.Action use;

	public void AnimationEvent()
	{
		use?.Invoke();
	}
}
