using Facepunch.Flexbox;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class VendorListing : FacepunchBehaviour
{
	public Image panelBacking;

	[Header("Text")]
	public RustText AvailableText;

	public RustText CostText;

	public RustText AvailableAmount;

	public RustText CostAmount;

	public RustText InStockAmount;

	public RustText InStockText;

	public VendingPriceMultiplierWidget PriceMultiplier;

	public VendingPriceMultiplierWidget RecievedCurrencyMultiplier;

	[Header("Icons")]
	public VirtualItemIcon AvaliableIcon;

	public VirtualItemIcon CostIcon;

	[Header("Tooltips")]
	public Tooltip avaliableIconTooltip;

	public Tooltip costIconTooltip;

	[SerializeField]
	private FlexTransition transition;

	public static Phrase inStockPhrase;

	public static Phrase outOfStockPhrase;

	public static Phrase attachmentsPhrase;

	public static Phrase ammoPhrase;

	static VendorListing()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		inStockPhrase = new Phrase("vendor_in_stock", "In Stock");
		outOfStockPhrase = new Phrase("vendor_out_stock", "Sold Out");
		attachmentsPhrase = new Phrase("vendor_attachments", "Attachments");
		ammoPhrase = new Phrase("vendor_ammo", "Ammo");
	}
}
