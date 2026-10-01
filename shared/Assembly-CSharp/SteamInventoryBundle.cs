using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Skins/Steam Bundle")]
public class SteamInventoryBundle : SteamInventoryAsset
{
	[Serializable]
	public struct Entry
	{
		public SteamInventoryItem item;

		[Min(1f)]
		public int quantity;
	}

	[Tooltip("Items granted on purchase")]
	public Entry[] items = Array.Empty<Entry>();
}
