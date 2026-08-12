using Rust.UI;
using UnityEngine;

public class UIMarketTerminal : UIDialog, IVendingMachineInterface
{
	public static readonly Phrase PendingDeliveryPluralPhrase;

	public static readonly Phrase PendingDeliverySingularPhrase;

	public Canvas canvas;

	public MapView mapView;

	public RectTransform shopDetailsPanel;

	public float shopDetailsMargin = 16f;

	public float easeDuration = 0.2f;

	public LeanTweenType easeType = LeanTweenType.linear;

	public TmProEmojiRedirector shopName;

	public GameObject shopOrderingPanel;

	public RectTransform sellOrderContainer;

	public GameObjectRef sellOrderPrefab;

	public VirtualItemIcon deliveryFeeIcon;

	public GameObject deliveryFeeCantAffordIndicator;

	public GameObject inventoryFullIndicator;

	public GameObject notEligiblePanel;

	public GameObject pendingDeliveryPanel;

	public RustText pendingDeliveryLabel;

	public RectTransform itemNoticesContainer;

	public GameObjectRef itemRemovedPrefab;

	public GameObjectRef itemPendingPrefab;

	public GameObjectRef itemAddedPrefab;

	public CanvasGroup gettingStartedTip;

	public SoundDefinition buyItemSoundDef;

	public SoundDefinition buttonPressSoundDef;

	static UIMarketTerminal()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		PendingDeliveryPluralPhrase = new Phrase("market.pending_delivery.plural", "Waiting for {n} deliveries...");
		PendingDeliverySingularPhrase = new Phrase("market.pending_delivery.singular", "Waiting for delivery...");
	}
}
