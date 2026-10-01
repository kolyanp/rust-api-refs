using System.Collections.Generic;
using UnityEngine;

public class LandmarkInfo : MonoBehaviour
{
	[Header("LandmarkInfo")]
	public bool shouldDisplayOnMap;

	public bool isLayerSpecific;

	public Phrase displayPhrase;

	public Sprite mapIcon;

	public bool isDynamic;

	private string _fallbackNameCache;

	public virtual MapLayer MapLayer => MapLayer.Overworld;

	protected virtual void Awake()
	{
		if (!isDynamic && Object.op_Implicit((Object)(object)TerrainMeta.Path))
		{
			TerrainMeta.Path.Landmarks.Add(this);
		}
	}

	public string GetFallbackName()
	{
		if (_fallbackNameCache != null)
		{
			return _fallbackNameCache;
		}
		_fallbackNameCache = GetFallbackNameImpl();
		return _fallbackNameCache;
		string GetFallbackNameImpl()
		{
			Transform root = ((Component)this).transform.root;
			GameObject gameObject = ((Component)root).gameObject;
			foreach (var (result, hashSet2) in World.SpawnedPrefabs)
			{
				if (hashSet2.Contains(gameObject))
				{
					return result;
				}
			}
			return ((Object)root).name;
		}
	}
}
