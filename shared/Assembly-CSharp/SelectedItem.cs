using System.Collections.Generic;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class SelectedItem : SingletonComponent<SelectedItem>, IInventoryChanged
{
	public static readonly Phrase DropTitle;

	public static readonly Phrase DropDesc;

	public static readonly Phrase ChangeAccessoryTitle;

	public static readonly Phrase ChangeAccessoryDesc;

	public Image icon;

	public Image iconSplitter;

	public RustText title;

	public RustText description;

	public GameObject splitPanel;

	public GameObject itemProtection;

	public GameObject OwnershipContainer;

	public ItemOwnershipPanel OwnershipItem;

	private List<ItemOwnershipPanel> ownershipPanels = new List<ItemOwnershipPanel>();

	public GameObject menuOption;

	public GameObject optionsParent;

	public GameObject innerPanelContainer;

	static SelectedItem()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		DropTitle = new Phrase("drop", "Drop");
		DropDesc = new Phrase("drop_desc", "");
		ChangeAccessoryTitle = new Phrase("changeAccessory", "Change Accessory");
		ChangeAccessoryDesc = new Phrase("changeAccessoryDesc", "");
	}
}
