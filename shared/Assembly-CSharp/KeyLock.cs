using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class KeyLock : BaseLock
{
	[ItemSelector]
	public ItemDefinition keyItemType;

	public int keyCode;

	public bool firstKeyCreated;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("KeyLock.OnRpcMessage"))
		{
			if (rpc == 4135414453u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_CreateKey"));
				}
				using (TimeWarning.New("RPC_CreateKey"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4135414453u, "RPC_CreateKey", this, player, 3f, checkParent: true))
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
							RPC_CreateKey(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_CreateKey");
					}
				}
				return true;
			}
			if (rpc == 954115386 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Lock"));
				}
				using (TimeWarning.New("RPC_Lock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(954115386u, "RPC_Lock", this, player, 3f, checkParent: true))
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
							RPC_Lock(rpc3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_Lock");
					}
				}
				return true;
			}
			if (rpc == 1663222372 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Unlock"));
				}
				using (TimeWarning.New("RPC_Unlock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1663222372u, "RPC_Unlock", this, player, 3f, checkParent: true))
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
							RPC_Unlock(rpc4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_Unlock");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool HasLockPermission(BasePlayer player)
	{
		if (player.IsDead())
		{
			return false;
		}
		if ((ulong)player.userID == base.OwnerID)
		{
			return true;
		}
		PooledList<Item> val = Pool.Get<PooledList<Item>>();
		try
		{
			player.inventory.FindItemsByItemID((List<Item>)(object)val, keyItemType.itemid);
			for (int i = 0; i < ((List<Item>)(object)val).Count; i++)
			{
				if (CanKeyUnlockUs(((List<Item>)(object)val)[i]))
				{
					return true;
				}
			}
			return false;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool CanKeyUnlockUs(Item key)
	{
		if (key.instanceData == null)
		{
			return false;
		}
		if (key.instanceData.dataInt != keyCode)
		{
			return false;
		}
		return true;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.keyLock != null)
		{
			LoadKeylockData(info.msg.keyLock);
		}
	}

	public void LoadKeylockData(KeyLock keyLock)
	{
		if (keyLock != null)
		{
			keyCode = keyLock.code;
		}
	}

	public override bool ShouldNetworkOwnerInfo()
	{
		return true;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (base.OwnerID == 0L && Object.op_Implicit((Object)(object)GetParentEntity()))
		{
			base.OwnerID = GetParentEntity().OwnerID;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.keyLock = Pool.Get<KeyLock>();
			info.msg.keyLock.code = keyCode;
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		keyCode = Random.Range(1, 100000);
		Lock(deployedBy);
	}

	public override bool OnTryToOpen(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUseLockedEntity", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (HasLockPermission(player))
		{
			return true;
		}
		return !IsLocked();
	}

	public override bool OnTryToClose(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUseLockedEntity", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (HasLockPermission(player))
		{
			return true;
		}
		return !IsLocked();
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f, CheckParent = true)]
	private void RPC_Unlock(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && IsLocked() && Interface.CallHook("CanUnlock", rpc.player, this) == null && HasLockPermission(rpc.player))
		{
			SetFlagLocal(Flags.Locked, b: false);
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f, CheckParent = true)]
	private void RPC_Lock(RPCMessage rpc)
	{
		Lock(rpc.player);
	}

	private void Lock(BasePlayer player)
	{
		if (!((Object)(object)player == (Object)null) && player.CanInteract() && !IsLocked() && Interface.CallHook("CanLock", player, this) == null && HasLockPermission(player))
		{
			LockLock(player);
			SendNetworkUpdate();
		}
	}

	[RPC_Server.MaxDistance(3f, CheckParent = true)]
	[RPC_Server]
	private void RPC_CreateKey(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract() || (IsLocked() && !HasLockPermission(rpc.player)))
		{
			return;
		}
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(keyItemType.itemid);
		if ((Object)(object)itemDefinition == (Object)null)
		{
			Debug.LogWarning((object)("RPC_CreateKey: Itemdef is missing! " + (object)keyItemType));
			return;
		}
		ItemBlueprint bp = ItemManager.FindBlueprint(itemDefinition);
		if (rpc.player.inventory.crafting.CanCraft(bp))
		{
			InstanceData val = Pool.Get<InstanceData>();
			val.dataInt = keyCode;
			rpc.player.inventory.crafting.CraftItem(bp, rpc.player, val);
			if (!firstKeyCreated)
			{
				LockLock(rpc.player);
				SendNetworkUpdate();
				firstKeyCreated = true;
			}
		}
	}

	public void LockLock(BasePlayer player)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Locked, b: true);
		}
		if (player.IsValid())
		{
			player.GiveAchievement("LOCK_LOCK");
		}
	}
}
