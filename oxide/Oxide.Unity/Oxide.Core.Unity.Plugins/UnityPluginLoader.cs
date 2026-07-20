using System;
using Oxide.Core.Plugins;

namespace Oxide.Core.Unity.Plugins;

public class UnityPluginLoader : PluginLoader
{
	public override Type[] CorePlugins => new Type[1] { typeof(UnityCore) };
}
