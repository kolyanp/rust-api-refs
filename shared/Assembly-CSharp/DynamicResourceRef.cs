using System;
using UnityEngine;

[Serializable]
public class DynamicResourceRef<T> : DynamicResourceRefBase where T : Object
{
	public T Load()
	{
		return FileSystem.Backend.LoadAssetFromDynamicBundle<T>(BundleName, Path);
	}
}
