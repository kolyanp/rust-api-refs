using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Carbon.Components;

public class ComponentCacheBank<T> : Dictionary<GameObject, List<T>>, IComponentBank where T : MonoBehaviour
{
	public static ComponentCacheBank<T> Instance { get; }

	static ComponentCacheBank()
	{
		Instance = new ComponentCacheBank<T>();
		ComponentCacheBankNonGeneric.All.Add(Instance);
	}

	public T Add(GameObject go)
	{
		if (!TryGetValue(go, out var value))
		{
			value = (base[go] = new List<T>());
		}
		T val = go.AddComponent<T>();
		value.Add(val);
		return val;
	}

	public T Get(GameObject go)
	{
		if (!TryGetValue(go, out var value))
		{
			value = (base[go] = new List<T>());
		}
		T val = value.FirstOrDefault((T x) => (Object)(object)x != (Object)null);
		if ((Object)(object)val != (Object)null)
		{
			return val;
		}
		if (!go.TryGetComponent<T>(ref val))
		{
			return default(T);
		}
		value.Add(val);
		return val;
	}

	public bool Remove(GameObject go, bool destroy = true)
	{
		if (!TryGetValue(go, out var value))
		{
			return false;
		}
		int num = value.RemoveAll(delegate(T x)
		{
			if (destroy)
			{
				Object.DestroyImmediate((Object)(object)x);
			}
			return true;
		});
		if (num > 0)
		{
			return base.Remove(go);
		}
		return false;
	}

	public bool Destroy(GameObject go)
	{
		if (!TryGetValue(go, out var value))
		{
			Object.Destroy((Object)(object)go);
			return false;
		}
		value.Clear();
		Remove(go, destroy: false);
		Object.Destroy((Object)(object)go);
		return true;
	}
}
