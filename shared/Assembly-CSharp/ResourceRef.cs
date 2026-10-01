using System;
using UnityEngine;

[Serializable]
public class ResourceRef<T> where T : Object
{
	public string guid;

	protected T _cachedObject;

	public bool isValid => !string.IsNullOrEmpty(guid);

	public string resourcePath => GameManifest.GUIDToPath(guid);

	public uint resourceID => StringPool.Get(resourcePath);

	public virtual T Get()
	{
		if ((Object)(object)_cachedObject == (Object)null)
		{
			Object val = GameManifest.GUIDToObject(guid);
			_cachedObject = (T)(object)((val is T) ? val : null);
		}
		return _cachedObject;
	}
}
