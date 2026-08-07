using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;

public class WaterTreatmentWaterTank : IOEntity
{
	[Header("Water Treatment Water Tank")]
	public Transform BladesTransform;

	public WaterBody Water;

	[Header("Blade Movement Settings")]
	public AnimationCurve BladeSpinUpCurve;

	public AnimationCurve BladeWindDownCurve;

	public AnimationCurve WaterBlendOverTimeCurve;

	[Header("Pressure Gauges")]
	public List<ClientPressureGauge> pressureGauges;

	[Header("Blade Sounds")]
	public GameObject soundEmitterObject;

	public SoundDefinition startBladeSound;

	public SoundDefinition bladeLoopSound;

	public SoundDefinition stopBladeSound;

	public SoundDefinition waterMovementLoopSound;

	public SoundDefinition spinnerFullyStoppedSound;

	[ReplicatedVar(Saved = true)]
	public static float maxPressure = 300f;

	[ServerVar(Saved = true)]
	public static float pressureOnThreshold = 200f;

	[ServerVar(Saved = true)]
	public static float pressureOffThreshold = 50f;

	[ServerVar(Saved = true)]
	public static float secondsBeforeDecayingPressure = 600f;

	[ServerVar(Saved = true)]
	public static float pressureDecayPerTick = 2f;

	public TimeSince timeSinceLastPressureIncrease;

	private float __sync_Pressure;

	[Sync(Autosave = true)]
	public float Pressure
	{
		[CompilerGenerated]
		get
		{
			return __sync_Pressure;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_Pressure, value))
			{
				__sync_Pressure = value;
				byte nameID = __GetWeaverID("Pressure");
				QueueSyncVar(nameID);
			}
		}
	}

	private float pressureToBeat
	{
		get
		{
			if (!IsOn())
			{
				return pressureOnThreshold;
			}
			return pressureOffThreshold;
		}
	}

	[ServerVar]
	public static void debug_wtp_pressure(ConsoleSystem.Arg arg)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		WaterTreatmentWaterTank[] array = Object.FindObjectsByType<WaterTreatmentWaterTank>((FindObjectsInactive)0, (FindObjectsSortMode)0);
		string text = "WaterTreatmentPlant WaterTank Pressures:\n";
		WaterTreatmentWaterTank[] array2 = array;
		foreach (WaterTreatmentWaterTank waterTreatmentWaterTank in array2)
		{
			if ((Object)(object)waterTreatmentWaterTank != (Object)null)
			{
				text += string.Format("Water Tank id:{0} Current Pressure: {1}, Is On: {2}, TimeSinceLastPressureIncrease: {3}\n", new object[4]
				{
					waterTreatmentWaterTank.GetEntity().net.ID,
					waterTreatmentWaterTank.Pressure,
					waterTreatmentWaterTank.Pressure >= pressureOnThreshold,
					waterTreatmentWaterTank.timeSinceLastPressureIncrease
				});
			}
		}
		text += string.Format("Max Pressure: {0}, Pressure On Threshold: {1}, Pressure Off Threshold: {2}, Seconds Before Decaying Pressure: {3}, Pressure Decay Per Tick: {4}", new object[5] { maxPressure, pressureOnThreshold, pressureOffThreshold, secondsBeforeDecayingPressure, pressureDecayPerTick });
		arg.ReplyWith(text);
	}

	[ServerVar]
	public static void force_pressure(ConsoleSystem.Arg arg)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		int num = arg.GetInt(0, 200);
		WaterTreatmentWaterTank[] array = Object.FindObjectsByType<WaterTreatmentWaterTank>((FindObjectsInactive)0, (FindObjectsSortMode)0);
		WaterTreatmentWaterTank[] array2 = array;
		foreach (WaterTreatmentWaterTank waterTreatmentWaterTank in array2)
		{
			if ((Object)(object)waterTreatmentWaterTank != (Object)null)
			{
				waterTreatmentWaterTank.Pressure = num;
				waterTreatmentWaterTank.timeSinceLastPressureIncrease = TimeSince.op_Implicit(0f);
			}
		}
		arg.ReplyWith($"Forced pressure to ({num}) for ({array.Length}) water tanks.");
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Pressure = 0f;
		InvokeRepeating(TickWaterPressure, 5f, 5f);
	}

	public override void ResetIOState()
	{
		Pressure = 0f;
		SetFlagLocal(Flags.On, b: false);
		SendNetworkUpdate_Flags();
		base.ResetIOState();
	}

	public override float IOInput(IOEntity from, IOType inputType, float inputAmount, int slot = 0)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (inputAmount > 0f)
		{
			if (Pressure >= maxPressure)
			{
				return inputAmount;
			}
			Pressure += inputAmount;
			timeSinceLastPressureIncrease = TimeSince.op_Implicit(0f);
			return 0f;
		}
		return inputAmount;
	}

	private void TickWaterPressure()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (TimeSince.op_Implicit(timeSinceLastPressureIncrease) >= secondsBeforeDecayingPressure)
		{
			Pressure -= pressureDecayPerTick;
			Pressure = Mathf.Max(Pressure, 0f);
		}
		bool b = Pressure >= pressureToBeat;
		SetFlagLocal(Flags.On, b);
		SendNetworkUpdate_Flags();
		MarkDirty();
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!IsOn())
		{
			return 0;
		}
		return 100;
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
				Debug.Log((object)("SyncVar Writing: Pressure for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_Pressure);
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
				_ = __sync_Pressure;
				float _sync_Pressure = reader.Float();
				__sync_Pressure = _sync_Pressure;
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
		if (propertyName == "Pressure")
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
		__sync_Pressure = 0f;
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
