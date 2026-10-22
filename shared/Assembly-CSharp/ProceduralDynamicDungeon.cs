using System;
using System.Collections;
using System.Collections.Generic;
using Facepunch;
using Network;
using Network.Visibility;
using ProtoBuf;
using Rust;
using UnityEngine;

public class ProceduralDynamicDungeon : BaseEntity
{
	public int gridResolution = 6;

	public float gridSpacing = 12f;

	public bool[] grid;

	public List<GameObjectRef> cellPrefabReferences = new List<GameObjectRef>();

	public List<ProceduralDungeonCell> spawnedCells = new List<ProceduralDungeonCell>();

	public EnvironmentVolume envVolume;

	public MonumentNavMesh monumentNavMesh;

	public GameObjectRef exitPortalPrefab;

	private EntityRef<BasePortal> exitPortal;

	public TriggerRadiation exitRadiation;

	public uint seed;

	public uint baseseed;

	public Vector3 mapOffset = Vector3.zero;

	public static readonly List<ProceduralDynamicDungeon> dungeons = new List<ProceduralDynamicDungeon>();

	private const float NetworkGroupFloorHeight = 25f;

	public ProceduralDungeonCell entranceHack;

	public override void InitShared()
	{
		base.InitShared();
		dungeons.Add(this);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		foreach (ProceduralDungeonCell spawnedCell in spawnedCells)
		{
			EntityFlag_Toggle[] componentsInChildren = ((Component)spawnedCell).GetComponentsInChildren<EntityFlag_Toggle>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].DoUpdate(this);
			}
		}
	}

	public BaseEntity GetExitPortal(bool serverSide)
	{
		return exitPortal.Get(serverSide);
	}

	public override void DestroyShared()
	{
		dungeons.Remove(this);
		RetireAllCells();
		base.DestroyShared();
	}

	public static bool TryFindFreeNetworkGroup(out Vector3 spawnPoint)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		Bounds worldBounds = SingletonComponent<ValidBounds>.Instance.worldBounds;
		Bounds area = default;
		area.SetMinMax(Vector3.Max(TerrainMeta.Position, worldBounds.min), Vector3.Min(TerrainMeta.Max, worldBounds.max));
		HashSet<Group> hashSet = Pool.Get<HashSet<Group>>();
		foreach (ProceduralDynamicDungeon dungeon in dungeons)
		{
			if ((Object)(object)dungeon != (Object)null && dungeon.isServer && dungeon.net?.group != null)
			{
				hashSet.Add(dungeon.net.group);
			}
		}
		ListHashSet<Group> val = Pool.Get<ListHashSet<Group>>();
		spawnPoint = Vector3.zero;
		for (int i = 10; i < 15; i++)
		{
			if (!(spawnPoint == Vector3.zero))
			{
				break;
			}
			val.Clear();
			Net.sv.visibility.AddGroups(i, area, val, create: true);
			Enumerator<Group> enumerator2 = val.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					Group current2 = enumerator2.Current;
					if (!hashSet.Contains(current2))
					{
						spawnPoint = current2.bounds.center;
						spawnPoint.y = current2.bounds.min.y + 25f;
						break;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
		}
		Pool.FreeUnmanaged<Group>(ref val);
		Pool.FreeUnmanaged<Group>(ref hashSet);
		return spawnPoint != Vector3.zero;
	}

	public void GetEntitiesInside<T>(List<T> results) where T : BaseNetworkable
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (net?.group?.networkables == null)
		{
			return;
		}
		Enumerator<Networkable> enumerator = net.group.networkables.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.handler is T item)
				{
					results.Add(item);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public bool ContainsAnyPlayers(bool includeSleepers)
	{
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		GetEntitiesInside(list);
		bool result = false;
		foreach (BasePlayer item in list)
		{
			if (includeSleepers || item.IsConnected)
			{
				result = true;
				break;
			}
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
		return result;
	}

	public void KillPlayers()
	{
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		GetEntitiesInside(list);
		foreach (BasePlayer item in list)
		{
			item.Hurt(10000f, DamageType.Suicide, null, useProtection: false);
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
	}

	internal override void DoServerDestroy()
	{
		KillPlayers();
		if (exitPortal.IsValid(serverside: true))
		{
			exitPortal.Get(serverside: true).Kill();
		}
		base.DoServerDestroy();
	}

	public override void ServerInit()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (!Application.isLoadingSave)
		{
			baseseed = (seed = (uint)Random.Range(0, 12345567));
		}
		base.ServerInit();
		bool flag = false;
		if (!Application.isLoadingSave && !flag)
		{
			DoGeneration();
			BasePortal component = ((Component)GameManager.server.CreateEntity(exitPortalPrefab.resourcePath, entranceHack.exitPointHack.position, entranceHack.exitPointHack.rotation)).GetComponent<BasePortal>();
			component.Spawn();
			exitPortal.Set(component);
		}
	}

	public void DoGeneration()
	{
		GenerateGrid();
		CreateAIZ();
		if (isServer)
		{
			((MonoBehaviour)this).StartCoroutine(UpdateNavMesh());
		}
		Invoke(InitSpawnGroups, 1f);
	}

	private void CreateAIZ()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		AIInformationZone aIInformationZone = ((Component)this).gameObject.AddComponent<AIInformationZone>();
		aIInformationZone.UseCalculatedCoverDistances = false;
		Bounds val = Net.sv.visibility.GetGroup(((Component)this).transform.position, networkRange).bounds;
		aIInformationZone.bounds = new Bounds(val.center - ((Component)this).transform.position, val.size);
		aIInformationZone.Init();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		DoGeneration();
	}

	public IEnumerator UpdateNavMesh()
	{
		yield return ((MonoBehaviour)this).StartCoroutine(monumentNavMesh.UpdateNavMeshAndWait());
	}

	public override void Save(SaveInfo info)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (info.msg.proceduralDungeon == null)
		{
			info.msg.proceduralDungeon = Pool.Get<ProceduralDungeon>();
		}
		info.msg.proceduralDungeon.seed = baseseed;
		info.msg.proceduralDungeon.exitPortalID = exitPortal.uid;
		info.msg.proceduralDungeon.mapOffset = mapOffset;
	}

	public BasePortal GetExitPortal()
	{
		return exitPortal.Get(serverside: true);
	}

	public void InitSpawnGroups()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		foreach (ProceduralDungeonCell spawnedCell in spawnedCells)
		{
			if (!((Object)(object)entranceHack != (Object)null) || !(Vector3.Distance(((Component)entranceHack).transform.position, ((Component)spawnedCell).transform.position) < 20f))
			{
				SpawnGroup[] spawnGroups = spawnedCell.spawnGroups;
				for (int i = 0; i < spawnGroups.Length; i++)
				{
					spawnGroups[i].Spawn();
				}
			}
		}
	}

	public void CleanupSpawnGroups()
	{
		foreach (ProceduralDungeonCell spawnedCell in spawnedCells)
		{
			SpawnGroup[] spawnGroups = spawnedCell.spawnGroups;
			for (int i = 0; i < spawnGroups.Length; i++)
			{
				spawnGroups[i].Clear();
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.proceduralDungeon != null)
		{
			baseseed = (seed = info.msg.proceduralDungeon.seed);
			exitPortal.uid = info.msg.proceduralDungeon.exitPortalID;
			mapOffset = info.msg.proceduralDungeon.mapOffset;
		}
	}

	[ContextMenu("Test Grid")]
	[ExecuteInEditMode]
	public void GenerateGrid()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position - new Vector3((float)gridResolution * gridSpacing * 0.5f, 0f, (float)gridResolution * gridSpacing * 0.5f);
		RetireAllCells();
		grid = new bool[gridResolution * gridResolution];
		for (int i = 0; i < grid.Length; i++)
		{
			grid[i] = SeedRandom.Range(ref seed, 0, 2) == 0;
		}
		SetEntrance(3, 0);
		for (int j = 0; j < gridResolution; j++)
		{
			for (int k = 0; k < gridResolution; k++)
			{
				if (GetGridState(j, k) && !HasPathToEntrance(j, k))
				{
					SetGridState(j, k, state: false);
				}
			}
		}
		for (int l = 0; l < gridResolution; l++)
		{
			for (int m = 0; m < gridResolution; m++)
			{
				if (!GetGridState(l, m))
				{
					continue;
				}
				bool gridState = GetGridState(l, m + 1);
				bool gridState2 = GetGridState(l, m - 1);
				bool gridState3 = GetGridState(l - 1, m);
				bool gridState4 = GetGridState(l + 1, m);
				bool flag = IsEntrance(l, m);
				GameObjectRef gameObjectRef = null;
				ProceduralDungeonCell proceduralDungeonCell = null;
				if ((Object)(object)proceduralDungeonCell == (Object)null)
				{
					foreach (GameObjectRef cellPrefabReference in cellPrefabReferences)
					{
						ProceduralDungeonCell component = cellPrefabReference.Get().GetComponent<ProceduralDungeonCell>();
						if (component.north == gridState && component.south == gridState2 && component.west == gridState3 && component.east == gridState4 && component.entrance == flag)
						{
							proceduralDungeonCell = component;
							gameObjectRef = cellPrefabReference;
							break;
						}
					}
				}
				if ((Object)(object)proceduralDungeonCell != (Object)null)
				{
					ProceduralDungeonCell proceduralDungeonCell2 = CellInstantiate(gameObjectRef.resourcePath);
					((Component)proceduralDungeonCell2).transform.position = val + new Vector3((float)l * gridSpacing, 0f, (float)m * gridSpacing);
					spawnedCells.Add(proceduralDungeonCell2);
					((Component)proceduralDungeonCell2).transform.SetParent(((Component)this).transform);
					if (proceduralDungeonCell2.entrance && (Object)(object)entranceHack == (Object)null)
					{
						entranceHack = proceduralDungeonCell2;
					}
				}
			}
		}
	}

	public ProceduralDungeonCell CellInstantiate(string path)
	{
		if (isServer)
		{
			return GameManager.server.CreatePrefab(path).GetComponent<ProceduralDungeonCell>();
		}
		return null;
	}

	public void RetireCell(GameObject cell)
	{
		if (!((Object)(object)cell == (Object)null) && isServer)
		{
			GameManager.server.Retire(cell);
		}
	}

	public void RetireAllCells()
	{
		if (isServer)
		{
			CleanupSpawnGroups();
		}
		for (int num = spawnedCells.Count - 1; num >= 0; num--)
		{
			ProceduralDungeonCell proceduralDungeonCell = spawnedCells[num];
			if (Object.op_Implicit((Object)(object)proceduralDungeonCell))
			{
				RetireCell(((Component)proceduralDungeonCell).gameObject);
			}
		}
		spawnedCells.Clear();
	}

	public bool CanSeeEntrance(int x, int y, ref List<int> checkedCells)
	{
		int gridIndex = GetGridIndex(x, y);
		if (checkedCells.Contains(gridIndex))
		{
			return false;
		}
		checkedCells.Add(gridIndex);
		if (!GetGridState(x, y))
		{
			return false;
		}
		if (IsEntrance(x, y))
		{
			return true;
		}
		bool flag = CanSeeEntrance(x, y + 1, ref checkedCells);
		bool flag2 = CanSeeEntrance(x, y - 1, ref checkedCells);
		bool flag3 = CanSeeEntrance(x - 1, y, ref checkedCells);
		bool flag4 = CanSeeEntrance(x + 1, y, ref checkedCells);
		return flag | flag4 | flag3 | flag2;
	}

	public bool HasPathToEntrance(int x, int y)
	{
		List<int> checkedCells = new List<int>();
		bool result = CanSeeEntrance(x, y, ref checkedCells);
		checkedCells.Clear();
		return result;
	}

	public bool CanFindEntrance(int x, int y)
	{
		new List<int>();
		GetGridState(x, y + 1);
		GetGridState(x, y - 1);
		GetGridState(x - 1, y);
		GetGridState(x + 1, y);
		return true;
	}

	public bool IsEntrance(int x, int y)
	{
		return GetGridIndex(x, y) == GetEntranceIndex();
	}

	public int GetEntranceIndex()
	{
		return GetGridIndex(3, 0);
	}

	public void SetEntrance(int x, int y)
	{
		grid[GetGridIndex(x, y)] = true;
		grid[GetGridIndex(x, y + 1)] = true;
		grid[GetGridIndex(x - 1, y)] = false;
		grid[GetGridIndex(x + 1, y)] = false;
		grid[GetGridIndex(x, y + 2)] = true;
		grid[GetGridIndex(x + 1, y + 2)] = SeedRandom.Range(ref seed, 0, 1) == 1;
		grid[GetGridIndex(x + 2, y + 2)] = SeedRandom.Range(ref seed, 0, 1) == 1;
		grid[GetGridIndex(x, y + 3)] = true;
		grid[GetGridIndex(x, y + 4)] = true;
		grid[GetGridIndex(x - 1, y + 4)] = SeedRandom.Range(ref seed, 0, 1) == 1;
		grid[GetGridIndex(x - 2, y + 4)] = SeedRandom.Range(ref seed, 0, 1) == 1;
	}

	public void SetGridState(int x, int y, bool state)
	{
		int gridIndex = GetGridIndex(x, y);
		grid[gridIndex] = state;
	}

	public bool GetGridState(int x, int y)
	{
		if (GetGridIndex(x, y) >= grid.Length)
		{
			return false;
		}
		if (x < 0 || x >= gridResolution)
		{
			return false;
		}
		if (y < 0 || y >= gridResolution)
		{
			return false;
		}
		return grid[GetGridIndex(x, y)];
	}

	public int GetGridX(int index)
	{
		return index % gridResolution;
	}

	public int GetGridY(int index)
	{
		return Mathf.FloorToInt((float)index / (float)gridResolution);
	}

	public int GetGridIndex(int x, int y)
	{
		return y * gridResolution + x;
	}

	public ProceduralDynamicDungeon()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
	}
}
