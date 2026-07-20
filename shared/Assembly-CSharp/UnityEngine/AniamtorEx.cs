using UnityEngine.Animations;

namespace UnityEngine;

public static class AniamtorEx
{
	public static void SetFloatFixed(this Animator animator, int id, float value, float dampTime, float deltaTime)
	{
		if (value == 0f)
		{
			float num = animator.GetFloat(id);
			if (num == 0f)
			{
				return;
			}
			if (num < float.Epsilon)
			{
				animator.SetFloat(id, 0f);
				return;
			}
		}
		animator.SetFloat(id, value, dampTime, deltaTime);
	}

	public static void SetFloatFixed(this AnimatorControllerPlayable playable, int id, float value, float dampTime, float deltaTime)
	{
		float num = ((AnimatorControllerPlayable)(ref playable)).GetFloat(id);
		if (value == 0f)
		{
			if (num == 0f)
			{
				return;
			}
			if (num < float.Epsilon)
			{
				((AnimatorControllerPlayable)(ref playable)).SetFloat(id, 0f);
				return;
			}
		}
		float num2 = Mathf.Lerp(num, value, deltaTime / Mathf.Max(dampTime, 0.0001f));
		((AnimatorControllerPlayable)(ref playable)).SetFloat(id, num2);
	}

	public static void SetBoolChecked(this AnimatorControllerPlayable playable, int id, bool value)
	{
		if (((AnimatorControllerPlayable)(ref playable)).GetBool(id) != value)
		{
			((AnimatorControllerPlayable)(ref playable)).SetBool(id, value);
		}
	}

	public static void SetBoolChecked(this Animator animator, int id, bool value)
	{
		if (animator.GetBool(id) != value)
		{
			animator.SetBool(id, value);
		}
	}
}
