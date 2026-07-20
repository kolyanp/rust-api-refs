using System;
using System.Collections;
using System.Collections.Generic;
using Facepunch;
using Facepunch.Extend;
using Network;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelManager
{
	public static string CurrentLevelName;

	private static readonly Queue<int> taskQueue = new Queue<int>();

	private static int nextTaskId = 0;

	private const string emptySceneName = "EmptyLastScene";

	public static bool isLoaded
	{
		get
		{
			if (CurrentLevelName == null)
			{
				return false;
			}
			if (CurrentLevelName == "")
			{
				return false;
			}
			if (CurrentLevelName == "Empty")
			{
				return false;
			}
			if (CurrentLevelName == "MenuBackground")
			{
				return false;
			}
			return true;
		}
	}

	public static bool IsValid(string strName)
	{
		return Application.CanStreamedLevelBeLoaded(strName);
	}

	public static IEnumerator LoadLevelAsync(string strName, bool keepLoadingScreenOpen = true, bool showLoadingScreen = true)
	{
		int taskId = nextTaskId++;
		taskQueue.Enqueue(taskId);
		while (taskQueue.Peek() != taskId)
		{
			yield return null;
		}
		try
		{
			if (strName == "proceduralmap")
			{
				strName = "Procedural Map";
			}
			Log("Loading level: " + strName);
			Scene sceneByName = SceneManager.GetSceneByName("EmptyLastScene");
			if (!((Scene)(ref sceneByName)).IsValid())
			{
				SceneManager.CreateScene("EmptyLastScene");
			}
			List<string> list = new List<string>();
			int sceneCount = SceneManager.sceneCount;
			for (int i = 0; i < sceneCount; i++)
			{
				Scene sceneAt = SceneManager.GetSceneAt(i);
				string name = ((Scene)(ref sceneAt)).name;
				if (CanUnloadScene(name))
				{
					list.Add(name);
				}
			}
			int num = List.FindIndex<string>((IReadOnlyList<string>)list, CurrentLevelName, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
			if (num >= 0)
			{
				string item = list[num];
				list.RemoveAt(num);
				list.Add(item);
			}
			List<GameObject> list2 = Pool.Get<List<GameObject>>();
			foreach (string item2 in list)
			{
				Log("Disabling all objects in scene: " + item2);
				Scene sceneByName2 = SceneManager.GetSceneByName(item2);
				if (!((Scene)(ref sceneByName2)).IsValid())
				{
					Debug.LogWarning((object)("Cannot disable objects in scene because it was not found: " + item2));
					continue;
				}
				list2.Clear();
				((Scene)(ref sceneByName2)).GetRootGameObjects(list2);
				foreach (GameObject item3 in list2)
				{
					if (!((Object)(object)item3 == (Object)null))
					{
						item3.SetActive(false);
					}
				}
			}
			Pool.FreeUnmanaged<GameObject>(ref list2);
			List<AsyncOperation> list3 = new List<AsyncOperation>();
			foreach (string item4 in list)
			{
				AsyncOperation val = SceneManager.UnloadSceneAsync(item4);
				if (val == null)
				{
					Debug.LogError((object)("Failed to unload scene: " + item4));
					continue;
				}
				Log("Unloading scene: " + item4);
				list3.Add(val);
			}
			foreach (AsyncOperation item5 in list3)
			{
				yield return item5;
			}
			Net.sv.Reset();
			Log("Loading scene: " + strName);
			AsyncOperation loadOp = SceneManager.LoadSceneAsync(strName, (LoadSceneMode)1);
			if (loadOp == null)
			{
				Debug.LogError((object)("Failed to load level: " + strName));
				yield break;
			}
			loadOp.allowSceneActivation = false;
			Scene newScene = SceneManager.GetSceneByName(strName);
			while (!loadOp.isDone)
			{
				if (loadOp.progress >= 0.9f)
				{
					Log("Level " + strName + " loaded, activating...");
					loadOp.allowSceneActivation = true;
					CurrentLevelName = strName;
				}
				yield return null;
			}
			Log("Making " + strName + " the default scene");
			SceneManager.SetActiveScene(newScene);
			Log("Level " + strName + " loaded successfully.");
		}
		finally
		{
			taskQueue.Dequeue();
		}
		static bool CanUnloadScene(string sceneName)
		{
			if (string.Equals(sceneName, "DontDestroyOnLoad", StringComparison.OrdinalIgnoreCase) || string.Equals(sceneName, "EmptyLastScene", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (AssetBundleBackend.Enabled)
			{
				foreach (Entry scene in AssetSceneManifest.Current.Scenes)
				{
					if (string.Equals(sceneName, scene.Name, StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public static IEnumerator UnloadLevelAsync(bool loadingScreen = true)
	{
		yield return LoadLevelAsync("Empty", keepLoadingScreenOpen: false, showLoadingScreen: false);
	}

	private static void Log(string message)
	{
	}
}
