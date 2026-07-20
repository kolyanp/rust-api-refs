using System.Collections.Generic;
using Oxide.Core.Plugins;

namespace Oxide.Core.Extensions;

public class ExtensionManager
{
	internal static List<Extension> extensionCache = new List<Extension>();

	private List<PluginLoader> pluginloaders = new List<PluginLoader>();

	public IEnumerable<PluginLoader> GetPluginLoaders()
	{
		return pluginloaders;
	}

	public void RegisterPluginLoader(PluginLoader loader)
	{
		pluginloaders.Add(loader);
	}
}
