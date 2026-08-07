using System;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class OilSwitchBroadcast : IOEntity
{
	public float OilOutputMultiplier = 1f;

	private static ListHashSet<IOilSwitchReceiver> receivers = new ListHashSet<IOilSwitchReceiver>();

	private static ListHashSet<OilSwitchBroadcast> activeSwitches = new ListHashSet<OilSwitchBroadcast>();

	private static float TotalOutputLevel
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			float num = 0f;
			Enumerator<OilSwitchBroadcast> enumerator = activeSwitches.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					OilSwitchBroadcast current = enumerator.Current;
					if ((Object)(object)current != (Object)null)
					{
						num += current.OilOutputMultiplier;
					}
				}
				return num;
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	public static void RegisterReceiver(IOilSwitchReceiver r)
	{
		receivers.TryAdd(r);
	}

	public static void DeregisterReceiver(IOilSwitchReceiver r)
	{
		receivers.Remove(r);
	}

	public override bool GetHasPower(int inputAmount, int inputSlot)
	{
		bool hasPower = base.GetHasPower(inputAmount, inputSlot);
		bool flag = false;
		if (!hasPower && activeSwitches.Contains(this))
		{
			activeSwitches.Remove(this);
			flag = true;
		}
		else if (hasPower && !activeSwitches.Contains(this))
		{
			activeSwitches.Add(this);
			flag = true;
		}
		if (flag)
		{
			Broadcast();
			if (hasPower && inputs.Length != 0 && inputs[0].IsConnected() && inputs[0].connectedTo.Get(base.isServer) is TimerSwitch timerSwitch && (Object)(object)timerSwitch.lastUsedPlayer != (Object)null)
			{
				timerSwitch.lastUsedPlayer.AddClanScore((ClanScoreEventType)17);
			}
		}
		return hasPower;
	}

	private static void Broadcast()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		float totalOutputLevel = TotalOutputLevel;
		Enumerator<IOilSwitchReceiver> enumerator = receivers.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.OnOilSwitchToggled(totalOutputLevel);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.oilswitchBroadcast = Pool.Get<OilSwitchBroadcast>();
			info.msg.oilswitchBroadcast.oilOutputMultiplier = OilOutputMultiplier;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.oilswitchBroadcast != null)
		{
			OilOutputMultiplier = info.msg.oilswitchBroadcast.oilOutputMultiplier;
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (activeSwitches.Contains(this))
		{
			activeSwitches.Remove(this);
			Broadcast();
		}
	}

	public override void ResetIOState()
	{
		base.ResetIOState();
		if (activeSwitches.Contains(this))
		{
			activeSwitches.Remove(this);
			Broadcast();
		}
	}
}
