using System;
using System.Collections;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust.Ai;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.AI;

public static class NavMeshTools
{
	public static void Log(string message)
	{
		Debug.Log((object)("[UnityNavmesh] " + message));
	}

	public static void LogWarning(string message)
	{
		Debug.LogWarning((object)("[UnityNavmesh] " + message));
	}

	public static IEnumerator CollectSourcesAsync(Bounds bounds, int mask, NavMeshCollectGeometry geometry, int area, bool useBakedTerrainMesh, int cellSize, List<NavMeshBuildSource> sources, Action<List<NavMeshBuildSource>> append, Action callback, Transform customNavMeshDataRoot, HashSet<Transform> ignoreRoots = null)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		while (!AI.move && !AiManager.nav_wait)
		{
			yield return CoroutineEx.waitForSeconds(1f);
		}
		if ((Object)(object)customNavMeshDataRoot != (Object)null)
		{
			((Component)customNavMeshDataRoot).gameObject.SetActive(true);
			yield return (object)new WaitForEndOfFrame();
		}
		float time = Time.realtimeSinceStartup;
		Log("Starting Navmesh Source Collecting");
		mask = ((!useBakedTerrainMesh) ? (mask | 0x800000) : (mask & -8388609));
		List<NavMeshBuildMarkup> list = new List<NavMeshBuildMarkup>();
		NavMeshBuildMarkup val;
		if (ignoreRoots != null)
		{
			foreach (Transform ignoreRoot in ignoreRoots)
			{
				val = default(NavMeshBuildMarkup);
				((NavMeshBuildMarkup)(ref val)).root = ignoreRoot;
				((NavMeshBuildMarkup)(ref val)).ignoreFromBuild = true;
				((NavMeshBuildMarkup)(ref val)).overrideIgnore = true;
				NavMeshBuildMarkup item = val;
				list.Add(item);
			}
		}
		int areaFromName = NavMesh.GetAreaFromName("Not Walkable");
		PooledList<RustNavmeshModifierVolume> modifierVolumes = Pool.Get<PooledList<RustNavmeshModifierVolume>>();
		try
		{
			RustNavmeshModifierVolume.AllModifierVolumes.GetInBounds(bounds, (List<RustNavmeshModifierVolume>)(object)modifierVolumes);
			foreach (RustNavmeshModifierVolume item3 in (List<RustNavmeshModifierVolume>)(object)modifierVolumes)
			{
				val = default(NavMeshBuildMarkup);
				((NavMeshBuildMarkup)(ref val)).root = ((Component)item3).transform;
				((NavMeshBuildMarkup)(ref val)).overrideArea = true;
				((NavMeshBuildMarkup)(ref val)).area = areaFromName;
				NavMeshBuildMarkup item2 = val;
				list.Add(item2);
			}
			NavMeshBuilder.CollectSources(bounds, mask, geometry, area, list, sources);
			if (useBakedTerrainMesh && (Object)(object)TerrainMeta.HeightMap != (Object)null)
			{
				for (float x = 0f - ((Bounds)(ref bounds)).extents.x; x < ((Bounds)(ref bounds)).extents.x - (float)(cellSize / 2); x += (float)cellSize)
				{
					for (float z = 0f - ((Bounds)(ref bounds)).extents.z; z < ((Bounds)(ref bounds)).extents.z - (float)(cellSize / 2); z += (float)cellSize)
					{
						AsyncTerrainNavMeshBake terrainSource = new AsyncTerrainNavMeshBake(new Vector3(x, 0f, z), cellSize, cellSize, normal: false, alpha: true);
						yield return terrainSource;
						sources.Add(terrainSource.CreateNavMeshBuildSource(area));
					}
				}
			}
			append?.Invoke(sources);
			Log($"Navmesh Source Collecting took {Time.realtimeSinceStartup - time:0.00} seconds");
			if ((Object)(object)customNavMeshDataRoot != (Object)null)
			{
				((Component)customNavMeshDataRoot).gameObject.SetActive(false);
			}
			callback?.Invoke();
		}
		finally
		{
			((IDisposable)modifierVolumes)?.Dispose();
		}
	}

	public static IEnumerator CollectSourcesAsync(IEnumerable<GameObject> roots, int mask, NavMeshCollectGeometry geometry, int area, List<NavMeshBuildSource> sources, Action<List<NavMeshBuildSource>> append, Action callback)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		while (!AI.move && !AiManager.nav_wait)
		{
			yield return CoroutineEx.waitForSeconds(1f);
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		Log("Starting Navmesh Source Collecting");
		List<NavMeshBuildMarkup> list = new List<NavMeshBuildMarkup>();
		List<NavMeshBuildSource> list2 = new List<NavMeshBuildSource>();
		foreach (GameObject root in roots)
		{
			if ((Object)(object)root == (Object)null)
			{
				continue;
			}
			NavMeshBuilder.CollectSources(root.transform, mask, geometry, area, list, list2);
			foreach (NavMeshBuildSource item in list2)
			{
				sources.Add(item);
			}
		}
		append?.Invoke(sources);
		Log($"Navmesh Source Collecting took {Time.realtimeSinceStartup - realtimeSinceStartup:0.00} seconds");
		callback?.Invoke();
	}
}
