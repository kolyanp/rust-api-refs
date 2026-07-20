using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

	public NavMeshBuildParams BuildParams = new NavMeshBuildParams(true);

	public NavMeshBuildParams BuildParamsHiRes = new NavMeshBuildParams(true);

	public int PathfindingMaxIterations = 1000;

	public Bounds CurrentNavmeshBounds;

	public Tile[] tiles;

	public IntPtr NavMeshHandle = IntPtr.Zero;

	private Vector2Int tileNum;

	private BackgroundTileBuilder tileBuilder;

	private static readonly int HeaderSize = Marshal.SizeOf<NavMeshSetHeader>();

	private static readonly int TileHeaderSize = Marshal.SizeOf<NavMeshTileHeader>();

	public bool IsValid()
	{
		return NavMeshHandle != IntPtr.Zero;
	}

	public RustNavmesh(BackgroundTileBuilder tileBuilder, NavMeshBuildParams? buildParamsOverride = null, NavMeshBuildParams? buildParamsHiResOverride = null, Bounds? boundsOverride = null, bool shouldBuild = true, bool synchronous = false)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh)
		{
			return;
		}
		RustNavigation.Log("Creating new navmesh...");
		if (tileBuilder == null)
		{
			RustNavigation.LogError("BackgroundTileBuilder is required to create a RustNavmesh");
			return;
		}
		this.tileBuilder = tileBuilder;
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
		if (buildParamsHiResOverride.HasValue)
		{
			BuildParamsHiRes = buildParamsHiResOverride.Value;
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
		RustNavigation.Log($"Queueing {((Vector2Int)(ref tileNum)).x * ((Vector2Int)(ref tileNum)).y} tiles for building...");
		for (int k = 0; k < ((Vector2Int)(ref tileNum)).y; k++)
		{
			for (int l = 0; l < ((Vector2Int)(ref tileNum)).x; l++)
			{
				tileBuilder.EnqueueOnMainThread(this, l, k, synchronous);
			}
		}
	}

	public bool AddTile(int tx, int ty, IntPtr tileData, int dataSize)
	{
		if (!RecastWrapper.AddPrebuiltTileToNavMesh(NavMeshHandle, tx, ty, tileData, dataSize))
		{
			tileData = IntPtr.Zero;
			return false;
		}
		Tile obj = tiles[Mathx.FlattenArrayCoord(tx, ty, ((Vector2Int)(ref tileNum)).x)];
		obj.tileBytes = tileData;
		obj.dataSize = dataSize;
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

	public NavMeshBuildParams GetBuildParamsForTile(int tx, int ty)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = rcCalcTileBounds(new Vector2Int(tx, ty));
		PooledList<Collider> val = Pool.Get<PooledList<Collider>>();
		try
		{
			GamePhysics.OverlapBounds(bounds, (List<Collider>)(object)val, 538968064, (QueryTriggerInteraction)2);
			foreach (Collider item in (List<Collider>)(object)val)
			{
				if (BaseNetworkableEx.Is<BuildingBlock>((Object)(object)GameObjectEx.ToBaseEntity(item), out BuildingBlock castedUnityObject) && castedUnityObject.isServer)
				{
					return BuildParamsHiRes;
				}
				if ((Object)(object)ConstructionErrors.GetPreventBuildingMonumentTag(item) != (Object)null)
				{
					return BuildParamsHiRes;
				}
			}
			return BuildParams;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
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

	public Bounds rcExpandTileBounds(Bounds tileBounds)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.CeilToInt(BuildParams.agentRadius / BuildParams.cellSize) + 3;
		((Bounds)(ref tileBounds)).min = ((Bounds)(ref tileBounds)).min - new Vector3(BuildParams.cellSize, 0f, BuildParams.cellSize) * (float)num;
		((Bounds)(ref tileBounds)).max = ((Bounds)(ref tileBounds)).max + new Vector3(BuildParams.cellSize, 0f, BuildParams.cellSize) * (float)num;
		return tileBounds;
	}

	public bool GetTilePolysInternal(int tx, int ty, List<Vector3> polys)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.GetTilePolysInternal"))
		{
			for (int i = 0; i < TilePolysBuffer.Length; i++)
			{
				TilePolysBuffer[i] = Vector3.zero;
			}
			if (!IsValid())
			{
				return false;
			}
			if (!RecastWrapper.GetTilePolys(NavMeshHandle, tx, ty, TilePolysBuffer, 2048, out var outPolyCount))
			{
				return false;
			}
			for (int j = 0; j < outPolyCount * 6; j++)
			{
				polys.Add(TilePolysBuffer[j]);
			}
			return true;
		}
	}

	private bool FillPathFromPathBuffer(List<Vector3> path, int pathCount)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!IsValid())
		{
			RustNavigation.LogError("NavMesh has not been built yet.");
			return false;
		}
		path.Clear();
		path.Capacity = Mathf.Max(path.Capacity, pathCount);
		for (int i = 0; i < pathCount; i++)
		{
			path.Add(PathBuffer[i]);
		}
		return true;
	}

	public bool SamplePosition(Vector3 position, out NavMeshHit hit, Vector3 extents)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.SamplePosition"))
		{
			hit = default(NavMeshHit);
			if (!IsValid())
			{
				RustNavigation.LogError("NavMesh has not been built yet.");
				return false;
			}
			if (!RecastWrapper.SamplePosition(NavMeshHandle, in position, in extents, out var nearestPosition))
			{
				return false;
			}
			if (nearestPosition == Vector3.zero)
			{
				return false;
			}
			NavMeshHit val = default(NavMeshHit);
			((NavMeshHit)(ref val)).position = nearestPosition;
			hit = val;
			return true;
		}
	}

	public bool Raycast(Vector3 startPos, Vector3 endPos, out NavMeshHit hit)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.Raycast"))
		{
			hit = default(NavMeshHit);
			if (!IsValid())
			{
				RustNavigation.LogError("NavMesh has not been built yet.");
				return false;
			}
			if (!RecastWrapper.Raycast(NavMeshHandle, in startPos, in endPos, out var hitLocation, out var hitNormal))
			{
				return false;
			}
			NavMeshHit val = default(NavMeshHit);
			((NavMeshHit)(ref val)).position = hitLocation;
			((NavMeshHit)(ref val)).normal = hitNormal;
			hit = val;
			return true;
		}
	}

	public bool Move(Vector3 startPos, Vector3 endPos, out Vector3 movedPos)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.Move"))
		{
			movedPos = startPos;
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			if (!IsValid())
			{
				RustNavigation.LogError("NavMesh has not been built yet.");
				return false;
			}
			return RecastWrapper.Move(NavMeshHandle, in startPos, in endPos, out movedPos);
		}
	}

	public bool CalculatePath(Vector3 start, Vector3 end, RustNavMeshPath path)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		path.Reset();
		if (!RustNavigation.EnsureNewNavmesh())
		{
			return false;
		}
		int pathLength;
		DtStatus dtStatus = RecastWrapper.FindPath(NavMeshHandle, in start, in end, PathBuffer, out pathLength, PathfindingMaxIterations);
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

	public unsafe bool Save(BinaryWriter writer)
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
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
			try
			{
				int num = 0;
				Tile[] array = tiles;
				foreach (Tile tile in array)
				{
					if (tile?.tileBytes != IntPtr.Zero && tile != null && tile.dataSize > 0)
					{
						num++;
					}
				}
				NavMeshSetHeader value = new NavMeshSetHeader
				{
					magic = 1297302868,
					version = 1,
					numTiles = num,
					buildParams = BuildParams,
					buildParamsHiRes = BuildParamsHiRes,
					currentNavmeshBounds = CurrentNavmeshBounds,
					tileNum = tileNum
				};
				Span<byte> span = stackalloc byte[HeaderSize];
				MemoryMarshal.Write(span, in value);
				writer.Write(span);
				int num2 = 0;
				array = tiles;
				foreach (Tile tile2 in array)
				{
					if (!(tile2.tileBytes == IntPtr.Zero) && tile2.dataSize > 0)
					{
						try
						{
							NavMeshTileHeader value2 = new NavMeshTileHeader
							{
								tx = tile2.tx,
								ty = tile2.ty,
								dataSize = tile2.dataSize
							};
							Span<byte> span2 = stackalloc byte[TileHeaderSize];
							MemoryMarshal.Write(span2, in value2);
							writer.Write(span2);
							writer.BaseStream.Write(new ReadOnlySpan<byte>((void*)tile2.tileBytes, tile2.dataSize));
							num2++;
						}
						catch (Exception ex)
						{
							RustNavigation.LogError($"Failed to write tile {tile2.tx},{tile2.ty}: {ex.Message}");
							return false;
						}
					}
				}
				if (num2 != num)
				{
					RustNavigation.LogError($"Expected to write {num} tiles but wrote {num2}");
					return false;
				}
				PooledList<(int, int)> val = Pool.Get<PooledList<(int, int)>>();
				try
				{
					tileBuilder.GetPendingTilesForNavmeshOnMainThread(this, (List<(int tx, int ty)>)(object)val);
					writer.Write(((List<(int, int)>)(object)val).Count);
					foreach (var (value3, value4) in (List<(int, int)>)(object)val)
					{
						writer.Write(value3);
						writer.Write(value4);
					}
					double num3 = (double)(Stopwatch.GetTimestamp() - timestamp) * 1000.0 / (double)Stopwatch.Frequency;
					RustNavigation.Log($"Successfully saved navmesh with {num2} tiles in {num3} ms");
					return true;
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			catch (Exception ex2)
			{
				RustNavigation.LogError("Failed to save navmesh: " + ex2.Message);
				return false;
			}
		}
	}

	public unsafe static RustNavmesh Load(BinaryReader reader, BackgroundTileBuilder tileBuilder, bool synchronous = false)
	{
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavmesh.Load"))
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return null;
			}
			long timestamp = Stopwatch.GetTimestamp();
			RustNavmesh rustNavmesh = null;
			try
			{
				if (reader.BaseStream.Length < HeaderSize)
				{
					RustNavigation.LogError("File too small to contain valid header");
					return null;
				}
				Span<byte> span = stackalloc byte[HeaderSize];
				if (reader.Read(span) != HeaderSize)
				{
					RustNavigation.LogError("Failed to read navmesh header");
					return null;
				}
				NavMeshSetHeader navMeshSetHeader = MemoryMarshal.Read<NavMeshSetHeader>(span);
				if (navMeshSetHeader.magic != 1297302868)
				{
					RustNavigation.LogError($"Invalid file format. Expected magic: {1297302868:X8}, got: {navMeshSetHeader.magic:X8}");
					return null;
				}
				if (navMeshSetHeader.version != 1)
				{
					RustNavigation.LogError($"Unsupported file version. Expected: {1}, got: {navMeshSetHeader.version}");
					return null;
				}
				if (navMeshSetHeader.numTiles < 0 || navMeshSetHeader.numTiles > 10000)
				{
					RustNavigation.LogError($"Invalid tile count: {navMeshSetHeader.numTiles}");
					return null;
				}
				Vector2Int val = navMeshSetHeader.tileNum;
				if (((Vector2Int)(ref val)).x <= 0 || ((Vector2Int)(ref val)).y <= 0)
				{
					RustNavigation.LogError($"Invalid tile dimensions: {((Vector2Int)(ref val)).x}x{((Vector2Int)(ref val)).y}");
					return null;
				}
				rustNavmesh = new RustNavmesh(tileBuilder, navMeshSetHeader.buildParams, navMeshSetHeader.buildParamsHiRes, navMeshSetHeader.currentNavmeshBounds, shouldBuild: false);
				if (!rustNavmesh.IsValid())
				{
					RustNavigation.LogError("Failed to initialize navmesh");
					return null;
				}
				int num = 0;
				int numTiles = navMeshSetHeader.numTiles;
				for (int i = 0; i < navMeshSetHeader.numTiles; i++)
				{
					if (reader.BaseStream.Position + TileHeaderSize > reader.BaseStream.Length)
					{
						RustNavigation.LogError($"Unexpected end of file while reading tile {i} header");
						rustNavmesh.Dispose();
						return null;
					}
					Span<byte> span2 = stackalloc byte[TileHeaderSize];
					if (reader.Read(span2) != TileHeaderSize)
					{
						RustNavigation.LogError($"Unexpected end of file while reading tile {i} header");
						rustNavmesh.Dispose();
						return null;
					}
					NavMeshTileHeader navMeshTileHeader = MemoryMarshal.Read<NavMeshTileHeader>(span2);
					if (navMeshTileHeader.dataSize <= 0)
					{
						RustNavigation.LogError($"Invalid tile data size: {navMeshTileHeader.dataSize} for tile {navMeshTileHeader.tx},{navMeshTileHeader.ty}");
						rustNavmesh.Dispose();
						return null;
					}
					if (navMeshTileHeader.tx < 0 || navMeshTileHeader.ty < 0 || navMeshTileHeader.tx >= ((Vector2Int)(ref val)).x || navMeshTileHeader.ty >= ((Vector2Int)(ref val)).y)
					{
						RustNavigation.LogError(string.Format("Invalid tile coordinates: {0},{1} (max: {2},{3})", new object[4]
						{
							navMeshTileHeader.tx,
							navMeshTileHeader.ty,
							((Vector2Int)(ref val)).x - 1,
							((Vector2Int)(ref val)).y - 1
						}));
						rustNavmesh.Dispose();
						return null;
					}
					if (reader.BaseStream.Position + navMeshTileHeader.dataSize > reader.BaseStream.Length)
					{
						RustNavigation.LogError($"Unexpected end of file while reading tile {navMeshTileHeader.tx},{navMeshTileHeader.ty} data");
						rustNavmesh.Dispose();
						return null;
					}
					IntPtr intPtr = IntPtr.Zero;
					try
					{
						intPtr = RecastWrapper.AllocateTileData(navMeshTileHeader.dataSize);
						if (intPtr == IntPtr.Zero)
						{
							RustNavigation.LogError($"Failed to allocate memory for tile {navMeshTileHeader.tx},{navMeshTileHeader.ty}");
							rustNavmesh.Dispose();
							return null;
						}
						Span<byte> buffer = new Span<byte>((void*)intPtr, navMeshTileHeader.dataSize);
						if (reader.Read(buffer) != navMeshTileHeader.dataSize)
						{
							RustNavigation.LogError($"Unexpected end of file while reading tile {navMeshTileHeader.tx},{navMeshTileHeader.ty} data");
							rustNavmesh.Dispose();
							return null;
						}
						if (!rustNavmesh.AddTile(navMeshTileHeader.tx, navMeshTileHeader.ty, intPtr, navMeshTileHeader.dataSize))
						{
							RustNavigation.LogError($"Failed to add tile {navMeshTileHeader.tx},{navMeshTileHeader.ty} to navmesh");
							rustNavmesh.Dispose();
							return null;
						}
						num++;
					}
					catch (Exception ex)
					{
						if (intPtr != IntPtr.Zero)
						{
							RecastWrapper.FreeTileData(intPtr);
						}
						RustNavigation.LogError($"Exception loading tile {navMeshTileHeader.tx},{navMeshTileHeader.ty}: {ex.Message}");
						rustNavmesh.Dispose();
						return null;
					}
				}
				if (num != numTiles)
				{
					RustNavigation.LogWarning($"Expected {numTiles} tiles but loaded {num}");
					rustNavmesh.Dispose();
					return null;
				}
				int num2 = reader.ReadInt32();
				if (num2 > 0)
				{
					RustNavigation.Log($"Queueing {num2} pending tiles for building...");
				}
				for (int j = 0; j < num2; j++)
				{
					int tx = reader.ReadInt32();
					int ty = reader.ReadInt32();
					tileBuilder.EnqueueOnMainThread(rustNavmesh, tx, ty, synchronous);
				}
				if (reader.BaseStream.Position != reader.BaseStream.Length)
				{
					RustNavigation.LogWarning($"File contains {reader.BaseStream.Length - reader.BaseStream.Position} bytes of trailing data");
					rustNavmesh.Dispose();
					return null;
				}
				double num3 = (double)(Stopwatch.GetTimestamp() - timestamp) * 1000.0 / (double)Stopwatch.Frequency;
				RustNavigation.Log($"Successfully loaded navmesh with {num} tiles in {num3} ms");
				return rustNavmesh;
			}
			catch (Exception ex2)
			{
				RustNavigation.LogError("Failed to load navmesh: " + ex2.Message);
				rustNavmesh?.Dispose();
				return null;
			}
		}
	}

	public bool FillDebugDrawProto(NavMeshData navMeshData, Bounds bounds, Matrix4x4? transform = null)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
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
						((List<Vector3>)(object)val2).AddRange((IEnumerable<Vector3>)val3);
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
				for (int i = 0; i < ((List<Vector3>)(object)val2).Count; i += 6)
				{
					VectorList val4 = Pool.Get<VectorList>();
					val4.vectorPoints = Pool.Get<List<Vector3>>();
					for (int j = 0; j < 6; j++)
					{
						Vector3 val5 = ((List<Vector3>)(object)val2)[i + j];
						if (val5 == Vector3.zero)
						{
							break;
						}
						if (transform.HasValue)
						{
							Matrix4x4 value = transform.Value;
							val5 = ((Matrix4x4)(ref value)).MultiplyPoint3x4(val5);
						}
						val4.vectorPoints.Add(val5);
					}
					navMeshData.polygons.Add(val4);
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
