using UnityEngine;

namespace ConVar;

[Factory("gamemode")]
public class gamemode : ConsoleSystem
{
	[ServerUserVar]
	public static void setteam(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (Object.op_Implicit((Object)(object)activeGameMode))
		{
			int num = arg.GetInt(0);
			if (num >= 0 && num < activeGameMode.GetNumTeams())
			{
				activeGameMode.ResetPlayerScores(basePlayer);
				activeGameMode.SetPlayerTeam(basePlayer, num);
				basePlayer.Respawn();
			}
		}
	}

	[ServerVar(Help = "(Generated) Sets the active game mode by name; game modes can alter loot tables, rules, and player abilities (e.g. softcore, hardcore)")]
	public static void set(Arg arg)
	{
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		string text = arg.GetString(0);
		if (string.IsNullOrEmpty(text))
		{
			Debug.Log((object)"Invalid gamemode");
		}
		BaseGameMode baseGameMode = null;
		GameObjectRef gameObjectRef = null;
		GameModeManifest gameModeManifest = GameModeManifest.Get();
		Debug.Log((object)("total gamemodes : " + gameModeManifest.gameModePrefabs.Count));
		foreach (GameObjectRef gameModePrefab in gameModeManifest.gameModePrefabs)
		{
			BaseGameMode component = gameModePrefab.Get().GetComponent<BaseGameMode>();
			if (component.shortname == text)
			{
				baseGameMode = component;
				gameObjectRef = gameModePrefab;
				Debug.Log((object)("Found :" + component.shortname + " prefab name is :" + component.PrefabName + ": rpath is " + gameModePrefab.resourcePath + ":"));
				break;
			}
			Debug.Log((object)("search name " + text + "searched against : " + component.shortname));
		}
		if ((Object)(object)baseGameMode == (Object)null)
		{
			Debug.Log((object)("Unknown gamemode : " + text));
			return;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (Object.op_Implicit((Object)(object)activeGameMode))
		{
			if (baseGameMode.shortname == activeGameMode.shortname)
			{
				Debug.Log((object)"Same gamemode, resetting");
			}
			if (activeGameMode.permanent)
			{
				Debug.LogError((object)"This game mode is permanent, you must reset the server to switch game modes.");
				return;
			}
			activeGameMode.ShutdownGame();
			activeGameMode.Kill();
			BaseGameMode.SetActiveGameMode(null, serverside: true);
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(gameObjectRef.resourcePath, Vector3.zero, Quaternion.identity);
		if (Object.op_Implicit((Object)(object)baseEntity))
		{
			Debug.Log((object)("Spawning new game mode : " + baseGameMode.shortname));
			baseEntity.Spawn();
		}
		else
		{
			Debug.Log((object)("Failed to create new game mode :" + baseGameMode.PrefabName));
		}
	}
}
