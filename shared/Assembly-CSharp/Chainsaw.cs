using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Chainsaw : BaseMelee
{
	public float attackFadeInTime = 0.1f;

	public float attackFadeInDelay = 0.1f;

	public float attackFadeOutTime = 0.1f;

	public float idleFadeInTimeFromOff = 0.1f;

	public float idleFadeInTimeFromAttack = 0.3f;

	public float idleFadeInDelay = 0.1f;

	public float idleFadeOutTime = 0.1f;

	public ChainsawAudioAdvanced chainsawAudioAdvanced;

	public static readonly Phrase UnloadAmmoTitle = new Phrase("unload_ammo", "Unload Ammo");

	public static readonly Phrase UnloadAmmoDesc = new Phrase("unload_ammo_desc", "Unload the ammunition in this weapon and place it in your inventory.");

	[Header("Chainsaw")]
	public float fuelPerSec = 1f;

	public int maxAmmo = 100;

	public int ammo = 100;

	public ItemDefinition fuelType;

	public float reloadDuration = 2.5f;

	public float engineStartChance = 0.33f;

	public Renderer[] chainRenderers;

	[InspectorName("Use Material Scrolling UVs")]
	public bool useMaterialScrollingUVs;

	[Header("Sounds")]
	public SoundPlayer idleLoop;

	public SoundPlayer attackLoopAir;

	public SoundPlayer revUp;

	public SoundPlayer revDown;

	public SoundPlayer offSound;

	private int failedAttempts;

	private Action _actionDelayedStopAttack;

	private Action _actionEngineTick;

	private Action _actionAttackTick;

	private Action _actionDisableHitEffects;

	private TimeSince lastReloadSignalFromClient;

	private float ammoRemainder;

	private Action actionDelayedStopAttack => DelayedStopAttack;

	private Action actionEngineTick => EngineTick;

	private Action actionAttackTick => AttackTick;

	private Action actionDisableHitEffects => DisableHitEffects;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Chainsaw.OnRpcMessage"))
		{
			if (rpc == 3381353917u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DoReload"));
				}
				using (TimeWarning.New("DoReload"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(3381353917u, "DoReload", this, player))
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
							DoReload(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in DoReload");
					}
				}
				return true;
			}
			if (rpc == 706698034 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_SetAttacking"));
				}
				using (TimeWarning.New("Server_SetAttacking"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(706698034u, "Server_SetAttacking", this, player))
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
							Server_SetAttacking(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in Server_SetAttacking");
					}
				}
				return true;
			}
			if (rpc == 3881794867u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_StartEngine"));
				}
				using (TimeWarning.New("Server_StartEngine"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(3881794867u, "Server_StartEngine", this, player))
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
							Server_StartEngine(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in Server_StartEngine");
					}
				}
				return true;
			}
			if (rpc == 841093980 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_StopEngine"));
				}
				using (TimeWarning.New("Server_StopEngine"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(841093980u, "Server_StopEngine", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_StopEngine(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in Server_StopEngine");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool EngineOn()
	{
		return HasFlag(Flags.On);
	}

	public bool IsAttacking()
	{
		return HasFlag(Flags.Busy);
	}

	public bool Flags_IsHittingSomething()
	{
		return HasAny(Flags.Reserved6 | Flags.Reserved7 | Flags.Reserved8);
	}

	protected override void OnReceivedSignalServer(Signal signal, string arg)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		base.OnReceivedSignalServer(signal, arg);
		if (signal == Signal.Reload && base.isServer)
		{
			lastReloadSignalFromClient = TimeSince.op_Implicit(0f);
		}
	}

	public void ServerNPCStart()
	{
		if (!HasFlag(Flags.On))
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if ((Object)(object)ownerPlayer != (Object)null && ownerPlayer.IsNpc)
			{
				DoReload(default(RPCMessage));
				SetEngineStatus(status: true, FlagsUpdateMode.SendNetworkUpdate_Flags);
			}
		}
	}

	public override void ServerUse(HeldEntityServerUseParams parameters)
	{
		base.ServerUse(parameters);
		SetAttackStatus(status: true, FlagsUpdateMode.SendNetworkUpdate_Flags);
		Invoke(actionDelayedStopAttack, attackSpacing + 0.5f);
	}

	public override void ServerUse_OnHit(HitInfo info)
	{
		EnableHitEffect(info.HitMaterial);
	}

	private void DelayedStopAttack()
	{
		SetAttackStatus(status: false, FlagsUpdateMode.SendNetworkUpdate_Flags);
	}

	protected override bool VerifyClientAttack(BasePlayer player)
	{
		if (!EngineOn() || !IsAttacking())
		{
			return false;
		}
		return base.VerifyClientAttack(player);
	}

	public override void CollectedForCrafting(Item item, BasePlayer crafter)
	{
		ServerCommand(item, "unload_ammo", crafter);
	}

	public override void SetHeld(bool bHeld)
	{
		if (!bHeld)
		{
			SetEngineStatus(status: false, FlagsUpdateMode.SendNetworkUpdate_Flags);
		}
		base.SetHeld(bHeld);
	}

	public void ReduceAmmo(float firingTime)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer != (Object)null && ownerPlayer.IsNpc)
		{
			return;
		}
		int num = ammo;
		ammoRemainder += firingTime;
		if (ammoRemainder >= 1f)
		{
			int num2 = Mathf.FloorToInt(ammoRemainder);
			ammoRemainder -= num2;
			if (ammoRemainder >= 1f)
			{
				num2++;
				ammoRemainder -= 1f;
			}
			ammo -= num2;
			if (ammo <= 0)
			{
				ammo = 0;
			}
		}
		if ((float)ammo <= 0f)
		{
			SetEngineStatus(status: false, (num == ammo) ? FlagsUpdateMode.SendNetworkUpdate_Flags : FlagsUpdateMode.Local);
		}
		if (num != ammo)
		{
			SendNetworkUpdate();
		}
		MarkChainsawItemDirty();
	}

	private void MarkChainsawItemDirty()
	{
		GetItem()?.MarkDirty();
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	public void DoReload(RPCMessage msg)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!((Object)(object)ownerPlayer == (Object)null) && !IsAttacking() && (ownerPlayer.IsNpc || (!(TimeSince.op_Implicit(lastReloadSignalFromClient) < reloadDuration * 0.25f) && !(TimeSince.op_Implicit(lastReloadSignalFromClient) > reloadDuration * 2f))))
		{
			Item item;
			while (ammo < maxAmmo && (item = GetAmmo()) != null && item.amount > 0)
			{
				int num = Mathf.Min(maxAmmo - ammo, item.amount);
				ammo += num;
				item.UseItem(num);
			}
			MarkChainsawItemDirty();
			SendNetworkUpdateImmediate();
			ItemManager.DoRemoves();
			ownerPlayer.inventory.ServerUpdate(0f);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.baseProjectile = Pool.Get<BaseProjectile>();
		info.msg.baseProjectile.primaryMagazine = Pool.Get<Magazine>();
		info.msg.baseProjectile.primaryMagazine.contents = ammo;
	}

	public void SetEngineStatus(bool status, FlagsUpdateMode flagsUpdateMode)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(flagsUpdateMode))
		{
			if (!status)
			{
				SetAttackStatus(status: false, FlagsUpdateMode.Local);
			}
			flagsUpdateScope.Set(Flags.On, status);
		}
		CancelInvoke(actionEngineTick);
		if (status)
		{
			InvokeRepeating(actionEngineTick, 0f, 1f);
		}
	}

	public void SetAttackStatus(bool status, FlagsUpdateMode flagsUpdateMode)
	{
		if (!EngineOn())
		{
			status = false;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(flagsUpdateMode))
		{
			flagsUpdateScope.Set(Flags.Busy, status);
		}
		CancelInvoke(actionAttackTick);
		if (status)
		{
			InvokeRepeating(actionAttackTick, 0f, 1f);
		}
	}

	public void EngineTick()
	{
		ReduceAmmo(0.05f);
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer != (Object)null && ownerPlayer.IsSleeping())
		{
			SetEngineStatus(status: false, FlagsUpdateMode.SendNetworkUpdate_Flags);
		}
	}

	public void AttackTick()
	{
		ReduceAmmo(fuelPerSec);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	public void Server_StartEngine(RPCMessage msg)
	{
		if (ammo > 0 && !EngineOn())
		{
			ReduceAmmo(0.25f);
			bool num = Random.Range(0f, 1f) <= engineStartChance;
			if (!num)
			{
				failedAttempts++;
			}
			if (num || failedAttempts >= 3)
			{
				failedAttempts = 0;
				SetEngineStatus(status: true, FlagsUpdateMode.SendNetworkUpdate_Flags);
			}
		}
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	public void Server_StopEngine(RPCMessage msg)
	{
		SetEngineStatus(status: false, FlagsUpdateMode.SendNetworkUpdate_Flags);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	public void Server_SetAttacking(RPCMessage msg)
	{
		bool flag = msg.read.Bit();
		if (IsAttacking() != flag && EngineOn())
		{
			SetAttackStatus(flag, FlagsUpdateMode.SendNetworkUpdate_Flags);
		}
	}

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (item == null || !(command == "unload_ammo"))
		{
			return;
		}
		int num = ammo;
		if (num > 0)
		{
			ammo = 0;
			item.MarkDirty();
			SendNetworkUpdateImmediate();
			Item item2 = ItemManager.Create(fuelType, num, 0uL, isServerSide: true, 0uL);
			if (!item2.MoveToContainer(player.inventory.containerMain))
			{
				item2.Drop(player.GetDropPosition(), player.GetDropVelocity());
			}
			MarkChainsawItemDirty();
		}
	}

	public void DisableHitEffects()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved6, b: false);
		flagsUpdateScope.Set(Flags.Reserved7, b: false);
		flagsUpdateScope.Set(Flags.Reserved8, b: false);
	}

	public void EnableHitEffect(uint hitMaterial)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved6, b: false);
		flagsUpdateScope.Set(Flags.Reserved7, b: false);
		flagsUpdateScope.Set(Flags.Reserved8, b: false);
		if (hitMaterial == StringPool.Get("Flesh"))
		{
			flagsUpdateScope.Set(Flags.Reserved8, b: true);
		}
		else if (hitMaterial == StringPool.Get("Wood"))
		{
			flagsUpdateScope.Set(Flags.Reserved7, b: true);
		}
		else
		{
			flagsUpdateScope.Set(Flags.Reserved6, b: true);
		}
		CancelInvoke(actionDisableHitEffects);
		Invoke(actionDisableHitEffects, 0.5f);
	}

	public override void DoAttackShared(HitInfo info)
	{
		base.DoAttackShared(info);
		if (base.isServer)
		{
			EnableHitEffect(info.HitMaterial);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.baseProjectile != null && info.msg.baseProjectile.primaryMagazine != null)
		{
			ammo = info.msg.baseProjectile.primaryMagazine.contents;
		}
	}

	public bool HasAmmo()
	{
		return GetAmmo() != null;
	}

	public Item GetAmmo()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!Object.op_Implicit((Object)(object)ownerPlayer))
		{
			return null;
		}
		object obj = Interface.CallHook("OnInventoryAmmoItemFind", ownerPlayer.inventory, fuelType);
		if (obj is Item)
		{
			return (Item)obj;
		}
		return ownerPlayer.inventory.FindItemByItemName(fuelType.shortname);
	}
}
