using System.Linq;
using Facepunch.Extend;
using Rust;
using Rust.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VehicleListTools : MonoBehaviour
{
	public GameObject categoryButton;

	public GameObject vehicleButtonPrefab;

	public Transform vehicleButtonParent;

	public RustInput searchInputText;

	internal Button lastCategory;

	public ScrollRect MainScrollRect;

	private IOrderedEnumerable<ItemDefinition> currentItems;

	private IOrderedEnumerable<ItemDefinition> allItems;

	public void OnPanelOpened()
	{
		CacheAllItems();
		Refresh();
		searchInputText.InputField.ActivateInputField();
	}

	private void OnOpenDevTools()
	{
		searchInputText.InputField.ActivateInputField();
	}

	private void CacheAllItems()
	{
		if (allItems == null)
		{
			allItems = from x in ItemManager.GetItemDefinitions()
				where x.vehicleItem
				orderby x.displayName.translated
				select x;
		}
	}

	public void Refresh()
	{
		RebuildCategories();
	}

	private void RebuildCategories()
	{
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		for (int i = 0; i < categoryButton.transform.parent.childCount; i++)
		{
			Transform child = categoryButton.transform.parent.GetChild(i);
			if (!((Object)(object)child == (Object)(object)categoryButton.transform))
			{
				GameManager.Destroy(((Component)child).gameObject);
			}
		}
		categoryButton.SetActive(true);
		foreach (IGrouping<VehicleCategory, ItemDefinition> item in from x in ItemManager.GetItemDefinitions()
			group x by x.vehicleCategory into x
			orderby x.First().vehicleCategory
			select x)
		{
			GameObject val = Object.Instantiate<GameObject>(categoryButton);
			val.transform.SetParent(categoryButton.transform.parent, false);
			((TMP_Text)val.GetComponentInChildren<TextMeshProUGUI>()).text = item.First().vehicleCategory.ToString();
			Button btn = val.GetComponentInChildren<Button>();
			ItemDefinition[] itemArray = item.ToArray();
			((UnityEvent)btn.onClick).AddListener((UnityAction)delegate
			{
				if (Object.op_Implicit((Object)(object)lastCategory))
				{
					((Selectable)lastCategory).interactable = true;
				}
				lastCategory = btn;
				((Selectable)lastCategory).interactable = false;
				SwitchItemCategory(itemArray);
			});
			if ((Object)(object)lastCategory == (Object)null)
			{
				lastCategory = btn;
				((Selectable)lastCategory).interactable = false;
				SwitchItemCategory(itemArray);
			}
		}
		categoryButton.SetActive(false);
	}

	private void SwitchItemCategory(ItemDefinition[] defs)
	{
		currentItems = defs.OrderBy((ItemDefinition x) => x.displayName.translated);
		searchInputText.Text = "";
		FilterItems(null);
		MainScrollRect.verticalNormalizedPosition = 1f;
	}

	[UnityEvent]
	public void FilterItems(string searchText)
	{
		if ((Object)(object)vehicleButtonPrefab == (Object)null)
		{
			return;
		}
		vehicleButtonParent.DestroyAllChildren();
		bool flag = !string.IsNullOrEmpty(searchText);
		string value = (flag ? searchText.ToLower() : null);
		IOrderedEnumerable<ItemDefinition> obj = (flag ? allItems : currentItems);
		int num = 0;
		foreach (ItemDefinition item in obj)
		{
			if (item.vehicleItem && (!flag || item.shortname.ToLower().Contains(value)))
			{
				GameObject obj2 = Object.Instantiate<GameObject>(vehicleButtonPrefab, vehicleButtonParent);
				VehicleButtonTools componentInChildren = obj2.GetComponentInChildren<VehicleButtonTools>();
				componentInChildren.itemDef = item;
				componentInChildren.MainScroll = MainScrollRect;
				componentInChildren.image.sprite = item.iconSprite;
				((Behaviour)componentInChildren.backgroundImage).enabled = !item.IsAllowed((EraRestriction)0);
				((TMP_Text)obj2.GetComponentInChildren<TextMeshProUGUI>()).text = item.displayName.translated;
				num++;
				if (num >= 160)
				{
					break;
				}
			}
		}
	}
}
