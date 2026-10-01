using System.Collections.Generic;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class SelectedItem : SingletonComponent<SelectedItem>, IInventoryChanged
{
	public static readonly Phrase DropTitle = new Phrase("drop", "Drop");

	public static readonly Phrase DropDesc = new Phrase("drop_desc", "");

	public static readonly Phrase ChangeAccessoryTitle = new Phrase("changeAccessory", "Change Accessory");

	public static readonly Phrase ChangeAccessoryDesc = new Phrase("changeAccessoryDesc", "");

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
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected Obj, but got Unknown
	}
}
