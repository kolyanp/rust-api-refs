using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class LadderHatch : Door
{
	public Collider ladderVolume;

	private const float MaxLadderDistance = 1.5f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("LadderHatch.OnRpcMessage"))
		{
			if (rpc == 2513908834u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_CloseLadderHatch"));
				}
				using (TimeWarning.New("RPC_CloseLadderHatch"))
				{
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
							RPC_CloseLadderHatch(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_CloseLadderHatch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	private void RPC_CloseLadderHatch(RPCMessage rpc)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = rpc.player.eyes.position;
		Bounds val = ladderVolume.bounds;
		if (val.SqrDistance(position) <= 2.25f || !(Distance(position) > 3f))
		{
			RPC_CloseDoor(rpc);
		}
	}
}
