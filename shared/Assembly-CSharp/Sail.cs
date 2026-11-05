using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class Sail : DecayEntity, IBoatBuildingPiece, IBoatPropulsion
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
		ToggleColliders();
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

	private void CacheIsWindBlocked()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (!Lowered && !Lowering)
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		if (!IsOutside())
		{
			flagsUpdateScope.Set(Flags.Reserved14, b: true);
			return;
		}
		bool b = IsLocationWindBlocked(WindBlockedCheckPoints, ((Component)this).transform.position, ((Component)this).transform.rotation, WindBlockedCheckRadius, WindBlockedCheckDistance, this);
		flagsUpdateScope.Set(Flags.Reserved14, b);
	}

	public bool WouldBlockSail(Vector3 blockerPos, Quaternion blockerRot, List<Transform> blockedCheckPoints, Vector3 blockedPos, Quaternion blockedRot)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BoxCollider> val = Pool.Get<PooledList<BoxCollider>>();
		try
		{
			((Component)this).GetComponentsInChildren<BoxCollider>(true, (List<BoxCollider>)(object)val);
			Matrix4x4 val2 = Matrix4x4.TRS(blockerPos, blockerRot, Vector3.one);
			Matrix4x4 val3 = Matrix4x4.TRS(blockedPos, blockedRot, Vector3.one);
			RaycastHit val9 = default;
			foreach (BoxCollider item in (List<BoxCollider>)(object)val)
			{
				if (((Collider)item).isTrigger)
				{
					continue;
				}
				Vector3 val4 = item.size + Vector3.one * (WindBlockedCheckRadius * 2f);
				Vector3 val5 = val2.MultiplyPoint3x4(((Component)item).transform.localPosition);
				Quaternion val6 = blockerRot * ((Component)item).transform.localRotation;
				OBB val7 = new OBB(val5, val6, new Bounds(item.center, val4));
				foreach (Transform blockedCheckPoint in blockedCheckPoints)
				{
					Ray val8 = new Ray(val3.MultiplyPoint3x4(blockedCheckPoint.localPosition), blockedRot * blockedCheckPoint.localRotation * Vector3.forward);
					if (val7.Trace(val8, ref val9, WindBlockedCheckDistance))
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

	public static bool IsLocationWindBlocked(List<Transform> checkPoints, Vector3 worldPosition, Quaternion worldRotation, float radius, float distance, BaseEntity toIgnore, bool ignoreClient = true, bool ignoreSails = false)
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
			GamePhysics.TraceAllUnordered(new Ray(val.MultiplyPoint3x4(checkPoint.localPosition), worldRotation * checkPoint.localRotation * Vector3.forward), radius, list, distance, layerMask, (QueryTriggerInteraction)0, toIgnore);
			for (int i = 0; i < list.Count; i++)
			{
				BaseEntity entity = RaycastHitEx.GetEntity(list[i]);
				if (!((Object.op_Implicit((Object)(object)entity) && entity.isClient) & ignoreClient) && (!ignoreSails || !(entity is Sail)))
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
		if (isServer && info.fromDisk)
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
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
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
			CacheIsWindBlocked();
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
		CacheIsWindBlocked();
		float sailBlockedUpdateInterval = PlayerBoat.SailBlockedUpdateInterval;
		InvokeRandomized(CacheIsWindBlocked, sailBlockedUpdateInterval, sailBlockedUpdateInterval, sailBlockedUpdateInterval * 0.1f);
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
		CancelInvoke(CacheIsWindBlocked);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: false);
			flagsUpdateScope.Set(Flags.Reserved13, b: false);
			flagsUpdateScope.Set(Flags.Reserved12, b: false);
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
			flagsUpdateScope.Set(Flags.Reserved14, b: false);
		}
		ToggleColliders();
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
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
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (CanRotate(player))
		{
			Transform transform = ((Component)this).transform;
			transform.localRotation *= Quaternion.AngleAxis(180f, Vector3.up);
			SendNetworkUpdateImmediate();
			if (sailRotateEffect.isValid)
			{
				Effect.server.Run(sailRotateEffect.resourcePath, this, 0u, default, default, null, false, null, 0, Effect.Type.Generic);
			}
		}
	}

	void IBoatBuildingPiece.OnAddedToBoat(PlayerBoat boat)
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
