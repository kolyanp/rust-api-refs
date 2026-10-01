using UnityEngine;

public class LiquidWeaponEffects : MonoBehaviour
{
	public ParticleSystem RootPS;

	public ParticleSystem EmissionPS;

	public ParticleSystem InnerEmissionPS;

	public LiquidWobble Liquid;

	[Header("Main Stream")]
	public float MinPressureSpeed = 1f;

	public float MaxPressureSpeed = 20f;

	public AnimationCurve PressureSpeedCurve;

	public Vector2 StreamSize = new Vector2(0.04f, 0.08f);

	public AnimationCurve PressureSizeMultiplierCurve;

	[Header("Inner Stream")]
	public float MinPressureInnerSpeed = 1f;

	public float MaxPressureInnerSpeed = 20f;

	public AnimationCurve InnerPressureSpeedCurve;

	public Vector2 InnerStreamSize = new Vector2(0.02f, 0.02f);

	public AnimationCurve InnerPressureSizeMultiplierCurve;

	[Header("Misc")]
	public bool UseImpactSplashEffect;

	public GameObjectRef ImpactSplashEffect;

	public float ImpactSplashEffectInterval = 0.1f;

	public float FillSpeed = 1f;

	[Header("Audio")]
	public bool firstPersonSounds;

	public SoundDefinition shootStartSoundDef;

	public SoundDefinition shootLoopSoundDef;

	public SoundDefinition shootLowPressureLoopSoundDef;

	public SoundDefinition impactStartSoundDef;

	public SoundDefinition impactLoopSoundDef;

	public LiquidWeaponEffects()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
	}
}
