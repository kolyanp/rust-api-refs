using System;
using System.Collections.Generic;
using Facepunch;
using Facepunch.Extend;
using UnityEngine;

public class AirfieldAirdropTerminal : ChargeUpIOEntity
{
	[Header("Airfield Airdrop Terminal")]
	public float airDropTickRateWhenActive = 2f;

	[Tooltip("Only used for reference")]
	public EventSchedule airdropEventPrefab;

	public AirfieldTerminalScreen infoScreen;

	public AirfieldTerminalScreen chargingScreen;

	public AirfieldTerminalScreen detailsScreen;

	public ToggleBlink quarterChargeBlinker;

	public ToggleBlink halfChargeBlinker;

	public ToggleBlink threeQuarterChargeBlinker;

	public ToggleBlink activeBlinker;

	private bool isOn;

	[ServerVar]
	public static void force_charge(ConsoleSystem.Arg arg)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		int num = arg.GetInt(0, 9999999);
		PooledList<AirfieldAirdropTerminal> val = Pool.Get<PooledList<AirfieldAirdropTerminal>>();
		try
		{
			Query.Server.GetInSphere(((Component)ArgEx.Player(arg)).transform.position, 100f, (List<AirfieldAirdropTerminal>)(object)val, Query.DistanceCheckType.Bounds);
			foreach (AirfieldAirdropTerminal item in (List<AirfieldAirdropTerminal>)(object)val)
			{
				item.AddCharge(num);
			}
			arg.ReplyWith($"Force charged {((List<AirfieldAirdropTerminal>)(object)val).Count} AirfieldAirdropTerminals.");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar]
	public static void force_shortcircuit(ConsoleSystem.Arg arg)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		PooledList<AirfieldAirdropTerminal> val = Pool.Get<PooledList<AirfieldAirdropTerminal>>();
		try
		{
			Query.Server.GetInSphere(((Component)ArgEx.Player(arg)).transform.position, 100f, (List<AirfieldAirdropTerminal>)(object)val, Query.DistanceCheckType.Bounds);
			foreach (AirfieldAirdropTerminal item in (List<AirfieldAirdropTerminal>)(object)val)
			{
				if (item.isActivated)
				{
					item.Deactivate();
					item.UpdateChargeFlags();
					arg.ReplyWith($"Deactivated AirfieldAirdropTerminal at {((Component)item).transform.position}.");
				}
				else
				{
					item.AddCharge(-9999f);
					arg.ReplyWith($"Drained charge from AirfieldAirdropTerminal at {((Component)item).transform.position}");
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void SetAirdropEventTickRate(bool on)
	{
		if (isOn != on)
		{
			isOn = on;
			float tickRate = (isOn ? airDropTickRateWhenActive : 1f);
			if (List.FindWith<EventSchedule, string>((IReadOnlyCollection<EventSchedule>)EventSchedule.allEvents, (Func<EventSchedule, string>)((EventSchedule e) => e.Key), airdropEventPrefab.Key, (IEqualityComparer<string>)null) is EventScheduleDynamicTickrate eventScheduleDynamicTickrate)
			{
				eventScheduleDynamicTickrate.tickRate = tickRate;
			}
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		SetAirdropEventTickRate(IsOn());
	}
}
