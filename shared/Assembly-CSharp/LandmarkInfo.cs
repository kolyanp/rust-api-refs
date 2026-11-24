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
			if (!World.TryGetSpawnedPrefabCategory(((Component)root).gameObject, out var category))
			{
				return ((Object)root).name;
			}
			return category;
		}
	}
}
