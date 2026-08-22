using System.Collections.Generic;
using System.Linq;
using Carbon;
using Carbon.Core;
using Oxide.Core.Plugins;
using Oxide.Plugins;

public class PluginManager
{
	public string ConfigPath => Defines.GetConfigsFolder();

	public event PluginEvent OnPluginAdded;

	public event PluginEvent OnPluginRemoved;

	public bool AddPlugin(RustPlugin plugin)
	{
		OnPluginAdded?.Invoke(plugin);
		ModLoader.Package package = plugin.Package;
		if (!package.IsValid || package.Plugins == null || package.Plugins.Contains(plugin))
		{
			return false;
		}
		package.AddPlugin(plugin);
		return true;
	}

	public bool RemovePlugin(RustPlugin plugin)
	{
		OnPluginRemoved?.Invoke(plugin);
		ModLoader.Package package = plugin.Package;
		if (!package.IsValid || package.Plugins == null || !package.Plugins.Contains(plugin))
		{
			return false;
		}
		package.RemovePlugin(plugin);
		return true;
	}

	public Plugin GetPlugin(string name)
	{
		if (name == "RustCore")
		{
			return Community.Runtime.Core;
		}
		return Community.Runtime.Plugins.FindPlugin(name);
	}

	public IEnumerable<Plugin> GetPlugins()
	{
		return Community.Runtime.Plugins.Plugins.AsEnumerable();
	}
}
