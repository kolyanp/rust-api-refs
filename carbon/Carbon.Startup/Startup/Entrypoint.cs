using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using API.Assembly;
using Carbon.Publicizer;
using Carbon.Startup.Core;
using Carbon.Startup.Extensions;
using Doorstop.Utility;
using HarmonyLib;

namespace Startup;

[SuppressUnmanagedCodeSecurity]
public sealed class Entrypoint
{
	private static readonly string[] PreloadPostUpdate = new string[9]
	{
		Path.Combine(Defines.GetLibFolder(), "0Harmony.dll"),
		Path.Combine(Defines.GetLibFolder(), "Ben.Demystifier.dll"),
		Path.Combine(Defines.GetLibFolder(), "ZstdSharp.dll"),
		Path.Combine(Defines.GetLibFolder(), "SharpCompress.dll"),
		Path.GetFullPath(Path.Combine(Defines.GetManagedFolder(), "Carbon.Compat.dll")),
		Path.GetFullPath(Path.Combine(Defines.GetLibFolder(), "protobuf-net.dll")),
		Path.GetFullPath(Path.Combine(Defines.GetLibFolder(), "protobuf-net.Core.dll")),
		Path.GetFullPath(Path.Combine(Defines.GetLibFolder(), "System.Collections.Immutable.dll")),
		Path.GetFullPath(Path.Combine(Defines.GetLibFolder(), "System.Diagnostics.DiagnosticSource.dll"))
	};

	private static readonly string[] Delete = new string[15]
	{
		Path.Combine(Defines.GetRustManagedFolder(), "x64"),
		Path.Combine(Defines.GetRustManagedFolder(), "x86"),
		Path.Combine(Defines.GetRustManagedFolder(), "Microsoft.CodeAnalysis.CSharp.dll"),
		Path.Combine(Defines.GetRustManagedFolder(), "Microsoft.CodeAnalysis.dll"),
		Path.Combine(Defines.GetRustManagedFolder(), "System.Collections.Immutable.dll"),
		Path.Combine(Defines.GetRustManagedFolder(), "Oxide.Common.dll"),
		Path.Combine(Defines.GetRustManagedFolder(), "Oxide.Core.dll"),
		Path.Combine(Defines.GetRustManagedFolder(), "Oxide.CSharp.dll"),
		Path.Combine(Defines.GetRustManagedFolder(), "Oxide.MySql.dll"),
		Path.Combine(Defines.GetRustManagedFolder(), "Oxide.References.dll"),
		Path.Combine(Defines.GetRustManagedFolder(), "Oxide.Rust.dll"),
		Path.Combine(Defines.GetRustManagedFolder(), "Oxide.SQLite.dll"),
		Path.Combine(Defines.GetRustManagedFolder(), "Oxide.Unity.dll"),
		Path.Combine(Defines.GetLibFolder(), "UniTask.dll"),
		Path.Combine(Defines.GetManagedFolder(), "Carbon.UniTask.dll")
	};

	private static readonly Dictionary<(string directory, string filter), string> WildcardMove = new Dictionary<(string, string), string> { [(Defines.GetRustManagedFolder(), "Oxide.Ext.")] = Path.Combine(new string[1] { Defines.GetExtensionsFolder() }) };

	private static readonly Dictionary<string, string> CopyTargetEmpty = new Dictionary<string, string>
	{
		[Path.Combine(Defines.GetRustRootFolder(), "oxide", "config")] = Path.Combine(Defines.GetRootFolder(), "configs"),
		[Path.Combine(Defines.GetRustRootFolder(), "oxide", "data")] = Path.Combine(Defines.GetRootFolder(), "data"),
		[Path.Combine(Defines.GetRustRootFolder(), "oxide", "plugins")] = Path.Combine(Defines.GetRootFolder(), "plugins"),
		[Path.Combine(Defines.GetRustRootFolder(), "oxide", "lang")] = Path.Combine(Defines.GetRootFolder(), "lang")
	};

	private static readonly Dictionary<string, string> Move = new Dictionary<string, string> { [Path.Combine(Defines.GetRootFolder(), "harmony")] = Path.Combine(Defines.GetRustRootFolder(), "HarmonyMods") };

	private static readonly Dictionary<string, string> Rename = new Dictionary<string, string>
	{
		[Path.Combine(Defines.GetRootFolder(), "carbonauto.cfg")] = Path.Combine(Defines.GetRootFolder(), "config.auto.json"),
		[Path.Combine(Defines.GetRootFolder(), "config.auto.cfg")] = Path.Combine(Defines.GetRootFolder(), "config.auto.json")
	};

	[DllImport("CarbonNative")]
	public unsafe static extern void init_profiler(char* ptr, int length);

	[DllImport("__Internal", CharSet = CharSet.Ansi)]
	public static extern void mono_dllmap_insert(ModuleHandle assembly, string dll, string func, string tdll, string tfunc);

	public unsafe static void InitNative()
	{
		mono_dllmap_insert(ModuleHandle.EmptyHandle, "CarbonNative", null, Path.Combine(Defines.GetRootFolder(), "native", "CarbonNative.dll"), null);
		string text = Path.Combine(Defines.GetRootFolder(), "config.profiler.json");
		fixed (char* ptr = text)
		{
			init_profiler(ptr, text.Length);
		}
	}

	public static void Start()
	{
		Config.Init(Defines.GetConfigFile());
		Logger.Log($" Initialized Carbon.Startup {typeof(Entrypoint).Assembly.GetName().Version}");
		string[] preloadPostUpdate = PreloadPostUpdate;
		foreach (string text in preloadPostUpdate)
		{
			try
			{
				Assembly assembly = Assembly.LoadFile(text);
				Logger.Log($" Preloaded {assembly.GetName().Name} {assembly.GetName().Version}");
			}
			catch (Exception ex)
			{
				Logger.Log("Unable to preload '" + text + "' (" + ex?.Message + ")");
			}
		}
		PerformPatch();
		PerformStartup();
	}

	public static void PerformPatch()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		new Harmony("com.carbon.locationpatch").PatchCategory("location");
	}

	public static void PerformStartup()
	{
		try
		{
			InitNative();
		}
		catch (Exception ex)
		{
			Logger.Error("Failed to init native", ex);
		}
		IEnumerable<string> enumerable = Directory.EnumerateFiles(Defines.GetRustManagedFolder());
		Patch.onBufferUpdate = delegate((string path, byte[] buffer) arg)
		{
			PatchedAssemblies.AssemblyCache[Path.GetFileNameWithoutExtension(arg.path)] = arg.buffer;
		};
		Patch.Init(Defines.GetModifierFolder(), Defines.GetManagedFolder(), Defines.GetRustManagedFolder());
		foreach (string item in enumerable)
		{
			try
			{
				string name = Path.GetFileName(item);
				Patch patch = BuiltInPatches.Current.FirstOrDefault((Patch x) => x.fileName.Equals(name));
				if (patch != null && patch.Execute())
				{
					patch.Load();
					if (Config.Singleton.DeveloperMode)
					{
						patch.Write(Path.Combine(Defines.GetDeveloperPatchedAssembliesFolder(), name));
					}
				}
				else
				{
					if (!Config.Singleton.Publicizer.PublicizedAssemblies.Any((string x) => name.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
					{
						continue;
					}
					patch = new Patch(Path.GetDirectoryName(item), name);
					if (patch.Execute())
					{
						patch.Load();
						if (Config.Singleton.DeveloperMode)
						{
							patch.Write(Path.Combine(Defines.GetDeveloperPatchedAssembliesFolder(), name));
						}
					}
					continue;
				}
			}
			catch (Exception ex2)
			{
				Logger.Error("Failed to patch", ex2);
			}
		}
		Patch.Uninit();
		try
		{
			PerformMove();
			PerformWildcardMove();
			PerformRename();
			PerformCleanup();
			PerformCopyTargetEmpty();
		}
		catch (Exception ex3)
		{
			Logger.Error("Preloader fatal failure", ex3);
		}
	}

	public static void PerformCleanup()
	{
		string[] delete = Delete;
		foreach (string text in delete)
		{
			try
			{
				if (!File.Exists(text))
				{
					if (Directory.Exists(text))
					{
						Logger.Log(" Removed '" + Path.GetFileName(text) + "'");
						Directory.Delete(text, recursive: true);
					}
				}
				else
				{
					Logger.Log(" Removed '" + Path.GetFileName(text) + "'");
					File.Delete(text);
				}
			}
			catch (Exception ex)
			{
				Logger.Error(" Cleanup process error! Failed removing '" + text + "'", ex);
			}
		}
	}

	public static void PerformWildcardMove()
	{
		foreach (KeyValuePair<(string, string), string> item in WildcardMove)
		{
			string[] files = Directory.GetFiles(item.Key.Item1);
			string[] array = files;
			foreach (string text in array)
			{
				if (text.Contains(item.Key.Item2))
				{
					string text2 = Path.Combine(item.Value, Path.GetFileName(text));
					if (!File.Exists(text2))
					{
						File.Move(text, text2);
						Logger.Log(" Moved " + Path.GetFileName(text) + " -> carbon/" + Path.GetFileName(item.Value));
					}
				}
			}
		}
	}

	public static void PerformCopyTargetEmpty()
	{
		if (CopyTargetEmpty.Any((KeyValuePair<string, string> x) => Directory.Exists(x.Value) && new DirectoryInfo(x.Value).GetFiles().Any()) || !Directory.Exists(Path.Combine(Defines.GetRustRootFolder(), "oxide")))
		{
			return;
		}
		Logger.Log(" Fresh Carbon installation detected. Migrating Oxide directories.");
		foreach (KeyValuePair<string, string> item in CopyTargetEmpty)
		{
			if (!Directory.Exists(item.Key))
			{
				continue;
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(item.Value);
			if (!directoryInfo.GetFiles().Any())
			{
				try
				{
					Logger.Log(" Copied oxide/" + Path.GetFileName(item.Key) + " -> carbon/" + Path.GetFileName(item.Value));
					OsEx.Copy(item.Key, item.Value);
				}
				catch (Exception ex)
				{
					Logger.Debug(" Unable to copy '" + item.Key + "' -> '" + item.Value + "' (" + ex?.Message + ")");
				}
			}
		}
	}

	public static void PerformMove()
	{
		foreach (KeyValuePair<string, string> item in Move)
		{
			if (Directory.Exists(item.Key))
			{
				if (!Directory.Exists(item.Value))
				{
					Directory.CreateDirectory(item.Value);
				}
				try
				{
					OsEx.Move(item.Key, item.Value);
				}
				catch (Exception ex)
				{
					Logger.Debug(" Unable to move '" + item.Key + "' -> '" + item.Value + "' (" + ex?.Message + ")");
				}
			}
		}
	}

	public static void PerformRename()
	{
		foreach (KeyValuePair<string, string> item in Rename)
		{
			try
			{
				if (File.Exists(item.Key))
				{
					File.Move(item.Key, item.Value);
				}
			}
			catch (Exception ex)
			{
				Logger.Debug(" Unable to rename '" + item.Key + "' -> '" + item.Value + "' (" + ex?.Message + ")");
			}
		}
	}
}
