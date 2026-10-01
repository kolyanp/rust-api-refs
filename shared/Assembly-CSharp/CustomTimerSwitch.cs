using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class CustomTimerSwitch : TimerSwitch
{
	public GameObjectRef timerPanelPrefab;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CustomTimerSwitch.OnRpcMessage"))
		{
			if (rpc == 1019813162 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_SetTime"));
				}
				using (TimeWarning.New("SERVER_SetTime"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1019813162u, "SERVER_SetTime", this, player, 3f))
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
							SERVER_SetTime(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_SetTime");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void SERVER_SetTime(RPCMessage msg)
	{
		if (CanPlayerAdmin(msg.player))
		{
			float num = msg.read.Float();
			if (!FloatEx.IsNaNOrInfinity(num))
			{
				timerLength = num;
				SendNetworkUpdate();
			}
		}
	}

	public bool CanPlayerAdmin(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null && player.CanBuild())
		{
			return !IsOn();
		}
		return false;
	}
}
