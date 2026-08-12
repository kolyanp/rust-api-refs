using Rust.UI;
using UnityEngine;

public class UI_RaidWindowStatus : MonoBehaviour, IClientComponent
{
	public GameObject openObject;

	public GameObject closedObject;

	public RustText openText;

	public RustText closedText;

	public static readonly Phrase ClosesIn;

	public static readonly Phrase OpensIn;

	static UI_RaidWindowStatus()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		ClosesIn = new Phrase("raidwindow.hud.closesin", "Closes in {0}");
		OpensIn = new Phrase("raidwindow.hud.opensin", "Opens in {0}");
	}
}
