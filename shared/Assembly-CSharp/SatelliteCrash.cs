using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Serialization;

public class SatelliteCrash : BaseCombatEntity
{
	private Vector3 descentStartPos;

	private float descentSecondsToTake;

	private float descentSecondsTaken;

	private const float phase1NetInterval = 0.5f;

	private float phase1NetTimer;

	private const float DebugDrawLife = 0.1f;

	private const float SafetyDespawnMargin = 30f;

	private const float ImpactDebugSphereSeconds = 180f;

	private const float DescentLogInterval = 5f;

	private float descentLogTimer;

	private readonly FlyoverCurve curve = new FlyoverCurve();

	private Vector3 descentVelocity;

	private const float phase2NetInterval = 0.25f;

	private float phase2NetTimer;

	private const float EntryDirCenterDeadzone = 25f;

	private static readonly Phrase CrashEventTitlePhrase;

	private static readonly Phrase CrashEventBodyPhrase;

	private bool hasCrashed;

	private bool computerNotified;

	private Vector3 crashTarget;

	private NetworkableId controlComputerId;

	private bool hasNoOwner;

	private int fuelAtLaunch = -1;

	private int fuelAtLockIn = -1;

	private const float DebrisVelocityInheritFraction = 0.25f;

	private const float CrateScatterMinRadius = 3f;

	private const float CrateScatterMaxRadius = 10f;

	private const float CrateDropHeight = 0.5f;

	private const int GroundedScatterAttempts = 8;

	private const int ConstructionMask = 136347904;

	private const int VegetationMask = 1141374977;

	public const float CrateClearanceRadius = 0.75f;

	private const int CrateClearanceMask = 1210263809;

	private const int CrateFloorMask = 8454145;

	private const float CrateFloorRayHeight = 30f;

	private const float CrateClearanceSkin = 0.1f;

	private static readonly Collider[] crateClearanceBuffer;

	private const float CrateMaxFloorSlope = 30f;

	[Header("Movement")]
	public float speed = 80f;

	[Header("Descent")]
	public float finalDescentSpeed = 106f;

	[Tooltip("Fraction of an in-game day the descent takes (1.0 = full day)")]
	public float descentDayFraction = 1f;

	[Header("Crash Effects")]
	public GameObjectRef explosionEffect;

	public GameObjectRef fireBall;

	public SoundDefinition orbitalHumLoopSound;

	public SoundDefinition reentryWhooshLoopSound;

	public SoundDefinition closeApproachSound;

	[Range(50f, 500f)]
	public float reentryWhooshStartDistance = 200f;

	[Tooltip("Crate prefabs to drop at the crash site. Each spawned crate picks one of these at random.")]
	public GameObjectRef[] cratesToDrop;

	public GameObjectRef debrisFieldMarker;

	public GameObjectRef impactSoundEffect;

	public GameObjectRef reentryTrailEffect;

	[Tooltip("Visual effect played at the crash position when the satellite hits the ground")]
	public GameObjectRef groundImpactEffect;

	[Tooltip("Loot budget at 1.0x mass scale, in crate-equivalents. Multiplied by the mass-to-loot curve; the result spawns as crates up to Max Crates Per Crash, with any overflow going into extra items per crate.")]
	[Header("Crash Config")]
	[FormerlySerializedAs("maxCratesToSpawn")]
	public int baselineCrateSpawnCount = 6;

	public int maxFireballs = 10;

	public float terrainImpactOffset = 5f;

	public float safetyDespawnTime = 120f;

	public float crateLifetimeMinutes = 60f;

	public float debrisMarkerDurationMinutes = 30f;

	public float startHeight = 200f;

	[Header("Loot Scaling")]
	[Tooltip("Maps satellite mass (kg) to the loot multiplier. The multiplier scales fireball count and the total crate loot budget (crate count up to Max Crates Per Crash, overflow into extra items per crate). Flat outside the first/last key.")]
	public AnimationCurve massToLootScale = AnimationCurve.Linear(1000f, 0.5f, 6000f, 3f);

	[Tooltip("Hard cap on crates spawned per crash, regardless of the loot multiplier. Budget beyond this goes into extra items per crate.")]
	public int maxCratesPerCrash = 5;

	[Header("Impact Entity")]
	public GameObjectRef impactEntityPrefab;

	[HideInInspector]
	public float satelliteMass = 2000f;

	[Header("Visual")]
	public Transform visualTransform;

	public GameObject dishArtwork;

	public GameObject billboardArtwork;

	public GameObject reentryArtwork;

	public GameObject crashingArtwork;

	public static float DayLengthMinutes
	{
		get
		{
			if (!((Object)(object)TOD_Sky.Instance != (Object)null))
			{
				return 30f;
			}
			return TOD_Sky.Instance.Components.Time.DayLengthInMinutes;
		}
	}

	private float FinalDescentSeconds => Mathf.Clamp(Satellite.final_descent_seconds, 1f, descentSecondsToTake);

	private float FinalDescentSpeed => Mathf.Max(1f, (Satellite.final_descent_speed > 0f) ? Satellite.final_descent_speed : finalDescentSpeed);

	private static float FlyoverDiveDistance => Mathf.Max(100f, Satellite.flyover_dive_distance);

	private float FuelFractionAtLockIn
	{
		get
		{
			if (fuelAtLaunch <= 0 || fuelAtLockIn < 0)
			{
				return 0.5f;
			}
			return Mathf.Clamp01((float)fuelAtLockIn / (float)fuelAtLaunch);
		}
	}

	public bool IsDescending { get; private set; }

	public override Vector3 GetLocalVelocityServer()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return descentVelocity;
	}

	public void InitOrbit(SatelliteData sat, Vector3 target, NetworkableId computerId = default(NetworkableId), int fuelRemainingAtLockIn = -1)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		crashTarget = target;
		satelliteMass = sat.mass;
		controlComputerId = computerId;
		hasNoOwner = !((NetworkableId)(ref computerId)).IsValid;
		fuelAtLaunch = sat.fuel;
		fuelAtLockIn = fuelRemainingAtLockIn;
		IsDescending = true;
		descentSecondsToTake = GetScheduledDescentSeconds(descentDayFraction);
		descentSecondsTaken = 0f;
		descentStartPos = ComputeDescentStartPos(target);
		((Component)this).transform.position = Phase1StartPos();
	}

	public static float GetScheduledDescentSeconds(float dayFraction)
	{
		return Mathf.Max((Satellite.descent_seconds > 0f) ? Satellite.descent_seconds : (DayLengthMinutes * 60f * dayFraction), Mathf.Max(Satellite.final_descent_seconds, 10f));
	}

	private Vector3 Phase1StartPos()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = crashTarget - descentStartPos;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		return descentStartPos - normalized * Satellite.phase1_extra_distance;
	}

	private Vector3 ComputeDescentStartPos(Vector3 target)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ComputeEntryDir(target);
		if (Satellite.flyover_altitude > 0f)
		{
			return ComputeFlyoverEntryPos(target, val);
		}
		float num = Mathf.Clamp(Satellite.descent_angle, 0f, 45f) * (MathF.PI / 180f);
		Vector3 val2 = Vector3.up * Mathf.Cos(num) + val * Mathf.Sin(num);
		float num2 = FinalDescentSpeed * FinalDescentSeconds;
		return target + val2 * num2;
	}

	private Vector3 ComputeFlyoverEntryPos(Vector3 target, Vector3 entryDir)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		float num = FinalDescentSpeed * FinalDescentSeconds;
		float flyover_altitude = Satellite.flyover_altitude;
		float flyoverDiveDistance = FlyoverDiveDistance;
		float num2 = Mathf.Max(num, flyoverDiveDistance * 2f);
		for (int i = 0; i < 4; i++)
		{
			Vector3 p = target + entryDir * num2 + Vector3.up * flyover_altitude;
			Vector3 p2 = target + entryDir * flyoverDiveDistance + Vector3.up * flyover_altitude;
			float num3 = FlyoverCurve.ApproximateLength(p, p2, target);
			num2 = Mathf.Max(flyoverDiveDistance * 2f, num2 * (num / Mathf.Max(1f, num3)));
		}
		return target + entryDir * num2 + Vector3.up * flyover_altitude;
	}

	private static Vector3 ComputeEntryDir(Vector3 target)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = TerrainMeta.Center - target;
		val.y = 0f;
		if (((Vector3)(ref val)).sqrMagnitude < 625f)
		{
			return RandomHorizontalDir();
		}
		return ((Vector3)(ref val)).normalized;
	}

	public void InitDirectDescent(Vector3 target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		crashTarget = target;
		descentSecondsToTake = Mathf.Max(1f, Satellite.final_descent_seconds);
		descentStartPos = ComputeDescentStartPos(target);
		((Component)this).transform.position = descentStartPos;
	}

	public void InitLateDescent(SatelliteData sat, Vector3 target, float secondsToImpact)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		crashTarget = target;
		crashTarget.y = TerrainMeta.HeightMap.GetHeight(crashTarget);
		satelliteMass = sat.mass;
		hasNoOwner = true;
		descentSecondsToTake = GetScheduledDescentSeconds(descentDayFraction);
		descentStartPos = ComputeDescentStartPos(crashTarget);
		float num = Mathf.Clamp(secondsToImpact, 1f, FinalDescentSeconds);
		descentSecondsTaken = descentSecondsToTake - num;
		((Component)this).transform.position = ScheduledPhase2Pos(num);
	}

	private Vector3 ScheduledPhase2Pos(float secondsLeft)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01(secondsLeft / FinalDescentSeconds);
		if (Satellite.flyover_altitude > 0f)
		{
			Vector3 val = descentStartPos - crashTarget;
			val.y = 0f;
			val = ((((Vector3)(ref val)).sqrMagnitude > 0.01f) ? ((Vector3)(ref val)).normalized : RandomHorizontalDir());
			Vector3 p = crashTarget + val * FlyoverDiveDistance;
			p.y = descentStartPos.y;
			FlyoverCurve flyoverCurve = new FlyoverCurve();
			flyoverCurve.Build(descentStartPos, p, crashTarget, FinalDescentSeconds);
			return flyoverCurve.EvalAtDistance(flyoverCurve.TotalLength * (1f - num));
		}
		return Vector3.Lerp(crashTarget, descentStartPos, num);
	}

	private void EnterDescentState()
	{
		if (!IsDescending)
		{
			StartFinalDescent();
		}
	}

	private void LogDescentProgress(float dt, string phase, float secondsToImpact)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (Satellite.debug)
		{
			descentLogTimer += dt;
			if (!(descentLogTimer < 5f))
			{
				descentLogTimer = 0f;
				Vector3 position = ((Component)this).transform.position;
				Debug.Log((object)string.Format("[SatelliteCrash] {0} — impact in {1:F0}s, pos=({2:F1}, {3:F1}, {4:F1})", new object[5] { phase, secondsToImpact, position.x, position.y, position.z }));
			}
		}
	}

	private void Phase1Tick(float dt)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		descentSecondsTaken += dt;
		DrawDescentDebugLine();
		LogDescentProgress(dt, "Phase 1 (orbital slide)", descentSecondsToTake - descentSecondsTaken);
		float num = descentSecondsToTake - FinalDescentSeconds;
		if (descentSecondsTaken < num)
		{
			float num2 = ((num > 0.01f) ? Mathf.Clamp01(descentSecondsTaken / num) : 1f);
			((Component)this).transform.position = Vector3.Lerp(Phase1StartPos(), descentStartPos, num2);
			phase1NetTimer += dt;
			if (phase1NetTimer >= 0.5f)
			{
				phase1NetTimer = 0f;
				SendNetworkUpdate();
			}
		}
		else
		{
			((Component)this).transform.position = descentStartPos;
			TransitionToCrash();
		}
	}

	private void TransitionToCrash()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		IsDescending = false;
		SendNetworkUpdate();
		if (explosionEffect.isValid)
		{
			Effect.server.Run(explosionEffect.resourcePath, ((Component)this).transform.position, Vector3.up, null, broadcast: true);
		}
		crashTarget.y = TerrainMeta.HeightMap.GetHeight(crashTarget);
		StartFinalDescent();
	}

	private void StartFinalDescent()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		float time = safetyDespawnTime;
		if (crashTarget != Vector3.zero)
		{
			float num = Mathf.Clamp(descentSecondsToTake - descentSecondsTaken, 1f, FinalDescentSeconds);
			if (ShouldFlyover())
			{
				StartFlyoverCurve(num);
			}
			else
			{
				Vector3 val = crashTarget - ((Component)this).transform.position;
				descentVelocity = ((Vector3)(ref val)).normalized * (((Vector3)(ref val)).magnitude / num);
			}
			((Component)this).transform.rotation = Quaternion.LookRotation(((Vector3)(ref descentVelocity)).normalized);
			time = Mathf.Max(safetyDespawnTime, num + 30f);
			DrawImpactDebugSphere(crashTarget);
		}
		else
		{
			Vector3 position = ((Component)this).transform.position;
			CalculateEntry(position, out var startPos, out var velocity);
			((Component)this).transform.position = startPos;
			descentVelocity = velocity;
			((Component)this).transform.rotation = Quaternion.LookRotation(((Vector3)(ref velocity)).normalized);
			DrawImpactDebugSphere(position);
		}
		Invoke(SafetyDespawn, time);
	}

	private bool ShouldFlyover()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (Satellite.flyover_altitude <= 0f || crashTarget == Vector3.zero)
		{
			return false;
		}
		Vector3 val = ((Component)this).transform.position - crashTarget;
		val.y = 0f;
		return ((Vector3)(ref val)).magnitude > FlyoverDiveDistance * 1.5f;
	}

	private void StartFlyoverCurve(float duration)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		Vector3 val = position - crashTarget;
		val.y = 0f;
		val = ((((Vector3)(ref val)).sqrMagnitude > 0.01f) ? ((Vector3)(ref val)).normalized : RandomHorizontalDir());
		Vector3 val2 = crashTarget + val * FlyoverDiveDistance;
		val2.y = position.y;
		curve.Build(position, val2, crashTarget, duration);
		Vector3 val3 = val2 - position;
		descentVelocity = ((Vector3)(ref val3)).normalized * (curve.TotalLength / curve.Duration);
	}

	private void FixedUpdate()
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isServer || hasCrashed)
		{
			return;
		}
		if (IsDescending)
		{
			Phase1Tick(Time.fixedDeltaTime);
			return;
		}
		descentSecondsTaken += Time.fixedDeltaTime;
		DrawDescentDebugLine();
		if (curve.Active)
		{
			curve.Elapsed += Time.fixedDeltaTime;
			Vector3 val = curve.EvalAtDistance(curve.ElapsedArcDistance());
			descentVelocity = (val - ((Component)this).transform.position) / Time.fixedDeltaTime;
			phase2NetTimer += Time.fixedDeltaTime;
			if (phase2NetTimer >= 0.25f)
			{
				phase2NetTimer = 0f;
				SendNetworkUpdate();
			}
		}
		if (crashTarget != Vector3.zero)
		{
			Vector3 val2 = crashTarget - ((Component)this).transform.position;
			float num = ((Vector3)(ref descentVelocity)).magnitude * Time.fixedDeltaTime;
			LogDescentProgress(Time.fixedDeltaTime, "Phase 2 (final descent)", curve.Active ? (curve.Duration - curve.Elapsed) : (((Vector3)(ref val2)).magnitude / Mathf.Max(0.01f, ((Vector3)(ref descentVelocity)).magnitude)));
			if (((Vector3)(ref val2)).magnitude <= num || Vector3.Dot(val2, descentVelocity) <= 0f)
			{
				((Component)this).transform.position = crashTarget;
				PerformCrash();
				return;
			}
		}
		else
		{
			float height = TerrainMeta.HeightMap.GetHeight(((Component)this).transform.position);
			LogDescentProgress(Time.fixedDeltaTime, "Phase 2 (final descent)", (((Component)this).transform.position.y - height - terrainImpactOffset) / Mathf.Max(0.01f, 0f - descentVelocity.y));
			if (((Component)this).transform.position.y <= height + terrainImpactOffset)
			{
				PerformCrash();
				return;
			}
		}
		Transform transform = ((Component)this).transform;
		transform.position += descentVelocity * Time.fixedDeltaTime;
		if (((Vector3)(ref descentVelocity)).sqrMagnitude > 1f)
		{
			((Component)this).transform.rotation = Quaternion.LookRotation(((Vector3)(ref descentVelocity)).normalized);
		}
	}

	private void SafetyDespawn()
	{
		if (!hasCrashed)
		{
			NotifyControlComputer(crashed: false);
			Kill();
		}
	}

	private void CalculateEntry(Vector3 targetPos, out Vector3 startPos, out Vector3 velocity)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		startPos = targetPos + RandomHorizontalDir() * startHeight + Vector3.up * startHeight;
		Vector3 val = targetPos - startPos;
		velocity = ((Vector3)(ref val)).normalized * speed;
	}

	private void DrawImpactDebugSphere(Vector3 target)
	{
	}

	private void DrawDescentDebugLine()
	{
	}

	public override void Save(SaveInfo info)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.satelliteCrash = Pool.Get<SatelliteCrash>();
		info.msg.satelliteCrash.satelliteMass = satelliteMass;
		info.msg.satelliteCrash.isDescending = IsDescending;
		info.msg.satelliteCrash.descentStartPos = descentStartPos;
		info.msg.satelliteCrash.descentSecondsToTake = descentSecondsToTake;
		info.msg.satelliteCrash.descentSecondsTaken = descentSecondsTaken;
		info.msg.satelliteCrash.crashTarget = crashTarget;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		hasCrashed = false;
		computerNotified = false;
		globalBroadcast = true;
		if (!Application.isLoadingSave)
		{
			EnterDescentState();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		EnterDescentState();
	}

	public override void OnKilled()
	{
		base.OnKilled();
		NotifyControlComputer(crashed: false);
	}

	private void NotifyControlComputer(bool crashed)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (computerNotified)
		{
			return;
		}
		computerNotified = true;
		if (!hasNoOwner)
		{
			SatelliteControlComputer satelliteControlComputer = (((NetworkableId)(ref controlComputerId)).IsValid ? (BaseNetworkable.serverEntities.Find(controlComputerId) as SatelliteControlComputer) : SatelliteControlComputer.ActiveDescending);
			if ((Object)(object)satelliteControlComputer != (Object)null)
			{
				satelliteControlComputer.OnSatelliteCrashed(crashed);
			}
		}
	}

	private void PerformCrash()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isServer || hasCrashed)
		{
			return;
		}
		hasCrashed = true;
		CancelInvoke(SafetyDespawn);
		Vector3 position = ((Component)this).transform.position;
		position.y = TerrainMeta.HeightMap.GetHeight(position);
		((Component)this).transform.position = position;
		Vector3 scatterVelocity = descentVelocity * 0.25f;
		ClearArea(position);
		float num = massToLootScale.Evaluate(satelliteMass);
		int count = Mathf.RoundToInt((float)maxFireballs * num);
		float num2 = (float)baselineCrateSpawnCount * num;
		int num3 = Mathf.Clamp(Mathf.RoundToInt(num2), 1, Mathf.Max(1, maxCratesPerCrash));
		float lootScale = num2 / (float)num3;
		if (impactSoundEffect.isValid)
		{
			Effect.server.Run(impactSoundEffect.resourcePath, position, Vector3.up, null, broadcast: true);
		}
		Quaternion rot;
		if (debrisFieldMarker.isValid)
		{
			GameManager server = GameManager.server;
			string resourcePath = debrisFieldMarker.resourcePath;
			Vector3 pos = position;
			rot = default(Quaternion);
			BaseEntity baseEntity = server.CreateEntity(resourcePath, pos, rot);
			if ((Object)(object)baseEntity != (Object)null)
			{
				baseEntity.Spawn();
				((Component)baseEntity).SendMessage("SetDuration", (object)debrisMarkerDurationMinutes, (SendMessageOptions)1);
			}
		}
		if (groundImpactEffect.isValid)
		{
			Vector3 val = descentVelocity;
			val.y = 0f;
			float num4;
			if (!(((Vector3)(ref val)).sqrMagnitude > 0.01f))
			{
				num4 = 0f;
			}
			else
			{
				rot = Quaternion.LookRotation(val);
				num4 = ((Quaternion)(ref rot)).eulerAngles.y;
			}
			float num5 = num4;
			Effect.server.Run(groundImpactEffect.resourcePath, position, Vector3.up, null, broadcast: true, null, Mathf.RoundToInt(num5));
		}
		PooledList<Collider> val2 = Pool.Get<PooledList<Collider>>();
		try
		{
			SpawnFireballs(position, scatterVelocity, count, (List<Collider>)(object)val2);
			SatelliteCrashRemains satelliteCrashRemains = null;
			if (impactEntityPrefab.isValid)
			{
				BaseEntity baseEntity2 = GameManager.server.CreateEntity(impactEntityPrefab.resourcePath, position, GetRemainsRotation(position, descentVelocity));
				if ((Object)(object)baseEntity2 != (Object)null)
				{
					satelliteCrashRemains = baseEntity2 as SatelliteCrashRemains;
					if ((Object)(object)satelliteCrashRemains != (Object)null)
					{
						satelliteCrashRemains.tooHotSeconds = Satellite.wreck_fire_duration;
						satelliteCrashRemains.thrusterModuleFuelFraction = FuelFractionAtLockIn;
					}
					baseEntity2.Spawn();
				}
			}
			SpawnLootCrates(position, (List<Collider>)(object)val2, num3, lootScale, satelliteCrashRemains);
			NotifyControlComputer(crashed: true);
			Kill(DestroyMode.Gib);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private static Quaternion GetRemainsRotation(Vector3 crashPos, Vector3 impactVelocity)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 up = Vector3.up;
		if (CrashSpotSearch.SampleFootprintPlane(crashPos, Satellite.site_footprint_radius, out var _, out var normal, out var _))
		{
			up = normal;
		}
		Vector3 val = impactVelocity;
		val.y = 0f;
		if (((Vector3)(ref val)).sqrMagnitude < 0.01f)
		{
			val = Vector3.forward;
		}
		return QuaternionEx.LookRotationForcedUp(((Vector3)(ref val)).normalized, up);
	}

	private void ClearArea(Vector3 crashPos)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		float kill_radius = Satellite.kill_radius;
		if (base.isServer && !(kill_radius <= 0f))
		{
			List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
			Vis.Entities(crashPos, kill_radius, list, 1277853953, (QueryTriggerInteraction)2);
			KillPlayers(list);
			KillConstruction(list);
			KillVegetation(list);
			Pool.FreeUnmanaged<BaseEntity>(ref list);
		}
	}

	private void KillConstruction(List<BaseEntity> entities)
	{
		foreach (BaseEntity entity in entities)
		{
			if (!((Object)(object)entity == (Object)null) && !entity.isClient && !entity.IsDestroyed && !((Object)(object)entity == (Object)(object)this) && ((1 << ((Component)entity).gameObject.layer) & 0x8208100) != 0)
			{
				entity.Kill(DestroyMode.Gib);
			}
		}
	}

	private void KillVegetation(List<BaseEntity> entities)
	{
		foreach (BaseEntity entity in entities)
		{
			if (!((Object)(object)entity == (Object)null) && !entity.isClient && !entity.IsDestroyed && (entity is ResourceEntity || entity is CollectibleEntity || entity is BushEntity))
			{
				entity.Kill();
			}
		}
	}

	private void KillPlayers(List<BaseEntity> entities)
	{
		foreach (BaseEntity entity in entities)
		{
			if (entity is BasePlayer basePlayer && !basePlayer.IsDead() && !basePlayer.IsNpc && !basePlayer.isClient)
			{
				basePlayer.Hurt(1000f, DamageType.Explosion, this);
			}
		}
	}

	public static Vector3 ClampAwayFromMonuments(Vector3 pos, float exclusionDistance)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TerrainMeta.Path == (Object)null || TerrainMeta.Path.Monuments == null)
		{
			return pos;
		}
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			if (!((Object)(object)monument == (Object)null) && monument.Distance(pos) < exclusionDistance)
			{
				Vector3 val = pos - ((Component)monument).transform.position;
				val.y = 0f;
				if (((Vector3)(ref val)).sqrMagnitude < 0.01f)
				{
					val = Vector3.forward;
				}
				((Vector3)(ref val)).Normalize();
				pos = ((Component)monument).transform.position + val * exclusionDistance;
				pos.y = TerrainMeta.HeightMap.GetHeight(pos);
			}
		}
		return pos;
	}

	private static Vector3 FindGroundedScatterPosition(Vector3 crashPos, float minRadius, float maxRadius)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 8; i++)
		{
			Vector3 pos = crashPos + RandomHorizontalDir() * Random.Range(minRadius, maxRadius);
			pos = FindCrateFloor(pos, out var floorCollider, out var floorNormal);
			if (IsCrateSpotUsable(pos, floorCollider, floorNormal))
			{
				return pos;
			}
		}
		Collider floorCollider2;
		Vector3 floorNormal2;
		return FindCrateFloor(crashPos, out floorCollider2, out floorNormal2);
	}

	private static Vector3 RandomHorizontalDir()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.Range(0f, 360f) * (MathF.PI / 180f);
		return new Vector3(Mathf.Cos(num), 0f, Mathf.Sin(num));
	}

	private static Vector3 RandomUpwardScatter()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector3 onUnitSphere = Random.onUnitSphere;
		onUnitSphere.y = Mathf.Abs(onUnitSphere.y);
		return onUnitSphere;
	}

	private static void ConfigureScatterRigidbody(Rigidbody rb, float mass, float drag, float angularDrag, bool useGravity)
	{
		rb.useGravity = useGravity;
		rb.mass = mass;
		rb.drag = drag;
		rb.angularDrag = angularDrag;
		rb.interpolation = (RigidbodyInterpolation)1;
		rb.collisionDetectionMode = (CollisionDetectionMode)2;
	}

	private void SpawnFireballs(Vector3 crashPos, Vector3 scatterVelocity, int count, List<Collider> fireColliders)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (!fireBall.isValid)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(fireBall.resourcePath, crashPos);
			if (!((Object)(object)baseEntity == (Object)null))
			{
				Vector3 val = RandomUpwardScatter();
				((Component)baseEntity).transform.position = crashPos + Vector3.up * 1.5f + val * Random.Range(1f, 8f);
				baseEntity.Spawn();
				baseEntity.SetVelocity(scatterVelocity + val * Random.Range(3f, 15f));
				Collider component = ((Component)baseEntity).GetComponent<Collider>();
				if ((Object)(object)component != (Object)null)
				{
					fireColliders.Add(component);
				}
			}
		}
	}

	private static void GetShuffledCrateSpawnPoints(SatelliteCrashRemains remains, List<Transform> points)
	{
		if ((Object)(object)remains == (Object)null || remains.crateSpawnPoints == null)
		{
			return;
		}
		Transform[] crateSpawnPoints = remains.crateSpawnPoints;
		foreach (Transform val in crateSpawnPoints)
		{
			if ((Object)(object)val != (Object)null)
			{
				points.Add(val);
			}
		}
		for (int num = points.Count - 1; num > 0; num--)
		{
			int num2 = Random.Range(0, num + 1);
			int i = num;
			int index = num2;
			Transform val2 = points[num2];
			Transform val3 = points[num];
			Transform val4 = (points[i] = val2);
			val4 = (points[index] = val3);
		}
	}

	private static Vector3 FindCrateFloor(Vector3 pos, out Collider floorCollider, out Vector3 floorNormal)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		floorCollider = null;
		float height = TerrainMeta.HeightMap.GetHeight(pos);
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(new Vector3(pos.x, Mathf.Max(pos.y, height) + 30f, pos.z), Vector3.down, ref val, 60f, 8454145, (QueryTriggerInteraction)1))
		{
			floorCollider = ((RaycastHit)(ref val)).collider;
			floorNormal = ((RaycastHit)(ref val)).normal;
			return ((RaycastHit)(ref val)).point;
		}
		floorNormal = TerrainMeta.HeightMap.GetNormal(pos);
		pos.y = height;
		return pos;
	}

	private static bool IsCrateSpotUsable(Vector3 floorPos, Collider floorCollider, Vector3 floorNormal)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (CrashSpotSearch.IsOutOfBounds(floorPos) || CrashSpotSearch.IsInWater(floorPos))
		{
			return false;
		}
		if (Vector3.Angle(floorNormal, Vector3.up) > 30f)
		{
			return false;
		}
		return IsCrateSpotClear(floorPos, floorCollider);
	}

	private static bool IsCrateSpotClear(Vector3 floorPos, Collider floorCollider)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		int num = Physics.OverlapSphereNonAlloc(floorPos + Vector3.up * 0.85f, 0.75f, crateClearanceBuffer, 1210263809, (QueryTriggerInteraction)1);
		for (int i = 0; i < num; i++)
		{
			Collider val = crateClearanceBuffer[i];
			if (!((Object)(object)val == (Object)null) && !((Object)(object)val == (Object)(object)floorCollider))
			{
				return false;
			}
		}
		return true;
	}

	private static bool TryTakeCrateSpawnPoint(List<Transform> points, out Vector3 groundPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		groundPos = default(Vector3);
		while (points.Count > 0)
		{
			Transform val = points[points.Count - 1];
			points.RemoveAt(points.Count - 1);
			if (!((Object)(object)val == (Object)null))
			{
				Vector3 val2 = FindCrateFloor(val.position, out var floorCollider, out var floorNormal);
				if (IsCrateSpotUsable(val2, floorCollider, floorNormal))
				{
					groundPos = val2;
					return true;
				}
			}
		}
		return false;
	}

	private void SpawnLootCrates(Vector3 crashPos, List<Collider> fireColliders, int count, float lootScale, SatelliteCrashRemains remains)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		if (cratesToDrop == null || cratesToDrop.Length == 0)
		{
			return;
		}
		PooledList<Transform> val = Pool.Get<PooledList<Transform>>();
		try
		{
			GetShuffledCrateSpawnPoints(remains, (List<Transform>)(object)val);
			float num = 0f;
			for (int i = 0; i < count; i++)
			{
				GameObjectRef gameObjectRef = cratesToDrop[Random.Range(0, cratesToDrop.Length)];
				if (gameObjectRef == null || !gameObjectRef.isValid)
				{
					continue;
				}
				if (!TryTakeCrateSpawnPoint((List<Transform>)(object)val, out var groundPos))
				{
					groundPos = FindGroundedScatterPosition(crashPos, 3f, 10f);
				}
				Vector3 pos = groundPos + Vector3.up * 0.5f;
				BaseEntity baseEntity = GameManager.server.CreateEntity(gameObjectRef.resourcePath, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
				if ((Object)(object)baseEntity == (Object)null)
				{
					continue;
				}
				LootContainer lootContainer = baseEntity as LootContainer;
				if ((Object)(object)lootContainer != (Object)null && lootScale > 1f)
				{
					float num2 = (float)lootContainer.maxDefinitionsToSpawn * lootScale + num;
					int num3 = Mathf.Max(1, Mathf.RoundToInt(num2));
					num = num2 - (float)num3;
					lootContainer.maxDefinitionsToSpawn = num3;
					lootContainer.scrapAmount = Mathf.RoundToInt((float)lootContainer.scrapAmount * lootScale);
				}
				if ((Object)(object)lootContainer != (Object)null)
				{
					lootContainer.clanScoreEventForFirstLooter = (ClanScoreEventType)14;
				}
				baseEntity.Spawn();
				if ((Object)(object)lootContainer != (Object)null)
				{
					lootContainer.Invoke(lootContainer.RemoveMe, crateLifetimeMinutes * 60f);
				}
				if (baseEntity is TimedUnlootableCrate timedUnlootableCrate)
				{
					timedUnlootableCrate.SetUnlootableFor(Satellite.crate_fire_duration);
				}
				Rigidbody obj = ((Component)baseEntity).gameObject.AddComponent<Rigidbody>();
				ConfigureScatterRigidbody(obj, 2f, 0.2f, 0.080000006f, useGravity: true);
				obj.velocity = Vector3.zero;
				obj.angularVelocity = Vector3.zero;
				if (this.fireBall.isValid)
				{
					FireBall fireBall = GameManager.server.CreateEntity(this.fireBall.resourcePath) as FireBall;
					if ((Object)(object)fireBall != (Object)null)
					{
						fireBall.SetParent(baseEntity);
						fireBall.Spawn();
						((Component)fireBall).GetComponent<Rigidbody>().isKinematic = true;
						((Component)fireBall).GetComponent<Collider>().enabled = false;
						fireBall.CancelInvoke(fireBall.TryToSpread);
						fireBall.CancelInvoke(fireBall.Extinguish);
						fireBall.Invoke(fireBall.Extinguish, Satellite.crate_fire_duration);
					}
				}
				Collider component = ((Component)baseEntity).GetComponent<Collider>();
				if ((Object)(object)component == (Object)null)
				{
					continue;
				}
				foreach (Collider fireCollider in fireColliders)
				{
					if ((Object)(object)fireCollider != (Object)null)
					{
						Physics.IgnoreCollision(component, fireCollider, true);
					}
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.satelliteCrash != null)
		{
			satelliteMass = info.msg.satelliteCrash.satelliteMass;
			IsDescending = info.msg.satelliteCrash.isDescending;
			_ = info.msg.satelliteCrash.descentStartPos;
			descentStartPos = info.msg.satelliteCrash.descentStartPos;
			descentSecondsToTake = info.msg.satelliteCrash.descentSecondsToTake;
			descentSecondsTaken = info.msg.satelliteCrash.descentSecondsTaken;
			_ = info.msg.satelliteCrash.crashTarget;
			crashTarget = info.msg.satelliteCrash.crashTarget;
		}
	}

	static SatelliteCrash()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		CrashEventTitlePhrase = new Phrase("satellite.event.title", "SATELLITE EVENT");
		CrashEventBodyPhrase = new Phrase("satellite.event.crashed", "A satellite has crashed at {0}!");
		crateClearanceBuffer = (Collider[])(object)new Collider[16];
	}
}
