using System;
using System.Collections.Generic;
using ConVar;
using Development.Attributes;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using VacuumBreather;

public class BaseBoat : BaseVehicle
{
	public enum CorrectionForceMode
	{
		Force = 0,
		Acceleration = 5
	}

	[Header("Boat")]
	public float engineThrust = 10f;

	public float steeringScale = 0.1f;

	public Transform thrustPoint;

	public Transform centerOfMass;

	public Buoyancy buoyancy;

	public bool preventDecayIndoors = true;

	[Header("Correction Forces")]
	public bool applyCorrectionForces = true;

	public Transform[] planeFitPoints;

	public Vector3 inAirPID;

	public float inAirDesiredPitch = -15f;

	public Vector3 wavePID;

	public MinMax correctionRange;

	public CorrectionForceMode correctionForceMode;

	public float correctionSpringForce;

	public float correctionSpringDamping;

	private Vector3[] worldAnchors;

	private PidQuaternionController pidController;

	[ServerVar(Help = "(Generated) When enabled, procedural patrol paths are generated for boats at server startup; false in editor to skip path generation during testing")]
	public static bool generate_paths = true;

	[ServerVar(Help = "(Generated) When enabled, boats without nearby players will slowly drift toward the shore after the shore drift delay elapses")]
	public static bool do_shore_drift = true;

	public static int secondsUntilShoreDrift = 7200;

	public static int secondsBetweenShoreDrift = 120;

	[ServerVar]
	[Help("Shore drift speed in metres per second")]
	public static float drift_speed = 1f;

	[ServerVar(Help = "(Generated) When enabled, logs debug information about AI ejection events when passengers are removed from boat seats")]
	public static bool debug_eject_ai = false;

	private float driftDelayedUntil;

	[NonSerialized]
	public float gasPedal;

	[NonSerialized]
	public float steering;

	private TimeSince shoreDriftTimer;

	private string lastDriftCheckStatus = "Never checked";

	protected virtual bool AllowKinematicDrift => false;

	protected virtual bool SkipBoatForcedUpdate => false;

	[Help("Seconds until boat starts drifting to shore if there's nobody around")]
	[ServerVar]
	public static int seconds_until_shore_drift(ConsoleSystem.Arg arg)
	{
		secondsUntilShoreDrift = arg.GetInt(0, secondsUntilShoreDrift);
		UpdateShoreDriftInvokeOnAll();
		return secondsUntilShoreDrift;
	}

	[Help("Seconds between shore drift teleport ticks")]
	[ServerVar]
	public static int seconds_between_shore_drift(ConsoleSystem.Arg arg)
	{
		secondsBetweenShoreDrift = arg.GetInt(0, secondsBetweenShoreDrift);
		UpdateShoreDriftInvokeOnAll();
		return secondsBetweenShoreDrift;
	}

	public virtual bool DriftDelayed()
	{
		return Time.realtimeSinceStartup < driftDelayedUntil;
	}

	public void SetDriftDelayAmount(float delayAmount)
	{
		driftDelayedUntil = Time.realtimeSinceStartup + delayAmount;
	}

	public bool InDryDock()
	{
		return (Object)(object)GetParentEntity() != (Object)null;
	}

	public override float AntiHackVelocity()
	{
		return 25f;
	}

	public override void ServerInit()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		rigidBody.isKinematic = false;
		if ((Object)(object)rigidBody == (Object)null)
		{
			Debug.LogWarning((object)"Boat rigidbody null");
			return;
		}
		if ((Object)(object)centerOfMass == (Object)null)
		{
			Debug.LogWarning((object)"boat COM null");
			return;
		}
		rigidBody.centerOfMass = centerOfMass.localPosition;
		if (planeFitPoints == null || planeFitPoints.Length != 3)
		{
			Debug.LogWarning((object)"Boats require 3 plane fit points");
			return;
		}
		worldAnchors = (Vector3[])(object)new Vector3[3];
		pidController = new PidQuaternionController(wavePID.x, wavePID.y, wavePID.z);
		if (Application.isLoadingSave)
		{
			InvokeRandomized(CheckDriftToShore, secondsBetweenShoreDrift, secondsBetweenShoreDrift, (float)secondsBetweenShoreDrift * 0.1f);
			return;
		}
		shoreDriftTimer = TimeSince.op_Implicit(0f);
		InvokeRandomized(CheckDriftToShore, secondsUntilShoreDrift, secondsBetweenShoreDrift, (float)secondsBetweenShoreDrift * 0.1f);
	}

	public override void RunAIDriverTick()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (aiInputProvider == null)
		{
			RemoveAIDriver();
		}
		else
		{
			aiInputProvider.OnTick(this, TimeSince.op_Implicit(base.TimeSinceLastAIUpdate), ref steering, ref gasPedal);
		}
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		if (IsDriver(player))
		{
			DriverInput(inputState, player);
		}
	}

	public virtual void DriverInput(InputState inputState, BasePlayer player)
	{
		if (inputState.IsDown(BUTTON.FORWARD))
		{
			gasPedal = 1f;
		}
		else if (inputState.IsDown(BUTTON.BACKWARD))
		{
			gasPedal = -0.5f;
		}
		else
		{
			gasPedal = 0f;
		}
		if (inputState.IsDown(BUTTON.LEFT))
		{
			steering = 1f;
		}
		else if (inputState.IsDown(BUTTON.RIGHT))
		{
			steering = -1f;
		}
		else
		{
			steering = 0f;
		}
	}

	public void OnPoolDestroyed()
	{
		Kill(DestroyMode.Gib);
	}

	public void WakeUp()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rigidBody != (Object)null)
		{
			rigidBody.WakeUp();
			rigidBody.AddForce(Vector3.up * 0.1f, (ForceMode)1);
		}
	}

	protected override void OnServerWake()
	{
		if ((Object)(object)buoyancy != (Object)null)
		{
			buoyancy.Wake();
		}
	}

	public virtual bool EngineOn()
	{
		if (HasDriver())
		{
			return !IsFlipped();
		}
		return false;
	}

	public virtual bool EngineOnEligible()
	{
		if (EngineOn() && !IsFlipped())
		{
			return base.healthFraction > 0f;
		}
		return false;
	}

	public override void VehicleFixedUpdate()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BaseBoat.VehicleFixedUpdate"))
		{
			if (!EngineOn())
			{
				gasPedal = 0f;
				steering = 0f;
			}
			base.VehicleFixedUpdate();
			if (SkipBoatForcedUpdate)
			{
				return;
			}
			if (!IsDead())
			{
				ApplyCorrectionForces();
			}
			if (gasPedal != 0f && buoyancy.submergedFraction > 0.3f && WaterLevel.Test(thrustPoint.position, waves: true, volumes: true, this))
			{
				Vector3 val = ((Component)this).transform.forward + ((Component)this).transform.right * (steering * steeringScale);
				Vector3 val2 = ((Vector3)(ref val)).normalized * (gasPedal * engineThrust);
				rigidBody.AddForceAtPosition(val2, thrustPoint.position, (ForceMode)0);
			}
			if (AnyMounted() && IsFlipped())
			{
				if (debug_eject_ai)
				{
					Debug.LogWarning((object)$"Ejecting players from flipped boat {this}");
					object arg = ((Component)this).transform.position;
					Quaternion rotation = ((Component)this).transform.rotation;
					Debug.LogWarning((object)$"Boat was in state / pos {arg} / rot {((Quaternion)(ref rotation)).eulerAngles}");
				}
				DismountAllPlayers();
			}
		}
	}

	protected void ApplyCorrectionForces()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ApplyCorrectionForces"))
		{
			if (applyCorrectionForces && planeFitPoints != null && planeFitPoints.Length == 3 && EngineOn() && !(buoyancy.submergedFraction < 0.5f))
			{
				Vector3 val = default(Vector3);
				Quaternion currentOrientation = default(Quaternion);
				((Component)this).transform.GetPositionAndRotation(ref val, ref currentOrientation);
				Vector3 eulerAngles = ((Quaternion)(ref currentOrientation)).eulerAngles;
				Matrix4x4 val2 = Matrix4x4.TRS(val, Quaternion.Euler(0f, eulerAngles.y, 0f), Vector3.one);
				for (int i = 0; i < planeFitPoints.Length; i++)
				{
					Vector3 val3 = ((Matrix4x4)(ref val2)).MultiplyPoint(planeFitPoints[i].localPosition);
					val3.y = WaterLevel.GetWaterLevel(val3, waves: true);
					worldAnchors[i] = val3;
				}
				float y = planeFitPoints[0].localPosition.y;
				float num = (worldAnchors[0].y + worldAnchors[1].y + worldAnchors[2].y) / 3f - y;
				float y2 = val.y;
				float x = correctionRange.x;
				float y3 = correctionRange.y;
				if (y2 > num + x && y2 < num + y3)
				{
					float num2 = num - y2;
					Vector3 linearVelocity = rigidBody.linearVelocity;
					float num3 = num2 * correctionSpringForce;
					float num4 = (0f - linearVelocity.y) * correctionSpringDamping;
					rigidBody.AddForce(0f, num3 + num4, 0f, (ForceMode)correctionForceMode);
				}
				if (y2 > num + y3)
				{
					Quaternion desiredOrientation = Quaternion.Euler(inAirDesiredPitch, eulerAngles.y, 0f);
					pidController.Kp = inAirPID.x;
					pidController.Ki = inAirPID.y;
					pidController.Kd = inAirPID.z;
					Vector3 val4 = pidController.ComputeRequiredAngularAcceleration(currentOrientation, desiredOrientation, rigidBody.angularVelocity, Time.fixedDeltaTime);
					rigidBody.AddTorque(val4, (ForceMode)5);
				}
				else if (y2 > num + x)
				{
					Vector3 val5 = Vector3.Cross(worldAnchors[1] - worldAnchors[0], worldAnchors[2] - worldAnchors[0]);
					Vector3 val6 = worldAnchors[2] - worldAnchors[1];
					Quaternion desiredOrientation2 = Quaternion.LookRotation(Vector3.Cross(val5, val6), val5);
					pidController.Kp = wavePID.x;
					pidController.Ki = wavePID.y;
					pidController.Kd = wavePID.z;
					Vector3 val7 = pidController.ComputeRequiredAngularAcceleration(currentOrientation, desiredOrientation2, rigidBody.angularVelocity, Time.fixedDeltaTime);
					val7.y = 0f;
					rigidBody.AddTorque(val7, (ForceMode)5);
				}
			}
		}
	}

	public static void WaterVehicleDecay(BaseCombatEntity entity, float decayTickRate, float timeSinceLastUsed, float outsideDecayMinutes, float deepWaterDecayMinutes, float decayStartDelayMinutes, bool preventDecayIndoors)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (entity.healthFraction != 0f && !(timeSinceLastUsed < 60f * decayStartDelayMinutes))
		{
			float overallWaterDepth = WaterLevel.GetOverallWaterDepth(((Component)entity).transform.position, waves: true, volumes: false);
			float num = outsideDecayMinutes;
			if (preventDecayIndoors && !entity.IsOutside())
			{
				num = float.PositiveInfinity;
			}
			if (overallWaterDepth > 12f)
			{
				float num2 = Mathf.InverseLerp(12f, 16f, overallWaterDepth);
				float num3 = Mathf.Lerp(0.1f, 1f, num2);
				num = Mathf.Min(num, deepWaterDecayMinutes / num3);
			}
			if (!float.IsPositiveInfinity(num))
			{
				float num4 = decayTickRate / 60f / num;
				entity.Hurt(entity.MaxHealth() * num4, DamageType.Decay, entity, useProtection: false);
			}
		}
	}

	private void CheckDriftToShore()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		if (!do_shore_drift)
		{
			return;
		}
		if (DriftDelayed())
		{
			lastDriftCheckStatus = "Drift delay active";
		}
		else if (TimeSince.op_Implicit(shoreDriftTimer) < (float)secondsUntilShoreDrift - 1f)
		{
			lastDriftCheckStatus = "Was too soon";
		}
		else if (!AllowKinematicDrift && rigidBody.isKinematic)
		{
			lastDriftCheckStatus = "Was kinematic";
		}
		else if (IsOn())
		{
			lastDriftCheckStatus = "Was on";
			shoreDriftTimer = TimeSince.op_Implicit(0f);
		}
		else if (AnyPlayersOnBoat())
		{
			lastDriftCheckStatus = "Players were on boat";
			shoreDriftTimer = TimeSince.op_Implicit(0f);
		}
		else if (HasParent())
		{
			lastDriftCheckStatus = "Was parented";
			shoreDriftTimer = TimeSince.op_Implicit(0f);
		}
		else if (IsDead())
		{
			lastDriftCheckStatus = "Is dead";
			shoreDriftTimer = TimeSince.op_Implicit(0f);
		}
		else if (WaterFactor() < 0.1f)
		{
			lastDriftCheckStatus = "Not in water";
			shoreDriftTimer = TimeSince.op_Implicit(0f);
		}
		else if ((Object)(object)TerrainTexturing.Instance == (Object)null)
		{
			lastDriftCheckStatus = "No terrain tex";
			shoreDriftTimer = TimeSince.op_Implicit(0f);
		}
		else if (BaseNetworkable.HasConnections(((Component)this).transform.position))
		{
			lastDriftCheckStatus = "Players were nearby";
		}
		else
		{
			if (DeepSeaManager.IsInsideDeepSea((BaseNetworkable)this))
			{
				return;
			}
			float num = BoundsEx.MaxExtent(bounds) + 2f;
			float num2 = drift_speed * (float)secondsBetweenShoreDrift;
			(Vector3 shoreDir, float shoreDist) coarseVectorToShore = TerrainTexturing.Instance.GetCoarseVectorToShore(((Component)this).transform.position);
			Vector3 item = coarseVectorToShore.shoreDir;
			float item2 = coarseVectorToShore.shoreDist;
			float num3 = 2f * num;
			if (item2 < num3)
			{
				shoreDriftTimer = TimeSince.op_Implicit(0f);
				lastDriftCheckStatus = "Was near shore already";
				return;
			}
			float maxDistance = num2 - num;
			Vector3 val = ((Component)this).transform.position + ((Bounds)(ref bounds)).center + item * (num + 1f);
			Vector3 val2 = ((Component)this).transform.position + ((Bounds)(ref bounds)).center + item * num2;
			Ray ray = new Ray(val, item);
			List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
			GamePhysics.TraceAll(ray, num, list, maxDistance, 1235583233, (QueryTriggerInteraction)1, this);
			if (list.Count > 0)
			{
				foreach (RaycastHit item3 in list)
				{
					RaycastHit current = item3;
					if ((Object)(object)((RaycastHit)(ref current)).collider != (Object)null)
					{
						BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((RaycastHit)(ref current)).collider);
						if ((Object)(object)baseEntity != (Object)null && baseEntity.HasEntityInParents(this))
						{
							continue;
						}
					}
					if (((RaycastHit)(ref current)).distance <= num3)
					{
						lastDriftCheckStatus = string.Format("Was blocked by {0}:{1} at {2}", ((Object)(object)((RaycastHit)(ref current)).transform.parent != (Object)null) ? ((Object)((RaycastHit)(ref current)).transform.parent).name : "", ((Object)((RaycastHit)(ref current)).transform).name, ((RaycastHit)(ref current)).transform.position);
						Pool.FreeUnmanaged<RaycastHit>(ref list);
						return;
					}
					val2 = ((RaycastHit)(ref current)).point - item * num3;
					val2.y = ((Component)this).transform.position.y + ((Bounds)(ref bounds)).center.y;
					break;
				}
			}
			Vector3 position = ((Component)this).transform.position;
			((Component)this).transform.position = val2 - ((Bounds)(ref bounds)).center;
			if (!rigidBody.isKinematic)
			{
				rigidBody.linearVelocity = Vector3.zero;
				rigidBody.angularVelocity = Vector3.zero;
			}
			lastDriftCheckStatus = $"Drifted {Vector3.Distance(position, ((Component)this).transform.position):F0}m";
			Pool.FreeUnmanaged<RaycastHit>(ref list);
			Invoke(GoToSleep, 0f);
		}
	}

	private void GoToSleep()
	{
		rigidBody.Sleep();
	}

	public virtual bool AnyPlayersOnBoat()
	{
		return AnyMounted();
	}

	[PoolAnalyzerNonCaching]
	public virtual void GetPlayersOnBoat(List<BasePlayer> players)
	{
		GetMountedPlayers(players);
	}

	public string GetDriftStatus()
	{
		return lastDriftCheckStatus;
	}

	public static void UpdateShoreDriftInvokeOnAll()
	{
		BaseBoat[] array = Util.FindAll<BaseBoat>();
		foreach (BaseBoat baseBoat in array)
		{
			if (baseBoat.IsValid())
			{
				baseBoat.UpdateShoreDriftInvoke();
			}
		}
	}

	private void UpdateShoreDriftInvoke()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float time = Mathf.Max(0f, (float)secondsUntilShoreDrift - TimeSince.op_Implicit(shoreDriftTimer));
		InvokeRandomized(CheckDriftToShore, time, secondsBetweenShoreDrift, (float)secondsBetweenShoreDrift * 0.1f);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.baseBoat = Pool.Get<BaseBoat>();
		info.msg.baseBoat.shoreDriftTimerValue = ((TimeSince)(ref shoreDriftTimer)).PassedSince(info.cachedTime.Time);
	}

	public override void Load(LoadInfo info)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.baseBoat != null && base.isServer)
		{
			shoreDriftTimer = TimeSince.op_Implicit(info.msg.baseBoat.shoreDriftTimerValue);
		}
	}

	public override float WaterFactorForPlayer(BasePlayer player, out WaterLevel.WaterInfo info)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		info = WaterLevel.GetWaterInfo(player.eyes.position, waves: true, volumes: true);
		if (!info.isValid)
		{
			return 0f;
		}
		return 1f;
	}

	public static List<Vector3> GenerateOceanPatrolPath(float minDistanceFromShore = 50f, float minWaterDepth = 8f)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		object obj = Interface.CallHook("OnBoatPathGenerate");
		if (obj is List<Vector3>)
		{
			return (List<Vector3>)obj;
		}
		float x = TerrainMeta.Size.x;
		float num = x * 2f * MathF.PI;
		float num2 = 30f;
		int num3 = Mathf.CeilToInt(num / num2);
		List<Vector3> list = new List<Vector3>();
		float num4 = x;
		float num5 = 0f;
		for (int i = 0; i < num3; i++)
		{
			float num6 = (float)i / (float)num3 * 360f;
			list.Add(new Vector3(Mathf.Sin(num6 * (MathF.PI / 180f)) * num4, num5, Mathf.Cos(num6 * (MathF.PI / 180f)) * num4));
		}
		float num7 = 4f;
		float num8 = 200f;
		bool flag = true;
		RaycastHit val9 = default(RaycastHit);
		for (int j = 0; (j < AI.ocean_patrol_path_iterations) & flag; j++)
		{
			flag = false;
			for (int k = 0; k < num3; k++)
			{
				Vector3 val = list[k];
				int index = ((k == 0) ? (num3 - 1) : (k - 1));
				int index2 = ((k != num3 - 1) ? (k + 1) : 0);
				Vector3 val2 = list[index2];
				Vector3 val3 = list[index];
				Vector3 val4 = val;
				Vector3 val5 = Vector3.zero - val;
				Vector3 normalized = ((Vector3)(ref val5)).normalized;
				Vector3 val6 = val + normalized * num7;
				if (Vector3.Distance(val6, val2) > num8 || Vector3.Distance(val6, val3) > num8)
				{
					continue;
				}
				bool flag2 = true;
				int num9 = 16;
				for (int l = 0; l < num9; l++)
				{
					float num10 = (float)l / (float)num9 * 360f;
					val5 = new Vector3(Mathf.Sin(num10 * (MathF.PI / 180f)), num5, Mathf.Cos(num10 * (MathF.PI / 180f)));
					Vector3 normalized2 = ((Vector3)(ref val5)).normalized;
					Vector3 val7 = val6 + normalized2 * 1f;
					Vector3 val8 = normalized;
					if (val7 != Vector3.zero)
					{
						val5 = val7 - val6;
						val8 = ((Vector3)(ref val5)).normalized;
					}
					if (!Physics.SphereCast(val4, 3f, val8, ref val9, minDistanceFromShore, 1084293377))
					{
						continue;
					}
					Collider collider = ((RaycastHit)(ref val9)).collider;
					if ((Object)(object)collider != (Object)null && !ColliderEx.IsOnLayer(collider, (Layer)23))
					{
						MonumentInfo monument = ColliderEx.GetMonument(collider);
						if ((Object)(object)monument != (Object)null && monument.IsOilRig())
						{
							continue;
						}
					}
					flag2 = false;
					break;
				}
				if (flag2)
				{
					flag = true;
					list[k] = val6;
				}
			}
		}
		if (flag)
		{
			Debug.LogWarning((object)"Failed to generate ocean patrol path");
			return null;
		}
		List<int> list2 = new List<int>();
		LineUtility.Simplify(list, 5f, list2);
		List<Vector3> list3 = list;
		list = new List<Vector3>();
		foreach (int item in list2)
		{
			list.Add(list3[item]);
		}
		Debug.Log((object)("Generated ocean patrol path with node count: " + list.Count));
		return list;
	}
}
