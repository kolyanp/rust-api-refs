using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using API.Abstracts;
using API.Commands;
using API.Events;
using Carbon.Components;
using Carbon.Core;
using Carbon.Extensions;
using Carbon.Hooks;
using Carbon.Jobs;
using Carbon.Managers;
using Carbon.Test;
using ConVar;
using Facepunch;
using Oxide.Core;
using Rust;
using UnityEngine;

namespace Carbon;

public class CommunityInternal : Community
{
	public static CommunityInternal InternalRuntime
	{
		get
		{
			return Community.Runtime as CommunityInternal;
		}
		set
		{
			Community.Runtime = value;
		}
	}

	public bool IsInitialized { get; set; }

	public override void ReloadPlugins(IEnumerable<string> except = null)
	{
		base.ReloadPlugins(except);
		ScriptLoader.LoadAll(except);
	}

	internal void _installCore()
	{
		Community runtime = Community.Runtime;
		CorePlugin core = (base.Core = new CorePlugin());
		runtime.Core = core;
		base.Core.Setup("Core", "Carbon Community", new VersionNumber(1, 0, 0), string.Empty);
		ModLoader.ProcessPrecompiledType(base.Core);
		CorePlugin core2 = base.Core;
		bool isCorePlugin = (base.Core.IsPrecompiled = true);
		core2.IsCorePlugin = isCorePlugin;
		base.Core.IInit();
		base.Core.ILoadDefaultMessages();
		ModLoader.RegisterPackage(base.Core.Package = ModLoader.Package.Get("Carbon Community", isCoreMod: true).AddPlugin(base.Core));
		ModLoader.Package package = (base.Plugins = ModLoader.Package.Get("Scripts", isCoreMod: false));
		ModLoader.RegisterPackage(package);
		package = (base.ZipPlugins = ModLoader.Package.Get("Zip Scripts", isCoreMod: false));
		ModLoader.RegisterPackage(package);
		ModLoader.ProcessCommands(typeof(CorePlugin), base.Core, BindingFlags.Instance | BindingFlags.NonPublic, "c");
		ModLoader.ProcessCommands(typeof(CorePlugin), base.Core, BindingFlags.Instance | BindingFlags.NonPublic, "carbon", hidden: true);
		int num = base.CommandManager.Chat.Count((Command x) => x.Reference == base.Core && !x.HasFlag(CommandFlags.Hidden)) + base.CommandManager.ClientConsole.Count((Command x) => x.Reference == base.Core && !x.HasFlag(CommandFlags.Hidden));
		if (!base.Config.Logging.ReducedLogging)
		{
			Logger.Log(string.Format("Initialized Carbon Core plugin ({0:n0} {1}, {2:n0} {3})", new object[4]
			{
				base.Core.Hooks.Count,
				base.Core.Hooks.Count.Plural("hook", "hooks"),
				num,
				num.Plural("command", "commands")
			}));
		}
		Carbon.Components.CarbonAuto.Init();
		API.Abstracts.CarbonAuto.Singleton.Load();
	}

	internal void _installProcessors()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		if (!base.Config.Logging.ReducedLogging)
		{
			Logger.Log("Installed processors");
		}
		_uninstallProcessors();
		GameObject val = new GameObject("Processors");
		base.ScriptProcessor = val.AddComponent<ScriptProcessor>();
		base.ZipScriptProcessor = val.AddComponent<ZipScriptProcessor>();
		base.CarbonProcessor = val.AddComponent<CarbonProcessor>();
		base.HookManager = val.AddComponent<PatchManager>();
		base.ModuleProcessor = val.AddComponent<ModuleProcessor>();
		_registerProcessors();
		ScriptCompilationThread._injectPatchedReferences();
	}

	internal void _installTest()
	{
		Integrations.Logger = new Logger();
		if (!base.Config.Logging.ReducedLogging)
		{
			Logger.Log("Initialized Carbon.Test backend");
		}
	}

	internal void _registerProcessors()
	{
		if (base.ScriptProcessor != null)
		{
			base.ScriptProcessor?.Start();
		}
		if (base.ZipScriptProcessor != null)
		{
			base.ZipScriptProcessor?.Start();
		}
		if (base.ScriptProcessor != null)
		{
			base.ScriptProcessor.InvokeRepeating(delegate
			{
				RefreshConsoleInfo();
			}, 1f, 1f);
		}
		if (!base.Config.Logging.ReducedLogging)
		{
			Logger.Log("Registered processors");
		}
	}

	internal void _uninstallProcessors()
	{
		GameObject val = ((base.ScriptProcessor == null) ? null : base.ScriptProcessor.gameObject);
		try
		{
			if (base.ScriptProcessor != null)
			{
				base.ScriptProcessor?.Dispose();
			}
			if (base.ZipScriptProcessor != null)
			{
				base.ZipScriptProcessor?.Dispose();
			}
			if (base.ModuleProcessor != null)
			{
				base.ModuleProcessor?.Dispose();
			}
			if (base.CarbonProcessor != null)
			{
				base.CarbonProcessor?.Dispose();
			}
		}
		catch
		{
		}
		try
		{
			if ((Object)(object)val != (Object)null)
			{
				Object.Destroy((Object)(object)val);
			}
		}
		catch
		{
		}
	}

	internal void _handleThreads()
	{
		ThreadEx.MainThread.Name = "Main";
	}

	public override void Initialize()
	{
		base.Initialize();
		if (IsInitialized)
		{
			return;
		}
		HookCaller.Caller = new HookCallerInternal();
		LoadConfig();
		LoadMonoProfilerConfig();
		if (!base.Config.Logging.ReducedLogging)
		{
			Logger.Log(Environment.NewLine + "                                               " + Environment.NewLine + "  ______ _______ ______ ______ _______ _______ " + Environment.NewLine + " |      |   _   |   __ \\   __ \\       |    |  |" + Environment.NewLine + " |   ---|       |      <   __ <   -   |       |" + Environment.NewLine + " |______|___|___|___|__|______/_______|__|____|" + Environment.NewLine + "                          discord.gg/carbonmod " + Environment.NewLine + "                                               " + Environment.NewLine);
			Logger.Log("Initializing...");
		}
		base.Compat.Init();
		base.Events.Trigger(CarbonEvent.CarbonStartup, EventArgs.Empty);
		Logger.InitTaskExceptions();
		if (!base.Config.Logging.ReducedLogging)
		{
			Logger.Log("Loaded config");
		}
		Defines.Initialize();
		Vault.Load();
		_handleThreads();
		_installProcessors();
		_installTest();
		base.Events.Subscribe(CarbonEvent.HooksInstalled, delegate
		{
			ClearCommands();
			_installCore();
			base.ModuleProcessor.Init();
			base.Events.Trigger(CarbonEvent.HookValidatorRefreshed, EventArgs.Empty);
		});
		base.Events.Subscribe(CarbonEvent.HookValidatorRefreshed, delegate
		{
			CommandLine.ExecuteCommands("+carbon.onboot", "Carbon boot");
			string file = Path.Combine(Server.GetServerFolder("cfg"), "server.cfg");
			string[] array = (OsEx.File.Exists(file) ? OsEx.File.ReadTextLines(file) : null);
			if (array != null)
			{
				CommandLine.ExecuteCommands("+carbon.onboot", "cfg/server.cfg", array);
				CommandLine.ExecuteCommands(array);
				Array.Clear(array, 0, array.Length);
			}
			ReloadPlugins();
		});
		Logger.Log("Carbon " + base.Analytics.Version + " [" + base.Analytics.Protocol + "] " + Build.Git.HashShort + " on " + base.Analytics.Platform.ToCamelCase());
		Logger.Log("       " + Build.Git.Author + " on " + Build.Git.Branch + " (" + Build.Git.Date + ")");
		Logger.Log("Rust   " + BuildInfo.Current.Build.Number + "/" + Protocol.printable + " on " + BuildInfo.Current.Scm.Branch + " (" + BuildInfo.Current.Scm.Date + ") " + BuildInfo.Current.Scm.ChangeId);
		Interface.Initialize();
		RefreshConsoleInfo();
		IsInitialized = true;
		base.Events.Trigger(CarbonEvent.CarbonStartupComplete, EventArgs.Empty);
		WebControlPanel.Init();
	}

	public override void Uninitialize()
	{
		try
		{
			base.Events.Trigger(CarbonEvent.CarbonShutdown, EventArgs.Empty);
			_uninstallProcessors();
			ClearCommands(all: true);
			ClearPlugins(all: true);
			ModLoader.Packages.Clear();
			Debug.Log((object)"Unloaded Carbon.");
			try
			{
				if (Community.IsConfigReady && base.Config.Misc.ShowConsoleInfo && (Object)(object)SingletonComponent<ServerConsole>.Instance != (Object)null && SingletonComponent<ServerConsole>.Instance.input != null)
				{
					SingletonComponent<ServerConsole>.Instance.input.statusText = new string[3];
				}
			}
			catch
			{
			}
			Logger.Dispose();
			base.Events.Trigger(CarbonEvent.CarbonShutdownComplete, EventArgs.Empty);
		}
		catch (Exception ex)
		{
			Logger.Error("Failed Carbon uninitialization.", ex);
			base.Events.Trigger(CarbonEvent.CarbonShutdownFailed, EventArgs.Empty);
		}
		InternalRuntime = null;
		base.Uninitialize();
	}
}
