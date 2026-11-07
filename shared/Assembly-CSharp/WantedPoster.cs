using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using ProtoBuf;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class WantedPoster : DecayEntity, ISignage, IUGCBrowserEntity, ILOD, IServerFileReceiver
{
	public uint imageCrc;

	public ulong playerId;

	public string playerName;

	public MeshRenderer PhotoImage;

	public TextMeshPro WantedNameText;

	public GameObjectRef AssignDialog;

	public const Flags HasTarget = Flags.Reserved1;

	public uiPlayerPreview.EffectMode EffectMode = uiPlayerPreview.EffectMode.Polaroid;

	public uint[] GetContentCRCs
	{
		get
		{
			if (imageCrc == 0)
			{
				return null;
			}
			return new uint[1] { imageCrc };
		}
	}

	public UGCType ContentType => UGCType.ImageJpg;

	public List<ulong> EditingHistory { get; } = new List<ulong>();

	public BaseNetworkable UgcEntity => this;

	public string ContentString => string.Empty;

	public override bool ShouldTransferAssociatedFiles => true;

	public Vector2i TextureSize
	{
		get
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			return new Vector2i(1024, 1024);
		}
	}

	public int TextureCount => 1;

	public NetworkableId NetworkID
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return net.ID;
		}
	}

	public FileStorage.Type FileType => FileStorage.Type.jpg;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("WantedPoster.OnRpcMessage"))
		{
			if (rpc == 2419123501u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ClearPlayer"));
				}
				using (TimeWarning.New("ClearPlayer"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2419123501u, "ClearPlayer", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2419123501u, "ClearPlayer", this, player, 3f))
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
							ClearPlayer(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ClearPlayer");
					}
				}
				return true;
			}
			if (rpc == 657465493 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - UpdatePoster"));
				}
				using (TimeWarning.New("UpdatePoster"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(657465493u, "UpdatePoster", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(657465493u, "UpdatePoster", this, player, 3f))
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
							UpdatePoster(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in UpdatePoster");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(2uL)]
	private void UpdatePoster(RPCMessage msg)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)msg.player == (Object)null || !CanUpdateSign(msg.player))
		{
			return;
		}
		ulong num = msg.read.UInt64();
		string text = msg.read.String();
		byte[] array = msg.read.BytesWithSize();
		playerId = num;
		playerName = text;
		SetFlagLocal(Flags.Reserved1, b: true);
		if (array == null)
		{
			FileStorage.server.Remove(net.ID, 0uL);
			imageCrc = 0u;
		}
		else
		{
			if (!ImageProcessing.IsValidJPG(array, TextureSize.x, TextureSize.y))
			{
				return;
			}
			imageCrc = FileStorage.server.Store(net.ID, 0uL, FileType, array);
			Analytics.Azure.OnUGCCreated(msg.player, this, "wanted_poster", array.Length);
			UGCModeration.Send(msg.player, this, "wanted_poster", imageCrc, FileType);
		}
		SendNetworkUpdate();
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void ClearPlayer(RPCMessage msg)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)msg.player == (Object)null) && CanUpdateSign(msg.player))
		{
			playerId = 0uL;
			playerName = string.Empty;
			SetFlagLocal(Flags.Reserved1, b: false);
			FileStorage.server.Remove(net.ID, 0uL);
			imageCrc = 0u;
			SendNetworkUpdate();
		}
	}

	public void RefreshFileCrcs()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		imageCrc = FileStorage.server.GetCrc(net.ID, 0uL);
		SendNetworkUpdate();
	}

	public void ClearContent()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		FileStorage.server.RemoveAll(net.ID);
		imageCrc = 0u;
		SendNetworkUpdate();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.wantedPoster = Pool.Get<WantedPoster>();
		if (!info.forDisk)
		{
			info.msg.wantedPoster.imageCrc = imageCrc;
		}
		info.msg.wantedPoster.playerId = playerId;
		info.msg.wantedPoster.playerName = playerName;
	}

	internal override void DoServerDestroy()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		base.DoServerDestroy();
		if (net != null)
		{
			FileStorage.server.RemoveAll(net.ID);
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.wantedPoster != null)
		{
			imageCrc = info.msg.wantedPoster.imageCrc;
			playerName = info.msg.wantedPoster.playerName;
			playerId = info.msg.wantedPoster.playerId;
		}
		if (isServer)
		{
			imageCrc = FileStorage.server.GetCrc(net.ID, 0uL);
		}
	}

	public bool CanUpdateSign(BasePlayer player)
	{
		if (player.IsAdmin || player.IsDeveloper)
		{
			return true;
		}
		if (!player.CanBuild())
		{
			return false;
		}
		if (IsLocked())
		{
			return (ulong)player.userID == OwnerID;
		}
		return true;
	}

	public uint[] GetTextureCRCs()
	{
		return new uint[1] { imageCrc };
	}
}
