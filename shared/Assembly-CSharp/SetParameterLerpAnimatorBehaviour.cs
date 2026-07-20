using UnityEngine;
using UnityEngine.Animations;

public class SetParameterLerpAnimatorBehaviour : StateMachineBehaviour
{
	public string FloatParameterName;

	public float LerpSpeed;

	public float Target;

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		((StateMachineBehaviour)this).OnStateUpdate(animator, stateInfo, layerIndex, controller);
		float num = ((AnimatorControllerPlayable)(ref controller)).GetFloat(FloatParameterName);
		num = Mathf.Lerp(num, Target, Time.deltaTime * LerpSpeed);
		((AnimatorControllerPlayable)(ref controller)).SetFloat(FloatParameterName, num);
	}
}
