using System;
using System.Collections.Generic;
using Facepunch.Flexbox;
using UnityEngine;

namespace Rust.UI.MainMenu;

[RequireComponent(typeof(FlexGridsElement))]
public class UI_StoreItemGrid : MonoBehaviour
{
	public enum OrderingRule
	{
		TakeoverOrder,
		WhitelistOrder,
		OwnedLast,
		OwnedFirst,
		PriceLowToHigh,
		PriceHighToLow,
		Alphabetical,
		ReverseAlphabetical,
		FeaturedFirst,
		LargestFirst,
		Random,
		FeaturedLast,
		FakeItemsOrder,
		FeaturingOrder
	}

	public enum RuleMatchMode
	{
		All,
		Any
	}

	public enum FilterRule
	{
		TagInclude,
		TagExclude,
		OnlyFeatured,
		ExcludeFeatured,
		NeedTakeOver,
		ItemShortName,
		ExcludeOwned
	}

	[Serializable]
	public class StoreFilterRule
	{
		public bool enabled = true;

		public FilterRule ruleType;

		public List<string> tags = new List<string>();

		public List<string> itemShortNames = new List<string>();
	}

	[Serializable]
	public struct ItemSizeSettings
	{
		public SteamInventoryItem Item;

		public int ItemID;

		[Range(1f, 12f)]
		public int SizeX;

		[Range(1f, 5f)]
		public int SizeY;

		public int GetItemID
		{
			get
			{
				if (!((Object)(object)Item != (Object)null))
				{
					return ItemID;
				}
				return Item.id;
			}
		}
	}

	[SerializeField]
	private FlexGridsElement grid;

	[Tooltip("The source of the items, for analytics")]
	[SerializeField]
	private StoreSource source;

	[Space]
	[SerializeField]
	private UI_StoreItemTile skinItemTilePrefab;

	[SerializeField]
	private UI_StoreItemTile featuredSkinItemTilePrefab;

	[SerializeField]
	private int maxCellCount;

	[SerializeField]
	[Min(0f)]
	public int cellWidth;

	[SerializeField]
	[Min(0f)]
	public int cellHeight;

	public bool fixedGrid;

	public List<Vector2Int> fixedSizes;

	[SerializeField]
	private bool autoSizing;

	[SerializeField]
	private Vector2 baseItemSize;

	[SerializeField]
	private Vector2 featuredItemSize;

	[SerializeField]
	private ItemSizeSettings[] sizeOverrides;

	[SerializeField]
	private List<OrderingRule> orderingRules;

	[SerializeField]
	private List<SteamInventoryItem> whiteListedItems;

	[SerializeField]
	private UI_StoreFakeItemsTakeover fakeAdditionalItems;

	public bool dynamicContent;

	[Tooltip("Items already spawned by these grids won't spawn here again, avoids duplicates across grids")]
	[SerializeField]
	private List<UI_StoreItemGrid> excludeItemsFromGrids;

	[SerializeField]
	private RuleMatchMode ruleMatchMode;

	[SerializeField]
	private List<StoreFilterRule> rules;

	public FlexGridsElement Grid => grid;

	private UI_Store store => UI_Store.Instance;

	public UI_StoreItemGrid()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		fixedSizes = new List<Vector2Int>();
		baseItemSize = new Vector2(1f, 1f);
		orderingRules = new List<OrderingRule>();
		whiteListedItems = new List<SteamInventoryItem>();
		dynamicContent = true;
		excludeItemsFromGrids = new List<UI_StoreItemGrid>();
		ruleMatchMode = RuleMatchMode.Any;
		rules = new List<StoreFilterRule>();
		((MonoBehaviour)this)._002Ector();
	}
}
