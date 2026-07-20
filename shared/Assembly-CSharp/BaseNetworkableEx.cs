using UnityEngine;

public static class BaseNetworkableEx
{
	public static bool IsValid(this BaseNetworkable ent)
	{
		if ((Object)(object)ent == (Object)null)
		{
			return false;
		}
		if (ent.net == null)
		{
			return false;
		}
		return true;
	}

	public static bool Is<T>(this Object unityObject, out T castedUnityObject) where T : Object
	{
		castedUnityObject = default(T);
		if (unityObject == (Object)null)
		{
			return false;
		}
		castedUnityObject = (T)(object)((unityObject is T) ? unityObject : null);
		if ((Object)(object)castedUnityObject == (Object)null)
		{
			return false;
		}
		return true;
	}

	public static bool IsRealNull(this BaseNetworkable ent)
	{
		return ent == null;
	}
}
