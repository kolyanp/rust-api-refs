using UnityEngine;

public class MeleeWeaponAnimationSubSystem : ChildAnimatorSubSystem
{
	[SerializeField]
	private AvatarMask TwoHandMask;

	[SerializeField]
	[Tooltip("Fade out both layers by this curve based on the PlayerVelocityCap")]
	private AnimationCurve PlayerVelocityMultiplier = AnimationCurve.Linear(0f, 1f, 1f, 0.8f);

	[SerializeField]
	private float PlayerVelocityCap = 2f;

	[SerializeField]
	private bool HasAttackHitAnims;
}
