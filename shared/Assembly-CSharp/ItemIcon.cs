using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemIcon : BaseMonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IDraggable, IPreInventoryChanged, IItemAmountChanged, IItemIconChanged
{
	private Color backgroundColor;

	public Color selectedBackgroundColor = new Color(31f / 255f, 107f / 255f, 160f / 255f, 200f / 255f);

	public float unoccupiedAlpha = 1f;

	public Color unoccupiedColor;

	public Color conditionFillColor = new Color(115f / 255f, 141f / 255f, 69f / 255f, 1f);

	public Color refrigeratedConditionFillColor = new Color(31f / 255f, 132f / 255f, 160f / 255f, 1f);

	public ItemContainerSource containerSource;

	public int slotOffset;

	[Range(0f, 64f)]
	public int slot;

	public bool setSlotFromSiblingIndex = true;

	public bool suppressCookingIcon;

	public GameObject slots;

	public CanvasGroup iconContents;

	public CanvasGroup canvasGroup;

	public Image iconImage;

	public Image underlayImage;

	public Text amountText;

	public Text hoverText;

	public Image hoverOutline;

	public Image cornerIcon;

	public Image lockedImage;

	public Image progressImage;

	public Image backgroundImage;

	public Image backgroundUnderlayImage;

	public Image progressPanel;

	public Sprite emptySlotBackgroundSprite;

	public CanvasGroup conditionObject;

	public Image conditionFill;

	public Image maxConditionFill;

	public GameObject lightEnabled;

	public GameObject burstEnabled;

	public AmmoIconIndicator ammoIconIndicator;

	public bool allowSelection = true;

	public bool allowDropping = true;

	public bool allowMove = true;

	public bool showCountDropShadow;

	[NonSerialized]
	public Item item;

	[NonSerialized]
	public bool invalidSlot;

	public SoundDefinition hoverSound;

	public virtual void OnPointerClick(PointerEventData eventData)
	{
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	public ItemIcon()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
	}
}
