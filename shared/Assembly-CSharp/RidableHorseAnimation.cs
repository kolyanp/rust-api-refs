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
	private float headBlend = 1f;

	[ReadOnly]
	public float headSecondaryBlend = 1f;

	public Transform[] neckBones;

	public AnimationCurve rotationResponsivenessCurve;

	public float yawInertiaFactor = 15f;

	public float maxYawAngle = 10f;

	public Vector2 minMaxStiffness;

	public Vector2 minMaxDamping;

	public float headTurnSpeed = 3.5f;

	public Vector3 headLookOffset = Vector3.zero;

	[Header("Spine")]
	[Range(0f, 1f)]
	[SerializeField]
	private float spineBlend = 0.5f;

	[ReadOnly]
	public float spineSecondaryBlend = 1f;

	public Transform[] spineBones;

	public float spineTurnSpeed = 3.5f;

	public float spineYawInertiaFactor = 150f;

	[ReadOnly]
	[SerializeField]
	private Quaternion[] targetNeckRotations;

	[SerializeField]
	[ReadOnly]
	private Quaternion[] targetSpineRotations;

	[SerializeField]
	[ReadOnly]
	private Vector3[] localSpinePositions;

	[SerializeField]
	[ReadOnly]
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
	}
}
