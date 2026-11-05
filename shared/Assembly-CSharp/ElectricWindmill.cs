using System;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using UnityEngine;

public class ElectricWindmill : IOEntity
{
	public int maxPowerGeneration = 100;

	public Transform vaneRot;

	public Transform sailsRot;

	public Transform transmissionRot;

	public SoundDefinition wooshSound;

	public Transform wooshOrigin;

	public SoundPlayer mechanicalSound;

	private float cachedHeight;

	private float __sync_WindSpeedScale;

	private float __sync_WindAimTime;

	[Sync(Autosave = true)]
	public float WindSpeedScale
	{
		[CompilerGenerated]
		get
		{
			return __sync_WindSpeedScale;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_WindSpeedScale, value))
			{
				__sync_WindSpeedScale = value;
				byte nameID = __GetWeaverID("WindSpeedScale");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public float WindAimTime
	{
		[CompilerGenerated]
		get
		{
			return __sync_WindAimTime;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_WindAimTime, value))
			{
				__sync_WindAimTime = value;
				byte nameID = __GetWeaverID("WindAimTime");
				QueueSyncVar(nameID);
			}
		}
	}

	public override int MaximalPowerOutput()
	{
		return maxPowerGeneration;
	}

	public override bool IsRootEntity()
	{
		return true;
	}

	public Vector3 GetWindAimDir(float time)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		float num = time / 3600f * 360f;
		int num2 = 10;
		Vector3 val = new Vector3(Mathf.Sin(num * (MathF.PI / 180f)) * (float)num2, 0f, Mathf.Cos(num * (MathF.PI / 180f)) * (float)num2);
		return val.normalized;
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public override void ServerInit()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		InvokeRandomized(WindUpdate, 1f, 20f, 2f);
		if (GamePhysics.Trace(new Ray(((Component)this).transform.position, Vector3.down), 0f, out var hitInfo, 10f, 8454144, (QueryTriggerInteraction)1, this))
		{
			cachedHeight = ((Component)this).transform.position.y - hitInfo.point.y;
		}
		else
		{
			cachedHeight = ((Component)this).transform.position.y - TerrainMeta.HeightMap.GetHeight(((Component)this).transform.position);
		}
	}

	public bool AmIVisible()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		int num = 15;
		Vector3 val = ((Component)this).transform.position + Vector3.up * 6f;
		if (!IsVisible(val + ((Component)this).transform.up * (float)num, (float)(num + 1)))
		{
			return false;
		}
		Vector3 windAimDir = GetWindAimDir(Time.time);
		if (!IsVisible(val + windAimDir * (float)num, (float)(num + 1)))
		{
			return false;
		}
		return true;
	}

	public void WindUpdate()
	{
		if (Interface.CallHook("OnWindmillUpdate", this) == null)
		{
			WindSpeedScale = (AmIVisible() ? GetWindSpeedScale() : 0f);
			WindAimTime = Time.time;
			int num = Mathf.FloorToInt((float)maxPowerGeneration * WindSpeedScale);
			bool flag = currentEnergy != num;
			currentEnergy = num;
			if (flag)
			{
				MarkDirty();
			}
			Interface.CallHook("OnWindmillUpdated", this);
		}
	}

	public float GetWindSpeedScale()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.time / 600f;
		float num2 = ((Component)this).transform.position.x / 512f;
		float num3 = ((Component)this).transform.position.z / 512f;
		float num4 = Mathf.PerlinNoise(num2 + num, num3 + num * 0.1f);
		float num5 = cachedHeight;
		if (num5 < 0f)
		{
			num5 = 0f;
		}
		return Mathf.Clamp01(Mathf.InverseLerp(0f, 50f, num5) * 0.5f + num4);
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (outputSlot != 0)
		{
			return 0;
		}
		return currentEnergy;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: WindSpeedScale for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_WindSpeedScale);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: WindAimTime for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_WindAimTime);
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
				_ = __sync_WindSpeedScale;
				float _sync_WindSpeedScale = reader.Float();
				__sync_WindSpeedScale = _sync_WindSpeedScale;
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_WindAimTime;
				float _sync_WindAimTime = reader.Float();
				__sync_WindAimTime = _sync_WindAimTime;
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
		if (!(propertyName == "WindSpeedScale"))
		{
			if (propertyName == "WindAimTime")
			{
				return 1;
			}
			return byte.MaxValue;
		}
		return 0;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
		WriteSyncVar(1, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
		OnSyncVar(1, reader, fromAutoSave: true);
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
		__sync_WindSpeedScale = 0f;
		__sync_WindAimTime = 0f;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}
}
