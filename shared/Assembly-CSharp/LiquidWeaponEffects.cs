using UnityEngine;

public class LiquidWeaponEffects : MonoBehaviour
{
	public ParticleSystem RootPS;

	public ParticleSystem EmissionPS;

	public ParticleSystem InnerEmissionPS;

	public LiquidWobble Liquid;

	[Header("Main Stream")]
	public float MinPressureSpeed;

	public float MaxPressureSpeed;

	public AnimationCurve PressureSpeedCurve;

	public Vector2 StreamSize;

	public AnimationCurve PressureSizeMultiplierCurve;

	[Header("Inner Stream")]
	public float MinPressureInnerSpeed;

	public float MaxPressureInnerSpeed;

	public AnimationCurve InnerPressureSpeedCurve;

	public Vector2 InnerStreamSize;

	public AnimationCurve InnerPressureSizeMultiplierCurve;

	[Header("Misc")]
	public bool UseImpactSplashEffect;

	public GameObjectRef ImpactSplashEffect;

	public float ImpactSplashEffectInterval;

	public float FillSpeed;

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
		MinPressureSpeed = 1f;
		MaxPressureSpeed = 20f;
		StreamSize = new Vector2(0.04f, 0.08f);
		MinPressureInnerSpeed = 1f;
		MaxPressureInnerSpeed = 20f;
		InnerStreamSize = new Vector2(0.02f, 0.02f);
		ImpactSplashEffectInterval = 0.1f;
		FillSpeed = 1f;
		((MonoBehaviour)this)._002Ector();
	}
}
