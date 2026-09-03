using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CompanionServer;
using CompanionServer.Handlers;
using ConVar;
using Cysharp.Threading.Tasks;
using Development.Attributes;
using Facepunch;
using Facepunch.Network;
using Facepunch.Network.Raknet;
using Facepunch.Rust;
using Facepunch.Rust.Profiling;
using Facepunch.Utility;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Ai;
using Rust.Ai.Gen2.Nav;
using Rust.UI;
using UnityEngine;
using UnityEngine.AI;

[ResetStaticFields]
public class Bootstrap : SingletonComponent<Bootstrap>
{
	internal static bool bootstrapInitRun;

	public static bool isErrored;

	public Phrase currentLoadingPhrase;

	private float currentLoadingProgress;

	public CanvasGroup BootstrapUiCanvas;

	public GameObject errorPanel;

	public RustText errorText;

	public RustText statusText;

	private const bool fastBootstrap = false;

	private Phrase openingBundles;

	private static string loadingStepName;

	private static RealTimeSince timeSinceStepStart;

	private static RealTimeSince timeSinceBootstrapStart;

	private static string lastWrittenValue;

	public static bool needsSetup => !bootstrapInitRun;

	private static bool ShouldDoServerStartupYields => true;

	public static bool GameUIEnabled => true;

	public static bool isPresent
	{
		get
		{
			if (bootstrapInitRun)
			{
				return true;
			}
			if (Object.FindObjectsByType<GameSetup>((FindObjectsSortMode)0).Count() > 0)
			{
				return true;
			}
			return false;
		}
	}

	public static void RunDefaults()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
		Application.targetFrameRate = 256;
		Time.fixedDeltaTime = 0.0625f;
		Time.maximumDeltaTime = 0.125f;
	}

	public static void Init_Tier0()
	{
		RunDefaults();
		GameSetup.RunOnce = true;
		bootstrapInitRun = true;
		ConsoleSystem.Index.Initialize(ConsoleGen.All);
		ConsoleSystem.Index.Reset();
		UnityButtons.Register();
		ExceptionReporter.InstallHook();
		Output.Install();
		Pool.ResizeBuffer<NetRead>(16384);
		Pool.ResizeBuffer<NetWrite>(16384);
		Pool.ResizeBuffer<BufferStream>(32768);
		Pool.ResizeBuffer<Networkable>(65536);
		Pool.ResizeBuffer<EntityLink>(65536);
		ConsoleSystem.Run(ConsoleSystem.Option.Unrestricted, "version");
		Pool.ResizeBuffer<EventRecord>(16384);
		Pool.ResizeBuffer<SellOrder>(2048);
		Pool.ResizeBuffer<ListHashSet<BaseNetworkable>>(2048);
		Pool.FillBuffer<Networkable>();
		Pool.FillBuffer<EntityLink>();
		if (CommandLine.HasSwitch("-nonetworkthread"))
		{
			BaseNetwork.Multithreading = false;
		}
		SteamNetworking.SetDebugFunction();
		if (CommandLine.HasSwitch("-swnet"))
		{
			NetworkInitSteamworks(enableSteamDatagramRelay: false);
		}
		else if (CommandLine.HasSwitch("-sdrnet"))
		{
			NetworkInitSteamworks(enableSteamDatagramRelay: true);
		}
		else if (CommandLine.HasSwitch("-raknet"))
		{
			NetworkInitRaknet();
		}
		else
		{
			NetworkInitRaknet();
		}
		AI.useUnityNavmesh = !CommandLine.HasSwitch("-useNewNavmesh");
		AI.checkTileValid = CommandLine.HasSwitch("-checkTileValid");
		if (!Application.isEditor)
		{
			string text = CommandLine.Full.Replace(CommandLine.GetSwitch("-rcon.password", CommandLine.GetSwitch("+rcon.password", "RCONPASSWORD")), "******");
			WriteToLog("Command Line: " + text);
		}
		Interface.Initialize();
		int parentProcessId = CommandLine.GetSwitchInt("-parent-pid", 0);
		if (parentProcessId != 0)
		{
			try
			{
				SynchronizationContext syncContext = SynchronizationContext.Current;
				Process processById = Process.GetProcessById(parentProcessId);
				processById.EnableRaisingEvents = true;
				processById.Exited += delegate
				{
					syncContext.Post(delegate
					{
						WriteToLog($"Parent process ID {parentProcessId} exited. Exiting the server now...");
						ConsoleSystem.Run(ConsoleSystem.Option.Server, "quit");
					}, null);
				};
				WriteToLog($"Watching parent process ID {parentProcessId}...");
			}
			catch (ArgumentException)
			{
				WriteToLog($"Parent process ID {parentProcessId} has exited during boot! Exiting now...");
				Application.Quit();
			}
		}
		UnityHookHandler.EnsureCreated();
		Awaiter.SetPoolRunnersActive(true);
	}

	public static void Init_Systems()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		Global.Init();
		Translate.Init();
		Integration val = new Integration();
		val.OnManifestUpdated += CpuAffinity.Apply;
		Application.Initialize((BaseIntegration)val);
		Performance.GetMemoryUsage = () => SystemInfoEx.systemMemoryUsed;
		PostUpdateHook.OnLateUpdate = (Action)Delegate.Combine(PostUpdateHook.OnLateUpdate, new Action(RuntimeProfiler.Update));
	}

	public static void Init_Config()
	{
		ConsoleNetwork.Init();
		ConsoleSystem.UpdateValuesFromCommandLine();
		ConsoleSystem.Run(ConsoleSystem.Option.Server, "server.readcfg");
		ServerUsers.Load();
		if (string.IsNullOrEmpty(ConVar.Server.server_id))
		{
			ConVar.Server.server_id = Guid.NewGuid().ToString("N");
			ConsoleSystem.Run(ConsoleSystem.Option.Server, "server.writecfg");
		}
		if (CommandLine.HasSwitch("-disable-server-occlusion"))
		{
			ServerOcclusion.OcclusionEnabled = false;
			ServerOcclusion.OcclusionIncludeRocks = false;
		}
		if (CommandLine.HasSwitch("-disable-server-occlusion-rocks"))
		{
			ServerOcclusion.OcclusionIncludeRocks = false;
		}
		HttpManager.UpdateMaxConnections();
		if (!RuntimeProfiler.runtime_profiling_persist)
		{
			RuntimeProfiler.Disable();
		}
		if (!CommandLine.HasSwitch("-disableconsolelog"))
		{
			ConsoleSystem.loggingEnabled = true;
		}
		ConsoleSystem.IdentityDirectory = ConVar.Server.rootFolder;
		ConsoleSystem.Run(ConsoleSystem.Option.Server, "relay.cfg_reload");
	}

	public static void NetworkInitRaknet()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		Net.sv = (Network.Server)new Server();
	}

	public static void NetworkInitSteamworks(bool enableSteamDatagramRelay)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		Net.sv = (Network.Server)new Server(enableSteamDatagramRelay);
	}

	private unsafe IEnumerator Start()
	{
		WriteToLog("Bootstrap Startup");
		timeSinceBootstrapStart = RealTimeSince.op_Implicit(0f);
		EarlyInitialize();
		BenchmarkTimer.Enabled = CommandLine.Full.Contains("+autobench");
		Stopwatch timer = BenchmarkTimer.Get("bootstrap");
		timer?.Start();
		if (!Application.isEditor)
		{
			BuildInfo current = BuildInfo.Current;
			bool flag = (current.Scm.Branch != null && current.Scm.Branch == "experimental/release") || current.Scm.Branch == "release";
			ExceptionReporter.Initialize("https://gw.facepunch.com/facetry/errors", flag ? "server-release" : "server-staging");
			bool num = CommandLine.Full.Contains("-official") || CommandLine.Full.Contains("-server.official") || CommandLine.Full.Contains("+official") || CommandLine.Full.Contains("+server.official");
			bool flag2 = CommandLine.Full.Contains("-stats") || CommandLine.Full.Contains("-server.stats") || CommandLine.Full.Contains("+stats") || CommandLine.Full.Contains("+server.stats");
			ExceptionReporter.Disabled = !(num & flag2);
		}
		Scope val;
		Scope val2;
		if (AssetBundleBackend.Enabled)
		{
			AssetBundleBackend newBackend = new AssetBundleBackend();
			val = BenchmarkTimer.Measure("bootstrap;bundles");
			try
			{
				yield return ((MonoBehaviour)this).StartCoroutine(LoadingUpdate(openingBundles));
				char directorySeparatorChar = Path.DirectorySeparatorChar;
				newBackend.Load("Bundles" + directorySeparatorChar + "Bundles");
				FileSystem.Backend = (FileSystemBackend)(object)newBackend;
			}
			finally
			{
				((IDisposable)(*(Scope*)(&val))/*cast due to constrained. prefix*/).Dispose();
			}
			if (FileSystem.Backend.isError)
			{
				ThrowError(FileSystem.Backend.loadingError);
				yield break;
			}
			val2 = BenchmarkTimer.Measure("bootstrap;bundlesindex");
			try
			{
				newBackend.BuildFileIndex();
			}
			finally
			{
				((IDisposable)(*(Scope*)(&val2))/*cast due to constrained. prefix*/).Dispose();
			}
			while (true)
			{
				if (FileSystem.Backend.isError)
				{
					ThrowError(FileSystem.Backend.loadingError);
					yield break;
				}
				float assetSceneProgress = newBackend.GetAssetSceneProgress("AssetScene-bootstrap");
				if (assetSceneProgress >= 1f)
				{
					break;
				}
				yield return ((MonoBehaviour)this).StartCoroutine(LoadingUpdate(Phrase.op_Implicit("Loading Menu Prefabs"), assetSceneProgress));
			}
		}
		if (FileSystem.Backend.isError)
		{
			ThrowError(FileSystem.Backend.loadingError);
			yield break;
		}
		if (!Application.isEditor)
		{
			WriteToLog(SystemInfoGeneralText.currentInfo);
		}
		Texture.SetGlobalAnisotropicFilteringLimits(1, 16);
		if (isErrored)
		{
			yield break;
		}
		val = BenchmarkTimer.Measure("bootstrap;gamemanifest");
		try
		{
			yield return ((MonoBehaviour)this).StartCoroutine(LoadingUpdate(Phrase.op_Implicit("Loading Game Manifest")));
			GameManifest.Load();
			yield return ((MonoBehaviour)this).StartCoroutine(LoadingUpdate(Phrase.op_Implicit("DONE!")));
		}
		finally
		{
			((IDisposable)(*(Scope*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
		val = BenchmarkTimer.Measure("bootstrap;selfcheck");
		try
		{
			yield return ((MonoBehaviour)this).StartCoroutine(LoadingUpdate(Phrase.op_Implicit("Running Self Check")));
			SelfCheck.Run();
		}
		finally
		{
			((IDisposable)(*(Scope*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
		if (isErrored)
		{
			yield break;
		}
		yield return ((MonoBehaviour)this).StartCoroutine(LoadingUpdate(Phrase.op_Implicit("Bootstrap Tier0")));
		val2 = BenchmarkTimer.Measure("bootstrap;tier0");
		try
		{
			Init_Tier0();
		}
		finally
		{
			((IDisposable)(*(Scope*)(&val2))/*cast due to constrained. prefix*/).Dispose();
		}
		val2 = BenchmarkTimer.Measure("bootstrap;commandlinevalues");
		try
		{
			ConsoleSystem.UpdateValuesFromCommandLine();
		}
		finally
		{
			((IDisposable)(*(Scope*)(&val2))/*cast due to constrained. prefix*/).Dispose();
		}
		yield return ((MonoBehaviour)this).StartCoroutine(LoadingUpdate(Phrase.op_Implicit("Bootstrap Systems")));
		val2 = BenchmarkTimer.Measure("bootstrap;init_systems");
		try
		{
			Init_Systems();
		}
		finally
		{
			((IDisposable)(*(Scope*)(&val2))/*cast due to constrained. prefix*/).Dispose();
		}
		yield return ((MonoBehaviour)this).StartCoroutine(LoadingUpdate(Phrase.op_Implicit("Bootstrap Config")));
		val2 = BenchmarkTimer.Measure("bootstrap;init_config");
		try
		{
			Init_Config();
		}
		finally
		{
			((IDisposable)(*(Scope*)(&val2))/*cast due to constrained. prefix*/).Dispose();
		}
		val2 = BenchmarkTimer.Measure("bootstrap;commandlinevalues2");
		try
		{
			ConsoleSystem.UpdateValuesFromCommandLine();
		}
		finally
		{
			((IDisposable)(*(Scope*)(&val2))/*cast due to constrained. prefix*/).Dispose();
		}
		if (!isErrored)
		{
			yield return ((MonoBehaviour)this).StartCoroutine(LoadingUpdate(Phrase.op_Implicit("Loading Items")));
			val2 = BenchmarkTimer.Measure("bootstrap;itemmanager");
			try
			{
				ItemManager.Initialize();
			}
			finally
			{
				((IDisposable)(*(Scope*)(&val2))/*cast due to constrained. prefix*/).Dispose();
			}
			if (!isErrored)
			{
				yield return ((MonoBehaviour)this).StartCoroutine(DedicatedServerStartup());
				timer?.Stop();
				WriteToLog($"[Bootstrap] completed in {RealTimeSince.op_Implicit(timeSinceBootstrapStart):0.00}s");
				GameManager.Destroy(((Component)this).gameObject);
			}
		}
	}

	private IEnumerator DedicatedServerStartup()
	{
		Application.isLoading = true;
		Application.backgroundLoadingPriority = (ThreadPriority)4;
		WriteToLog("Skinnable Warmup");
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		GameManifest.LoadAssets();
		WriteToLog("Initializing Nexus");
		yield return ((MonoBehaviour)this).StartCoroutine(StartNexusServer());
		WriteToLog("Loading Scene");
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		Physics.defaultSolverIterations = 3;
		int num = PlayerPrefs.GetInt("UnityGraphicsQuality");
		QualitySettings.SetQualityLevel(0);
		PlayerPrefs.SetInt("UnityGraphicsQuality", num);
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		Object.DontDestroyOnLoad((Object)(object)GameManager.server.CreatePrefab("assets/bundled/prefabs/system/server_console.prefab"));
		StartupShared();
		World.InitSize(ConVar.Server.worldsize);
		World.InitSeed(ConVar.Server.seed);
		World.InitSalt(ConVar.Server.salt);
		World.Url = ConVar.Server.levelurl;
		World.Transfer = ConVar.Server.leveltransfer;
		yield return LevelManager.LoadLevelAsync(ConVar.Server.level);
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		yield return ((MonoBehaviour)this).StartCoroutine(FileSystem_Warmup.Run(WriteToLog, "Asset Warmup ({0}/{1})"));
		yield return ((MonoBehaviour)this).StartCoroutine(StartServer(!CommandLine.HasSwitch("-skipload"), "", allowOutOfDateSaves: false));
		if (!Object.op_Implicit((Object)(object)Object.FindObjectOfType<Performance>()))
		{
			Object.DontDestroyOnLoad((Object)(object)GameManager.server.CreatePrefab("assets/bundled/prefabs/system/performance.prefab"));
		}
		Rust.GC.Collect();
		Application.isLoading = false;
	}

	private static void EnsureRootFolderCreated()
	{
		try
		{
			Directory.CreateDirectory(ConVar.Server.rootFolder);
		}
		catch (Exception arg)
		{
			Debug.LogWarning((object)$"Failed to automatically create the save directory: {ConVar.Server.rootFolder}\n\n{arg}");
		}
	}

	public static IEnumerator StartNexusServer()
	{
		EnsureRootFolderCreated();
		yield return NexusServer.Initialize();
		if (NexusServer.FailedToStart)
		{
			Debug.LogError((object)"Nexus server failed to start, terminating");
			Application.Quit();
		}
	}

	public unsafe static IEnumerator StartServer(bool doLoad, string saveFileOverride, bool allowOutOfDateSaves)
	{
		float timeScale = Time.timeScale;
		if (ConVar.Time.pausewhileloading)
		{
			Time.timeScale = 0f;
		}
		RCon.Initialize();
		BaseEntity.Query.Server = new BaseEntity.Query.EntityTree(8096f);
		EnsureRootFolderCreated();
		if (Object.op_Implicit((Object)(object)SingletonComponent<WorldSetup>.Instance))
		{
			yield return ((MonoBehaviour)SingletonComponent<WorldSetup>.Instance).StartCoroutine(SingletonComponent<WorldSetup>.Instance.InitCoroutine());
		}
		if (AI.useUnityNavmesh && Object.op_Implicit((Object)(object)SingletonComponent<DynamicNavMesh>.Instance) && ((Behaviour)SingletonComponent<DynamicNavMesh>.Instance).enabled && !AiManager.nav_disable)
		{
			yield return ((MonoBehaviour)SingletonComponent<DynamicNavMesh>.Instance).StartCoroutine(SingletonComponent<DynamicNavMesh>.Instance.UpdateNavMeshAndWait());
		}
		if (Object.op_Implicit((Object)(object)SingletonComponent<AiManager>.Instance) && ((Behaviour)SingletonComponent<AiManager>.Instance).enabled)
		{
			SingletonComponent<AiManager>.Instance.Initialize();
			if (!AiManager.nav_disable && AI.npc_enable && (Object)(object)TerrainMeta.Path != (Object)null)
			{
				foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
				{
					if (monument.HasNavmesh)
					{
						yield return ((MonoBehaviour)monument).StartCoroutine(monument.GetMonumentNavMesh().UpdateNavMeshAndWait());
					}
				}
				if (Object.op_Implicit((Object)(object)TerrainMeta.Path) && World.SpawnedPrefabs.TryGetValue("Dungeon", out var value))
				{
					DungeonNavmesh dungeonNavmesh = new GameObject("DungeonGridNavMesh").AddComponent<DungeonNavmesh>();
					dungeonNavmesh.NavMeshCollectGeometry = (NavMeshCollectGeometry)1;
					dungeonNavmesh.LayerMask = LayerMask.op_Implicit(65537);
					yield return ((MonoBehaviour)dungeonNavmesh).StartCoroutine(dungeonNavmesh.UpdateNavMeshAndWait(value));
				}
				else
				{
					NavMeshTools.LogWarning("Failed to find DungeonGridRoot, NOT generating Dungeon navmesh");
				}
				if (Object.op_Implicit((Object)(object)TerrainMeta.Path) && World.SpawnedPrefabs.TryGetValue("DungeonBase", out var value2))
				{
					DungeonNavmesh dungeonNavmesh2 = new GameObject("DungeonBaseNavMesh").AddComponent<DungeonNavmesh>();
					dungeonNavmesh2.NavmeshResolutionModifier = 0.3f;
					dungeonNavmesh2.NavMeshCollectGeometry = (NavMeshCollectGeometry)1;
					dungeonNavmesh2.LayerMask = LayerMask.op_Implicit(65537);
					yield return ((MonoBehaviour)dungeonNavmesh2).StartCoroutine(dungeonNavmesh2.UpdateNavMeshAndWait(value2));
				}
				else
				{
					NavMeshTools.LogWarning("Failed to find DungeonBaseRoot , NOT generating Dungeon navmesh");
				}
				GenerateDungeonBase.SetupAI();
			}
		}
		Object.DontDestroyOnLoad((Object)(object)GameManager.server.CreatePrefab("assets/bundled/prefabs/system/shared.prefab"));
		GameObject val = GameManager.server.CreatePrefab("assets/bundled/prefabs/system/server.prefab");
		Object.DontDestroyOnLoad((Object)(object)val);
		ServerMgr serverMgr = val.GetComponent<ServerMgr>();
		bool saveWasLoaded = serverMgr.Initialize(doLoad, saveFileOverride, allowOutOfDateSaves);
		if (ShouldDoServerStartupYields)
		{
			yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		}
		if (!AI.useUnityNavmesh && !AiManager.nav_disable && Object.op_Implicit((Object)(object)RustNavigation.Instance) && ((Behaviour)RustNavigation.Instance).enabled && !RustNavigation.Instance.IsDefaultNavmeshBuilt())
		{
			RustNavigation.Log("No navmesh loaded from save, building navmesh now");
			((MonoBehaviour)RustNavigation.Instance).StartCoroutine(RustNavigation.Instance.BootstrapBuildNavMesh());
		}
		if (ShouldDoServerStartupYields)
		{
			yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		}
		SaveRestore.InitializeEntityLinks();
		if (ShouldDoServerStartupYields)
		{
			yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		}
		SaveRestore.InitializeEntitySupports();
		if (ShouldDoServerStartupYields)
		{
			yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		}
		SaveRestore.InitializeEntityConditionals();
		if (ShouldDoServerStartupYields)
		{
			yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		}
		SaveRestore.GetSaveCache();
		if (ShouldDoServerStartupYields)
		{
			yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		}
		BaseGameMode.CreateGameMode();
		if (ShouldDoServerStartupYields)
		{
			yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		}
		MissionManifest.Get();
		if (ShouldDoServerStartupYields)
		{
			yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		}
		if (Clan.enabled)
		{
			ClanManager clanManager = ClanManager.ServerInstance;
			if ((Object)(object)clanManager == (Object)null)
			{
				Debug.LogError((object)"ClanManager was not spawned!");
				Application.Quit();
				yield break;
			}
			Task initializeTask = clanManager.Initialize();
			yield return (object)new WaitUntil((Func<bool>)(() => initializeTask.IsCompleted));
			initializeTask.Wait();
			clanManager.LoadClanInfoForSleepers();
		}
		else if ((Object)(object)ClanManager.ServerInstance != (Object)null)
		{
			ClanManager.ServerInstance.Kill();
		}
		if (ShouldDoServerStartupYields)
		{
			yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		}
		if (ServerOcclusion.OcclusionEnabled)
		{
			ServerOcclusion.SetupGrid();
		}
		if (ShouldDoServerStartupYields)
		{
			yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		}
		if (NexusServer.Started)
		{
			NexusServer.UploadMapImage();
			if (saveWasLoaded)
			{
				NexusServer.RestoreUnsavedState();
			}
			NexusServer.ZoneClient.StartListening();
		}
		if (ConVar.Server.autoUploadMap)
		{
			Task uploadTask = MapUploader.UploadMap();
			while (!uploadTask.IsCompleted)
			{
				yield return null;
			}
			if (!uploadTask.IsCompletedSuccessfully)
			{
				Debug.LogError((object)"Failed to upload map file:");
				Debug.LogException((Exception)uploadTask.Exception);
			}
		}
		serverMgr.OpenConnection();
		CompanionServer.Server.Initialize();
		if (ConVar.Server.autoUploadMapImages && Map.ImageData != null)
		{
			MapUploader.UploadMapImage(Map.ImageData);
		}
		Scope val2 = BenchmarkTimer.Measure("Boombox.LoadStations");
		try
		{
			BoomBox.LoadStations();
		}
		finally
		{
			((IDisposable)(*(Scope*)(&val2))/*cast due to constrained. prefix*/).Dispose();
		}
		RustEmojiLibrary.FindAllServerEmoji();
		_ = PaintballColorLookup.instance;
		UnderwearManifest.Get();
		if (ConVar.Time.pausewhileloading)
		{
			Time.timeScale = timeScale;
		}
		WriteToLog("Server startup complete");
		Application.isServerStarted = true;
	}

	private void StartupShared()
	{
		Interface.CallHook("InitLogging");
		ItemManager.Initialize();
	}

	public bool RetrySteam()
	{
		if (!CommandLine.HasSwitch("-nosteam"))
		{
			return PlatformService.Initialize((IPlatformHooks)(object)RustPlatformHooks.Instance);
		}
		return true;
	}

	public void ThrowError(string error)
	{
		isErrored = true;
	}

	public void ClearError()
	{
		isErrored = false;
	}

	public void ThrowSteamError()
	{
		isErrored = true;
	}

	[UnityEvent]
	public void ExitGame()
	{
		Debug.Log((object)"Exiting due to Exit Game button on bootstrap error panel");
		Application.Quit();
	}

	public static IEnumerator LoadingUpdate(Phrase phrase, float progress = -1f)
	{
		if (Object.op_Implicit((Object)(object)SingletonComponent<Bootstrap>.Instance))
		{
			LogLoadingStep(phrase.english);
			SingletonComponent<Bootstrap>.Instance.currentLoadingPhrase = phrase;
			SingletonComponent<Bootstrap>.Instance.currentLoadingProgress = progress;
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
		}
	}

	private static void LogLoadingStep(string step)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!(step == loadingStepName))
		{
			if (loadingStepName != null)
			{
				WriteToLog($"[Bootstrap] {loadingStepName} done in {RealTimeSince.op_Implicit(timeSinceStepStart):0.00}s");
			}
			WriteToLog("[Bootstrap] " + step);
			loadingStepName = step;
			timeSinceStepStart = RealTimeSince.op_Implicit(0f);
		}
	}

	public static void WriteToLog(string str)
	{
		if (!(lastWrittenValue == str))
		{
			DebugEx.Log(str, (StackTraceLogType)0);
			lastWrittenValue = str;
		}
	}

	private static void EarlyInitialize()
	{
	}

	public Bootstrap()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		currentLoadingProgress = -1f;
		openingBundles = new Phrase("bootstrap.openingbundles", "Opening Bundles");
		base._002Ector();
	}
}
