using System;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseLock : BaseEntity
{
	[ItemSelector]
	public ItemDefinition itemType;

	public bool CanRemove = true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseLock.OnRpcMessage"))
		{
			if (rpc == 3572556655u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_TakeLock"));
				}
				using (TimeWarning.New("RPC_TakeLock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3572556655u, "RPC_TakeLock", this, player, 3f, checkParent: true))
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
							RPC_TakeLock(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_TakeLock");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool CanBeRedirectSwapped(BasePlayer player)
	{
		if (!GetPlayerLockPermission(player))
		{
			SprayCan.LastReskinError = SprayCan.NeedLockAccess;
			return false;
		}
		return base.CanBeRedirectSwapped(player);
	}

	public virtual bool GetPlayerLockPermission(BasePlayer player)
	{
		return OnTryToOpen(player);
	}

	public virtual bool OnTryToOpen(BasePlayer player)
	{
		return !IsLocked();
	}

	public virtual bool OnTryToClose(BasePlayer player)
	{
		return true;
	}

	public virtual bool HasLockPermission(BasePlayer player)
	{
		return true;
	}

	[RPC_Server.MaxDistance(3f, CheckParent = true)]
	[RPC_Server]
	public void RPC_TakeLock(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && CanRemove && Interface.CallHook("CanPickupLock", rpc.player, this) == null && !IsLocked())
		{
			Item item = ItemManager.Create(itemType, 1, skinID, isServerSide: true, 0uL);
			if (item != null)
			{
				rpc.player.GiveItem(item);
			}
			Facepunch.Rust.Analytics.Azure.OnEntityPickedUp(rpc.player, this);
			BaseEntity baseEntity = GetParentEntity();
			if ((Object)(object)baseEntity != (Object)null && (Object)(object)baseEntity.GetSlot(Slot.Lock) == (Object)(object)this)
			{
				baseEntity.SetSlot(Slot.Lock, null);
			}
			Kill();
		}
	}

	public override float AntiHackPadding()
	{
		return 2f;
	}
}
