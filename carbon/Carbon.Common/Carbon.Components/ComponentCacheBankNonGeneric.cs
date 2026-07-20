using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Carbon.Components;

public static class ComponentCacheBankNonGeneric
{
	public static List<IComponentBank> All = new List<IComponentBank>();

	public static T AddComponentCache<T>(this GameObject go) where T : MonoBehaviour
	{
		if (!((Object)(object)go == (Object)null))
		{
			return ComponentCacheBank<T>.Instance.Add(go);
		}
		return default(T);
	}

	public static T GetComponentCache<T>(this GameObject go) where T : MonoBehaviour
	{
		if (!((Object)(object)go == (Object)null))
		{
			return ComponentCacheBank<T>.Instance.Get(go);
		}
		return default(T);
	}

	public static bool RemoveComponentCache<T>(this GameObject go) where T : MonoBehaviour
	{
		if (!((Object)(object)go == (Object)null))
		{
			return ComponentCacheBank<T>.Instance.Remove(go);
		}
		return false;
	}

	public static bool TryGetOrAddComponentCache<T>(this GameObject go, out T component) where T : MonoBehaviour
	{
		T obj = go.GetComponentCache<T>() ?? go.AddComponentCache<T>();
		T val = obj;
		component = obj;
		return (Object)(object)val != (Object)null;
	}

	public static bool DestroyCache(this GameObject go)
	{
		if ((Object)(object)go == (Object)null)
		{
			return false;
		}
		return All.Count((IComponentBank cache) => cache.Destroy(go)) > 0;
	}

	public static void OnEntityDestruct(BaseEntity entity)
	{
		if ((Object)(object)entity == (Object)null || (Object)(object)((Component)entity).gameObject == (Object)null)
		{
			return;
		}
		foreach (IComponentBank item in All)
		{
			item.Remove(((Component)entity).gameObject);
		}
	}
}
