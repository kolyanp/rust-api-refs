using System;
using System.IO;
using System.Reflection;
using API.Events;
using Carbon.Components;
using Components;
using HarmonyLib;
using Patches;
using UnityEngine;
using Utility;

namespace Carbon;

public sealed class Bootstrap
{
	private static readonly string identifier;

	private static readonly string assemblyName;

	private static GameObject _gameObject;

	private static Harmony _harmonyInstance;

	internal static AnalyticsManager Analytics;

	internal static AssemblyManager AssemblyEx;

	internal static CommandManager Commands;

	internal static DownloadManager Downloader;

	internal static EventManager Events;

	internal static FileWatcherManager Watcher;

	public static string Name => assemblyName;

	internal static Harmony Harmony => _harmonyInstance;

	static Bootstrap()
	{
		ConVarSnapshots.TakeSnapshot();
		identifier = $"{Guid.NewGuid():N}";
		assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
	}

	public static void Initialize()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		Utility.Logger.Log(assemblyName + " loaded.");
		_harmonyInstance = new Harmony(identifier);
		string text = Path.Combine(Context.CarbonLogs, "Carbon.Harmony.log");
		Environment.SetEnvironmentVariable("HARMONY_LOG_FILE", text);
		typeof(FileLog).GetField("_logPathInited", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, false);
		Harmony.DEBUG = false;
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		_gameObject = new GameObject("Carbon");
		Object.DontDestroyOnLoad((Object)(object)_gameObject);
		Commands = _gameObject.AddComponent<CommandManager>();
		Events = _gameObject.AddComponent<EventManager>();
		Watcher = _gameObject.AddComponent<FileWatcherManager>();
		Analytics = _gameObject.AddComponent<AnalyticsManager>();
		Downloader = _gameObject.AddComponent<DownloadManager>();
		Events.Subscribe(CarbonEvent.StartupShared, delegate
		{
			AssemblyEx = _gameObject.AddComponent<AssemblyManager>();
			AssemblyEx.Components.Load("Carbon.dll", "CarbonEvent.StartupShared");
		});
		Events.Subscribe(CarbonEvent.CarbonStartupComplete, delegate
		{
			((Behaviour)Watcher).enabled = true;
		});
		try
		{
			Utility.Logger.Log("Applying Harmony patches");
			Harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
		catch (Exception ex)
		{
			Utility.Logger.Error("Unable to apply all patches", ex);
		}
		Events.Subscribe(CarbonEvent.HooksInstalled, delegate
		{
			FileSystem_WarmupHalt.IsReady = true;
		});
	}
}
