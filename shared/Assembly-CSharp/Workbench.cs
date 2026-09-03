using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Workbench : StorageContainer
{
	[Serializable]
	public struct UpgradeVisualPoint
	{
		public ItemDefinition upgradeItem;

		public Transform point;

		[Tooltip("Optional prevent-building volume to activate when this upgrade is installed.")]
		public BoxCollider preventBuildingVolume;
	}

	[Serializable]
	public struct CachedUpgradeVisualPoint
	{
		public ItemDefinition upgradeItem;

		public Vector3 localPosition;

		public Quaternion localRotation;

		public Vector3 localScale;

		[Tooltip("Optional prevent-building volume to activate when this upgrade is installed. Also used as the clearance zone — upgrade is blocked from installation if this volume is occupied by a deployable.")]
		public BoxCollider preventBuildingVolume;
	}

	[Serializable]
	public struct UpgradeFillerVisual
	{
		[Tooltip("Transform to toggle on/off. Disabled when any of the associated upgrades are installed.")]
		public Transform fillerTransform;

		[Tooltip("When any of these upgrades are installed, this filler is hidden to make room for the upgrade visual.")]
		public ItemDefinition[] upgrades;
	}

	private struct CachedUpgrade
	{
		public Item item;

		public ItemModWorkbenchUpgrade mod;
	}

	[Header("Upgrades")]
	public int upgradeSlotCount;

	public TriggerComfort upgradeComfortTrigger;

	public SphereCollider upgradeComfortCollider;

	[Tooltip("Maps each upgrade item to its visual spawn point on this workbench.")]
	public UpgradeVisualPoint[] upgradeVisualPoints;

	[Tooltip("Cached local-space positions baked from UpgradeVisualPlacement. Used at runtime instead of the transform hierarchy.")]
	public CachedUpgradeVisualPoint[] cachedUpgradeVisualPoints;

	private int clearanceCacheFrame;

	private ItemDefinition clearanceCacheItem;

	private bool clearanceCacheResult;

	[Tooltip("Editor-only transform holding visual placement points. Should be removed at runtime.")]
	public Transform upgradeVisualPlacement;

	[Header("Filler Visuals")]
	[Tooltip("Active when no upgrades are installed at all. Disabled when any upgrade is present.")]
	public Transform fullFillerVisual;

	[Tooltip("Individual filler transforms that are hidden when their associated upgrade is installed.")]
	public UpgradeFillerVisual[] upgradeFillerVisuals;

	private readonly List<CachedUpgrade> cachedServerUpgrades;

	public static readonly Phrase RecycleBinNotEmptyPhrase;

	public const int blueprintSlot = 0;

	public const int experimentSlot = 1;

	public const int firstUpgradeSlot = 2;

	public bool Static;

	public int Workbenchlevel;

	public bool isIOBench;

	public TriggerWorkbench WorkbenchTrigger;

	private const string legacyWorkbenchLootPanel = "workbench";

	private const string upgradeWorkbenchLootPanel = "workbench_upgrades";

	private const string recycleBinLootPanel = "generic_resizable";

	private Vector3 originalCraftTriggerSize;

	private Vector3 originalCraftTriggerCenter;

	private bool craftTriggerCached;

	private float originalComfortBaseValue;

	private float originalComfortTriggerRadius;

	private float originalMaxHealth;

	public LootSpawn experimentalItems;

	public GameObjectRef experimentStartEffect;

	public GameObjectRef experimentSuccessEffect;

	public ItemDefinition experimentResource;

	public TechTreeData[] techTrees;

	private float clientTechTreeMultiplier;

	public static ItemDefinition blueprintBaseDef;

	private ItemDefinition pendingBlueprint;

	private bool creatingBlueprint;

	public int UpgradeSlotCount => Mathf.Max(0, upgradeSlotCount);

	public int RequiredInventorySlots => 2 + UpgradeSlotCount;

	public int InstalledUpgradeCount
	{
		get
		{
			if (base.inventory == null)
			{
				return 0;
			}
			int num = 0;
			for (int i = 2; i < RequiredInventorySlots; i++)
			{
				if (base.inventory.GetSlot(i) != null)
				{
					num++;
				}
			}
			return num;
		}
	}

	public override bool ValidateMeleeColliderAntihack => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Workbench.OnRpcMessage"))
		{
			if (rpc == 2308794761u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_BeginExperiment"));
				}
				using (TimeWarning.New("RPC_BeginExperiment"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2308794761u, "RPC_BeginExperiment", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_BeginExperiment(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_BeginExperiment");
					}
				}
				return true;
			}
			if (rpc == 2475703927u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenRecycleBin"));
				}
				using (TimeWarning.New("RPC_OpenRecycleBin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2475703927u, "RPC_OpenRecycleBin", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_OpenRecycleBin(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_OpenRecycleBin");
					}
				}
				return true;
			}
			if (rpc == 2535666051u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenUpgradeInventory"));
				}
				using (TimeWarning.New("RPC_OpenUpgradeInventory"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2535666051u, "RPC_OpenUpgradeInventory", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_OpenUpgradeInventory(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_OpenUpgradeInventory");
					}
				}
				return true;
			}
			if (rpc == 3268333598u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_SendTechTreeMultiplier"));
				}
				using (TimeWarning.New("RPC_SendTechTreeMultiplier"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3268333598u, "RPC_SendTechTreeMultiplier", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_SendTechTreeMultiplier(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RPC_SendTechTreeMultiplier");
					}
				}
				return true;
			}
			if (rpc == 4127240744u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_TechTreeUnlock"));
				}
				using (TimeWarning.New("RPC_TechTreeUnlock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4127240744u, "RPC_TechTreeUnlock", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg6 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_TechTreeUnlock(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in RPC_TechTreeUnlock");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsUpgradeSlot(int slot)
	{
		if (slot >= 2)
		{
			return slot < RequiredInventorySlots;
		}
		return false;
	}

	private void RefreshPreventBuildingZones(HashSet<ItemDefinition> installedUpgrades)
	{
		if (cachedUpgradeVisualPoints == null)
		{
			return;
		}
		for (int i = 0; i < cachedUpgradeVisualPoints.Length; i++)
		{
			ref CachedUpgradeVisualPoint reference = ref cachedUpgradeVisualPoints[i];
			if (!((Object)(object)reference.preventBuildingVolume == (Object)null))
			{
				bool active = (Object)(object)reference.upgradeItem != (Object)null && installedUpgrades != null && installedUpgrades.Contains(reference.upgradeItem);
				((Component)reference.preventBuildingVolume).gameObject.SetActive(active);
			}
		}
	}

	public float GetTechTreeCostMultiplier()
	{
		if (base.isClient)
		{
			return clientTechTreeMultiplier;
		}
		if (base.isServer)
		{
			return CalculateTechTreeCostMultiplier();
		}
		return 1f;
	}

	public bool IsUpgradeBlockedByClearanceZone(ItemDefinition upgradeDef)
	{
		if (ConVar.Workbench.skipclearancechecks)
		{
			return false;
		}
		int frameCount = Time.frameCount;
		if (frameCount == clearanceCacheFrame && (Object)(object)clearanceCacheItem == (Object)(object)upgradeDef)
		{
			return clearanceCacheResult;
		}
		clearanceCacheFrame = frameCount;
		clearanceCacheItem = upgradeDef;
		clearanceCacheResult = IsUpgradeBlockedByClearanceZoneInternal(upgradeDef);
		return clearanceCacheResult;
	}

	private bool IsUpgradeBlockedByClearanceZoneInternal(ItemDefinition upgradeDef)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		if (cachedUpgradeVisualPoints == null)
		{
			return false;
		}
		OBB val3 = default(OBB);
		for (int i = 0; i < cachedUpgradeVisualPoints.Length; i++)
		{
			ref CachedUpgradeVisualPoint reference = ref cachedUpgradeVisualPoints[i];
			if ((Object)(object)reference.preventBuildingVolume == (Object)null || (Object)(object)reference.upgradeItem != (Object)(object)upgradeDef)
			{
				continue;
			}
			BoxCollider preventBuildingVolume = reference.preventBuildingVolume;
			Transform transform = ((Component)preventBuildingVolume).transform;
			Vector3 val = transform.TransformPoint(preventBuildingVolume.center);
			Vector3 lossyScale = transform.lossyScale;
			Vector3 val2 = Vector3.Scale(preventBuildingVolume.size, lossyScale);
			((OBB)(ref val3))._002Ector(val, val2, transform.rotation);
			PooledList<BaseEntity> val4 = Pool.Get<PooledList<BaseEntity>>();
			try
			{
				Vis.Entities(val3.position, ((Vector3)(ref val3.extents)).magnitude + 0.25f, (List<BaseEntity>)(object)val4, 256, (QueryTriggerInteraction)1);
				for (int j = 0; j < ((List<BaseEntity>)(object)val4).Count; j++)
				{
					BaseEntity baseEntity = ((List<BaseEntity>)(object)val4)[j];
					if ((Object)(object)baseEntity == (Object)null || baseEntity.IsDestroyed || baseEntity.EqualNetID((BaseNetworkable)this))
					{
						continue;
					}
					if (baseEntity.HasParent())
					{
						BaseEntity baseEntity2 = baseEntity.GetParentEntity();
						if (baseEntity2 != null && baseEntity2.EqualNetID((BaseNetworkable)this))
						{
							continue;
						}
					}
					DeployVolume[] array = PrefabAttribute.server.FindAll<DeployVolume>(baseEntity.prefabID);
					if (array == null || array.Length == 0)
					{
						continue;
					}
					PooledList<DeployVolume> val5 = Pool.Get<PooledList<DeployVolume>>();
					try
					{
						DeployVolume[] array2 = array;
						foreach (DeployVolume deployVolume in array2)
						{
							if ((deployVolume.ignore & ColliderInfo.Flags.OnlyEvaluatePreventBuildingInMonuments) == 0 && DeployVolume.ShouldApplyVolumeForEntity(deployVolume, this))
							{
								((List<DeployVolume>)(object)val5).Add(deployVolume);
							}
						}
						if (((List<DeployVolume>)(object)val5).Count == 0 || !DeployVolume.Check(((Component)baseEntity).transform.position, ((Component)baseEntity).transform.rotation, (List<DeployVolume>)(object)val5, val3, 536870912))
						{
							continue;
						}
						return true;
					}
					finally
					{
						((IDisposable)val5)?.Dispose();
					}
				}
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
		}
		return false;
	}

	private void RebuildUpgradeCache()
	{
		cachedServerUpgrades.Clear();
		if (base.inventory == null)
		{
			return;
		}
		for (int i = 2; i < RequiredInventorySlots; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot != null)
			{
				ItemModWorkbenchUpgrade component = ((Component)slot.info).GetComponent<ItemModWorkbenchUpgrade>();
				if (!((Object)(object)component == (Object)null))
				{
					cachedServerUpgrades.Add(new CachedUpgrade
					{
						item = slot,
						mod = component
					});
				}
			}
		}
	}

	private float CalculateTechTreeCostMultiplier()
	{
		float num = 1f;
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			num *= cachedServerUpgrades[i].mod.GetTechTreeCostMultiplier();
		}
		return num;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_SendTechTreeMultiplier(RPCMessage msg)
	{
		ClientRPC(RpcTarget.Player("RPC_TechTreeMultiplier", msg.player), GetTechTreeCostMultiplier());
	}

	public void SaveUpgrades(Workbench wbProto)
	{
		if (wbProto != null)
		{
			wbProto.upgradeItemIds = Pool.Get<List<int>>();
			for (int i = 2; i < RequiredInventorySlots; i++)
			{
				Item slot = base.inventory.GetSlot(i);
				wbProto.upgradeItemIds.Add(slot?.info.itemid ?? 0);
			}
		}
	}

	private void RefreshRuntimeUpgrades()
	{
		RefreshReinforcedHealth();
		RefreshCraftRange();
		RefreshComfort();
		RefreshServerPreventBuildingZones();
	}

	private void OnUpgradeAddedOrRemoved(Item item, bool added)
	{
		if (added)
		{
			NotifyUpgradeInstalled(item);
			DoUpgradeEffect(item);
		}
		else
		{
			NotifyUpgradeRemoved(item);
		}
		RebuildUpgradeCache();
		RefreshRuntimeUpgrades();
		SendTechTreeMultiplierToGroup();
		SendNetworkUpdateImmediate();
	}

	private void SendTechTreeMultiplierToGroup()
	{
		ClientRPC(RpcTarget.NetworkGroup("RPC_TechTreeMultiplier"), GetTechTreeCostMultiplier());
	}

	private void RefreshReinforcedHealth()
	{
		if (originalMaxHealth <= 0f)
		{
			return;
		}
		float num = 0f;
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			if (cachedServerUpgrades[i].mod is ItemModWorkbenchReinforced itemModWorkbenchReinforced)
			{
				num += itemModWorkbenchReinforced.GetHealthBonusForWorkbench(Workbenchlevel, isIOBench);
			}
		}
		if (num > 0f)
		{
			OverrideMaxHealth(originalMaxHealth + num, sendNetworkUpdate: true, clampHealth: false);
		}
		else
		{
			OverrideMaxHealth(0f);
		}
	}

	private void RefreshCraftRange()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (craftTriggerCached && !((Object)(object)WorkbenchTrigger == (Object)null))
		{
			BoxCollider component = ((Component)WorkbenchTrigger).GetComponent<BoxCollider>();
			if (!((Object)(object)component == (Object)null))
			{
				float rangeMultiplier = GetRangeMultiplier();
				float num = originalCraftTriggerSize.z * (rangeMultiplier - 1f);
				component.size = new Vector3(originalCraftTriggerSize.x, originalCraftTriggerSize.y, originalCraftTriggerSize.z * rangeMultiplier);
				component.center = new Vector3(originalCraftTriggerCenter.x, originalCraftTriggerCenter.y, originalCraftTriggerCenter.z + num * 0.5f);
			}
		}
	}

	private void RefreshComfort()
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)upgradeComfortTrigger == (Object)null)
		{
			return;
		}
		float maxComfortOverride = GetMaxComfortOverride();
		bool flag = maxComfortOverride > 0f;
		((Component)upgradeComfortTrigger).gameObject.SetActive(flag);
		if (!flag)
		{
			return;
		}
		float num = ((originalComfortBaseValue >= 0f) ? originalComfortBaseValue : 0f);
		upgradeComfortTrigger.baseComfort = Mathf.Max(num, maxComfortOverride);
		if (originalComfortTriggerRadius > 0f)
		{
			float num2 = (ConVar.Workbench.scalecomfortradius ? GetRangeMultiplier() : 1f);
			float num3 = ((num2 > 1f) ? (num2 * ConVar.Workbench.comfortradiusscale) : 1f);
			if ((Object)(object)upgradeComfortCollider != (Object)null)
			{
				upgradeComfortCollider.radius = originalComfortTriggerRadius * num3;
				upgradeComfortTrigger.triggerSize = upgradeComfortCollider.radius * ((Component)upgradeComfortTrigger).transform.localScale.y;
			}
		}
	}

	private void RefreshServerPreventBuildingZones()
	{
		if (cachedUpgradeVisualPoints == null || cachedUpgradeVisualPoints.Length == 0)
		{
			return;
		}
		PooledHashSet<ItemDefinition> val = Pool.Get<PooledHashSet<ItemDefinition>>();
		try
		{
			for (int i = 0; i < cachedServerUpgrades.Count; i++)
			{
				((HashSet<ItemDefinition>)(object)val).Add(cachedServerUpgrades[i].item.info);
			}
			RefreshPreventBuildingZones((HashSet<ItemDefinition>)(object)val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private float GetMaxComfortOverride()
	{
		float num = 0f;
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			num = Mathf.Max(num, cachedServerUpgrades[i].mod.GetMinComfortLevel());
		}
		return num;
	}

	public void ApplyUpgradesToCraftedItem(BasePlayer crafter, ItemCraftTask task, Item craftedItem)
	{
		if (craftedItem != null)
		{
			for (int i = 0; i < cachedServerUpgrades.Count; i++)
			{
				CachedUpgrade cachedUpgrade = cachedServerUpgrades[i];
				cachedUpgrade.mod.ApplyToCraftedItem(this, crafter, task, craftedItem, cachedUpgrade.item);
			}
		}
	}

	public void GiveBonusItems(BasePlayer crafter, ItemCraftTask task, Item craftedItem)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		List<Item> list = Pool.Get<List<Item>>();
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			CachedUpgrade cachedUpgrade = cachedServerUpgrades[i];
			list.Clear();
			cachedUpgrade.mod.GetBonusItems(this, crafter, task, craftedItem, cachedUpgrade.item, list);
			foreach (Item item in list)
			{
				item.OnVirginSpawn(crafter);
				item.SetItemOwnership(crafter, ItemOwnershipPhrases.CraftedPhrase);
				if (!cachedUpgrade.mod.TryGiveBonusItem(this, crafter, cachedUpgrade.item, item) && !crafter.inventory.GiveItem(item))
				{
					item.Drop(crafter.inventory.containerMain.dropPosition, crafter.inventory.containerMain.dropVelocity);
				}
			}
		}
		Pool.FreeUnmanaged<Item>(ref list);
	}

	public void CollectBonusItems(BasePlayer crafter, ItemCraftTask task, Item craftedItem, List<Item> overflow, string ownerUsername, Phrase ownershipReason)
	{
		List<Item> list = Pool.Get<List<Item>>();
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			CachedUpgrade cachedUpgrade = cachedServerUpgrades[i];
			list.Clear();
			cachedUpgrade.mod.GetBonusItems(this, crafter, task, craftedItem, cachedUpgrade.item, list);
			foreach (Item item in list)
			{
				item.SetItemOwnership(ownerUsername, ownershipReason);
				if (!cachedUpgrade.mod.TryGiveBonusItem(this, crafter, cachedUpgrade.item, item))
				{
					overflow.Add(item);
				}
			}
		}
		Pool.FreeUnmanaged<Item>(ref list);
	}

	public void NotifyUpgradeInstalled(Item upgradeItem)
	{
		((upgradeItem != null) ? ((Component)upgradeItem.info).GetComponent<ItemModWorkbenchUpgrade>() : null)?.OnUpgradeInstalled(this, upgradeItem);
	}

	public void NotifyUpgradeRemoved(Item upgradeItem)
	{
		((upgradeItem != null) ? ((Component)upgradeItem.info).GetComponent<ItemModWorkbenchUpgrade>() : null)?.OnUpgradeRemoved(this, upgradeItem);
	}

	public float GetCraftSpeedMultiplier(ItemCraftTask task)
	{
		float num = 1f;
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			num *= cachedServerUpgrades[i].mod.GetCraftSpeedMultiplier(task);
		}
		return num;
	}

	public float GetRangeMultiplier()
	{
		float num = 1f;
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			num *= cachedServerUpgrades[i].mod.GetRangeMultiplier();
		}
		return num;
	}

	private void DoUpgradeEffect(Item upgradeItem)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		ItemModWorkbenchUpgrade itemModWorkbenchUpgrade = ((upgradeItem != null) ? ((Component)upgradeItem.info).GetComponent<ItemModWorkbenchUpgrade>() : null);
		if ((Object)(object)itemModWorkbenchUpgrade != (Object)null && itemModWorkbenchUpgrade.installEffectPrefab.isValid)
		{
			Effect.server.Run(itemModWorkbenchUpgrade.installEffectPrefab.resourcePath, ((Component)this).transform.position);
		}
	}

	private PooledList<ItemModWorkbenchUpgrade> GetInstalledUpgradeMods()
	{
		PooledList<ItemModWorkbenchUpgrade> val = Pool.Get<PooledList<ItemModWorkbenchUpgrade>>();
		if (base.isServer)
		{
			for (int i = 0; i < cachedServerUpgrades.Count; i++)
			{
				((List<ItemModWorkbenchUpgrade>)(object)val).Add(cachedServerUpgrades[i].mod);
			}
		}
		return val;
	}

	public bool HasTechTreeBypassUpgrade()
	{
		PooledList<ItemModWorkbenchUpgrade> installedUpgradeMods = GetInstalledUpgradeMods();
		try
		{
			for (int i = 0; i < ((List<ItemModWorkbenchUpgrade>)(object)installedUpgradeMods).Count; i++)
			{
				if (((List<ItemModWorkbenchUpgrade>)(object)installedUpgradeMods)[i].CanBypassTechTreePath())
				{
					return true;
				}
			}
			return false;
		}
		finally
		{
			((IDisposable)installedUpgradeMods)?.Dispose();
		}
	}

	public float GetTechTreeFailChance()
	{
		float num = 0f;
		PooledList<ItemModWorkbenchUpgrade> installedUpgradeMods = GetInstalledUpgradeMods();
		try
		{
			for (int i = 0; i < ((List<ItemModWorkbenchUpgrade>)(object)installedUpgradeMods).Count; i++)
			{
				num = Mathf.Max(num, ((List<ItemModWorkbenchUpgrade>)(object)installedUpgradeMods)[i].GetTechTreeFailChance());
			}
			return num;
		}
		finally
		{
			((IDisposable)installedUpgradeMods)?.Dispose();
		}
	}

	public float GetBypassCostMultiplier()
	{
		float num = 1f;
		PooledList<ItemModWorkbenchUpgrade> installedUpgradeMods = GetInstalledUpgradeMods();
		try
		{
			for (int i = 0; i < ((List<ItemModWorkbenchUpgrade>)(object)installedUpgradeMods).Count; i++)
			{
				num *= ((List<ItemModWorkbenchUpgrade>)(object)installedUpgradeMods)[i].GetBypassCostMultiplier();
			}
			return num;
		}
		finally
		{
			((IDisposable)installedUpgradeMods)?.Dispose();
		}
	}

	public IEnumerable<TechTreeData> GetTechTrees()
	{
		TechTreeData[] array = techTrees;
		foreach (TechTreeData techTreeData in array)
		{
			if (techTreeData.IsAllowedInEra(ConVar.Server.Era) && techTreeData.IsAllowedInGameMode(base.isServer))
			{
				yield return techTreeData;
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}

	public TechTreeData GetTechTreeForLevel(int level)
	{
		foreach (TechTreeData techTree in GetTechTrees())
		{
			if (techTree.techTreeLevel == level)
			{
				return techTree;
			}
		}
		return null;
	}

	public int GetScrapForExperiment()
	{
		if (Workbenchlevel == 1)
		{
			return 75;
		}
		if (Workbenchlevel == 2)
		{
			return 300;
		}
		if (Workbenchlevel == 3)
		{
			return 1000;
		}
		Debug.LogWarning((object)"GetScrapForExperiment fucked up big time.");
		return 0;
	}

	public bool IsWorking()
	{
		return HasFlag(Flags.On);
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (children.Count != 0)
		{
			pickupErrorToFormat = (format: PickupErrors.ItemHasAttachment, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		if (string.IsNullOrWhiteSpace(panelToOpen))
		{
			panelToOpen = (ConVar.Server.useLegacyWorkbenchInteraction ? "workbench" : "workbench_upgrades");
		}
		return base.PlayerOpenLoot(player, panelToOpen, doPositionChecks);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_OpenUpgradeInventory(RPCMessage msg)
	{
		if (isLootable && !Static)
		{
			BasePlayer player = msg.player;
			if (Object.op_Implicit((Object)(object)player) && player.CanInteract() && player.CanBuild())
			{
				PlayerOpenLoot(player, "workbench_upgrades");
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_TechTreeUnlock(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		int id = msg.read.Int32();
		int level = msg.read.Int32();
		bool flag = msg.read.Bit();
		TechTreeData techTreeForLevel = GetTechTreeForLevel(level);
		if ((Object)(object)techTreeForLevel == (Object)null || player.currentCraftLevel <= (float)techTreeForLevel.techTreeLevel || player.IsInTutorial || (flag && !HasTechTreeBypassUpgrade()))
		{
			return;
		}
		TechTreeData.NodeInstance byID = techTreeForLevel.GetByID(id);
		if (byID == null)
		{
			Debug.Log((object)("Node for unlock not found :" + id));
		}
		else
		{
			if (Interface.CallHook("OnTechTreeNodeUnlock", this, byID, player) != null)
			{
				return;
			}
			int itemid = ItemManager.FindItemDefinition("scrap").itemid;
			int amount = player.inventory.GetAmount(itemid);
			if (!flag)
			{
				PooledList<TechTreeData.NodeInstance> val = Pool.Get<PooledList<TechTreeData.NodeInstance>>();
				try
				{
					techTreeForLevel.GetNodesRequiredToUnlock(player, byID, (List<TechTreeData.NodeInstance>)(object)val);
					for (int num = ((List<TechTreeData.NodeInstance>)(object)val).Count - 1; num >= 0; num--)
					{
						TechTreeData.NodeInstance nodeInstance = ((List<TechTreeData.NodeInstance>)(object)val)[num];
						if ((Object)(object)nodeInstance.itemDef == (Object)null || player.blueprints.HasUnlocked(nodeInstance.itemDef))
						{
							((List<TechTreeData.NodeInstance>)(object)val).RemoveAt(num);
						}
					}
					PooledList<ItemDefinition> val2 = Pool.Get<PooledList<ItemDefinition>>();
					try
					{
						int num2 = 0;
						foreach (TechTreeData.NodeInstance item in (List<TechTreeData.NodeInstance>)(object)val)
						{
							if (item != null && !((Object)(object)item.itemDef == (Object)null))
							{
								num2 += ScrapForResearch(item.itemDef, techTreeForLevel.techTreeLevel, out var tax, this);
								num2 += tax;
								((List<ItemDefinition>)(object)val2).Add(item.itemDef);
							}
						}
						if (amount < num2)
						{
							return;
						}
						foreach (TechTreeData.NodeInstance item2 in (List<TechTreeData.NodeInstance>)(object)val)
						{
							if (!item2.IsGroup())
							{
								continue;
							}
							foreach (int output in item2.outputs)
							{
								TechTreeData.NodeInstance byID2 = techTreeForLevel.GetByID(output);
								if (byID2 != null && (Object)(object)byID2.itemDef != (Object)null)
								{
									player.blueprints.Unlock(byID2.itemDef);
									Facepunch.Rust.Analytics.Azure.OnBlueprintLearned(player, byID2.itemDef, "techtree", 0, this);
								}
							}
							Debug.Log((object)("Player unlocked group :" + item2.groupName));
						}
						player.inventory.Take(null, itemid, num2);
						player.blueprints.UnlockList((List<ItemDefinition>)(object)val2);
						Interface.CallHook("OnTechTreeNodeUnlocked", this, byID, player, val2);
						foreach (ItemDefinition item3 in (List<ItemDefinition>)(object)val2)
						{
							int num3 = ScrapForResearch(item3, techTreeForLevel.techTreeLevel, out var tax2, this);
							Facepunch.Rust.Analytics.Azure.OnBlueprintLearned(player, item3, "techtree", num3 + tax2, this);
						}
						return;
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			if ((Object)(object)byID.itemDef == (Object)null || player.blueprints.HasUnlocked(byID.itemDef))
			{
				return;
			}
			int num4 = Mathf.RoundToInt((float)(ScrapForResearch(byID.itemDef, techTreeForLevel.techTreeLevel, out var tax3, this) + tax3) * GetBypassCostMultiplier());
			if (amount >= num4)
			{
				player.inventory.Take(null, itemid, num4);
				float techTreeFailChance = GetTechTreeFailChance();
				if (Random.value < techTreeFailChance)
				{
					ClientRPC(RpcTarget.Player("RPC_PrototypeFailed", player));
					return;
				}
				player.blueprints.Unlock(byID.itemDef);
				Facepunch.Rust.Analytics.Azure.OnBlueprintLearned(player, byID.itemDef, "techtree_prototype", num4, this);
			}
		}
	}

	public static ItemDefinition GetBlueprintTemplate()
	{
		if ((Object)(object)blueprintBaseDef == (Object)null)
		{
			blueprintBaseDef = ItemManager.FindItemDefinition("blueprintbase");
		}
		return blueprintBaseDef;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_BeginExperiment(RPCMessage msg)
	{
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if ((Object)(object)player == (Object)null || IsWorking())
		{
			return;
		}
		PersistantPlayer persistantPlayerInfo = player.PersistantPlayerInfo;
		int num = Random.Range(0, experimentalItems.subSpawn.Length);
		for (int i = 0; i < experimentalItems.subSpawn.Length; i++)
		{
			int num2 = i + num;
			if (num2 >= experimentalItems.subSpawn.Length)
			{
				num2 -= experimentalItems.subSpawn.Length;
			}
			ItemDefinition itemDef = experimentalItems.subSpawn[num2].category.items[0].itemDef;
			if (Object.op_Implicit((Object)(object)itemDef.Blueprint) && !itemDef.Blueprint.defaultBlueprint && itemDef.Blueprint.userCraftable && itemDef.Blueprint.isResearchable && !itemDef.Blueprint.NeedsSteamItem && !itemDef.Blueprint.NeedsSteamDLC && !persistantPlayerInfo.unlockedItems.Contains(itemDef.itemid))
			{
				pendingBlueprint = itemDef;
				break;
			}
		}
		if ((Object)(object)pendingBlueprint == (Object)null)
		{
			player.ChatMessage("You have already unlocked everything for this workbench tier.");
		}
		else
		{
			if (Interface.CallHook("OnExperimentStart", this, player) != null)
			{
				return;
			}
			Item slot = base.inventory.GetSlot(0);
			if (slot != null)
			{
				if (!slot.MoveToContainer(player.inventory.containerMain))
				{
					slot.Drop(GetDropPosition(), GetDropVelocity());
				}
				player.inventory.loot.SendImmediate();
			}
			if (experimentStartEffect.isValid)
			{
				Effect.server.Run(experimentStartEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
			SetFlagLocal(Flags.On, b: true);
			base.inventory.SetLocked(isLocked: true);
			CancelInvoke(ExperimentComplete);
			Invoke(ExperimentComplete, 5f);
			SendNetworkUpdate();
			Interface.CallHook("OnExperimentStarted", this, player);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (base.inventory != null)
		{
			info.msg.workbench = Pool.Get<Workbench>();
			SaveUpgrades(info.msg.workbench);
		}
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		CancelInvoke(ExperimentComplete);
	}

	public int GetAvailableExperimentResources()
	{
		Item experimentResourceItem = GetExperimentResourceItem();
		if (experimentResourceItem == null || (Object)(object)experimentResourceItem.info != (Object)(object)experimentResource)
		{
			return 0;
		}
		return experimentResourceItem.amount;
	}

	public Item GetExperimentResourceItem()
	{
		return base.inventory.GetSlot(1);
	}

	public void ExperimentComplete()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		Item experimentResourceItem = GetExperimentResourceItem();
		int scrapForExperiment = GetScrapForExperiment();
		if ((Object)(object)pendingBlueprint == (Object)null)
		{
			Debug.LogWarning((object)"Pending blueprint was null!");
		}
		if (Interface.CallHook("OnExperimentEnd", this) != null)
		{
			return;
		}
		if (experimentResourceItem != null && experimentResourceItem.amount >= scrapForExperiment && (Object)(object)pendingBlueprint != (Object)null)
		{
			experimentResourceItem.UseItem(scrapForExperiment);
			Item item = ItemManager.Create(GetBlueprintTemplate(), 1, 0uL, isServerSide: true, 0uL);
			item.blueprintTarget = pendingBlueprint.itemid;
			creatingBlueprint = true;
			if (!item.MoveToContainer(base.inventory, 0))
			{
				item.Drop(GetDropPosition(), GetDropVelocity());
			}
			creatingBlueprint = false;
			if (experimentSuccessEffect.isValid)
			{
				Effect.server.Run(experimentSuccessEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
		}
		SetFlagLocal(Flags.On, b: false);
		pendingBlueprint = null;
		base.inventory.SetLocked(isLocked: false);
		SendNetworkUpdate();
		Interface.CallHook("OnExperimentEnded", this);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		if (base.inventory != null)
		{
			base.inventory.SetLocked(isLocked: false);
		}
		RebuildUpgradeCache();
		RefreshRuntimeUpgrades();
	}

	public override void ServerInit()
	{
		inventorySlots = Mathf.Max(inventorySlots, RequiredInventorySlots);
		base.ServerInit();
		base.inventory.canAcceptItem = ItemFilter;
		originalMaxHealth = startHealth;
		CacheOriginalTriggerValues();
	}

	private void CacheOriginalTriggerValues()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)WorkbenchTrigger != (Object)null && !craftTriggerCached)
		{
			BoxCollider component = ((Component)WorkbenchTrigger).GetComponent<BoxCollider>();
			if ((Object)(object)component != (Object)null)
			{
				originalCraftTriggerSize = component.size;
				originalCraftTriggerCenter = component.center;
				craftTriggerCached = true;
			}
		}
		if ((Object)(object)upgradeComfortTrigger != (Object)null && originalComfortBaseValue < 0f)
		{
			originalComfortBaseValue = upgradeComfortTrigger.baseComfort;
			if ((Object)(object)upgradeComfortCollider != (Object)null)
			{
				originalComfortTriggerRadius = upgradeComfortCollider.radius;
			}
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if (!Application.isLoadingSave && item != null && base.inventory != null && IsUpgradeSlot(item.position))
		{
			OnUpgradeAddedOrRemoved(item, added);
		}
	}

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		if ((targetSlot != 1 || !((Object)(object)item.info == (Object)(object)experimentResource)) && targetSlot == 0)
		{
			_ = creatingBlueprint;
		}
		if (IsUpgradeSlot(targetSlot))
		{
			if (Static)
			{
				return false;
			}
			Item slot = base.inventory.GetSlot(targetSlot);
			if (slot != null && slot.contents != null && !slot.contents.IsEmpty())
			{
				return false;
			}
			ItemModWorkbenchUpgrade component = ((Component)item.info).GetComponent<ItemModWorkbenchUpgrade>();
			if ((Object)(object)component != (Object)null && component.CanInstallInWorkbench(this, item, targetSlot))
			{
				return !IsUpgradeBlockedByClearanceZone(item.info);
			}
			return false;
		}
		return false;
	}

	public override PlayerInventory.CanMoveFromResponse CanMoveFrom(BasePlayer player, Item item)
	{
		PlayerInventory.CanMoveFromResponse result = base.CanMoveFrom(player, item);
		if (!result.allowed)
		{
			return result;
		}
		if (item.parent == base.inventory && IsUpgradeSlot(item.position) && item.contents != null && !item.contents.IsEmpty())
		{
			return PlayerInventory.CanMoveFromResponse.Failure(RecycleBinNotEmptyPhrase);
		}
		return result;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_OpenRecycleBin(RPCMessage msg)
	{
		if (!isLootable || Static)
		{
			return;
		}
		BasePlayer player = msg.player;
		if (!Object.op_Implicit((Object)(object)player) || !player.CanInteract())
		{
			return;
		}
		Item recycleBinUpgradeItem = GetRecycleBinUpgradeItem();
		if (recycleBinUpgradeItem?.contents != null)
		{
			if (IsLocked() || IsTransferring())
			{
				player.ShowToast(GameTip.Styles.Red_Normal, StorageContainer.LockedMessage, false);
			}
			else if (onlyOneUser && IsOpen())
			{
				player.ShowToast(GameTip.Styles.Red_Normal, StorageContainer.InUseMessage, false);
			}
			else if (CanOpenLootPanel(player, "generic_resizable") && player.inventory.loot.StartLootingEntity(this))
			{
				SetFlagLocal(Flags.Open, b: true);
				player.inventory.loot.AddContainer(recycleBinUpgradeItem.contents);
				player.inventory.loot.SendImmediate();
				player.ClientRPC(RpcTarget.Player("RPC_OpenLootPanel", player), "generic_resizable");
				SendNetworkUpdate();
			}
		}
	}

	private Item GetRecycleBinUpgradeItem()
	{
		for (int i = 2; i < RequiredInventorySlots; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot != null && (Object)(object)((Component)slot.info).GetComponent<ItemModWorkbenchRecycleBin>() != (Object)null)
			{
				return slot;
			}
		}
		return null;
	}

	public static int ScrapForResearch(ItemDefinition info, int workbenchLevel, out int tax, Workbench workbench = null)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		if ((int)info.rarity == 1)
		{
			num = 15;
		}
		if ((int)info.rarity == 2)
		{
			num = 30;
		}
		if ((int)info.rarity == 3)
		{
			num = 60;
		}
		if ((int)info.rarity == 4 || (int)info.rarity == 0)
		{
			num = 120;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if ((Object)(object)activeGameMode != (Object)null)
		{
			BaseGameMode.ResearchCostResult scrapCostForResearch = activeGameMode.GetScrapCostForResearch(info, ResearchTable.ResearchType.TechTree);
			if (scrapCostForResearch.Scale.HasValue)
			{
				num = Mathf.RoundToInt((float)num * scrapCostForResearch.Scale.Value);
			}
			else if (scrapCostForResearch.Amount.HasValue)
			{
				num = scrapCostForResearch.Amount.Value;
			}
		}
		float taxRateForWorkbenchUnlock = ConVar.Server.GetTaxRateForWorkbenchUnlock(workbenchLevel);
		tax = 0;
		if (taxRateForWorkbenchUnlock > 0f)
		{
			tax = Mathf.CeilToInt((float)num * (taxRateForWorkbenchUnlock / 100f));
		}
		if ((Object)(object)workbench != (Object)null)
		{
			float techTreeCostMultiplier = workbench.GetTechTreeCostMultiplier();
			num = Mathf.RoundToInt((float)num * techTreeCostMultiplier);
			tax = Mathf.RoundToInt((float)tax * techTreeCostMultiplier);
		}
		return num;
	}

	public override void ScaleDamage(HitInfo info)
	{
		base.ScaleDamage(info);
		if (base.inventory == null)
		{
			return;
		}
		for (int i = 2; i < RequiredInventorySlots; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot == null)
			{
				continue;
			}
			ItemModWorkbenchUpgrade component = ((Component)slot.info).GetComponent<ItemModWorkbenchUpgrade>();
			if (!((Object)(object)component == (Object)null))
			{
				float explosiveDamageReduction = component.GetExplosiveDamageReduction();
				if (!(explosiveDamageReduction <= 0f))
				{
					info.damageTypes.Scale(DamageType.Explosion, 1f - Mathf.Clamp01(explosiveDamageReduction));
				}
			}
		}
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	public Workbench()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		upgradeSlotCount = 4;
		cachedServerUpgrades = new List<CachedUpgrade>();
		originalCraftTriggerSize = Vector3.zero;
		originalCraftTriggerCenter = Vector3.zero;
		originalComfortBaseValue = -1f;
		originalComfortTriggerRadius = -1f;
		clientTechTreeMultiplier = 1f;
		base._002Ector();
	}

	static Workbench()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		RecycleBinNotEmptyPhrase = new Phrase("workbench.recyclebin.notempty", "Empty the recycle bin before removing it");
	}
}
