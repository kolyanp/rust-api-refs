using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using ConVar;
using Cysharp.Text;
using Facepunch;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2.Nav;

public class RustNavmesh : IDisposable
{
	private static Vector3[] TilePolysBuffer = new Vector3[12288];

	private static byte[] TileAreasBuffer = new byte[2048];

	private static ushort[] TileFlagsBuffer = new ushort[2048];

	private static Vector3[] PathBuffer = new Vector3[256];

	private static Vector3[] CornerBuffer = new Vector3[256];

	private static Vector3[] DonutPointsBuffer = new Vector3[64];

	public NavMeshBuildParams BuildParams = new NavMeshBuildParams(true);

	public NavMeshBuildParams BuildParamsHiRes = new NavMeshBuildParams(true);

	public int PathfindingMaxIterations = 1000;

	public Bounds CurrentNavmeshBounds;

	public Tile[] tiles;

	public IntPtr NavMeshHandle = IntPtr.Zero;

	public string debugName = "unnamed";

	public long workerBuildTicks;

	public double lastFullBuildSeconds = -1.0;

	public const string TempSaveSuffix = ".new";

	private float cachedMaxBorderMeters;

	private double builtStartTime;

	private int numBuiltTiles;

	private Vector2Int tileNum;

	private BackgroundTileBuilder tileBuilder;

	private int dataVersionAtLastSave;

	private readonly List<Tile> dirtyTilesSinceSave = new List<Tile>();

	private int tilesWithData;

	private readonly RustNavmeshSaveJob saveJob = new RustNavmeshSaveJob();

	private string deltaBasePath;

	private long deltaBaseBytes;

	private long deltaAppendedBytes;

	private NavmeshSaveStats inFlightStats;

	private string inFlightPath;

	private string followUpPath;

	public bool EmitTileChangeEvents;

	public bool ForceHiRes;

	private static bool loggedSaveProcessorCounts;

	private const int MaxSaveThreads = 32;

	private const int RenameAttempts = 3;

	private const int RenameRetryDelayMs = 100;

	public int NumBuiltTiles => numBuiltTiles;

	public int TotalTiles
	{
		get
		{
			if (tiles == null)
			{
				return 0;
			}
			return tiles.Length;
		}
	}

	public NavmeshSaveStats LastSaveStats { get; private set; }

	public bool IsSaveInFlight { get; private set; }

	public bool CullTilesFarFromShore { get; private set; }

	public int TileChangeVersion { get; private set; }

	public int TileDataVersion { get; private set; }

	private static int DebugSaveDelayMs => 0;

	public bool IsValid()
	{
		return NavMeshHandle != IntPtr.Zero;
	}

	public RustNavmesh(BackgroundTileBuilder tileBuilder, NavMeshBuildParams? buildParamsOverride = null, NavMeshBuildParams? buildParamsHiResOverride = null, Bounds? boundsOverride = null, bool shouldBuild = true, bool synchronous = false, bool forceHiRes = false, bool cullTilesFarFromShore = false)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh)
		{
			return;
		}
		if (tileBuilder == null)
		{
			RustNavigation.LogError("BackgroundTileBuilder is required to create a RustNavmesh");
			return;
		}
		this.tileBuilder = tileBuilder;
		ForceHiRes = forceHiRes;
		CullTilesFarFromShore = cullTilesFarFromShore;
		if (boundsOverride.HasValue)
		{
			CurrentNavmeshBounds = boundsOverride.Value;
		}
		else
		{
			CurrentNavmeshBounds = new Bounds(TerrainMeta.Center, TerrainMeta.Size);
		}
		if (buildParamsOverride.HasValue)
		{
			BuildParams = buildParamsOverride.Value;
		}
		else
		{
			BuildParams = RustNavigation.Instance.BuildParams;
		}
		if (buildParamsHiResOverride.HasValue)
		{
			BuildParamsHiRes = buildParamsHiResOverride.Value;
		}
		else
		{
			BuildParamsHiRes = RustNavigation.Instance.BuildParamsHiRes;
		}
		cachedMaxBorderMeters = Mathf.Max(BorderMeters(in BuildParams), BorderMeters(in BuildParamsHiRes));
		float num = BuildParams.tileSize * BuildParams.cellSize;
		float num2 = BuildParamsHiRes.tileSize * BuildParamsHiRes.cellSize;
		if (Mathf.Abs(num - num2) > 0.001f)
		{
			RustNavigation.LogError($"Tile world size mismatch: lo {num:F6} hi {num2:F6}");
			return;
		}
		NavMeshHandle = RecastWrapper.CreateEmptyNavMesh(in BuildParams, CurrentNavmeshBounds.min, CurrentNavmeshBounds.max);
		if (NavMeshHandle == IntPtr.Zero)
		{
			RustNavigation.LogError("Failed to create empty navmesh");
			Dispose();
			return;
		}
		tileNum = rcCalcTileNum();
		tiles = new Tile[tileNum.x * tileNum.y];
		for (int i = 0; i < tileNum.y; i++)
		{
			for (int j = 0; j < tileNum.x; j++)
			{
				Tile tile = new Tile(j, i);
				tiles[Mathx.FlattenArrayCoord(j, i, tileNum.x)] = tile;
			}
		}
		if (!shouldBuild)
		{
			return;
		}
		builtStartTime = Time.realtimeSinceStartupAsDouble;
		int num3 = 0;
		for (int k = 0; k < tileNum.y; k++)
		{
			for (int l = 0; l < tileNum.x; l++)
			{
				if (!tileBuilder.EnqueueOnMainThread(this, l, k, synchronous))
				{
					num3++;
				}
			}
		}
		if (num3 > 0)
		{
			RustNavigation.Log($"Dropped {num3} of {tiles.Length} tiles for sitting more than {RustNav.maxShoreDistance:0.#}m out to sea.");
		}
	}

	private RustNavmesh(BackgroundTileBuilder tileBuilder, IntPtr loadedHandle, in ManagedNavPayload payload, bool cullTilesFarFromShore)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh)
		{
			return;
		}
		this.tileBuilder = tileBuilder;
		CullTilesFarFromShore = cullTilesFarFromShore;
		CurrentNavmeshBounds = payload.currentNavmeshBounds;
		BuildParams = payload.buildParams;
		BuildParamsHiRes = payload.buildParamsHiRes;
		cachedMaxBorderMeters = Mathf.Max(BorderMeters(in BuildParams), BorderMeters(in BuildParamsHiRes));
		NavMeshHandle = loadedHandle;
		Vector2Int val = rcCalcTileNum();
		if (payload.tileNum != val)
		{
			RustNavigation.LogError($"Loaded navmesh tile grid {payload.tileNum} does not match bounds/params ({val})");
			NavMeshHandle = IntPtr.Zero;
			return;
		}
		tileNum = payload.tileNum;
		tiles = new Tile[tileNum.x * tileNum.y];
		for (int i = 0; i < tileNum.y; i++)
		{
			for (int j = 0; j < tileNum.x; j++)
			{
				tiles[Mathx.FlattenArrayCoord(j, i, tileNum.x)] = new Tile(j, i);
			}
		}
	}

	public void SetTileBuilder(BackgroundTileBuilder tileBuilder)
	{
		this.tileBuilder = tileBuilder;
	}

	public bool IsBuilt()
	{
		if (NavMeshHandle == IntPtr.Zero)
		{
			return false;
		}
		return numBuiltTiles == tiles.Length;
	}

	public void NotifyPolyFlagsChanged()
	{
		TileChangeVersion++;
	}

	private void MarkTileAsBuilt(Tile tile, bool dataChanged = true)
	{
		if (tile == null)
		{
			return;
		}
		TileChangeVersion++;
		if (dataChanged)
		{
			TileDataVersion++;
			if (deltaBasePath != null && !tile.dirtySinceSave)
			{
				tile.dirtySinceSave = true;
				dirtyTilesSinceSave.Add(tile);
			}
		}
		if (EmitTileChangeEvents)
		{
			RustNavigation.NotifyDefaultNavmeshTileChanged(tile.tx, tile.ty);
		}
		if (!tile.wasBuiltOnce)
		{
			numBuiltTiles++;
			tile.wasBuiltOnce = true;
			if (IsBuilt())
			{
				lastFullBuildSeconds = Time.realtimeSinceStartupAsDouble - builtStartTime;
				RustNavigation.Log($"Navmesh '{debugName}' is now fully built in {lastFullBuildSeconds:F2} seconds ({numBuiltTiles} tiles).");
			}
		}
	}

	public void FailTile(int tx, int ty)
	{
		if (tiles == null)
		{
			return;
		}
		Tile tile = GetTile(tx, ty);
		if (tile == null)
		{
			RustNavigation.LogError($"FailTile: tile coordinates out of range: {tx},{ty}");
			return;
		}
		bool hasData = tile.hasData;
		if (hasData && NavMeshHandle != IntPtr.Zero)
		{
			EnsureNoSaveInFlight("FailTile");
			RecastWrapper.RemoveTileFromNavMesh(NavMeshHandle, tx, ty);
		}
		if (hasData)
		{
			tilesWithData--;
		}
		tile.hasData = false;
		MarkTileAsBuilt(tile, hasData);
	}

	public bool AddTile(int tx, int ty, IntPtr tileData, int dataSize)
	{
		if (tiles == null)
		{
			return false;
		}
		EnsureNoSaveInFlight("AddTile");
		if (!RecastWrapper.AddPrebuiltTileToNavMesh(NavMeshHandle, tx, ty, tileData, dataSize))
		{
			FailTile(tx, ty);
			return false;
		}
		Tile tile = GetTile(tx, ty);
		if (tile == null)
		{
			RustNavigation.LogError($"AddTile: tile coordinates out of range: {tx},{ty}");
			return false;
		}
		if (!tile.hasData)
		{
			tilesWithData++;
		}
		tile.hasData = true;
		MarkTileAsBuilt(tile);
		RustNavDoorGates.OnTileBuilt(this, tx, ty);
		return true;
	}

	private static void DoorPolyFlagMasks(DoorPolyState state, out ushort setFlags, out ushort clearFlags)
	{
		switch (state)
		{
		case DoorPolyState.Open:
			setFlags = 1;
			clearFlags = 32;
			break;
		case DoorPolyState.NpcOpenable:
			setFlags = 32;
			clearFlags = 1;
			break;
		default:
			setFlags = 0;
			clearFlags = 33;
			break;
		}
	}

	public bool SetDoorPolyFlagsForRefs(ulong[] refs, int refCount, DoorPolyState state, NavVector3 center, Vector3 halfExtents, Vector2 axisXZ)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.SetDoorPolyFlagsForRefs"))
		{
			if (!IsValid() || refs == null || refCount <= 0)
			{
				return false;
			}
			EnsureNoSaveInFlight("SetDoorPolyFlagsForRefs");
			DoorPolyFlagMasks(state, out var setFlags, out var clearFlags);
			int num = RecastWrapper.SetPolyFlagsForRefs(NavMeshHandle, refs, refCount, setFlags, clearFlags);
			if (num < 0)
			{
				return false;
			}
			if (num == 0)
			{
				return true;
			}
			NotifyPolyFlagsChanged();
			NotifyDoorBoxTilesChanged(center, halfExtents, axisXZ);
			return true;
		}
	}

	private void NotifyDoorBoxTilesChanged(NavVector3 center, Vector3 halfExtents, Vector2 axisXZ)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (!EmitTileChangeEvents)
		{
			return;
		}
		Vector3 val = new Vector3(Mathf.Abs(axisXZ.x) * halfExtents.x + Mathf.Abs(axisXZ.y) * halfExtents.z, halfExtents.y, Mathf.Abs(axisXZ.y) * halfExtents.x + Mathf.Abs(axisXZ.x) * halfExtents.z);
		Vector2Int val2 = rcCalcTileCoordFromPos(center.Value - val);
		Vector2Int val3 = rcCalcTileCoordFromPos(center.Value + val);
		for (int i = val2.x; i <= val3.x; i++)
		{
			for (int j = val2.y; j <= val3.y; j++)
			{
				RustNavigation.NotifyDefaultNavmeshTileChanged(i, j);
			}
		}
	}

	public bool SetDoorPolyFlags(NavVector3 center, Vector3 halfExtents, Vector2 axisXZ, DoorPolyState state, int area, NavVector3 ownerPos, Vector3[] groupOwnerPositions, int groupOwnerCount, ulong[] outRefs, int maxOutRefs, out int outRefCount)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.SetDoorPolyFlags"))
		{
			outRefCount = -1;
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return false;
			}
			EnsureNoSaveInFlight("SetDoorPolyFlags");
			DoorPolyFlagMasks(state, out var setFlags, out var clearFlags);
			if (RecastWrapper.SetPolyFlagsInObb(NavMeshHandle, in center.Value, in halfExtents, axisXZ.x, axisXZ.y, area, setFlags, clearFlags, in ownerPos.Value, groupOwnerPositions, groupOwnerCount, outRefs, maxOutRefs, out outRefCount) <= 0)
			{
				return false;
			}
			NotifyPolyFlagsChanged();
			NotifyDoorBoxTilesChanged(center, halfExtents, axisXZ);
			return true;
		}
	}

	public Tile GetTile(int tx, int ty)
	{
		if (tiles == null || tx < 0 || ty < 0 || tx >= tileNum.x || ty >= tileNum.y)
		{
			return null;
		}
		return tiles[Mathx.FlattenArrayCoord(tx, ty, tileNum.x)];
	}

	public void GetTilesInBounds(Bounds bounds, List<Vector2Int> tiles)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		tiles.Clear();
		Vector2Int val = rcCalcTileCoordFromPos(bounds.min);
		Vector2Int val2 = rcCalcTileCoordFromPos(bounds.max);
		for (int i = val.x; i <= val2.x; i++)
		{
			for (int j = val.y; j <= val2.y; j++)
			{
				tiles.Add(new Vector2Int(i, j));
			}
		}
	}

	public void RebuildTilesInBounds(Bounds rebuildBounds, bool synchronous)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.RebuildTilesInBounds"))
		{
			if (!IsValid())
			{
				return;
			}
			rebuildBounds = rcExpandTileBounds(rebuildBounds);
			if (!CurrentNavmeshBounds.Intersects(rebuildBounds))
			{
				return;
			}
			Vector2Int val = rcCalcTileCoordFromPos(rebuildBounds.min);
			Vector2Int val2 = rcCalcTileCoordFromPos(rebuildBounds.max);
			for (int i = val.x; i <= val2.x; i++)
			{
				for (int j = val.y; j <= val2.y; j++)
				{
					tileBuilder.EnqueueOnMainThread(this, i, j, synchronous);
				}
			}
		}
	}

	public Vector2Int rcCalcTileCoordFromPos(Vector3 pos)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		float num = BuildParams.tileSize * BuildParams.cellSize;
		int num2 = Mathf.FloorToInt((pos.x - CurrentNavmeshBounds.min.x) / num);
		int num3 = Mathf.FloorToInt((pos.z - CurrentNavmeshBounds.min.z) / num);
		int num4 = Mathf.Clamp(num2, 0, tileNum.x - 1);
		num3 = Mathf.Clamp(num3, 0, tileNum.y - 1);
		return new Vector2Int(num4, num3);
	}

	private Vector2Int rcCalcTileNum()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)((CurrentNavmeshBounds.max.x - CurrentNavmeshBounds.min.x) / BuildParams.cellSize + 0.5f);
		int num2 = (int)((CurrentNavmeshBounds.max.z - CurrentNavmeshBounds.min.z) / BuildParams.cellSize + 0.5f);
		int num3 = (int)(((float)num + BuildParams.tileSize - 1f) / BuildParams.tileSize);
		int num4 = (int)(((float)num2 + BuildParams.tileSize - 1f) / BuildParams.tileSize);
		return new Vector2Int(num3, num4);
	}

	public Bounds rcCalcTileBounds(Vector2Int tileCoord)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		float num = BuildParams.tileSize * BuildParams.cellSize;
		Vector3 val = new Vector3(CurrentNavmeshBounds.min.x + (float)tileCoord.x * num, CurrentNavmeshBounds.min.y, CurrentNavmeshBounds.min.z + (float)tileCoord.y * num);
		Vector3 val2 = new Vector3(CurrentNavmeshBounds.min.x + (float)(tileCoord.x + 1) * num, CurrentNavmeshBounds.max.y, CurrentNavmeshBounds.min.z + (float)(tileCoord.y + 1) * num);
		return new Bounds((val + val2) * 0.5f, val2 - val);
	}

	private static float BorderMeters(in NavMeshBuildParams p)
	{
		return (float)(Mathf.CeilToInt(p.agentRadius / p.cellSize) + 3) * p.cellSize;
	}

	public Bounds rcExpandTileBounds(Bounds tileBounds)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = new Vector3(cachedMaxBorderMeters, 0f, cachedMaxBorderMeters);
		tileBounds.min -= val;
		tileBounds.max += val;
		return tileBounds;
	}

	public bool IsTileFarFromShore(int tx, int ty)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if (!CullTilesFarFromShore)
		{
			return false;
		}
		float maxShoreDistance = RustNav.maxShoreDistance;
		if (maxShoreDistance <= 0f)
		{
			return false;
		}
		TerrainTexturing texturing = TerrainMeta.Texturing;
		if ((Object)(object)texturing == (Object)null || !texturing.TexturesInitialized)
		{
			return false;
		}
		Bounds worldBounds = rcExpandTileBounds(rcCalcTileBounds(new Vector2Int(tx, ty)));
		float coarseDistanceToShore = texturing.GetCoarseDistanceToShore(worldBounds.center);
		if (!float.IsFinite(coarseDistanceToShore))
		{
			return false;
		}
		Vector2 val = new Vector2(worldBounds.extents.x, worldBounds.extents.z);
		float magnitude = val.magnitude;
		if (coarseDistanceToShore - magnitude <= maxShoreDistance)
		{
			return false;
		}
		if (!RustNavigation.HasTunnelRegions || (Object)(object)RustNavigation.Instance == (Object)null)
		{
			return true;
		}
		return !RustNavigation.Instance.IsInTunnelRegion(worldBounds);
	}

	public bool GetTilePolysInternal(int tx, int ty, List<Vector3> polys)
	{
		return GetTilePolysWithStateInternal(tx, ty, polys, null, null);
	}

	public void ResetDoorPolyFlagsToOpen()
	{
		if (RustNavigation.EnsureNewNavmesh() && IsValid())
		{
			EnsureNoSaveInFlight("ResetDoorPolyFlagsToOpen");
			DoorPolyFlagMasks(DoorPolyState.Open, out var setFlags, out var clearFlags);
			if (RecastWrapper.ResetPolyFlagsForArea(NavMeshHandle, 3, setFlags, clearFlags) + RecastWrapper.ResetPolyFlagsForArea(NavMeshHandle, 6, setFlags, clearFlags) > 0)
			{
				NotifyPolyFlagsChanged();
			}
		}
	}

	private bool FillPathFromPathBuffer(List<NavVector3> path, int pathCount)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (!IsValid())
		{
			if (AI.logIssues)
			{
				RustNavigation.LogError("NavMesh has not been built yet.");
			}
			return false;
		}
		path.Clear();
		path.Capacity = Mathf.Max(path.Capacity, pathCount);
		for (int i = 0; i < pathCount; i++)
		{
			path.Add(new NavVector3(PathBuffer[i]));
		}
		return true;
	}

	public bool SamplePosition(NavVector3 position, out NavHit hit, Vector3 extents, bool allowNpcDoors = false)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		ulong nearestPolyRef;
		return SamplePositionPoly(position, out hit, extents, out nearestPolyRef, includeGatedDoorPolys: false, allowNpcDoors);
	}

	public bool SamplePositionPoly(NavVector3 position, out NavHit hit, Vector3 extents, out ulong nearestPolyRef, bool includeGatedDoorPolys = false, bool allowNpcDoors = false)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.SamplePosition"))
		{
			hit = default;
			nearestPolyRef = 0uL;
			if (!IsValid())
			{
				if (AI.logIssues)
				{
					RustNavigation.LogError("NavMesh has not been built yet.");
				}
				return false;
			}
			if (!RecastWrapper.SamplePosition(NavMeshHandle, in position.Value, in extents, out var nearestPosition, out nearestPolyRef, includeGatedDoorPolys ? 1 : 0, allowNpcDoors ? 1 : 0))
			{
				return false;
			}
			if (nearestPosition == Vector3.zero)
			{
				return false;
			}
			hit = new NavHit
			{
				position = new NavVector3(nearestPosition)
			};
			return true;
		}
	}

	public bool Raycast(NavVector3 startPos, NavVector3 endPos, out NavHit hit, bool allowNpcDoors = false)
	{
		ulong startRef = 0uL;
		return Raycast(ref startRef, startPos, endPos, out hit, allowNpcDoors);
	}

	public bool Raycast(ref ulong startRef, NavVector3 startPos, NavVector3 endPos, out NavHit hit, bool allowNpcDoors = false)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.Raycast"))
		{
			hit = default;
			if (!IsValid())
			{
				if (AI.logIssues)
				{
					RustNavigation.LogError("NavMesh has not been built yet.");
				}
				hit = new NavHit
				{
					position = new NavVector3(Vector3.negativeInfinity)
				};
				return false;
			}
			bool result = RecastWrapper.Raycast(NavMeshHandle, ref startRef, in startPos.Value, in endPos.Value, out var hitLocation, out var hitNormal, allowNpcDoors ? 1 : 0);
			hit = new NavHit
			{
				position = new NavVector3(hitLocation),
				normal = new NavVector3(hitNormal)
			};
			return result;
		}
	}

	public bool Move(ref ulong polyRef, NavVector3 startPos, NavVector3 endPos, out NavVector3 movedPos, bool allowNpcDoors = false)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.Move"))
		{
			movedPos = startPos;
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			if (!IsValid())
			{
				if (AI.logIssues)
				{
					RustNavigation.LogError("NavMesh has not been built yet.");
				}
				return false;
			}
			if (!RecastWrapper.Move(NavMeshHandle, ref polyRef, in startPos.Value, in endPos.Value, out var movedPos2, allowNpcDoors ? 1 : 0))
			{
				return false;
			}
			movedPos = new NavVector3(movedPos2);
			return true;
		}
	}

	public bool CalculatePath(NavVector3 start, NavVector3 end, RustNavMeshPath path, bool allowNpcDoors = false)
	{
		ulong startRef = 0uL;
		return CalculatePath(ref startRef, start, end, path, allowNpcDoors);
	}

	public bool CalculatePath(ref ulong startRef, NavVector3 start, NavVector3 end, RustNavMeshPath path, bool allowNpcDoors = false)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		path.Reset();
		if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
		{
			return false;
		}
		DtStatus dtStatus = RecastWrapper.FindPath(NavMeshHandle, ref startRef, in start.Value, in end.Value, PathBuffer, out var pathLength, path.polyRefs, out path.polyRefCount, PathfindingMaxIterations, allowNpcDoors ? 1 : 0);
		if ((dtStatus & DtStatus.Failure) == DtStatus.Failure)
		{
			return false;
		}
		if (pathLength <= 0)
		{
			return false;
		}
		if (!FillPathFromPathBuffer(path.corners, pathLength))
		{
			return false;
		}
		path.status = (NavMeshPathStatus)((dtStatus & (DtStatus.BufferTooSmall | DtStatus.PartialResult)) != 0);
		return true;
	}

	public bool IsValidPolyRef(ulong polyRef)
	{
		if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
		{
			return false;
		}
		return RecastWrapper.IsValidPolyRef(NavMeshHandle, polyRef);
	}

	public bool CorridorMove(IntPtr corridor, NavVector3 desiredPos, out NavVector3 resultPos, out ulong firstPolyRef, bool allowNpcDoors = false)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.CorridorMove"))
		{
			resultPos = desiredPos;
			firstPolyRef = 0uL;
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return false;
			}
			if (!RecastWrapper.CorridorMove(NavMeshHandle, corridor, in desiredPos.Value, out var resultPos2, out firstPolyRef, allowNpcDoors ? 1 : 0))
			{
				return false;
			}
			resultPos = new NavVector3(resultPos2);
			return true;
		}
	}

	public bool CorridorOptimizeAndMove(IntPtr corridor, NavVector3 optimizeNextNS, float optimizationRange, NavVector3 desiredPosNS, out NavVector3 resultPosNS, bool allowNpcDoors = false)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.CorridorOptimizeAndMove"))
		{
			resultPosNS = desiredPosNS;
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return false;
			}
			if (!RecastWrapper.CorridorOptimizeAndMove(NavMeshHandle, corridor, in optimizeNextNS.Value, optimizationRange, in desiredPosNS.Value, out var resultPos, out var _, allowNpcDoors ? 1 : 0))
			{
				return false;
			}
			resultPosNS = new NavVector3(resultPos);
			return true;
		}
	}

	public bool CorridorMoveTargetPosition(IntPtr corridor, NavVector3 desiredTargetNS, out NavVector3 resultTargetNS, bool allowNpcDoors = false)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.CorridorMoveTargetPosition"))
		{
			resultTargetNS = desiredTargetNS;
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return false;
			}
			if (!RecastWrapper.CorridorMoveTargetPosition(NavMeshHandle, corridor, in desiredTargetNS.Value, out var resultTarget, allowNpcDoors ? 1 : 0))
			{
				return false;
			}
			resultTargetNS = new NavVector3(resultTarget);
			return true;
		}
	}

	public int CorridorFindCorners(IntPtr corridor, List<NavVector3> corners, int maxCorners, out bool endReached)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.CorridorFindCorners"))
		{
			endReached = false;
			corners.Clear();
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return 0;
			}
			int num = RecastWrapper.CorridorFindCorners(NavMeshHandle, corridor, CornerBuffer, maxCorners, out endReached);
			for (int i = 0; i < num; i++)
			{
				corners.Add(new NavVector3(CornerBuffer[i]));
			}
			return num;
		}
	}

	public bool CorridorIsValid(IntPtr corridor, int maxLookAhead, bool allowNpcDoors = false)
	{
		uint currentStamp;
		return CorridorIsValid(corridor, maxLookAhead, allowNpcDoors, 0u, out currentStamp);
	}

	public bool CorridorIsValid(IntPtr corridor, int maxLookAhead, bool allowNpcDoors, uint lastCheckedStamp, out uint currentStamp)
	{
		using (TimeWarning.New("RustNavmesh.CorridorIsValid"))
		{
			currentStamp = lastCheckedStamp;
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return false;
			}
			return RecastWrapper.CorridorIsValid(NavMeshHandle, corridor, maxLookAhead, allowNpcDoors ? 1 : 0, lastCheckedStamp, out currentStamp);
		}
	}

	public void CorridorOptimizeVisibility(IntPtr corridor, NavVector3 next, float optimizationRange, bool allowNpcDoors = false)
	{
		using (TimeWarning.New("RustNavmesh.CorridorOptimizeVisibility"))
		{
			if (RustNavigation.EnsureNewNavmesh() && IsValid())
			{
				RecastWrapper.CorridorOptimizeVisibility(NavMeshHandle, corridor, in next.Value, optimizationRange, allowNpcDoors ? 1 : 0);
			}
		}
	}

	public bool FindDistanceToWall(ref ulong startRef, NavVector3 centerPos, float maxRadius, out NavHit hit, bool allowNpcDoors = false)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.FindDistanceToWall"))
		{
			hit = default;
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			if (!IsValid())
			{
				if (AI.logIssues)
				{
					RustNavigation.LogError("NavMesh has not been built yet.");
				}
				return false;
			}
			if (!RecastWrapper.FindDistanceToWall(NavMeshHandle, ref startRef, in centerPos.Value, maxRadius, out var hitDistance, out var hitLocation, out var hitNormal, allowNpcDoors ? 1 : 0))
			{
				return false;
			}
			hit = new NavHit
			{
				position = new NavVector3(hitLocation),
				normal = new NavVector3(hitNormal),
				distance = hitDistance,
				hit = true
			};
			return true;
		}
	}

	public bool FindDonutPointsInCircle(ref ulong startRef, NavVector3 centerNS, float maxRadius, float minRadius, float angleOffset, int count, List<NavVector3> resultsNS, bool allowNpcDoors = false)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.FindDonutPointsInCircle"))
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			if (!IsValid())
			{
				if (AI.logIssues)
				{
					RustNavigation.LogError("NavMesh has not been built yet.");
				}
				return false;
			}
			count = Mathf.Min(count, 64);
			if (!RecastWrapper.FindDonutPointsInCircle(NavMeshHandle, ref startRef, in centerNS.Value, maxRadius, minRadius, angleOffset, count, DonutPointsBuffer, out var numFound, allowNpcDoors ? 1 : 0))
			{
				return false;
			}
			for (int i = 0; i < numFound; i++)
			{
				resultsNS.Add(new NavVector3(DonutPointsBuffer[i]));
			}
			return numFound > 0;
		}
	}

	public bool Save(string path)
	{
		JoinSaveAndLandParkedTilesOnMainThread();
		if (!BeginSave(path))
		{
			return false;
		}
		JoinSaveOnMainThread();
		return LastSaveStats.succeeded;
	}

	public bool BeginSave(string path)
	{
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.BeginSave"))
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			if (IsSaveInFlight)
			{
				if (followUpPath != null && followUpPath != path)
				{
					RustNavigation.LogWarning("A navmesh save to " + followUpPath + " was queued behind the one in flight and is replaced by one to " + path + ", only the latter will run");
				}
				followUpPath = path;
				return false;
			}
			long timestamp = Stopwatch.GetTimestamp();
			if (!IsValid())
			{
				RustNavigation.Log("Navmesh not built, nothing to save.");
				return false;
			}
			RustNavDoorGates.FlushSaveDeferredDoors();
			PooledList<(int, int)> val = Pool.Get<PooledList<(int, int)>>();
			try
			{
				tileBuilder.GetPendingTilesForNavmeshOnMainThread(this, (List<(int tx, int ty)>)(object)val);
				string tempPath = path + ".new";
				string text = (ShouldAppendDelta(path) ? path : null);
				IntPtr intPtr = BuildSavePayload(val, out var payloadSize);
				IntPtr intPtr2 = IntPtr.Zero;
				int count = 0;
				if (text != null)
				{
					try
					{
						intPtr2 = BuildDirtyCoords(out count);
					}
					catch
					{
						Marshal.FreeHGlobal(intPtr);
						throw;
					}
				}
				ConsumeSaveWindow(out var dirtyTiles, out var dataVersionDelta);
				if (!saveJob.TryBegin(path, tempPath, NavMeshHandle, in BuildParams, CurrentNavmeshBounds.min, CurrentNavmeshBounds.max, intPtr, payloadSize, ResolveSaveFlags(), ResolveSaveThreadCount(), DebugSaveDelayMs, intPtr2, count, text, deltaBaseBytes + deltaAppendedBytes))
				{
					Marshal.FreeHGlobal(intPtr);
					if (intPtr2 != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr2);
					}
					deltaBasePath = null;
					LastSaveStats = new NavmeshSaveStats
					{
						valid = true,
						failure = "the save thread could not be started",
						mainMs = BakeStats.TicksToMs(Stopwatch.GetTimestamp() - timestamp),
						pendingTiles = ((List<(int, int)>)(object)val).Count,
						dirtyTiles = dirtyTiles,
						dataVersionDelta = dataVersionDelta,
						deltaAppendedBytes = deltaAppendedBytes,
						deltaBaseBytes = deltaBaseBytes,
						deltaBudget = RustNav.saveDeltaBudget
					};
					RustNavigation.LogError("Could not start the navmesh save thread for " + path + ", this save is skipped and the next one rewrites the whole file");
					return false;
				}
				IsSaveInFlight = true;
				if (followUpPath == path)
				{
					followUpPath = null;
				}
				inFlightPath = path;
				inFlightStats = new NavmeshSaveStats
				{
					mainMs = BakeStats.TicksToMs(Stopwatch.GetTimestamp() - timestamp),
					pendingTiles = ((List<(int, int)>)(object)val).Count,
					dirtyTiles = dirtyTiles,
					dataVersionDelta = dataVersionDelta,
					deltaBudget = RustNav.saveDeltaBudget
				};
				return true;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public void PollSaveOnMainThread()
	{
		if (!IsSaveInFlight)
		{
			StartFollowUpIfDue();
		}
		else
		{
			ReapFinishedSaveOnMainThread();
		}
	}

	private void ReapFinishedSaveOnMainThread()
	{
		if (IsSaveInFlight && saveJob.TryReapIfFinished(out var result))
		{
			CompleteSaveOnMainThread(in result);
		}
	}

	private void StartFollowUpIfDue()
	{
		if (followUpPath != null && (tileBuilder == null || !tileBuilder.SaveGateCatchUp.inProgress) && !RustNavDoorGates.HasSaveDeferredDoors)
		{
			string path = followUpPath;
			followUpPath = null;
			BeginSave(path);
		}
	}

	public void JoinSaveOnMainThread()
	{
		if (!IsSaveInFlight)
		{
			return;
		}
		using (TimeWarning.New("RustNavmesh.JoinSave"))
		{
			CompleteSaveOnMainThread(saveJob.Join());
		}
	}

	public void JoinSaveAndLandParkedTilesOnMainThread()
	{
		JoinSaveOnMainThread();
		if (tileBuilder != null)
		{
			tileBuilder.LandParkedResultsOnMainThread();
		}
	}

	public void FlushSavesOnMainThread()
	{
		JoinSaveAndLandParkedTilesOnMainThread();
		if (followUpPath != null)
		{
			string path = followUpPath;
			followUpPath = null;
			Save(path);
		}
	}

	private void CompleteSaveOnMainThread(in NavmeshSaveThreadResult result)
	{
		IsSaveInFlight = false;
		NavmeshSaveStats stats = result.stats;
		stats.valid = true;
		stats.mainMs = inFlightStats.mainMs;
		stats.pendingTiles = inFlightStats.pendingTiles;
		stats.dirtyTiles = inFlightStats.dirtyTiles;
		stats.dataVersionDelta = inFlightStats.dataVersionDelta;
		stats.deltaBudget = inFlightStats.deltaBudget;
		if (result.warning != null)
		{
			RustNavigation.LogWarning("Navmesh save to " + inFlightPath + ": " + result.warning);
		}
		if (!stats.succeeded)
		{
			deltaBasePath = null;
		}
		else if (stats.wroteDelta)
		{
			deltaAppendedBytes = stats.bytes - deltaBaseBytes;
		}
		else
		{
			deltaBasePath = inFlightPath;
			deltaBaseBytes = stats.bytes;
			deltaAppendedBytes = 0L;
		}
		stats.deltaAppendedBytes = deltaAppendedBytes;
		stats.deltaBaseBytes = deltaBaseBytes;
		LastSaveStats = stats;
		if (stats.succeeded)
		{
			LogSaveCompleted(stats);
		}
		else
		{
			RustNavigation.LogError("Failed to save navmesh to " + inFlightPath + ": " + stats.failure);
		}
		inFlightPath = null;
		if (tileBuilder != null)
		{
			tileBuilder.OnNavmeshSaveGateLifted();
		}
	}

	private void EnsureNoSaveInFlight(string caller)
	{
		if (IsSaveInFlight)
		{
			RustNavigation.LogError(caller + " wrote the navmesh while a save was in flight. That path has to park or defer instead, joining the save.");
			JoinSaveOnMainThread();
		}
	}

	public void JoinSaveForSynchronousMutation(string caller)
	{
		if (IsSaveInFlight)
		{
			RustNavigation.LogWarning(caller + " needs the navmesh now, waiting for the save in flight to finish");
			JoinSaveOnMainThread();
		}
	}

	private void ConsumeSaveWindow(out int dirtyTiles, out int dataVersionDelta)
	{
		dirtyTiles = dirtyTilesSinceSave.Count;
		dataVersionDelta = TileDataVersion - dataVersionAtLastSave;
		dataVersionAtLastSave = TileDataVersion;
		ClearDirtyTilesSinceSave();
	}

	private bool ShouldAppendDelta(string path)
	{
		if (!RustNav.saveDelta || !RecastWrapper.HasAppendNavMeshDelta)
		{
			return false;
		}
		if (deltaBasePath != path)
		{
			return false;
		}
		if (RustNav.saveDeltaBudget > 0f && (float)deltaAppendedBytes > (float)deltaBaseBytes * RustNav.saveDeltaBudget)
		{
			return false;
		}
		if (RustNav.saveDeltaBudget > 0f && (float)dirtyTilesSinceSave.Count > (float)tilesWithData * RustNav.saveDeltaBudget)
		{
			return false;
		}
		return true;
	}

	private IntPtr BuildDirtyCoords(out int count)
	{
		count = dirtyTilesSinceSave.Count;
		if (count == 0)
		{
			return IntPtr.Zero;
		}
		IntPtr intPtr = Marshal.AllocHGlobal(count * 4 * 2);
		for (int i = 0; i < count; i++)
		{
			Marshal.WriteInt32(intPtr, i * 4 * 2, dirtyTilesSinceSave[i].tx);
			Marshal.WriteInt32(intPtr, i * 4 * 2 + 4, dirtyTilesSinceSave[i].ty);
		}
		return intPtr;
	}

	private unsafe IntPtr BuildSavePayload(PooledList<(int tx, int ty)> pendingTiles, out int payloadSize)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		payloadSize = System.Runtime.CompilerServices.Unsafe.SizeOf<ManagedNavPayload>() + ((List<(int, int)>)(object)pendingTiles).Count * 4 * 2;
		IntPtr intPtr = Marshal.AllocHGlobal(payloadSize);
		ManagedNavPayload managedNavPayload = new ManagedNavPayload
		{
			payloadVersion = 5,
			buildParams = BuildParams,
			buildParamsHiRes = BuildParamsHiRes,
			currentNavmeshBounds = CurrentNavmeshBounds,
			tileNum = tileNum,
			pendingTileCount = ((List<(int, int)>)(object)pendingTiles).Count
		};
		System.Runtime.CompilerServices.Unsafe.Write((void*)intPtr, managedNavPayload);
		int* ptr = (int*)((byte*)(void*)intPtr + System.Runtime.CompilerServices.Unsafe.SizeOf<ManagedNavPayload>());
		foreach (var (num, num2) in (List<(int, int)>)(object)pendingTiles)
		{
			*(ptr++) = num;
			*(ptr++) = num2;
		}
		return intPtr;
	}

	private static int ResolveSaveFlags()
	{
		int num = 2;
		if (RustNav.saveCompression)
		{
			num |= 1;
		}
		return num;
	}

	private static int ResolveSaveThreadCount()
	{
		int saveThreads = RustNav.saveThreads;
		int num = ((saveThreads > 0) ? Mathf.Min(saveThreads, 32) : Mathf.Clamp(Environment.ProcessorCount / 2, 1, 4));
		if (!loggedSaveProcessorCounts)
		{
			loggedSaveProcessorCounts = true;
			RustNavigation.Log(string.Format("Navmesh save and load threads: {0} (Environment.ProcessorCount {1}, SystemInfo.processorCount {2}, rustnav.savethreads {3})", new object[4]
			{
				num,
				Environment.ProcessorCount,
				SystemInfo.processorCount,
				saveThreads
			}));
		}
		return num;
	}

	internal static string DeleteStaleTempFile(string tempPath)
	{
		try
		{
			if (File.Exists(tempPath))
			{
				File.Delete(tempPath);
			}
			return null;
		}
		catch (Exception ex)
		{
			return "Could not remove the stale navmesh temp file " + tempPath + " (" + ex.GetType().Name + ": " + ex.Message + ").";
		}
	}

	internal static bool MoveSavedFileIntoPlace(string tempPath, string finalPath, out NavmeshSaveReplaceMode mode, out string note)
	{
		mode = NavmeshSaveReplaceMode.None;
		note = null;
		for (int i = 1; i <= 3; i++)
		{
			try
			{
				if (File.Exists(finalPath))
				{
					File.Replace(tempPath, finalPath, null);
					mode = NavmeshSaveReplaceMode.Replace;
				}
				else
				{
					File.Move(tempPath, finalPath);
					mode = NavmeshSaveReplaceMode.Move;
				}
				return true;
			}
			catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) && i < 3)
			{
				note = string.Format("Renaming {0} onto {1} failed on attempt {2} ({3}: {4}), retrying.", new object[5]
				{
					tempPath,
					finalPath,
					i,
					ex.GetType().Name,
					ex.Message
				});
				Thread.Sleep(100);
			}
			catch (Exception ex2)
			{
				note = "Renaming " + tempPath + " onto " + finalPath + " failed (" + ex2.GetType().Name + ": " + ex2.Message + "), fell back to a copy.";
				break;
			}
		}
		try
		{
			File.Copy(tempPath, finalPath, overwrite: true);
		}
		catch (Exception arg)
		{
			note = $"Copying {tempPath} onto {finalPath} failed and may have left it torn, the temp file is kept: {arg}";
			return false;
		}
		mode = NavmeshSaveReplaceMode.Copy;
		try
		{
			File.Delete(tempPath);
		}
		catch (Exception ex3)
		{
			note = note + " The temp file could not be removed and goes before the next save (" + ex3.Message + ").";
		}
		return true;
	}

	private void ClearDirtyTilesSinceSave()
	{
		for (int i = 0; i < dirtyTilesSinceSave.Count; i++)
		{
			dirtyTilesSinceSave[i].dirtySinceSave = false;
		}
		dirtyTilesSinceSave.Clear();
	}

	internal static bool CreateMissingDirectory(string filePath, out string note)
	{
		note = null;
		try
		{
			string directoryName = Path.GetDirectoryName(filePath);
			if (string.IsNullOrEmpty(directoryName) || Directory.Exists(directoryName))
			{
				return false;
			}
			Directory.CreateDirectory(directoryName);
			note = "The navmesh save folder " + directoryName + " was missing and has been created.";
			return true;
		}
		catch (Exception ex)
		{
			note = "Could not create the navmesh save folder for " + filePath + " (" + ex.GetType().Name + ": " + ex.Message + ").";
			return false;
		}
	}

	internal static long FileLengthOrZero(string path)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(path);
			return fileInfo.Exists ? fileInfo.Length : 0;
		}
		catch (Exception)
		{
			return 0L;
		}
	}

	private static void LogSaveCompleted(NavmeshSaveStats stats)
	{
		double num = stats.mainMs + stats.threadMs + stats.replaceMs;
		string message = CompletionLine(in stats, num);
		if (RustNav.saveWarnMs > 0f && num > (double)RustNav.saveWarnMs)
		{
			DebugEx.LogWarning(message, (StackTraceLogType)0);
		}
		else
		{
			DebugEx.Log(message, (StackTraceLogType)0);
		}
	}

	private static string CompletionLine(in NavmeshSaveStats stats, double total)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Utf16ValueStringBuilder sb = ZString.CreateStringBuilder();
		try
		{
			sb.Append("[RustNav] ");
			sb.Append("Successfully saved navmesh (");
			sb.Append(stats.pendingTiles);
			sb.Append(" pending tiles) in ");
			sb.Append(total, "F1");
			sb.Append(" ms, ");
			stats.AppendDescription(ref sb);
			return ((object)sb/*cast due to constrained. prefix*/).ToString();
		}
		finally
		{
			sb.Dispose();
		}
	}

	public unsafe static RustNavmesh Load(string path, BackgroundTileBuilder tileBuilder, bool synchronous = false, bool cullTilesFarFromShore = false)
	{
		using (TimeWarning.New("RustNavmesh.Load"))
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return null;
			}
			long timestamp = Stopwatch.GetTimestamp();
			IntPtr intPtr = RecastWrapper.LoadNavMesh(path, out var managedBlob, out var managedBlobSize, ResolveSaveThreadCount());
			if (intPtr == IntPtr.Zero)
			{
				return null;
			}
			RustNavmesh rustNavmesh = null;
			try
			{
				if (managedBlob == IntPtr.Zero || managedBlobSize < System.Runtime.CompilerServices.Unsafe.SizeOf<ManagedNavPayload>())
				{
					RustNavigation.LogError($"Navmesh file has an invalid managed payload ({managedBlobSize} bytes)");
					return null;
				}
				ManagedNavPayload payload = System.Runtime.CompilerServices.Unsafe.Read<ManagedNavPayload>((void*)managedBlob);
				if (payload.payloadVersion != 5)
				{
					RustNavigation.LogWarning($"Saved navmesh is payload version {payload.payloadVersion}, this build wants {5}. Rebuilding from scratch.");
					return null;
				}
				if (payload.pendingTileCount < 0 || managedBlobSize != System.Runtime.CompilerServices.Unsafe.SizeOf<ManagedNavPayload>() + (long)payload.pendingTileCount * 4L * 2)
				{
					RustNavigation.LogError($"Managed payload size mismatch ({managedBlobSize} bytes for {payload.pendingTileCount} pending tiles)");
					return null;
				}
				if (payload.tileNum.x <= 0 || payload.tileNum.y <= 0)
				{
					RustNavigation.LogError($"Invalid tile dimensions: {payload.tileNum.x}x{payload.tileNum.y}");
					return null;
				}
				rustNavmesh = new RustNavmesh(tileBuilder, intPtr, in payload, cullTilesFarFromShore);
				if (!rustNavmesh.IsValid())
				{
					rustNavmesh = null;
					return null;
				}
				intPtr = IntPtr.Zero;
				int navMeshTileCoords = RecastWrapper.GetNavMeshTileCoords(rustNavmesh.NavMeshHandle, IntPtr.Zero, 0);
				if (navMeshTileCoords > 0)
				{
					IntPtr intPtr2 = Marshal.AllocHGlobal(navMeshTileCoords * 4 * 2);
					try
					{
						RecastWrapper.GetNavMeshTileCoords(rustNavmesh.NavMeshHandle, intPtr2, navMeshTileCoords);
						int* ptr = (int*)(void*)intPtr2;
						for (int i = 0; i < navMeshTileCoords; i++)
						{
							int num = *(ptr++);
							int num2 = *(ptr++);
							Tile tile = rustNavmesh.GetTile(num, num2);
							if (tile == null)
							{
								RustNavigation.LogError($"Loaded tile {num},{num2} is outside the tile grid");
								return null;
							}
							tile.hasData = true;
							rustNavmesh.tilesWithData++;
							rustNavmesh.MarkTileAsBuilt(tile, dataChanged: false);
						}
					}
					finally
					{
						Marshal.FreeHGlobal(intPtr2);
					}
				}
				int* ptr2 = (int*)((byte*)(void*)managedBlob + System.Runtime.CompilerServices.Unsafe.SizeOf<ManagedNavPayload>());
				for (int j = 0; j < payload.pendingTileCount; j++)
				{
					int num3 = *(ptr2++);
					int num4 = *(ptr2++);
					if (num3 < 0 || num4 < 0 || num3 >= payload.tileNum.x || num4 >= payload.tileNum.y)
					{
						RustNavigation.LogError(string.Format("Invalid pending tile coordinates: {0},{1} (max: {2},{3})", new object[4]
						{
							num3,
							num4,
							payload.tileNum.x - 1,
							payload.tileNum.y - 1
						}));
						return null;
					}
					tileBuilder.EnqueueOnMainThread(rustNavmesh, num3, num4, synchronous);
				}
				PooledList<(int, int)> val = Pool.Get<PooledList<(int, int)>>();
				try
				{
					tileBuilder.GetPendingTilesForNavmeshOnMainThread(rustNavmesh, (List<(int tx, int ty)>)(object)val);
					PooledHashSet<(int, int)> val2 = Pool.Get<PooledHashSet<(int, int)>>();
					try
					{
						foreach (var item in (List<(int, int)>)(object)val)
						{
							((HashSet<(int, int)>)(object)val2).Add(item);
						}
						for (int k = 0; k < payload.tileNum.y; k++)
						{
							for (int l = 0; l < payload.tileNum.x; l++)
							{
								Tile tile2 = rustNavmesh.GetTile(l, k);
								if (rustNavmesh.IsTileFarFromShore(l, k))
								{
									rustNavmesh.FailTile(l, k);
								}
								else if ((tile2 == null || !tile2.hasData) && !((HashSet<(int, int)>)(object)val2).Contains((l, k)))
								{
									rustNavmesh.FailTile(l, k);
								}
							}
						}
						double num5 = (double)(Stopwatch.GetTimestamp() - timestamp) * 1000.0 / (double)Stopwatch.Frequency;
						RustNavigation.Log($"Successfully loaded navmesh with {navMeshTileCoords} tiles in {num5} ms");
						RustNavmesh result = rustNavmesh;
						rustNavmesh = null;
						return result;
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			catch (Exception ex)
			{
				RustNavigation.LogError("Failed to load navmesh: " + ex.Message);
				return null;
			}
			finally
			{
				rustNavmesh?.Dispose();
				if (intPtr != IntPtr.Zero)
				{
					RecastWrapper.DestroyNavMesh(intPtr);
				}
				if (managedBlob != IntPtr.Zero)
				{
					RecastWrapper.FreeManagedBlob(managedBlob);
				}
			}
		}
	}

	public bool FillDebugDrawProto(NavMeshData navMeshData, Bounds bounds, Matrix4x4? transform = null, Vector3? sectionPivot = null, Vector3 sectionSign = default(Vector3))
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		if (!RustNavigation.EnsureNewNavmesh())
		{
			return false;
		}
		if (!IsValid())
		{
			return false;
		}
		PooledList<Vector2Int> val = Pool.Get<PooledList<Vector2Int>>();
		try
		{
			GetTilesInBounds(bounds, (List<Vector2Int>)(object)val);
			PooledList<Vector3> val2 = Pool.Get<PooledList<Vector3>>();
			try
			{
				foreach (Vector2Int item in (List<Vector2Int>)(object)val)
				{
					Vector2Int current = item;
					PooledList<Vector3> val3 = Pool.Get<PooledList<Vector3>>();
					try
					{
						GetTilePolysInternal(current.x, current.y, (List<Vector3>)(object)val3);
						if (!sectionPivot.HasValue)
						{
							((List<Vector3>)(object)val2).AddRange((IEnumerable<Vector3>)val3);
							continue;
						}
						Vector3 value = sectionPivot.Value;
						for (int i = 0; i < ((List<Vector3>)(object)val3).Count; i += 6)
						{
							Vector3 val4 = Vector3.zero;
							int num = 0;
							for (int j = 0; j < 6; j++)
							{
								Vector3 val5 = ((List<Vector3>)(object)val3)[i + j];
								if (val5 == Vector3.zero)
								{
									break;
								}
								val4 += val5;
								num++;
							}
							if (num == 0)
							{
								continue;
							}
							Vector3 val6 = val4 / (float)num;
							float num2 = ((val6.x >= value.x) ? 1f : (-1f));
							float num3 = ((val6.z >= value.z) ? 1f : (-1f));
							if (num2 == sectionSign.x && num3 == sectionSign.z)
							{
								for (int k = 0; k < 6; k++)
								{
									((List<Vector3>)(object)val2).Add(((List<Vector3>)(object)val3)[i + k]);
								}
							}
						}
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
				for (int l = 0; l < ((List<Vector3>)(object)val2).Count; l += 6)
				{
					VectorList val7 = Pool.Get<VectorList>();
					val7.vectorPoints = Pool.Get<List<Vector3>>();
					for (int m = 0; m < 6; m++)
					{
						Vector3 val8 = ((List<Vector3>)(object)val2)[l + m];
						if (val8 == Vector3.zero)
						{
							break;
						}
						if (transform.HasValue)
						{
							Matrix4x4 value2 = transform.Value;
							val8 = value2.MultiplyPoint3x4(val8);
						}
						val7.vectorPoints.Add(val8);
					}
					navMeshData.polygons.Add(val7);
				}
				return true;
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool FillDebugDrawProtoForTile(NavMeshData navMeshData, int tx, int ty, Matrix4x4? transform = null, NavMeshData doorOpenData = null, NavMeshData doorClosedData = null, NavMeshData doorNpcData = null)
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.FillDebugDrawProtoForTile"))
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			if (!IsValid())
			{
				return false;
			}
			bool flag = doorOpenData != null && doorClosedData != null;
			PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
			try
			{
				PooledList<byte> val2 = Pool.Get<PooledList<byte>>();
				try
				{
					PooledList<ushort> val3 = Pool.Get<PooledList<ushort>>();
					try
					{
						GetTilePolysWithStateInternal(tx, ty, (List<Vector3>)(object)val, (List<byte>)(object)(flag ? val2 : null), (List<ushort>)(object)(flag ? val3 : null));
						int num = 0;
						int num2 = 0;
						while (num < ((List<Vector3>)(object)val).Count)
						{
							NavMeshData val4 = navMeshData;
							if (flag && ((List<byte>)(object)val2)[num2] == 3)
							{
								if ((((List<ushort>)(object)val3)[num2] & 1) != 0)
								{
									val4 = doorOpenData;
								}
								else
								{
									val4 = ((doorNpcData == null || (((List<ushort>)(object)val3)[num2] & 0x20) == 0) ? doorClosedData : doorNpcData);
								}
							}
							else if (flag && ((List<byte>)(object)val2)[num2] == 6 && (((List<ushort>)(object)val3)[num2] & 1) == 0)
							{
								val4 = doorClosedData;
							}
							VectorList val5 = Pool.Get<VectorList>();
							val5.vectorPoints = Pool.Get<List<Vector3>>();
							for (int i = 0; i < 6; i++)
							{
								Vector3 val6 = ((List<Vector3>)(object)val)[num + i];
								if (val6 == Vector3.zero)
								{
									break;
								}
								if (transform.HasValue)
								{
									Matrix4x4 value = transform.Value;
									val6 = value.MultiplyPoint3x4(val6);
								}
								val5.vectorPoints.Add(val6);
							}
							val4.polygons.Add(val5);
							num += 6;
							num2++;
						}
						return true;
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public bool GetTilePolysWithStateInternal(int tx, int ty, List<Vector3> polys, List<byte> areas, List<ushort> flags)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.GetTilePolysWithStateInternal"))
		{
			if (!IsValid())
			{
				return false;
			}
			if (!RecastWrapper.GetTilePolysEx(NavMeshHandle, tx, ty, TilePolysBuffer, TileAreasBuffer, TileFlagsBuffer, 2048, out var outPolyCount))
			{
				return false;
			}
			for (int i = 0; i < outPolyCount * 6; i++)
			{
				polys.Add(TilePolysBuffer[i]);
			}
			if (areas != null && flags != null)
			{
				for (int j = 0; j < outPolyCount; j++)
				{
					areas.Add(TileAreasBuffer[j]);
					flags.Add(TileFlagsBuffer[j]);
				}
			}
			return true;
		}
	}

	public void Dispose()
	{
		RustNavigation.Log("Disposing navmesh...");
		followUpPath = null;
		JoinSaveOnMainThread();
		tileBuilder.CancelPendingTilesForOnMainThread(this);
		if (NavMeshHandle != IntPtr.Zero)
		{
			RecastWrapper.DestroyNavMesh(NavMeshHandle);
			NavMeshHandle = IntPtr.Zero;
		}
		tiles = null;
	}
}
