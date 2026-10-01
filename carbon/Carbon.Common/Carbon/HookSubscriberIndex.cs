using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Carbon.Base;
using Carbon.Base.Interfaces;
using Carbon.Core;
using Facepunch;
using Oxide.Plugins;

namespace Carbon;

public static class HookSubscriberIndex
{
	private const BindingFlags DefaultFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly Dictionary<uint, BaseHookable[]> _index = new Dictionary<uint, BaseHookable[]>();

	private static int _version;

	private static int _builtVersion = -1;

	public static int Version => _version;

	public static int BuiltVersion => _builtVersion;

	public static IReadOnlyDictionary<uint, BaseHookable[]> Current => _index;

	public static void Invalidate()
	{
		Interlocked.Increment(ref _version);
	}

	public static BaseHookable[] Get(uint hookId)
	{
		return Get(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static BaseHookable[] Get(uint hookId, BindingFlags flag)
	{
		if (_builtVersion != _version)
		{
			Refresh(flag);
		}
		if (!_index.TryGetValue(hookId, out var value))
		{
			value = (_index[hookId] = Collect(hookId));
		}
		return value;
	}

	private static void Refresh(BindingFlags flag)
	{
		_index.Clear();
		List<BaseHookable> modules = Community.Runtime.ModuleProcessor.Modules;
		for (int i = 0; i < modules.Count; i++)
		{
			BaseHookable baseHookable = modules[i];
			if (!(baseHookable is IModule module) || module.IsEnabled())
			{
				EnsureCache(baseHookable, flag);
			}
		}
		ModLoader.PackageBank packages = ModLoader.Packages;
		for (int j = 0; j < packages.Count; j++)
		{
			List<RustPlugin> plugins = packages[j].Plugins;
			if (plugins != null)
			{
				for (int k = 0; k < plugins.Count; k++)
				{
					EnsureCache(plugins[k], flag);
				}
			}
		}
		_builtVersion = _version;
	}

	private static void EnsureCache(BaseHookable hookable, BindingFlags flag)
	{
		if (hookable.HasBuiltHookCache || hookable.HookableType == null || hookable.Hooks == null)
		{
			return;
		}
		try
		{
			hookable.BuildHookCache(flag);
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed building hook cache for '{hookable.Name} v{hookable.Version}'", ex);
		}
	}

	private static BaseHookable[] Collect(uint hookId)
	{
		List<BaseHookable> list = Pool.Get<List<BaseHookable>>();
		List<BaseHookable> modules = Community.Runtime.ModuleProcessor.Modules;
		for (int i = 0; i < modules.Count; i++)
		{
			BaseHookable baseHookable = modules[i];
			if (baseHookable.HookPool != null && baseHookable.HookPool.ContainsKey(hookId))
			{
				list.Add(baseHookable);
			}
		}
		ModLoader.PackageBank packages = ModLoader.Packages;
		for (int j = 0; j < packages.Count; j++)
		{
			List<RustPlugin> plugins = packages[j].Plugins;
			if (plugins == null)
			{
				continue;
			}
			for (int k = 0; k < plugins.Count; k++)
			{
				RustPlugin rustPlugin = plugins[k];
				if (rustPlugin.HookPool != null && rustPlugin.HookPool.ContainsKey(hookId))
				{
					list.Add(rustPlugin);
				}
			}
		}
		BaseHookable[] result = ((list.Count == 0) ? Array.Empty<BaseHookable>() : list.ToArray());
		Pool.FreeUnmanaged<BaseHookable>(ref list);
		return result;
	}
}
