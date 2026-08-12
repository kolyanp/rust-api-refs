using UnityEngine;

public class BeehiveLootPanel : LootPanel
{
	[Header("Info Bars")]
	public InfoBar Indoors;

	public InfoBar Humidity;

	public InfoBar Temperature;

	public InfoBar Overall;

	[Header("Grids")]
	public LootGrid LootGrid_Input;

	public LootGrid LootGrid_Output;

	[Header("Status")]
	public StatusPanel status;

	public static readonly Phrase YesIndoors;

	public static readonly Phrase NoIndoors;

	static BeehiveLootPanel()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		YesIndoors = new Phrase("beehive.indoors.yes", "YES");
		NoIndoors = new Phrase("beehive.indoors.no", "NO");
	}
}
