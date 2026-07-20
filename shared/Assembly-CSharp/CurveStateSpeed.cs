using UnityEngine;
using UnityEngine.Animations;

public class CurveStateSpeed : StateMachineBehaviour
{
	public AnimationCurve SpeedCurve = AnimationCurve.Constant(0f, 1f, 1f);

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		((StateMachineBehaviour)this).OnStateUpdate(animator, stateInfo, layerIndex, controller);
		float speed = 1f;
		if (!animator.IsInTransition(layerIndex))
		{
			speed = SpeedCurve.Evaluate(((AnimatorStateInfo)(ref stateInfo)).normalizedTime);
		}
		animator.speed = speed;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((StateMachineBehaviour)this).OnStateExit(animator, stateInfo, layerIndex);
		animator.speed = 1f;
	}
}
