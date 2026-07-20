using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class SeismicSensor : IOEntity
{
	private static readonly BaseEntity[] resultBuffer = new BaseEntity[128];

	public static int MinRange = 1;

	public static int MaxRange = 30;

	public int range = 30;

	public GameObjectRef sensorPanelPrefab;

	private int vibrationLevel;

	private const int holdTime = 3;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SeismicSensor.OnRpcMessage"))
		{
			if (rpc == 128851379 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_SetRange"));
				}
				using (TimeWarning.New("RPC_SetRange"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(128851379u, "RPC_SetRange", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(128851379u, "RPC_SetRange", this, player, 3f))
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
							RPC_SetRange(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_SetRange");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static void Notify(Vector3 position, int value)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (value == 0)
		{
			return;
		}
		int inSphereFast = Query.Server.GetInSphereFast(position, MaxRange, resultBuffer, FilterOutSensors);
		for (int i = 0; i < inSphereFast; i++)
		{
			SeismicSensor seismicSensor = resultBuffer[i] as SeismicSensor;
			Vector3 position2 = ((Component)seismicSensor).transform.position;
			Vector3 val = position - position2;
			float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
			float num = (float)seismicSensor.range + 0.5f;
			if (sqrMagnitude < num * num)
			{
				seismicSensor.SetVibrationLevel(value);
			}
		}
	}

	private static bool FilterOutSensors(BaseEntity entity)
	{
		SeismicSensor seismicSensor = entity as SeismicSensor;
		if ((Object)(object)seismicSensor != (Object)null && BaseEntityEx.IsValidEntityReference(seismicSensor))
		{
			return seismicSensor.HasFlag(Flags.Reserved8);
		}
		return false;
	}

	public void SetVibrationLevel(int value)
	{
		float num = value;
		if (num <= 0f)
		{
			SetOff();
			return;
		}
		if (num > (float)vibrationLevel)
		{
			vibrationLevel = Mathf.RoundToInt(num);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: true);
			}
			MarkDirty();
		}
		if (IsInvoking(SetOff))
		{
			CancelInvoke(SetOff);
		}
		Invoke(SetOff, 3f);
	}

	public int GetVibrationLevel()
	{
		return vibrationLevel;
	}

	private void SetOff()
	{
		vibrationLevel = 0;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		MarkDirty();
	}

	public void SetRange(int value)
	{
		value = Mathf.Clamp(value, MinRange, MaxRange);
		range = value;
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	public void RPC_SetRange(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && player.CanBuild())
		{
			int num = msg.read.Int32();
			SetRange(num);
		}
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		if (inputAmount == 0)
		{
			ResetIOState();
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!IsPowered())
		{
			return 0;
		}
		return vibrationLevel;
	}

	public override void ResetIOState()
	{
		vibrationLevel = 0;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: false);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.ioEntity.genericInt1 = range;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			range = info.msg.ioEntity.genericInt1;
			if (info.fromDisk && base.isServer)
			{
				SetVibrationLevel(0);
			}
		}
	}
}
