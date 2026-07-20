using UnityEngine;

public class AnimationFlagHandler : MonoBehaviour
{
	public Animator animator;

	public bool forceUpdateIfDisabled;

	[UnityEvent]
	public void SetBoolTrue(string name)
	{
		animator.SetBool(name, true);
		TryForceAnimation();
	}

	[UnityEvent]
	public void SetBoolFalse(string name)
	{
		animator.SetBool(name, false);
		TryForceAnimation();
	}

	private void TryForceAnimation()
	{
		if (forceUpdateIfDisabled && !((Behaviour)animator).isActiveAndEnabled)
		{
			((Behaviour)animator).enabled = true;
			animator.Update(10f);
			SingletonComponent<InvokeHandler>.Instance.Invoke(DisableAnimator, 2f);
		}
	}

	private void DisableAnimator()
	{
		if (!((Object)(object)animator == (Object)null))
		{
			((Behaviour)animator).enabled = false;
		}
	}
}
