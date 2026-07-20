using System;
using API.Hooks;
using Carbon.Core;
using Carbon.Managers;
using Carbon.Modules;
using Oxide.Core;
using Oxide.Core.Plugins;

namespace Carbon.Hooks;

public class Category_Plugin
{
	public class Plugin_Init
	{
		[Patch("Init", "Init [Instance]", typeof(ModLoader), "InitializePlugin", null)]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Plugin")]
		[Info("Gets called right after config and lang phrases are read.")]
		[OxideCompatible]
		public class Init : Patch
		{
		}
	}

	public class Plugin_LoadDefaultMessages
	{
		[Patch("LoadDefaultMessages", "LoadDefaultMessages [Instance]", typeof(ModLoader), "InitializePlugin", null)]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Plugin")]
		[Info("Gets called when originally there aren't message found in the `carbon/lang` folder.")]
		[Info("Gets called when it initially creates the `carbon/lang` files for the plugin.")]
		[OxideCompatible]
		public class LoadDefaultMessages : Patch
		{
		}
	}

	public class Plugin_Loaded
	{
		[Patch("Loaded", "Loaded [Instance]", typeof(ModLoader), "InitializePlugin", null)]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Plugin")]
		[Info("Gets called when the plugin executes the Load method on the plugin.")]
		[OxideCompatible]
		public class Loaded : Patch
		{
		}
	}

	public class Plugin_OnLoaded
	{
		[Patch("OnLoaded", "OnLoaded [Instance]", typeof(ModLoader), "InitializePlugin", null)]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Plugin")]
		[Info("Gets called when the plugin executes the Load method on the plugin.")]
		[OxideCompatible]
		public class OnLoaded : Patch
		{
		}
	}

	public class Plugin_Outdated
	{
		[Patch("OnPluginCompileFailure", "OnPluginCompileFailure", typeof(ScriptLoader), "Compile")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Plugin")]
		[Info("Gets called whenever a plugin compilation fails.")]
		[Parameter("pluginName", typeof(string), false)]
		[Parameter("error", typeof(Exception), false)]
		public class OnPluginCompileFailure : Patch
		{
		}

		[Patch("OnPluginOutdated", "OnPluginOutdated", typeof(Vendor), "VersionCheck")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Plugin")]
		[Info("Gets called whenever a plugin from the Admin module -> Plugins tab is marked as 'auto-updateable' and is outdated.")]
		[Parameter("pluginName", typeof(string), false)]
		[Parameter("currentVersion", typeof(VersionNumber), false)]
		[Parameter("newVersion", typeof(VersionNumber), false)]
		[Parameter("plugin", typeof(Plugin), false)]
		[Parameter("vendorName", typeof(string), false)]
		public class OnPluginOutdated : Patch
		{
		}
	}

	public class Plugin_Unload
	{
		[Patch("Unload", "Unload [Instance]", typeof(ModLoader), "InitializePlugin", null)]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Plugin")]
		[Info("Gets called when the plugin has fully shut down and disposed.")]
		[OxideCompatible]
		public class Unload : Patch
		{
		}
	}
}
