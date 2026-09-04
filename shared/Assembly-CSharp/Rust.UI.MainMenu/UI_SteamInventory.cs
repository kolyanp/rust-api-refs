using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_SteamInventory : UI_Page
{
	public static UI_SteamInventory Instance;

	[SerializeField]
	private FlexVirtualScroll virtualScrollFlex;

	[Space]
	[SerializeField]
	private UI_SteamInventoryCrafting crafting;

	[SerializeField]
	private UI_SteamInventoryItem inventoryItemPrefab;

	[SerializeField]
	private RectTransform inventoryItemParent;

	[Space]
	public GameObject loadingOverlay;

	[SerializeField]
	private GameObject noConnectionOverlay;

	[SerializeField]
	private UI_StoreTakeover takeovers;

	[SerializeField]
	private RustButton refreshButton;

	[SerializeField]
	private RustInput searchBar;

	private static readonly Phrase inventoryRetryPhrase;

	private static readonly Phrase inventoryConnectionIssuePhrase;

	public static UI_SteamInventoryCrafting Crafting => Instance?.crafting;

	static UI_SteamInventory()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		inventoryRetryPhrase = new Phrase("inventory.retry", "Retry");
		inventoryConnectionIssuePhrase = new Phrase("inventory.connection_issue", "Your Steam inventory failed to load. Steam might be down?");
	}
}
