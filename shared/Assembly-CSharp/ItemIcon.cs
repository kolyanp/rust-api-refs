using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemIcon : BaseMonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IDraggable, IPreInventoryChanged, IItemAmountChanged, IItemIconChanged
{
	private Color backgroundColor;

	public Color selectedBackgroundColor;

	public float unoccupiedAlpha;

	public Color unoccupiedColor;

	public Color conditionFillColor;

	public Color refrigeratedConditionFillColor;

	public ItemContainerSource containerSource;

	public int slotOffset;

	[Range(0f, 64f)]
	public int slot;

	public bool setSlotFromSiblingIndex;

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

	public bool allowSelection;

	public bool allowDropping;

	public bool allowMove;

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
		selectedBackgroundColor = new Color(0.12156863f, 0.41960785f, 32f / 51f, 40f / 51f);
		unoccupiedAlpha = 1f;
		conditionFillColor = new Color(23f / 51f, 47f / 85f, 23f / 85f, 1f);
		refrigeratedConditionFillColor = new Color(0.12156863f, 44f / 85f, 32f / 51f, 1f);
		setSlotFromSiblingIndex = true;
		allowSelection = true;
		allowDropping = true;
		allowMove = true;
		base._002Ector();
	}
}
