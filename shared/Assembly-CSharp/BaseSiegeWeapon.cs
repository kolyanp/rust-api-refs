using System;
using System.Collections.Generic;
using ConVar;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class BaseSiegeWeapon : GroundVehicle, TriggerHurtNotChild.IHurtTriggerUser, CarPhysics<BaseSiegeWeapon>.ICar, ITowing, IEngineControllerUser, IEntity, VehicleChassisVisuals<BaseSiegeWeapon>.IClientWheelUser
{
	public SimpleCarVisualsController controller;

	protected CarPhysics<BaseSiegeWeapon> carPhysics;

	private bool disablePhysics;

	[ServerVar(Help = "How many minutes before a siege weapon loses all its health while outside")]
	public static float outsideDecayMinutes = 600f;

	private const float DECAY_TICK_TIME = 60f;

	private const float INSIDE_DECAY_MULTIPLIER = 0.1f;

	private float lastUseTime;

	private TimeSince timeSinceDragModSet;

	private VehicleTerrainHandler terrainHandler;

	private Vector3 localPullPosition;

	private Vector3 lastPlayerPosition;

	private float playerMovementThreshold = 0.01f;

	[SerializeField]
	[Header("Siege Weapon")]
	protected Transform centreOfMassTransform;

	[SerializeField]
	protected CarSettings carSettings;

	public VisualCarWheel[] wheels;

	[Header("Towing")]
	public Transform towAnchor;

	[SerializeField]
	[Header("Pulling")]
	private List<ModifierDefintion> pullingPlayerModifiers;

	public const Flags Flag_IsPulled = Flags.Reserved12;

	private BasePlayer pullingPlayer;

	private float _mass = -1f;

	public virtual float SteerAngle { get; }

	public virtual float MaxSteerAngle { get; }

	public bool IsTowing => HasFlag(Flags.Reserved14);

	public bool IsTowingAllowed => CheckTowingAllowed();

	public BaseEntity TowEntity => this;

	public Transform TowAnchor => towAnchor;

	public Rigidbody TowBody => rigidBody;

	public VehicleTerrainHandler.Surface OnSurface
	{
		get
		{
			if (terrainHandler == null)
			{
				return VehicleTerrainHandler.Surface.Default;
			}
			return terrainHandler.OnSurface;
		}
	}

	public SiegeWeaponVehicleAudio vehicleAudio => (SiegeWeaponVehicleAudio)gvAudio;

	private float Mass
	{
		get
		{
			if (base.isServer)
			{
				return rigidBody.mass;
			}
			return _mass;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseSiegeWeapon.OnRpcMessage"))
		{
			if (rpc == 3106222818u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_StartPulling"));
				}
				using (TimeWarning.New("SERVER_StartPulling"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3106222818u, "SERVER_StartPulling", this, player, 3f))
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
							SERVER_StartPulling(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_StartPulling");
					}
				}
				return true;
			}
			if (rpc == 1702315436 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_StopPulling"));
				}
				using (TimeWarning.New("SERVER_StopPulling"))
				{
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
							SERVER_StopPulling(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SERVER_StopPulling");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		rigidBody.centerOfMass = centreOfMassTransform.localPosition;
		carPhysics = new CarPhysics<BaseSiegeWeapon>(this, ((Component)this).transform, rigidBody, carSettings);
		terrainHandler = new VehicleTerrainHandler(this);
		lastUseTime = Time.realtimeSinceStartup;
		if (!disablePhysics)
		{
			rigidBody.isKinematic = false;
		}
		InvokeRandomized(DecayTick, Random.Range(30f, 60f), 60f, 6f);
	}

	private void DecayTick()
	{
		if (base.IsDestroyed)
		{
			return;
		}
		float num = 1f;
		num /= outsideDecayMinutes;
		if (!(Time.time < lastUseTime + 300f))
		{
			if (!IsOutside())
			{
				num *= 0.1f;
			}
			Hurt(MaxHealth() * num, DamageType.Decay);
		}
	}

	public void RefreshLastUseTime()
	{
		lastUseTime = Time.time;
	}

	public void DisablePhysics()
	{
		disablePhysics = true;
		rigidBody.isKinematic = true;
	}

	public void EnablePhysics()
	{
		disablePhysics = false;
		rigidBody.isKinematic = false;
		rigidBody.WakeUp();
	}

	public virtual bool CheckTowingAllowed()
	{
		return !IsTowing;
	}

	public virtual void OnTowAttach()
	{
		EnablePhysics();
		carSettings.disableHandbrakes = true;
		carSettings.canSleep = false;
	}

	public virtual void OnTowDetach()
	{
		carSettings.disableHandbrakes = false;
		carSettings.canSleep = true;
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		float speed = GetSpeed();
		carPhysics.FixedUpdate(Time.fixedDeltaTime, speed);
		terrainHandler.FixedUpdate();
	}

	public virtual bool GetSteerSpeedMod(float speed)
	{
		return false;
	}

	public virtual float GetSteerMaxMult(float speed)
	{
		return 1f;
	}

	public virtual float GetAdjustedDriveForce(float absSpeed, float topSpeed)
	{
		float maxDriveForce = GetMaxDriveForce();
		float num = Mathf.Lerp(0.3f, 0.75f, GetPerformanceFraction());
		float num2 = MathEx.BiasedLerp(1f - absSpeed / topSpeed, num);
		return maxDriveForce * num2;
	}

	public virtual float GetPerformanceFraction()
	{
		float num = Mathf.InverseLerp(0.25f, 0.5f, base.healthFraction);
		return Mathf.Lerp(0.5f, 1f, num);
	}

	public virtual CarWheel[] GetWheels()
	{
		return wheels;
	}

	public virtual float GetWheelsMidPos()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return (((Component)wheels[0].wheelCollider).transform.localPosition.z - ((Component)wheels[2].wheelCollider).transform.localPosition.z) * 0.5f;
	}

	public override void OnDied(HitInfo info = null)
	{
		if (HasFlag(Flags.Reserved12) && (Object)(object)pullingPlayer != (Object)null)
		{
			StopPulling();
		}
		base.OnDied(info);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void SERVER_StartPulling(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (CanPullNow(player) && !((Object)(object)rigidBody == (Object)null) && Interface.CallHook("OnSiegeWeaponPull", this, msg.player) == null && (!OnlyOwnerAccessible() || !((Object)(object)player != (Object)(object)creatorEntity)))
		{
			player.metabolism.calories.Subtract(3f);
			player.metabolism.SendChanges();
			if (rigidBody.IsSleeping())
			{
				rigidBody.WakeUp();
			}
			StartPulling(player);
		}
	}

	[RPC_Server]
	public void SERVER_StopPulling(RPCMessage msg)
	{
		if (!((Object)(object)msg.player != (Object)(object)pullingPlayer))
		{
			StopPulling();
		}
	}

	private void StartPulling(BasePlayer player)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		localPullPosition = ((Component)this).transform.InverseTransformPoint(((Component)player).transform.position);
		lastPlayerPosition = ((Component)player).transform.position;
		pullingPlayer = player;
		if ((Object)(object)pullingPlayer != (Object)null)
		{
			PlayerModifiers.AddToPlayer(pullingPlayer, pullingPlayerModifiers);
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved12, b: true);
		}
		carSettings.disableHandbrakes = true;
		InvokeRepeating(DoPullAction, 0f, 0f);
	}

	private void StopPulling()
	{
		if ((Object)(object)pullingPlayer != (Object)null)
		{
			pullingPlayer.modifiers.RemoveFromSource(Modifier.ModifierSource.Interaction);
		}
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_StopPulling"));
		pullingPlayer = null;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved12, b: false);
		}
		carSettings.disableHandbrakes = false;
		CancelInvoke(DoPullAction);
	}

	protected virtual void DoPullAction()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rigidBody == (Object)null || (Object)(object)pullingPlayer == (Object)null)
		{
			StopPulling();
			return;
		}
		Vector3 position = ((Component)pullingPlayer).transform.position;
		Vector3 val = ((Component)this).transform.TransformPoint(localPullPosition);
		if (Vector3.Distance(position, val) >= 1f || pullingPlayer.IsDead() || pullingPlayer.IsSleeping())
		{
			StopPulling();
			return;
		}
		Vector3 val2 = ((Component)pullingPlayer).transform.position - lastPlayerPosition;
		lastPlayerPosition = ((Component)pullingPlayer).transform.position;
		if (((Vector3)(ref val2)).magnitude > playerMovementThreshold)
		{
			Vector3 val3 = rigidBody.linearVelocity;
			if (((Vector3)(ref val3)).magnitude < 1.5f)
			{
				val3 = position - val;
				Vector3 normalized = ((Vector3)(ref val3)).normalized;
				float mass = rigidBody.mass;
				rigidBody.AddForceAtPosition(normalized * mass, val, (ForceMode)0);
			}
		}
	}

	public override float GetThrottleInput()
	{
		return 0f;
	}

	public override float GetBrakeInput()
	{
		if (base.isServer)
		{
			if (!IsTowing)
			{
				return 1f;
			}
			return 0f;
		}
		return 1f;
	}

	public override float GetMaxForwardSpeed()
	{
		return GetMaxDriveForce() / Mass * 2f;
	}

	public virtual float GetMaxDriveForce()
	{
		return 100f;
	}

	public virtual float GetSteerInput()
	{
		return 0f;
	}

	public virtual bool IsWaterlogged()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)waterloggedPoint != (Object)null)
		{
			return WaterLevel.Test(waterloggedPoint.position, waves: true, volumes: true, this);
		}
		return false;
	}

	protected override bool CanPushNow(BasePlayer pusher)
	{
		if (!base.CanPushNow(pusher))
		{
			return false;
		}
		if (HasFlag(Flags.Reserved12))
		{
			return false;
		}
		if (pusher.isMounted || pusher.IsSwimming())
		{
			return false;
		}
		return !pusher.IsStandingOnEntity(this, 8192);
	}

	protected virtual bool CanPullNow(BasePlayer puller)
	{
		if (HasFlag(Flags.Reserved12) || (Object)(object)pullingPlayer != (Object)null)
		{
			return false;
		}
		if (puller.isMounted || puller.IsSwimming())
		{
			return false;
		}
		if (!puller.IsStandingOnEntity(this, 8192))
		{
			return puller.CanInteract();
		}
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && info.fromDisk)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved12, b: false);
			}
		}
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
