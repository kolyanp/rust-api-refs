using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_StoreCart : UI_Window
{
	public static readonly Phrase CartEmptyPhrase = new Phrase("store.cart", "Cart");

	public static readonly Phrase CartPhrase = new Phrase("store.cart.items", "Cart ({0})");

	[SerializeField]
	[Space]
	private StyleAsset emptyStyle;

	[SerializeField]
	private StyleAsset notEmptySyle;

	[SerializeField]
	private RustButton cartButton;

	[SerializeField]
	private Canvas cartButtonCanvas;

	[SerializeField]
	private RustText cartButtonText;

	[SerializeField]
	private RustText itemCountText;

	[SerializeField]
	private RustText totalValueText;

	[SerializeField]
	[Space]
	private RectTransform itemParent;

	[SerializeField]
	private GameObject cartItemPrefab;

	[SerializeField]
	private RustButton checkoutButton;

	[SerializeField]
	private GameObject emptyGroup;

	[SerializeField]
	private GameObject footer;
}
