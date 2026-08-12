using Rust.UI;
using UnityEngine;

public class LootPanelRecycler : LootPanel
{
	public GameObject controlsDisabled;

	public GameObject controlsOff;

	public GameObject controlsOn;

	public RustText recycler_stats;

	public Color goodStatsColor;

	public Color badStatsColor;

	public LootPanelRecycler()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		goodStatsColor = new Color(0.584712f, 0.75f, 0.2922794f);
		badStatsColor = new Color(41f / 51f, 0.254902f, 0.1686275f);
		base._002Ector();
	}
}
