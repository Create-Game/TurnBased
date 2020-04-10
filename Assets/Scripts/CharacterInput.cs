using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterInput : MonoBehaviour
{
	[SerializeField]
	SkinnedMeshRenderer characterRenderer;
	[SerializeField]
	NavMeshAgent agent;
	[SerializeField]
	Animator animator;
	[SerializeField]
	UseAnimationEvent animEvent;
	[SerializeField]
	UnityEngine.UI.Text debugUseText;

	ActionContainer actionContainer = new ActionContainer();


	abstract class Action
	{
		public Action child;

		public bool complete { get; private set; }
		public bool failed { get; private set; }

		public abstract void Update(float time);

		public void Complete() { complete = true; }
		public void Fail()
		{
			OnFail();
			failed = true;
		}

		protected virtual void OnFail() { }
	}

	class MoveAction : Action
	{
		NavMeshAgent agent;
		Animator animator;
		Vector3 target;
		float radius;
		float defaultRadius;

		public MoveAction(NavMeshAgent agent, Vector3 target, Animator animator, float radius)
		{
			this.agent = agent;
			this.animator = animator;
			this.target = target;
			this.radius = radius;
			this.defaultRadius = agent.stoppingDistance;

			if (!agent.SetDestination(target))
			{
				Fail();
			}
			else
			{
				defaultRadius = agent.stoppingDistance;
				agent.stoppingDistance = radius;
			}
		}

		public override void Update(float time)
		{
			if ((agent.transform.position - target).sqrMagnitude <= radius)
			{
				Complete();
				animator.SetFloat("speed", 0);
				agent.stoppingDistance = defaultRadius;
			}
			else
			{
				animator.SetFloat("speed", agent.velocity.magnitude);
			}
		}

		protected override void OnFail()
		{
			animator.SetFloat("speed", 0);
			agent.stoppingDistance = defaultRadius;
		}
	}

	class UseAnimationEventAction : Action
	{
		bool executed;
		Animator animator;
		string trigger;
		UseAnimationEvent animEvent;

		public UseAnimationEventAction(Animator animator, string trigger, UseAnimationEvent animEvent)
		{
			this.animator = animator;
			this.trigger = trigger;
			this.animEvent = animEvent;
		}

		public override void Update(float time)
		{
			if (!executed)
			{
				executed = true;
				animator.SetTrigger(trigger);
				animEvent.use += Use;
			}
		}

		void Use()
		{
			animEvent.use -= Use;
			Complete();
		}

		protected override void OnFail()
		{
			animEvent.use -= Use;
		}
	}

	class EventAction : Action
	{
		System.Action eventAction;

		public EventAction(System.Action eventAction)
		{
			this.eventAction = eventAction;
		}

		public override void Update(float time)
		{
			eventAction?.Invoke();
		}
	}

	class ActionContainer
	{
		public Action currentAction { get; private set; }

		public void SetCurrentAction(Action newAction)
		{
			currentAction?.Fail();
			currentAction = newAction;
		}
		public void StopAction()
		{
			currentAction?.Fail();
			currentAction = null;
		}

		public void UpdateAction()
		{
			if (currentAction != null)
			{
				currentAction.Update(Time.deltaTime);

				if (currentAction.complete)
				{
					currentAction = currentAction.child;
				}
				else if (currentAction.failed)
				{
					currentAction = null;
				}
			}
		}
	}

	void Update()
    {
		GameInput.UpdateCharacterInput();

		debugUseText.text = GameInput.useState ? "Use" : "Move";

		if (GameInput.GetUse())
		{
			UpdateUse();
		}
		else if (GameInput.GetMove())
		{
			UpdateMove();
		}

		actionContainer.UpdateAction();
	}

	void UpdateMove()
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit info;
		if (Physics.Raycast(ray, out info))
		{
			actionContainer.SetCurrentAction(new MoveAction(agent, info.point, animator, Mathf.Epsilon));
		}
	}

	void UpdateUse()
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit info;
		if (Physics.Raycast(ray, out info))
		{
			Equippable equippable = info.collider.GetComponent<Equippable>();

			Action move = new MoveAction(agent, info.point, animator, equippable.radius);
			Action use = move.child = new UseAnimationEventAction(animator, "use", animEvent);
			use.child = new EventAction(() => equippable.Equip(characterRenderer));

			actionContainer.SetCurrentAction(move);
		}
	}
}
