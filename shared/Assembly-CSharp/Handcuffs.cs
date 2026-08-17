using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class Handcuffs : BaseMelee
{
	public static int PrisonerHoodItemID = -892718768;

	[ServerVar(Help = "(Generated) Damage dealt to a restrained (handcuffed) player when they attempt to push or escape; default 5")]
	public static float restrainedPushDamage = 5f;

	[ServerVar(Help = "(Generated) Maximum handcuff condition loss fraction applied per push attempt; at 0.4 the cuffs lose up to 40% condition per escape push")]
	public static float maxConditionRepairLossOnPush = 0.4f;

	[Header("Handcuffs")]
	public AnimatorOverrideController CaptiveHoldAnimationOverride;

	public GameObjectRef lockEffect;

	public GameObjectRef escapeEffect;

	[Header("Handcuff Behaviour")]
	public bool BlockInventory = true;

	public bool BlockSuicide = true;

	public bool BlockUse = true;

	public bool BlockCrafting = true;

	public float UnlockMiniGameDuration = 60f;

	public float UseDistance = 1.8f;

	public float ConditionLossPerSecond = 1f;

	private float unlockStartTime;

	private float startCondition;

	public bool Locked
	{
		get
		{
			if (GetItem() != null)
			{
				return GetItem().IsOn();
			}
			return false;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Handcuffs.OnRpcMessage"))
		{
			if (rpc == 695796023 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ReqCancelUnlockMiniGame"));
				}
				using (TimeWarning.New("RPC_ReqCancelUnlockMiniGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(695796023u, "RPC_ReqCancelUnlockMiniGame", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(695796023u, "RPC_ReqCancelUnlockMiniGame", this, player))
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
							RPC_ReqCancelUnlockMiniGame(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_ReqCancelUnlockMiniGame");
					}
				}
				return true;
			}
			if (rpc == 3883360127u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ReqCompleteUnlockMiniGame"));
				}
				using (TimeWarning.New("RPC_ReqCompleteUnlockMiniGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3883360127u, "RPC_ReqCompleteUnlockMiniGame", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3883360127u, "RPC_ReqCompleteUnlockMiniGame", this, player))
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
							RPC_ReqCompleteUnlockMiniGame(rpc3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_ReqCompleteUnlockMiniGame");
					}
				}
				return true;
			}
			if (rpc == 1571851761 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ReqLock"));
				}
				using (TimeWarning.New("RPC_ReqLock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1571851761u, "RPC_ReqLock", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1571851761u, "RPC_ReqLock", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_ReqLock(rpc4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_ReqLock");
					}
				}
				return true;
			}
			if (rpc == 3248381320u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ReqStartUnlockMiniGame"));
				}
				using (TimeWarning.New("RPC_ReqStartUnlockMiniGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3248381320u, "RPC_ReqStartUnlockMiniGame", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3248381320u, "RPC_ReqStartUnlockMiniGame", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_ReqStartUnlockMiniGame(rpc5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RPC_ReqStartUnlockMiniGame");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		Item item = GetItem();
		if (base.isServer && item != null)
		{
			SetLocked(Locked);
		}
		SetWMLocked(Locked);
	}

	private void SetWMLocked(bool flag)
	{
	}

	private void StartUnlockMiniGame()
	{
		InterruptUnlockMiniGame();
		unlockStartTime = Time.realtimeSinceStartup;
	}

	public void HeldWhenOwnerDied(BasePlayer player)
	{
		if (Locked)
		{
			SetLocked(flag: false, player);
		}
	}

	public void SetLocked(bool flag, BasePlayer player = null, Item handcuffsItem = null)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (base.isClient)
		{
			return;
		}
		if (handcuffsItem == null)
		{
			handcuffsItem = GetOwnerItem();
		}
		handcuffsItem?.SetFlag(Item.Flag.IsOn, flag);
		if ((Object)(object)player == (Object)null)
		{
			player = GetOwnerPlayer();
		}
		if (!((Object)(object)player == (Object)null))
		{
			player.SetPlayerFlag(BasePlayer.PlayerFlags.IsRestrained, flag);
			if (handcuffsItem != null)
			{
				player.restraintItemId = (flag ? new ItemId?(handcuffsItem.uid) : ((ItemId?)null));
			}
			else
			{
				player.restraintItemId = null;
			}
			if (BlockInventory)
			{
				player.inventory.SetLockedByRestraint(flag);
			}
			ClientRPC(RpcTarget.Player("CL_SetLocked", player), Locked);
		}
	}

	[ServerVar(Help = "(Generated) Toggles the locked state of the handcuffs held by the calling admin player, switching between locked and unlocked")]
	public static void togglecuffslocked(ConsoleSystem.Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		HeldEntity heldEntity = basePlayer.GetHeldEntity();
		if (!((Object)(object)heldEntity == (Object)null))
		{
			Handcuffs handcuffs = heldEntity as Handcuffs;
			if (!((Object)(object)handcuffs == (Object)null))
			{
				handcuffs.SetLocked(!handcuffs.Locked, basePlayer);
			}
		}
	}

	private void ModifyConditionForElapsedTime(float elapsed)
	{
		if (unlockStartTime <= 0f || elapsed <= 0f)
		{
			return;
		}
		Item ownerItem = GetOwnerItem();
		if (ownerItem == null)
		{
			return;
		}
		float num = elapsed * ConditionLossPerSecond;
		if (num + 1f >= ownerItem.condition)
		{
			num = ownerItem.condition;
		}
		if (!(num > 1f) && !(num >= ownerItem.condition))
		{
			return;
		}
		ownerItem.condition -= num;
		if (ownerItem.condition <= 0f)
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if ((Object)(object)ownerPlayer != (Object)null)
			{
				ownerPlayer.ApplyWoundedStartTime();
			}
			SetLocked(flag: false);
			ownerItem.UseItem();
		}
	}

	public void RepairOnPush()
	{
		if (base.isServer)
		{
			GetOwnerItem()?.DoRepair(maxConditionRepairLossOnPush);
		}
	}

	public void InterruptUnlockMiniGame(bool wasPushedOrDamaged = false)
	{
		if (base.isServer && unlockStartTime > 0f && !wasPushedOrDamaged)
		{
			ModifyConditionForElapsedTime(Time.realtimeSinceStartup - unlockStartTime);
		}
		unlockStartTime = 0f;
		if (base.isServer)
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if (!((Object)(object)ownerPlayer == (Object)null))
			{
				ClientRPC(RpcTarget.Player("CL_CancelUnlockMiniGame", ownerPlayer), wasPushedOrDamaged ? 2f : 0f);
			}
		}
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	private void RPC_ReqStartUnlockMiniGame(RPCMessage rpc)
	{
		BasePlayer player = rpc.player;
		if (!((Object)(object)player == (Object)null))
		{
			SV_StartUnlockMiniGame(player);
		}
	}

	private void SV_StartUnlockMiniGame(BasePlayer player)
	{
		if (!player.IsDead() && !player.IsWounded())
		{
			StartUnlockMiniGame();
			ClientRPC(RpcTarget.Player("CL_StartUnlockMiniGame", player));
		}
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.FromOwner]
	private void RPC_ReqCancelUnlockMiniGame(RPCMessage rpc)
	{
		BasePlayer player = rpc.player;
		if (!((Object)(object)player == (Object)null))
		{
			SV_CancelUnlockMiniGame(player);
		}
	}

	private void SV_CancelUnlockMiniGame(BasePlayer player)
	{
		InterruptUnlockMiniGame();
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	private void RPC_ReqCompleteUnlockMiniGame(RPCMessage rpc)
	{
		BasePlayer player = rpc.player;
		if (!((Object)(object)player == (Object)null))
		{
			SV_ReqCompleteUnlockMiniGame(player);
		}
	}

	private void SV_ReqCompleteUnlockMiniGame(BasePlayer player)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		InterruptUnlockMiniGame();
		Effect.server.Run(escapeEffect.resourcePath, player, 0u, Vector3.zero, Vector3.zero);
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	private void RPC_ReqLock(RPCMessage rpc)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = rpc.player;
		if (!((Object)(object)player == (Object)null))
		{
			NetworkableId uid = rpc.read.EntityID();
			BasePlayer basePlayer = BaseNetworkable.serverEntities.Find(uid) as BasePlayer;
			if (!((Object)(object)basePlayer == (Object)null))
			{
				SV_HandcuffVictim(basePlayer, player);
			}
		}
	}

	private void SV_HandcuffVictim(BasePlayer victim, BasePlayer handcuffer)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)victim == (Object)null || (Object)(object)handcuffer == (Object)null || victim.IsRestrained || (!victim.CurrentGestureIsSurrendering && !victim.IsWounded()) || Vector3.Distance(((Component)victim).transform.position, ((Component)handcuffer).transform.position) > UseDistance)
		{
			return;
		}
		Item ownerItem = GetOwnerItem();
		if (ownerItem == null || Interface.CallHook("OnPlayerHandcuff", victim, handcuffer) != null)
		{
			return;
		}
		victim.SetPlayerFlag(BasePlayer.PlayerFlags.IsRestrained, b: true);
		victim.SendNetworkUpdateImmediate();
		ownerItem.SetFlag(Item.Flag.IsOn, b: true);
		bool flag = true;
		if (!ownerItem.MoveToContainer(victim.inventory.containerBelt))
		{
			Item slot = victim.inventory.containerBelt.GetSlot(0);
			if (slot != null)
			{
				if (!slot.MoveToContainer(victim.inventory.containerMain))
				{
					if (slot.contents != null)
					{
						slot.contents.SetLocked(isLocked: false, lockSubItems: true);
					}
					slot.DropAndTossUpwards(((Component)victim).transform.position);
				}
				if (!ownerItem.MoveToContainer(victim.inventory.containerBelt))
				{
					flag = false;
				}
			}
		}
		if (!flag)
		{
			ownerItem.SetFlag(Item.Flag.IsOn, b: false);
			victim.SetPlayerFlag(BasePlayer.PlayerFlags.IsRestrained, b: false);
		}
		ownerItem.MarkDirty();
		if (flag)
		{
			victim.Server_CancelGesture();
			if (victim.IsBot)
			{
				Inventory.EquipItemInSlot(victim, 0);
			}
			victim.ClientRPC(RpcTarget.Player("SetActiveBeltSlot", victim), ownerItem.position, ownerItem.uid);
			SetLocked(flag: true, victim, ownerItem);
			Effect.server.Run(lockEffect.resourcePath, victim, 0u, Vector3.zero, Vector3.zero);
			Interface.CallHook("OnPlayerHandcuffed", victim, handcuffer);
		}
	}

	public void UnlockAndReturnToPlayer(BasePlayer returnToPlayer)
	{
		SetLocked(flag: false);
		if (!((Object)(object)returnToPlayer == (Object)null))
		{
			Item ownerItem = GetOwnerItem();
			if (ownerItem != null)
			{
				returnToPlayer.GiveItem(ownerItem);
			}
		}
	}

	public override bool CanHit(HitTest info)
	{
		if (info.HitEntity is BasePlayer basePlayer)
		{
			if (!basePlayer.CurrentGestureIsSurrendering && !basePlayer.IsSleeping())
			{
				return basePlayer.IsWounded();
			}
			return true;
		}
		return false;
	}

	public override void DoAttackShared(HitInfo info)
	{
		if (!base.isServer)
		{
			return;
		}
		BasePlayer basePlayer = info.HitEntity as BasePlayer;
		if ((Object)(object)basePlayer != (Object)null)
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if ((Object)(object)ownerPlayer != (Object)null && (Object)(object)basePlayer != (Object)null)
			{
				SV_HandcuffVictim(basePlayer, ownerPlayer);
			}
		}
	}
}
