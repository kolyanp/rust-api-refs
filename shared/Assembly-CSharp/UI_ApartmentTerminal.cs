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

	[SerializeField]
	[Header("Opening")]
	[Space]
	private RectTransform crtScreen;

	[SerializeField]
	private GameObject splashScreen;

	[SerializeField]
	private GameObject mainMenu;

	[SerializeField]
	private RustText bootLogText;

	[SerializeField]
	private RustText subtitleText;

	[SerializeField]
	[Header("CCTV")]
	[Space]
	private RawImage feedImage;

	[SerializeField]
	private Camera feedCamera;

	[SerializeField]
	private GameObject feedNoSignal;

	[Header("Tabs")]
	[SerializeField]
	[Space]
	private GameObject apartmentsPanel;

	[SerializeField]
	private GameObject shopsPanel;

	[Space]
	[Header("Shops")]
	[SerializeField]
	private RectTransform shopList;

	[SerializeField]
	private GameObjectRef shopRowPrefab;

	[SerializeField]
	private RustText shopsAvailableText;

	[SerializeField]
	private RustText shopsOccupiedText;

	private static readonly string[] BootLines = new string[9] { "APRT-OS v1.4  (C) COBALT SYSTEMS", "", "> POST .................. OK", "> MEM CHECK 640K ........ OK", "> TENANT REGISTRY ....... MOUNTED", "> NET LINK .............. ESTABLISHED", "> AUTHENTICATING ........ OK", "", "READY." };

	private static readonly Phrase occupiedPhrase = new Phrase("apartment.occupied-plots", "{0} Occupied");

	private static readonly Phrase availablePhrase = new Phrase("apartment.available-plots", "{0} Available");

	private static readonly Phrase supplierPhrase = new Phrase("apartment.terminal.supplier", "[SUPPLIER OF AFFORDABLE LIVING SPACES]");

	private static readonly Phrase apartmentsTabPhrase = new Phrase("apartment.terminal.tab.apartments", "Apartments");

	private static readonly Phrase shopsTabPhrase = new Phrase("apartment.terminal.tab.shops", "Shops");

	private static readonly Phrase shopNumberPhrase = new Phrase("apartment.shop.number", "Shop {0}");
}
