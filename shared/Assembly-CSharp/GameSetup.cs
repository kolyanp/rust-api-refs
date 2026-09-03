using System.Collections;
using System.IO;
using ConVar;
using Facepunch;
using Network;
using Rust;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSetup : MonoBehaviour
{
	public enum GameModeOverride
	{
		DontOverride,
		Vanilla,
		Softcore,
		Hardcore,
		Primitive,
		Deathmatch,
		TeamDeathmatch,
		OneDeath,
		KingOfTheHillDM,
		KingOfTheHillTeam,
		WeaponTest
	}

	public static bool RunOnce;

	public bool startServer = true;

	public string clientConnectCommand = "client.connect 127.0.0.1:28015";

	public bool loadMenu = true;

	public bool loadLevel;

	public string loadLevelScene = "";

	public string initializationFile = "";

	public string initializationCommands = "";

	public bool normalRendering;

	public bool loadDemo;

	public string loadDemoName = string.Empty;

	public bool forceDeepSea;

	public GameModeOverride gameMode;

	private static string GameModeShortName(GameModeOverride mode)
	{
		return mode switch
		{
			GameModeOverride.Vanilla => "vanilla", 
			GameModeOverride.Softcore => "softcore", 
			GameModeOverride.Hardcore => "hardcore", 
			GameModeOverride.Primitive => "primitive", 
			GameModeOverride.Deathmatch => "deathmatch", 
			GameModeOverride.TeamDeathmatch => "teamdeathmatch", 
			GameModeOverride.OneDeath => "onedeath", 
			GameModeOverride.KingOfTheHillDM => "kingofthehillDM", 
			GameModeOverride.KingOfTheHillTeam => "kingofthehillTeam", 
			GameModeOverride.WeaponTest => "weapontest", 
			_ => null, 
		};
	}

	protected void Awake()
	{
		if (RunOnce)
		{
			GameManager.Destroy(((Component)this).gameObject);
			return;
		}
		if (!string.IsNullOrEmpty(initializationCommands))
		{
			CommandLine.Force(CommandLine.Full + " " + initializationCommands);
		}
		Render.use_normal_rendering = normalRendering;
		GameManifest.Load();
		GameManifest.LoadAssets();
		RunOnce = true;
		if (Bootstrap.needsSetup)
		{
			Bootstrap.Init_Tier0();
			if (!string.IsNullOrEmpty(initializationFile))
			{
				if (!File.Exists(initializationFile))
				{
					Debug.Log((object)("Unable to load " + initializationFile + ", does not exist"));
				}
				else
				{
					Debug.Log((object)("Loading initialization file: " + initializationFile));
					ConsoleSystem.RunFile(ConsoleSystem.Option.Server, File.ReadAllText(initializationFile));
				}
			}
			if (!string.IsNullOrEmpty(initializationCommands))
			{
				string[] array = initializationCommands.Split(';');
				foreach (string text in array)
				{
					Debug.Log((object)("Running initialization command: " + text));
					string strCommand = text.Trim();
					ConsoleSystem.Run(ConsoleSystem.Option.Server, strCommand);
				}
			}
			Bootstrap.Init_Systems();
			Bootstrap.Init_Config();
		}
		((MonoBehaviour)this).StartCoroutine(DoGameSetup());
	}

	private IEnumerator DoGameSetup()
	{
		Application.isLoading = true;
		TerrainMeta.InitNoTerrain();
		ItemManager.Initialize();
		Scene activeScene = SceneManager.GetActiveScene();
		LevelManager.CurrentLevelName = ((Scene)(ref activeScene)).name;
		if (startServer)
		{
			string value = GameModeShortName(gameMode);
			if (!string.IsNullOrEmpty(value))
			{
				ConVar.Server.gamemode = value;
			}
			yield return ((MonoBehaviour)this).StartCoroutine(Bootstrap.StartNexusServer());
		}
		if (loadLevel && !string.IsNullOrEmpty(loadLevelScene))
		{
			Net.sv.Reset();
			ConVar.Server.level = loadLevelScene;
			UI_LoadingScreen.Update("LOADING SCENE");
			Application.LoadLevelAdditive(loadLevelScene);
			UI_LoadingScreen.Update(loadLevelScene.ToUpper() + " LOADED");
		}
		if (startServer)
		{
			yield return ((MonoBehaviour)this).StartCoroutine(StartServer());
		}
		yield return null;
		Application.isLoading = false;
	}

	private IEnumerator StartServer()
	{
		ConVar.GC.collect();
		ConVar.GC.unload();
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		bool doLoad = false;
		string empty = string.Empty;
		yield return ((MonoBehaviour)this).StartCoroutine(Bootstrap.StartServer(doLoad, empty, allowOutOfDateSaves: true));
	}
}
