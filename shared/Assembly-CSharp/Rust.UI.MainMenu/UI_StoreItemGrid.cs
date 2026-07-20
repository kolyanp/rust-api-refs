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

	[SerializeField]
	[Tooltip("The source of the items, for analytics")]
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

	[Min(0f)]
	[SerializeField]
	public int cellHeight;

	public bool fixedGrid;

	public List<Vector2Int> fixedSizes = new List<Vector2Int>();

	[SerializeField]
	private bool autoSizing;

	[SerializeField]
	private Vector2 baseItemSize = new Vector2(1f, 1f);

	[SerializeField]
	private Vector2 featuredItemSize;

	[SerializeField]
	private ItemSizeSettings[] sizeOverrides;

	[SerializeField]
	private List<OrderingRule> orderingRules = new List<OrderingRule>();

	[SerializeField]
	private List<SteamInventoryItem> whiteListedItems = new List<SteamInventoryItem>();

	[SerializeField]
	private UI_StoreFakeItemsTakeover fakeAdditionalItems;

	public bool dynamicContent = true;

	[SerializeField]
	private RuleMatchMode ruleMatchMode = RuleMatchMode.Any;

	[SerializeField]
	private List<StoreFilterRule> rules = new List<StoreFilterRule>();

	public FlexGridsElement Grid => grid;

	private UI_Store store => UI_Store.Instance;
}
