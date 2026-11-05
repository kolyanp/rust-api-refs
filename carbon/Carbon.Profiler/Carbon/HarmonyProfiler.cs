using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Carbon.Components;
using Carbon.Profiler;
using Facepunch;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using Steamworks;
using UnityEngine;

namespace Carbon;

public sealed class HarmonyProfiler : IHarmonyModHooks
{
	public class ProfilerRunner : FacepunchBehaviour
	{
	}

	public static readonly string configPath = Path.Combine(HarmonyLoader.modPath, "config.profiler.json");

	private static FacepunchBehaviour _runner;

	public static bool IsCarbonInstalled;

	public static bool IsOxideInstalled;

	public static bool IsAlreadyInstalled;

	public static MonoProfiler.Sample ProfileSample;

	private static readonly List<Command> commands;

	private static Command[] originalCommands;

	public static string profilesFolderPath
	{
		get
		{
			string text = Path.Combine(HarmonyLoader.modPath, "profiles");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return text;
		}
	}

	public static FacepunchBehaviour Runner
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			return _runner ?? (_runner = (FacepunchBehaviour)(object)new GameObject("Profiler Runner").AddComponent<ProfilerRunner>());
		}
	}

	public void OnLoaded(OnHarmonyModLoadedArgs args)
	{
		if (IsAlreadyInstalled)
		{
			Debug.LogError((object)"Carbon.Profiler was already set up once! To use an updated version of the profiler, a reboot is required!");
			_runner.Invoke((Action)(() =>
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				ConsoleSystem.Run(Option.Server, "harmony.unload Carbon.Profiler", Array.Empty<object>());
			}), 0.1f);
			return;
		}
		if (IsCarbonInstalled)
		{
			Debug.LogWarning((object)"Carbon is installed! Remove the Carbon.Profiler HarmonyMod since the profiler is already built in.");
			Runner.Invoke((Action)(() =>
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				ConsoleSystem.Run(Option.Server, "harmony.unload Carbon.Profiler", Array.Empty<object>());
			}), 0.1f);
			return;
		}
		if (IsOxideInstalled)
		{
			Debug.LogWarning((object)"Oxide is installed! Plugin and extension processing is hooked into.");
		}
		MonoProfilerConfig.Load(configPath);
		InitNative();
		MonoProfiler.ConfigPath = configPath;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			MonoProfiler.TryStartProfileFor(MonoProfilerConfig.ProfileTypes.Assembly, assembly, assembly.GetName().Name);
		}
		Debug.LogWarning((object)string.Format("Carbon.Profiler {0}! (NATIVE_PROTOCOL:{1} MANAGED_PROTOCOL:{2})", MonoProfiler.Crashed ? "crashed" : "initialized", 5, 127));
		Object.DontDestroyOnLoad((Object)(object)((Component)Runner).gameObject);
		if (SteamServer.IsValid)
		{
			Patches.Bootstrap_Init_Tier0.Postfix();
		}
	}

	public void OnUnloaded(OnHarmonyModUnloadedArgs args)
	{
		UninstallCommands();
	}

	private static MonoProfiler.ProfilerArgs GetProfilerFlags(Arg arg)
	{
		MonoProfiler.ProfilerArgs profilerArgs = MonoProfiler.ProfilerArgs.None;
		if (arg.HasArg("-cm", false))
		{
			profilerArgs |= MonoProfiler.ProfilerArgs.CallMemory;
		}
		if (arg.HasArg("-am", false))
		{
			profilerArgs |= MonoProfiler.ProfilerArgs.AdvancedMemory;
		}
		if (arg.HasArg("-t", false))
		{
			profilerArgs |= MonoProfiler.ProfilerArgs.Timings;
		}
		if (arg.HasArg("-c", false))
		{
			profilerArgs |= MonoProfiler.ProfilerArgs.Calls;
		}
		if (arg.HasArg("-gc", false))
		{
			profilerArgs |= MonoProfiler.ProfilerArgs.GCEvents;
		}
		return (profilerArgs == MonoProfiler.ProfilerArgs.None) ? (MonoProfiler.ProfilerArgs.CallMemory | MonoProfiler.ProfilerArgs.AdvancedMemory | MonoProfiler.ProfilerArgs.Timings | MonoProfiler.ProfilerArgs.Calls | MonoProfiler.ProfilerArgs.GCEvents) : profilerArgs;
	}

	private static bool IsTrackingCalls(Arg arg)
	{
		if (MonoProfilerConfig.Instance.TrackCalls)
		{
			return true;
		}
		arg.ReplyWith("Call tracking is off. Set TrackCalls to true in config.profiler.json and restart the server.");
		return false;
	}

	private static bool TryReadTarget(Arg arg, string command, out string name, out float duration)
	{
		name = arg.GetString(0, "");
		duration = arg.GetFloat(1, 0f);
		if (string.IsNullOrEmpty(name) || duration < 1f)
		{
			arg.ReplyWith("Syntax: " + command + " <name> <duration> [-cm] [-am] [-t] [-c] [-gc]");
			return false;
		}
		if (MonoProfiler.IsRecording || TargetedProfile.IsActive)
		{
			arg.ReplyWith("Profiler is actively recording");
			return false;
		}
		return IsTrackingCalls(arg);
	}

	private static void PrepareNextLoad(MonoProfilerConfig.ProfileTypes type, string value, bool tracked)
	{
		bool flag = tracked;
		bool flag2 = flag;
		if (flag2)
		{
			bool flag3 = ((type == MonoProfilerConfig.ProfileTypes.Plugin || type == MonoProfilerConfig.ProfileTypes.Harmony) ? true : false);
			flag2 = flag3;
		}
		if (flag2)
		{
			MonoProfiler.Start();
		}
		if (type == MonoProfilerConfig.ProfileTypes.Plugin && IsOxideInstalled)
		{
			OxidePlugins.MarkForRecompile(value);
		}
	}

	public static void InstallCommands()
	{
		if (originalCommands == null)
		{
			originalCommands = Index.All;
		}
		commands.Clear();
		commands.AddRange(originalCommands);
		AddCommand("carbon", "profile", (Arg arg) =>
		{
			float num = arg.GetFloat(0, 0f);
			MonoProfiler.ProfilerArgs profilerFlags = GetProfilerFlags(arg);
			if (MonoProfiler.IsRecording)
			{
				MonoProfiler.ToggleProfiling(profilerFlags);
				ProfileSample.Resample();
				MonoProfiler.Clear();
			}
			else if (num <= 0f)
			{
				MonoProfiler.ToggleProfiling(profilerFlags);
			}
			else
			{
				MonoProfiler.ToggleProfilingTimed(num, profilerFlags, (MonoProfiler.ProfilerArgs args) =>
				{
					ProfileSample.Resample();
					MonoProfiler.Clear();
				});
			}
		}, "Toggles the current state of the Carbon.Profiler", "[duration] [-cm] [-am] [-t] [-c] [-gc]");
		AddCommand("carbon", "abort_profile", (Arg arg) =>
		{
			if (!MonoProfiler.IsRecording && !TargetedProfile.IsActive)
			{
				arg.ReplyWith("No profiling process active");
			}
			else
			{
				if (TargetedProfile.IsActive)
				{
					arg.ReplyWith("Cancelled profiling '" + TargetedProfile.Name + "'");
				}
				if (MonoProfiler.IsRecording)
				{
					MonoProfiler.ToggleProfiling(MonoProfiler.ProfilerArgs.Abort);
					ProfileSample.Clear();
				}
				TargetedProfile.End();
			}
		}, "Stops a current profile from running");
		AddCommand("carbon", "profile_mod", (Arg arg) =>
		{
			if (TryReadTarget(arg, "carbon.profile_mod", out var name, out var duration))
			{
				arg.ReplyWith(TargetedProfile.StartMod(name, duration, GetProfilerFlags(arg)));
			}
		}, "Profiles one HarmonyMod: reloads it with profiler hooks, records for the duration, then reloads it without them", "<name> <duration> [-cm] [-am] [-t] [-c] [-gc]");
		AddCommand("carbon", "profile_plugin", (Arg arg) =>
		{
			if (TryReadTarget(arg, "carbon.profile_plugin", out var name, out var duration))
			{
				arg.ReplyWith(TargetedProfile.StartPlugin(name, duration, GetProfilerFlags(arg)));
			}
		}, "Profiles one Oxide plugin: recompiles it with profiler hooks, records for the duration, then recompiles it without them", "<name> <duration> [-cm] [-am] [-t] [-c] [-gc]");
		AddCommand("carbon", "export_profile", (Arg arg) =>
		{
			if (MonoProfiler.IsRecording)
			{
				arg.ReplyWith("Profiler is actively recording");
			}
			else
			{
				switch (arg.GetString(0, ""))
				{
				case "-c":
					arg.ReplyWith(WriteFileString("csv", ProfileSample.ToCSV()));
					break;
				case "-j":
					arg.ReplyWith(WriteFileString("json", ProfileSample.ToJson(indented: true)));
					break;
				case "-t":
					arg.ReplyWith(WriteFileString("txt", ProfileSample.ToTable()));
					break;
				default:
					arg.ReplyWith(WriteFileBytes("cprf", ProfileSample.ToProto()));
					break;
				}
			}
		}, "Exports to disk the most recent profile", "-c=CSV, -j=JSON, -t=Table, -p=ProtoBuf [default]");
		AddCommand("carbon", "tracked", (Arg arg) =>
		{
			arg.ReplyWith($"Tracked Assemblies ({MonoProfilerConfig.Instance.Assemblies.Count:n0}):\n" + string.Join("\n", MonoProfilerConfig.Instance.Assemblies.Select((string x) => "- " + x)) + "\n" + $"Tracked Plugins ({MonoProfilerConfig.Instance.Plugins.Count:n0}):\n" + string.Join("\n", MonoProfilerConfig.Instance.Plugins.Select((string x) => "- " + x)) + "\n" + $"Tracked Modules ({MonoProfilerConfig.Instance.Modules.Count:n0}):\n" + string.Join("\n", MonoProfilerConfig.Instance.Modules.Select((string x) => "- " + x)) + "\n" + $"Tracked Extensions ({MonoProfilerConfig.Instance.Extensions.Count:n0}):\n" + string.Join("\n", MonoProfilerConfig.Instance.Extensions.Select((string x) => "- " + x)) + "\n" + $"Tracked HarmonyMods ({MonoProfilerConfig.Instance.Harmony.Count:n0}):\n" + string.Join("\n", MonoProfilerConfig.Instance.Harmony.Select((string x) => "- " + x)) + "\nUse wildcard (*) to include all.");
		}, "All tracking lists present in the config which are used by the Mono profiler for tracking");
		AddCommand("carbon", "track", (Arg arg) =>
		{
			if (!arg.HasArgs(2))
			{
				InvalidReturn(arg);
			}
			else if (IsTrackingCalls(arg))
			{
				string text = arg.GetString(0, "");
				string text2 = arg.GetString(1, "");
				MonoProfilerConfig.ProfileTypes profileTypes = MonoProfilerConfig.ProfileTypes.Assembly;
				if (1 == 0)
				{
				}
				bool? flag = text switch
				{
					"assembly" => (bool?)MonoProfilerConfig.Instance.AppendProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Assembly, text2), 
					"plugin" => MonoProfilerConfig.Instance.AppendProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Plugin, text2), 
					"module" => MonoProfilerConfig.Instance.AppendProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Module, text2), 
					"ext" => MonoProfilerConfig.Instance.AppendProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Extension, text2), 
					"harmony" => MonoProfilerConfig.Instance.AppendProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Harmony, text2), 
					_ => null, 
				};
				if (1 == 0)
				{
				}
				bool? flag2 = flag;
				if (!flag2.HasValue)
				{
					InvalidReturn(arg);
				}
				else
				{
					arg.ReplyWith(flag2.Value ? $" Added {profileTypes} object '{text2}' to tracking" : $" Couldn't add {profileTypes} object '{text2}' for tracking");
					if (flag2.Value)
					{
						MonoProfilerConfig.Save(configPath);
						PrepareNextLoad(profileTypes, text2, tracked: true);
					}
				}
			}
		}, "Adds an object to be tracked. Reload a tracked plugin or HarmonyMod to profile it. Assemblies, modules and extensions need Enabled in config.profiler.json and a server restart", "[assembly|plugin|module|ext|harmony] [value]");
		AddCommand("carbon", "untrack", (Arg arg) =>
		{
			if (!arg.HasArgs(2))
			{
				InvalidReturn2(arg);
			}
			else
			{
				string text = arg.GetString(0, "");
				string text2 = arg.GetString(1, "");
				MonoProfilerConfig.ProfileTypes profileTypes = MonoProfilerConfig.ProfileTypes.Assembly;
				if (1 == 0)
				{
				}
				bool? flag = text switch
				{
					"assembly" => (bool?)MonoProfilerConfig.Instance.RemoveProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Assembly, text2), 
					"plugin" => MonoProfilerConfig.Instance.RemoveProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Plugin, text2), 
					"module" => MonoProfilerConfig.Instance.RemoveProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Module, text2), 
					"ext" => MonoProfilerConfig.Instance.RemoveProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Extension, text2), 
					"harmony" => MonoProfilerConfig.Instance.RemoveProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Harmony, text2), 
					_ => null, 
				};
				if (1 == 0)
				{
				}
				bool? flag2 = flag;
				if (!flag2.HasValue)
				{
					InvalidReturn2(arg);
				}
				else
				{
					arg.ReplyWith(flag2.Value ? $" Removed {profileTypes} object '{text2}' from tracking" : $" Couldn't remove {profileTypes} object '{text2}' for tracking");
					if (flag2.Value)
					{
						MonoProfilerConfig.Save(configPath);
						PrepareNextLoad(profileTypes, text2, tracked: false);
					}
				}
			}
		}, "Removes an object from being tracked. Reload an untracked plugin or HarmonyMod to remove its profiler hooks. Assemblies, modules and extensions need a server restart", "[assembly|plugin|module|ext|harmony] [value]");
		AddCommand("carbon", "profiler_version", (Arg arg) =>
		{
			TextTable val = Pool.Get<TextTable>();
			val.Clear();
			val.AddColumns(new string[4] { "version", "protocol", "managed", "native" });
			val.AddRow(new string[4]
			{
				SelfUpdate.CurrentVersion.ToString(),
				string.Empty,
				127.ToString(),
				5.ToString()
			});
			string text = ((object)val).ToString().TrimEnd();
			Pool.FreeUnsafe<TextTable>(ref val);
			arg.ReplyWith(text);
		}, "Prints the version of Carbon profiler");
		AddCommand("carbon", "update_profiler", (Arg arg) =>
		{
			SelfUpdate.Api((JArray data) =>
			{
				JToken val = ((IEnumerable<JToken>)data).FirstOrDefault((JToken x) => x[(object)"name"].ToObject<string>().Equals("profiler_build"));
				Version version = new Version(((val != null) ? val[(object)"version"].ToObject<string>() : null) + ".0");
				if (!SelfUpdate.CurrentVersion.Equals(version))
				{
					Debug.Log((object)$"Carbon.Profiler is out of date! (current {SelfUpdate.CurrentVersion}, newer {version})");
					SelfUpdate.Update(() =>
					{
						Debug.Log((object)"Updated Carbon.Profiler. Restart the server to load it.");
					});
				}
				else
				{
					Debug.Log((object)$"Carbon.Profiler is up to date! ({SelfUpdate.CurrentVersion})");
				}
			});
			arg.ReplyWith("Checking for updates..");
		}, "Checks if the profiler is out of date, and updates itself if it is.");
		Index.All = commands.ToArray();
		static void AddCommand(string parent, string name, Action<Arg> callback, string description = null, string arguments = null)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Expected Obj, but got Unknown
			Command val = new Command
			{
				Name = name,
				Parent = parent,
				FullName = parent + "." + name,
				Call = callback,
				ServerAdmin = true,
				Description = (description ?? string.Empty),
				Arguments = (arguments ?? string.Empty)
			};
			commands.Add(val);
			AddCommandToServerIndex(val);
			Debug.LogWarning((object)("Carbon.Profiler: Installed '" + val.FullName + "'"));
		}
		static void InvalidReturn(Arg arg)
		{
			arg.ReplyWith("Syntax: carbon.track [assembly|plugin|module|ext|harmony] [value]");
		}
		static void InvalidReturn2(Arg arg)
		{
			arg.ReplyWith("Syntax: carbon.untrack [assembly|plugin|module|ext|harmony] [value]");
		}
		static string WriteFileBytes(string extension, byte[] data)
		{
			DateTime now = DateTime.Now;
			string text = Path.Combine(profilesFolderPath, string.Format("profile-{0}_{1:00}_{2:00}_{3:00}{4:00}{5:00}.{6}", new object[7] { now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, extension }));
			File.WriteAllBytes(text, data);
			return "Exported profile output at '" + text + "'";
		}
		static string WriteFileString(string extension, string data)
		{
			DateTime now = DateTime.Now;
			string text = Path.Combine(profilesFolderPath, string.Format("profile-{0}_{1:00}_{2:00}_{3:00}{4:00}{5:00}.{6}", new object[7] { now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, extension }));
			File.WriteAllText(text, data);
			return "Exported profile output at '" + text + "'";
		}
	}

	private static void AddCommandToServerIndex(Command command)
	{
		try
		{
			if (!(AccessTools.Field(typeof(Server), "Dict")?.GetValue(null) is IDictionary dictionary))
			{
				throw new InvalidOperationException("ConsoleSystem.Index.Server.Dict is not available");
			}
			Type type = dictionary.GetType().GetGenericArguments().FirstOrDefault();
			if (type == null)
			{
				throw new InvalidOperationException("Couldn't determine server command index key type '" + dictionary.GetType().FullName + "'");
			}
			object obj = ((type == typeof(string)) ? command.FullName : AccessTools.Constructor(type, new Type[1] { typeof(string) }, false)?.Invoke(new object[1] { command.FullName }));
			if (obj == null)
			{
				throw new InvalidOperationException("Couldn't create server command index key '" + type.FullName + "'");
			}
			dictionary[obj] = command;
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException("Carbon.Profiler couldn't add '" + command.FullName + "' to server command index", innerException);
		}
	}

	public static void UninstallCommands()
	{
		Index.All = originalCommands;
	}

	[DllImport("CarbonNative")]
	public unsafe static extern void init_profiler(char* ptr, int length);

	[DllImport("__Internal", CharSet = CharSet.Ansi)]
	public static extern void mono_dllmap_insert(ModuleHandle assembly, string dll, string func, string tdll, string tfunc);

	public unsafe static void InitNative()
	{
		mono_dllmap_insert(ModuleHandle.EmptyHandle, "CarbonNative", null, Path.Combine(HarmonyLoader.modPath, "native", "CarbonNative.dll"), null);
		fixed (char* ptr = configPath)
		{
			init_profiler(ptr, configPath.Length);
		}
	}

	static HarmonyProfiler()
	{
		GameObject val = GameObject.Find("Profiler Runner");
		_runner = ((val != null) ? val.GetComponent<FacepunchBehaviour>() : null);
		IsCarbonInstalled = Type.GetType("Carbon.Community,Carbon.Common") != null;
		IsOxideInstalled = Type.GetType("Oxide.Core.Interface,Oxide.Core") != null;
		IsAlreadyInstalled = (Object)(object)_runner != (Object)null;
		ProfileSample = MonoProfiler.Sample.Create();
		commands = new List<Command>();
	}
}
