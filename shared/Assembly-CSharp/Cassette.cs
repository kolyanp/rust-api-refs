using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Cassette : BaseEntity, IUGCBrowserEntity, IServerFileReceiver
{
	public float MaxCassetteLength = 15f;

	[ReplicatedVar]
	public static float MaxCassetteFileSizeMB = 5f;

	public ulong CreatorSteamId;

	public PreloadedCassetteContent.PreloadType PreloadType;

	public PreloadedCassetteContent PreloadContent;

	public SoundDefinition InsertCassetteSfx;

	public int ViewmodelIndex;

	public Sprite HudSprite;

	public int MaximumVoicemailSlots = 1;

	public int preloadedAudioId;

	public ICassettePlayer currentCassettePlayer;

	public uint AudioId { get; private set; }

	public SoundDefinition PreloadedAudio => PreloadContent.GetSoundContent(preloadedAudioId, PreloadType);

	public override bool ShouldTransferAssociatedFiles => true;

	public uint[] GetContentCRCs
	{
		get
		{
			if (AudioId == 0)
			{
				return Array.Empty<uint>();
			}
			return new uint[1] { AudioId };
		}
	}

	public UGCType ContentType => UGCType.AudioOgg;

	public List<ulong> EditingHistory => new List<ulong> { CreatorSteamId };

	public BaseNetworkable UgcEntity => this;

	public string ContentString => string.Empty;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Cassette.OnRpcMessage"))
		{
			if (rpc == 4031457637u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_MakeNewFile"));
				}
				using (TimeWarning.New("Server_MakeNewFile"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4031457637u, "Server_MakeNewFile", this, player, 1uL))
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
							Server_MakeNewFile(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_MakeNewFile");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[ServerVar(Help = "(Generated) Clears the saved audio recording from all cassette items currently on the server; returns count of cassettes cleared")]
	public static void ClearCassettes(ConsoleSystem.Arg arg)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is Cassette cassette && cassette.ClearSavedAudio())
				{
					num++;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		arg.ReplyWith($"Deleted the contents of {num} cassettes");
	}

	[ServerVar(Help = "(Generated) Clears saved audio from all cassettes created by the given Steam64 ID; returns count of cassettes cleared")]
	public static void ClearCassettesByUser(ConsoleSystem.Arg arg)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		ulong uInt = arg.GetUInt64(0, 0uL);
		int num = 0;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is Cassette cassette && cassette.CreatorSteamId == uInt)
				{
					cassette.ClearSavedAudio();
					num++;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		arg.ReplyWith($"Deleted {num} cassettes recorded by {uInt}");
	}

	public override void Load(LoadInfo info)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.cassette == null)
		{
			return;
		}
		_ = AudioId;
		AudioId = info.msg.cassette.audioId;
		CreatorSteamId = info.msg.cassette.creatorSteamId;
		preloadedAudioId = info.msg.cassette.preloadAudioId;
		if (base.isServer && ((NetworkableId)(ref info.msg.cassette.holder)).IsValid)
		{
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(info.msg.cassette.holder);
			if ((Object)(object)baseNetworkable != (Object)null && baseNetworkable is ICassettePlayer cassettePlayer)
			{
				currentCassettePlayer = cassettePlayer;
			}
		}
	}

	public void AssignPreloadContent()
	{
		switch (PreloadType)
		{
		case PreloadedCassetteContent.PreloadType.Short:
			preloadedAudioId = Random.Range(0, PreloadContent.ShortTapeContent.Length);
			break;
		case PreloadedCassetteContent.PreloadType.Medium:
			preloadedAudioId = Random.Range(0, PreloadContent.MediumTapeContent.Length);
			break;
		case PreloadedCassetteContent.PreloadType.Long:
			preloadedAudioId = Random.Range(0, PreloadContent.LongTapeContent.Length);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.cassette = Pool.Get<Cassette>();
		info.msg.cassette.audioId = AudioId;
		info.msg.cassette.creatorSteamId = CreatorSteamId;
		info.msg.cassette.preloadAudioId = preloadedAudioId;
		if (!ObjectEx.IsUnityNull(currentCassettePlayer) && currentCassettePlayer.ToBaseEntity.IsValid())
		{
			info.msg.cassette.holder = currentCassettePlayer.ToBaseEntity.net.ID;
		}
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		currentCassettePlayer?.OnCassetteRemoved(this);
		currentCassettePlayer = null;
		if ((Object)(object)newParent != (Object)null && newParent is ICassettePlayer cassettePlayer)
		{
			Invoke(DelayedCassetteInserted, 0.1f);
			currentCassettePlayer = cassettePlayer;
		}
	}

	public void DelayedCassetteInserted()
	{
		if (currentCassettePlayer != null)
		{
			currentCassettePlayer.OnCassetteInserted(this);
		}
	}

	public void SetAudioId(uint id, ulong userId)
	{
		AudioId = id;
		CreatorSteamId = userId;
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	public void Server_MakeNewFile(RPCMessage msg)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)msg.player == (Object)null) && GetParentEntity() is RecorderTool recorderTool && !((Object)(object)recorderTool.GetOwnerPlayer() != (Object)(object)msg.player) && !((Object)(object)recorderTool.cachedCassette != (Object)(object)this))
		{
			byte[] data = msg.read.BytesWithSize();
			if (IsOggValid(data, this))
			{
				FileStorage.server.RemoveAllByEntity(net.ID);
				uint id = FileStorage.server.Store(data, FileStorage.Type.ogg, net.ID);
				SetAudioId(id, msg.player.userID);
			}
		}
	}

	public bool ClearSavedAudio()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		FileStorage.server.RemoveAllByEntity(net.ID);
		if (AudioId == 0)
		{
			return false;
		}
		AudioId = 0u;
		CreatorSteamId = 0uL;
		SendNetworkUpdate();
		return true;
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		ClearSavedAudio();
	}

	public void ClearContent()
	{
		AudioId = 0u;
		SendNetworkUpdate();
	}

	public static bool IsOggValid(byte[] data, Cassette c)
	{
		return IsOggValid(data, c.MaxCassetteLength);
	}

	public static bool IsOggValid(byte[] data, float maxLength)
	{
		if (data == null)
		{
			return false;
		}
		if (ByteToMegabyte(data.Length) >= MaxCassetteFileSizeMB)
		{
			Debug.Log((object)"Audio file is too large! Aborting");
			return false;
		}
		double oggLength = GetOggLength(data);
		if (oggLength > (double)(maxLength * 1.2f))
		{
			Debug.Log((object)$"Audio duration is longer than cassette limit! {oggLength} > {maxLength * 1.2f}");
			return false;
		}
		return true;
	}

	public static float ByteToMegabyte(int byteSize)
	{
		return (float)byteSize / 1024f / 1024f;
	}

	public static double GetOggLength(byte[] t)
	{
		int num = t.Length;
		long num2 = -1L;
		int num3 = -1;
		for (int num4 = num - 1 - 8 - 2 - 4; num4 >= 0; num4--)
		{
			if (t[num4] == 79 && t[num4 + 1] == 103 && t[num4 + 2] == 103 && t[num4 + 3] == 83)
			{
				num2 = BitConverter.ToInt64(new byte[8]
				{
					t[num4 + 6],
					t[num4 + 7],
					t[num4 + 8],
					t[num4 + 9],
					t[num4 + 10],
					t[num4 + 11],
					t[num4 + 12],
					t[num4 + 13]
				}, 0);
				break;
			}
		}
		for (int i = 0; i < num - 8 - 2 - 4; i++)
		{
			if (t[i] == 118 && t[i + 1] == 111 && t[i + 2] == 114 && t[i + 3] == 98 && t[i + 4] == 105 && t[i + 5] == 115)
			{
				num3 = BitConverter.ToInt32(new byte[4]
				{
					t[i + 11],
					t[i + 12],
					t[i + 13],
					t[i + 14]
				}, 0);
				break;
			}
		}
		if (RecorderTool.debugRecording)
		{
			Debug.Log((object)$"{num2} / {num3}");
		}
		return (double)num2 / (double)num3;
	}
}
