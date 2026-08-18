using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class Shield : HeldEntity
{
	public const Flags Blocking = Flags.Reserved6;

	public float DeployDelay = 1f;

	public float ShieldOnBackToggleCooldown = 3f;

	public ProtectionProperties Protection;

	[Range(0f, 1f)]
	public float DamageMitigationFactor;

	public Collider ShieldCollider;

	[Tooltip("This is the collider for the shield when not actively in use")]
	public Collider sideShieldCollider;

	public float MaxBlockTime = 4f;

	public float MinBlockTime = 1f;

	public float damageToLoseOneSecond = 50f;

	[Tooltip("How long after we stop blocking before we begin charging our block")]
	public float chargeDelay = 1f;

	public GameObjectRef MeleeLocalPlayerImpactFxPrefab;

	public GameObjectRef RangedLocalPlayerImpactFxPrefab;

	public ShieldAnimationSubSystem ShieldAnimSystem;

	private float lastBlockTime;

	[ReplicatedVar]
	public static bool InfiniteShieldBlock;

	private Action shieldBlockTick;

	private bool serverWantsBlock;

	private static Vector3 MaximumLocalPosition;

	private static Vector3 MinimumLocalPosition;

	private static Vector3 MaximumLocalRotation;

	private static Vector3 MinimumLocalRotation;

	private TimeSince serverSideShieldBlockStarted;

	private float serverSideBlockPower;

	private TimeSince lastLocalPlayerUpdateTick;

	private HeldEntity tickingHeldEntity;

	private bool lastAppliedShieldOnBack;

	private TimeSince timeSinceShieldOnBackChanged;

	public override bool IsShield => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Shield.OnRpcMessage"))
		{
			if (rpc == 2238556937u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerToggleBlock"));
				}
				using (TimeWarning.New("ServerToggleBlock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2238556937u, "ServerToggleBlock", this, player, 10uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(2238556937u, "ServerToggleBlock", this, player))
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
							ServerToggleBlock(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ServerToggleBlock");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsBlocking()
	{
		return HasFlag(Flags.Reserved6);
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
		if (base.isServer)
		{
			ServerSideAttack(info);
		}
	}

	public bool RaycastAgainstColliders(Ray r, float maxDistance)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if ((Object)(object)ShieldCollider != (Object)null)
		{
			return ShieldCollider.Raycast(r, ref val, maxDistance);
		}
		return false;
	}

	public bool SphereCastAgainstColliders(Vector3 center, float radius)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(ClosestPoint(center), center) <= radius;
	}

	public string GetHitMaterialString()
	{
		return AssetNameCache.GetName(ShieldCollider.sharedMaterial);
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(10uL)]
	private void ServerToggleBlock(RPCMessage msg)
	{
		bool flag = msg.read.Bit();
		serverWantsBlock = flag;
		if (shieldBlockTick == null)
		{
			shieldBlockTick = ShieldBlockTick;
		}
		if (!IsInvoking(shieldBlockTick))
		{
			InvokeRepeating(shieldBlockTick, 0f, 0f);
		}
	}

	private void ServerSideAttack(HitInfo info)
	{
		Item item = GetItem();
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (IsBlocking() && (Object)(object)ownerPlayer != (Object)null)
		{
			float num = info.damageTypes.Total();
			serverSideBlockPower = Mathf.Clamp(serverSideBlockPower + num / damageToLoseOneSecond, 0f, MaxBlockTime);
			ClientRPC(RpcTarget.Player("ClientUpdateShieldPowerTime", ownerPlayer), serverSideBlockPower / MaxBlockTime);
		}
		if (item != null)
		{
			Protection.Scale(info.damageTypes);
			info.HitBone = 0u;
			float num2 = info.damageTypes.Total();
			info.damageTypes.ScaleAll(Mathf.Clamp01(1f - DamageMitigationFactor));
			float amount = num2 - info.damageTypes.Total();
			if ((Object)(object)ownerPlayer != (Object)null)
			{
				ownerPlayer.OnAttacked(info);
			}
			item.LoseCondition(amount);
		}
		bool arg = (Object)(object)info.Weapon != (Object)null && info.Weapon is BaseMelee;
		if ((Object)(object)ownerPlayer != (Object)null)
		{
			ClientRPC(RpcTarget.NetworkGroup("ClientShieldHit", ownerPlayer), arg, ((Object)(object)info.InitiatorPlayer != (Object)null) ? info.InitiatorPlayer.userID.Get() : 0);
		}
	}

	private void DestroyShield()
	{
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		foreach (BaseEntity child in children)
		{
			list.Add(child);
		}
		foreach (BaseEntity item in list)
		{
			item.SetParent(null, worldPositionStays: true);
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	public override float AntiHackPadding()
	{
		if ((Object)(object)GetOwnerPlayer() != (Object)null && GetOwnerPlayer().IsBot)
		{
			return 3f;
		}
		return 0.75f;
	}

	public override void SetHeld(bool bHeld)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		base.SetHeld(bHeld);
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer != (Object)null)
		{
			ownerPlayer.modelState.blocking = false;
			if (ownerPlayer.ActivePlayerInd != -1)
			{
				NativeArray<Flag> playerModelStateFlags = BasePlayer.PlayerStates.PlayerModelStateFlags;
				int activePlayerInd = ownerPlayer.ActivePlayerInd;
				playerModelStateFlags[activePlayerInd] = (Flag)(playerModelStateFlags[activePlayerInd] & -65537);
			}
		}
	}

	private void ShieldBlockTick()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (IsDisabled() || (Object)(object)ownerPlayer == (Object)null)
		{
			return;
		}
		HeldEntity heldEntity = ownerPlayer.GetHeldEntity();
		if ((Object)(object)heldEntity != (Object)(object)tickingHeldEntity)
		{
			tickingHeldEntity = heldEntity;
			serverSideBlockPower = 0f;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (serverWantsBlock)
		{
			if (!IsBlocking() && serverSideBlockPower < MaxBlockTime - MinBlockTime)
			{
				serverSideShieldBlockStarted = TimeSince.op_Implicit(0f);
				flagsUpdateScope.Set(Flags.Reserved6, b: true);
			}
		}
		else if (IsBlocking() && TimeSince.op_Implicit(serverSideShieldBlockStarted) > MinBlockTime)
		{
			flagsUpdateScope.Set(Flags.Reserved6, b: false);
		}
		ownerPlayer.modelState.blocking = IsBlocking();
		if (ownerPlayer.ActivePlayerInd != -1)
		{
			NativeArray<Flag> playerModelStateFlags = BasePlayer.PlayerStates.PlayerModelStateFlags;
			if (ownerPlayer.modelState.blocking)
			{
				ref NativeArray<Flag> reference = ref playerModelStateFlags;
				int activePlayerInd = ownerPlayer.ActivePlayerInd;
				reference[activePlayerInd] = (Flag)(reference[activePlayerInd] | 0x10000);
			}
			else
			{
				ref NativeArray<Flag> reference = ref playerModelStateFlags;
				int activePlayerInd = ownerPlayer.ActivePlayerInd;
				reference[activePlayerInd] = (Flag)(reference[activePlayerInd] & -65537);
			}
		}
		if (IsBlocking())
		{
			lastBlockTime = Time.realtimeSinceStartup;
		}
		bool flag = Time.realtimeSinceStartup >= lastBlockTime + chargeDelay;
		serverSideBlockPower = Mathf.MoveTowards(serverSideBlockPower, IsBlocking() ? MaxBlockTime : (flag ? 0f : serverSideBlockPower), Time.deltaTime);
		if (TimeSince.op_Implicit(lastLocalPlayerUpdateTick) > 0.5f)
		{
			ClientRPC(RpcTarget.Player("ClientUpdateShieldPowerTime", ownerPlayer), serverSideBlockPower / MaxBlockTime);
			lastLocalPlayerUpdateTick = TimeSince.op_Implicit(0f);
		}
		if (HasFlag(Flags.Reserved6) && serverSideBlockPower >= MaxBlockTime && !InfiniteShieldBlock)
		{
			flagsUpdateScope.Set(Flags.Reserved6, b: false);
		}
		if (!IsBlocking() && serverSideBlockPower <= 0f)
		{
			CancelInvoke(shieldBlockTick);
		}
	}

	public override void ServerTick(BasePlayer byPlayer)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		base.ServerTick(byPlayer);
		if (IsDisabled())
		{
			return;
		}
		bool flag = byPlayer.WantsShieldOnBack();
		if (flag != lastAppliedShieldOnBack && TimeSince.op_Implicit(timeSinceShieldOnBackChanged) > ShieldOnBackToggleCooldown)
		{
			lastAppliedShieldOnBack = flag;
			timeSinceShieldOnBackChanged = TimeSince.op_Implicit(0f);
			byPlayer.GetHeldEntity()?.UpdateShieldState(bHeld: true);
		}
		if (byPlayer.modelState != null)
		{
			Vector3 val = byPlayer.modelState.localShieldPos;
			if (Vector3Ex.IsNaNOrInfinity(val))
			{
				val = Vector3.Lerp(MinimumLocalPosition, MaximumLocalPosition, 0.5f);
			}
			Vector3 val2 = byPlayer.modelState.localShieldRot;
			if (Vector3Ex.IsNaNOrInfinity(val2))
			{
				val2 = Vector3.Lerp(MinimumLocalRotation, MaximumLocalRotation, 0.5f);
			}
			val.x = Mathf.Clamp(val.x, MinimumLocalPosition.x, MaximumLocalPosition.x);
			val.y = Mathf.Clamp(val.y, MinimumLocalPosition.y, MaximumLocalPosition.y);
			val.z = Mathf.Clamp(val.z, MinimumLocalPosition.z, MaximumLocalPosition.z);
			val2.x = Mathf.Clamp(val2.x, MinimumLocalRotation.x, MaximumLocalRotation.x);
			val2.y = Mathf.Clamp(val2.y, MinimumLocalRotation.y, MaximumLocalRotation.y);
			val2.z = Mathf.Clamp(val2.z, MinimumLocalRotation.z, MaximumLocalRotation.z);
			((Component)this).transform.SetLocalPositionAndRotation(val, Quaternion.Euler(val2));
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		bool flag = (next & Flags.Broken) != Flags.Broken && (next & Flags.Reserved4) == Flags.Reserved4;
		if (!base.isServer)
		{
			return;
		}
		if ((old & Flags.Broken) == Flags.Broken != ((next & Flags.Broken) == Flags.Broken))
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if ((Object)(object)ownerPlayer != (Object)null)
			{
				HeldEntity heldEntity = ownerPlayer.GetHeldEntity();
				if ((Object)(object)heldEntity != (Object)null)
				{
					heldEntity.UpdateShieldState(bHeld: true);
				}
				if ((next & Flags.Broken) == Flags.Broken)
				{
					DestroyShield();
					if ((Object)(object)heldEntity != (Object)null)
					{
						heldEntity.UpdateShieldState(bHeld: false);
					}
				}
			}
		}
		else if ((old & Flags.Reserved4) == Flags.Reserved4 && (next & Flags.Reserved4) != Flags.Reserved4)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved6, b: false);
			}
			serverWantsBlock = false;
		}
		SetMainColliderState(flag && (next & Flags.Reserved6) == Flags.Reserved6);
		SetSideColliderState(flag && (next & Flags.Reserved6) != Flags.Reserved6);
	}

	public void SetMainColliderState(bool enable)
	{
		ShieldCollider.enabled = enable;
	}

	public void SetSideColliderState(bool enable)
	{
		sideShieldCollider.enabled = enable;
	}

	static Shield()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		InfiniteShieldBlock = false;
		MaximumLocalPosition = new Vector3(0.39f, 1.62f, 0.41f);
		MinimumLocalPosition = new Vector3(-0.66f, 0.66f, -0.44f);
		MaximumLocalRotation = new Vector3(360f, 360f, 360f);
		MinimumLocalRotation = new Vector3(2.5f, 2.14f, 0.04f);
	}
}
