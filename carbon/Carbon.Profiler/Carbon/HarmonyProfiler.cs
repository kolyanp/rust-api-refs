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

	public static FacepunchBehaviour Runner => _runner ?? (_runner = (FacepunchBehaviour)(object)new GameObject("Profiler Runner").AddComponent<ProfilerRunner>());

	public void OnLoaded(OnHarmonyModLoadedArgs args)
	{
		if (IsAlreadyInstalled)
		{
			Debug.LogError((object)"Carbon.Profiler was already set up once! To use an updated version of the profiler, a reboot is required!");
			_runner.Invoke((Action)delegate
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				ConsoleSystem.Run(Option.Server, "harmony.unload Carbon.Profiler", Array.Empty<object>());
			}, 0.1f);
			return;
		}
		if (IsCarbonInstalled)
		{
			Debug.LogWarning((object)"Carbon is installed! Remove the Carbon.Profiler HarmonyMod since the profiler is already built in.");
			Runner.Invoke((Action)delegate
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				ConsoleSystem.Run(Option.Server, "harmony.unload Carbon.Profiler", Array.Empty<object>());
			}, 0.1f);
			return;
		}
		if (IsOxideInstalled)
		{
			Debug.LogWarning((object)"Oxide is installed! Plugin and extension processing is hooked into.");
		}
		MonoProfilerConfig.Load(configPath);
		InitNative();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			MonoProfiler.TryStartProfileFor(MonoProfilerConfig.ProfileTypes.Assembly, assembly, assembly.GetName().Name);
		}
		Debug.LogWarning((object)string.Format("Carbon.Profiler {0}! (NATIVE_PROTOCOL:{1} MANAGED_PROTOCOL:{2})", MonoProfiler.Crashed ? "crashed" : "initialized", 4, 127));
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

	public static void InstallCommands()
	{
		if (originalCommands == null)
		{
			originalCommands = Index.All;
		}
		commands.Clear();
		commands.AddRange(originalCommands);
		AddCommand("carbon", "profile", delegate(Arg arg)
		{
			if (!MonoProfiler.Enabled)
			{
				arg.ReplyWith("Mono profiler is disabled. Enable it in the 'config.profiler.json' config file. Must restart the server for changes to apply.");
			}
			else
			{
				float num = arg.GetFloat(0, 0f);
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
				if (profilerArgs == MonoProfiler.ProfilerArgs.None)
				{
					profilerArgs = MonoProfiler.ProfilerArgs.CallMemory | MonoProfiler.ProfilerArgs.AdvancedMemory | MonoProfiler.ProfilerArgs.Timings | MonoProfiler.ProfilerArgs.Calls | MonoProfiler.ProfilerArgs.GCEvents;
				}
				if (MonoProfiler.IsRecording)
				{
					MonoProfiler.ToggleProfiling(profilerArgs);
					ProfileSample.Resample();
					MonoProfiler.Clear();
				}
				else if (num <= 0f)
				{
					MonoProfiler.ToggleProfiling(profilerArgs);
				}
				else
				{
					MonoProfiler.ToggleProfilingTimed(num, profilerArgs, delegate
					{
						ProfileSample.Resample();
						MonoProfiler.Clear();
					});
				}
			}
		}, "Toggles the current state of the Carbon.Profiler", "[duration] [-cm] [-am] [-t] [-c] [-gc]");
		AddCommand("carbon", "abort_profile", delegate(Arg arg)
		{
			if (!MonoProfiler.IsRecording)
			{
				arg.ReplyWith("No profiling process active");
			}
			else
			{
				MonoProfiler.ToggleProfiling(MonoProfiler.ProfilerArgs.Abort);
				ProfileSample.Clear();
			}
		}, "Stops a current profile from running");
		AddCommand("carbon", "export_profile", delegate(Arg arg)
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
		AddCommand("carbon", "tracked", delegate(Arg arg)
		{
			arg.ReplyWith($"Tracked Assemblies ({MonoProfilerConfig.Instance.Assemblies.Count:n0}):\n" + string.Join("\n", MonoProfilerConfig.Instance.Assemblies.Select((string x) => "- " + x)) + "\n" + $"Tracked Plugins ({MonoProfilerConfig.Instance.Plugins.Count:n0}):\n" + string.Join("\n", MonoProfilerConfig.Instance.Plugins.Select((string x) => "- " + x)) + "\n" + $"Tracked Modules ({MonoProfilerConfig.Instance.Modules.Count:n0}):\n" + string.Join("\n", MonoProfilerConfig.Instance.Modules.Select((string x) => "- " + x)) + "\n" + $"Tracked Extensions ({MonoProfilerConfig.Instance.Extensions.Count:n0}):\n" + string.Join("\n", MonoProfilerConfig.Instance.Extensions.Select((string x) => "- " + x)) + "\nUse wildcard (*) to include all.");
		}, "All tracking lists present in the config which are used by the Mono profiler for tracking");
		AddCommand("carbon", "track", delegate(Arg arg)
		{
			if (!arg.HasArgs(2))
			{
				InvalidReturn(arg);
			}
			else
			{
				string text = arg.GetString(0, "");
				string text2 = arg.GetString(1, "");
				MonoProfilerConfig.ProfileTypes profileTypes = MonoProfilerConfig.ProfileTypes.Assembly;
				if (1 == 0)
				{
				}
				bool flag = text switch
				{
					"assembly" => MonoProfilerConfig.Instance.AppendProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Assembly, text2), 
					"plugin" => MonoProfilerConfig.Instance.AppendProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Plugin, text2), 
					"module" => MonoProfilerConfig.Instance.AppendProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Module, text2), 
					"ext" => MonoProfilerConfig.Instance.AppendProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Extension, text2), 
					_ => InvalidReturn(arg), 
				};
				if (1 == 0)
				{
				}
				bool flag2 = flag;
				arg.ReplyWith(flag2 ? $" Added {profileTypes} object '{text2}' to tracking" : $" Couldn't add {profileTypes} object '{text2}' for tracking");
				if (flag2)
				{
					MonoProfilerConfig.Save(configPath);
				}
			}
		}, "Adds an object to be tracked. Reloading the plugin will start tracking. Restarting required for assemblies, modules and extensions", "[assembly|plugin|module|ext] [value]");
		AddCommand("carbon", "untrack", delegate(Arg arg)
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
				bool flag = text switch
				{
					"assembly" => MonoProfilerConfig.Instance.RemoveProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Assembly, text2), 
					"plugin" => MonoProfilerConfig.Instance.RemoveProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Plugin, text2), 
					"module" => MonoProfilerConfig.Instance.RemoveProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Module, text2), 
					"ext" => MonoProfilerConfig.Instance.RemoveProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Extension, text2), 
					_ => InvalidReturn2(arg), 
				};
				if (1 == 0)
				{
				}
				bool flag2 = flag;
				arg.ReplyWith(flag2 ? $" Removed {profileTypes} object '{text2}' from tracking" : $" Couldn't remove {profileTypes} object '{text2}' for tracking");
				if (flag2)
				{
					MonoProfilerConfig.Save(configPath);
				}
			}
		}, "Removes a plugin from being tracked. Reloading the plugin will remove it from being tracked. Restarting required for assemblies, modules and extensions", "[assembly|plugin|module|ext] [value]");
		AddCommand("carbon", "profiler_version", delegate(Arg arg)
		{
			TextTable val = Pool.Get<TextTable>();
			val.Clear();
			val.AddColumns(new string[4] { "version", "protocol", "managed", "native" });
			val.AddRow(new string[4]
			{
				SelfUpdate.CurrentVersion.ToString(),
				string.Empty,
				127.ToString(),
				4.ToString()
			});
			string text = ((object)val).ToString().TrimEnd();
			Pool.FreeUnsafe<TextTable>(ref val);
			arg.ReplyWith(text);
		}, "Prints the version of Carbon profiler");
		AddCommand("carbon", "update_profiler", delegate(Arg arg)
		{
			SelfUpdate.Api(delegate(JArray data)
			{
				JToken val = ((IEnumerable<JToken>)data).FirstOrDefault((JToken x) => x[(object)"name"].ToObject<string>().Equals("profiler_build"));
				Version version = new Version(((val != null) ? val[(object)"version"].ToObject<string>() : null) + ".0");
				if (!SelfUpdate.CurrentVersion.Equals(version))
				{
					Debug.Log((object)$"Carbon.Profiler is out of date! (current {SelfUpdate.CurrentVersion}, newer {version})");
					SelfUpdate.Update(delegate
					{
						Debug.Log((object)"Updated successfully.");
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
			//IL_0056: Expected O, but got Unknown
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
		static bool InvalidReturn(Arg arg)
		{
			arg.ReplyWith("Syntax: carbon.track [assembly|plugin|module|ext] [value]");
			return false;
		}
		static bool InvalidReturn2(Arg arg)
		{
			arg.ReplyWith("Syntax: carbon.untrack [assembly|plugin|module|ext] [value]");
			return false;
		}
		static string WriteFileBytes(string extension, byte[] data)
		{
			DateTime now = DateTime.Now;
			string text = Path.Combine(profilesFolderPath, string.Format("profile-{0}_{1}_{2}_{3}{4}{5}.{6}", new object[7] { now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, extension }));
			File.WriteAllBytes(text, data);
			return "Exported profile output at '" + text + "'";
		}
		static string WriteFileString(string extension, string data)
		{
			DateTime now = DateTime.Now;
			string text = Path.Combine(profilesFolderPath, string.Format("profile-{0}_{1}_{2}_{3}{4}{5}.{6}", new object[7] { now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, extension }));
			File.WriteAllText(text, data);
			return "Exported profile output at '" + text + "'";
		}
	}

	private static void AddCommandToServerIndex(Command command)
	{
		try
		{
			if (!(typeof(Server).GetField("Dict", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) is IDictionary dictionary))
			{
				throw new InvalidOperationException("ConsoleSystem.Index.Server.Dict is not available");
			}
			Type type = dictionary.GetType().GetGenericArguments().FirstOrDefault();
			if (type == null)
			{
				throw new InvalidOperationException("Couldn't determine server command index key type '" + dictionary.GetType().FullName + "'");
			}
			object obj = ((type == typeof(string)) ? command.FullName : Activator.CreateInstance(type, command.FullName));
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
		GameObject obj = GameObject.Find("Profiler Runner");
		_runner = ((obj != null) ? obj.GetComponent<FacepunchBehaviour>() : null);
		IsCarbonInstalled = Type.GetType("Carbon.Community,Carbon.Common") != null;
		IsOxideInstalled = Type.GetType("Oxide.Core.Interface,Oxide.Core") != null;
		IsAlreadyInstalled = (Object)(object)_runner != (Object)null;
		ProfileSample = MonoProfiler.Sample.Create();
		commands = new List<Command>();
	}
}
