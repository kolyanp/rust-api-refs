using System;
using System.Collections.Generic;
using System.Linq;
using Carbon.Base;
using Carbon.Extensions;
using Oxide.Core;
using UnityEngine;

namespace Carbon.Modules;

public class StackManagerModule : CarbonModule<StackManagerConfig, StackManagerData>
{
	public override string Name => "StackManager";

	public override bool ForceModded => true;

	public override VersionNumber Version => new VersionNumber(1, 0, 0);

	public override Type Type => typeof(StackManagerModule);

	public override bool EnabledByDefault => false;

	public override void OnEnabled(bool initialized)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnabled(initialized);
		if (!initialized || ItemManager.itemList == null)
		{
			return;
		}
		bool flag = false;
		foreach (ItemDefinition item in ItemManager.itemList.Where((ItemDefinition item) => !base.DataInstance.Items.ContainsKey(item.shortname)))
		{
			base.DataInstance.Items.Add(item.shortname, item.stackable);
			flag = true;
		}
		if (flag)
		{
			((BaseModule)this).Save();
		}
		foreach (KeyValuePair<ItemCategory, float> category in base.ConfigInstance.Categories)
		{
			foreach (ItemDefinition item2 in ItemManager.itemList)
			{
				if (!IsBypassed(item2) && item2.category == category.Key && !base.ConfigInstance.Blacklist.Contains(item2.shortname) && !base.ConfigInstance.Items.ContainsKey(item2.shortname))
				{
					base.DataInstance.Items.TryGetValue(item2.shortname, out var value);
					if (value > 0)
					{
						item2.stackable = Mathf.Clamp((int)((float)value * category.Value * base.ConfigInstance.GlobalMultiplier), 1, int.MaxValue);
					}
				}
			}
		}
		foreach (ItemDefinition item3 in ItemManager.itemList)
		{
			if (!IsBypassed(item3) && base.ConfigInstance.Items.ContainsKey(item3.shortname))
			{
				float num = base.ConfigInstance.Items[item3.shortname];
				base.DataInstance.Items.TryGetValue(item3.shortname, out var value2);
				if (value2 > 0)
				{
					item3.stackable = Mathf.Clamp((int)(num * base.ConfigInstance.GlobalItemsMultiplier), 1, int.MaxValue);
				}
			}
		}
	}

	public override void OnDisabled(bool initialized)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base.OnDisabled(initialized);
		if (!initialized)
		{
			return;
		}
		Logger.Log((object)"Rolling back item manager");
		foreach (KeyValuePair<ItemCategory, float> category in base.ConfigInstance.Categories)
		{
			foreach (ItemDefinition item in ItemManager.itemList)
			{
				if (item.category == category.Key && !base.ConfigInstance.Blacklist.Contains(item.shortname) && !base.ConfigInstance.Items.ContainsKey(item.shortname))
				{
					base.DataInstance.Items.TryGetValue(item.shortname, out var value);
					if (value > 0)
					{
						item.stackable = MathEx.Clamp(value, 1, int.MaxValue);
					}
				}
			}
		}
		foreach (ItemDefinition item2 in ItemManager.itemList)
		{
			if (base.ConfigInstance.Items.ContainsKey(item2.shortname))
			{
				base.DataInstance.Items.TryGetValue(item2.shortname, out var value2);
				if (value2 > 0)
				{
					item2.stackable = MathEx.Clamp(value2, 1, int.MaxValue);
				}
			}
		}
	}

	public bool IsBypassed(ItemDefinition definition)
	{
		if (definition.itemMods == null || definition.itemMods.Length == 0)
		{
			return false;
		}
		ItemMod[] itemMods = definition.itemMods;
		foreach (ItemMod val in itemMods)
		{
			if ((base.ConfigInstance.ProhibitItemContainerStacking && val is ItemModContainer) || (base.ConfigInstance.ProhibitItemConsumableContainerStacking && val is ItemModConsumeContents) || (base.ConfigInstance.ProhibitItemFishableStacking && val is ItemModFishable) || val is ItemModPhoto)
			{
				return true;
			}
		}
		return false;
	}

	public override bool PreLoadShouldSave(bool newConfig, bool newData)
	{
		if (newConfig)
		{
			base.ConfigInstance.Blacklist.Add("water");
			base.ConfigInstance.Blacklist.Add("water.salt");
			base.ConfigInstance.Blacklist.Add("water.radioactive");
			return true;
		}
		return false;
	}

	public override void Load()
	{
		base.Load();
		base.OnEnableStatus();
	}

	public override void OnServerInit(bool initial)
	{
		base.OnServerInit(initial);
		if (initial)
		{
			((CarbonModule<StackManagerConfig, StackManagerData>)this).OnEnabled(true);
		}
	}
}
