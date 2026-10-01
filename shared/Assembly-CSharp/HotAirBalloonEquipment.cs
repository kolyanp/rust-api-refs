using System;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;

public class HotAirBalloonEquipment : BaseCombatEntity
{
	[SerializeField]
	private DamageRenderer damageRenderer;

	public HealthThresholdToggle healthThresholdToggle;

	[HideInInspector]
	public float DelayNextUpgradeOnRemoveDuration;

	private EntityRef<HotAirBalloon> hotAirBalloon;

	private int __sync_DetachedPanelsSync;

	[Sync(Autosave = true)]
	public int DetachedPanelsSync
	{
		[CompilerGenerated]
		get
		{
			return __sync_DetachedPanelsSync;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_DetachedPanelsSync, value))
			{
				__sync_DetachedPanelsSync = value;
				byte nameID = __GetWeaverID("DetachedPanelsSync");
				QueueSyncVar(nameID);
			}
		}
	}

	public override float Health()
	{
		if (GetParentEntity() is HotAirBalloon hotAirBalloon)
		{
			return hotAirBalloon.Health();
		}
		return base.Health();
	}

	public override float MaxHealth()
	{
		if (GetParentEntity() is HotAirBalloon hotAirBalloon)
		{
			return hotAirBalloon.MaxHealth();
		}
		return base.MaxHealth();
	}

	public virtual void Added(HotAirBalloon hab, bool fromSave)
	{
		hotAirBalloon.Set(hab);
	}

	public virtual void Removed(HotAirBalloon hab)
	{
		hotAirBalloon.Set(null);
	}

	public void NoteBalloonDamage(HitInfo info, Transform balloon)
	{
		if ((Object)(object)healthThresholdToggle != (Object)null)
		{
			healthThresholdToggle.NoteDamage(info, balloon);
		}
	}

	public void UpdateArmorPanels(float balloonHealthFraction, float healedHealthFraction)
	{
		if (!((Object)(object)healthThresholdToggle == (Object)null))
		{
			if (healedHealthFraction > 0f)
			{
				healthThresholdToggle.NoteHealing(healedHealthFraction);
			}
			DetachedPanelsSync = healthThresholdToggle.UpdateDetachedMask(DetachedPanelsSync, balloonHealthFraction);
			healthThresholdToggle.ApplyMask(DetachedPanelsSync);
		}
	}

	public override void DoRepair(BasePlayer player)
	{
		HotAirBalloon hotAirBalloon = this.hotAirBalloon.Get(serverside: true);
		if (hotAirBalloon.IsValid())
		{
			hotAirBalloon.DoRepair(player);
		}
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: DetachedPanelsSync for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_DetachedPanelsSync);
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
				_ = __sync_DetachedPanelsSync;
				int _sync_DetachedPanelsSync = reader.Int32();
				__sync_DetachedPanelsSync = _sync_DetachedPanelsSync;
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
		if (propertyName == "DetachedPanelsSync")
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
		(byte[] Buffer, int Length) buffer = netWrite.GetBuffer();
		byte[] item = buffer.Buffer;
		int item2 = buffer.Length;
		byte[] array = _autosaveBuffer;
		if (array == null || array.Length < item2)
		{
			byte[] array2 = BaseEntity._autosaveBufferPool.Rent(item2);
			while (array == null || array.Length < item2)
			{
				byte[] array3 = Interlocked.CompareExchange(ref _autosaveBuffer, array2, array);
				if (array3 == array)
				{
					if (array3 != null)
					{
						BaseEntity._autosaveBufferPool.Return(array3);
					}
					array = array2;
					break;
				}
				array = array3;
			}
			if (array != array2)
			{
				BaseEntity._autosaveBufferPool.Return(array2);
			}
		}
		Buffer.BlockCopy(item, 0, array, 0, item2);
		save.msg.baseEntity.syncVars = array;
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
		__sync_DetachedPanelsSync = 0;
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
