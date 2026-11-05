using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimalSaleInfoConversation : MonoBehaviour
{
	public RustText AnimalName;

	[Tooltip("The animal's potential, which its price is multiplied by. Hidden for wool.")]
	public RustText PotentialText;

	public Image AnimalIcon;

	public Sprite WoolIcon;

	[Tooltip("One per item an offer can bundle, at most LivestockVendor.MaxOfferItems.")]
	public VirtualItemIcon[] RewardIcons;
}
