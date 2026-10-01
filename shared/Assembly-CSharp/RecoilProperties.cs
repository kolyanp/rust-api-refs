using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Recoil Properties")]
public class RecoilProperties : ScriptableObject
{
	public float recoilYawMin;

	public float recoilYawMax;

	public float recoilPitchMin;

	public float recoilPitchMax;

	public float timeToTakeMin;

	public float timeToTakeMax = 0.1f;

	public float ADSScale = 0.5f;

	public float movementPenalty;

	public float clampPitch = float.NegativeInfinity;

	public AnimationCurve pitchCurve = new AnimationCurve(new Keyframe[2]
	{
		new Keyframe(0f, 1f),
		new Keyframe(1f, 1f)
	});

	public AnimationCurve yawCurve = new AnimationCurve(new Keyframe[2]
	{
		new Keyframe(0f, 1f),
		new Keyframe(1f, 1f)
	});

	public bool useCurves;

	public bool curvesAsScalar;

	public int shotsUntilMax = 30;

	public float maxRecoilRadius = 5f;

	[Header("AimCone")]
	public bool overrideAimconeWithCurve;

	public float aimconeCurveScale = 1f;

	[Tooltip("How much to scale aimcone by based on how far into the shot sequence we are (shots v shotsUntilMax)")]
	public AnimationCurve aimconeCurve = new AnimationCurve(new Keyframe[2]
	{
		new Keyframe(0f, 1f),
		new Keyframe(1f, 1f)
	});

	[Tooltip("Randomly select how much to scale final aimcone by per shot, you can use this to weigh a fraction of shots closer to the center")]
	public AnimationCurve aimconeProbabilityCurve = new AnimationCurve(new Keyframe[3]
	{
		new Keyframe(0f, 1f),
		new Keyframe(0.5f, 0f),
		new Keyframe(1f, 1f)
	});

	[Tooltip("Scale the actual final aimcone produced by the ammunition itself if the ammo contains multiple projectiles")]
	public float ammoAimconeScaleMultiProjectile = 1f;

	[Tooltip("Scale the actual final aimcone produced by the ammunition itself if the ammo contains only one projectile")]
	public float ammoAimconeScaleSingleProjectile = 1f;

	public RecoilProperties newRecoilOverride;

	public RecoilProperties GetRecoil()
	{
		if (!((Object)(object)newRecoilOverride != (Object)null))
		{
			return this;
		}
		return newRecoilOverride;
	}

	public RecoilProperties()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected Obj, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected Obj, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected Obj, but got Unknown
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected Obj, but got Unknown
	}
}
