using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class MapEntity : HeldEntity
{
	[NonSerialized]
	public uint[] fogImages = new uint[1];

	[NonSerialized]
	public uint[] paintImages = new uint[144];

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MapEntity.OnRpcMessage"))
		{
			if (rpc == 1443560440 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ImageUpdate"));
				}
				using (TimeWarning.New("ImageUpdate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1443560440u, "ImageUpdate", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1443560440u, "ImageUpdate", this, player))
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
							ImageUpdate(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ImageUpdate");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.mapEntity != null)
		{
			if (info.msg.mapEntity.fogImages.Count == fogImages.Length)
			{
				fogImages = info.msg.mapEntity.fogImages.ToArray();
			}
			if (info.msg.mapEntity.paintImages.Count == paintImages.Length)
			{
				paintImages = info.msg.mapEntity.paintImages.ToArray();
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.mapEntity = Pool.Get<MapEntity>();
		info.msg.mapEntity.fogImages = Pool.Get<List<uint>>();
		info.msg.mapEntity.fogImages.AddRange(fogImages);
		info.msg.mapEntity.paintImages = Pool.Get<List<uint>>();
		info.msg.mapEntity.paintImages.AddRange(paintImages);
	}

	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	public void ImageUpdate(RPCMessage msg)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)msg.player == (Object)null)
		{
			return;
		}
		byte b = msg.read.UInt8();
		byte b2 = msg.read.UInt8();
		uint num = msg.read.UInt32();
		if ((b == 0 && fogImages[b2] == num) || (b == 1 && paintImages[b2] == num))
		{
			return;
		}
		uint num2 = (uint)(b * 1000 + b2);
		byte[] array = msg.read.BytesWithSize();
		if (array != null)
		{
			FileStorage.server.RemoveEntityNum(net.ID, num2);
			uint num3 = FileStorage.server.Store(array, FileStorage.Type.png, net.ID, num2);
			if (b == 0)
			{
				fogImages[b2] = num3;
			}
			if (b == 1)
			{
				paintImages[b2] = num3;
			}
			InvalidateNetworkCache();
			Interface.CallHook("OnMapImageUpdated");
		}
	}
}
