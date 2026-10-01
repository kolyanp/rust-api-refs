using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class SetParameterLerpAnimatorBehaviour : StateMachineBehaviour
{
	public string FloatParameterName;

	public float LerpSpeed;

	public float Target;

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		((StateMachineBehaviour)this).OnStateUpdate(animator, stateInfo, layerIndex, controller);
		if (PlayableExtensions.IsValid<AnimatorControllerPlayable>(controller))
		{
			float num = controller.GetFloat(FloatParameterName);
			num = Mathf.Lerp(num, Target, Time.deltaTime * LerpSpeed);
			controller.SetFloat(FloatParameterName, num);
		}
	}
}
