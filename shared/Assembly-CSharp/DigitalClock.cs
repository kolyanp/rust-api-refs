using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class DigitalClock : IOEntity, INotifyLOD
{
	public struct Alarm
	{
		public TimeSpan time;

		public bool active;

		public Alarm(TimeSpan time, bool active)
		{
			this.time = time;
			this.active = active;
		}

		public Alarm(DigitalClockAlarm alarm)
		{
			time = DigitalClockEx.ToTimeSpan(alarm.time);
			active = alarm.active;
		}
	}

	[SerializeField]
	private TextMeshPro clockText;

	[SerializeField]
	private SoundDefinition ringingSoundDef;

	[SerializeField]
	private SoundDefinition ringingStartSoundDef;

	[SerializeField]
	private SoundDefinition ringingStopSoundDef;

	public GameObjectRef clockConfigPanel;

	public List<Alarm> alarms = new List<Alarm>();

	[HideInInspector]
	public bool muted;

	private bool isRinging;

	public const int MAX_ALARMS = 5;

	public const float ringDuration = 5f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("DigitalClock.OnRpcMessage"))
		{
			if (rpc == 2287159130u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_SetAlarms"));
				}
				using (TimeWarning.New("RPC_SetAlarms"))
				{
					using (msg.read.UseRepeatedElementLimit(5))
					{
						using (TimeWarning.New("Conditions"))
						{
							if (!RPC_Server.CallsPerSecond.Test(2287159130u, "RPC_SetAlarms", this, player, 5uL))
							{
								return true;
							}
							long position = msg.read.Position;
							DigitalClockMessage val = msg.read.Proto<DigitalClockMessage>((DigitalClockMessage)null);
							try
							{
								foreach (DigitalClockAlarm alarm in val.alarms)
								{
									if (!RPC_Server.InputValidation.Test(alarm.time))
									{
										return true;
									}
								}
								msg.read.Position = position;
								if (!RPC_Server.IsVisible.Test(2287159130u, "RPC_SetAlarms", this, player, 3f))
								{
									return true;
								}
							}
							finally
							{
								((IDisposable)val)?.Dispose();
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
								RPC_SetAlarms(msg2);
							}
						}
						catch (Exception ex)
						{
							Debug.LogException(ex);
							player.Kick("RPC Error in RPC_SetAlarms");
						}
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private void OnMinute()
	{
		if (IsOn() && base.isServer)
		{
			CheckAlarms();
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	public bool CanPlayerAdmin(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			return player.CanBuild();
		}
		return false;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		TOD_Sky.Instance.Components.Time.OnMinute += OnMinute;
	}

	private void CheckAlarms()
	{
		TimeSpan timeOfDay = TOD_Sky.Instance.Cycle.DateTime.TimeOfDay;
		foreach (Alarm alarm in alarms)
		{
			if (!alarm.active)
			{
				continue;
			}
			int hours = timeOfDay.Hours;
			TimeSpan time = alarm.time;
			if (hours == time.Hours)
			{
				int minutes = timeOfDay.Minutes;
				time = alarm.time;
				if (minutes == time.Minutes)
				{
					Ring();
				}
			}
		}
	}

	private void Ring()
	{
		if (Interface.CallHook("OnDigitalClockRing", this) == null)
		{
			isRinging = true;
			ClientRPC(RpcTarget.NetworkGroup("RPC_StartRinging"));
			Invoke(StopRinging, 5f);
			MarkDirty();
		}
	}

	private void StopRinging()
	{
		if (Interface.CallHook("OnDigitalClockRingStop", this) == null)
		{
			isRinging = false;
			ClientRPC(RpcTarget.NetworkGroup("RPC_StopRinging"));
			MarkDirty();
		}
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.InputValidation(new Type[] { typeof(DigitalClockMessage) })]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxRepeatedElements(5)]
	public void RPC_SetAlarms(RPCMessage msg)
	{
		if (!CanPlayerAdmin(msg.player))
		{
			return;
		}
		DigitalClockMessage val = msg.read.Proto<DigitalClockMessage>((DigitalClockMessage)null);
		if (Interface.CallHook("OnDigitalClockAlarmsSet", this, val) != null)
		{
			return;
		}
		try
		{
			if (val.alarms.Count > 5)
			{
				return;
			}
			foreach (DigitalClockAlarm alarm in val.alarms)
			{
				if (alarm.time < 0f || alarm.time >= 24f)
				{
					return;
				}
			}
			alarms.Clear();
			foreach (DigitalClockAlarm alarm2 in val.alarms)
			{
				Alarm item = new Alarm(DigitalClockEx.ToTimeSpan(alarm2.time), alarm2.active);
				alarms.Add(item);
			}
			muted = val.muted;
			MarkDirty();
			SendNetworkUpdate();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!isRinging)
		{
			return 0;
		}
		return base.GetPassthroughAmount(outputSlot);
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		if (inputAmount == 0)
		{
			ResetIOState();
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, HasFlag(Flags.Reserved8));
	}

	public override void ResetIOState()
	{
		isRinging = false;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: false);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.digitalClock = Pool.Get<DigitalClock>();
		List<DigitalClockAlarm> list = Pool.Get<List<DigitalClockAlarm>>();
		foreach (Alarm alarm in alarms)
		{
			DigitalClockAlarm val = Pool.Get<DigitalClockAlarm>();
			val.active = alarm.active;
			val.time = DigitalClockEx.ToFloat(alarm.time);
			list.Add(val);
		}
		info.msg.digitalClock.alarms = list;
		info.msg.digitalClock.muted = muted;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.digitalClock == null)
		{
			return;
		}
		alarms.Clear();
		foreach (DigitalClockAlarm alarm in info.msg.digitalClock.alarms)
		{
			if (alarms.Count >= 5)
			{
				break;
			}
			alarms.Add(new Alarm(alarm));
		}
		muted = info.msg.digitalClock.muted;
	}
}
