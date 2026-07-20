using Rust.UI;
using UnityEngine;

public class UI_RaidWindowStatus : MonoBehaviour, IClientComponent
{
	public GameObject openObject;

	public GameObject closedObject;

	public RustText openText;

	public RustText closedText;

	public static readonly Phrase ClosesIn = new Phrase("raidwindow.hud.closesin", "Closes in {0}");

	public static readonly Phrase OpensIn = new Phrase("raidwindow.hud.opensin", "Opens in {0}");
}
