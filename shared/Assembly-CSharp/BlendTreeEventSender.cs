using UnityEngine;

public class BlendTreeEventSender : StateMachineBehaviour
{
	[Tooltip("The name of the Blend Parameter to monitor.")]
	public string blendParameter = "BlendParameter";

	[Tooltip("The blend value threshold to trigger the event.")]
	public float triggerThreshold = 0.5f;

	[Tooltip("The event name that will be sent via SendMessage.")]
	public string eventName = "OnAnimationEvent";

	[Tooltip("Custom string parameter to send along with the event.")]
	public string eventParameter = "";

	[Tooltip("Should the event only fire once per entry?")]
	public bool fireOnce = true;

	private bool eventTriggered;

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		float num = animator.GetFloat(blendParameter);
		if (num >= triggerThreshold && (!fireOnce || !eventTriggered))
		{
			FireEvent(animator);
			eventTriggered = true;
		}
		if (num < triggerThreshold)
		{
			eventTriggered = false;
		}
	}

	private void FireEvent(Animator animator)
	{
		((Component)animator).gameObject.SendMessage(eventName, (object)eventParameter, (SendMessageOptions)1);
	}
}
