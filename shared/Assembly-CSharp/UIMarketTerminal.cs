using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIMarketTerminal : UIDialog, IVendingMachineInterface
{
	public static readonly Phrase PendingDeliveryPluralPhrase = new Phrase("market.pending_delivery.plural", "Waiting for {n} deliveries...");

	public static readonly Phrase PendingDeliverySingularPhrase = new Phrase("market.pending_delivery.singular", "Waiting for delivery...");

	public static readonly Phrase ChargePhrase = new Phrase("market.charge", "POWER {n}%");

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

	public GameObject chargePanel;

	public Image chargeBarFill;

	public RustText chargeLabel;

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
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
	}
}
