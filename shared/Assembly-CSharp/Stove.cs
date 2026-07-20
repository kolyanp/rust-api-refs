public class Stove : BaseOven
{
	public override string TitleItemShortname => "stove";

	public override bool ShowFuelInLootPanel => false;

	public override bool CanRunWithNoFuel
	{
		protected get
		{
			return true;
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		UpdateAutoState();
	}

	public override void OnItemAddedOrRemoved(Item item, bool bAdded)
	{
		base.OnItemAddedOrRemoved(item, bAdded);
		UpdateAutoState();
	}

	private void UpdateAutoState()
	{
		bool flag = HasInputItems();
		if (flag && !IsOn())
		{
			StartCooking();
		}
		else if (!flag && IsOn())
		{
			StopCooking();
			SendNetworkUpdate();
		}
	}
}
