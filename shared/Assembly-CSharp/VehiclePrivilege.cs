using System;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class VehiclePrivilege : SimplePrivilege
{
	public GameObject assignDialog;

	public bool SupportFriendListAdd;

	public bool OnlyDriverCanModifyAuth = true;

	private BaseVehicle parentVehicle;

	public BaseVehicle ParentVehicle
	{
		get
		{
			if ((Object)(object)parentVehicle == (Object)null)
			{
				parentVehicle = FindParentVehicleRecursive(this, 5);
			}
			return parentVehicle;
		}
		set
		{
			parentVehicle = value;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("VehiclePrivilege.OnRpcMessage"))
		{
			if (rpc == 82205621 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - AddAuthorize"));
				}
				using (TimeWarning.New("AddAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(82205621u, "AddAuthorize", this, player, 3f))
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
							AddAuthorize(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in AddAuthorize");
					}
				}
				return true;
			}
			if (rpc == 1092560690 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - AddSelfAuthorize"));
				}
				using (TimeWarning.New("AddSelfAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1092560690u, "AddSelfAuthorize", this, player, 3f))
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
							AddSelfAuthorize(rpc3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in AddSelfAuthorize");
					}
				}
				return true;
			}
			if (rpc == 253307592 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ClearList"));
				}
				using (TimeWarning.New("ClearList"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(253307592u, "ClearList", this, player, 3f))
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
							ClearList(rpc4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in ClearList");
					}
				}
				return true;
			}
			if (rpc == 3617985969u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RemoveSelfAuthorize"));
				}
				using (TimeWarning.New("RemoveSelfAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3617985969u, "RemoveSelfAuthorize", this, player, 3f))
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
							RemoveSelfAuthorize(rpc5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RemoveSelfAuthorize");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private static BaseVehicle FindParentVehicleRecursive(BaseEntity startEnt, int maxDepth)
	{
		if ((Object)(object)startEnt == (Object)null)
		{
			return null;
		}
		BaseEntity baseEntity = startEnt;
		int num = 0;
		while ((Object)(object)baseEntity != (Object)null && num++ < maxDepth)
		{
			BaseEntity baseEntity2 = baseEntity.GetParentEntity();
			if ((Object)(object)baseEntity2 == (Object)null)
			{
				return null;
			}
			BaseVehicle baseVehicle = baseEntity2 as BaseVehicle;
			if ((Object)(object)baseVehicle != (Object)null)
			{
				return baseVehicle;
			}
			baseEntity = baseEntity2;
		}
		return null;
	}

	public bool IsDriver(BasePlayer player)
	{
		BaseEntity baseEntity = GetParentEntity();
		if ((Object)(object)baseEntity == (Object)null)
		{
			return false;
		}
		BaseVehicle baseVehicle = baseEntity as BaseVehicle;
		if ((Object)(object)baseVehicle == (Object)null)
		{
			return false;
		}
		return baseVehicle.IsDriver(player);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void AddSelfAuthorize(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && CanModifyAuth(rpc.player) && Interface.CallHook("OnCupboardAuthorize", this, rpc.player) == null)
		{
			AddPlayer(rpc.player);
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void AddAuthorize(RPCMessage rpc)
	{
		if (SupportFriendListAdd && rpc.player.CanInteract() && IsAuthed(rpc.player) && CanModifyAuth(rpc.player))
		{
			ulong targetPlayerId = rpc.read.UInt64();
			AddPlayer(rpc.player, targetPlayerId);
			SendNetworkUpdate();
		}
	}

	public void AddPlayer(BasePlayer granter, ulong targetPlayerId)
	{
		if (!AtMaxAuthCapacity())
		{
			authorizedPlayers.Add(targetPlayerId);
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, granter, authorizedPlayers, "added", targetPlayerId);
			UpdateMaxAuthCapacity();
			UpdatePrivilegeReceivers();
		}
	}

	public void AddPlayer(BasePlayer player)
	{
		if (!AtMaxAuthCapacity())
		{
			authorizedPlayers.Add(player.userID);
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, player, authorizedPlayers, "added", player.userID);
			UpdateMaxAuthCapacity();
			UpdatePrivilegeReceivers();
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RemoveSelfAuthorize(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && CanModifyAuth(rpc.player) && Interface.CallHook("OnCupboardDeauthorize", this, rpc.player) == null)
		{
			authorizedPlayers.Remove(rpc.player.userID);
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, rpc.player, authorizedPlayers, "removed", rpc.player.userID);
			UpdateMaxAuthCapacity();
			UpdatePrivilegeReceivers();
			SendNetworkUpdate();
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void ClearList(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && CanModifyAuth(rpc.player) && Interface.CallHook("OnCupboardClearList", this, rpc.player) == null)
		{
			authorizedPlayers.Clear();
			UpdateMaxAuthCapacity();
			UpdatePrivilegeReceivers();
			SendNetworkUpdate();
		}
	}

	private bool CanModifyAuth(BasePlayer player)
	{
		if (!OnlyDriverCanModifyAuth)
		{
			return true;
		}
		return IsDriver(player);
	}

	private void UpdatePrivilegeReceivers()
	{
		if ((Object)(object)ParentVehicle == (Object)null || !(ParentVehicle is PlayerBoat playerBoat))
		{
			return;
		}
		foreach (BaseEntity item in playerBoat.Deployables.Cached)
		{
			if (item is IPrivilegeUpdateReceiver privilegeUpdateReceiver)
			{
				privilegeUpdateReceiver.OnPrivilegeUpdated(this, authorizedPlayers);
			}
		}
	}
}
