using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomFloat : StateMachineBehaviour
{
	[System.Serializable]
	struct RandomValue
	{
		public float value;
		[Range(0f, 100f)]
		public int probability;
	}

	[SerializeField]
	string parameter;
	[SerializeField]
	float defaultValue;
	[SerializeField]
	RandomValue[] values;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		float result = defaultValue;

		int random = Random.Range(0, 100);

		int rangeEnd = 0;
		foreach (var value in values)
		{
			rangeEnd += value.probability;
			if (random < rangeEnd)
			{
				result = value.value;
				break;
			}
		}

		animator.SetFloat(parameter, result);
		animator.SetTrigger("next");
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    
	//}

	// OnStateMove is called right after Animator.OnAnimatorMove()
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    // Implement code that processes and affects root motion
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK()
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    // Implement code that sets up animation IK (inverse kinematics)
	//}
}
