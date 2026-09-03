using System;
using System.Collections.Generic;
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

	bool IPrefabPreProcess.CanRunDuringBundling => true;

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (bundling || !FileSystem.IsBundled)
		{
			EntityNameToItemDefinitions = BuildEntityNameToItemDefinitionTable();
		}
	}

	private static EntityNameToItemDefinition[] BuildEntityNameToItemDefinitionTable()
	{
		List<ItemDefinition> list = GetItemList();
		Dictionary<string, ItemDefinition> dictionary = new Dictionary<string, ItemDefinition>(StringComparer.OrdinalIgnoreCase);
		ItemModEntity itemModEntity = default(ItemModEntity);
		ThrownWeapon thrownWeapon = default(ThrownWeapon);
		ItemModDeployable itemModDeployable = default(ItemModDeployable);
		foreach (ItemDefinition item in list)
		{
			dictionary.TryAdd(item.shortname, item);
			try
			{
				if (((Component)item).TryGetComponent<ItemModEntity>(ref itemModEntity) && itemModEntity.entityPrefab.isValid)
				{
					GameObject val = itemModEntity.entityPrefab.Get();
					dictionary.TryAdd(((Object)val).name, item);
					if (val.TryGetComponent<ThrownWeapon>(ref thrownWeapon) && thrownWeapon.prefabToThrow.isValid)
					{
						dictionary.TryAdd(((Object)thrownWeapon.prefabToThrow.Get()).name, item);
					}
				}
				if (((Component)item).TryGetComponent<ItemModDeployable>(ref itemModDeployable) && itemModDeployable.entityPrefab != null && itemModDeployable.entityPrefab.isValid)
				{
					dictionary.TryAdd(((Object)itemModDeployable.entityPrefab.Get()).name, item);
				}
				ItemModProjectile[] components = ((Component)item).GetComponents<ItemModProjectile>();
				foreach (ItemModProjectile itemModProjectile in components)
				{
					if (itemModProjectile.projectileObject != null && itemModProjectile.projectileObject.isValid)
					{
						dictionary.TryAdd(((Object)itemModProjectile.projectileObject.Get()).name, item);
					}
					if (itemModProjectile.SkinOverrides == null)
					{
						continue;
					}
					ItemModProjectile.SkinOverride[] skinOverrides = itemModProjectile.SkinOverrides;
					for (int j = 0; j < skinOverrides.Length; j++)
					{
						ItemModProjectile.SkinOverride skinOverride = skinOverrides[j];
						if ((Object)(object)skinOverride.OverrideProjectile.Get() != (Object)null)
						{
							dictionary.TryAdd(((Object)skinOverride.OverrideProjectile.Get()).name, item);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("Invalid entity found in LifeInfographic.PreProcess - likely a corrupt prefab: " + item.shortname));
				Debug.LogError((object)ex);
			}
		}
		return dictionary.Select((KeyValuePair<string, ItemDefinition> kvp) => new EntityNameToItemDefinition
		{
			Name = kvp.Key,
			ItemDefinition = kvp.Value
		}).OrderBy<EntityNameToItemDefinition, string>((EntityNameToItemDefinition x) => x.Name, StringComparer.OrdinalIgnoreCase).ToArray();
		static List<ItemDefinition> GetItemList()
		{
			return ItemManager.itemList;
		}
	}
}
