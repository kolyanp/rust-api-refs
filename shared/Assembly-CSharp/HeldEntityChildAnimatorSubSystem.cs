using UnityEngine;

public abstract class HeldEntityChildAnimatorSubSystem : ChildAnimatorSubSystem
{
	[SerializeField]
	[Tooltip("Second mask for poses that need both arms, blended by the RightArmLayerWeight and BothArmsLayerWeight animator floats")]
	private AvatarMask TwoHandMask;
}
