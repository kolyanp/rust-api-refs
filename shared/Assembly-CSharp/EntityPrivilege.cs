using System;
using ConVar;
using Facepunch.Rust;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class EntityPrivilege : SimplePrivilege
{
	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("EntityPrivilege.OnRpcMessage"))
		{
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
							RPCMessage rpc2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							AddSelfAuthorize(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
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
							RPCMessage rpc3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ClearList(rpc3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
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
							RPCMessage rpc4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RemoveSelfAuthorize(rpc4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RemoveSelfAuthorize");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private bool CanAdministrate(BasePlayer player)
	{
		if (!((Object)(object)player == (Object)null))
		{
			player.CanInteract();
		}
		return false;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void AddSelfAuthorize(RPCMessage rpc)
	{
		if (CanAdministrate(rpc.player))
		{
			AddPlayer(rpc.player);
			SendNetworkUpdate();
		}
	}

	public void AddPlayer(BasePlayer player)
	{
		if (!AtMaxAuthCapacity())
		{
			authorizedPlayers.Add(player.userID);
			Analytics.Azure.OnEntityAuthChanged(this, player, authorizedPlayers, "added", player.userID);
			UpdateMaxAuthCapacity();
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void RemoveSelfAuthorize(RPCMessage rpc)
	{
		if (CanAdministrate(rpc.player))
		{
			authorizedPlayers.Remove(rpc.player.userID);
			Analytics.Azure.OnEntityAuthChanged(this, rpc.player, authorizedPlayers, "removed", rpc.player.userID);
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void ClearList(RPCMessage rpc)
	{
		if (CanAdministrate(rpc.player))
		{
			authorizedPlayers.Clear();
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}
}
