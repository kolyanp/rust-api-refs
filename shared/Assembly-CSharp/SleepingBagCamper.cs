using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class SleepingBagCamper : SleepingBag
{
	public EntityRef<BaseVehicleSeat> AssociatedSeat;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SleepingBagCamper.OnRpcMessage"))
		{
			if (rpc == 2177887503u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerClearBed"));
				}
				using (TimeWarning.New("ServerClearBed"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2177887503u, "ServerClearBed", this, player, 3f))
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
							ServerClearBed(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ServerClearBed");
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
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, b: true);
	}

	protected override void PostPlayerSpawn(BasePlayer p)
	{
		base.PostPlayerSpawn(p);
		BaseVehicleSeat baseVehicleSeat = AssociatedSeat.Get(base.isServer);
		if ((Object)(object)baseVehicleSeat != (Object)null)
		{
			if (p.IsConnected)
			{
				p.EndSleeping();
			}
			baseVehicleSeat.MountPlayer(p);
			p.PauseTickDistanceDetection(2f);
			p.ApplyStallProtection(2f);
		}
	}

	public void SetSeat(BaseVehicleSeat seat, bool sendNetworkUpdate = false)
	{
		AssociatedSeat.Set(seat);
		if (sendNetworkUpdate)
		{
			SendNetworkUpdate();
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.sleepingBagCamper = Pool.Get<SleepingBagCamper>();
			info.msg.sleepingBagCamper.seatID = AssociatedSeat.uid;
		}
	}

	public override RespawnState GetRespawnState(ulong userID)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		RespawnState respawnState = base.GetRespawnState(userID);
		if ((int)respawnState != 1)
		{
			return respawnState;
		}
		if (AssociatedSeat.IsValid(base.isServer))
		{
			BasePlayer mounted = AssociatedSeat.Get(base.isServer).GetMounted();
			if ((Object)(object)mounted != (Object)null && (ulong)mounted.userID != userID)
			{
				return (RespawnState)2;
			}
		}
		return (RespawnState)1;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void ServerClearBed(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && AssociatedSeat.IsValid(base.isServer) && !((Object)(object)AssociatedSeat.Get(base.isServer).GetMounted() != (Object)(object)player))
		{
			AssignToUser(0uL);
		}
	}
}
