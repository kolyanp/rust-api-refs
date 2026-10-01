using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimalSaleInfoConversation : MonoBehaviour
{
	public RustText AnimalName;

	public Image AnimalIcon;

	public Sprite WoolIcon;

	[Tooltip("One per item an offer can bundle, at most LivestockVendor.MaxOfferItems.")]
	public VirtualItemIcon[] RewardIcons;
}
