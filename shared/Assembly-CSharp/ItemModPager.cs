using UnityEngine;

public class ItemModPager : ItemModRFListener
{
	public static readonly Phrase SilentOffTitle;

	public static readonly Phrase SilentOffDesc;

	public static readonly Phrase SilentOnTitle;

	public static readonly Phrase SilentOnDesc;

	public static readonly Phrase StopTitle;

	public static readonly Phrase StopDesc;

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		base.ServerCommand(item, command, player);
		PagerEntity component = ((Component)ItemModAssociatedEntity<BaseEntity>.GetAssociatedEntity(item)).GetComponent<PagerEntity>();
		if (Object.op_Implicit((Object)(object)component))
		{
			switch (command)
			{
			case "stop":
				component.SetOff();
				break;
			case "silenton":
				component.SetSilentMode(wantsSilent: true);
				break;
			case "silentoff":
				component.SetSilentMode(wantsSilent: false);
				break;
			}
		}
	}

	static ItemModPager()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		SilentOffTitle = new Phrase("silent_off", "Silent OFF");
		SilentOffDesc = new Phrase("silent_off_desc", "Emits a small buzzing sound at short distance");
		SilentOnTitle = new Phrase("silent_on", "Silent ON");
		SilentOnDesc = new Phrase("silent_on_desc", "Emits an audible sound at a distance");
		StopTitle = new Phrase("stop", "Stop");
		StopDesc = new Phrase("stop_desc", "Stop the alert");
	}
}
