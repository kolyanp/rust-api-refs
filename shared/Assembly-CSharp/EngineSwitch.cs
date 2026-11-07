using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class EngineSwitch : BaseEntity
{
	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("EngineSwitch.OnRpcMessage"))
		{
			if (rpc == 1249530220 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - StartEngine"));
				}
				using (TimeWarning.New("StartEngine"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1249530220u, "StartEngine", this, player, 3f))
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
							StartEngine(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in StartEngine");
					}
				}
				return true;
			}
			if (rpc == 1739656243 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - StopEngine"));
				}
				using (TimeWarning.New("StopEngine"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1739656243u, "StopEngine", this, player, 3f))
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
							StopEngine(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in StopEngine");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void StopEngine(RPCMessage msg)
	{
		MiningQuarry miningQuarry = GetParentEntity() as MiningQuarry;
		if (Object.op_Implicit((Object)(object)miningQuarry) && Interface.CallHook("OnQuarryToggle", miningQuarry, msg.player) == null)
		{
			miningQuarry.EngineSwitch(isOn: false);
			Interface.CallHook("OnQuarryToggled", miningQuarry, msg.player);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void StartEngine(RPCMessage msg)
	{
		MiningQuarry miningQuarry = GetParentEntity() as MiningQuarry;
		if (Object.op_Implicit((Object)(object)miningQuarry) && Interface.CallHook("OnQuarryToggle", miningQuarry, msg.player) == null)
		{
			miningQuarry.EngineSwitch(isOn: true);
			Interface.CallHook("OnQuarryToggled", miningQuarry, msg.player);
		}
	}
}
