using System;
using Oxide.Core.Plugins;

namespace Oxide.Game.Rust;

public class RustPluginLoader : PluginLoader
{
	public override Type[] CorePlugins => new Type[1] { typeof(RustCore) };
}
