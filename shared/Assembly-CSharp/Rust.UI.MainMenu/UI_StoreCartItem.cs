using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_StoreCartItem : MonoBehaviour
{
	public RustButton closeButton;

	public StoreSource source;

	[SerializeField]
	private GameObject duplicateGroup;

	[SerializeField]
	private RustText duplicateText;

	private static readonly Phrase alreadyInCartPhrase = new Phrase("store.cart.already_in_cart", "Already in your cart: {0}");

	private IPlayerItemDefinition _item;

	static UI_StoreCartItem()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
	}
}
