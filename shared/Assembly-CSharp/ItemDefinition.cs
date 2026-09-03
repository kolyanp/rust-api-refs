using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Rust;
using UnityEngine;

public class ItemDefinition : MonoBehaviour, IEqualityComparer<ItemDefinition>
{
	[Flags]
	public enum LootDistributionModifierType
	{
		None = 0,
		Firearm = 1,
		FirearmAmmunition = 2,
		Unused = int.MinValue
	}

	[Serializable]
	public struct Condition
	{
		[Serializable]
		public class WorldSpawnCondition
		{
			public float fractionMin = 1f;

			public float fractionMax = 1f;
		}

		public bool enabled;

		[Tooltip("The maximum condition this item type can have, new items will start with this value")]
		public float max;

		[Tooltip("If false then item will destroy when condition reaches 0")]
		public bool repairable;

		[Tooltip("If true, never lose max condition when repaired")]
		public bool maintainMaxCondition;

		public bool ovenCondition;

		public WorldSpawnCondition foundCondition;

		public bool hideConditionBar;

		public GameObjectRef breakEffect;
	}

	[Serializable]
	public struct OverrideWorldModel
	{
		public GameObjectRef worldModel;

		public int minStackSize;
	}

	public enum RedirectVendingBehaviour
	{
		NoListing,
		ListAsUniqueItem
	}

	[Flags]
	public enum Flag
	{
		NoDropping = 1,
		NotStraightToBelt = 2,
		NotAllowedInBelt = 4,
		Backpack = 8,
		PrioritizeBelt = 0x10
	}

	public enum AmountType
	{
		Count,
		Millilitre,
		Feet,
		Genetics,
		OxygenSeconds,
		Frequency,
		Generic,
		BagLimit,
		ShelterLimit,
		ContentCount,
		TurretLimit,
		NucleusGrades,
		BBSLimit
	}

	[Header("Item")]
	[ReadOnly]
	public int itemid;

	[Tooltip("The shortname should be unique. A hash will be generated from it to identify the item type. If this name changes at any point it will make all saves incompatible")]
	public string shortname;

	public Era era;

	public EraRestriction eraRestrictions;

	public LootDistributionModifierType lootDistributionType;

	[Header("Appearance")]
	public Phrase displayName;

	public Phrase displayDescription;

	public Sprite iconSprite;

	public ItemCategory category;

	public ItemSelectionPanel selectionPanel;

	[Header("Appearance - Vehicle Item")]
	public bool vehicleItem;

	public VehicleCategory vehicleCategory = VehicleCategory.Misc;

	[Header("Containment")]
	public int maxDraggable;

	public ItemContainer.ContentsType itemType = ItemContainer.ContentsType.Generic;

	public AmountType amountType;

	[InspectorFlags]
	public ItemSlot occupySlots = ItemSlot.None;

	public int stackable;

	public int volume;

	public float baseRadioactivity;

	[NonSerialized]
	public float ApartmentTaxPerStack;

	public bool quickDespawn;

	public bool blockStealingInSafeZone;

	[Tooltip("Should this item be blocked from being burried and found by other players? Off by default to allow most items.")]
	public bool allowBurying;

	public BasePlayer.TutorialItemAllowance tutorialAllowance;

	[Tooltip("If true, this item will support item ownership even if it's stacksize is >1")]
	public bool supportsStackableOwnership;

	[Tooltip("How rare this item is and how much it costs to research")]
	[Header("Spawn Tables")]
	public Rarity rarity;

	public Rarity despawnRarity;

	public bool spawnAsBlueprint;

	[Header("Sounds")]
	public SoundDefinition inventoryGrabSound;

	public SoundDefinition inventoryDropSound;

	public SoundDefinition physImpactSoundDef;

	public Condition condition;

	[Header("Misc")]
	public bool hidden;

	[InspectorFlags]
	public Flag flags;

	public bool hideSelectedPanel;

	[Tooltip("User can craft this item on any server if they have this steam item")]
	public SteamInventoryItem steamItem;

	[Tooltip("User can craft this item if they have this DLC purchased")]
	public SteamDLCItem steamDlc;

	public bool supportsAccessories;

	[Tooltip("Can only craft this item if the parent is craftable (tech tree)")]
	public ItemDefinition Parent;

	[Header("World Model")]
	public GameObjectRef worldModelPrefab;

	public OverrideWorldModel[] worldModelOverrides;

	public bool treatAsComponentForRepairs;

	public bool AlignWorldModelOnDrop;

	public Vector3 WorldModelDropOffset;

	public bool AdjustCenterOfMassOnDrop;

	public Vector3 DropCenterOfMass;

	public ItemDefinition isRedirectOf;

	public RedirectVendingBehaviour redirectVendingBehaviour;

	[NonSerialized]
	public ItemMod[] itemMods;

	public BaseEntity.TraitFlag Traits;

	private string _harvestStatKey;

	public ItemSkinDirectory.Skin[] skins;

	[NonSerialized]
	public IPlayerItemDefinition[] _skins2;

	private float _worldModelMass;

	[Tooltip("Panel to show in the inventory menu when selected")]
	public GameObject panel;

	private ItemBlueprint _blueprint;

	[NonSerialized]
	public ItemDefinition[] Children = new ItemDefinition[0];

	public string HarvestStatKey
	{
		get
		{
			if (_harvestStatKey == null)
			{
				_harvestStatKey = "harvest." + shortname;
			}
			return _harvestStatKey;
		}
	}

	public IPlayerItemDefinition[] skins2
	{
		get
		{
			if (_skins2 != null)
			{
				return _skins2;
			}
			if (PlatformService.Instance.IsValid && PlatformService.Instance.ItemDefinitions != null)
			{
				string prefabname = ((Object)this).name;
				_skins2 = PlatformService.Instance.ItemDefinitions.Where((IPlayerItemDefinition x) => (x.ItemShortName == shortname || x.ItemShortName == prefabname) && x.WorkshopId != 0).ToArray();
			}
			return _skins2;
		}
	}

	public ItemBlueprint Blueprint
	{
		get
		{
			if (_blueprint == null)
			{
				_blueprint = ((Component)this).GetComponent<ItemBlueprint>();
			}
			return _blueprint;
		}
	}

	public int craftingStackable => Mathf.Max(10, stackable);

	public bool isWearable => (Object)(object)ItemModWearable != (Object)null;

	public ItemModWearable ItemModWearable { get; set; }

	public ItemModBurnable ItemModBurnable { get; set; }

	public CookableItemInfo ItemModCookable { get; set; }

	public CookableItemInfo ItemModCompostable { get; private set; }

	public ItemModEntity ItemModEntity { get; private set; }

	public bool HasItemModEntity { get; private set; }

	public ItemModSpriteConfig ItemModSpriteConfig { get; private set; }

	public bool isHoldable { get; private set; }

	public bool isUsable { get; private set; }

	public bool HasSkins
	{
		get
		{
			if (skins2 != null && skins2.Length != 0)
			{
				return true;
			}
			if (skins != null && skins.Length != 0)
			{
				return true;
			}
			return false;
		}
	}

	public bool CraftableWithSkin { get; private set; }

	public bool Hidden()
	{
		return hidden;
	}

	public bool MatchesItemId(int itemid, bool redirectAllowed)
	{
		if (this.itemid == itemid)
		{
			return true;
		}
		if (redirectAllowed && (Object)(object)isRedirectOf != (Object)null)
		{
			return isRedirectOf.itemid == itemid;
		}
		return false;
	}

	public void InvalidateWorkshopSkinCache()
	{
		_skins2 = null;
	}

	public bool IsAllowed(EraRestriction targetRestriction)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if ((Object)(object)activeGameMode != (Object)null && !activeGameMode.IsAllowed(this, targetRestriction))
		{
			return false;
		}
		return IsAllowedInEra(targetRestriction);
	}

	public bool IsAllowed(EraRestriction targetRestriction, Era serverEra)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if ((Object)(object)activeGameMode != (Object)null && !activeGameMode.IsAllowed(this, targetRestriction))
		{
			return false;
		}
		return IsAllowedInEra(targetRestriction, serverEra);
	}

	public bool IsAllowedInEra(EraRestriction targetRestriction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if ((int)ConVar.Server.Era == 0)
		{
			return true;
		}
		return IsAllowedInEra(targetRestriction, ConVar.Server.Era);
	}

	private bool IsAllowedInEra(EraRestriction targetRestriction, Era serverEra)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if ((int)serverEra == 0)
		{
			return true;
		}
		if ((Object)(object)isRedirectOf != (Object)null)
		{
			return isRedirectOf.IsAllowedInEra(targetRestriction);
		}
		Era val = era;
		if ((int)val != 0)
		{
			if ((int)val == 1)
			{
				return true;
			}
			if (era <= serverEra)
			{
				if ((int)targetRestriction != 0 && (int)eraRestrictions != 0 && (EraRestriction)(eraRestrictions & targetRestriction) != eraRestrictions)
				{
					return false;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static ulong FindSkin(int itemID, int skinID)
	{
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(itemID);
		if ((Object)(object)itemDefinition == (Object)null)
		{
			return 0uL;
		}
		IPlayerItemDefinition itemDefinition2 = PlatformService.Instance.GetItemDefinition(skinID);
		if (itemDefinition2 != null)
		{
			ulong workshopDownload = itemDefinition2.WorkshopDownload;
			if (workshopDownload != 0L)
			{
				string itemShortName = itemDefinition2.ItemShortName;
				if (itemShortName == itemDefinition.shortname || itemShortName == ((Object)itemDefinition).name)
				{
					return workshopDownload;
				}
			}
		}
		for (int i = 0; i < itemDefinition.skins.Length; i++)
		{
			if (itemDefinition.skins[i].id == skinID)
			{
				return (ulong)skinID;
			}
		}
		return 0uL;
	}

	public float GetWorldModelMass()
	{
		if (_worldModelMass != 0f)
		{
			return _worldModelMass;
		}
		GameObject val = worldModelPrefab?.Get();
		if ((Object)(object)val != (Object)null)
		{
			WorldModel component = val.GetComponent<WorldModel>();
			if ((Object)(object)component != (Object)null && component.mass != 0f)
			{
				_worldModelMass = component.mass;
				return _worldModelMass;
			}
		}
		_worldModelMass = 1f;
		return _worldModelMass;
	}

	public int GetWorldModelTriCount(int lod = 0)
	{
		if (worldModelPrefab == null || !worldModelPrefab.isValid)
		{
			return 0;
		}
		GameObject val = worldModelPrefab.Get();
		if ((Object)(object)val == (Object)null)
		{
			return 0;
		}
		WorldModel worldModel = default(WorldModel);
		if (val.TryGetComponent<WorldModel>(ref worldModel))
		{
			return worldModel.GetTriCount(lod);
		}
		return 0;
	}

	public bool HasFlag(Flag f)
	{
		return (flags & f) == f;
	}

	public void Initialize(List<ItemDefinition> itemList)
	{
		if (itemMods != null)
		{
			Debug.LogError((object)("Item Definition Initializing twice: " + ((Object)this).name));
		}
		skins = ItemSkinDirectory.ForItem(this);
		itemMods = ((Component)this).GetComponentsInChildren<ItemMod>(true);
		ItemMod[] array = itemMods;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ModInit(this);
		}
		Children = itemList.Where((ItemDefinition x) => (Object)(object)x.Parent == (Object)(object)this).ToArray();
		ItemModWearable = ((Component)this).GetComponent<ItemModWearable>();
		ItemModBurnable = ((Component)this).GetComponent<ItemModBurnable>();
		ItemModCookable component = ((Component)this).GetComponent<ItemModCookable>();
		if ((Object)(object)component != (Object)null)
		{
			ItemModCookable = new CookableItemInfo(component);
		}
		ItemModEntity = ((Component)this).GetComponent<ItemModEntity>();
		HasItemModEntity = (Object)(object)ItemModEntity != (Object)null;
		ItemModSpriteConfig = ((Component)this).GetComponent<ItemModSpriteConfig>();
		isHoldable = (Object)(object)((Component)this).GetComponent<ItemModEntity>() != (Object)null;
		isUsable = (Object)(object)((Component)this).GetComponent<ItemModEntity>() != (Object)null || (Object)(object)((Component)this).GetComponent<ItemModConsume>() != (Object)null;
		ItemModCompostable component2 = ((Component)this).GetComponent<ItemModCompostable>();
		if ((Object)(object)component2 != (Object)null && component2.TotalFertilizerProduced > 0f)
		{
			ItemModCompostable = new CookableItemInfo(component2);
		}
	}

	public GameObjectRef GetWorldModel(int amount)
	{
		if (worldModelOverrides == null || worldModelOverrides.Length == 0)
		{
			return worldModelPrefab;
		}
		for (int num = worldModelOverrides.Length - 1; num >= 0; num--)
		{
			if (amount >= worldModelOverrides[num].minStackSize)
			{
				return worldModelOverrides[num].worldModel;
			}
		}
		return worldModelPrefab;
	}

	public int GetWorldModelIndex(int amount)
	{
		if (worldModelOverrides == null || worldModelOverrides.Length == 0)
		{
			return -1;
		}
		for (int num = worldModelOverrides.Length - 1; num >= 0; num--)
		{
			if (amount >= worldModelOverrides[num].minStackSize)
			{
				return num;
			}
		}
		return -1;
	}

	public bool SupportsItemOwnership()
	{
		if (stackable != 1)
		{
			if (supportsStackableOwnership)
			{
				return Inventory.stackable_item_ownership;
			}
			return false;
		}
		return true;
	}

	public bool Equals(ItemDefinition x, ItemDefinition y)
	{
		if (x == null)
		{
			return false;
		}
		if (y == null)
		{
			return false;
		}
		return x.itemid == y.itemid;
	}

	public int GetHashCode(ItemDefinition obj)
	{
		return obj.itemid;
	}

	public static Phrase GetCategoryLabel(ItemCategory category)
	{
		return (Phrase)(category switch
		{
			ItemCategory.Weapon => Translate.GetPhrase("bp_weapons"), 
			ItemCategory.Attire => Translate.GetPhrase("bp_clothing"), 
			ItemCategory.Tool => Translate.GetPhrase("bp_tools"), 
			ItemCategory.Ammunition => Translate.GetPhrase("bp_ammo"), 
			ItemCategory.Misc => Translate.GetPhrase("bp_misc"), 
			_ => Translate.GetPhrase("bp_" + category.ToString().ToLower()), 
		});
	}

	public static Phrase GetVehicleCategoryLabel(VehicleCategory category)
	{
		if (category == VehicleCategory.Misc)
		{
			return Translate.GetPhrase("bp_misc");
		}
		return Translate.GetPhrase("vehicle." + category.ToString().ToLower());
	}
}
