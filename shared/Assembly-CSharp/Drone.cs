using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Drone : RemoteControlEntity, IRemoteControllableClientCallbacks, IRemoteControllable, IRemoteControllablePromptProvider, IRemoteControllableHostileProvider, SamSite.ISamSiteTarget
{
	public struct DroneInputState
	{
		public Vector3 movement;

		public float throttle;

		public float pitch;

		public float yaw;

		public void Reset()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			movement = Vector3.zero;
			pitch = 0f;
			yaw = 0f;
		}
	}

	[ReplicatedVar(Help = "How far drones can be flown away from the controlling computer station", ShowInAdminUI = true, Default = "600")]
	public static float maxControlRange = 750f;

	[ServerVar(Help = "If greater than zero, overrides the drone's planar movement speed")]
	public static float movementSpeedOverride = 0f;

	[ServerVar(Help = "If greater than zero, overrides the drone's vertical movement speed")]
	public static float altitudeSpeedOverride = 0f;

	[ServerVar]
	public static bool disableSamTargeting;

	[ServerVar(Help = "Radius at which a SAM missile's proximity fuse detonates against drones, capped by the missile's 20m trigger sphere")]
	public static float samProximityFuseRadius = 10f;

	[ClientVar(ClientAdmin = true)]
	public static float windTimeDivisor = 10f;

	[ClientVar(ClientAdmin = true)]
	public static float windPositionDivisor = 100f;

	[ClientVar(ClientAdmin = true)]
	public static float windPositionScale = 1f;

	[ClientVar(ClientAdmin = true)]
	public static float windRotationMultiplier = 45f;

	[ClientVar(ClientAdmin = true)]
	public static float windLerpSpeed = 0.1f;

	public const Flags Flag_ThrottleUp = Flags.Reserved1;

	public const Flags Flag_Flying = Flags.Reserved2;

	public const Flags Flag_HoldingItem = Flags.Reserved3;

	private const Flags Flag_IsHostile = Flags.Reserved4;

	public const Flags Flag_DropCooldown = Flags.Reserved5;

	[Header("Drone")]
	public Rigidbody body;

	public Transform modelRoot;

	public bool killInWater = true;

	public bool killInTerrain = true;

	public bool enableGrounding = true;

	public bool keepAboveTerrain = true;

	public float groundTraceDist = 0.1f;

	public float groundCheckInterval = 0.05f;

	public float altitudeAcceleration = 10f;

	public float movementAcceleration = 10f;

	public float yawSpeed = 2f;

	public float uprightSpeed = 2f;

	public float uprightPrediction = 0.15f;

	public float uprightDot = 0.5f;

	public float leanWeight = 0.1f;

	public float leanMaxVelocity = 5f;

	public float hurtVelocityThreshold = 3f;

	public float hurtDamagePower = 3f;

	public float collisionDisableTime = 0.25f;

	public float pitchMin = -60f;

	public float pitchMax = 60f;

	public float pitchSensitivity = -5f;

	public bool disableWhenHurt;

	[Range(0f, 1f)]
	public float disableWhenHurtChance = 0.25f;

	public float playerCheckInterval = 0.1f;

	public float playerCheckRadius;

	public float deployYOffset = 0.1f;

	public Phrase computerPromptPhrase;

	public Phrase cooldownComputerPromptPhrase;

	[Header("Sound")]
	public SoundDefinition movementLoopSoundDef;

	public SoundDefinition movementStartSoundDef;

	public SoundDefinition movementStopSoundDef;

	public AnimationCurve movementLoopPitchCurve;

	public float movementSpeedReference = 50f;

	[Header("Animation")]
	public float propellerMaxSpeed = 1000f;

	public float propellerAcceleration = 3f;

	public Transform propellerA;

	public Transform propellerB;

	public Transform propellerC;

	public Transform propellerD;

	public float pitch;

	private EntityRef<DroneStorage> storageDrop;

	public Vector3? targetPosition;

	public DroneInputState currentInput;

	public float lastInputTime;

	public double lastCollision = -1000.0;

	public TimeSince lastGroundCheck;

	public bool isGrounded;

	public RealTimeSinceEx lastPlayerCheck;

	private float avgTerrainHeight;

	private BasePlayer cachedController;

	public Phrase ComputerPromptPhrase
	{
		get
		{
			if (HasFlag(Flags.Reserved5))
			{
				return cooldownComputerPromptPhrase;
			}
			return computerPromptPhrase;
		}
	}

	public override bool RequiresMouse => true;

	public override float MaxRange => maxControlRange;

	public override bool CanAcceptInput => true;

	public SamSite.SamTargetType SAMTargetType => SamSite.targetTypeDrone;

	public override bool PositionTickFixedTime
	{
		protected get
		{
			return true;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Drone.OnRpcMessage"))
		{
			if (rpc == 795740894 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_OpenStorage"));
				}
				using (TimeWarning.New("SV_OpenStorage"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(795740894u, "SV_OpenStorage", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SV_OpenStorage(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SV_OpenStorage");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsRemoteControllableHostile()
	{
		return HasFlag(Flags.Reserved4);
	}

	public override void Spawn()
	{
		base.Spawn();
		isGrounded = true;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		foreach (BaseEntity child in children)
		{
			if (child is DroneStorage droneStorage)
			{
				storageDrop.Set(droneStorage);
				droneStorage.Drone = this;
			}
		}
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (child is DroneStorage droneStorage)
		{
			storageDrop.Set(droneStorage);
			droneStorage.Drone = this;
		}
	}

	public bool IsValidSAMTarget(bool staticRespawn)
	{
		if (disableSamTargeting)
		{
			return false;
		}
		if (!base.IsBeingControlled || isGrounded)
		{
			return false;
		}
		if (!IsHostile())
		{
			return false;
		}
		return true;
	}

	public override void StopControl(CameraViewerId viewerID)
	{
		CameraViewerId? controllingViewerId = base.ControllingViewerId;
		if (viewerID == controllingViewerId)
		{
			SetFlagLocal(Flags.Reserved1, b: false);
			SetFlagLocal(Flags.Reserved2, b: false);
			pitch = 0f;
			SendNetworkUpdate();
		}
		base.StopControl(viewerID);
	}

	public override void UserInput(InputState inputState, CameraViewerId viewerID)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		CameraViewerId? controllingViewerId = base.ControllingViewerId;
		if (!(viewerID != controllingViewerId))
		{
			currentInput.Reset();
			int num = (inputState.IsDown(BUTTON.FORWARD) ? 1 : 0) + (inputState.IsDown(BUTTON.BACKWARD) ? (-1) : 0);
			int num2 = (inputState.IsDown(BUTTON.RIGHT) ? 1 : 0) + (inputState.IsDown(BUTTON.LEFT) ? (-1) : 0);
			ref DroneInputState reference = ref currentInput;
			Vector3 val = new Vector3((float)num2, 0f, (float)num);
			reference.movement = ((Vector3)(ref val)).normalized;
			currentInput.throttle = (inputState.IsDown(BUTTON.SPRINT) ? 1 : 0) + (inputState.IsDown(BUTTON.DUCK) ? (-1) : 0);
			currentInput.yaw = inputState.current.mouseDelta.x;
			currentInput.pitch = inputState.current.mouseDelta.y;
			if (inputState.WasJustPressed(BUTTON.FIRE_PRIMARY))
			{
				TryStorageDrop();
			}
			lastInputTime = Time.time;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = currentInput.throttle > 0f;
			if (flag3 != HasFlag(Flags.Reserved1))
			{
				SetFlagLocal(Flags.Reserved1, flag3);
				flag = true;
			}
			float num3 = pitch;
			pitch += currentInput.pitch * pitchSensitivity;
			pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
			if (!Mathf.Approximately(pitch, num3))
			{
				flag2 = true;
			}
			if (flag2)
			{
				SendNetworkUpdateImmediate();
			}
			else if (flag)
			{
				SendNetworkUpdate_Flags();
			}
		}
	}

	private void OnPhysicsNeighbourChanged()
	{
		body.isKinematic = false;
		body.WakeUp();
	}

	public virtual void Update_Server()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isServer || IsDead())
		{
			return;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			if (storageDrop.IsSet)
			{
				DroneStorage droneStorage = storageDrop.Get(serverside: true);
				flagsUpdateScope.Set(Flags.Reserved3, !droneStorage.inventory.IsEmpty());
				droneStorage.UpdateFlags();
			}
			if (base.IsBeingControlled)
			{
				flagsUpdateScope.Set(Flags.Reserved4, IsHostile());
			}
		}
		if (base.IsBeingControlled || !targetPosition.HasValue)
		{
			return;
		}
		Vector3 position = ((Component)this).transform.position;
		float height = TerrainMeta.HeightMap.GetHeight(position);
		Vector3 val = targetPosition.Value - body.linearVelocity * 0.5f;
		if (keepAboveTerrain)
		{
			val.y = Mathf.Max(val.y, height + 1f);
		}
		Vector2 val2 = Vector3Ex.XZ2D(val);
		Vector2 val3 = Vector3Ex.XZ2D(position);
		Vector3 val4 = default(Vector3);
		float num = default(float);
		Vector3Ex.ToDirectionAndMagnitude(Vector3Ex.XZ3D(val2 - val3), ref val4, ref num);
		currentInput.Reset();
		lastInputTime = Time.time;
		if (position.y - height > 1f)
		{
			float num2 = Mathf.Clamp01(num);
			currentInput.movement = ((Component)this).transform.InverseTransformVector(val4) * num2;
			if (num > 0.5f)
			{
				Quaternion val5 = ((Component)this).transform.rotation;
				float y = ((Quaternion)(ref val5)).eulerAngles.y;
				val5 = Quaternion.FromToRotation(Vector3.forward, val4);
				float y2 = ((Quaternion)(ref val5)).eulerAngles.y;
				currentInput.yaw = Mathf.Clamp(Mathf.LerpAngle(y, y2, Time.deltaTime) - y, -2f, 2f);
			}
		}
		currentInput.throttle = Mathf.Clamp(val.y - position.y, -1f, 1f);
	}

	public void FixedUpdate()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_048b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		//IL_049b: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isServer || IsDead())
		{
			return;
		}
		Vector3 position = ((Component)this).transform.position;
		if (killInTerrain && AntiHack.TestInsideTerrain(position))
		{
			Kill();
			return;
		}
		if (killInWater)
		{
			float num = WaterFactor();
			if (num > 0f)
			{
				if (num > 0.99f)
				{
					Kill();
				}
				return;
			}
		}
		if ((!base.IsBeingControlled && !targetPosition.HasValue) || (isGrounded && currentInput.throttle <= 0f))
		{
			if (HasFlag(Flags.Reserved2))
			{
				SetFlagLocal(Flags.Reserved2, b: false);
				SendNetworkUpdate_Flags();
			}
			if (!body.isKinematic && body.IsSleeping() && TimeSince.op_Implicit(lastGroundCheck) >= groundCheckInterval * 8f)
			{
				lastGroundCheck = TimeSince.op_Implicit(0f);
				RaycastHit val = default(RaycastHit);
				if (body.SweepTest(Vector3.down, ref val, groundTraceDist, (QueryTriggerInteraction)1) && !Object.op_Implicit((Object)(object)((RaycastHit)(ref val)).rigidbody))
				{
					body.isKinematic = true;
				}
			}
			return;
		}
		if (playerCheckRadius > 0f && (double)lastPlayerCheck > (double)playerCheckInterval)
		{
			lastPlayerCheck = 0.0;
			List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
			Vis.Entities(position, playerCheckRadius, list, 131072, (QueryTriggerInteraction)2);
			if (list.Count > 0)
			{
				lastCollision = TimeEx.currentTimestamp;
			}
			Pool.FreeUnmanaged<BasePlayer>(ref list);
		}
		double currentTimestamp = TimeEx.currentTimestamp;
		bool num2 = lastCollision > 0.0 && currentTimestamp - lastCollision < (double)collisionDisableTime;
		if (enableGrounding)
		{
			if (TimeSince.op_Implicit(lastGroundCheck) >= groundCheckInterval)
			{
				lastGroundCheck = TimeSince.op_Implicit(0f);
				RaycastHit val2 = default(RaycastHit);
				bool flag = body.SweepTest(Vector3.down, ref val2, groundTraceDist, (QueryTriggerInteraction)1);
				if (!flag && isGrounded)
				{
					lastPlayerCheck = playerCheckInterval;
				}
				isGrounded = flag;
			}
			if (isGrounded && body.IsSleeping())
			{
				body.isKinematic = true;
			}
		}
		else
		{
			isGrounded = false;
		}
		Vector3 val3 = ((Component)this).transform.TransformDirection(currentInput.movement);
		Vector3 val4 = default(Vector3);
		float num3 = default(float);
		Vector3Ex.ToDirectionAndMagnitude(Vector3Ex.WithY(body.linearVelocity, 0f), ref val4, ref num3);
		float num4 = Mathf.Clamp01(num3 / leanMaxVelocity);
		Vector3 val5 = (Mathf.Approximately(((Vector3)(ref val3)).sqrMagnitude, 0f) ? ((0f - num4) * val4) : val3);
		Vector3 val6 = Vector3.up + val5 * leanWeight * num4;
		Vector3 normalized = ((Vector3)(ref val6)).normalized;
		Vector3 up = ((Component)this).transform.up;
		float num5 = Mathf.Max(Vector3.Dot(normalized, up), 0f);
		if (!num2 || isGrounded)
		{
			Vector3 val7 = ((isGrounded && currentInput.throttle <= 0f) ? Vector3.zero : (up * (-1f * Physics.gravity.y)));
			Vector3 val8 = (isGrounded ? Vector3.zero : (val3 * ((movementSpeedOverride > 0f) ? movementSpeedOverride : movementAcceleration)));
			float serviceCeiling = HotAirBalloon.serviceCeiling;
			float num6 = Mathf.Max(HotAirBalloon.minimumAltitudeTerrain, TerrainMeta.HeightMap.GetHeight(position));
			avgTerrainHeight = Mathf.Lerp(avgTerrainHeight, num6, Time.deltaTime);
			float num7 = 1f - Mathf.InverseLerp(avgTerrainHeight + serviceCeiling - 30f, avgTerrainHeight + serviceCeiling, position.y);
			Vector3 val9 = up * (currentInput.throttle * ((altitudeSpeedOverride > 0f) ? altitudeSpeedOverride : altitudeAcceleration));
			Vector3 val10 = num7 * (val7 + val9) + val8;
			body.isKinematic = false;
			body.AddForce(val10 * num5, (ForceMode)5);
		}
		if (!num2 && !isGrounded)
		{
			Vector3 val11 = ((Component)this).transform.TransformVector(0f, currentInput.yaw * yawSpeed, 0f);
			Vector3 val12 = Vector3.Cross(Quaternion.Euler(body.angularVelocity * uprightPrediction) * up, normalized) * uprightSpeed;
			float num8 = ((num5 < uprightDot) ? 0f : num5);
			Vector3 val13 = val11 * num5 + val12 * num8;
			body.isKinematic = false;
			body.AddTorque(val13 * num5, (ForceMode)5);
		}
		bool flag2 = !num2;
		if (flag2 == HasFlag(Flags.Reserved2))
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved2, flag2);
	}

	private void TryStorageDrop()
	{
		if (!isGrounded && !IsDead())
		{
			double currentTimestamp = TimeEx.currentTimestamp;
			if (!(lastCollision > 0.0) || !(currentTimestamp - lastCollision < (double)collisionDisableTime))
			{
				storageDrop.Get(serverside: true).TryServerDrop();
			}
		}
	}

	public void OnCollisionEnter(Collision collision)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			lastCollision = TimeEx.currentTimestamp;
			Vector3 relativeVelocity = collision.relativeVelocity;
			float magnitude = ((Vector3)(ref relativeVelocity)).magnitude;
			if (magnitude > hurtVelocityThreshold)
			{
				Hurt(Mathf.Pow(magnitude, hurtDamagePower), DamageType.Fall, null, useProtection: false);
			}
		}
	}

	public void OnCollisionStay()
	{
		if (base.isServer)
		{
			lastCollision = TimeEx.currentTimestamp;
		}
	}

	public void OnTriggerStay(Collider other)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer && ((Component)other).CompareTag("MLRSRocketTrigger"))
		{
			TimedExplosive componentInParent = ((Component)other).GetComponentInParent<TimedExplosive>();
			if (!((Object)(object)componentInParent == (Object)null) && !(Vector3.Distance(((Component)componentInParent).transform.position, CenterPoint()) > samProximityFuseRadius))
			{
				Hurt(base.health * 2f, DamageType.Explosion, componentInParent.creatorEntity, useProtection: false);
				componentInParent.Explode();
			}
		}
	}

	public override void Hurt(HitInfo info)
	{
		base.Hurt(info);
		if (base.isServer && disableWhenHurt && info.damageTypes.GetMajorityDamageType() != DamageType.Fall && Random.value < disableWhenHurtChance)
		{
			lastCollision = TimeEx.currentTimestamp;
		}
	}

	public override void OnDied(HitInfo info)
	{
		if (storageDrop.IsSet)
		{
			storageDrop.Get(serverside: true).DropItems(info.Initiator);
		}
		base.OnDied(info);
	}

	public override float GetNetworkTime()
	{
		return Time.fixedTime;
	}

	public override Vector3 GetLocalVelocityServer()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)body == (Object)null)
		{
			return Vector3.zero;
		}
		return body.linearVelocity;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.drone = Pool.Get<Drone>();
			info.msg.drone.pitch = pitch;
		}
	}

	public override void OnPickedUp(Item createdItem, BasePlayer player)
	{
		base.OnPickedUp(createdItem, player);
		DroneStorage droneStorage = storageDrop.Get(serverside: true);
		if ((Object)(object)droneStorage != (Object)null && !droneStorage.inventory.IsEmpty())
		{
			player.GiveItem(droneStorage.inventory.GetSlot(0), GiveItemReason.PickedUp);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SV_OpenStorage(RPCMessage msg)
	{
		if (CanBeLooted(msg.player))
		{
			DroneStorage droneStorage = storageDrop.Get(serverside: true);
			if (!((Object)(object)droneStorage == (Object)null))
			{
				droneStorage.PlayerOpenLoot(msg.player);
			}
		}
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer && TriggerSafeZone.IsBoundsInsideSafeZone(WorldSpaceBounds(), checkCombatZones: false))
		{
			return false;
		}
		return base.CanBeLooted(player);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.drone != null)
		{
			pitch = info.msg.drone.pitch;
		}
	}

	public virtual void Update()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Update_Server();
		if (HasFlag(Flags.Reserved2))
		{
			Quaternion localRotation = viewEyes.localRotation;
			Vector3 eulerAngles = ((Quaternion)(ref localRotation)).eulerAngles;
			eulerAngles.x = Mathf.LerpAngle(eulerAngles.x, pitch, 0.1f);
			viewEyes.localRotation = Quaternion.Euler(eulerAngles);
		}
	}

	public override bool CanChangeID(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null && base.OwnerID == (ulong)player.userID)
		{
			return !HasFlag(Flags.Reserved2);
		}
		return false;
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (!HasFlag(Flags.Reserved2))
		{
			return base.ShouldDisplayPickupOption(player);
		}
		return false;
	}

	public override BasePlayer ToPlayer()
	{
		if (!base.IsBeingControlled)
		{
			return null;
		}
		if (!base.ControllingViewerId.HasValue)
		{
			return null;
		}
		ulong steamId = base.ControllingViewerId.Value.SteamId;
		if ((Object)(object)cachedController == (Object)null || cachedController.OwnerID != steamId)
		{
			cachedController = BasePlayer.FindByID(steamId) ?? BasePlayer.FindSleeping(steamId);
		}
		return cachedController;
	}

	public override void OnPickedUpPreItemMove(Item createdItem, BasePlayer player)
	{
		base.OnPickedUpPreItemMove(createdItem, player);
		if ((Object)(object)player != (Object)null && (ulong)player.userID == base.OwnerID)
		{
			createdItem.text = GetIdentifier();
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		base.OnDeployed(parent, deployedBy, fromItem);
		Transform transform = ((Component)this).transform;
		transform.position += ((Component)this).transform.up * deployYOffset;
		if ((Object)(object)body != (Object)null)
		{
			body.linearVelocity = Vector3.zero;
			body.angularVelocity = Vector3.zero;
		}
		if (fromItem != null && !string.IsNullOrEmpty(fromItem.text) && ComputerStation.IsValidIdentifier(fromItem.text))
		{
			UpdateIdentifier(fromItem.text);
		}
	}

	public override bool IsHostile()
	{
		if (!base.IsHostile())
		{
			if (storageDrop.IsValid(serverside: true))
			{
				return storageDrop.Get(serverside: true).inventory.GetSlot(0)?.GetHeldEntity() is ThrownWeapon;
			}
			return false;
		}
		return true;
	}

	public override bool ShouldNetworkOwnerInfo()
	{
		return true;
	}

	public override bool ShouldInheritNetworkGroup()
	{
		return false;
	}

	public override float AntiHackVelocity()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 linearVelocity = body.linearVelocity;
		return ((Vector3)(ref linearVelocity)).magnitude + 1f;
	}
}
