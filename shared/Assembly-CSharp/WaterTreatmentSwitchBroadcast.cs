using System;

public class WaterTreatmentSwitchBroadcast : IOEntity
{
	public static bool WaterTreatmentSwitchEnabled;

	private static ListHashSet<IWaterTreatmentSwitchReceiver> receivers = new ListHashSet<IWaterTreatmentSwitchReceiver>();

	private static ListHashSet<WaterTreatmentSwitchBroadcast> activeSwitches = new ListHashSet<WaterTreatmentSwitchBroadcast>();

	private static bool? debugForcedState;

	public override void ResetIOState()
	{
		base.ResetIOState();
		SetActive(active: false);
	}

	[ServerVar(Help = "Force the water treatment switch broadcast on/off for testing. Pass no argument (or 'clear') to release the override and return to power-derived state.")]
	public static void SendDebugBroadcast(ConsoleSystem.Arg arg)
	{
		if (!arg.HasArgs() || arg.GetString(0) == "clear")
		{
			debugForcedState = null;
			RefreshEnabledState();
			arg.ReplyWith($"Cleared water treatment switch debug override. Enabled: {WaterTreatmentSwitchEnabled}");
		}
		else
		{
			debugForcedState = arg.GetBool(0, def: true);
			RefreshEnabledState();
			arg.ReplyWith($"Forced water treatment switch broadcast to: {WaterTreatmentSwitchEnabled} (override active)");
		}
	}

	[ServerVar]
	public static void get_num_listeners(ConsoleSystem.Arg arg)
	{
		arg.ReplyWith($"Number of listeners: {receivers.Count}");
	}

	public static void RegisterReceiver(IWaterTreatmentSwitchReceiver r)
	{
		receivers.TryAdd(r);
	}

	public static void DeregisterReceiver(IWaterTreatmentSwitchReceiver r)
	{
		receivers.Remove(r);
	}

	public override bool GetHasPower(int inputAmount, int inputSlot)
	{
		if (inputSlot != 0)
		{
			return false;
		}
		bool hasPower = base.GetHasPower(inputAmount, inputSlot);
		SetActive(hasPower);
		return hasPower;
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		SetActive(active: false);
	}

	private void SetActive(bool active)
	{
		if (active ? activeSwitches.TryAdd(this) : activeSwitches.Remove(this))
		{
			RefreshEnabledState();
		}
	}

	private static void RefreshEnabledState()
	{
		bool flag = debugForcedState ?? (activeSwitches.Count > 0);
		if (flag != WaterTreatmentSwitchEnabled)
		{
			WaterTreatmentSwitchEnabled = flag;
			SendBroadcast(flag);
		}
	}

	private static void SendBroadcast(bool toggle)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<IWaterTreatmentSwitchReceiver> enumerator = receivers.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.OnWaterTreatmentSwitchToggled(toggle);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
