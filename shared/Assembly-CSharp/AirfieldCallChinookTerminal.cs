using System;
using System.Collections.Generic;
using System.Linq;
using Facepunch;
using Facepunch.Extend;
using UnityEngine;

public class AirfieldCallChinookTerminal : ChargeUpIOEntity
{
	[Tooltip("Only used for reference")]
	[Header("Airfield Call Chinook Terminal")]
	public EventSchedule chinookEventPrefab;

	public CH47DropZone associatedDropZone;

	public AirfieldTerminalScreen infoScreen;

	public AirfieldTerminalScreen chargingScreen;

	public ToggleBlink quarterChargeBlinker;

	public ToggleBlink halfChargeBlinker;

	public ToggleBlink threeQuarterChargeBlinker;

	public ToggleBlink activeBlinker;

	public static bool isActive;

	public static CH47HelicopterAIController selectedChinook;

	public static CH47DropZone selectedChinookDropZone;

	[ServerVar]
	public static void force_charge(ConsoleSystem.Arg arg)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		int num = arg.GetInt(0, 9999999);
		PooledList<AirfieldCallChinookTerminal> val = Pool.Get<PooledList<AirfieldCallChinookTerminal>>();
		try
		{
			Query.Server.GetInSphere(((Component)ArgEx.Player(arg)).transform.position, 100f, (List<AirfieldCallChinookTerminal>)(object)val, Query.DistanceCheckType.Bounds);
			foreach (AirfieldCallChinookTerminal item in (List<AirfieldCallChinookTerminal>)(object)val)
			{
				item.AddCharge(num);
			}
			arg.ReplyWith($"Force charged {((List<AirfieldCallChinookTerminal>)(object)val).Count} AirfieldCallChinookTerminals.");
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
		PooledList<AirfieldCallChinookTerminal> val = Pool.Get<PooledList<AirfieldCallChinookTerminal>>();
		try
		{
			Query.Server.GetInSphere(((Component)ArgEx.Player(arg)).transform.position, 100f, (List<AirfieldCallChinookTerminal>)(object)val, Query.DistanceCheckType.Bounds);
			foreach (AirfieldCallChinookTerminal item in (List<AirfieldCallChinookTerminal>)(object)val)
			{
				if (item.isActivated)
				{
					item.Deactivate();
					item.UpdateChargeFlags();
					arg.ReplyWith($"Deactivated AirfieldCallChinookTerminal at {((Component)item).transform.position}.");
				}
				else
				{
					item.AddCharge(-9999f);
					arg.ReplyWith($"Drained charge from AirfieldCallChinookTerminal at {((Component)item).transform.position}");
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override void Activate()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.Activate();
		isActive = true;
		selectedChinook = null;
		selectedChinookDropZone = (((Object)(object)associatedDropZone != (Object)null) ? associatedDropZone : CH47DropZone.GetClosest(((Component)this).transform.position));
		if (CH47HelicopterAIController.activeScientistCH47s.Count == 0 || CH47HelicopterAIController.activeScientistCH47s.All((CH47HelicopterAIController c) => !c.CanDropCrate()))
		{
			TriggerChinookEvent();
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		isActive = false;
		selectedChinook = null;
		selectedChinookDropZone = null;
	}

	private void TriggerChinookEvent()
	{
		EventSchedule eventSchedule = List.FindWith<EventSchedule, string>((IReadOnlyCollection<EventSchedule>)EventSchedule.enabledEvents, (Func<EventSchedule, string>)((EventSchedule e) => e.Key), chinookEventPrefab.Key, (IEqualityComparer<string>)null);
		if ((Object)(object)eventSchedule != (Object)null)
		{
			eventSchedule.Trigger();
		}
	}
}
