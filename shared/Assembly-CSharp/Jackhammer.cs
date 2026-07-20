using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class Jackhammer : BaseMelee
{
	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Jackhammer.OnRpcMessage"))
		{
			if (rpc == 1699910227 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_SetEngineStatus"));
				}
				using (TimeWarning.New("Server_SetEngineStatus"))
				{
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
							Server_SetEngineStatus(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_SetEngineStatus");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool HasAmmo()
	{
		return true;
	}

	[RPC_Server]
	public void Server_SetEngineStatus(RPCMessage msg)
	{
		SetEngineStatus(msg.read.Bit());
	}

	public void SetEngineStatus(bool on)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved8, on);
	}

	public override void SetHeld(bool bHeld)
	{
		if (!bHeld)
		{
			SetEngineStatus(on: false);
		}
		base.SetHeld(bHeld);
	}
}
