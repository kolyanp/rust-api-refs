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
		CorePlugin core = (Core = new CorePlugin());
		runtime.Core = core;
		Core.Setup("Core", "Carbon Community", new VersionNumber(1, 0, 0), string.Empty);
		ModLoader.ProcessPrecompiledType(Core);
		CorePlugin core2 = Core;
		bool isCorePlugin = (Core.IsPrecompiled = true);
		core2.IsCorePlugin = isCorePlugin;
		Core.IInit();
		Core.ILoadDefaultMessages();
		ModLoader.RegisterPackage(Core.Package = ModLoader.Package.Get("Carbon Community", isCoreMod: true).AddPlugin(Core));
		ModLoader.Package package = (Plugins = ModLoader.Package.Get("Scripts", isCoreMod: false));
		ModLoader.RegisterPackage(package);
		package = (ZipPlugins = ModLoader.Package.Get("Zip Scripts", isCoreMod: false));
		ModLoader.RegisterPackage(package);
		ModLoader.ProcessCommands(typeof(CorePlugin), Core, BindingFlags.Instance | BindingFlags.NonPublic, "c");
		ModLoader.ProcessCommands(typeof(CorePlugin), Core, BindingFlags.Instance | BindingFlags.NonPublic, "carbon", hidden: true);
		int num = CommandManager.Chat.Count((Command x) => x.Reference == Core && !x.HasFlag(CommandFlags.Hidden)) + CommandManager.ClientConsole.Count((Command x) => x.Reference == Core && !x.HasFlag(CommandFlags.Hidden));
		if (!Config.Logging.ReducedLogging)
		{
			Logger.Log(string.Format("Initialized Carbon Core plugin ({0:n0} {1}, {2:n0} {3})", new object[4]
			{
				Core.Hooks.Count,
				Core.Hooks.Count.Plural("hook", "hooks"),
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
		//IL_002d: Expected Obj, but got Unknown
		if (!Config.Logging.ReducedLogging)
		{
			Logger.Log("Installed processors");
		}
		_uninstallProcessors();
		GameObject val = new GameObject("Processors");
		ScriptProcessor = val.AddComponent<ScriptProcessor>();
		ZipScriptProcessor = val.AddComponent<ZipScriptProcessor>();
		CarbonProcessor = val.AddComponent<CarbonProcessor>();
		HookManager = val.AddComponent<PatchManager>();
		ModuleProcessor = val.AddComponent<ModuleProcessor>();
		_registerProcessors();
		ScriptCompilationThread._injectPatchedReferences();
	}

	internal void _installTest()
	{
		Integrations.Logger = new Logger();
		if (!Config.Logging.ReducedLogging)
		{
			Logger.Log("Initialized Carbon.Test backend");
		}
	}

	internal void _registerProcessors()
	{
		if (ScriptProcessor != null)
		{
			ScriptProcessor?.Start();
		}
		if (ZipScriptProcessor != null)
		{
			ZipScriptProcessor?.Start();
		}
		if (ScriptProcessor != null)
		{
			ScriptProcessor.InvokeRepeating(() =>
			{
				RefreshConsoleInfo();
			}, 1f, 1f);
		}
		if (!Config.Logging.ReducedLogging)
		{
			Logger.Log("Registered processors");
		}
	}

	internal void _uninstallProcessors()
	{
		GameObject val = ((ScriptProcessor == null) ? null : ScriptProcessor.gameObject);
		try
		{
			if (ScriptProcessor != null)
			{
				ScriptProcessor?.Dispose();
			}
			if (ZipScriptProcessor != null)
			{
				ZipScriptProcessor?.Dispose();
			}
			if (ModuleProcessor != null)
			{
				ModuleProcessor?.Dispose();
			}
			if (CarbonProcessor != null)
			{
				CarbonProcessor?.Dispose();
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
		RefreshConsoleInfo();
		if (!Config.Logging.ReducedLogging)
		{
			Logger.Log(Environment.NewLine + "                                               " + Environment.NewLine + "  ______ _______ ______ ______ _______ _______ " + Environment.NewLine + " |      |   _   |   __ \\   __ \\       |    |  |" + Environment.NewLine + " |   ---|       |      <   __ <   -   |       |" + Environment.NewLine + " |______|___|___|___|__|______/_______|__|____|" + Environment.NewLine + "                          discord.gg/carbonmod " + Environment.NewLine + "                                               " + Environment.NewLine);
			Logger.Log("Initializing...");
		}
		Compat.Init();
		Events.Trigger(CarbonEvent.CarbonStartup, EventArgs.Empty);
		Logger.InitTaskExceptions();
		if (!Config.Logging.ReducedLogging)
		{
			Logger.Log("Loaded config");
		}
		Defines.Initialize();
		Vault.Load();
		_handleThreads();
		_installProcessors();
		_installTest();
		Events.Subscribe(CarbonEvent.HooksInstalled, (EventArgs args) =>
		{
			ClearCommands();
			_installCore();
			ModuleProcessor.Init();
			Events.Trigger(CarbonEvent.HookValidatorRefreshed, EventArgs.Empty);
		});
		Events.Subscribe(CarbonEvent.HookValidatorRefreshed, (EventArgs args) =>
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
		Logger.Log("Carbon " + Analytics.Version + " [" + Analytics.Protocol + "] " + Build.Git.HashShort + " on " + Analytics.Platform.ToCamelCase());
		Logger.Log("       " + Build.Git.Author + " on " + Build.Git.Branch + " (" + Build.Git.Date + ")");
		Logger.Log("Rust   " + BuildInfo.Current.Build.Number + "/" + Protocol.printable + " on " + BuildInfo.Current.Scm.Branch + " (" + BuildInfo.Current.Scm.Date + ") " + BuildInfo.Current.Scm.ChangeId);
		Interface.Initialize();
		IsInitialized = true;
		Events.Trigger(CarbonEvent.CarbonStartupComplete, EventArgs.Empty);
		WebControlPanel.Init();
	}

	public override void Uninitialize()
	{
		try
		{
			Events.Trigger(CarbonEvent.CarbonShutdown, EventArgs.Empty);
			_uninstallProcessors();
			ClearCommands(all: true);
			ClearPlugins(all: true);
			ModLoader.Packages.Clear();
			HookSubscriberIndex.Invalidate();
			Debug.Log((object)"Unloaded Carbon.");
			try
			{
				if (Community.IsConfigReady && Config.Misc.ShowConsoleInfo && (Object)(object)SingletonComponent<ServerConsole>.Instance != (Object)null && SingletonComponent<ServerConsole>.Instance.input != null)
				{
					SingletonComponent<ServerConsole>.Instance.input.statusText = new string[3];
				}
			}
			catch
			{
			}
			Logger.Dispose();
			Events.Trigger(CarbonEvent.CarbonShutdownComplete, EventArgs.Empty);
		}
		catch (Exception ex)
		{
			Logger.Error("Failed Carbon uninitialization.", ex);
			Events.Trigger(CarbonEvent.CarbonShutdownFailed, EventArgs.Empty);
		}
		InternalRuntime = null;
		base.Uninitialize();
	}
}
