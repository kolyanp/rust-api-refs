using UnityEngine;

public class RidableHorseAudio : FacepunchBehaviour, IClientComponent
{
	[SerializeField]
	private RidableHorse horse;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	[Space]
	public SoundPlayer breathingSound;

	[SerializeField]
	private SoundDefinition saddleMovementSoundDef;

	[SerializeField]
	private SoundDefinition saddleMovementSoundDefWood;

	[SerializeField]
	private SoundDefinition saddleMovementSoundDefRoadsign;

	[SerializeField]
	private SoundDefinition saddleMovementSoundDefLNY;

	[SerializeField]
	private AnimationCurve saddleMovementGainCurve;

	[SerializeField]
	[Space]
	private MaterialEffect footstepEffects;

	[SerializeField]
	private Transform[] feet;

	[Space]
	[SerializeField]
	private GameObjectRef swimmingSloshEffect;

	[SerializeField]
	private string BaseFolder;

	[Space]
	[SerializeField]
	private SoundDefinition skidLoopSoundDef;

	[SerializeField]
	private AnimationCurve skidLoopGainCurve;
}
