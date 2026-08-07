using UnityEngine;

public class TugboatSounds : MonoBehaviour, IClientComponent
{
	[SerializeField]
	private Tugboat tugboat;

	[SerializeField]
	private float roughHalfWidth = 5f;

	[SerializeField]
	private float roughHalfLength = 10f;

	private float soundCullDistanceSq;

	[Header("Engine")]
	[SerializeField]
	private SoundDefinition engineLoopDef;

	private Sound engineLoop;

	private SoundModulation.Modulator engineGainMod;

	private SoundModulation.Modulator enginePitchMod;

	[SerializeField]
	private SoundDefinition engineStartDef;

	[SerializeField]
	private SoundDefinition engineStartBridgeDef;

	[SerializeField]
	private SoundDefinition engineStopDef;

	[SerializeField]
	private SoundDefinition engineStopBridgeDef;

	[SerializeField]
	private float engineGainChangeRate = 1f;

	[SerializeField]
	private float enginePitchChangeRate = 0.5f;

	[SerializeField]
	private Transform engineTransform;

	[SerializeField]
	private Transform bridgeControlsTransform;

	[Header("Water")]
	[SerializeField]
	private SoundDefinition waterIdleDef;

	[SerializeField]
	private SoundDefinition waterSideMovementSlowDef;

	[SerializeField]
	private SoundDefinition waterSideMovementFastDef;

	[SerializeField]
	private SoundDefinition waterSternMovementDef;

	[SerializeField]
	private SoundDefinition waterInteriorIdleDef;

	[SerializeField]
	private SoundDefinition waterInteriorDef;

	[SerializeField]
	private AnimationCurve waterMovementGainCurve;

	[SerializeField]
	private float waterMovementGainChangeRate = 0.5f;

	[SerializeField]
	private AnimationCurve waterDistanceGainCurve;

	private Sound leftWaterSound;

	private SoundModulation.Modulator leftWaterGainMod;

	private Sound rightWaterSound;

	private SoundModulation.Modulator rightWaterGainMod;

	private Sound sternWaterSound;

	private SoundModulation.Modulator sternWaterGainMod;

	[SerializeField]
	private Transform wakeTransform;

	[SerializeField]
	private Vector3 sideSoundLineStern;

	[SerializeField]
	private Vector3 sideSoundLineBow;

	[SerializeField]
	[Header("Ambient")]
	private SoundDefinition ambientActiveLoopDef;

	private Sound ambientActiveSound;

	[SerializeField]
	private SoundDefinition hullGroanDef;

	[SerializeField]
	private float hullGroanCooldown = 1f;

	private float lastHullGroan;

	[SerializeField]
	private SoundDefinition chainRattleDef;

	[SerializeField]
	private float chainRattleCooldown = 1f;

	[SerializeField]
	private Transform[] chainRattleLocations;

	[SerializeField]
	private float chainRattleAngleDeltaThreshold = 1f;

	private float lastChainRattle;

	[Header("Horn")]
	[SerializeField]
	private SoundDefinition hornLoop;

	[SerializeField]
	private SoundDefinition hornStart;

	[SerializeField]
	private SoundDefinition hornStop;

	[Tooltip("The maximum amount of time a looped horn can last. If 0s then this is ignored.")]
	[Min(0f)]
	[SerializeField]
	private float maxHornTime;

	[SerializeField]
	[Tooltip("Used for rate limiting. This defines the maximum number of horn spams that can be invoked one after another.")]
	[Min(0f)]
	private int hornTokenCapacity = 3;

	[Tooltip("Used for rate limiting. This defines how quickly a new token regenerates, permitting another horn honk.")]
	[Min(0f)]
	[SerializeField]
	private float hornTokenRegenerationTime = 1f;

	private Line leftSoundLine;

	private Line rightSoundLine;

	[Header("Runtime")]
	public bool engineOn;

	public bool throttleOn;

	public bool inWater = true;
}
