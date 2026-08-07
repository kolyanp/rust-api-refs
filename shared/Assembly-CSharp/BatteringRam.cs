using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class BatteringRam : BaseSiegeWeapon, IEngineControllerUser, IEntity, VehicleChassisVisuals<BatteringRam>.IClientWheelUser
{
	[Serializable]
	private struct ImpactEffect
	{
		public string materialName;

		public GameObjectRef prefab;
	}

	public const Flags Flags_DoorOpened = Flags.Unused23;

	public const Flags Flags_DoorBusy = Flags.Reserved14;

	public const Flags Flags_DoorBroken = Flags.Reserved15;

	private readonly float idealBreakingForce = 100000f;

	private TimeSince lastDoorMovingTime;

	private TransformHandle doorPhysHingeHandle;

	private float steerInput;

	private bool steerMod;

	private float brakeInput;

	private float throttleInput;

	[SerializeField]
	[Header("Battering Ram")]
	private Animator animator;

	[SerializeField]
	private Transform damagePoint;

	[SerializeField]
	[Space]
	private float timeBetweenFire = 2f;

	[SerializeField]
	private float maxForwardSpeed = 1.5f;

	[SerializeField]
	[Header("Head")]
	[Space]
	private BatteringRamHead headPrefab;

	[SerializeField]
	private int headDamagePerHit = 20;

	[SerializeField]
	private DamageRenderer headDamageRenderer;

	private EntityRef<BatteringRamHead> headRef;

	[SerializeField]
	private GameObjectRef defaultRamImpactEffect;

	[SerializeField]
	private ImpactEffect[] impactEffects;

	[Header("IK")]
	[SerializeField]
	private Transform leftHandTarget;

	[SerializeField]
	private Transform rightHandTarget;

	[SerializeField]
	private Transform leftFootTarget;

	[SerializeField]
	private Transform rightFootTarget;

	[SerializeField]
	private Transform steeringWheel;

	[SerializeField]
	[HideInInspector]
	private Vector3 steerAngle;

	public VehicleModuleEngine.Engine engine;

	public List<DamageTypeEntry> damageTypes = new List<DamageTypeEntry>();

	[Header("Cockpit")]
	public Transform fuelGauge;

	[HideInInspector]
	[SerializeField]
	private Vector3 fuelAngle;

	private float cachedFuelFraction;

	[ServerVar(ClientAdmin = true, Default = "2", Help = "(Generated) Maximum building block upgrade grade (0=twig,1=wood,2=stone,3=metal,4=top tier) that the battering ram can damage; default 2 (stone)")]
	public static int maxBuildingBlockGrade = 2;

	[SerializeField]
	[Header("Door")]
	private Transform doorTransform;

	[SerializeField]
	private GameObjectRef doorServerGib;

	[SerializeField]
	private Rigidbody doorRigidBody;

	[SerializeField]
	private Transform doorPhysicsHinge;

	[SerializeField]
	private HingeJoint doorJoint;

	public GameObjectRef openEffect;

	public GameObjectRef openBounceEffect;

	public GameObjectRef closeEffect;

	public GameObjectRef closeEndEffect;

	[Tooltip("Effect played at local 0,0,0 in addition to the impact effects")]
	[Header("Effects")]
	public GameObjectRef hitEffect;

	public VehicleLight[] vehicleLights;

	[Header("Engine FX")]
	public ParticleSystemContainer[] engineParticles;

	public ParticleSystem[] exhaustSmoke;

	public float exhaustMinRate = 1f;

	public float exhaustMaxRate = 10f;

	public float exhaustRateChangeSpeed = 0.5f;

	[Space]
	public float exhaustSmokeMinOpacity = 1f;

	public float exhaustSmokeMaxOpacity = 10f;

	public float exhaustSmokeChangeSpeed = 0.5f;

	private float clientDoorAngle;

	public const Flags Flags_DamagedLow = Flags.Reserved6;

	public const Flags Flags_DamagedMid = Flags.Reserved10;

	public const Flags Flags_DamagedHeavy = Flags.Reserved8;

	public const int DAMAGE_LAYER = 1210286337;

	private BatteringRamHead _head;

	private bool EngineIsOn => base.CurEngineState == VehicleEngineController<GroundVehicle>.EngineState.On;

	private BatteringRamHead Head
	{
		get
		{
			if ((Object)(object)_head == (Object)null)
			{
				_head = GetHead();
			}
			return _head;
		}
	}

	public Vector3 DamagePointPosition => damagePoint.position;

	public override float DriveWheelVelocity
	{
		get
		{
			if (base.isServer)
			{
				return carPhysics.DriveWheelVelocity;
			}
			return 0f;
		}
	}

	public override float SteerAngle
	{
		get
		{
			if (base.isServer)
			{
				return carPhysics.SteerAngle;
			}
			return 0f;
		}
	}

	public override float MaxSteerAngle
	{
		get
		{
			if (base.isServer)
			{
				return carSettings.maxSteerAngle;
			}
			return 0f;
		}
	}

	public float DoorAngle
	{
		get
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (base.isServer)
			{
				if (BaseNetworkable.UseParallelSaves)
				{
					Quaternion localRotMT = Facepunch.Extend.TransformEx.Unsafe.GetLocalRotMT(in doorPhysHingeHandle);
					return ((Quaternion)(ref localRotMT)).eulerAngles.x;
				}
				return doorPhysicsHinge.localEulerAngles.x;
			}
			return 0f;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BatteringRam.OnRpcMessage"))
		{
			if (rpc == 3999508679u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_CloseDoor"));
				}
				using (TimeWarning.New("RPC_CloseDoor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3999508679u, "RPC_CloseDoor", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3999508679u, "RPC_CloseDoor", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_CloseDoor(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_CloseDoor");
					}
				}
				return true;
			}
			if (rpc == 3314360565u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenDoor"));
				}
				using (TimeWarning.New("RPC_OpenDoor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3314360565u, "RPC_OpenDoor", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3314360565u, "RPC_OpenDoor", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_OpenDoor(rpc3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_OpenDoor");
					}
				}
				return true;
			}
			if (rpc == 1851540757 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenFuel"));
				}
				using (TimeWarning.New("RPC_OpenFuel"))
				{
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
							RPC_OpenFuel(msg2);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_OpenFuel");
					}
				}
				return true;
			}
			if (rpc == 2422512421u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_WantsAttack"));
				}
				using (TimeWarning.New("SERVER_WantsAttack"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2422512421u, "SERVER_WantsAttack", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SERVER_WantsAttack(msg3);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in SERVER_WantsAttack");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private bool CanOpenDoor()
	{
		if (HasDoor() && !IsDoorBusy())
		{
			return !IsDoorOpen();
		}
		return false;
	}

	private bool CanCloseDoor()
	{
		if (HasDoor() && !IsDoorBusy())
		{
			return IsDoorOpen();
		}
		return false;
	}

	public bool IsDoorOpen()
	{
		return HasFlag(Flags.Unused23);
	}

	private bool IsDoorBusy()
	{
		return HasFlag(Flags.Reserved14);
	}

	private void DisableDoor()
	{
		ComponentExtensions.SetActive<Transform>(doorTransform, false);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(2uL)]
	protected void RPC_OpenDoor(RPCMessage rpc)
	{
		if (rpc.player.CanInteract(usableWhileCrawling: true) && CanOpenDoor() && Interface.CallHook("OnSiegeWeaponDoorOpen", this, rpc.player) == null)
		{
			OpenDoor();
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(2uL)]
	protected void RPC_CloseDoor(RPCMessage rpc)
	{
		if (rpc.player.CanInteract(usableWhileCrawling: true) && CanCloseDoor() && Interface.CallHook("OnSiegeWeaponDoorClose", this, rpc.player) == null)
		{
			CloseDoor();
		}
	}

	private void OpenDoor()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		doorRigidBody.mass = 100f;
		doorRigidBody.AddForceAtPosition(-((Component)this).transform.up * rigidBody.mass * 1.5f, ((Component)this).transform.position + Vector3.up * 2f, (ForceMode)1);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Unused23, b: true);
			flagsUpdateScope.Set(Flags.Reserved14, b: true);
		}
		doorJoint.useSpring = false;
		lastDoorMovingTime = TimeSince.op_Implicit(0f);
		if (IsInvokingFixedTime(MoveToNormalBreakForce))
		{
			CancelInvokeFixedTime(MoveToNormalBreakForce);
		}
		InvokeRepeatingFixedTime(MoveToNormalBreakForce);
		OnDoorOpening();
	}

	private void CloseDoor()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Unused23, b: false);
			flagsUpdateScope.Set(Flags.Reserved14, b: true);
		}
		doorJoint.useSpring = true;
		JointSpring spring = doorJoint.spring;
		spring.targetPosition = 0f;
		doorJoint.spring = spring;
		lastDoorMovingTime = TimeSince.op_Implicit(0f);
		carPhysics.lastMovingTime = Time.time;
		OnDoorClosing();
	}

	private void DoorBusyTick()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = doorRigidBody.angularVelocity;
		if (!(Mathf.Abs(((Vector3)(ref val)).magnitude) > 1f))
		{
			val = doorRigidBody.linearVelocity;
			if (!(Mathf.Abs(((Vector3)(ref val)).magnitude) > 1f))
			{
				goto IL_004e;
			}
		}
		lastDoorMovingTime = TimeSince.op_Implicit(0f);
		goto IL_004e;
		IL_004e:
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (TimeSince.op_Implicit(lastDoorMovingTime) > 3f && IsDoorOpen())
		{
			OnDoorOpened();
			flagsUpdateScope.Set(Flags.Reserved14, b: false);
		}
		if (IsDoorOpen())
		{
			if (carPhysics.IsWheelGrounded(6) || carPhysics.IsWheelGrounded(7))
			{
				OnDoorOpened();
				flagsUpdateScope.Set(Flags.Reserved14, b: false);
			}
			return;
		}
		Quaternion localRotation = doorPhysicsHinge.localRotation;
		if (Mathf.Abs(((Quaternion)(ref localRotation)).eulerAngles.x - 90f) <= 1f)
		{
			OnDoorClosed();
			flagsUpdateScope.Set(Flags.Reserved14, b: false);
		}
	}

	private void OnDoorOpening()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Effect.server.Run(openEffect.resourcePath, this, 0u, doorTransform.localPosition, Vector3.zero);
	}

	private void OnDoorClosing()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Effect.server.Run(closeEffect.resourcePath, this, 0u, doorTransform.localPosition, Vector3.zero);
		carPhysics.lastMovingTime = Time.time;
	}

	private void OnDoorClosed()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		doorRigidBody.mass = 1000f;
		((Joint)doorJoint).breakForce = idealBreakingForce * 4f;
		((Joint)doorJoint).breakTorque = idealBreakingForce * 4f;
		Effect.server.Run(closeEndEffect.resourcePath, this, 0u, doorTransform.localPosition, Vector3.zero);
	}

	private void OnDoorOpened()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Effect.server.Run(openBounceEffect.resourcePath, this, 0u, doorTransform.localPosition, Vector3.zero);
	}

	private void MoveToNormalBreakForce()
	{
		if ((Object)(object)doorJoint == (Object)null || Mathf.Approximately(((Joint)doorJoint).breakForce, idealBreakingForce))
		{
			CancelInvokeFixedTime(MoveToNormalBreakForce);
			return;
		}
		float num = Mathf.Lerp(((Joint)doorJoint).breakForce, idealBreakingForce, Time.fixedDeltaTime * 10f);
		((Joint)doorJoint).breakForce = num;
		((Joint)doorJoint).breakTorque = num;
	}

	private void SpawnDoorGib()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		CancelInvokeFixedTime(MoveToNormalBreakForce);
		ServerGib obj = GameManager.server.CreateEntity(doorServerGib.resourcePath, doorTransform.position, doorTransform.rotation) as ServerGib;
		obj.Spawn();
		Rigidbody component = ((Component)obj).GetComponent<Rigidbody>();
		component.linearVelocity = doorRigidBody.linearVelocity;
		component.angularVelocity = doorRigidBody.angularVelocity;
		component.WakeUp();
	}

	public override void ServerInit()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		InvokeRandomized(UpdateClients, 0f, 0.1f, 0.01f);
		if ((Object)(object)doorPhysicsHinge != (Object)null)
		{
			doorPhysHingeHandle = ((Component)doorPhysicsHinge).transformHandle;
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved14, b: false);
		}
		if (!HasDoor())
		{
			DisableDoor();
		}
		else if (HasFlag(Flags.Unused23))
		{
			OpenDoor();
		}
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		if (!IsDriver(player))
		{
			return;
		}
		throttleInput = 0f;
		brakeInput = 0f;
		if (engineController.IsOff)
		{
			if ((inputState.IsDown(BUTTON.FORWARD) && !inputState.WasDown(BUTTON.FORWARD)) || (inputState.IsDown(BUTTON.BACKWARD) && !inputState.WasDown(BUTTON.BACKWARD)))
			{
				engineController.TryStartEngine(player);
			}
		}
		else if (engineController.IsOn)
		{
			float num = 0f;
			if (inputState.IsDown(BUTTON.FORWARD))
			{
				num = 1f;
			}
			else if (inputState.IsDown(BUTTON.BACKWARD))
			{
				num = -1f;
			}
			if (GetSpeed() > 1f && num < -0.1f)
			{
				throttleInput = 0f;
				brakeInput = 0f - num;
			}
			else
			{
				throttleInput = num;
				brakeInput = 0f;
			}
			steerInput = 0f;
			if (inputState.IsDown(BUTTON.LEFT))
			{
				steerInput = -1f;
			}
			else if (inputState.IsDown(BUTTON.RIGHT))
			{
				steerInput = 1f;
			}
		}
	}

	protected override void ServerFlagsChanged(Flags old, Flags next)
	{
		base.ServerFlagsChanged(old, next);
		if (base.isServer)
		{
			if (base.CurEngineState == VehicleEngineController<GroundVehicle>.EngineState.Off)
			{
				RefreshLastUseTime();
			}
			if ((old & Flags.Reserved14) != Flags.Reserved14 && (next & Flags.Reserved14) == Flags.Reserved14)
			{
				InvokeRepeatingFixedTime(DoorBusyTick);
			}
			else if ((old & Flags.Reserved14) == Flags.Reserved14 && (next & Flags.Reserved14) != Flags.Reserved14)
			{
				CancelInvokeFixedTime(DoorBusyTick);
			}
		}
	}

	public override void OnHealthChanged(float oldvalue, float newvalue)
	{
		base.OnHealthChanged(oldvalue, newvalue);
		UpdateDamageFlags();
	}

	private void UpdateDamageFlags()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		float num = base.healthFraction;
		flagsUpdateScope.Set(Flags.Reserved6, b: false);
		flagsUpdateScope.Set(Flags.Reserved10, b: false);
		flagsUpdateScope.Set(Flags.Reserved8, b: false);
		if (num <= 0.25f)
		{
			flagsUpdateScope.Set(Flags.Reserved8, b: true);
		}
		else if (num <= 0.5f)
		{
			flagsUpdateScope.Set(Flags.Reserved10, b: true);
		}
		else if (num <= 0.75f)
		{
			flagsUpdateScope.Set(Flags.Reserved6, b: true);
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void SERVER_WantsAttack(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		BasePlayer driver = GetDriver();
		if ((Object)(object)driver == (Object)null || (Object)(object)player == (Object)null || (Object)(object)driver != (Object)(object)player || !CanAttack() || driver.InSafeZone() || Interface.CallHook("OnSiegeWeaponFire", this, player) != null)
		{
			return;
		}
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_Attack"));
		Invoke(delegate
		{
			ScanEntities(driver);
		}, 2f);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: true);
		}
		Invoke(delegate
		{
			using FlagsUpdateScope flagsUpdateScope2 = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope2.Set(Flags.Busy, b: false);
		}, timeBetweenFire);
	}

	private void ScanEntities(BasePlayer driver)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		List<BaseEntity> entities = Pool.Get<List<BaseEntity>>();
		Vis.Entities(damagePoint.position, 1f, entities, 1210286337, (QueryTriggerInteraction)1);
		FilterEntities(entities, driver);
		bool flag = entities.Count != 0;
		if (!flag)
		{
			List<Collider> list = Pool.Get<List<Collider>>();
			GamePhysics.OverlapSphere(damagePoint.position, 1f, list, 1210286337, (QueryTriggerInteraction)1);
			foreach (Collider item in list)
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item);
				if (!((Object)(object)baseEntity != (Object)null) || (!baseEntity.isClient && !((Object)(object)baseEntity == (Object)(object)this) && !((Object)(object)baseEntity == (Object)(object)Head)))
				{
					flag = true;
				}
			}
			Pool.FreeUnmanaged<Collider>(ref list);
		}
		bool arg = flag && Head.health - (float)headDamagePerHit <= Head.brokenHealthThreshold;
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_AttackResult"), flag, arg);
		Invoke(delegate
		{
			OnRamImpact(driver, entities);
		}, 0.5f);
	}

	private void FilterEntities(List<BaseEntity> entityList, BasePlayer driver)
	{
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		for (int i = 0; i < entityList.Count; i++)
		{
			BaseEntity baseEntity = entityList[i];
			if (baseEntity.isServer && !list.Contains(baseEntity) && (!((Object)(object)driver != (Object)null) || !((Object)(object)baseEntity == (Object)(object)driver)) && !((Object)(object)baseEntity == (Object)(object)this) && !((Object)(object)baseEntity == (Object)(object)Head))
			{
				list.Add(baseEntity);
			}
		}
		entityList.Clear();
		entityList.AddRange(list);
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	private void OnRamImpact(BasePlayer driver, List<BaseEntity> entities)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (entities.Count != 0)
		{
			DamageEntities(entities, driver, damageTypes, damagePoint.position);
			Head.TakeDamage(headDamagePerHit);
		}
		Pool.FreeUnmanaged<BaseEntity>(ref entities);
		GameObjectRef ramImpactEffect = GetRamImpactEffect();
		if (ramImpactEffect != null && ramImpactEffect.isValid)
		{
			Effect.server.Run(ramImpactEffect.resourcePath, damagePoint.position, -damagePoint.forward, null, broadcast: true);
		}
		if (hitEffect != null && hitEffect.isValid)
		{
			Effect.server.Run(hitEffect.resourcePath, this, 0u, Vector3.zero, Vector3.up, null, broadcast: true);
		}
		SeismicSensor.Notify(damagePoint.position, 2);
		AttackRecoilPush();
	}

	private void DamageEntities(List<BaseEntity> entityList, BaseEntity attackingPlayer, List<DamageTypeEntry> damages, Vector3 hitPos)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < entityList.Count; i++)
		{
			BaseEntity baseEntity = entityList[i];
			if ((Object)(object)baseEntity == (Object)null || !baseEntity.IsValid() || (baseEntity is BuildingBlock buildingBlock && (int)buildingBlock.grade > maxBuildingBlockGrade))
			{
				continue;
			}
			HitInfo hitInfo = new HitInfo();
			hitInfo.Initiator = attackingPlayer;
			hitInfo.WeaponPrefab = LookupPrefab();
			hitInfo.damageTypes.Add(damages);
			hitInfo.PointStart = hitPos;
			Vector3 val = baseEntity.ClosestPoint(hitPos);
			hitInfo.HitPositionWorld = baseEntity.ClosestPoint(hitPos);
			Vector3 val2 = hitPos - val;
			hitInfo.HitNormalWorld = ((Vector3)(ref val2)).normalized;
			hitInfo.PointEnd = hitInfo.HitPositionWorld;
			baseEntity.OnAttacked(hitInfo);
			BaseVehicle baseVehicle = baseEntity as BaseVehicle;
			if (baseVehicle != null)
			{
				if (baseEntity is BaseVehicleModule baseVehicleModule)
				{
					baseVehicle = baseVehicleModule.Vehicle;
				}
				baseVehicle.rigidBody.AddForceAtPosition(damagePoint.forward * 1000f, hitInfo.HitPositionWorld, (ForceMode)1);
				baseVehicle.TryShowCollisionFX(hitInfo.HitPositionWorld);
			}
		}
	}

	private GameObjectRef GetRamImpactEffect()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		GamePhysics.Trace(new Ray(damagePoint.position, damagePoint.forward), 1f, out var hitInfo, 0.2f, 1210286337, (QueryTriggerInteraction)0);
		if ((Object)(object)RaycastHitEx.GetCollider(hitInfo) != (Object)null)
		{
			uint num = StringPool.Get(((Object)(object)RaycastHitEx.GetCollider(hitInfo).sharedMaterial != (Object)null) ? AssetNameCache.GetName(RaycastHitEx.GetCollider(hitInfo).sharedMaterial) : "generic");
			ImpactEffect impactEffect = List.FindWith<ImpactEffect, uint>((IReadOnlyCollection<ImpactEffect>)impactEffects, (Func<ImpactEffect, uint>)((ImpactEffect x) => StringPool.Get(x.materialName)), num, (IEqualityComparer<uint>)null);
			if (impactEffect.prefab == null || string.IsNullOrEmpty(impactEffect.prefab.resourcePath))
			{
				impactEffect.prefab = defaultRamImpactEffect;
			}
			return impactEffect.prefab;
		}
		return null;
	}

	public void AttackRecoilPush()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rigidBody == (Object)null))
		{
			if (rigidBody.IsSleeping())
			{
				rigidBody.WakeUp();
			}
			Vector3 val = Vector3.ProjectOnPlane(((Component)this).transform.forward, ((Component)this).transform.up);
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			rigidBody.AddForce(normalized * rigidBody.mass * 1f, (ForceMode)1);
			rigidBody.AddForceAtPosition(Vector3.up * rigidBody.mass * 2.3f, centreOfMassTransform.position + ((Component)this).transform.forward * 1f, (ForceMode)1);
		}
	}

	[RPC_Server]
	public void RPC_OpenFuel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && CanBeLooted(player))
		{
			GetFuelSystem()?.LootFuel(player);
		}
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (!base.CanBeLooted(player))
		{
			return false;
		}
		if (AnyMounted())
		{
			return PlayerIsMounted(player);
		}
		return true;
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		float speed = GetSpeed();
		carPhysics.FixedUpdate(Time.fixedDeltaTime, speed);
		engineController.CheckEngineState();
		if (!HasDriver() || !IsOn())
		{
			throttleInput = 0f;
			steerInput = 0f;
		}
		if ((Object)(object)doorJoint == (Object)null && !HasFlag(Flags.Reserved15))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved14, b: false);
				flagsUpdateScope.Set(Flags.Reserved15, b: true);
			}
			SpawnDoorGib();
		}
		if ((Object)(object)doorRigidBody != (Object)null)
		{
			if (rigidBody.isKinematic != doorRigidBody.isKinematic)
			{
				doorRigidBody.isKinematic = rigidBody.isKinematic;
			}
			if (rigidBody.IsSleeping() != doorRigidBody.IsSleeping())
			{
				if (rigidBody.IsSleeping())
				{
					doorRigidBody.Sleep();
				}
				else
				{
					doorRigidBody.WakeUp();
				}
			}
		}
		if (base.IsMovingOrOn && base.CurEngineState == VehicleEngineController<GroundVehicle>.EngineState.On)
		{
			float fuelPerSecond = Mathf.Lerp(engine.idleFuelPerSec, engine.maxFuelPerSec, Mathf.Abs(GetThrottleInput()));
			engineController.TickFuel(fuelPerSecond);
		}
	}

	public override bool GetSteerSpeedMod(float speed)
	{
		return steerMod;
	}

	public override float GetSteerMaxMult(float speed)
	{
		return 1f;
	}

	public override float GetMaxDriveForce()
	{
		return (float)engine.engineKW * 10f;
	}

	public override float GetWheelsMidPos()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return (((Component)wheels[0].wheelCollider).transform.localPosition.z - ((Component)wheels[5].wheelCollider).transform.localPosition.z) * 0.5f;
	}

	public override float GetMaxForwardSpeed()
	{
		return maxForwardSpeed;
	}

	private void UpdateClients()
	{
		byte num = (byte)((GetThrottleInput() + 1f) * 7f);
		byte b = (byte)(GetBrakeInput() * 15f);
		byte arg = (byte)(num + (b << 4));
		byte arg2 = (byte)(GetFuelFraction() * 255f);
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_BatteringRamUpdate"), GetNetworkTime(), GetSteerInput(), arg, DriveWheelVelocity, arg2, DoorAngle);
	}

	protected override void ProcessCollision(Collision collision, Rigidbody ourRigidbody)
	{
		if (!base.isClient && collision != null && !((Object)(object)collision.gameObject == (Object)null) && !((Object)(object)collision.gameObject == (Object)null) && !((Object)(object)((ContactPoint)(ref collision.contacts[0])).thisCollider == (Object)(object)Head.serverCollider))
		{
			base.ProcessCollision(collision, ourRigidbody);
		}
	}

	public override void OnTowAttach()
	{
	}

	public override void OnTowDetach()
	{
	}

	public override void OnEngineStartFailed()
	{
		ClientRPC(RpcTarget.NetworkGroup("EngineStartFailed"));
	}

	public override bool MeetsEngineRequirements()
	{
		return HasDriver();
	}

	public bool HasDoor()
	{
		return !HasFlag(Flags.Reserved15);
	}

	public BatteringRamHead GetHead()
	{
		BatteringRamHead batteringRamHead = headRef.Get(base.isServer);
		if (batteringRamHead.IsValid())
		{
			return batteringRamHead;
		}
		return null;
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (child.prefabID == headPrefab.GetEntity().prefabID)
		{
			BatteringRamHead batteringRamHead = (BatteringRamHead)child;
			headRef.Set(batteringRamHead);
			batteringRamHead.batteringRamOwner = this;
		}
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		if ((Object)(object)steeringWheel != (Object)null)
		{
			steerAngle = steeringWheel.localEulerAngles;
		}
		if ((Object)(object)fuelGauge != (Object)null)
		{
			fuelAngle = fuelGauge.localEulerAngles;
		}
	}

	public bool CanAttack()
	{
		if (IsOn() && !HasFlag(Flags.Busy))
		{
			return !HasFlag(Flags.Broken);
		}
		return false;
	}

	public float GetFuelFraction()
	{
		if (base.isServer)
		{
			return engineController.FuelSystem.GetFuelFraction();
		}
		return cachedFuelFraction;
	}

	public override float AntiHackVelocity()
	{
		return Mathf.Max(GetMaxForwardSpeed() * 1.2f, 30f);
	}

	public override float GetThrottleInput()
	{
		if (base.isServer)
		{
			return Mathf.Clamp(throttleInput, -1f, 1f);
		}
		return 0f;
	}

	public override float GetBrakeInput()
	{
		if (base.isServer)
		{
			return brakeInput;
		}
		return 0f;
	}

	public override float GetSteerInput()
	{
		if (base.isServer)
		{
			return Mathf.Clamp(steerInput, -1f, 1f);
		}
		return 0f;
	}

	public override void Save(SaveInfo info)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.batteringRam = Pool.Get<BatteringRam>();
		info.msg.batteringRam.fuelStorageID = GetFuelSystem().GetInstanceID();
		info.msg.batteringRam.headID = headRef.uid;
		info.msg.batteringRam.steerInput = GetSteerInput();
		info.msg.batteringRam.driveWheelVel = DriveWheelVelocity;
		info.msg.batteringRam.throttleInput = GetThrottleInput();
		info.msg.batteringRam.brakeInput = GetBrakeInput();
		info.msg.batteringRam.fuelFraction = GetFuelFraction();
		if (HasDoor())
		{
			info.msg.batteringRam.doorAngle = DoorAngle;
		}
		info.msg.batteringRam.time = GetNetworkTime(in info.cachedTime);
	}

	public override void Load(LoadInfo info)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.batteringRam != null)
		{
			engineController.FuelSystem.SetInstanceID(info.msg.batteringRam.fuelStorageID);
			headRef.uid = info.msg.batteringRam.headID;
			cachedFuelFraction = info.msg.batteringRam.fuelFraction;
		}
	}

	public override bool IsWaterlogged()
	{
		bool result = false;
		if (base.isServer)
		{
			result = engineController.IsWaterlogged();
		}
		return result;
	}

	void IEngineControllerUser.Invoke(Action action, float time)
	{
		Invoke(action, time);
	}

	void IEngineControllerUser.CancelInvoke(Action action)
	{
		CancelInvoke(action);
	}
}
