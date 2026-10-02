using UnityEngine;

public class GenericChildAnimatorSubSystem : ChildAnimatorSubSystem
{
	[Tooltip("Bool parameter driven by the player's aiming state, leave empty to ignore")]
	[SerializeField]
	private string AimingParameter = string.Empty;

	[Tooltip("Bool parameter driven by a flag on the entity this system is attached to, leave empty to ignore")]
	[SerializeField]
	private string FlagParameter = string.Empty;

	[SerializeField]
	private BaseEntity.Flags Flag;

	[Tooltip("Trigger parameter fired when the entity broadcasts TriggerSignal, leave empty to ignore")]
	[SerializeField]
	private string TriggerParameter = string.Empty;

	[SerializeField]
	private BaseEntity.Signal TriggerSignal;

	[Tooltip("Second mask for poses that need both arms, blended by the RightArmLayerWeight and BothArmsLayerWeight animator floats")]
	[SerializeField]
	private AvatarMask TwoHandMask;
}
