using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.UI;

public class LifeInfographic : SingletonComponent<LifeInfographic>, IPrefabPreProcess
{
	[Serializable]
	public struct DamageSetting
	{
		public DamageType ForType;

		public string Display;

		public Sprite DamageSprite;
	}

	[Serializable]
	public struct EntityNameToItemDefinition
	{
		public string Name;

		public ItemDefinition ItemDefinition;
	}

	[NonSerialized]
	public PlayerLifeStory life;

	public GameObject container;

	public RawImage AttackerAvatarImage;

	public Image DamageSourceImage;

	public LifeInfographicStat[] Stats;

	public Animator[] AllAnimators;

	public GameObject WeaponRoot;

	public GameObject DistanceRoot;

	public GameObject DistanceDivider;

	public Image WeaponImage;

	public DamageSetting[] DamageDisplays;

	public Texture2D defaultAvatarTexture;

	public bool ShowDebugData;

	[ReadOnly]
	[Tooltip("Automatically filled in by prefab preprocess")]
	public EntityNameToItemDefinition[] EntityNameToItemDefinitions;

	bool IPrefabPreProcess.CanPreProcessDuringBundling => true;

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (!PrefabPreProcess.IsProcessingBundledPrefab)
		{
			EntityNameToItemDefinitions = BuildEntityNameToItemDefinitionTable();
		}
	}

	private static EntityNameToItemDefinition[] BuildEntityNameToItemDefinitionTable()
	{
		List<ItemDefinition> list = GetItemList();
		if (list == null)
		{
			Debug.LogWarning((object)"LifeInfographic.PreProcess ran before ItemManager.Initialize, so the death screen will be unable to fill out item info");
			return Array.Empty<EntityNameToItemDefinition>();
		}
		Dictionary<string, ItemDefinition> entityNameToItemDefinitions = new Dictionary<string, ItemDefinition>(StringComparer.OrdinalIgnoreCase);
		ItemModEntity itemModEntity = default;
		ItemModDeployable itemModDeployable = default;
		foreach (ItemDefinition item in list)
		{
			entityNameToItemDefinitions.TryAdd(item.shortname, item);
			if (((Component)item).TryGetComponent<ItemModEntity>(ref itemModEntity))
			{
				AddEntityName(itemModEntity.entityPrefab, item);
			}
			if (((Component)item).TryGetComponent<ItemModDeployable>(ref itemModDeployable))
			{
				AddEntityName(itemModDeployable.entityPrefab, item);
			}
			ItemModProjectile[] components = ((Component)item).GetComponents<ItemModProjectile>();
			foreach (ItemModProjectile itemModProjectile in components)
			{
				AddEntityName(itemModProjectile.projectileObject, item);
				if (itemModProjectile.SkinOverrides != null)
				{
					ItemModProjectile.SkinOverride[] skinOverrides = itemModProjectile.SkinOverrides;
					for (int j = 0; j < skinOverrides.Length; j++)
					{
						AddEntityName(skinOverrides[j].OverrideProjectile, item);
					}
				}
			}
		}
		return entityNameToItemDefinitions.Select((KeyValuePair<string, ItemDefinition> kvp) => new EntityNameToItemDefinition
		{
			Name = kvp.Key,
			ItemDefinition = kvp.Value
		}).OrderBy((EntityNameToItemDefinition x) => x.Name, StringComparer.OrdinalIgnoreCase).ToArray();
		void AddEntityName(GameObjectRef entityPrefab, ItemDefinition itemDefinition)
		{
			if (entityPrefab != null && entityPrefab.isValid)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(entityPrefab.resourcePath);
				if (string.IsNullOrEmpty(fileNameWithoutExtension))
				{
					Debug.LogWarning((object)("Invalid entity found in LifeInfographic.PreProcess - " + itemDefinition.shortname + " references a missing prefab (" + entityPrefab.guid + ")"));
				}
				else
				{
					entityNameToItemDefinitions.TryAdd(fileNameWithoutExtension, itemDefinition);
				}
			}
		}
		static List<ItemDefinition> GetItemList()
		{
			return ItemManager.itemList;
		}
	}
}
