using System;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;

public class SatelliteDish : BaseEntity
{
	public const float DefaultTurnSpeed = 15f;

	[Header("Satellite Dish")]
	[Tooltip("Turns about the entity's up axis")]
	public Transform yawPivot;

	[Tooltip("Tilts about the yaw pivot's right axis, its up axis is the dish's aim")]
	public Transform pitchPivot;

	public float minElevation;

	public float maxElevation = 90f;

	public SoundDefinition rotationLoopDef;

	private float currentYaw;

	private float currentElevation;

	private Action _actionRotateTick;

	private float __sync_Yaw;

	private float __sync_Elevation;

	private float __sync_TurnSpeed;

	[Sync(Autosave = true)]
	public float Yaw
	{
		[CompilerGenerated]
		get
		{
			return __sync_Yaw;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_Yaw, value))
			{
				__sync_Yaw = value;
				byte nameID = __GetWeaverID("Yaw");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public float Elevation
	{
		[CompilerGenerated]
		get
		{
			return __sync_Elevation;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_Elevation, value))
			{
				__sync_Elevation = value;
				byte nameID = __GetWeaverID("Elevation");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public float TurnSpeed
	{
		[CompilerGenerated]
		get
		{
			return __sync_TurnSpeed;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_TurnSpeed, value))
			{
				__sync_TurnSpeed = value;
				byte nameID = __GetWeaverID("TurnSpeed");
				QueueSyncVar(nameID);
			}
		}
	}

	private Action actionRotateTick => RotateTick;

	public float CurrentYaw => currentYaw;

	public float CurrentElevation => currentElevation;

	public bool IsAimed
	{
		get
		{
			if (Mathf.Approximately(Mathf.DeltaAngle(currentYaw, Yaw), 0f))
			{
				return Mathf.Approximately(currentElevation, Elevation);
			}
			return false;
		}
	}

	public Vector3 AimDirection
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return pitchPivot.up;
		}
	}

	public event Action<SatelliteDish> AimReached;

	private void SnapToTarget()
	{
		currentYaw = Yaw;
		currentElevation = Elevation;
		ApplyRotation();
	}

	private void ApplyRotation()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		yawPivot.localRotation = Quaternion.Euler(0f, currentYaw, 0f);
		pitchPivot.localRotation = Quaternion.Euler(90f - currentElevation, 0f, 0f);
	}

	private void StartRotating()
	{
		if (!IsInvoking(actionRotateTick))
		{
			InvokeRepeating(actionRotateTick, 0f, 0f);
		}
	}

	private void RotateTick()
	{
		if (TurnSpeed <= 0f)
		{
			currentYaw = Yaw;
			currentElevation = Elevation;
		}
		else
		{
			float num = TurnSpeed * Time.deltaTime;
			currentYaw = Mathf.Repeat(Mathf.MoveTowardsAngle(currentYaw, Yaw, num), 360f);
			currentElevation = Mathf.MoveTowards(currentElevation, Elevation, num);
		}
		ApplyRotation();
		if (IsAimed)
		{
			CancelInvoke(actionRotateTick);
			if (isServer)
			{
				AimReached?.Invoke(this);
			}
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		SnapToTarget();
	}

	public void SetAim(float yaw, float elevation)
	{
		Yaw = Mathf.Repeat(yaw, 360f);
		Elevation = Mathf.Clamp(elevation, minElevation, maxElevation);
		StartRotating();
	}

	public void AimAt(Vector3 worldPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		AimInDirection(worldPosition - pitchPivot.position);
	}

	public void AimInDirection(Vector3 worldDirection)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!(worldDirection.sqrMagnitude < 0.0001f))
		{
			Vector3 val = ((Component)this).transform.InverseTransformDirection(worldDirection);
			Vector3 normalized = val.normalized;
			float yaw = Mathf.Atan2(normalized.x, normalized.z) * 57.29578f;
			float elevation = Mathf.Asin(Mathf.Clamp(normalized.y, -1f, 1f)) * 57.29578f;
			SetAim(yaw, elevation);
		}
	}

	public void SetTurnSpeed(float degreesPerSecond)
	{
		TurnSpeed = degreesPerSecond;
		if (!IsAimed)
		{
			StartRotating();
		}
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: Yaw for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_Yaw);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: Elevation for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_Elevation);
			return true;
		case 2:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: TurnSpeed for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_TurnSpeed);
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
				_ = __sync_Yaw;
				float _sync_Yaw = reader.Float();
				__sync_Yaw = _sync_Yaw;
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_Elevation;
				float _sync_Elevation = reader.Float();
				__sync_Elevation = _sync_Elevation;
			}
			catch (Exception ex3)
			{
				Debug.LogException(ex3);
			}
			return true;
		case 2:
			try
			{
				_ = __sync_TurnSpeed;
				float _sync_TurnSpeed = reader.Float();
				__sync_TurnSpeed = _sync_TurnSpeed;
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
			"Yaw" => (byte)0, 
			"Elevation" => (byte)1, 
			"TurnSpeed" => (byte)2, 
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
		__sync_Yaw = 0f;
		__sync_Elevation = 30f;
		__sync_TurnSpeed = 15f;
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

	public SatelliteDish()
	{
		Elevation = 30f;
		TurnSpeed = 15f;
		currentElevation = 30f;
		__sync_Elevation = 30f;
		__sync_TurnSpeed = 15f;
		base._002Ector();
	}
}
