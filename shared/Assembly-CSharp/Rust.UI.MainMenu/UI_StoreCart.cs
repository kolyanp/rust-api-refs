using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_StoreCart : UI_Window
{
	public static readonly Phrase CartEmptyPhrase;

	public static readonly Phrase CartPhrase;

	[Space]
	[SerializeField]
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

	[Space]
	[SerializeField]
	private RectTransform itemParent;

	[SerializeField]
	private GameObject cartItemPrefab;

	[SerializeField]
	private RustButton checkoutButton;

	[SerializeField]
	private GameObject emptyGroup;

	[SerializeField]
	private GameObject footer;

	static UI_StoreCart()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		CartEmptyPhrase = new Phrase("store.cart", "Cart");
		CartPhrase = new Phrase("store.cart.items", "Cart ({0})");
	}
}
