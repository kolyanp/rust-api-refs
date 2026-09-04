using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
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
						if (!RPC_Server.CallsPerSecond.Test(657465493u, "UpdatePoster", this, player, 5uL))
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

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void UpdatePoster(RPCMessage msg)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
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
			if (imageCrc != 0)
			{
				FileStorage.server.RemoveExact(imageCrc, FileType, net.ID, 0u);
			}
			imageCrc = 0u;
		}
		else
		{
			if (!ImageProcessing.IsValidJPG(array, 1024, 1024))
			{
				return;
			}
			if (imageCrc != 0)
			{
				FileStorage.server.RemoveExact(imageCrc, FileType, net.ID, 0u);
			}
			imageCrc = FileStorage.server.Store(array, FileType, net.ID);
		}
		SendNetworkUpdate();
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void ClearPlayer(RPCMessage msg)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)msg.player == (Object)null) && CanUpdateSign(msg.player))
		{
			playerId = 0uL;
			playerName = string.Empty;
			SetFlagLocal(Flags.Reserved1, b: false);
			if (imageCrc != 0)
			{
				FileStorage.server.RemoveExact(imageCrc, FileType, net.ID, 0u);
				imageCrc = 0u;
			}
			SendNetworkUpdate();
		}
	}

	public void SetTextureCRCs(uint[] crcs)
	{
		imageCrc = crcs[0];
		SendNetworkUpdate();
	}

	public void ClearContent()
	{
		imageCrc = 0u;
		SendNetworkUpdate();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.wantedPoster = Pool.Get<WantedPoster>();
		info.msg.wantedPoster.imageCrc = imageCrc;
		info.msg.wantedPoster.playerId = playerId;
		info.msg.wantedPoster.playerName = playerName;
	}

	internal override void DoServerDestroy()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		base.DoServerDestroy();
		if (net != null)
		{
			FileStorage.server.RemoveAllByEntity(net.ID);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.wantedPoster != null)
		{
			imageCrc = info.msg.wantedPoster.imageCrc;
			playerName = info.msg.wantedPoster.playerName;
			playerId = info.msg.wantedPoster.playerId;
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
			return (ulong)player.userID == base.OwnerID;
		}
		return true;
	}

	public uint[] GetTextureCRCs()
	{
		return new uint[1] { imageCrc };
	}
}
