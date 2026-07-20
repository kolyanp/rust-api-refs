using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Entity Colour Swap Lookup")]
public class EntityColorSwapLookup : BaseScriptableObject
{
	[Serializable]
	public class EntityColorData
	{
		public BaseEntityRef entityPrefab;

		public string colorDatasetIdentifier;
	}

	[Serializable]
	public class ColorDataset
	{
		public string identifier;

		[InspectorName("Client ConVar")]
		public string clientConVar;

		public ColorDataEntry[] colorDataEntries = Array.Empty<ColorDataEntry>();
	}

	[Serializable]
	public class ColorDataEntry
	{
		public Phrase colorNamePhrase;

		public Phrase colorDescriptionPhrase;

		public Color materialColor;

		public Color radialSelectMenuColor;
	}

	private static EntityColorSwapLookup _instance;

	[SerializeField]
	private EntityColorData[] entityEntries = Array.Empty<EntityColorData>();

	[SerializeField]
	private ColorDataset[] colorDatasets = Array.Empty<ColorDataset>();

	private Dictionary<uint, ColorDataset> _entityColorDataLookup;

	private Dictionary<string, ColorDataset> _colorDatasetLookup;

	public static EntityColorSwapLookup instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FileSystem.Load<EntityColorSwapLookup>("assets/content/config/entitycolourswaplookup.asset", true);
			}
			if (_instance == null)
			{
				Debug.LogError((object)"Failed to load EntityColorSwapLookup");
			}
			return _instance;
		}
	}

	private Dictionary<uint, ColorDataset> entityColorDataLookup
	{
		get
		{
			if (_entityColorDataLookup == null)
			{
				_entityColorDataLookup = new Dictionary<uint, ColorDataset>();
				EntityColorData[] array = entityEntries;
				foreach (EntityColorData entityColorData in array)
				{
					if (!entityColorData.entityPrefab.isValid)
					{
						continue;
					}
					BaseEntity baseEntity = entityColorData.entityPrefab.Get();
					if (!((Object)(object)baseEntity == (Object)null))
					{
						if (!colorDatasetLookup.TryGetValue(entityColorData.colorDatasetIdentifier, out var value))
						{
							Debug.LogError((object)("Failed to retrieve a colour dataset for identifier: (" + entityColorData.colorDatasetIdentifier + ") for deployable prefab " + ((Object)baseEntity).name + " in EntityColorSwapLookup"), (Object)(object)this);
						}
						else
						{
							_entityColorDataLookup.Add(baseEntity.prefabID, value);
						}
					}
				}
			}
			return _entityColorDataLookup;
		}
	}

	private Dictionary<string, ColorDataset> colorDatasetLookup
	{
		get
		{
			if (_colorDatasetLookup == null)
			{
				_colorDatasetLookup = new Dictionary<string, ColorDataset>();
				ColorDataset[] array = colorDatasets;
				foreach (ColorDataset colorDataset in array)
				{
					if (!_colorDatasetLookup.TryAdd(colorDataset.identifier, colorDataset))
					{
						Debug.LogError((object)("Double color dataset identifier: (" + colorDataset.identifier + ") in EntityColorSwapLookup"), (Object)(object)this);
					}
				}
			}
			return _colorDatasetLookup;
		}
	}

	public bool EntityHasColorData(BaseEntity entity)
	{
		return entityColorDataLookup.ContainsKey(entity.prefabID);
	}

	public bool TryGetEntityColorDataset(BaseEntity decorDeployable, out ColorDataset colorDataset)
	{
		return entityColorDataLookup.TryGetValue(decorDeployable.prefabID, out colorDataset);
	}

	public bool TryGetColorDataset(string identifier, out ColorDataset colorDataset)
	{
		return colorDatasetLookup.TryGetValue(identifier, out colorDataset);
	}
}
