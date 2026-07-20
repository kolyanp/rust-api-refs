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
	private UI_StoreTakeover takeovers;

	[SerializeField]
	private RustButton refreshButton;

	[SerializeField]
	private RustInput searchBar;

	public static UI_SteamInventoryCrafting Crafting => Instance?.crafting;
}
