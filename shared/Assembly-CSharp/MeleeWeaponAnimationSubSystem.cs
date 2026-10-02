using UnityEngine;

public class MeleeWeaponAnimationSubSystem : ChildAnimatorSubSystem
{
	[SerializeField]
	private AvatarMask TwoHandMask;

	[Tooltip("Fade out both layers by this curve based on the PlayerVelocityCap")]
	[SerializeField]
	private AnimationCurve PlayerVelocityMultiplier = AnimationCurve.Linear(0f, 1f, 1f, 0.8f);

	[SerializeField]
	private float PlayerVelocityCap = 2f;

	[Tooltip("Optional prop-bone-only mask. Held at full weight so the velocity fade never pulls the held item out of the hand.")]
	[SerializeField]
	private AvatarMask PropMask;

	[SerializeField]
	private bool HasAttackHitAnims;
}
