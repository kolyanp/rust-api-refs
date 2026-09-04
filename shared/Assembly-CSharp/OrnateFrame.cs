using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class OrnateFrame : PhotoFrame
{
	[Header("Ornate Frame Dependencies")]
	public TextMeshPro frameTextComponent;

	public GameObjectRef configureFrameDialog;

	private string __sync_FrameText;

	private Color __sync_TextColour;

	[Sync(Autosave = true)]
	public string FrameText
	{
		[CompilerGenerated]
		get
		{
			return __sync_FrameText;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_FrameText, value))
			{
				__sync_FrameText = value;
				byte nameID = __GetWeaverID("FrameText");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public Color TextColour
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return __sync_TextColour;
		}
		[CompilerGenerated]
		set
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (!IsSyncVarEqual<Color>(__sync_TextColour, value))
			{
				__sync_TextColour = value;
				byte nameID = __GetWeaverID("TextColour");
				QueueSyncVar(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("OrnateFrame.OnRpcMessage"))
		{
			if (rpc == 3398916869u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ConfigureFrame"));
				}
				using (TimeWarning.New("RPC_ConfigureFrame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3398916869u, "RPC_ConfigureFrame", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3398916869u, "RPC_ConfigureFrame", this, player, 3f))
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
							RPC_ConfigureFrame(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_ConfigureFrame");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public virtual bool CanUpdateFrame(BasePlayer player)
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

	public override void ServerInit()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		TextColour = Color.black;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server]
	public void RPC_ConfigureFrame(RPCMessage msg)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		string frameText = msg.read.String();
		Color textColour = msg.read.Color();
		SetFrameText(frameText);
		SetTextColour(textColour);
	}

	public void SetFrameText(string text)
	{
		FrameText = text;
	}

	public void SetTextColour(Color colour)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		TextColour = colour;
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: FrameText for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_FrameText);
			return true;
		case 2:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: TextColour for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite<Color>(writer, __sync_TextColour);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 1:
			try
			{
				_ = __sync_FrameText;
				string _sync_FrameText = reader.String();
				__sync_FrameText = _sync_FrameText;
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 2:
			try
			{
				_ = __sync_TextColour;
				Color _sync_TextColour = reader.Color();
				__sync_TextColour = _sync_TextColour;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (!(propertyName == "FrameText"))
		{
			if (propertyName == "TextColour")
			{
				return 2;
			}
			return byte.MaxValue;
		}
		return 1;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(1, writer);
		WriteSyncVar(2, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(1, reader, fromAutoSave: true);
		OnSyncVar(2, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		var (src, num) = netWrite.GetBuffer();
		if (_autosaveBuffer == null)
		{
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		if (_autosaveBuffer.Length < num)
		{
			BaseEntity._autosaveBufferPool.Return(_autosaveBuffer);
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		Buffer.BlockCopy(src, 0, _autosaveBuffer, 0, num);
		save.msg.baseEntity.syncVars = _autosaveBuffer;
		Pool.Free<NetWrite>(ref netWrite);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(netRead);
			Pool.Free<NetRead>(ref netRead);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		base.ResetSyncVars();
		__sync_FrameText = null;
		__sync_TextColour = default(Color);
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			1 => true, 
			2 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}
}
