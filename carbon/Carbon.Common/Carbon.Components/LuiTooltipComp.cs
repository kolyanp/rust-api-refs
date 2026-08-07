namespace Carbon.Components;

public class LuiTooltipComp : LuiCompBase
{
	public string tooltipType;

	public string offset;

	public bool useCentre;

	public string text;

	public string delay;

	public string position;

	public LuiTooltipComp()
	{
		type = LuiCompType.Tooltip;
	}
}
