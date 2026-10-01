using UnityEngine;
using UnityEngine.UI;

public class LootPanelReclaim : LootPanel
{
	public int oldOverflow = -1;

	public Text overflowText;

	public GameObject overflowObject;

	public static readonly Phrase MorePhrase = new Phrase("reclaim.more", "additional items...");

	static LootPanelReclaim()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
	}
}
