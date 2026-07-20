using UnityEngine;

public class Compass : HeldEntity
{
	private const string WORLDMODEL_NEEDLE_PATH = "w_compass_rig/weapon_parent/weapon_root/needle";

	private const string ADMIRE_ANIMEVENT_TAP1 = "AdmireTap1";

	private const string ADMIRE_ANIMEVENT_TAP2 = "AdmireTap2";

	[Header("Compass")]
	[SerializeField]
	[Tooltip("Change this if it does not feel like the needle direction is accurate.")]
	private float needleYawAngleOffset;

	[SerializeField]
	[Min(0f)]
	private float needleStiffness = 50f;

	[SerializeField]
	[Range(0f, 1f)]
	private float needleDamping = 0.5f;

	[Min(0f)]
	[SerializeField]
	private float maxAcceleration = 1500f;

	[Min(0f)]
	[SerializeField]
	private float maxSpeed = 360f;

	[Tooltip("Random variance in how far off the needle will be from target when first deployed.")]
	[MinMax(0f, 180f)]
	[SerializeField]
	private MinMax startingAngleDelta;

	[SerializeField]
	private float wiggleAmplitude = 0.5f;

	[SerializeField]
	private float wiggleFrequency = 1.5f;

	[Tooltip("Velocity change when the admire animation taps the needle")]
	[Min(0f)]
	[SerializeField]
	private float admireTapNeedleVelocity = 300f;
}
