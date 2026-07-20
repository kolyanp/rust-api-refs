using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class SingleUseMissionStorageContainer : LootContainer
{
	[Serializable]
	public class MissionEntry
	{
		public BaseMission mission;

		public MissionObjective[] objectives = Array.Empty<MissionObjective>();
	}

	[Header("Mission settings")]
	public MissionEntry[] validMissions = Array.Empty<MissionEntry>();

	[Header("Physics")]
	public Collider mainCollider;

	public TriggerPlayerForce playerForcer;

	private const float TIMEOUT_DURATION = 60f;

	private Action _action_DestroyNoLoot;

	private Action _action_CheckShouldDisablePlayerRepulse;

	private int noOfTicks;

	private const int ticksUntilDisablePlayerRepulse = 10;

	private ulong __sync_permittedUserId;

	[Sync(Autosave = true, Pack = false)]
	private ulong permittedUserId
	{
		[CompilerGenerated]
		get
		{
			return __sync_permittedUserId;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_permittedUserId, value))
			{
				__sync_permittedUserId = value;
				byte nameID = __GetWeaverID("permittedUserId");
				SV_SyncVarSend(nameID);
			}
		}
	}

	public ulong PermittedUserId => permittedUserId;

	private Action action_DestroyNoLoot
	{
		get
		{
			if (_action_DestroyNoLoot == null)
			{
				_action_DestroyNoLoot = Cleanup_NoLoot;
			}
			return _action_DestroyNoLoot;
		}
	}

	private Action action_CheckShouldDisablePlayerRepulse
	{
		get
		{
			_action_CheckShouldDisablePlayerRepulse = CheckShouldDisablePlayerRepulse;
			return _action_CheckShouldDisablePlayerRepulse;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SingleUseMissionStorageContainer.OnRpcMessage"))
		{
			if (rpc == 2761122970u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_UpdateNoteText"));
				}
				using (TimeWarning.New("Server_UpdateNoteText"))
				{
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
							Server_UpdateNoteText(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_UpdateNoteText");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ResetCleanupTimer();
		StartPlayerRepulseCountdown();
	}

	private void ResetCleanupTimer()
	{
		CancelInvoke(action_DestroyNoLoot);
		Invoke(action_DestroyNoLoot, 60f);
	}

	private void StartPlayerRepulseCountdown()
	{
		mainCollider.enabled = false;
		((Behaviour)playerForcer).enabled = true;
		InvokeRepeatingFixedTime(action_CheckShouldDisablePlayerRepulse);
	}

	private void CheckShouldDisablePlayerRepulse()
	{
		if (noOfTicks < 10)
		{
			noOfTicks++;
			return;
		}
		mainCollider.enabled = true;
		((Behaviour)playerForcer).enabled = false;
		CancelInvokeFixedTime(action_CheckShouldDisablePlayerRepulse);
	}

	public void PermitUserId(ulong userId)
	{
		if (base.isServer)
		{
			if (permittedUserId != 0L)
			{
				Debug.LogError((object)$"Tried to permit user {userId} to {((Object)this).name} but {permittedUserId} has already been assigned");
			}
			else
			{
				permittedUserId = userId;
			}
		}
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if ((ulong)player.userID == permittedUserId)
		{
			return base.CanBeLooted(player);
		}
		return false;
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		if ((ulong)player.userID == permittedUserId)
		{
			Cleanup_DropItems();
		}
	}

	public override void OnKilled()
	{
		base.OnKilled();
		if (permittedUserId != 0L && BasePlayer.TryFindAwakeOrSleepingByID(permittedUserId, out var basePlayer))
		{
			BaseMission.MissionEventPayload payload = new BaseMission.MissionEventPayload
			{
				UintIdentifier = prefabID
			};
			basePlayer.ProcessMissionEvent(BaseMission.MissionEventType.KILL_ENTITY, payload, 0f);
			if (PlayerHasValidMission(basePlayer, out var activeMissionInstance, out var _))
			{
				activeMissionInstance.persistentMissionEntities.Remove((BaseEntity)this);
				basePlayer.MissionsDirty(saveImmediately: true);
			}
		}
	}

	private void Cleanup_NoLoot()
	{
		base.inventory.Clear();
		Kill(DestroyMode.Gib);
	}

	private void Cleanup_DropItems()
	{
		DropItems();
		Kill(DestroyMode.Gib);
	}

	[RPC_Server]
	private void Server_UpdateNoteText(RPCMessage msg)
	{
		Item slot = base.inventory.GetSlot(0);
		if (slot != null)
		{
			slot.text = msg.read.String();
			slot.MarkDirty();
		}
	}

	private bool PlayerHasValidMission(BasePlayer player, out BaseMission.MissionInstance activeMissionInstance, out int activeObjectiveIndex)
	{
		activeMissionInstance = null;
		activeObjectiveIndex = -1;
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (!player.TryGetActiveMissionInstance(out activeMissionInstance))
		{
			return false;
		}
		BaseMission mission = activeMissionInstance.GetMission();
		int count = activeMissionInstance.objectiveStatuses.Count;
		int num = mission.objectives.Length;
		if (count != num)
		{
			Debug.LogError((object)$"Mission instance for mission {((Object)mission).name} contains data for {count} objectives but mission has {num} objectives", (Object)(object)mission);
			return false;
		}
		for (int i = 0; i < validMissions.Length; i++)
		{
			MissionEntry missionEntry = validMissions[i];
			if (mission != missionEntry.mission)
			{
				continue;
			}
			for (int j = 0; j < activeMissionInstance.objectiveStatuses.Count; j++)
			{
				BaseMission.MissionInstance.ObjectiveStatus objectiveStatus = activeMissionInstance.objectiveStatuses[j];
				MissionObjective objective = mission.objectives[j].objective;
				if (!objectiveStatus.IsObjectiveActive())
				{
					continue;
				}
				for (int k = 0; k < missionEntry.objectives.Length; k++)
				{
					if ((Object)(object)objective == (Object)(object)missionEntry.objectives[k])
					{
						activeObjectiveIndex = j;
						return true;
					}
				}
			}
		}
		return false;
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: permittedUserId for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_permittedUserId);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 0)
		{
			try
			{
				_ = __sync_permittedUserId;
				ulong _sync_permittedUserId = reader.UInt64();
				__sync_permittedUserId = _sync_permittedUserId;
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
		if (propertyName == "permittedUserId")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
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
		__sync_permittedUserId = 0uL;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}
}
