using System;
using System.Collections.Generic;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class ImageStorageEntity : BaseEntity
{
	private struct ImageRequest
	{
		public IImageReceiver Receiver;

		public float Time;
	}

	private List<ImageRequest> _requests;

	protected virtual FileStorage.Type StorageType => FileStorage.Type.jpg;

	protected virtual uint CrcToLoad => 0u;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ImageStorageEntity.OnRpcMessage"))
		{
			if (rpc == 652912521 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ImageRequested"));
				}
				using (TimeWarning.New("ImageRequested"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(652912521u, "ImageRequested", this, player, 3uL))
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
							ImageRequested(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ImageRequested");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	private void ImageRequested(RPCMessage msg)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)msg.player == (Object)null))
		{
			byte[] array = FileStorage.server.Get(CrcToLoad, StorageType, net.ID);
			if (array == null)
			{
				Debug.LogWarning((object)"Image entity has no image!");
				return;
			}
			SendInfo sendInfo = new SendInfo(msg.connection);
			sendInfo.method = SendMethod.Reliable;
			sendInfo.channel = 2;
			SendInfo sendInfo2 = sendInfo;
			ClientRPC(RpcTarget.SendInfo("ReceiveImage", sendInfo2), (uint)array.Length, array);
		}
	}
}
