using System;
using System.Collections.Generic;
using ConVar;
using Development.Attributes;
using Facepunch;
using Spatial;
using UnityEngine;
using UnityEngine.AI;

[ResetStaticFields]
public abstract class BuildingManager
{
	public struct PrivilegeCandidate(BuildingBlock block, OBB bounds) : IEquatable<PrivilegeCandidate>, IComparable<PrivilegeCandidate>
	{
		public BuildingBlock Block = block;

		public ulong NetId = block.net?.ID.Value ?? 0;

		public Vector3 BoundsCentre = bounds.position;

		public float BoundsExtent = bounds.extents.magnitude;

		public bool Equals(PrivilegeCandidate other)
		{
			return Block == other.Block;
		}

		public override bool Equals(object obj)
		{
			if (obj is PrivilegeCandidate other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((object)Block).GetHashCode();
		}

		public int CompareTo(PrivilegeCandidate other)
		{
			return NetId.CompareTo(other.NetId);
		}
	}

	public class Building
	{
		private readonly List<ItemAmount> buildingUpkeepCost = new List<ItemAmount>();

		private readonly List<ItemAmount> doorUpkeepCost = new List<ItemAmount>();

		private readonly List<ItemAmount> highWallUpkeepCost = new List<ItemAmount>();

		public uint ID;

		public ListHashSet<BuildingPrivlidge> buildingPrivileges = new ListHashSet<BuildingPrivlidge>();

		public ListHashSet<BuildingBlock> buildingBlocks = new ListHashSet<BuildingBlock>();

		public ListHashSet<DecayEntity> decayEntities = new ListHashSet<DecayEntity>();

		public ListHashSet<Door> doors = new ListHashSet<Door>();

		public NavMeshObstacle buildingNavMeshObstacle;

		public ListHashSet<NavMeshObstacle> navmeshCarvers;

		public bool isNavMeshCarvingDirty;

		public bool isNavMeshCarveOptimized;

		public List<ItemAmount> GetCachedUpkeepCost(Upkeep.Category category)
		{
			return category switch
			{
				Upkeep.Category.HighWall => highWallUpkeepCost, 
				Upkeep.Category.Door => doorUpkeepCost, 
				_ => buildingUpkeepCost, 
			};
		}

		public void AddUpkeepCost(DecayEntity ent)
		{
			if (decayEntities.Contains(ent))
			{
				ent.AddUpkeepCostTo(GetCachedUpkeepCost(ent.GetUpkeepCategory()));
			}
		}

		public void RemoveUpkeepCost(DecayEntity ent)
		{
			ent.RemoveUpkeepCostFrom(GetCachedUpkeepCost(ent.GetUpkeepCategory()));
		}

		public void RefreshUpkeepCost(DecayEntity ent)
		{
			if (ent.HasAddedUpkeepToBuilding)
			{
				RemoveUpkeepCost(ent);
				AddUpkeepCost(ent);
			}
		}

		public bool IsEmpty()
		{
			if (HasBuildingPrivileges())
			{
				return false;
			}
			if (HasBuildingBlocks())
			{
				return false;
			}
			if (HasDecayEntities())
			{
				return false;
			}
			return true;
		}

		public BuildingPrivlidge GetDominatingBuildingPrivilege()
		{
			BuildingPrivlidge buildingPrivlidge = null;
			if (HasBuildingPrivileges())
			{
				for (int i = 0; i < buildingPrivileges.Count; i++)
				{
					BuildingPrivlidge buildingPrivlidge2 = buildingPrivileges[i];
					if (!((Object)(object)buildingPrivlidge2 == (Object)null) && buildingPrivlidge2.IsOlderThan(buildingPrivlidge))
					{
						buildingPrivlidge = buildingPrivlidge2;
					}
				}
			}
			return buildingPrivlidge;
		}

		public bool HasBuildingPrivileges()
		{
			if (buildingPrivileges != null)
			{
				return buildingPrivileges.Count > 0;
			}
			return false;
		}

		public bool HasBuildingBlocks()
		{
			if (buildingBlocks != null)
			{
				return buildingBlocks.Count > 0;
			}
			return false;
		}

		public bool HasDecayEntities()
		{
			if (decayEntities != null)
			{
				return decayEntities.Count > 0;
			}
			return false;
		}

		public void AddBuildingPrivilege(BuildingPrivlidge ent)
		{
			if (!((Object)(object)ent == (Object)null) && !buildingPrivileges.Contains(ent))
			{
				buildingPrivileges.Add(ent);
			}
		}

		public void RemoveBuildingPrivilege(BuildingPrivlidge ent)
		{
			if (!((Object)(object)ent == (Object)null))
			{
				buildingPrivileges.Remove(ent);
			}
		}

		public void AddBuildingBlock(BuildingBlock ent)
		{
			if ((Object)(object)ent == (Object)null || buildingBlocks.Contains(ent))
			{
				return;
			}
			buildingBlocks.Add(ent);
			if (!AI.useUnityNavmesh || !AI.nav_carve_use_building_optimization)
			{
				return;
			}
			NavMeshObstacle component = ((Component)ent).GetComponent<NavMeshObstacle>();
			if ((Object)(object)component != (Object)null)
			{
				isNavMeshCarvingDirty = true;
				if (navmeshCarvers == null)
				{
					navmeshCarvers = new ListHashSet<NavMeshObstacle>();
				}
				navmeshCarvers.Add(component);
			}
		}

		public void RemoveBuildingBlock(BuildingBlock ent)
		{
			if ((Object)(object)ent == (Object)null)
			{
				return;
			}
			buildingBlocks.Remove(ent);
			if (!AI.useUnityNavmesh || !AI.nav_carve_use_building_optimization || navmeshCarvers == null)
			{
				return;
			}
			NavMeshObstacle component = ((Component)ent).GetComponent<NavMeshObstacle>();
			if (!((Object)(object)component != (Object)null))
			{
				return;
			}
			navmeshCarvers.Remove(component);
			if (navmeshCarvers.Count == 0)
			{
				navmeshCarvers = null;
			}
			isNavMeshCarvingDirty = true;
			if (navmeshCarvers == null)
			{
				Building building = ent.GetBuilding();
				if (building != null)
				{
					int ticks = 2;
					server.UpdateNavMeshCarver(building, ref ticks, 0);
				}
			}
		}

		public void AddDecayEntity(DecayEntity ent)
		{
			if (!((Object)(object)ent == (Object)null))
			{
				if (decayEntities.TryAdd(ent) && ent.isServer)
				{
					AddUpkeepCost(ent);
				}
				if (ent is Door door && !doors.Contains(door))
				{
					doors.Add(ent as Door);
				}
			}
		}

		public void RemoveDecayEntity(DecayEntity ent)
		{
			if (!((Object)(object)ent == (Object)null))
			{
				if (ent.isServer)
				{
					RemoveUpkeepCost(ent);
				}
				decayEntities.Remove(ent);
				if (ent is Door door)
				{
					doors.Remove(door);
				}
			}
		}

		public void Add(DecayEntity ent)
		{
			AddDecayEntity(ent);
			AddBuildingBlock(ent as BuildingBlock);
			AddBuildingPrivilege(ent as BuildingPrivlidge);
		}

		public void Remove(DecayEntity ent)
		{
			RemoveDecayEntity(ent);
			RemoveBuildingBlock(ent as BuildingBlock);
			RemoveBuildingPrivilege(ent as BuildingPrivlidge);
		}

		public void Dirty()
		{
			BuildingPrivlidge dominatingBuildingPrivilege = GetDominatingBuildingPrivilege();
			if ((Object)(object)dominatingBuildingPrivilege != (Object)null)
			{
				dominatingBuildingPrivilege.BuildingDirty();
			}
		}

		public bool IsABoatBuilding()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Enumerator<BuildingBlock> enumerator = buildingBlocks.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current is BoatBuildingBlock)
					{
						return true;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			return false;
		}
	}

	[ClientVar(Help = "Whether building privilege lookups use the cache instead of physics queries")]
	[ServerVar(Help = "Whether building privilege lookups use the cache instead of physics queries")]
	public static bool PrivilegeCache = true;

	private SortedGrid<PrivilegeCandidate> buildingBlockGrid = new SortedGrid<PrivilegeCandidate>(16, 8096f);

	private readonly Dictionary<BuildingBlock, OBB> buildingBlockBounds = new Dictionary<BuildingBlock, OBB>();

	private float maxBuildingBlockExtent;

	[ClientVar(ClientAdmin = true, Help = "Draws this client's building privilege queries")]
	[ServerVar(Help = "Draws server building privilege queries")]
	public static bool DebugPrivilegeQueries = false;

	[ServerVar(Help = "Draws the building block grid around admins")]
	public static bool DebugPrivilegeGrid = false;

	private float nextDebugGridBroadcastTime;

	public static ServerBuildingManager server = new ServerBuildingManager();

	public ListDictionary<uint, Building> buildingDictionary = new ListDictionary<uint, Building>();

	private void AddToPrivilegeCache(DecayEntity ent)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (ent is BuildingBlock buildingBlock && !(buildingBlock is BoatBuildingBlock) && buildingBlock.net != null)
		{
			OBB val = buildingBlock.WorldSpaceBounds();
			if (buildingBlockBounds.TryAdd(buildingBlock, val))
			{
				buildingBlockGrid.Add(new PrivilegeCandidate(buildingBlock, val), val.position.x, val.position.z);
				maxBuildingBlockExtent = Mathf.Max(maxBuildingBlockExtent, val.extents.magnitude);
			}
		}
	}

	private void RemoveFromPrivilegeCache(DecayEntity ent)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (ent is BuildingBlock buildingBlock && buildingBlockBounds.Remove(buildingBlock, out var value))
		{
			buildingBlockGrid.Remove(new PrivilegeCandidate(buildingBlock, value), value.position.x, value.position.z);
		}
	}

	private void ClearPrivilegeCache()
	{
		buildingBlockGrid = new SortedGrid<PrivilegeCandidate>(16, 8096f);
		buildingBlockBounds.Clear();
		maxBuildingBlockExtent = 0f;
	}

	public void QueryBuildingBlockCells(Vector3 position, float radius, List<List<PrivilegeCandidate>> cells)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		buildingBlockGrid.QueryCells(position.x, position.z, radius + maxBuildingBlockExtent, cells);
	}

	public OBB GetBuildingBlockBounds(BuildingBlock block)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return buildingBlockBounds[block];
	}

	public void RefreshBuildingBlock(BuildingBlock block)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (buildingBlockBounds.TryGetValue(block, out var value))
		{
			OBB val = block.WorldSpaceBounds();
			buildingBlockBounds[block] = val;
			buildingBlockGrid.Remove(new PrivilegeCandidate(block, value), value.position.x, value.position.z);
			buildingBlockGrid.Add(new PrivilegeCandidate(block, val), val.position.x, val.position.z);
		}
	}

	protected void DrawPrivilegeGridDebug()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		if (!DebugPrivilegeGrid || Time.realtimeSinceStartup < nextDebugGridBroadcastTime)
		{
			return;
		}
		nextDebugGridBroadcastTime = Time.realtimeSinceStartup + 1f;
		List<PrivilegeCandidate> list = Pool.Get<List<PrivilegeCandidate>>();
		Dictionary<Vector2Int, int> dictionary = Pool.Get<Dictionary<Vector2Int, int>>();
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			int num = default;
			int num2 = default;
			int num5 = default;
			int num6 = default;
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!current.IsAdmin)
				{
					continue;
				}
				Vector3 position = ((Component)current).transform.position;
				buildingBlockGrid.GetCell(position.x, position.z, ref num, ref num2);
				Vector2Int val = new Vector2Int(num, num2);
				list.Clear();
				dictionary.Clear();
				buildingBlockGrid.Query(position.x, position.z, 48f, list);
				float num3 = float.MaxValue;
				float num4 = float.MinValue;
				foreach (PrivilegeCandidate item in list)
				{
					OBB val2 = buildingBlockBounds[item.Block];
					buildingBlockGrid.GetCell(val2.position.x, val2.position.z, ref num5, ref num6);
					Vector2Int val3 = new Vector2Int(num5, num6);
					dictionary.TryGetValue(val3, out var value);
					dictionary[val3] = value + 1;
					num3 = Mathf.Min(num3, val2.position.y - val2.extents.y);
					num4 = Mathf.Max(num4, val2.position.y + val2.extents.y);
					if (val3 == val)
					{
						ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Box(val2.position, 1f, Color.cyan.WithAlpha(0.15f), val2.extents * 2f, val2.rotation, distanceFade: false, zTest: false));
						ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(val2.position, 1f, Color.white, 0.15f, distanceFade: false, zTest: false));
					}
				}
				foreach (KeyValuePair<Vector2Int, int> item2 in dictionary)
				{
					float a = ((item2.Key == val) ? 1f : 0.05f);
					Vector2Int key = item2.Key;
					int x = key.x;
					key = item2.Key;
					Bounds gridCellBox = GetGridCellBox(x, key.y, num3, num4);
					ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Bounds(gridCellBox, 1f, Color.yellow.WithAlpha(a), distanceFade: false));
					ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Text(new Vector3(gridCellBox.center.x, gridCellBox.max.y, gridCellBox.center.z), 1f, Color.yellow, $"{item2.Value} blocks", 2f, distanceFade: false));
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Pool.FreeUnmanaged<PrivilegeCandidate>(ref list);
		Pool.FreeUnmanaged<Vector2Int, int>(ref dictionary);
	}

	public void DrawPrivilegeQueryDebug(BaseEntity querier, OBB obb, float searchRadius, BuildingBlock bestBlock, BuildingPrivlidge bestPriv)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		Color color = (((Object)(object)bestPriv != (Object)null) ? Color.green : Color.red);
		string id = $"privilegequery{((Object)querier).GetInstanceID()}";
		DebugDrawClear(id);
		float num = searchRadius + maxBuildingBlockExtent;
		int num2 = default;
		int num3 = default;
		buildingBlockGrid.GetCell(obb.position.x - num, obb.position.z - num, ref num2, ref num3);
		int num4 = default;
		int num5 = default;
		buildingBlockGrid.GetCell(obb.position.x + num, obb.position.z + num, ref num4, ref num5);
		List<PrivilegeCandidate> list = Pool.Get<List<PrivilegeCandidate>>();
		buildingBlockGrid.Query(obb.position.x, obb.position.z, num, list);
		float num6 = float.MaxValue;
		float num7 = float.MinValue;
		foreach (PrivilegeCandidate item in list)
		{
			OBB val = buildingBlockBounds[item.Block];
			num6 = Mathf.Min(num6, val.position.y - val.extents.y);
			num7 = Mathf.Max(num7, val.position.y + val.extents.y);
		}
		int count = list.Count;
		Pool.FreeUnmanaged<PrivilegeCandidate>(ref list);
		int num8 = -1;
		int num9 = -1;
		if ((Object)(object)bestBlock != (Object)null)
		{
			Vector3 position = buildingBlockBounds[bestBlock].position;
			buildingBlockGrid.GetCell(position.x, position.z, ref num8, ref num9);
		}
		if (count > 0)
		{
			for (int i = num2; i <= num4; i++)
			{
				for (int j = num3; j <= num5; j++)
				{
					float a = ((i == num8 && j == num9) ? 1f : 0.05f);
					DebugDrawBounds(GetGridCellBox(i, j, num6, num7), Color.yellow.WithAlpha(a), id);
				}
			}
		}
		DebugDrawSphere(obb.position, 0.2f, color, id);
		DebugDrawText(obb.position + Vector3.up * 0.5f, string.Format("{0}: {1} blocks\nTC {2}", querier.isServer ? "SV" : "CL", count, ((Object)(object)bestPriv != (Object)null) ? ((object)bestPriv.net.ID/*cast due to constrained. prefix*/).ToString() : "none"), color, id);
		if ((Object)(object)bestBlock != (Object)null && (Object)(object)bestPriv != (Object)null)
		{
			OBB val2 = buildingBlockBounds[bestBlock];
			DebugDrawBox(val2, Color.cyan, id);
			DebugDrawLine(obb.position, val2.position, Color.yellow, id);
			DebugDrawLine(val2.position, ((Component)bestPriv).transform.position, Color.cyan, id);
			DebugDrawBox(bestPriv.WorldSpaceBounds(), Color.green, id);
		}
	}

	private void DebugDrawClear(string id)
	{
	}

	private void DebugDrawBounds(Bounds bounds, Color color, string id)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (this is ServerBuildingManager)
		{
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Bounds(bounds, 1f, color, distanceFade: false));
		}
	}

	private void DebugDrawSphere(Vector3 position, float radius, Color color, string id)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (this is ServerBuildingManager)
		{
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(position, 1f, color, radius, distanceFade: false));
		}
	}

	private void DebugDrawText(Vector3 position, string text, Color color, string id)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (this is ServerBuildingManager)
		{
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Text(position, 1f, color, text, 2f, distanceFade: false));
		}
	}

	private void DebugDrawLine(Vector3 start, Vector3 end, Color color, string id)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (this is ServerBuildingManager)
		{
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Line(start, end, 1f, color, distanceFade: false));
		}
	}

	private void DebugDrawBox(OBB bounds, Color color, string id)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (this is ServerBuildingManager)
		{
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Box(bounds.position, 1f, color, bounds.extents * 2f, bounds.rotation, distanceFade: false, zTest: false));
		}
	}

	private Bounds GetGridCellBox(int cellX, int cellZ, float bottom, float top)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		float num = default;
		float num2 = default;
		buildingBlockGrid.GetCellMin(cellX, cellZ, ref num, ref num2);
		int cellSize = buildingBlockGrid.CellSize;
		Bounds result = default;
		result.SetMinMax(new Vector3(num, bottom, num2), new Vector3(num + (float)cellSize, top, num2 + (float)cellSize));
		return result;
	}

	public Building GetBuilding(uint buildingID)
	{
		Building result = null;
		buildingDictionary.TryGetValue(buildingID, ref result);
		return result;
	}

	public void Add(DecayEntity ent)
	{
		if (ent.buildingID != 0)
		{
			Building building = GetBuilding(ent.buildingID);
			if (building == null)
			{
				building = CreateBuilding(ent.buildingID);
				buildingDictionary.Add(ent.buildingID, building);
			}
			building.Add(ent);
			building.Dirty();
			AddToPrivilegeCache(ent);
		}
	}

	public void Remove(DecayEntity ent)
	{
		if (ent.buildingID == 0)
		{
			return;
		}
		RemoveFromPrivilegeCache(ent);
		Building building = GetBuilding(ent.buildingID);
		if (building != null)
		{
			building.Remove(ent);
			if (building.IsEmpty())
			{
				buildingDictionary.Remove(ent.buildingID);
				DisposeBuilding(ref building);
			}
			else
			{
				building.Dirty();
			}
		}
	}

	public void Clear()
	{
		buildingDictionary.Clear();
		ClearPrivilegeCache();
	}

	protected abstract Building CreateBuilding(uint id);

	protected abstract void DisposeBuilding(ref Building building);
}
