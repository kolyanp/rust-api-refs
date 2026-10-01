using System;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class LivestockCorpse : BaseCorpse
{
	private int __sync_Genes;

	private string __sync_AnimalName;

	private bool __sync_IsMale;

	private int __sync_ShornFleece;

	[Sync(Autosave = true)]
	public int Genes
	{
		[CompilerGenerated]
		get
		{
			return __sync_Genes;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_Genes, value))
			{
				__sync_Genes = value;
				byte nameID = __GetWeaverID("Genes");
				QueueSyncVar(nameID);
			}
		}
	}

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

	[Sync(Autosave = true)]
	public bool IsMale
	{
		[CompilerGenerated]
		get
		{
			return __sync_IsMale;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_IsMale, value))
			{
				__sync_IsMale = value;
				byte nameID = __GetWeaverID("IsMale");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public int ShornFleece
	{
		[CompilerGenerated]
		get
		{
			return __sync_ShornFleece;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_ShornFleece, value))
			{
				__sync_ShornFleece = value;
				byte nameID = __GetWeaverID("ShornFleece");
				QueueSyncVar(nameID);
			}
		}
	}

	public override void ServerInitCorpse(BaseEntity pr, Vector3 posOnDeath, Quaternion rotOnDeath, BasePlayer.PlayerFlags playerFlagsOnDeath, ModelState modelState)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInitCorpse(pr, posOnDeath, rotOnDeath, playerFlagsOnDeath, modelState);
		if (pr is LivestockAnimal livestockAnimal)
		{
			Genes = livestockAnimal.Genes;
			AnimalName = livestockAnimal.AnimalName;
			IsMale = livestockAnimal.IsMale;
			ShornFleece = livestockAnimal.ShornFleece;
			livestockAnimal.AddCorpseYield(((Component)this).GetComponent<ResourceDispenser>());
		}
	}

	public override bool FillHeadData(HeadEntity head)
	{
		head.AssignLivestock(Genes, AnimalName);
		return true;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: Genes for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_Genes);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: AnimalName for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_AnimalName);
			return true;
		case 2:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: IsMale for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_IsMale);
			return true;
		case 3:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: ShornFleece for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_ShornFleece);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		switch (id)
		{
		case 0:
			try
			{
				_ = __sync_Genes;
				int _sync_Genes = reader.Int32();
				__sync_Genes = _sync_Genes;
			}
			catch (Exception ex4)
			{
				Debug.LogException(ex4);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_AnimalName;
				string _sync_AnimalName = reader.String();
				__sync_AnimalName = _sync_AnimalName;
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 2:
			try
			{
				_ = __sync_IsMale;
				bool _sync_IsMale = reader.Bool();
				__sync_IsMale = _sync_IsMale;
			}
			catch (Exception ex3)
			{
				Debug.LogException(ex3);
			}
			return true;
		case 3:
			try
			{
				_ = __sync_ShornFleece;
				int _sync_ShornFleece = reader.Int32();
				__sync_ShornFleece = _sync_ShornFleece;
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
			"Genes" => (byte)0, 
			"AnimalName" => (byte)1, 
			"IsMale" => (byte)2, 
			"ShornFleece" => (byte)3, 
			_ => byte.MaxValue, 
		};
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
		WriteSyncVar(1, writer);
		WriteSyncVar(2, writer);
		WriteSyncVar(3, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
		OnSyncVar(1, reader, fromAutoSave: true);
		OnSyncVar(2, reader, fromAutoSave: true);
		OnSyncVar(3, reader, fromAutoSave: true);
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
		__sync_Genes = 0;
		__sync_AnimalName = null;
		__sync_IsMale = false;
		__sync_ShornFleece = 0;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			2 => true, 
			3 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}
}
