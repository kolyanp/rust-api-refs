using UnityEngine;
using UnityEngine.UI;

public class VirtualItemIcon : MonoBehaviour
{
	public ItemDefinition itemDef;

	public int itemAmount;

	public bool asBlueprint;

	public Image iconImage;

	public Image bpUnderlay;

	public Text amountText;

	public Text hoverText;

	public CanvasGroup iconContents;

	public Tooltip ToolTip;

	[Space]
	public CanvasGroup conditionObject;

	public Image conditionFill;

	public Image maxConditionFill;

	public Image cornerIcon;

	[Space]
	public Sprite emptySlotBackgroundSprite;

	public Image backgroundUnderlayImage;

	[Header("Slots")]
	public GameObject slots;

	public Image[] slotImages;

	public static Phrase attachmentsPhrase;

	public static Phrase ammoPhrase;

	static VirtualItemIcon()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		attachmentsPhrase = new Phrase("vendor_attachments", "Attachments");
		ammoPhrase = new Phrase("vendor_ammo", "Ammo");
	}
}
