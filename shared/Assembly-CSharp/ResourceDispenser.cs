using System;
using System.Collections.Generic;
using Facepunch;
using Facepunch.Rust;
using Oxide.Core;
using Rust;
using UnityEngine;

public class ResourceDispenser : EntityComponent<BaseEntity>, IServerComponent
{
	public enum GatherType
	{
		Tree,
		Ore,
		Flesh,
		UNSET,
		LAST
	}

	[Serializable]
	public class GatherPropertyEntry
	{
		public float gatherDamage;

		public float destroyFraction;

		public float conditionLost;
	}

	[Serializable]
	public class GatherProperties
	{
		public GatherPropertyEntry Tree;

		public GatherPropertyEntry Ore;

		public GatherPropertyEntry Flesh;

		public bool ProduceHeadItem;

		public float GetProficiency()
		{
			float num = 0f;
			for (int i = 0; i < 3; i++)
			{
				GatherPropertyEntry fromIndex = GetFromIndex((GatherType)i);
				float num2 = fromIndex.gatherDamage * fromIndex.destroyFraction;
				if (num2 > 0f)
				{
					num += fromIndex.gatherDamage / num2;
				}
			}
			return num;
		}

		public bool Any()
		{
			for (int i = 0; i < 3; i++)
			{
				GatherPropertyEntry fromIndex = GetFromIndex((GatherType)i);
				if (fromIndex.gatherDamage > 0f || fromIndex.conditionLost > 0f)
				{
					return true;
				}
			}
			return false;
		}

		public GatherPropertyEntry GetFromIndex(GatherType index)
		{
			return index switch
			{
				GatherType.Tree => Tree, 
				GatherType.Ore => Ore, 
				GatherType.Flesh => Flesh, 
				_ => null, 
			};
		}
	}

	public Phrase OwnershipPhrase;

	public GatherType gatherType = GatherType.UNSET;

	public List<ItemAmount> containedItems;

	public float maxDestroyFractionForFinishBonus = 0.2f;

	public List<ItemAmount> finishBonus;

	public bool forceFullFinishBonus;

	public float fractionRemaining = 1f;

	private float categoriesRemaining;

	private float startingItemCounts;

	private static Dictionary<GatherType, HashSet<int>> cachedResourceItemTypes;

	public void Start()
	{
		Initialize();
	}

	public void Initialize()
	{
		CacheResourceTypeItems();
		UpdateFraction();
		UpdateRemainingCategories();
		CountAllItems();
	}

	private void CacheResourceTypeItems()
	{
		if (cachedResourceItemTypes == null)
		{
			cachedResourceItemTypes = new Dictionary<GatherType, HashSet<int>>();
			HashSet<int> hashSet = new HashSet<int>();
			hashSet.Add(ItemManager.FindItemDefinition("wood").itemid);
			cachedResourceItemTypes.Add(GatherType.Tree, hashSet);
			HashSet<int> hashSet2 = new HashSet<int>();
			hashSet2.Add(ItemManager.FindItemDefinition("stones").itemid);
			hashSet2.Add(ItemManager.FindItemDefinition("sulfur.ore").itemid);
			hashSet2.Add(ItemManager.FindItemDefinition("metal.ore").itemid);
			hashSet2.Add(ItemManager.FindItemDefinition("hq.metal.ore").itemid);
			cachedResourceItemTypes.Add(GatherType.Ore, hashSet2);
		}
	}

	public void DoGather(HitInfo info, BaseCorpse corpse = null)
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		if (!base.baseEntity.isServer || !info.CanGather || info.DidGather)
		{
			return;
		}
		if (gatherType == GatherType.UNSET)
		{
			Debug.LogWarning((object)("Object :" + ((Object)((Component)this).gameObject).name + ": has unset gathertype!"));
			return;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = (((Object)(object)info.InitiatorPlayer != (Object)null && info.InitiatorPlayer.HasPlayerFlag(BasePlayer.PlayerFlags.IsInTutorial)) ? 3f : 1f);
		BaseMelee baseMelee = (((Object)(object)info.Weapon == (Object)null) ? null : (info.Weapon as BaseMelee));
		if ((Object)(object)baseMelee != (Object)null)
		{
			GatherPropertyEntry gatherInfoFromIndex = baseMelee.GetGatherInfoFromIndex(gatherType);
			num = gatherInfoFromIndex.gatherDamage * info.gatherScale * num3;
			num2 = gatherInfoFromIndex.destroyFraction;
			if (num == 0f)
			{
				return;
			}
			baseMelee.SendPunch(new Vector3(Random.Range(0.5f, 1f), Random.Range(-0.25f, -0.5f), 0f) * -30f * (gatherInfoFromIndex.conditionLost / 6f), 0.05f);
			baseMelee.LoseCondition(gatherInfoFromIndex.conditionLost);
			if (!baseMelee.IsValid() || baseMelee.IsBroken())
			{
				return;
			}
			info.DidGather = true;
		}
		else
		{
			num = info.damageTypes.Total();
			num2 = 0.5f;
		}
		float num4 = fractionRemaining;
		GiveResources(info.InitiatorPlayer, num, num2, info.Weapon);
		UpdateFraction();
		float num5 = 0f;
		if (fractionRemaining <= 0f)
		{
			num5 = base.baseEntity.MaxHealth();
			if (info.DidGather && num2 < maxDestroyFractionForFinishBonus)
			{
				AssignFinishBonus(info.InitiatorPlayer, 1f - num2, info.Weapon);
			}
			HeadDispenser headDispenser = default(HeadDispenser);
			if (((Component)this).gameObject.TryGetComponent<HeadDispenser>(ref headDispenser))
			{
				headDispenser.DispenseHead(info, corpse);
			}
		}
		else
		{
			num5 = (num4 - fractionRemaining) * base.baseEntity.MaxHealth();
		}
		HitInfo hitInfo = Pool.Get<HitInfo>();
		hitInfo.Init(info.Initiator, base.baseEntity, DamageType.Generic, num5, ((Component)this).transform.position);
		hitInfo.gatherScale = 0f;
		hitInfo.PointStart = info.PointStart;
		hitInfo.PointEnd = info.PointEnd;
		hitInfo.WeaponPrefab = info.WeaponPrefab;
		hitInfo.Weapon = info.Weapon;
		base.baseEntity.OnAttacked(hitInfo);
		Pool.Free<HitInfo>(ref hitInfo);
	}

	public void AssignFinishBonus(BasePlayer player, float fraction, AttackEntity weapon)
	{
		if (forceFullFinishBonus)
		{
			fraction = 1f;
		}
		((Component)this).SendMessage("FinishBonusAssigned", (SendMessageOptions)1);
		if (fraction <= 0f || finishBonus == null)
		{
			return;
		}
		foreach (ItemAmount finishBonu in finishBonus)
		{
			int num = Mathf.CeilToInt((float)(int)finishBonu.amount * Mathf.Clamp01(fraction));
			int num2 = CalculateGatherBonus(player, finishBonu, num);
			Item item = ItemManager.Create(finishBonu.itemDef, ScaleGatherAmount(num + num2), 0uL, isServerSide: true, 0uL);
			if (item != null)
			{
				object obj = Interface.CallHook("OnDispenserBonus", this, player, item);
				if (obj is Item)
				{
					item = (Item)obj;
				}
				ApplyItemOwnership(player, item);
				Facepunch.Rust.Analytics.Azure.OnGatherItem(item.info.shortname, item.amount, base.baseEntity, player, weapon);
				Interface.CallHook("OnDispenserBonusReceived", this, player, item);
				while (item.amount > item.MaxStackable())
				{
					Item item2 = item.SplitItem(item.MaxStackable());
					player.GiveItem(item2, BaseEntity.GiveItemReason.ResourceHarvested, GiveItemOptions.BackpackOverflow);
				}
				player.GiveItem(item, BaseEntity.GiveItemReason.ResourceHarvested, GiveItemOptions.BackpackOverflow);
			}
		}
	}

	public void OnResourceDispenserAttacked(HitInfo info)
	{
		DoGather(info);
	}

	private void GiveResources(BasePlayer entity, float gatherDamage, float destroyFraction, AttackEntity attackWeapon)
	{
		if (!entity.IsValid() || gatherDamage <= 0f)
		{
			return;
		}
		ItemAmount itemAmount = null;
		int num = containedItems.Count;
		int num2 = Random.Range(0, containedItems.Count);
		while (num > 0)
		{
			if (num2 >= containedItems.Count)
			{
				num2 = 0;
			}
			if (containedItems[num2].amount > 0f)
			{
				itemAmount = containedItems[num2];
				break;
			}
			num2++;
			num--;
		}
		if (itemAmount != null)
		{
			GiveResourceFromItem(entity, itemAmount, gatherDamage, destroyFraction, attackWeapon);
			UpdateVars();
		}
	}

	public void DestroyFraction(float fraction)
	{
		foreach (ItemAmount containedItem in containedItems)
		{
			if (containedItem.amount > 0f)
			{
				containedItem.amount -= fraction / categoriesRemaining;
			}
		}
		UpdateVars();
	}

	private void GiveResourceFromItem(BasePlayer entity, ItemAmount itemAmt, float gatherDamage, float destroyFraction, AttackEntity attackWeapon)
	{
		if (itemAmt.amount == 0f)
		{
			return;
		}
		float num = Mathf.Min(gatherDamage, base.baseEntity.Health()) / base.baseEntity.MaxHealth();
		float num2 = itemAmt.startAmount / startingItemCounts;
		float num3 = Mathf.Clamp(itemAmt.startAmount * num / num2, 0f, itemAmt.amount);
		num3 = Mathf.Round(num3);
		float num4 = num3 * destroyFraction * 2f;
		if (itemAmt.amount <= num3 + num4)
		{
			float num5 = (num3 + num4) / itemAmt.amount;
			num3 /= num5;
			num4 /= num5;
		}
		itemAmt.amount -= Mathf.Floor(num3);
		itemAmt.amount -= Mathf.Floor(num4);
		if (num3 < 1f)
		{
			num3 = ((Random.Range(0f, 1f) <= num3) ? 1f : 0f);
			itemAmt.amount = 0f;
		}
		if (itemAmt.amount < 0f)
		{
			itemAmt.amount = 0f;
		}
		if (!(num3 >= 1f))
		{
			return;
		}
		int num6 = CalculateGatherBonus(entity, itemAmt, num3);
		int iAmount = ScaleGatherAmount(Mathf.FloorToInt(num3) + num6);
		Item item = ItemManager.CreateByItemID(itemAmt.itemid, iAmount, 0uL, 0uL);
		if (Interface.CallHook("OnDispenserGather", this, entity, item) == null && item != null)
		{
			ApplyItemOwnership(entity, item);
			OverrideOwnership(item, attackWeapon);
			Facepunch.Rust.Analytics.Azure.OnGatherItem(item.info.shortname, item.amount, base.baseEntity, entity, attackWeapon);
			Interface.CallHook("OnDispenserGathered", this, entity, item);
			while (item.amount > item.MaxStackable())
			{
				Item item2 = item.SplitItem(item.MaxStackable());
				entity.GiveItem(item2, BaseEntity.GiveItemReason.ResourceHarvested, GiveItemOptions.BackpackOverflow);
			}
			entity.GiveItem(item, BaseEntity.GiveItemReason.ResourceHarvested, GiveItemOptions.BackpackOverflow);
		}
	}

	private static int ScaleGatherAmount(int amount)
	{
		if (amount <= 0)
		{
			return amount;
		}
		float num = 1f;
		if (BaseGameMode.GetActiveGameMode(serverside: true) is GameModeSoftcore)
		{
			num = Mathf.Max(0f, GameModeSoftcore.gather_rate);
		}
		return Mathf.Max(1, Mathf.RoundToInt((float)amount * num));
	}

	private void ApplyItemOwnership(BasePlayer player, Item item)
	{
		if (OwnershipPhrase != null && !string.IsNullOrEmpty(OwnershipPhrase.token))
		{
			item.SetItemOwnership(player, OwnershipPhrase.token);
		}
		else
		{
			item.SetItemOwnership(player, ItemOwnershipPhrases.GatheredPhrase);
		}
	}

	private int CalculateGatherBonus(BaseEntity entity, ItemAmount item, float amountToGive)
	{
		if ((Object)(object)entity == (Object)null)
		{
			return 0;
		}
		BasePlayer basePlayer = entity.ToPlayer();
		if ((Object)(object)basePlayer == (Object)null)
		{
			return 0;
		}
		if ((Object)(object)basePlayer.modifiers == (Object)null)
		{
			return 0;
		}
		amountToGive = Mathf.FloorToInt(amountToGive);
		float num = 1f;
		Modifier.ModifierType type;
		switch (gatherType)
		{
		case GatherType.Tree:
			type = Modifier.ModifierType.Wood_Yield;
			break;
		case GatherType.Ore:
			type = Modifier.ModifierType.Ore_Yield;
			break;
		case GatherType.Flesh:
			type = Modifier.ModifierType.Harvesting;
			break;
		default:
			return 0;
		}
		if (!IsProducedItemOfGatherType(item))
		{
			return 0;
		}
		num += basePlayer.modifiers.GetValue(type);
		float variableValue = basePlayer.modifiers.GetVariableValue(type, 0f);
		float num2 = ((num > 1f) ? Mathf.Max(amountToGive * num - amountToGive, 0f) : 0f);
		variableValue += num2;
		int num3 = 0;
		if (variableValue >= 1f)
		{
			num3 = (int)variableValue;
			variableValue -= (float)num3;
		}
		basePlayer.modifiers.SetVariableValue(type, variableValue);
		return num3;
	}

	private bool IsProducedItemOfGatherType(ItemAmount item)
	{
		if (gatherType == GatherType.Tree)
		{
			return cachedResourceItemTypes[GatherType.Tree].Contains(item.itemid);
		}
		if (gatherType == GatherType.Ore)
		{
			return cachedResourceItemTypes[GatherType.Ore].Contains(item.itemid);
		}
		if (gatherType == GatherType.Flesh)
		{
			return item.startAmount > 1f;
		}
		return false;
	}

	public virtual bool OverrideOwnership(Item item, AttackEntity weapon)
	{
		return false;
	}

	private void UpdateVars()
	{
		UpdateFraction();
		UpdateRemainingCategories();
	}

	public void UpdateRemainingCategories()
	{
		int num = 0;
		foreach (ItemAmount containedItem in containedItems)
		{
			if (containedItem.amount > 0f)
			{
				num++;
			}
		}
		categoriesRemaining = num;
	}

	public void CountAllItems()
	{
		float num = 0f;
		foreach (ItemAmount containedItem in containedItems)
		{
			num += containedItem.startAmount;
		}
		startingItemCounts = num;
	}

	private void UpdateFraction()
	{
		float num = 0f;
		float num2 = 0f;
		foreach (ItemAmount containedItem in containedItems)
		{
			num += containedItem.startAmount;
			num2 += containedItem.amount;
		}
		if (num == 0f)
		{
			fractionRemaining = 0f;
		}
		else
		{
			fractionRemaining = num2 / num;
		}
	}

	public bool HasItemToDispense(ItemDefinition def)
	{
		if ((Object)(object)def == (Object)null)
		{
			return false;
		}
		foreach (ItemAmount containedItem in containedItems)
		{
			if ((Object)(object)containedItem.itemDef == (Object)(object)def && containedItem.amount > 0f)
			{
				return true;
			}
		}
		return false;
	}
}
