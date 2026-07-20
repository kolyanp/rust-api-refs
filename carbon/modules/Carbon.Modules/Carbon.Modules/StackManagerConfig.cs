using System.Collections.Generic;

namespace Carbon.Modules;

public class StackManagerConfig
{
	public float GlobalMultiplier = 1f;

	public float GlobalItemsMultiplier = 1f;

	public bool ProhibitItemContainerStacking;

	public bool ProhibitItemConsumableContainerStacking = true;

	public bool ProhibitItemFishableStacking;

	public Dictionary<ItemCategory, float> Categories = new Dictionary<ItemCategory, float>
	{
		{
			(ItemCategory)8,
			1f
		},
		{
			(ItemCategory)4,
			1f
		},
		{
			(ItemCategory)13,
			1f
		},
		{
			(ItemCategory)1,
			1f
		},
		{
			(ItemCategory)16,
			1f
		},
		{
			(ItemCategory)7,
			1f
		},
		{
			(ItemCategory)17,
			1f
		},
		{
			(ItemCategory)2,
			1f
		},
		{
			(ItemCategory)6,
			1f
		},
		{
			(ItemCategory)10,
			1f
		},
		{
			(ItemCategory)3,
			1f
		},
		{
			(ItemCategory)5,
			1f
		},
		{
			(ItemCategory)9,
			1f
		},
		{
			(ItemCategory)0,
			1f
		}
	};

	public Dictionary<string, float> Items = new Dictionary<string, float>();

	public HashSet<string> Blacklist = new HashSet<string>();
}
