using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Facepunch;
using Facepunch.Rust;
using Network;
using UnityEngine;
using UnityEngine.SceneManagement;

[Factory("global")]
public class DiagnosticsConSys : ConsoleSystem
{
	public class SceneDumpOutput
	{
		public string SceneName;

		public int SceneIndex;

		public int TransformCount;

		public int TransfromPlusComponentCount;

		public Transform[] Roots;
	}

	private class PrefabContainer
	{
		public GameObject Prefab;

		public int TransformCount;

		public int TransformPlusComponentCount;

		public string SceneName;
	}

	private class GameObjectRecursiveData
	{
		public GameObject GameObject;

		public Component[] Components;

		public int Depth;

		public int TotalTransformCount;

		public int TotalComponentCount;

		public List<GameObjectRecursiveData> Children = new List<GameObjectRecursiveData>();
	}

	[ServerVar(Help = "Dumps a diagnostic snapshot of animators into a diagnostics subfolder ")]
	[ClientVar(ClientAdmin = true, Help = "Dumps a diagnostic snapshot of animators into a diagnostics subfolder ")]
	public static void dumpAnimators(Arg args)
	{
		string targetFolder = GetTargetFolder();
		DumpAnimators(targetFolder);
		args.ReplyWith("Dumped animator diagnostics to: " + targetFolder);
	}

	private static void DumpAnimators(string targetFolder)
	{
		Animator[] array = Object.FindObjectsByType<Animator>((FindObjectsSortMode)0);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("All animators");
		stringBuilder.AppendLine();
		Animator[] array2 = array;
		foreach (Animator val in array2)
		{
			stringBuilder.AppendFormat("{1}\t{0}", TransformEx.GetRecursiveName(((Component)val).transform), ((Behaviour)val).enabled);
			stringBuilder.AppendLine();
		}
		WriteTextToFile(targetFolder + "UnityEngine.Animators.List.txt", stringBuilder.ToString());
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.AppendLine("All animators - grouped by object name");
		stringBuilder2.AppendLine();
		foreach (IGrouping<string, Animator> item in from x in array
			group x by TransformEx.GetRecursiveName(((Component)x).transform) into x
			orderby x.Count() descending
			select x)
		{
			stringBuilder2.AppendFormat("{1:N0}\t{0}", TransformEx.GetRecursiveName(((Component)item.First()).transform), item.Count());
			stringBuilder2.AppendLine();
		}
		WriteTextToFile(targetFolder + "UnityEngine.Animators.Counts.txt", stringBuilder2.ToString());
		StringBuilder stringBuilder3 = new StringBuilder();
		stringBuilder3.AppendLine("All animators - grouped by enabled/disabled");
		stringBuilder3.AppendLine();
		foreach (IGrouping<string, Animator> item2 in from x in array
			group x by TransformEx.GetRecursiveName(((Component)x).transform, ((Behaviour)x).enabled ? "" : " (DISABLED)") into x
			orderby x.Count() descending
			select x)
		{
			stringBuilder3.AppendFormat("{1:N0}\t{0}", TransformEx.GetRecursiveName(((Component)item2.First()).transform, ((Behaviour)item2.First()).enabled ? "" : " (DISABLED)"), item2.Count());
			stringBuilder3.AppendLine();
		}
		WriteTextToFile(targetFolder + "UnityEngine.Animators.Counts.Enabled.txt", stringBuilder3.ToString());
	}

	[ServerVar(Help = "Dumps a diagnostic snapshot of entities into a diagnostics subfolder ")]
	[ClientVar(ClientAdmin = true, Help = "Dumps a diagnostic snapshot of entities into a diagnostics subfolder ")]
	public static void dumpEntities(Arg args)
	{
		string targetFolder = GetTargetFolder();
		DumpEntities(targetFolder);
		args.ReplyWith("Dumped entity diagnostics to: " + targetFolder);
	}

	private static void DumpEntities(string targetFolder)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("All entities");
		stringBuilder.AppendLine();
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseNetworkable current = enumerator.Current;
				stringBuilder.AppendFormat("{1}\t{0}", current.PrefabName, ((NetworkableId)(((_003F?)current.net?.ID) ?? default(NetworkableId))).Value);
				stringBuilder.AppendLine();
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		WriteTextToFile(targetFolder + "UnityEngine.Entity.SV.List.txt", stringBuilder.ToString());
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.AppendLine("All entities");
		stringBuilder2.AppendLine();
		foreach (IGrouping<uint, BaseNetworkable> item in from x in BaseNetworkable.serverEntities
			group x by x.prefabID into x
			orderby x.Count() descending
			select x)
		{
			stringBuilder2.AppendFormat("{1:N0}\t{0}", item.First().PrefabName, item.Count());
			stringBuilder2.AppendLine();
		}
		WriteTextToFile(targetFolder + "UnityEngine.Entity.SV.Counts.txt", stringBuilder2.ToString());
		StringBuilder stringBuilder3 = new StringBuilder();
		stringBuilder3.AppendLine("Saved entities");
		stringBuilder3.AppendLine();
		foreach (IGrouping<uint, BaseEntity> item2 in from x in BaseEntity.saveList
			group x by x.prefabID into x
			orderby x.Count() descending
			select x)
		{
			stringBuilder3.AppendFormat("{1:N0}\t{0}", item2.First().PrefabName, item2.Count());
			stringBuilder3.AppendLine();
		}
		WriteTextToFile(targetFolder + "UnityEngine.Entity.SV.Savelist.Counts.txt", stringBuilder3.ToString());
	}

	[ClientVar(ClientAdmin = true, Help = "Dumps a diagnostic snapshot of LOD Groups into a diagnostics subfolder")]
	[ServerVar(Help = "Dumps a diagnostic snapshot of LOD Groups into a diagnostics subfolder")]
	public static void dumpLODGroups(Arg args)
	{
		string targetFolder = GetTargetFolder();
		DumpLODGroups(targetFolder);
		args.ReplyWith("Dumped LOD Group diagnostics to: " + targetFolder);
	}

	private static void DumpLODGroups(string targetFolder)
	{
		DumpLODGroupTotals(targetFolder);
	}

	private static void DumpLODGroupTotals(string targetFolder)
	{
		LODGroup[] source = Object.FindObjectsByType<LODGroup>((FindObjectsSortMode)0);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("LODGroups");
		stringBuilder.AppendLine();
		foreach (IGrouping<string, LODGroup> item in from x in source
			group x by TransformEx.GetRecursiveName(((Component)x).transform) into x
			orderby x.Count() descending
			select x)
		{
			stringBuilder.AppendFormat("{1:N0}\t{0}", item.Key, item.Count());
			stringBuilder.AppendLine();
		}
		WriteTextToFile(targetFolder + "LODGroups.Objects.txt", stringBuilder.ToString());
	}

	[ClientVar(ClientAdmin = true, Help = "Dumps a diagnostic snapshot of network info into a diagnostics subfolder")]
	[ServerVar(Help = "Dumps a diagnostic snapshot of network info into a diagnostics subfolder")]
	public static void dumpNetwork(Arg args)
	{
		string targetFolder = GetTargetFolder();
		DumpNetwork(targetFolder);
		args.ReplyWith("Dumped network diagnostics to: " + targetFolder);
	}

	private static void DumpNetwork(string targetFolder)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (!Net.sv.IsConnected())
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Server Network Statistics");
		stringBuilder.AppendLine();
		stringBuilder.Append(Net.sv.GetDebug(null).Replace("\n", "\r\n"));
		stringBuilder.AppendLine();
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				stringBuilder.AppendLine("Name: " + current.displayName);
				stringBuilder.AppendLine("SteamID: " + current.userID.Get());
				stringBuilder.Append((current.net == null) ? "INVALID - NET IS NULL" : Net.sv.GetDebug(current.net.connection).Replace("\n", "\r\n"));
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		WriteTextToFile(targetFolder + "Network.Server.txt", stringBuilder.ToString());
	}

	[ClientVar(ClientAdmin = true, Help = "Dumps a diagnostic snapshot of objects into a diagnostics subfolder")]
	[ServerVar(Help = "Dumps a diagnostic snapshot of objects into a diagnostics subfolder")]
	public static void dumpObjects(Arg args)
	{
		string targetFolder = GetTargetFolder();
		DumpObjects(targetFolder);
		args.ReplyWith("Dumped object diagnostics to: " + targetFolder);
	}

	private static void DumpObjects(string targetFolder)
	{
		Object[] array = Object.FindObjectsByType<Object>((FindObjectsSortMode)0);
		Object[] array2 = Object.FindObjectsByType<Object>((FindObjectsInactive)1, (FindObjectsSortMode)0);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("All active UnityEngine.Object, ordered by count");
		stringBuilder.AppendLine($"Total: {array.Length}");
		stringBuilder.AppendLine();
		foreach (IGrouping<Type, Object> item in from x in array
			group x by ((object)x).GetType() into x
			orderby x.Count() descending
			select x)
		{
			stringBuilder.AppendFormat("{1:N0}\t{0}", ((object)item.First()).GetType().Name, item.Count());
			stringBuilder.AppendLine();
		}
		WriteTextToFile(targetFolder + "UnityEngine.Object.Count.txt", stringBuilder.ToString());
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.AppendLine("All active + inactive UnityEngine.Object, ordered by count");
		stringBuilder2.AppendLine($"Total: {array2.Length}");
		stringBuilder2.AppendLine();
		foreach (IGrouping<Type, Object> item2 in from x in array2
			group x by ((object)x).GetType() into x
			orderby x.Count() descending
			select x)
		{
			stringBuilder2.AppendFormat("{1:N0}\t{0}", ((object)item2.First()).GetType().Name, item2.Count());
			stringBuilder2.AppendLine();
		}
		WriteTextToFile(targetFolder + "UnityEngine.Object_All.Count.txt", stringBuilder2.ToString());
		StringBuilder stringBuilder3 = new StringBuilder();
		stringBuilder3.AppendLine("All active UnityEngine.ScriptableObject, ordered by count");
		stringBuilder3.AppendLine();
		foreach (IGrouping<Type, Object> item3 in from x in array
			where x is ScriptableObject
			group x by ((object)x).GetType() into x
			orderby x.Count() descending
			select x)
		{
			stringBuilder3.AppendFormat("{1:N0}\t{0}", ((object)item3.First()).GetType().Name, item3.Count());
			stringBuilder3.AppendLine();
		}
		WriteTextToFile(targetFolder + "UnityEngine.ScriptableObject.Count.txt", stringBuilder3.ToString());
	}

	[ServerVar(Help = "Dumps a diagnostic snapshot of physics into a diagnostics subfolder")]
	[ClientVar(ClientAdmin = true, Help = "Dumps a diagnostic snapshot of physics into a diagnostics subfolder")]
	public static void dumpPhysics(Arg args)
	{
		string targetFolder = GetTargetFolder();
		DumpPhysics(targetFolder);
		args.ReplyWith("Dumped physics diagnostics to: " + targetFolder);
	}

	private static void DumpPhysics(string targetFolder)
	{
		DumpTotals(targetFolder);
		DumpColliders(targetFolder);
		DumpRigidBodies(targetFolder);
	}

	private static void DumpTotals(string targetFolder)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Physics Information");
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Total Colliders:\t{0:N0}", Object.FindObjectsByType<Collider>((FindObjectsSortMode)0).Count());
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Active Colliders:\t{0:N0}", (from x in Object.FindObjectsByType<Collider>((FindObjectsSortMode)0)
			where x.enabled
			select x).Count());
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Total RigidBodys:\t{0:N0}", Object.FindObjectsByType<Rigidbody>((FindObjectsSortMode)0).Count());
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Mesh Colliders:\t{0:N0}", Object.FindObjectsByType<MeshCollider>((FindObjectsSortMode)0).Count());
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Box Colliders:\t{0:N0}", Object.FindObjectsByType<BoxCollider>((FindObjectsSortMode)0).Count());
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Sphere Colliders:\t{0:N0}", Object.FindObjectsByType<SphereCollider>((FindObjectsSortMode)0).Count());
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Capsule Colliders:\t{0:N0}", Object.FindObjectsByType<CapsuleCollider>((FindObjectsSortMode)0).Count());
		stringBuilder.AppendLine();
		WriteTextToFile(targetFolder + "Physics.txt", stringBuilder.ToString());
	}

	private static void DumpColliders(string targetFolder)
	{
		Collider[] source = Object.FindObjectsByType<Collider>((FindObjectsSortMode)0);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Physics Colliders");
		stringBuilder.AppendLine();
		foreach (IGrouping<string, Collider> item in from x in source
			group x by TransformEx.GetRecursiveName(((Component)x).transform) into x
			orderby x.Count() descending
			select x)
		{
			stringBuilder.AppendFormat("{1:N0}\t{0} ({2:N0} triggers) ({3:N0} enabled)", new object[4]
			{
				item.Key,
				item.Count(),
				item.Count((Collider x) => x.isTrigger),
				item.Count((Collider x) => x.enabled)
			});
			stringBuilder.AppendLine();
		}
		WriteTextToFile(targetFolder + "Physics.Colliders.Objects.txt", stringBuilder.ToString());
	}

	private static void DumpRigidBodies(string targetFolder)
	{
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody[] source = Object.FindObjectsByType<Rigidbody>((FindObjectsSortMode)0);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("RigidBody");
		stringBuilder.AppendLine();
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.AppendLine("RigidBody");
		stringBuilder2.AppendLine();
		foreach (IGrouping<string, Rigidbody> item in from x in source
			group x by TransformEx.GetRecursiveName(((Component)x).transform) into x
			orderby x.Count() descending
			select x)
		{
			stringBuilder.AppendFormat("{1:N0}\t{0} ({2:N0} awake) ({3:N0} kinematic) ({4:N0} non-discrete)", new object[5]
			{
				item.Key,
				item.Count(),
				item.Count((Rigidbody x) => !x.IsSleeping()),
				item.Count((Rigidbody x) => x.isKinematic),
				item.Count(delegate(Rigidbody x)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0007: Invalid comparison between Unknown and I4
					return (int)x.collisionDetectionMode > 0;
				})
			});
			stringBuilder.AppendLine();
			foreach (Rigidbody item2 in item)
			{
				stringBuilder2.AppendFormat("{0} -{1}{2}{3}", new object[4]
				{
					item.Key,
					item2.isKinematic ? " KIN" : "",
					item2.IsSleeping() ? " SLEEP" : "",
					item2.useGravity ? " GRAVITY" : ""
				});
				stringBuilder2.AppendLine();
				stringBuilder2.AppendFormat("Mass: {0}\tVelocity: {1}\tsleepThreshold: {2}", item2.mass, item2.linearVelocity, item2.sleepThreshold);
				stringBuilder2.AppendLine();
				stringBuilder2.AppendLine();
			}
		}
		WriteTextToFile(targetFolder + "Physics.RigidBody.Objects.txt", stringBuilder.ToString());
		WriteTextToFile(targetFolder + "Physics.RigidBody.All.txt", stringBuilder2.ToString());
	}

	[ServerVar(Help = "Dumps a diagnostic snapshot of root objects (all gameobjects across all scenes) into a /diagnostics/ subfolder. WARNING: this will stall the server and can cause it to crash.")]
	[ClientVar(ClientAdmin = true, Help = "Dumps a diagnostic snapshot of root objects (all gameobjects across all scenes) into a /diagnostics/ subfolder. WARNING: this will stall the server and can cause it to crash.")]
	public static void dumpRootObjects(Arg args)
	{
		string targetFolder = GetTargetFolder();
		Transform[] allRootObjects = GetAllRootObjects();
		SceneDumpOutput[] scenes = DumpObjectsPerScene(targetFolder, allRootObjects);
		DumpSceneSummary(targetFolder, scenes);
		DumpGameObjects(targetFolder, allRootObjects);
		args.ReplyWith("Dumped root object diagnostics to: " + targetFolder);
	}

	private static string GetOutputDirectoryForScene(string targetFolder, string sceneName)
	{
		string path = sceneName.Replace("\\", "_").Replace("/", "_").Replace(" ", "_");
		targetFolder = Path.Combine(targetFolder, "Scenes", path);
		string fullName = Directory.CreateDirectory(targetFolder).FullName;
		char directorySeparatorChar = Path.DirectorySeparatorChar;
		return fullName + directorySeparatorChar;
	}

	private static SceneDumpOutput DumpSceneGameObjects(string targetFolder, int sceneIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Scene sceneAt = SceneManager.GetSceneAt(sceneIndex);
		Transform[] array = (from x in ((Scene)(ref sceneAt)).GetRootGameObjects()
			select x.transform).ToArray();
		string sceneFolderName = $"{sceneIndex}_{((Scene)(ref sceneAt)).name}";
		SceneDumpOutput sceneDumpOutput = DumpSceneGameObjects(targetFolder, sceneFolderName, array);
		sceneDumpOutput.Roots = array;
		sceneDumpOutput.SceneName = ((Scene)(ref sceneAt)).name;
		sceneDumpOutput.SceneIndex = sceneIndex;
		return sceneDumpOutput;
	}

	private static Transform[] GetAllRootObjects()
	{
		return TransformUtil.GetAllRootObjects();
	}

	private static SceneDumpOutput[] DumpObjectsPerScene(string targetFolder, Transform[] allRootObjects)
	{
		List<SceneDumpOutput> list = new List<SceneDumpOutput>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			list.Add(DumpSceneGameObjects(targetFolder, i));
		}
		HashSet<Transform> sceneRoots = list.SelectMany((SceneDumpOutput x) => x.Roots).ToHashSet();
		Transform[] rootObjects = allRootObjects.Where((Transform x) => !sceneRoots.Contains(x)).ToArray();
		SceneDumpOutput sceneDumpOutput = DumpSceneGameObjects(targetFolder, "DontDestroyOnLoad", rootObjects);
		sceneDumpOutput.SceneName = "DontDestroyOnLoad";
		sceneDumpOutput.SceneIndex = -1;
		list.Add(sceneDumpOutput);
		return list.ToArray();
	}

	private static void DumpSceneSummary(string targetFolder, SceneDumpOutput[] scenes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Summary of all scenes");
		stringBuilder.AppendLine($"Total Transforms: {scenes.Sum((SceneDumpOutput x) => x.TransformCount)}");
		stringBuilder.AppendLine($"Total Components + Transforms: {scenes.Sum((SceneDumpOutput x) => x.TransfromPlusComponentCount)}");
		stringBuilder.AppendLine();
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumns(new string[4] { "Index", "Scene", "Transforms", "Transforms + Components" });
			foreach (SceneDumpOutput item in scenes.OrderByDescending((SceneDumpOutput x) => x.TransfromPlusComponentCount))
			{
				val.AddValue(item.SceneIndex);
				val.AddValue(item.SceneName ?? "Unknown");
				val.AddValue(item.TransformCount.ToString("N0"));
				val.AddValue(item.TransfromPlusComponentCount.ToString("N0"));
			}
			stringBuilder.AppendLine(((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		WriteTextToFile(targetFolder + "Scenes.Summary.txt", stringBuilder.ToString());
	}

	private static void DumpPrefabPreProcess(string targetFolder)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		GameObject[] array = GameManager.server.preProcessed.prefabList.Values.ToArray();
		List<PrefabContainer> list = new List<PrefabContainer>();
		GameObject[] array2 = array;
		foreach (GameObject val in array2)
		{
			Component[] componentsInChildren = val.GetComponentsInChildren<Component>(true);
			int transformCount = Enumerable.Count(componentsInChildren, (Component x) => x is Transform);
			PrefabContainer obj = new PrefabContainer
			{
				Prefab = val,
				TransformPlusComponentCount = componentsInChildren.Length,
				TransformCount = transformCount
			};
			Scene scene = val.scene;
			object sceneName;
			if (!((Scene)(ref scene)).IsValid())
			{
				sceneName = "INVALID";
			}
			else
			{
				scene = val.scene;
				sceneName = ((Scene)(ref scene)).name;
			}
			obj.SceneName = (string)sceneName;
			list.Add(obj);
		}
		int num = list.Sum((PrefabContainer x) => x.TransformCount);
		int num2 = list.Sum((PrefabContainer x) => x.TransformPlusComponentCount);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Prefabs: {list.Count}");
		stringBuilder.AppendLine($"Total Transforms: {num}");
		stringBuilder.AppendLine($"Total Components + Transforms: {num2}");
		stringBuilder.AppendLine();
		TextTable val2 = Pool.Get<TextTable>();
		try
		{
			val2.AddColumns(new string[4] { "Prefab", "Transforms + Components", "Transforms", "Scene" });
			foreach (PrefabContainer item in list.OrderByDescending((PrefabContainer x) => x.TransformPlusComponentCount))
			{
				val2.AddValue(((Object)item.Prefab).name ?? "Unknown");
				val2.AddValue(item.TransformPlusComponentCount.ToString("N0"));
				val2.AddValue(item.TransformCount.ToString("N0"));
				val2.AddValue(item.SceneName);
			}
			stringBuilder.AppendLine(((object)val2).ToString());
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		WriteTextToFile(targetFolder + "PrefabPreProcess.txt", stringBuilder.ToString());
	}

	private static SceneDumpOutput DumpSceneGameObjects(string targetFolder, string sceneFolderName, Transform[] rootObjects)
	{
		return DumpGameObjects(GetOutputDirectoryForScene(targetFolder, sceneFolderName), rootObjects);
	}

	private static SceneDumpOutput DumpGameObjects(string targetFolder, Transform[] allRootObjects)
	{
		SceneDumpOutput sceneDumpOutput = new SceneDumpOutput();
		Transform[] source = allRootObjects.Where((Transform x) => ((Component)x).gameObject.activeSelf).ToArray();
		Transform[] source2 = allRootObjects.Where((Transform x) => !((Component)x).gameObject.activeSelf).ToArray();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("All active + inactive game objects");
		stringBuilder.AppendLine();
		DumpHierarchy(stringBuilder, allRootObjects);
		WriteTextToFile(targetFolder + "GameObject.Hierarchy.txt", stringBuilder.ToString());
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.AppendLine("All active + inactive game objects including components");
		stringBuilder2.AppendLine();
		DumpHierarchy(stringBuilder2, allRootObjects, includeComponents: true);
		WriteTextToFile(targetFolder + "GameObject.Hierarchy.Components.txt", stringBuilder2.ToString());
		StringBuilder stringBuilder3 = new StringBuilder();
		stringBuilder3.AppendLine("All active + inactive game objects including components");
		stringBuilder3.AppendLine();
		DumpHierarchy(stringBuilder3, allRootObjects, includeComponents: false, showTotals: true);
		WriteTextToFile(targetFolder + "GameObject.Hierarchy.Totals.txt", stringBuilder3.ToString());
		StringBuilder stringBuilder4 = new StringBuilder();
		stringBuilder4.AppendLine("Active + inactive root gameobjects, grouped by name, ordered by the total count of root objects");
		stringBuilder4.AppendLine();
		foreach (IGrouping<string, Transform> item in from x in source
			group x by ((Object)x).name into x
			orderby x.Count() descending
			select x)
		{
			Transform val = item.First();
			stringBuilder4.AppendFormat("{1:N0}\t{0}", ((Object)val).name, item.Count());
			stringBuilder4.AppendLine();
		}
		WriteTextToFile(targetFolder + "GameObject.Roots.txt", stringBuilder4.ToString());
		stringBuilder4 = new StringBuilder();
		stringBuilder4.AppendLine("Active root gameobjects, grouped by name, ordered by the total count of root objects");
		stringBuilder4.AppendLine();
		foreach (IGrouping<string, Transform> item2 in from x in source
			group x by ((Object)x).name into x
			orderby x.Count() descending
			select x)
		{
			Transform val2 = item2.First();
			stringBuilder4.AppendFormat("{1:N0}\t{0}", ((Object)val2).name, item2.Count());
			stringBuilder4.AppendLine();
		}
		WriteTextToFile(targetFolder + "GameObject.Roots.Active.txt", stringBuilder4.ToString());
		stringBuilder4 = new StringBuilder();
		stringBuilder4.AppendLine("Inactive root gameobjects, grouped by name, ordered by the total count of root objects");
		stringBuilder4.AppendLine();
		foreach (IGrouping<string, Transform> item3 in from x in source2
			group x by ((Object)x).name into x
			orderby x.Count() descending
			select x)
		{
			Transform val3 = item3.First();
			stringBuilder4.AppendFormat("{1:N0}\t{0}", ((Object)val3).name, item3.Count());
			stringBuilder4.AppendLine();
		}
		WriteTextToFile(targetFolder + "GameObject.Roots.Inactive.txt", stringBuilder4.ToString());
		StringBuilder stringBuilder5 = new StringBuilder();
		stringBuilder5.AppendLine("Active + inactive hierachies, ordered by the size of the root + all children transforms");
		stringBuilder5.AppendLine();
		IOrderedEnumerable<KeyValuePair<Transform, int>> orderedEnumerable = from x in allRootObjects
			group x by ((Object)x).name into x
			select new KeyValuePair<Transform, int>(x.First(), x.Sum((Transform y) => TransformEx.GetAllChildren(y).Count)) into x
			orderby x.Value descending
			select x;
		sceneDumpOutput.TransformCount = orderedEnumerable.Sum((KeyValuePair<Transform, int> x) => x.Value);
		foreach (KeyValuePair<Transform, int> item4 in orderedEnumerable)
		{
			stringBuilder5.AppendFormat("{1:N0}\t{0}", ((Object)item4.Key).name, item4.Value);
			stringBuilder5.AppendLine();
		}
		WriteTextToFile(targetFolder + "GameObject.Children.txt", stringBuilder5.ToString());
		stringBuilder5 = new StringBuilder();
		stringBuilder5.AppendLine("Active hierachies, ordered by the size of the root + all children transforms");
		stringBuilder5.AppendLine();
		foreach (KeyValuePair<Transform, int> item5 in from x in source
			group x by ((Object)x).name into x
			select new KeyValuePair<Transform, int>(x.First(), x.Sum((Transform y) => TransformEx.GetAllChildren(y).Count)) into x
			orderby x.Value descending
			select x)
		{
			stringBuilder5.AppendFormat("{1:N0}\t{0}", ((Object)item5.Key).name, item5.Value);
			stringBuilder5.AppendLine();
		}
		WriteTextToFile(targetFolder + "GameObject.Children.Active.txt", stringBuilder5.ToString());
		stringBuilder5 = new StringBuilder();
		stringBuilder5.AppendLine("Inactive hierachies, ordered by the size of the root + all children transforms");
		stringBuilder5.AppendLine();
		foreach (KeyValuePair<Transform, int> item6 in from x in source2
			group x by ((Object)x).name into x
			select new KeyValuePair<Transform, int>(x.First(), x.Sum((Transform y) => TransformEx.GetAllChildren(y).Count)) into x
			orderby x.Value descending
			select x)
		{
			stringBuilder5.AppendFormat("{1:N0}\t{0}", ((Object)item6.Key).name, item6.Value);
			stringBuilder5.AppendLine();
		}
		WriteTextToFile(targetFolder + "GameObject.Children.Inactive.txt", stringBuilder5.ToString());
		Component[] array = (from x in allRootObjects.SelectMany((Transform x) => ((Component)x).GetComponentsInChildren<Component>(true))
			where (Object)(object)x != (Object)null
			select x).ToArray();
		Dictionary<Type, int> source3 = (from x in array
			group x by ((object)x).GetType()).ToDictionary((IGrouping<Type, Component> x) => x.Key, (IGrouping<Type, Component> x) => x.Count());
		sceneDumpOutput.TransfromPlusComponentCount = array.Length;
		StringBuilder stringBuilder6 = new StringBuilder();
		stringBuilder6.AppendLine("All transform roots and all it's children + components active + inactive, ordered by count");
		stringBuilder6.AppendLine($"Total: {array.Length}");
		foreach (KeyValuePair<Type, int> item7 in source3.OrderByDescending((KeyValuePair<Type, int> x) => x.Value))
		{
			stringBuilder6.AppendFormat("{1:N0}\t{0}", item7.Key.Name, item7.Value);
			stringBuilder6.AppendLine();
		}
		WriteTextToFile(targetFolder + "UnityEngine.Object.Count.txt", stringBuilder6.ToString());
		return sceneDumpOutput;
	}

	private static void DumpHierarchy(StringBuilder str, Transform[] roots, bool includeComponents = false, bool showTotals = false)
	{
		foreach (GameObjectRecursiveData item in from x in roots.Select((Transform x) => CollectGameObjectRecursive(x)).ToArray()
			orderby x.TotalComponentCount descending
			select x)
		{
			PrintGameObjectRecursive(str, item, includeComponents, showTotals);
			str.AppendLine();
		}
	}

	private static GameObjectRecursiveData CollectGameObjectRecursive(Transform tx, int depth = 0)
	{
		GameObjectRecursiveData gameObjectRecursiveData = new GameObjectRecursiveData
		{
			GameObject = ((Component)tx).gameObject,
			Depth = depth,
			Components = ((Component)tx).GetComponents<Component>()
		};
		gameObjectRecursiveData.TotalTransformCount = 1;
		gameObjectRecursiveData.TotalComponentCount = gameObjectRecursiveData.Components.Length + 1;
		for (int i = 0; i < tx.childCount; i++)
		{
			GameObjectRecursiveData gameObjectRecursiveData2 = CollectGameObjectRecursive(tx.GetChild(i), depth + 1);
			gameObjectRecursiveData.Children.Add(gameObjectRecursiveData2);
			gameObjectRecursiveData.TotalTransformCount += gameObjectRecursiveData2.TotalTransformCount;
			gameObjectRecursiveData.TotalComponentCount += gameObjectRecursiveData2.TotalComponentCount;
		}
		return gameObjectRecursiveData;
	}

	private static void PrintGameObjectRecursive(StringBuilder str, GameObjectRecursiveData data, bool includeComponents, bool showTotals)
	{
		int num = data.Depth * 4;
		str.Append(' ', num);
		str.Append(data.GameObject.activeSelf ? "+ " : "- ");
		str.Append(((Object)data.GameObject).name);
		str.Append(" [").Append(data.Components.Length - 1).Append(']');
		if (showTotals)
		{
			str.Append(" (").Append(data.TotalComponentCount.ToString("N0")).Append(")");
		}
		str.AppendLine();
		if (includeComponents)
		{
			Component[] components = data.Components;
			foreach (Component val in components)
			{
				if (!(val is Transform))
				{
					str.Append(' ', num + 3);
					bool? flag = ComponentEx.IsEnabled(val);
					if (!flag.HasValue)
					{
						str.Append("[~] ");
					}
					else if (flag == true)
					{
						str.Append("[✓] ");
					}
					else
					{
						str.Append("[ ] ");
					}
					str.Append(((Object)(object)val == (Object)null) ? "NULL" : ((object)val).GetType().ToString());
					str.AppendLine();
				}
			}
		}
		foreach (GameObjectRecursiveData item in data.Children.OrderByDescending((GameObjectRecursiveData x) => x.TotalComponentCount))
		{
			PrintGameObjectRecursive(str, item, includeComponents, showTotals);
		}
	}

	[ClientVar(ClientAdmin = true, Help = "Dumps a diagnostic snapshot of warmup info into a diagnostics subfolder")]
	[ServerVar(Help = "Dumps a diagnostic snapshot of warmup info into a diagnostics subfolder")]
	public static void dumpWarmup(Arg args)
	{
		string targetFolder = GetTargetFolder();
		DumpWarmup(targetFolder);
		args.ReplyWith("Dumped warmup diagnostics to: " + targetFolder);
	}

	private static void DumpWarmup(string targetFolder)
	{
		DumpWarmupTimings(targetFolder);
		DumpWorldSpawnTimings(targetFolder);
	}

	private static void DumpWarmupTimings(string targetFolder)
	{
		if (!FileSystem_Warmup.GetWarmupTimes().Any())
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("index,prefab,time");
		int num = 0;
		foreach (var warmupTime in FileSystem_Warmup.GetWarmupTimes())
		{
			object arg = num;
			var (arg2, timeSpan) = warmupTime;
			stringBuilder.AppendLine($"{arg},{arg2},{timeSpan.Ticks * EventRecord.NSPerTick}");
			num++;
		}
		WriteTextToFile(targetFolder + "Asset.Warmup.csv", stringBuilder.ToString());
	}

	private static void DumpWorldSpawnTimings(string targetFolder)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (!World.GetSpawnTimings().Any())
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("index,prefab,time,category,position,rotation");
		int num = 0;
		foreach (World.SpawnTiming spawnTiming in World.GetSpawnTimings())
		{
			object[] obj = new object[6]
			{
				num,
				spawnTiming.prefab.Name,
				null,
				null,
				null,
				null
			};
			TimeSpan time = spawnTiming.time;
			obj[2] = time.Ticks * EventRecord.NSPerTick;
			obj[3] = spawnTiming.category;
			obj[4] = spawnTiming.position;
			obj[5] = spawnTiming.rotation;
			stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}", obj));
			num++;
		}
		WriteTextToFile(targetFolder + "World.Spawn.csv", stringBuilder.ToString());
	}

	[ClientVar(ClientAdmin = true, Help = "Dumps a diagnostic snapshot into a folder in /diagnostics/. WARNING: this will stall the server and can cause it to crash.")]
	[ServerVar(Help = "Dumps a diagnostic snapshot into a folder in /diagnostics/. WARNING: this will stall the server and can cause it to crash.")]
	public static void dump(Arg args)
	{
		string targetFolder = GetTargetFolder();
		DumpLODGroups(targetFolder);
		DumpSystemInformation(targetFolder);
		Transform[] allRootObjects = GetAllRootObjects();
		SceneDumpOutput[] scenes = DumpObjectsPerScene(targetFolder, allRootObjects);
		DumpSceneSummary(targetFolder, scenes);
		DumpGameObjects(targetFolder, allRootObjects);
		DumpPrefabPreProcess(targetFolder);
		DumpObjects(targetFolder);
		DumpEntities(targetFolder);
		DumpNetwork(targetFolder);
		DumpPhysics(targetFolder);
		DumpAnimators(targetFolder);
		DumpWarmup(targetFolder);
		args.ReplyWith("Dumped diagnostics to: " + targetFolder);
	}

	[ClientVar(ClientAdmin = true, Help = "Dumps a diagnostic snapshot of system information into a diagnostics subfolder")]
	[ServerVar(Help = "Dumps a diagnostic snapshot of system information into a diagnostics subfolder")]
	public static void dumpSystemInformation(Arg args)
	{
		string targetFolder = GetTargetFolder();
		DumpSystemInformation(targetFolder);
		args.ReplyWith("Dumped system information to: " + targetFolder + "System.Info.txt");
	}

	[ServerVar(Help = "Dump list of prefabs pre-processed into a diagnostics subfolder")]
	[ClientVar(ClientAdmin = true, Help = "Dump list of prefabs pre-processed into a diagnostics subfolder")]
	public static void dumppreprocess(Arg args)
	{
		string targetFolder = GetTargetFolder();
		DumpPrefabPreProcess(targetFolder);
		args.ReplyWith("Dumped PrefabPreProcess to: " + targetFolder + "PrefabPreProcess.txt");
	}

	private static void DumpSystemInformation(string targetFolder)
	{
		WriteTextToFile(targetFolder + "System.Info.txt", SystemInfoGeneralText.currentInfo);
	}

	private static void WriteTextToFile(string file, string text)
	{
		File.WriteAllText(file, text);
	}

	private static string GetTargetFolder()
	{
		if (Directory.Exists("diagnostics"))
		{
			Directory.CreateDirectory("diagnostics");
		}
		int num = 1;
		while (Directory.Exists("diagnostics/" + num))
		{
			num++;
		}
		Directory.CreateDirectory("diagnostics/" + num);
		return "diagnostics/" + num + "/";
	}
}
