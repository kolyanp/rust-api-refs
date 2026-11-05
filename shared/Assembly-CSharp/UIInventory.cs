using Development.Attributes;
using Rust.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ResetStaticFields]
public class UIInventory : SingletonComponent<UIInventory>
{
	public TextMeshProUGUI PlayerName;

	public static bool isOpen;

	public static float LastOpened;

	public VerticalLayoutGroup rightContents;

	public GameObject QuickCraft;

	public Transform InventoryIconContainer;

	public RustButton ChangelogPanelButton;

	public ContactsPanel contactsPanel;

	public GameObject backpackSlot;

	public UnderwearSkinPicker underwearSkinPicker;
}
