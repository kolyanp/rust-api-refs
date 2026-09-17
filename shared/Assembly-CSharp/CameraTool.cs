using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class CameraTool : HeldEntity
{
	public GameObjectRef screenshotEffect;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CameraTool.OnRpcMessage"))
		{
			if (rpc == 3167878597u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SVNoteScreenshot"));
				}
				using (TimeWarning.New("SVNoteScreenshot"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(3167878597u, "SVNoteScreenshot", this, player))
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
							SVNoteScreenshot(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SVNoteScreenshot");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void SVNoteScreenshot(RPCMessage msg)
	{
	}
}
