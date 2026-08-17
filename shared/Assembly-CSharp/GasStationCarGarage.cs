using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class GasStationCarGarage : ModularCarGarage
{
	[Header("GasStationCarGarage")]
	public Transform liftTransform;

	public Transform loweredPosition;

	public Transform raisedPosition;

	public AnimationCurve movementAnimationCurve;

	public float movementDuration = 5f;

	public VehicleLiftOccupantTrigger bottomCrusherOccupantTrigger;

	public VehicleLiftOccupantTrigger topCrusherOccupantTrigger;

	public SoundDefinition liftStartMovingSound;

	public SoundDefinition liftMovingLoopSound;

	public SoundDefinition liftStopMovingSound;

	private VehicleLiftState LiftHeightState = VehicleLiftState.Up;

	private bool isMoving;

	private TimeSince timeSinceStartMove;

	private bool __sync_isLiftUp;

	protected override bool LiftIsUp => LiftHeightState == VehicleLiftState.Up;

	protected override bool LiftIsMoving => isMoving;

	protected override bool LiftIsDown => LiftHeightState == VehicleLiftState.Down;

	[Sync(Autosave = true)]
	public bool isLiftUp
	{
		[CompilerGenerated]
		get
		{
			return __sync_isLiftUp;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_isLiftUp, value))
			{
				__sync_isLiftUp = value;
				byte nameID = __GetWeaverID("isLiftUp");
				QueueSyncVar(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("GasStationCarGarage.OnRpcMessage"))
		{
			if (rpc == 401754885 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ToggleLiftHeight"));
				}
				using (TimeWarning.New("RPC_ToggleLiftHeight"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(401754885u, "RPC_ToggleLiftHeight", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(401754885u, "RPC_ToggleLiftHeight", this, player, 3f))
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
							RPC_ToggleLiftHeight(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_ToggleLiftHeight");
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
		isLiftUp = true;
		LiftHeightState = VehicleLiftState.Up;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_ToggleLiftHeight(RPCMessage msg)
	{
		bool flag = msg.read.Bool();
		isLiftUp = flag;
		LiftHeightState = (isLiftUp ? VehicleLiftState.Up : VehicleLiftState.Down);
		StartLiftMovement();
	}

	public void StartLiftMovement()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!isMoving)
		{
			timeSinceStartMove = TimeSince.op_Implicit(0f);
			isMoving = true;
			if (base.isServer)
			{
				InvokeRepeatingFixedTime(ProcessLiftMovement);
			}
			else if (base.isClient)
			{
				InvokeRepeating(ProcessLiftMovement, 0f, 0f);
			}
		}
	}

	public void ProcessLiftMovement()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		float num = ((movementDuration > 0f) ? Mathf.Clamp01(TimeSince.op_Implicit(timeSinceStartMove) / movementDuration) : 1f);
		float num2 = movementAnimationCurve.Evaluate(num);
		Vector3 val = ((LiftHeightState == VehicleLiftState.Up) ? loweredPosition.position : raisedPosition.position);
		Vector3 val2 = ((LiftHeightState == VehicleLiftState.Up) ? raisedPosition.position : loweredPosition.position);
		liftTransform.position = Vector3.LerpUnclamped(val, val2, num2);
		if (num >= 0.4f)
		{
			if (num <= 0.6f)
			{
				if ((Object)(object)bottomCrusherOccupantTrigger.carOccupant != (Object)null)
				{
					bottomCrusherOccupantTrigger.carOccupant.Kill(DestroyMode.Gib);
				}
				else if ((Object)(object)bottomCrusherOccupantTrigger.vehicleOccupant != (Object)null)
				{
					bottomCrusherOccupantTrigger.vehicleOccupant.Die();
				}
			}
			if ((Object)(object)topCrusherOccupantTrigger.carOccupant != (Object)null)
			{
				topCrusherOccupantTrigger.carOccupant.Kill(DestroyMode.Gib);
			}
			else if ((Object)(object)topCrusherOccupantTrigger.vehicleOccupant != (Object)null)
			{
				topCrusherOccupantTrigger.vehicleOccupant.Die();
			}
		}
		if (num >= 1f)
		{
			liftTransform.position = val2;
			isMoving = false;
			base.GetVehicleLiftPos.hasChanged = true;
			if (base.isServer)
			{
				CancelInvokeFixedTime(ProcessLiftMovement);
			}
			if (base.isClient)
			{
				CancelInvoke(ProcessLiftMovement);
			}
		}
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
				Debug.Log((object)("SyncVar Writing: isLiftUp for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_isLiftUp);
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
				_ = __sync_isLiftUp;
				bool _sync_isLiftUp = reader.Bool();
				__sync_isLiftUp = _sync_isLiftUp;
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
		if (propertyName == "isLiftUp")
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
		__sync_isLiftUp = false;
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
