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

	protected override bool AutomaticallyStartCooking => true;
}
