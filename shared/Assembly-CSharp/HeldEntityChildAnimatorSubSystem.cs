using UnityEngine;

public abstract class HeldEntityChildAnimatorSubSystem : ChildAnimatorSubSystem
{
	[Tooltip("Second mask for poses that need both arms, blended by the RightArmLayerWeight and BothArmsLayerWeight animator floats")]
	[SerializeField]
	private AvatarMask TwoHandMask;
}
