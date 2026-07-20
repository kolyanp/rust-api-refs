using System;
using System.Collections.Generic;
using System.Reflection;
using API.Abstracts;
using API.Assembly;
using Loaders;
using UnityEngine;

namespace Components;

internal abstract class AddonManager : CarbonBehaviour, IAddonManager
{
	internal class Item
	{
		public byte[] PostProcessedRaw { get; internal set; }

		public ICarbonAddon Addon { get; internal set; }

		public IReadOnlyList<Type> Types { get; internal set; }

		public IReadOnlyList<Type> Shared { get; internal set; }

		public string File { get; internal set; }
	}

	internal readonly AssemblyLoader _loader = new AssemblyLoader();

	internal IAssemblyManager AssemblyManager => ((Component)this).GetComponentInParent<IAssemblyManager>();

	internal List<Item> _loaded { get; set; } = new List<Item>();

	public WatchFolder Watcher { get; internal set; }

	public IReadOnlyDictionary<Type, KeyValuePair<string, byte[]>> Loaded
	{
		get
		{
			Dictionary<Type, KeyValuePair<string, byte[]>> dictionary = new Dictionary<Type, KeyValuePair<string, byte[]>>();
			foreach (Item item in _loaded)
			{
				foreach (Type type in item.Types)
				{
					if (!dictionary.ContainsKey(type))
					{
						dictionary.Add(type, new KeyValuePair<string, byte[]>(item.File, item.PostProcessedRaw));
					}
				}
			}
			return dictionary;
		}
	}

	public IReadOnlyDictionary<Type, string> Shared
	{
		get
		{
			Dictionary<Type, string> dictionary = new Dictionary<Type, string>();
			foreach (Item item in _loaded)
			{
				foreach (Type item2 in item.Shared)
				{
					if (!dictionary.ContainsKey(item2))
					{
						dictionary.Add(item2, item.File);
					}
				}
			}
			return dictionary;
		}
	}

	public byte[] Read(string file)
	{
		return _loader.ReadFromCache(file).Raw;
	}

	public abstract Assembly Load(string file, string requester);

	public abstract void Unload(string file, string requester);

	internal virtual void Hydrate(Assembly assembly, ICarbonAddon addon)
	{
	}
}
