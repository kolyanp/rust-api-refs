using System;
using System.Collections.Generic;
using System.Linq;
using API.Analytics;
using API.Assembly;
using API.Commands;
using API.Contracts;
using API.Events;
using API.Hooks;
using API.Logger;
using Carbon.Base;
using Carbon.Components;
using Carbon.Contracts;
using Carbon.Core;
using Carbon.Extensions;
using Carbon.Profiler;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core.Libraries;
using Oxide.Plugins;
using UnityEngine;

namespace Carbon;

public class Community
{
	private readonly Lazy<IAnalyticsManager> _analyticsManager = new Lazy<IAnalyticsManager>((Func<IAnalyticsManager>)GameObject.GetComponent<IAnalyticsManager>);

	private readonly Lazy<IAssemblyManager> _assemblyEx = new Lazy<IAssemblyManager>((Func<IAssemblyManager>)GameObject.GetComponent<IAssemblyManager>);

	private readonly Lazy<ICommandManager> _commandManager = new Lazy<ICommandManager>((Func<ICommandManager>)GameObject.GetComponent<ICommandManager>);

	private readonly Lazy<IDownloadManager> _downloadManager = new Lazy<IDownloadManager>((Func<IDownloadManager>)GameObject.GetComponent<IDownloadManager>);

	private readonly Lazy<IEventManager> _eventManager = new Lazy<IEventManager>((Func<IEventManager>)GameObject.GetComponent<IEventManager>);

	private readonly Lazy<ICompatManager> _compatManager = new Lazy<ICompatManager>((Func<ICompatManager>)GameObject.GetComponent<ICompatManager>);

	private static readonly Lazy<GameObject> _gameObject = new Lazy<GameObject>(delegate
	{
		GameObject val = GameObject.Find("Carbon");
		if (!((Object)(object)val == (Object)null))
		{
			return val;
		}
		throw new Exception("Carbon GameObject not found");
	});

	internal static string _runtimeId;

	public Config Config { get; set; }

	public MonoProfilerConfig MonoProfilerConfig => MonoProfilerConfig.Instance;

	public static Community Runtime { get; set; }

	public CorePlugin Core { get; set; }

	public ModLoader.Package Plugins { get; set; }

	public ModLoader.Package ZipPlugins { get; set; }

	public static GameObject GameObject => _gameObject.Value;

	public IAnalyticsManager Analytics => _analyticsManager.Value;

	public IAssemblyManager AssemblyEx => _assemblyEx.Value;

	public ICommandManager CommandManager => _commandManager.Value;

	public IDownloadManager Downloader => _downloadManager.Value;

	public IEventManager Events => _eventManager.Value;

	public ICompatManager Compat => _compatManager.Value;

	public IPatchManager HookManager { get; set; }

	public IScriptProcessor ScriptProcessor { get; set; }

	public IModuleProcessor ModuleProcessor { get; set; }

	public IZipScriptProcessor ZipScriptProcessor { get; set; }

	public ICarbonProcessor CarbonProcessor { get; set; }

	public static bool IsServerInitialized { get; internal set; }

	public static bool IsConfigReady
	{
		get
		{
			if (Runtime != null)
			{
				return Runtime.Config != null;
			}
			return false;
		}
	}

	public static bool AllProcessorsFinalized
	{
		get
		{
			if (Runtime.ScriptProcessor.AllPendingScriptsComplete())
			{
				return Runtime.ZipScriptProcessor.AllPendingScriptsComplete();
			}
			return false;
		}
	}

	public static int AllProcesses => Runtime.ScriptProcessor.InstanceBuffer.Count + Runtime.ZipScriptProcessor.InstanceBuffer.Count;

	public static string RuntimeId
	{
		get
		{
			if (string.IsNullOrEmpty(_runtimeId))
			{
				DateTime now = DateTime.Now;
				_runtimeId = now.Year.ToString() + now.Month + now.Day + now.Hour + now.Minute + now.Second + now.Millisecond;
			}
			return _runtimeId;
		}
	}

	public void ForceEnsurePublicizedAssembly(string value, ref bool needsSave)
	{
		if (!Config.Publicizer.PublicizedAssemblies.Contains(value))
		{
			Config.Publicizer.PublicizedAssemblies.Add(value);
			needsSave = true;
		}
	}

	public void LoadConfig(bool postProcess = true)
	{
		bool needsSave = false;
		if (!OsEx.File.Exists(Defines.GetConfigFile()))
		{
			needsSave = true;
			if (Config == null)
			{
				Config config = (Config = new Config());
			}
		}
		else
		{
			Config = JsonConvert.DeserializeObject<Config>(OsEx.File.ReadText(Defines.GetConfigFile()));
		}
		if (postProcess)
		{
			if (Config.Compiler.ConditionalCompilationSymbols == null)
			{
				Config.Compiler.ConditionalCompilationSymbols = new List<string>();
				needsSave = true;
			}
			if (string.IsNullOrEmpty(Config.Permissions.AdminDefaultGroup))
			{
				Config.Permissions.AdminDefaultGroup = "admin";
			}
			if (string.IsNullOrEmpty(Config.Permissions.PlayerDefaultGroup))
			{
				Config.Permissions.PlayerDefaultGroup = "default";
			}
			if (string.IsNullOrEmpty(Config.Permissions.ModeratorDefaultGroup))
			{
				Config.Permissions.ModeratorDefaultGroup = "moderator";
			}
			if (!Config.Compiler.ConditionalCompilationSymbols.Contains("CARBON"))
			{
				Config.Compiler.ConditionalCompilationSymbols.Add("CARBON");
			}
			if (!Config.Compiler.ConditionalCompilationSymbols.Contains("RUST"))
			{
				Config.Compiler.ConditionalCompilationSymbols.Add("RUST");
			}
			if (!Config.Compiler.ConditionalCompilationSymbols.Contains("OXIDE_PUBLICIZED"))
			{
				Config.Compiler.ConditionalCompilationSymbols.Add("OXIDE_PUBLICIZED");
			}
			Config.Compiler.ConditionalCompilationSymbols = Config.Compiler.ConditionalCompilationSymbols.Distinct().ToList();
			if (Config.Prefixes == null)
			{
				Config.Prefixes = new List<Command.Prefix>();
				needsSave = true;
			}
			if (Config.Aliases == null)
			{
				Config.Aliases = new Dictionary<string, string>();
				needsSave = true;
			}
			else
			{
				List<string> list = Pool.Get<List<string>>();
				list.AddRange(Config.Aliases.Where(delegate(KeyValuePair<string, string> keyValuePair2)
				{
					Config config3 = Config;
					KeyValuePair<string, string> keyValuePair = keyValuePair2;
					if (config3.IsValidAlias(keyValuePair.Key, out var _))
					{
						keyValuePair = keyValuePair2;
						string key = keyValuePair.Key;
						keyValuePair = keyValuePair2;
						return key == keyValuePair.Value;
					}
					return true;
				}).Select(delegate(KeyValuePair<string, string> keyValuePair2)
				{
					KeyValuePair<string, string> keyValuePair = keyValuePair2;
					return keyValuePair.Key;
				}));
				foreach (string item in list)
				{
					Config.Aliases.Remove(item);
					Logger.Warn(" Removed invalid alias: " + item);
				}
				if (list.Count > 0)
				{
					needsSave = true;
				}
				Pool.FreeUnmanaged<string>(ref list);
				foreach (KeyValuePair<string, string> alias in Config.Aliases)
				{
					if (Index.All.Any((Command x) => x.FullName.Equals(alias.Key, StringComparison.OrdinalIgnoreCase)))
					{
						Logger.Warn(" Alias '" + alias.Key + "' overrides an already existing Rust command");
					}
				}
			}
			if (Config.Prefixes.Count == 0)
			{
				Config.Prefixes.Add(new Command.Prefix
				{
					Value = "/",
					PrintToChat = false,
					PrintToConsole = false,
					SuggestionAuthLevel = 2
				});
			}
			Config.PublicizerConfig publicizer = Config.Publicizer;
			if (publicizer.PublicizedAssemblies == null)
			{
				publicizer.PublicizedAssemblies = new List<string>();
			}
			ForceEnsurePublicizedAssembly("Assembly-CSharp.dll", ref needsSave);
			ForceEnsurePublicizedAssembly("Facepunch.Console.dll", ref needsSave);
			ForceEnsurePublicizedAssembly("Facepunch.Network.dll", ref needsSave);
			ForceEnsurePublicizedAssembly("Facepunch.Nexus.dll", ref needsSave);
			ForceEnsurePublicizedAssembly("Facepunch.Ping.dll", ref needsSave);
			ForceEnsurePublicizedAssembly("Facepunch.Unity.dll", ref needsSave);
			ForceEnsurePublicizedAssembly("Facepunch.Rcon.dll", ref needsSave);
			ForceEnsurePublicizedAssembly("Rust.Localization.dll", ref needsSave);
			ForceEnsurePublicizedAssembly("Rust.Clans.Local.dll", ref needsSave);
			ForceEnsurePublicizedAssembly("Rust.FileSystem.dll", ref needsSave);
			ForceEnsurePublicizedAssembly("Rust.Harmony.dll", ref needsSave);
			ForceEnsurePublicizedAssembly("Rust.Global.dll", ref needsSave);
			ForceEnsurePublicizedAssembly("Rust.Data.dll", ref needsSave);
			ForceEnsurePublicizedAssembly("Fleck.dll", ref needsSave);
			publicizer = Config.Publicizer;
			if (publicizer.PublicizerMemberIgnores == null)
			{
				publicizer.PublicizerMemberIgnores = new List<string>(3) { "^HiddenValueBase$", "^HiddenValue`1$", "^Pool$" };
			}
			if (Config.Aliases.Count == 0)
			{
				Config.Aliases["carbon"] = "c.version";
				Config.Aliases["plugins"] = "c.plugins";
				Config.Aliases["reload"] = "c.reload";
				Config.Aliases["load"] = "c.load";
				Config.Aliases["unload"] = "c.unload";
				needsSave = true;
			}
			else if (Config.Aliases.Remove("harmony.load") || Config.Aliases.Remove("harmony.unload"))
			{
				needsSave = true;
			}
			Command.Prefixes = Config.Prefixes;
			if (Logger.CoreLog == null)
			{
				Logger.CoreLog = new FileLogger("Carbon.Core");
			}
			Logger.CoreLog.SplitSize = (int)(Config.Logging.LogSplitSize * 1000000.0);
			if (needsSave)
			{
				SaveConfig();
			}
		}
		if (Config.Analytics.Enabled)
		{
			Logger.Warn("Carbon Analytics are ON. They're entirely anonymous and help us to further improve.");
		}
		else
		{
			Logger.Error("Carbon Analytics are OFF.");
		}
	}

	public void SaveConfig()
	{
		if (Config == null)
		{
			Config config = (Config = new Config());
		}
		OsEx.File.Create(Defines.GetConfigFile(), JsonConvert.SerializeObject((object)Config, (Formatting)1));
	}

	public void LoadMonoProfilerConfig()
	{
		MonoProfilerConfig.Load(Defines.GetMonoProfilerConfigFile());
	}

	public void SaveMonoProfilerConfig()
	{
		MonoProfilerConfig.Save(Defines.GetMonoProfilerConfigFile());
	}

	public Community()
	{
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected O, but got Unknown
		try
		{
			Events.Subscribe(CarbonEvent.CarbonStartup, delegate
			{
				if (!Config.Logging.ReducedLogging)
				{
					Logger.Log("Carbon fingerprint: " + Analytics.ClientID);
					Logger.Log("System fingerprint: " + Analytics.SystemID);
				}
				Analytics.SessionStart();
			});
			Events.Subscribe(CarbonEvent.CarbonStartupComplete, delegate
			{
				Carbon.Components.Analytics.on_server_startup();
			});
			char[] newlineSplit = new char[1] { '\n' };
			Application.logMessageReceived += (LogCallback)delegate(string condition, string stackTrace, LogType type)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Invalid comparison between Unknown and I4
				//IL_0004: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Invalid comparison between Unknown and I4
				//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
				if (((int)type <= 1 || (int)type == 4) && !string.IsNullOrEmpty(condition) && (condition.StartsWith("Null") || condition.StartsWith("Index")))
				{
					string[] array = stackTrace.Split(newlineSplit, StringSplitOptions.RemoveEmptyEntries);
					string text = string.Empty;
					foreach (string text2 in array)
					{
						if (!string.IsNullOrEmpty(text2))
						{
							text = text + "  at " + text2 + "\n";
						}
					}
					Array.Clear(array, 0, array.Length);
					text = text.TrimEnd();
					Logger.Write(Severity.Error, "Unhandled error occurred (" + condition + ")\n" + text, null, 1, nativeLog: false);
					SingletonComponent<ServerConsole>.Instance.HandleLog(text, (string)null, type);
				}
			};
		}
		catch (Exception ex)
		{
			Logger.Error("Critical error", ex);
		}
	}

	public void MarkServerInitialized(bool wants)
	{
		if (wants && !IsServerInitialized)
		{
			Oxide.Core.Libraries.Timer.FireDueStartupTimers();
		}
		IsServerInitialized = wants;
		if (wants)
		{
			Oxide.Core.Libraries.Timer.ConvertRemainingStartupTimersToInvokes();
		}
	}

	public void ClearCommands(bool all = false)
	{
		CommandManager.ClearCommands((Command command) => all || (command.Reference is RustPlugin rustPlugin && !rustPlugin.IsCorePlugin));
	}

	public void RefreshConsoleInfo()
	{
		if (IsConfigReady && Config.Misc.ShowConsoleInfo && IsServerInitialized && !((Object)(object)SingletonComponent<ServerConsole>.Instance == (Object)null) && SingletonComponent<ServerConsole>.Instance.input != null)
		{
			if (SingletonComponent<ServerConsole>.Instance.input.statusText.Length != 4)
			{
				SingletonComponent<ServerConsole>.Instance.input.statusText = new string[4];
			}
			string version = Analytics.Version;
			SingletonComponent<ServerConsole>.Instance.input.statusText[3] = " Carbon" + string.Format(" v{0}, {1:n0} mods, {2:n0} plgs, {3:n0}/{4:n0} mdls, {5:n0} exts, {6:n0} mdfs", new object[7]
			{
				version,
				ModLoader.Packages.Count,
				ModLoader.Packages.Sum((ModLoader.Package x) => x.Plugins.Count),
				ModuleProcessor.Modules.Count((BaseHookable x) => x is BaseModule baseModule && baseModule.IsEnabled()),
				ModuleProcessor.Modules.Count,
				AssemblyEx.Extensions.Loaded.Count,
				StoredModifiers.Entities?.Count
			});
		}
	}

	public virtual void Initialize()
	{
		StoredModifiers.Init();
	}

	public virtual void Uninitialize()
	{
		Runtime = null;
	}

	public static void LogCommand(object message, BasePlayer player = null)
	{
		if ((Object)(object)player == (Object)null)
		{
			Logger.Log(message);
		}
		else
		{
			player.SendConsoleCommand($"echo {message}", Array.Empty<object>());
		}
	}

	public virtual void ReloadPlugins(IEnumerable<string> except = null)
	{
		ModLoader.IsBatchComplete = false;
		ModLoader.ClearAllErrored();
		ModLoader.ClearAllRequirees();
	}

	public void ClearPlugins(bool all = false)
	{
		Runtime.ClearCommands(all);
		ModLoader.UnloadCarbonMods();
	}

	public unsafe static string Protect(string name)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(name))
		{
			return string.Empty;
		}
		StringView val = default(StringView);
		((StringView)(ref val))._002Ector(name);
		int num = ((StringView)(ref val)).IndexOf(' ');
		if (num < 0)
		{
			return Vault.Pool.Get(((object)(*(StringView*)(&val))/*cast due to constrained. prefix*/).ToString() + RuntimeId).ToString();
		}
		StringView val2 = ((StringView)(ref val)).Substring(0, num);
		StringView val3 = ((StringView)(ref val)).Substring(num + 1);
		return Vault.Pool.Get(((object)(*(StringView*)(&val2))/*cast due to constrained. prefix*/).ToString() + RuntimeId) + " " + ((object)(*(StringView*)(&val3))/*cast due to constrained. prefix*/).ToString();
	}
}
