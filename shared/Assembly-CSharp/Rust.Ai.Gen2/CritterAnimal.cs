using System;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class CritterAnimal : BaseNPC2, ISimpleHearingReceiver
{
	public enum CritterType
	{
		Frog,
		Rabbit,
		Squirrel,
		Crabs
	}

	public const Flags Interested = Flags.Reserved8;

	public const Flags Deactivated = Flags.Reserved12;

	public CritterType CritterSpecies;

	public bool resetAnimatorOnEnable;

	private TimeSince lastLoudNoise;

	public const int MaxAnimalNameLength = 24;

	private string __sync_AnimalName;

	[Sync(Autosave = true)]
	public string AnimalName
	{
		[CompilerGenerated]
		get
		{
			return __sync_AnimalName;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_AnimalName, value))
			{
				__sync_AnimalName = value;
				byte nameID = __GetWeaverID("AnimalName");
				QueueSyncVar(nameID);
			}
		}
	}

	public bool RecentlyHeardLoudNoise
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return TimeSince.op_Implicit(lastLoudNoise) < 10f;
		}
	}

	public bool IsInterested()
	{
		return HasFlag(Flags.Reserved8);
	}

	public bool IsDeactivated()
	{
		return HasFlag(Flags.Reserved12);
	}

	public override void ServerInit()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		lastLoudNoise = TimeSince.op_Implicit(100f);
	}

	public void SetInterested(bool interested)
	{
		SetNetworkedFlag(Flags.Reserved8, interested);
	}

	public void SetDeactivatedFlag(bool toggle)
	{
		SetNetworkedFlag(Flags.Reserved12, toggle);
	}

	protected void SetNetworkedFlag(Flags flag, bool value)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(flag, value);
	}

	public void OnHeardNoise(NpcNoiseEvent noise)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (noise.Intensity >= NpcNoiseIntensity.Medium)
		{
			lastLoudNoise = TimeSince.op_Implicit(0f);
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
				Debug.Log((object)("SyncVar Writing: AnimalName for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_AnimalName);
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
				_ = __sync_AnimalName;
				string _sync_AnimalName = reader.String();
				__sync_AnimalName = _sync_AnimalName;
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
		if (propertyName == "AnimalName")
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
		__sync_AnimalName = null;
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
