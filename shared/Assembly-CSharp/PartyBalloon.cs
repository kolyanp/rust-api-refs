using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using Rust.UI;
using UnityEngine;
using UnityEngine.Assertions;
using ntw.CurvedTextMeshPro;

public class PartyBalloon : BaseCombatEntity
{
	[Header("Party Balloon Dependencies")]
	public RustText balloonTextComponent;

	public RustText backsideBalloonTextComponent;

	public TextProOnACircle textProOnACircleComponent;

	public TextProOnACircle backsideTextProOnACircleComponent;

	public GameObjectRef configureBalloonDialog;

	public List<Renderer> balloonRenderers;

	[Header("Party Balloon Text Settings")]
	public int arcRadCharactersLimit = 20;

	public int minArcDegrees = 12;

	public int maxArcDegrees = 37;

	public int textLinesLimit = 4;

	public float minLineHeight = 10f;

	public float maxLineHeight = 30f;

	public float fontSizeDivider = 1f;

	[Header("Party Balloon FX")]
	public GameObjectRef partyBalloonPopFX;

	private static readonly int COLOR = Shader.PropertyToID("_Color");

	private string __sync_BalloonText;

	private Color __sync_BalloonColour;

	private Color __sync_TextColour;

	[Sync(Autosave = true)]
	public string BalloonText
	{
		[CompilerGenerated]
		get
		{
			return __sync_BalloonText;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_BalloonText, value))
			{
				__sync_BalloonText = value;
				byte nameID = __GetWeaverID("BalloonText");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public Color BalloonColour
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return __sync_BalloonColour;
		}
		[CompilerGenerated]
		set
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (!IsSyncVarEqual<Color>(__sync_BalloonColour, value))
			{
				__sync_BalloonColour = value;
				byte nameID = __GetWeaverID("BalloonColour");
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
		using (TimeWarning.New("PartyBalloon.OnRpcMessage"))
		{
			if (rpc == 1887711985 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - LockBalloon"));
				}
				using (TimeWarning.New("LockBalloon"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1887711985u, "LockBalloon", this, player, 3f))
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
							LockBalloon(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in LockBalloon");
					}
				}
				return true;
			}
			if (rpc == 473707823 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ConfigureBalloon"));
				}
				using (TimeWarning.New("RPC_ConfigureBalloon"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(473707823u, "RPC_ConfigureBalloon", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(473707823u, "RPC_ConfigureBalloon", this, player, 3f))
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
							RPC_ConfigureBalloon(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_ConfigureBalloon");
					}
				}
				return true;
			}
			if (rpc == 2622659557u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - UnLockBalloon"));
				}
				using (TimeWarning.New("UnLockBalloon"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2622659557u, "UnLockBalloon", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							UnLockBalloon(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in UnLockBalloon");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public virtual bool CanUpdateBalloon(BasePlayer player)
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

	public bool CanUnlockBalloon(BasePlayer player)
	{
		if (!IsLocked())
		{
			return false;
		}
		return CanUpdateBalloon(player);
	}

	public bool CanLockBalloon(BasePlayer player)
	{
		if (IsLocked())
		{
			return false;
		}
		return CanUpdateBalloon(player);
	}

	public override void ServerInit()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		BalloonColour = Color.white;
		TextColour = Color.white;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server.IsVisible(3f)]
	public void RPC_ConfigureBalloon(RPCMessage msg)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		string balloonText = msg.read.String();
		Color balloonColour = msg.read.Color();
		Color textColour = msg.read.Color();
		SetBalloonText(balloonText);
		SetBalloonColour(balloonColour);
		SetTextColour(textColour);
	}

	public void SetBalloonText(string text)
	{
		BalloonText = text;
	}

	public void SetBalloonColour(Color colour)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		BalloonColour = colour;
	}

	public void SetTextColour(Color colour)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		TextColour = colour;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void LockBalloon(RPCMessage msg)
	{
		if (msg.player.CanInteract() && CanUpdateBalloon(msg.player))
		{
			SetFlagLocal(Flags.Locked, b: true);
			SendNetworkUpdate();
			base.OwnerID = msg.player.userID;
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void UnLockBalloon(RPCMessage msg)
	{
		if (msg.player.CanInteract() && CanUnlockBalloon(msg.player))
		{
			SetFlagLocal(Flags.Locked, b: false);
			SendNetworkUpdate();
		}
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: BalloonText for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_BalloonText);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: BalloonColour for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite<Color>(writer, __sync_BalloonColour);
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
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			try
			{
				_ = __sync_BalloonText;
				string _sync_BalloonText = reader.String();
				__sync_BalloonText = _sync_BalloonText;
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_BalloonColour;
				Color _sync_BalloonColour = reader.Color();
				__sync_BalloonColour = _sync_BalloonColour;
			}
			catch (Exception ex3)
			{
				Debug.LogException(ex3);
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
		return propertyName switch
		{
			"BalloonText" => 0, 
			"BalloonColour" => 1, 
			"TextColour" => 2, 
			_ => byte.MaxValue, 
		};
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
		WriteSyncVar(1, writer);
		WriteSyncVar(2, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
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
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		base.ResetSyncVars();
		__sync_BalloonText = null;
		__sync_BalloonColour = default(Color);
		__sync_TextColour = default(Color);
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			2 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}
}
