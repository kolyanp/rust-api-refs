using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class Sail : DecayEntity, global::IBoatBuildingPiece, IBoatPropulsion
{
	[ReplicatedVar]
	public static float MaxThrustMultiplier = 1f;

	[Header("Sail")]
	[SerializeField]
	private float maxThrust = 1000f;

	public float RaiseDuration = 1.5f;

	public float LowerDuration = 1.5f;

	public GameObject LoweredCollider;

	public GameObject RaisedCollider;

	public List<Transform> WindBlockedCheckPoints;

	public float WindBlockedCheckRadius = 0.5f;

	public float WindBlockedCheckDistance = 1.5f;

	[Header("Visuals")]
	public Transform SailVisualRoot;

	public Animator Animator;

	public GameObject RaisedFarVisual;

	public GameObject LoweredFarVisual;

	public GameObjectRef sailRotateEffect;

	public const Flags Flag_Lowered = Flags.Reserved3;

	public const Flags Flag_Lowering = Flags.Reserved12;

	public const Flags Flag_Raising = Flags.Reserved13;

	public const Flags Flag_WindBlocked = Flags.Reserved14;

	private static readonly int WindBlockedHash = Animator.StringToHash("windblocked");

	private static readonly int LoweredHash = Animator.StringToHash("lowered");

	private static readonly int LoweringHash = Animator.StringToHash("lowering");

	private static readonly int RaisingHash = Animator.StringToHash("raising");

	private TimeUntil timeUntilLoweredRaised;

	public float MaxThrust => maxThrust * MaxThrustMultiplier;

	public bool Lowering => HasFlag(Flags.Reserved12);

	public bool Raising => HasFlag(Flags.Reserved13);

	public bool Lowered => HasFlag(Flags.Reserved3);

	public bool Blowing
	{
		get
		{
			if (Lowered || Lowering || Raising)
			{
				return !WindBlocked;
			}
			return false;
		}
	}

	public bool WindBlocked => HasFlag(Flags.Reserved14);

	public Vector3 ThrustPosition
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position + ((Component)this).transform.up * 1f;
		}
	}

	public Vector3 Direction
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.forward;
		}
	}

	float IBoatPropulsion.MaxThrust => MaxThrust;

	public float CurrentThrust
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			if (Blowing)
			{
				if (Lowering)
				{
					return (LowerDuration - TimeUntil.op_Implicit(timeUntilLoweredRaised)) / LowerDuration * MaxThrust;
				}
				if (Raising)
				{
					return TimeUntil.op_Implicit(timeUntilLoweredRaised) / RaiseDuration * MaxThrust;
				}
				return MaxThrust;
			}
			return 0f;
		}
	}

	public float ThrustRatio => CurrentThrust / MaxThrust;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Sail.OnRpcMessage"))
		{
			if (rpc == 842631481 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - LowerSail"));
				}
				using (TimeWarning.New("LowerSail"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(842631481u, "LowerSail", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(842631481u, "LowerSail", this, player, 3f))
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
							LowerSail(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in LowerSail");
					}
				}
				return true;
			}
			if (rpc == 1744516204 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RaiseSail"));
				}
				using (TimeWarning.New("RaiseSail"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1744516204u, "RaiseSail", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1744516204u, "RaiseSail", this, player, 3f))
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
							RaiseSail(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RaiseSail");
					}
				}
				return true;
			}
			if (rpc == 2730316685u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RotateSail"));
				}
				using (TimeWarning.New("RotateSail"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2730316685u, "RotateSail", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2730316685u, "RotateSail", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RotateSail(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RotateSail");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ResetFlags();
		CacheIsWindBlocked();
	}

	private void ResetFlags()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved14, b: false);
		flagsUpdateScope.Set(Flags.Reserved3, b: false);
		flagsUpdateScope.Set(Flags.Reserved13, b: false);
		flagsUpdateScope.Set(Flags.Reserved12, b: false);
		flagsUpdateScope.Set(Flags.Busy, b: false);
	}

	public override void Hurt(HitInfo info)
	{
		PlayerBoat parentPlayerBoat = PlayerBoat.GetParentPlayerBoat(this);
		if ((Object)(object)parentPlayerBoat != (Object)null && !parentPlayerBoat.IsDestructibleWreck)
		{
			parentPlayerBoat.OnBoatDeployableHurt(this, info);
		}
		else
		{
			base.Hurt(info);
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		CancelInvoke(CacheIsWindBlocked);
		InvokeRandomized(CacheIsWindBlocked, 0f, 5f, 2f);
	}

	private void CacheIsWindBlocked()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!Lowered)
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (!IsOutside())
		{
			flagsUpdateScope.Set(Flags.Reserved14, b: true);
			return;
		}
		bool b = IsLocationWindBlocked(WindBlockedCheckPoints, ((Component)this).transform.position, ((Component)this).transform.rotation, WindBlockedCheckRadius, WindBlockedCheckDistance, this);
		flagsUpdateScope.Set(Flags.Reserved14, b);
	}

	public bool WouldSailBeBlockedByNewSailInLocation(Vector3 newSailPos, Quaternion newSailRot)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BoxCollider> val = Pool.Get<PooledList<BoxCollider>>();
		try
		{
			((Component)this).GetComponentsInChildren<BoxCollider>(true, (List<BoxCollider>)(object)val);
			Matrix4x4 val2 = Matrix4x4.TRS(newSailPos, newSailRot, Vector3.one);
			OBB val3 = default(OBB);
			Ray val4 = default(Ray);
			RaycastHit val5 = default(RaycastHit);
			foreach (BoxCollider item in (List<BoxCollider>)(object)val)
			{
				if (((Collider)item).isTrigger)
				{
					continue;
				}
				((OBB)(ref val3))._002Ector(((Component)item).transform.localPosition, ((Component)item).transform.localRotation, new Bounds(item.center, item.size));
				val3.position = ((Matrix4x4)(ref val2)).MultiplyPoint3x4(val3.position);
				val3.rotation = ((Matrix4x4)(ref val2)).rotation * val3.rotation;
				foreach (Transform windBlockedCheckPoint in WindBlockedCheckPoints)
				{
					((Ray)(ref val4))._002Ector(windBlockedCheckPoint.position, windBlockedCheckPoint.rotation * Vector3.forward);
					if (((OBB)(ref val3)).Trace(val4, ref val5, WindBlockedCheckDistance))
					{
						return true;
					}
				}
			}
			return false;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static bool IsLocationWindBlocked(List<Transform> checkPoints, Vector3 worldPosition, Quaternion worldRotation, float radius, float distance, BaseEntity toIgnore, bool ignoreClient = true)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		int layerMask = 136323328;
		List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
		Matrix4x4 val = Matrix4x4.TRS(worldPosition, worldRotation, Vector3.one);
		foreach (Transform checkPoint in checkPoints)
		{
			list.Clear();
			GamePhysics.TraceAllUnordered(new Ray(((Matrix4x4)(ref val)).MultiplyPoint3x4(checkPoint.localPosition), worldRotation * checkPoint.localRotation * Vector3.forward), radius, list, distance, layerMask, (QueryTriggerInteraction)0, toIgnore);
			for (int i = 0; i < list.Count; i++)
			{
				BaseEntity entity = RaycastHitEx.GetEntity(list[i]);
				if (!((Object.op_Implicit((Object)(object)entity) && entity.isClient) & ignoreClient))
				{
					Pool.FreeUnmanaged<RaycastHit>(ref list);
					return true;
				}
			}
		}
		Pool.FreeUnmanaged<RaycastHit>(ref list);
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && info.fromDisk)
		{
			Raise(null, instant: true);
		}
		ToggleColliders();
	}

	private void ToggleColliders()
	{
		RaisedCollider.SetActive(!Lowered);
		LoweredCollider.SetActive(Lowered);
	}

	public bool CanBeRaised(BasePlayer player)
	{
		object obj = Interface.CallHook("CanRaiseSail", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if ((Object)(object)player != (Object)null && !PlayerBoat.IsPlayerAuthedOnChildEntity(this, player, authedIfNoPrivOrLock: true))
		{
			return false;
		}
		if (!PlayerBoat.IsChildOfInteractablePlayerBoat(this))
		{
			return false;
		}
		if (IsBusy())
		{
			return false;
		}
		if (!Lowered)
		{
			return false;
		}
		return true;
	}

	public bool CanBeLowered(BasePlayer player)
	{
		object obj = Interface.CallHook("CanLowerSail", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if ((Object)(object)player != (Object)null && !PlayerBoat.IsPlayerAuthedOnChildEntity(this, player, authedIfNoPrivOrLock: true))
		{
			return false;
		}
		if (!PlayerBoat.IsChildOfInteractablePlayerBoat(this))
		{
			return false;
		}
		if (IsBusy())
		{
			return false;
		}
		if (Lowered)
		{
			return false;
		}
		return true;
	}

	public bool CanRotate(BasePlayer player)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		object obj = Interface.CallHook("CanRotateSail", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (IsBusy())
		{
			return false;
		}
		if (Lowered || Lowering)
		{
			return false;
		}
		if (!PlayerBoat.IsPlayerAuthedOnChildEntity(this, player, authedIfNoPrivOrLock: true))
		{
			return false;
		}
		DeployVolume[] volumes = PrefabAttribute.server.FindAll<DeployVolume>(prefabID);
		return DeployVolume.Check(((Component)this).transform.position, ((Component)this).transform.rotation * Quaternion.AngleAxis(180f, Vector3.up), volumes, ~(1 << ((Component)this).gameObject.layer));
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	public void LowerSail(RPCMessage msg)
	{
		Lower(msg.player);
	}

	public void Lower(BasePlayer player)
	{
		if (CanBeLowered(player))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: true);
				flagsUpdateScope.Set(Flags.Reserved12, b: true);
			}
			WaitForLower();
		}
	}

	private void WaitForLower()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		CancelInvoke(OnFullyLowered);
		timeUntilLoweredRaised = TimeUntil.op_Implicit(LowerDuration);
		Invoke(OnFullyLowered, LowerDuration);
	}

	private void OnFullyLowered()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: false);
			flagsUpdateScope.Set(Flags.Reserved12, b: false);
			flagsUpdateScope.Set(Flags.Reserved13, b: false);
			flagsUpdateScope.Set(Flags.Reserved3, b: true);
		}
		OnRaisedOrLowered();
	}

	private void OnRaisedOrLowered()
	{
		CacheIsWindBlocked();
		ToggleColliders();
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RaiseSail(RPCMessage msg)
	{
		Raise(msg.player);
	}

	public void Raise(BasePlayer player, bool instant = false)
	{
		if (instant)
		{
			OnFullyRaised();
		}
		else if (CanBeRaised(player))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: true);
				flagsUpdateScope.Set(Flags.Reserved13, b: true);
			}
			WaitForRaise();
		}
	}

	private void WaitForRaise()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		CancelInvoke(OnFullyRaised);
		timeUntilLoweredRaised = TimeUntil.op_Implicit(RaiseDuration);
		Invoke(OnFullyRaised, RaiseDuration);
	}

	private void OnFullyRaised()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: false);
			flagsUpdateScope.Set(Flags.Reserved13, b: false);
			flagsUpdateScope.Set(Flags.Reserved12, b: false);
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		OnRaisedOrLowered();
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	public void RotateSail(RPCMessage msg)
	{
		RotateSail(msg.player);
	}

	private void RotateSail(BasePlayer player)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (CanRotate(player))
		{
			Transform transform = ((Component)this).transform;
			transform.localRotation *= Quaternion.AngleAxis(180f, Vector3.up);
			CacheIsWindBlocked();
			SendNetworkUpdateImmediate();
			if (sailRotateEffect.isValid)
			{
				Effect.server.Run(sailRotateEffect.resourcePath, this, 0u, default(Vector3), default(Vector3), null, false, null, 0, Effect.Type.Generic);
			}
		}
	}

	void global::IBoatBuildingPiece.OnAddedToBoat(PlayerBoat boat)
	{
		Raise(null);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if ((old & Flags.Reserved3) == Flags.Reserved3 != ((next & Flags.Reserved3) == Flags.Reserved3))
		{
			ToggleColliders();
		}
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (base.ShouldDisplayPickupOption(player))
		{
			return !PlayerBoat.IsChildOfFinishedPlayerBoat(this);
		}
		return false;
	}
}
