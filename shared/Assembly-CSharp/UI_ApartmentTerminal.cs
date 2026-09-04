using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

public class UI_ApartmentTerminal : UI_Window
{
	[SerializeField]
	private RectTransform plotList;

	[SerializeField]
	private GameObjectRef plotRowPrefab;

	[Space]
	[SerializeField]
	private RustText timeText;

	[SerializeField]
	private RustText availableText;

	[SerializeField]
	private RustText occupiedText;

	[Header("Opening")]
	[Space]
	[SerializeField]
	private RectTransform crtScreen;

	[SerializeField]
	private GameObject splashScreen;

	[SerializeField]
	private GameObject mainMenu;

	[SerializeField]
	private RustText bootLogText;

	[SerializeField]
	private RustText subtitleText;

	[Space]
	[Header("CCTV")]
	[SerializeField]
	private RawImage feedImage;

	[SerializeField]
	private Camera feedCamera;

	[SerializeField]
	private GameObject feedNoSignal;

	[SerializeField]
	[Header("Tabs")]
	[Space]
	private GameObject apartmentsPanel;

	[SerializeField]
	private GameObject shopsPanel;

	[SerializeField]
	[Space]
	[Header("Shops")]
	private RectTransform shopList;

	[SerializeField]
	private GameObjectRef shopRowPrefab;

	[SerializeField]
	private RustText shopsAvailableText;

	[SerializeField]
	private RustText shopsOccupiedText;

	private static readonly string[] BootLines;

	private static readonly Phrase occupiedPhrase;

	private static readonly Phrase availablePhrase;

	private static readonly Phrase supplierPhrase;

	private static readonly Phrase apartmentsTabPhrase;

	private static readonly Phrase shopsTabPhrase;

	private static readonly Phrase shopNumberPhrase;

	static UI_ApartmentTerminal()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		BootLines = new string[9] { "APRT-OS v1.4  (C) COBALT SYSTEMS", "", "> POST .................. OK", "> MEM CHECK 640K ........ OK", "> TENANT REGISTRY ....... MOUNTED", "> NET LINK .............. ESTABLISHED", "> AUTHENTICATING ........ OK", "", "READY." };
		occupiedPhrase = new Phrase("apartment.occupied-plots", "{0} Occupied");
		availablePhrase = new Phrase("apartment.available-plots", "{0} Available");
		supplierPhrase = new Phrase("apartment.terminal.supplier", "[SUPPLIER OF AFFORDABLE LIVING SPACES]");
		apartmentsTabPhrase = new Phrase("apartment.terminal.tab.apartments", "Apartments");
		shopsTabPhrase = new Phrase("apartment.terminal.tab.shops", "Shops");
		shopNumberPhrase = new Phrase("apartment.shop.number", "Shop {0}");
	}
}
