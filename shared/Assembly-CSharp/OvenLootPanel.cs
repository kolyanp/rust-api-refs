using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class OvenLootPanel : MonoBehaviour
{
	public GameObject controlsOn;

	public GameObject controlsOff;

	public Image TitleBackground;

	public RustText TitleText;

	public Color AlertBackgroundColor;

	public Color AlertTextColor;

	public Color OffBackgroundColor;

	public Color OffTextColor;

	public Color OnBackgroundColor;

	public Color OnTextColor;

	private Phrase OffPhrase;

	private Phrase OnPhrase;

	private Phrase NoFuelPhrase;

	public GameObject FuelRowPrefab;

	public GameObject MaterialRowPrefab;

	public GameObject ItemRowPrefab;

	public LootGrid LootGrid_Wood;

	public LootGrid LootGrid_Input;

	public LootGrid LootGrid_Output;

	public GameObject Contents;

	public GameObject[] ElectricDisableRoots;

	public OvenLootPanel()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		OffPhrase = new Phrase("off", "off");
		OnPhrase = new Phrase("on", "on");
		NoFuelPhrase = new Phrase("no_fuel", "No Fuel");
		ElectricDisableRoots = (GameObject[])(object)new GameObject[0];
		((MonoBehaviour)this)._002Ector();
	}
}
