using FIMSpace.FProceduralAnimation;
using UnityEngine;

[DefaultExecutionOrder(-1302)]
public class RidableHorseAnimation : EntityComponent<RidableHorse>, IClientComponent
{
	[SerializeField]
	private RidableHorse horse;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	public LegsAnimator legsAnimator;

	[Space]
	[SerializeField]
	private Transform backHipTransform;

	[SerializeField]
	private Vector3 skiddingHipPosition;

	[SerializeField]
	private Vector3 skiddingHipRotation;

	[Header("Head")]
	[Range(0f, 1f)]
	[SerializeField]
	private float headBlend;

	[ReadOnly]
	public float headSecondaryBlend;

	public Transform[] neckBones;

	public AnimationCurve rotationResponsivenessCurve;

	public float yawInertiaFactor;

	public float maxYawAngle;

	public Vector2 minMaxStiffness;

	public Vector2 minMaxDamping;

	public float headTurnSpeed;

	public Vector3 headLookOffset;

	[SerializeField]
	[Header("Spine")]
	[Range(0f, 1f)]
	private float spineBlend;

	[ReadOnly]
	public float spineSecondaryBlend;

	public Transform[] spineBones;

	public float spineTurnSpeed;

	public float spineYawInertiaFactor;

	[ReadOnly]
	[SerializeField]
	private Quaternion[] targetNeckRotations;

	[SerializeField]
	[ReadOnly]
	private Quaternion[] targetSpineRotations;

	[ReadOnly]
	[SerializeField]
	private Vector3[] localSpinePositions;

	[ReadOnly]
	[SerializeField]
	private Vector3 originalHipPosition;

	[SerializeField]
	[ReadOnly]
	private Quaternion originalHipRotation;

	public Vector3 spineOffset;

	private float HeadBlend => headBlend * headSecondaryBlend;

	private float SpineBlend => spineBlend * spineSecondaryBlend;

	public RidableHorseAnimation()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		headBlend = 1f;
		headSecondaryBlend = 1f;
		yawInertiaFactor = 15f;
		maxYawAngle = 10f;
		headTurnSpeed = 3.5f;
		headLookOffset = Vector3.zero;
		spineBlend = 0.5f;
		spineSecondaryBlend = 1f;
		spineTurnSpeed = 3.5f;
		spineYawInertiaFactor = 150f;
		base._002Ector();
	}
}
