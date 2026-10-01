using System;
using UnityEngine;

[Serializable]
public class BaseEntityRef : ResourceRef<BaseEntity>
{
	public override BaseEntity Get()
	{
		if ((Object)(object)_cachedObject != (Object)null)
		{
			return _cachedObject;
		}
		Object val = GameManifest.GUIDToObject(guid);
		GameObject val2 = (GameObject)(object)((val is GameObject) ? val : null);
		BaseEntity baseEntity = null;
		if ((Object)(object)val2 != (Object)null && val2.TryGetComponent<BaseEntity>(ref baseEntity))
		{
			_cachedObject = baseEntity;
		}
		return baseEntity;
	}
}
