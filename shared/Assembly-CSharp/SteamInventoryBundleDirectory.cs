using System;
using UnityEngine;

public class SteamInventoryBundleDirectory : ScriptableObject
{
	[Serializable]
	public struct Bundle
	{
		public int id;

		public string name;

		public int[] contents;

		private SteamInventoryBundle _asset;

		public SteamInventoryBundle asset
		{
			get
			{
				if ((Object)(object)_asset == (Object)null && !string.IsNullOrEmpty(name))
				{
					_asset = FileSystem.Load<SteamInventoryBundle>(name, true);
				}
				return _asset;
			}
		}
	}

	private static SteamInventoryBundleDirectory _Instance;

	public Bundle[] bundles;

	public static SteamInventoryBundleDirectory Instance
	{
		get
		{
			if ((Object)(object)_Instance == (Object)null)
			{
				_Instance = FileSystem.Load<SteamInventoryBundleDirectory>("assets/steamskinbundles.asset", false);
			}
			return _Instance;
		}
	}

	public static bool TryGet(int definitionId, out Bundle bundle)
	{
		bundle = default;
		SteamInventoryBundleDirectory instance = Instance;
		if ((Object)(object)instance == (Object)null || instance.bundles == null)
		{
			return false;
		}
		Bundle[] array = instance.bundles;
		for (int i = 0; i < array.Length; i++)
		{
			Bundle bundle2 = array[i];
			if (bundle2.id == definitionId)
			{
				bundle = bundle2;
				return true;
			}
		}
		return false;
	}
}
