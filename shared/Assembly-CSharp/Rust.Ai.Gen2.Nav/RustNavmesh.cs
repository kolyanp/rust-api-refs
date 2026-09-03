using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ConVar;
using Facepunch;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2.Nav;

public class RustNavmesh : IDisposable
{
	private static Vector3[] TilePolysBuffer = (Vector3[])(object)new Vector3[12288];

	private static Vector3[] PathBuffer = (Vector3[])(object)new Vector3[256];

	private static Vector3[] CornerBuffer = (Vector3[])(object)new Vector3[256];

	private static Vector3[] DonutPointsBuffer = (Vector3[])(object)new Vector3[64];

	public NavMeshBuildParams BuildParams;

	public NavMeshBuildParams BuildParamsHiRes;

	public int PathfindingMaxIterations;

	public Bounds CurrentNavmeshBounds;

	public Tile[] tiles;

	public IntPtr NavMeshHandle;

	public string debugName;

	public long workerBuildTicks;

	public double lastFullBuildSeconds;

	private float cachedMaxBorderMeters;

	private double builtStartTime;

	private int numBuiltTiles;

	private Vector2Int tileNum;

	private BackgroundTileBuilder tileBuilder;

	public bool EmitTileChangeEvents;

	public bool ForceHiRes;

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

	public bool CullTilesFarFromShore { get; private set; }

	public int TileChangeVersion { get; private set; }

	public bool IsValid()
	{
		return NavMeshHandle != IntPtr.Zero;
	}

	public RustNavmesh(BackgroundTileBuilder tileBuilder, NavMeshBuildParams? buildParamsOverride = null, NavMeshBuildParams? buildParamsHiResOverride = null, Bounds? boundsOverride = null, bool shouldBuild = true, bool synchronous = false, bool forceHiRes = false, bool cullTilesFarFromShore = false)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		BuildParams = new NavMeshBuildParams(true);
		BuildParamsHiRes = new NavMeshBuildParams(true);
		PathfindingMaxIterations = 1000;
		NavMeshHandle = IntPtr.Zero;
		debugName = "unnamed";
		lastFullBuildSeconds = -1.0;
		base._002Ector();
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
		NavMeshHandle = RecastWrapper.CreateEmptyNavMesh(in BuildParams, ((Bounds)(ref CurrentNavmeshBounds)).min, ((Bounds)(ref CurrentNavmeshBounds)).max);
		if (NavMeshHandle == IntPtr.Zero)
		{
			RustNavigation.LogError("Failed to create empty navmesh");
			Dispose();
			return;
		}
		tileNum = rcCalcTileNum();
		tiles = new Tile[((Vector2Int)(ref tileNum)).x * ((Vector2Int)(ref tileNum)).y];
		for (int i = 0; i < ((Vector2Int)(ref tileNum)).y; i++)
		{
			for (int j = 0; j < ((Vector2Int)(ref tileNum)).x; j++)
			{
				Tile tile = new Tile(j, i);
				tiles[Mathx.FlattenArrayCoord(j, i, ((Vector2Int)(ref tileNum)).x)] = tile;
			}
		}
		if (!shouldBuild)
		{
			return;
		}
		builtStartTime = Time.realtimeSinceStartupAsDouble;
		int num3 = 0;
		for (int k = 0; k < ((Vector2Int)(ref tileNum)).y; k++)
		{
			for (int l = 0; l < ((Vector2Int)(ref tileNum)).x; l++)
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
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		BuildParams = new NavMeshBuildParams(true);
		BuildParamsHiRes = new NavMeshBuildParams(true);
		PathfindingMaxIterations = 1000;
		NavMeshHandle = IntPtr.Zero;
		debugName = "unnamed";
		lastFullBuildSeconds = -1.0;
		base._002Ector();
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
		tiles = new Tile[((Vector2Int)(ref tileNum)).x * ((Vector2Int)(ref tileNum)).y];
		for (int i = 0; i < ((Vector2Int)(ref tileNum)).y; i++)
		{
			for (int j = 0; j < ((Vector2Int)(ref tileNum)).x; j++)
			{
				tiles[Mathx.FlattenArrayCoord(j, i, ((Vector2Int)(ref tileNum)).x)] = new Tile(j, i);
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

	private void MarkTileAsBuilt(Tile tile)
	{
		if (tile == null)
		{
			return;
		}
		TileChangeVersion++;
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
		if (tile.hasData && NavMeshHandle != IntPtr.Zero)
		{
			RecastWrapper.RemoveTileFromNavMesh(NavMeshHandle, tx, ty);
		}
		tile.hasData = false;
		MarkTileAsBuilt(tile);
	}

	public bool AddTile(int tx, int ty, IntPtr tileData, int dataSize)
	{
		if (tiles == null)
		{
			return false;
		}
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
		tile.hasData = true;
		MarkTileAsBuilt(tile);
		return true;
	}

	public Tile GetTile(int tx, int ty)
	{
		if (tx < 0 || ty < 0 || tx >= ((Vector2Int)(ref tileNum)).x || ty >= ((Vector2Int)(ref tileNum)).y)
		{
			return null;
		}
		return tiles[Mathx.FlattenArrayCoord(tx, ty, ((Vector2Int)(ref tileNum)).x)];
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
		Vector2Int val = rcCalcTileCoordFromPos(((Bounds)(ref bounds)).min);
		Vector2Int val2 = rcCalcTileCoordFromPos(((Bounds)(ref bounds)).max);
		for (int i = ((Vector2Int)(ref val)).x; i <= ((Vector2Int)(ref val2)).x; i++)
		{
			for (int j = ((Vector2Int)(ref val)).y; j <= ((Vector2Int)(ref val2)).y; j++)
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
			if (!((Bounds)(ref CurrentNavmeshBounds)).Intersects(rebuildBounds))
			{
				return;
			}
			Vector2Int val = rcCalcTileCoordFromPos(((Bounds)(ref rebuildBounds)).min);
			Vector2Int val2 = rcCalcTileCoordFromPos(((Bounds)(ref rebuildBounds)).max);
			for (int i = ((Vector2Int)(ref val)).x; i <= ((Vector2Int)(ref val2)).x; i++)
			{
				for (int j = ((Vector2Int)(ref val)).y; j <= ((Vector2Int)(ref val2)).y; j++)
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
		int num2 = Mathf.FloorToInt((pos.x - ((Bounds)(ref CurrentNavmeshBounds)).min.x) / num);
		int num3 = Mathf.FloorToInt((pos.z - ((Bounds)(ref CurrentNavmeshBounds)).min.z) / num);
		int num4 = Mathf.Clamp(num2, 0, ((Vector2Int)(ref tileNum)).x - 1);
		num3 = Mathf.Clamp(num3, 0, ((Vector2Int)(ref tileNum)).y - 1);
		return new Vector2Int(num4, num3);
	}

	private Vector2Int rcCalcTileNum()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)((((Bounds)(ref CurrentNavmeshBounds)).max.x - ((Bounds)(ref CurrentNavmeshBounds)).min.x) / BuildParams.cellSize + 0.5f);
		int num2 = (int)((((Bounds)(ref CurrentNavmeshBounds)).max.z - ((Bounds)(ref CurrentNavmeshBounds)).min.z) / BuildParams.cellSize + 0.5f);
		int num3 = (int)(((float)num + BuildParams.tileSize - 1f) / BuildParams.tileSize);
		int num4 = (int)(((float)num2 + BuildParams.tileSize - 1f) / BuildParams.tileSize);
		return new Vector2Int(num3, num4);
	}

	public Bounds rcCalcTileBounds(Vector2Int tileCoord)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		float num = BuildParams.tileSize * BuildParams.cellSize;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(((Bounds)(ref CurrentNavmeshBounds)).min.x + (float)((Vector2Int)(ref tileCoord)).x * num, ((Bounds)(ref CurrentNavmeshBounds)).min.y, ((Bounds)(ref CurrentNavmeshBounds)).min.z + (float)((Vector2Int)(ref tileCoord)).y * num);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(((Bounds)(ref CurrentNavmeshBounds)).min.x + (float)(((Vector2Int)(ref tileCoord)).x + 1) * num, ((Bounds)(ref CurrentNavmeshBounds)).max.y, ((Bounds)(ref CurrentNavmeshBounds)).min.z + (float)(((Vector2Int)(ref tileCoord)).y + 1) * num);
		return new Bounds((val + val2) * 0.5f, val2 - val);
	}

	private static float BorderMeters(in NavMeshBuildParams p)
	{
		return (float)(Mathf.CeilToInt(p.agentRadius / p.cellSize) + 3) * p.cellSize;
	}

	public Bounds rcExpandTileBounds(Bounds tileBounds)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(cachedMaxBorderMeters, 0f, cachedMaxBorderMeters);
		((Bounds)(ref tileBounds)).min = ((Bounds)(ref tileBounds)).min - val;
		((Bounds)(ref tileBounds)).max = ((Bounds)(ref tileBounds)).max + val;
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
		Bounds val = rcExpandTileBounds(rcCalcTileBounds(new Vector2Int(tx, ty)));
		float coarseDistanceToShore = texturing.GetCoarseDistanceToShore(((Bounds)(ref val)).center);
		if (!float.IsFinite(coarseDistanceToShore))
		{
			return false;
		}
		Vector2 val2 = new Vector2(((Bounds)(ref val)).extents.x, ((Bounds)(ref val)).extents.z);
		float magnitude = ((Vector2)(ref val2)).magnitude;
		return coarseDistanceToShore - magnitude > maxShoreDistance;
	}

	public bool GetTilePolysInternal(int tx, int ty, List<Vector3> polys)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.GetTilePolysInternal"))
		{
			using (TimeWarning.New("ClearBuffer"))
			{
				for (int i = 0; i < TilePolysBuffer.Length; i++)
				{
					TilePolysBuffer[i] = Vector3.zero;
				}
			}
			if (!IsValid())
			{
				return false;
			}
			if (!RecastWrapper.GetTilePolys(NavMeshHandle, tx, ty, TilePolysBuffer, 2048, out var outPolyCount))
			{
				return false;
			}
			using (TimeWarning.New("ApplyPolys"))
			{
				for (int j = 0; j < outPolyCount * 6; j++)
				{
					polys.Add(TilePolysBuffer[j]);
				}
			}
			return true;
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

	public bool SamplePosition(NavVector3 position, out NavHit hit, Vector3 extents)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		ulong nearestPolyRef;
		return SamplePositionPoly(position, out hit, extents, out nearestPolyRef);
	}

	public bool SamplePositionPoly(NavVector3 position, out NavHit hit, Vector3 extents, out ulong nearestPolyRef)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.SamplePosition"))
		{
			hit = default(NavHit);
			nearestPolyRef = 0uL;
			if (!IsValid())
			{
				if (AI.logIssues)
				{
					RustNavigation.LogError("NavMesh has not been built yet.");
				}
				return false;
			}
			if (!RecastWrapper.SamplePosition(NavMeshHandle, in position.Value, in extents, out var nearestPosition, out nearestPolyRef))
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

	public bool Raycast(NavVector3 startPos, NavVector3 endPos, out NavHit hit)
	{
		ulong startRef = 0uL;
		return Raycast(ref startRef, startPos, endPos, out hit);
	}

	public bool Raycast(ref ulong startRef, NavVector3 startPos, NavVector3 endPos, out NavHit hit)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.Raycast"))
		{
			hit = default(NavHit);
			if (!IsValid())
			{
				if (AI.logIssues)
				{
					RustNavigation.LogError("NavMesh has not been built yet.");
				}
				return false;
			}
			if (!RecastWrapper.Raycast(NavMeshHandle, ref startRef, in startPos.Value, in endPos.Value, out var hitLocation, out var hitNormal))
			{
				return false;
			}
			hit = new NavHit
			{
				position = new NavVector3(hitLocation),
				normal = new NavVector3(hitNormal)
			};
			return true;
		}
	}

	public bool Move(ref ulong polyRef, NavVector3 startPos, NavVector3 endPos, out NavVector3 movedPos)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
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
			if (!RecastWrapper.Move(NavMeshHandle, ref polyRef, in startPos.Value, in endPos.Value, out var movedPos2))
			{
				return false;
			}
			movedPos = new NavVector3(movedPos2);
			return true;
		}
	}

	public bool CalculatePath(NavVector3 start, NavVector3 end, RustNavMeshPath path)
	{
		ulong startRef = 0uL;
		return CalculatePath(ref startRef, start, end, path);
	}

	public bool CalculatePath(ref ulong startRef, NavVector3 start, NavVector3 end, RustNavMeshPath path)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		path.Reset();
		if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
		{
			return false;
		}
		DtStatus dtStatus = RecastWrapper.FindPath(NavMeshHandle, ref startRef, in start.Value, in end.Value, PathBuffer, out var pathLength, path.polyRefs, out path.polyRefCount, PathfindingMaxIterations);
		if (((uint)dtStatus & 0x80000000u) == 2147483648u)
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
		path.status = (NavMeshPathStatus)((dtStatus & DtStatus.PartialResult) == DtStatus.PartialResult);
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

	public bool CorridorMove(IntPtr corridor, NavVector3 desiredPos, out NavVector3 resultPos, out ulong firstPolyRef)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.CorridorMove"))
		{
			resultPos = desiredPos;
			firstPolyRef = 0uL;
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return false;
			}
			if (!RecastWrapper.CorridorMove(NavMeshHandle, corridor, in desiredPos.Value, out var resultPos2, out firstPolyRef))
			{
				return false;
			}
			resultPos = new NavVector3(resultPos2);
			return true;
		}
	}

	public bool CorridorOptimizeAndMove(IntPtr corridor, NavVector3 optimizeNextNS, float optimizationRange, NavVector3 desiredPosNS, out NavVector3 resultPosNS)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.CorridorOptimizeAndMove"))
		{
			resultPosNS = desiredPosNS;
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return false;
			}
			if (!RecastWrapper.CorridorOptimizeAndMove(NavMeshHandle, corridor, in optimizeNextNS.Value, optimizationRange, in desiredPosNS.Value, out var resultPos, out var _))
			{
				return false;
			}
			resultPosNS = new NavVector3(resultPos);
			return true;
		}
	}

	public bool CorridorMoveTargetPosition(IntPtr corridor, NavVector3 desiredTargetNS, out NavVector3 resultTargetNS)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.CorridorMoveTargetPosition"))
		{
			resultTargetNS = desiredTargetNS;
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return false;
			}
			if (!RecastWrapper.CorridorMoveTargetPosition(NavMeshHandle, corridor, in desiredTargetNS.Value, out var resultTarget))
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

	public bool CorridorIsValid(IntPtr corridor, int maxLookAhead)
	{
		using (TimeWarning.New("RustNavmesh.CorridorIsValid"))
		{
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return false;
			}
			return RecastWrapper.CorridorIsValid(NavMeshHandle, corridor, maxLookAhead);
		}
	}

	public void CorridorOptimizeVisibility(IntPtr corridor, NavVector3 next, float optimizationRange)
	{
		using (TimeWarning.New("RustNavmesh.CorridorOptimizeVisibility"))
		{
			if (RustNavigation.EnsureNewNavmesh() && IsValid())
			{
				RecastWrapper.CorridorOptimizeVisibility(NavMeshHandle, corridor, in next.Value, optimizationRange);
			}
		}
	}

	public bool FindDistanceToWall(ref ulong startRef, NavVector3 centerPos, float maxRadius, out NavHit hit)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.FindDistanceToWall"))
		{
			hit = default(NavHit);
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
			if (!RecastWrapper.FindDistanceToWall(NavMeshHandle, ref startRef, in centerPos.Value, maxRadius, out var hitDistance, out var hitLocation, out var hitNormal))
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

	public bool FindDonutPointsInCircle(ref ulong startRef, NavVector3 centerNS, float maxRadius, float minRadius, float angleOffset, int count, List<NavVector3> resultsNS)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
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
			if (!RecastWrapper.FindDonutPointsInCircle(NavMeshHandle, ref startRef, in centerNS.Value, maxRadius, minRadius, angleOffset, count, DonutPointsBuffer, out var numFound))
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

	public unsafe bool Save(string path)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.Save"))
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			long timestamp = Stopwatch.GetTimestamp();
			if (!IsValid())
			{
				RustNavigation.Log("Navmesh not built, nothing to save.");
				return false;
			}
			PooledList<(int, int)> val = Pool.Get<PooledList<(int, int)>>();
			try
			{
				tileBuilder.GetPendingTilesForNavmeshOnMainThread(this, (List<(int tx, int ty)>)(object)val);
				int num = System.Runtime.CompilerServices.Unsafe.SizeOf<ManagedNavPayload>() + ((List<(int, int)>)(object)val).Count * 4 * 2;
				IntPtr intPtr = Marshal.AllocHGlobal(num);
				bool flag;
				try
				{
					ManagedNavPayload managedNavPayload = new ManagedNavPayload
					{
						payloadVersion = 1,
						buildParams = BuildParams,
						buildParamsHiRes = BuildParamsHiRes,
						currentNavmeshBounds = CurrentNavmeshBounds,
						tileNum = tileNum,
						pendingTileCount = ((List<(int, int)>)(object)val).Count
					};
					System.Runtime.CompilerServices.Unsafe.Write((void*)intPtr, managedNavPayload);
					int* ptr = (int*)((byte*)(void*)intPtr + System.Runtime.CompilerServices.Unsafe.SizeOf<ManagedNavPayload>());
					foreach (var (num2, num3) in (List<(int, int)>)(object)val)
					{
						*(ptr++) = num2;
						*(ptr++) = num3;
					}
					int num4 = 2;
					if (RustNav.saveCompression)
					{
						num4 |= 1;
					}
					flag = RecastWrapper.SaveNavMesh(path, NavMeshHandle, in BuildParams, ((Bounds)(ref CurrentNavmeshBounds)).min, ((Bounds)(ref CurrentNavmeshBounds)).max, intPtr, num, num4, RustNav.saveThreads);
				}
				finally
				{
					Marshal.FreeHGlobal(intPtr);
				}
				if (!flag)
				{
					RustNavigation.LogError("Failed to save navmesh to " + path);
					return false;
				}
				double num5 = (double)(Stopwatch.GetTimestamp() - timestamp) * 1000.0 / (double)Stopwatch.Frequency;
				RustNavigation.Log($"Successfully saved navmesh ({((List<(int, int)>)(object)val).Count} pending tiles) in {num5} ms");
				return true;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
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
			IntPtr intPtr = RecastWrapper.LoadNavMesh(path, out var managedBlob, out var managedBlobSize, RustNav.saveThreads);
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
				if (payload.payloadVersion != 1)
				{
					RustNavigation.LogError($"Unsupported managed payload version {payload.payloadVersion}");
					return null;
				}
				if (payload.pendingTileCount < 0 || managedBlobSize != System.Runtime.CompilerServices.Unsafe.SizeOf<ManagedNavPayload>() + payload.pendingTileCount * 4 * 2)
				{
					RustNavigation.LogError($"Managed payload size mismatch ({managedBlobSize} bytes for {payload.pendingTileCount} pending tiles)");
					return null;
				}
				if (((Vector2Int)(ref payload.tileNum)).x <= 0 || ((Vector2Int)(ref payload.tileNum)).y <= 0)
				{
					RustNavigation.LogError($"Invalid tile dimensions: {((Vector2Int)(ref payload.tileNum)).x}x{((Vector2Int)(ref payload.tileNum)).y}");
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
							rustNavmesh.MarkTileAsBuilt(tile);
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
					if (num3 < 0 || num4 < 0 || num3 >= ((Vector2Int)(ref payload.tileNum)).x || num4 >= ((Vector2Int)(ref payload.tileNum)).y)
					{
						RustNavigation.LogError(string.Format("Invalid pending tile coordinates: {0},{1} (max: {2},{3})", new object[4]
						{
							num3,
							num4,
							((Vector2Int)(ref payload.tileNum)).x - 1,
							((Vector2Int)(ref payload.tileNum)).y - 1
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
						for (int k = 0; k < ((Vector2Int)(ref payload.tileNum)).y; k++)
						{
							for (int l = 0; l < ((Vector2Int)(ref payload.tileNum)).x; l++)
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
						GetTilePolysInternal(((Vector2Int)(ref current)).x, ((Vector2Int)(ref current)).y, (List<Vector3>)(object)val3);
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
							val8 = ((Matrix4x4)(ref value2)).MultiplyPoint3x4(val8);
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

	public bool FillDebugDrawProtoForTile(NavMeshData navMeshData, int tx, int ty, Matrix4x4? transform = null)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
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
			PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
			try
			{
				GetTilePolysInternal(tx, ty, (List<Vector3>)(object)val);
				for (int i = 0; i < ((List<Vector3>)(object)val).Count; i += 6)
				{
					VectorList val2 = Pool.Get<VectorList>();
					val2.vectorPoints = Pool.Get<List<Vector3>>();
					for (int j = 0; j < 6; j++)
					{
						Vector3 val3 = ((List<Vector3>)(object)val)[i + j];
						if (val3 == Vector3.zero)
						{
							break;
						}
						if (transform.HasValue)
						{
							Matrix4x4 value = transform.Value;
							val3 = ((Matrix4x4)(ref value)).MultiplyPoint3x4(val3);
						}
						val2.vectorPoints.Add(val3);
					}
					navMeshData.polygons.Add(val2);
				}
				return true;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public void Dispose()
	{
		RustNavigation.Log("Disposing navmesh...");
		tileBuilder.CancelPendingTilesForOnMainThread(this);
		if (NavMeshHandle != IntPtr.Zero)
		{
			RecastWrapper.DestroyNavMesh(NavMeshHandle);
			NavMeshHandle = IntPtr.Zero;
		}
		tiles = null;
	}
}
