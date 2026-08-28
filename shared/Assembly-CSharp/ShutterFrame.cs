using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class ShutterFrame : PhotoFrame, IFlagNotify
{
	[Tooltip("Tiling for the shutter material UVs, needs to be set because it will be overwritten during animation if not")]
	[Header("Shutter Frame")]
	public Vector2 shutterDefaultTiling;

	[Tooltip("Offsets for the shutter material UVs, needs to be set because it will be overwritten during animation if not")]
	public Vector2 shutterDefaultOffset;

	[Tooltip("UV -> V Offsets for the shutter when open and closed respectively")]
	public Vector2 shutterUVOffsets;

	public List<Renderer> shutterRenderers;

	public float shutterMoveSpeed;

	public AnimationCurve shutterMovementCurve;

	public GameObjectRef IoEntity;

	public Transform IoEntityAnchor;

	[Header("Sound")]
	public SoundDefinition moveStartSoundDef;

	public SoundDefinition moveStopSoundDef;

	public SoundDefinition moveLoopSoundDef;

	private EntityRef<IOEntity> spawnedIo;

	private bool __sync_IsShutterOpen;

	[Sync(Autosave = true)]
	public bool IsShutterOpen
	{
		[CompilerGenerated]
		get
		{
			return __sync_IsShutterOpen;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_IsShutterOpen, value))
			{
				__sync_IsShutterOpen = value;
				byte nameID = __GetWeaverID("IsShutterOpen");
				QueueSyncVar(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ShutterFrame.OnRpcMessage"))
		{
			if (rpc == 3472018092u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ToggleShutter"));
				}
				using (TimeWarning.New("RPC_ToggleShutter"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3472018092u, "RPC_ToggleShutter", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3472018092u, "RPC_ToggleShutter", this, player, 6f))
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
							RPC_ToggleShutter(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_ToggleShutter");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Load(LoadInfo info)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.simpleUID != null)
		{
			spawnedIo.uid = info.msg.simpleUID.uid;
		}
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
		base.ServerInit();
		if (!Application.isLoadingSave)
		{
			SpawnIOEnt();
		}
	}

	private void SpawnIOEnt()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (IoEntity.isValid && (Object)(object)IoEntityAnchor != (Object)null)
		{
			IOEntity iOEntity = GameManager.server.CreateEntity(IoEntity.resourcePath, IoEntityAnchor.position, IoEntityAnchor.rotation) as IOEntity;
			iOEntity.SetParent(this, worldPositionStays: true);
			spawnedIo.Set(iOEntity);
			iOEntity.Spawn();
		}
	}

	public void OnFlagToggled(bool state)
	{
		if (base.isServer && IsShutterOpen != !state)
		{
			IsShutterOpen = !state;
		}
	}

	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(6f)]
	public void RPC_ToggleShutter(RPCMessage msg)
	{
		IsShutterOpen = !IsShutterOpen;
	}

	public override void Save(SaveInfo info)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (info.msg.simpleUID == null)
		{
			info.msg.simpleUID = Pool.Get<SimpleUID>();
		}
		info.msg.simpleUID.uid = spawnedIo.uid;
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 1)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: IsShutterOpen for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_IsShutterOpen);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 1)
		{
			try
			{
				_ = __sync_IsShutterOpen;
				bool _sync_IsShutterOpen = reader.Bool();
				__sync_IsShutterOpen = _sync_IsShutterOpen;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "IsShutterOpen")
		{
			return 1;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(1, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(1, reader, fromAutoSave: true);
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
		base.ResetSyncVars();
		__sync_IsShutterOpen = false;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 1)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}

	public ShutterFrame()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		shutterDefaultTiling = new Vector2(1.15f, 1f);
		shutterDefaultOffset = new Vector2(-0.07f, -0.2f);
		shutterUVOffsets = new Vector2(0f, -0.48f);
		shutterMoveSpeed = 1f;
		shutterMovementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		base._002Ector();
	}
}
