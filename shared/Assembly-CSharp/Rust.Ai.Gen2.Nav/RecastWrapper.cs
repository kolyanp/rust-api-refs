using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Rust.Ai.Gen2.Nav;

public static class RecastWrapper
{
	public delegate void LogCallback(string message);

	public const int MAX_PATH_SIZE = 256;

	public const int DT_VERTS_PER_POLYGON = 6;

	public const int MAX_POLYS_PER_TILE = 2048;

	public const byte RC_FORCE_UNWALKABLE_AREA = 62;

	public const byte RC_AREA_ORDINARY = 0;

	public const int RC_AREA_DOOR = 3;

	public const int RC_AREA_DOOR_LEAF = 6;

	public const ushort POLYFLAGS_WALK = 1;

	public const ushort POLYFLAGS_DOOR = 4;

	public const ushort POLYFLAGS_NPC_DOOR = 32;

	private const string DLLName = "RustNative";

	public const int MAX_DONUT_POINTS = 64;

	public const int MAX_FLOOR_POLYS = 2048;

	public const int MAX_FLOOR_TILES = 256;

	private static bool floorAroundPolyMissing;

	public const int WALL_SEGMENT_FLOATS = 5;

	private static bool wallsAroundMissing;

	private static bool saveNavMeshExMissing;

	private static bool appendDeltaMissing;

	public static bool WallsAroundAvailable => !wallsAroundMissing;

	public static bool HasAppendNavMeshDelta => !appendDeltaMissing;

	[DllImport("RustNative")]
	public static extern void DestroyNavMesh(IntPtr navMesh);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool SamplePosition(IntPtr navMesh, in Vector3 position, in Vector3 extents, out Vector3 nearestPosition, out ulong nearestPolyRef, int includeGatedDoorPolys, int allowNpcDoors);

	[DllImport("RustNative", EntryPoint = "SamplePosition")]
	public unsafe static extern byte SamplePositionInJob(IntPtr navMesh, Vector3* position, Vector3* extents, Vector3* nearestPosition, ulong* nearestPolyRef, int includeGatedDoorPolys, int allowNpcDoors);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool Raycast(IntPtr navMesh, ref ulong startRef, in Vector3 startPos, in Vector3 endPos, out Vector3 hitLocation, out Vector3 hitNormal, int allowNpcDoors);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool Move(IntPtr navMesh, ref ulong polyRef, in Vector3 startPos, in Vector3 endPos, out Vector3 movedPos, int allowNpcDoors);

	public unsafe static DtStatus FindPath(IntPtr navMesh, ref ulong startRef, in Vector3 start, in Vector3 end, Vector3[] path, out int pathLength, ulong[] pathPolys, out int pathPolyCount, int maxIterations, int allowNpcDoors)
	{
		if (path == null || path.Length < 256)
		{
			throw new ArgumentException("Path buffer must hold MAX_PATH_SIZE entries.", "path");
		}
		if (pathPolys == null || pathPolys.Length < 256)
		{
			throw new ArgumentException("Polygon buffer must hold MAX_PATH_SIZE entries.", "pathPolys");
		}
		fixed (Vector3* path2 = path)
		{
			fixed (ulong* pathPolys2 = pathPolys)
			{
				return FindPathInternal(navMesh, ref startRef, in start, in end, path2, out pathLength, pathPolys2, out pathPolyCount, maxIterations, allowNpcDoors);
			}
		}
	}

	[DllImport("RustNative", EntryPoint = "FindPath")]
	private unsafe static extern DtStatus FindPathInternal(IntPtr navMesh, ref ulong startRef, in Vector3 start, in Vector3 end, Vector3* path, out int pathLength, ulong* pathPolys, out int pathPolyCount, int maxIterations, int allowNpcDoors);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FindDistanceToWall(IntPtr navMesh, ref ulong startRef, in Vector3 centerPos, float maxRadius, out float hitDistance, out Vector3 hitLocation, out Vector3 hitNormal, int allowNpcDoors);

	public unsafe static bool FindDonutPointsInCircle(IntPtr navMesh, ref ulong startRef, in Vector3 centerPos, float maxRadius, float minRadius, float angleOffset, int maxPoints, Vector3[] points, out int numFound, int allowNpcDoors)
	{
		if (points == null || maxPoints > points.Length)
		{
			throw new ArgumentException("Point buffer must hold maxPoints entries.", "points");
		}
		fixed (Vector3* points2 = points)
		{
			return FindDonutPointsInCircleInternal(navMesh, ref startRef, in centerPos, maxRadius, minRadius, angleOffset, maxPoints, points2, out numFound, allowNpcDoors);
		}
	}

	[DllImport("RustNative", EntryPoint = "FindDonutPointsInCircle")]
	[return: MarshalAs(UnmanagedType.U1)]
	private unsafe static extern bool FindDonutPointsInCircleInternal(IntPtr navMesh, ref ulong startRef, in Vector3 centerPos, float maxRadius, float minRadius, float angleOffset, int maxPoints, Vector3* points, out int numFound, int allowNpcDoors);

	public unsafe static bool GetFloorAroundPoly(IntPtr navMesh, ref ulong startRef, in Vector3 centerPos, float maxArea, ulong[] refs, out int count, uint[] tiles, out int tileCount, out float area, out Vector3 boundsMin, out Vector3 boundsMax, out bool open, out uint stamp, int allowNpcDoors)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (refs == null || refs.Length < 2048)
		{
			throw new ArgumentException("Ref buffer must hold MAX_FLOOR_POLYS entries.", "refs");
		}
		if (tiles == null || tiles.Length < 256)
		{
			throw new ArgumentException("Tile buffer must hold MAX_FLOOR_TILES entries.", "tiles");
		}
		count = 0;
		tileCount = 0;
		area = 0f;
		boundsMin = default;
		boundsMax = default;
		open = false;
		stamp = 0u;
		if (floorAroundPolyMissing)
		{
			return false;
		}
		try
		{
			fixed (ulong* refs2 = refs)
			{
				fixed (uint* tiles2 = tiles)
				{
					bool floorAroundPolyInternal = GetFloorAroundPolyInternal(navMesh, ref startRef, in centerPos, maxArea, refs2, 2048, out count, tiles2, 256, out tileCount, out area, out boundsMin, out boundsMax, out var open2, out stamp, allowNpcDoors);
					open = open2 != 0;
					return floorAroundPolyInternal;
				}
			}
		}
		catch (EntryPointNotFoundException)
		{
			floorAroundPolyMissing = true;
			Debug.LogWarning((object)"RustNative has no GetFloorAroundPoly, livestock floors read as no navmesh until the library is rolled.");
			return false;
		}
	}

	[DllImport("RustNative", EntryPoint = "GetFloorAroundPoly")]
	[return: MarshalAs(UnmanagedType.U1)]
	private unsafe static extern bool GetFloorAroundPolyInternal(IntPtr navMesh, ref ulong startRef, in Vector3 centerPos, float maxArea, ulong* refs, int maxRefs, out int count, uint* tiles, int maxTiles, out int tileCount, out float area, out Vector3 boundsMin, out Vector3 boundsMax, out int open, out uint stamp, int allowNpcDoors);

	public unsafe static bool TileStampsUnchangedSince(IntPtr navMesh, uint[] tiles, int count, uint lastStamp)
	{
		if (tiles == null || count > tiles.Length)
		{
			throw new ArgumentException("Tile buffer must hold count entries.", "tiles");
		}
		fixed (uint* tiles2 = tiles)
		{
			return TileStampsUnchangedSinceInternal(navMesh, tiles2, count, lastStamp);
		}
	}

	public unsafe static bool TileStampsUnchangedSince(IntPtr navMesh, uint* tiles, int count, uint lastStamp)
	{
		return TileStampsUnchangedSinceInternal(navMesh, tiles, count, lastStamp);
	}

	[DllImport("RustNative", EntryPoint = "TileStampsUnchangedSince")]
	[return: MarshalAs(UnmanagedType.U1)]
	private unsafe static extern bool TileStampsUnchangedSinceInternal(IntPtr navMesh, uint* tiles, int count, uint lastStamp);

	public unsafe static int GetWallsAround(IntPtr navMesh, ulong startRef, in Vector3 centerPos, float radius, float* segments, int maxSegments, uint* tiles, int maxTiles, out int tileCount, out uint stamp, int allowNpcDoors)
	{
		tileCount = 0;
		stamp = 0u;
		if (wallsAroundMissing)
		{
			return 0;
		}
		try
		{
			return GetWallsAroundInternal(navMesh, startRef, in centerPos, radius, segments, maxSegments, tiles, maxTiles, out tileCount, out stamp, allowNpcDoors);
		}
		catch (EntryPointNotFoundException)
		{
			wallsAroundMissing = true;
			Debug.LogWarning((object)"RustNative has no GetWallsAround, agents keep only their pivot off walls until the library is rolled.");
			return 0;
		}
	}

	[DllImport("RustNative", EntryPoint = "GetWallsAround")]
	private unsafe static extern int GetWallsAroundInternal(IntPtr navMesh, ulong startRef, in Vector3 centerPos, float radius, float* segments, int maxSegments, uint* tiles, int maxTiles, out int tileCount, out uint stamp, int allowNpcDoors);

	public unsafe static int GetFloorBoundary(IntPtr navMesh, ulong[] refs, int count, Vector3[] segments, int allowNpcDoors)
	{
		if (refs == null || count > refs.Length || segments == null)
		{
			throw new ArgumentException("Buffers must hold count refs and an even number of points.");
		}
		fixed (ulong* refs2 = refs)
		{
			fixed (Vector3* segments2 = segments)
			{
				return GetFloorBoundaryInternal(navMesh, refs2, count, segments2, segments.Length / 2, allowNpcDoors);
			}
		}
	}

	[DllImport("RustNative", EntryPoint = "GetFloorBoundary")]
	private unsafe static extern int GetFloorBoundaryInternal(IntPtr navMesh, ulong* refs, int count, Vector3* segments, int maxSegments, int allowNpcDoors);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool IsValidPolyRef(IntPtr navMesh, ulong polyRef);

	[DllImport("RustNative")]
	public static extern IntPtr CreateCorridor();

	[DllImport("RustNative")]
	public static extern void FreeCorridor(IntPtr corridor);

	[DllImport("RustNative")]
	public static extern IntPtr CreateMovementQuery();

	[DllImport("RustNative")]
	public static extern void FreeMovementQuery(IntPtr query);

	[DllImport("RustNative")]
	public unsafe static extern int CorridorMoveWithQuery(IntPtr navMesh, IntPtr corridor, IntPtr query, Vector3* desiredPosition, Vector3* optimizeNext, float optimizationRange, Vector3* resultPosition, int allowNpcDoors);

	[DllImport("RustNative")]
	public static extern void CorridorReset(IntPtr corridor, ulong polyRef, in Vector3 pos);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool CorridorSetPath(IntPtr corridor, [In] ulong[] polys, int npolys, in Vector3 targetPos);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool CorridorMove(IntPtr navMesh, IntPtr corridor, in Vector3 desiredPos, out Vector3 resultPos, out ulong firstPolyRef, int allowNpcDoors);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool CorridorMoveTargetPosition(IntPtr navMesh, IntPtr corridor, in Vector3 desiredTarget, out Vector3 resultTarget, int allowNpcDoors);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool CorridorOptimizeAndMove(IntPtr navMesh, IntPtr corridor, in Vector3 optimizeNext, float optimizationRange, in Vector3 desiredPos, out Vector3 resultPos, out ulong firstPolyRef, int allowNpcDoors);

	public unsafe static int CorridorFindCorners(IntPtr navMesh, IntPtr corridor, Vector3[] cornerVerts, int maxCorners, out bool endReached)
	{
		if (cornerVerts == null)
		{
			throw new ArgumentNullException("cornerVerts");
		}
		maxCorners = Math.Min(maxCorners, Math.Min(cornerVerts.Length, 256));
		if (maxCorners <= 0)
		{
			endReached = false;
			return 0;
		}
		fixed (Vector3* cornerVerts2 = cornerVerts)
		{
			return CorridorFindCornersInternal(navMesh, corridor, cornerVerts2, maxCorners, out endReached);
		}
	}

	[DllImport("RustNative", EntryPoint = "CorridorFindCorners")]
	private unsafe static extern int CorridorFindCornersInternal(IntPtr navMesh, IntPtr corridor, Vector3* cornerVerts, int maxCorners, [MarshalAs(UnmanagedType.U1)] out bool endReached);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool CorridorIsValid(IntPtr navMesh, IntPtr corridor, int maxLookAhead, int allowNpcDoors, uint lastCheckedStamp, out uint currentStamp);

	[DllImport("RustNative")]
	public static extern void CorridorOptimizeVisibility(IntPtr navMesh, IntPtr corridor, in Vector3 next, float optimizationRange, int allowNpcDoors);

	[DllImport("RustNative")]
	public static extern ulong CorridorGetFirstPoly(IntPtr corridor);

	[DllImport("RustNative")]
	public static extern void SetLogCallback(LogCallback callback);

	[DllImport("RustNative")]
	public static extern void SetLegacyBuild([MarshalAs(UnmanagedType.U1)] bool enabled);

	[DllImport("RustNative")]
	public static extern IntPtr CreateEmptyNavMesh(in NavMeshBuildParams buildParams, in Vector3 bmin, in Vector3 bmax);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool RemoveTileFromNavMesh(IntPtr navMeshWrapper, int tx, int ty);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreeTileData(IntPtr tileData);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool AddPrebuiltTileToNavMesh(IntPtr navMeshWrapper, int tx, int ty, IntPtr tileData, int dataSize);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ComputeTriangleYExtent(IntPtr verts, IntPtr tris, int triCount, float minX, float maxX, float minZ, float maxZ, out float outMinY, out float outMaxY);

	[DllImport("RustNative")]
	public static extern IntPtr CreateHeightFieldRaw(in NavMeshBuildParams buildParams, IntPtr verts, int nverts, IntPtr tris, int triCount, in Vector3 bmin, in Vector3 bmax, IntPtr triAreaOverrides);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreeHeightField(IntPtr heightfield);

	public unsafe static IntPtr CreateCompactHeightField(in NavMeshBuildParams buildParams, IntPtr heightField, NavMeshBuildVolume[] volumes, int volumeCount)
	{
		if (volumes != null && volumeCount > volumes.Length)
		{
			throw new ArgumentException("Volume count exceeds the buffer length.", "volumeCount");
		}
		fixed (NavMeshBuildVolume* volumes2 = volumes)
		{
			return CreateCompactHeightFieldNative(in buildParams, heightField, volumes2, volumeCount);
		}
	}

	[DllImport("RustNative", EntryPoint = "CreateCompactHeightField")]
	private unsafe static extern IntPtr CreateCompactHeightFieldNative(in NavMeshBuildParams buildParams, IntPtr heightField, NavMeshBuildVolume* volumes, int volumeCount);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreeCompactHeightField(IntPtr compactHeightfield);

	[DllImport("RustNative")]
	public static extern IntPtr CreatePolymesh(in NavMeshBuildParams buildParams, IntPtr compactHeightField);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreePolymesh(IntPtr polymesh);

	[DllImport("RustNative")]
	public static extern IntPtr CreateDetailPolymesh(in NavMeshBuildParams buildParams, IntPtr polyMesh, IntPtr compactHeightField, float sampleDistMult, float sampleMaxErrorMult);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreeDetailPolymesh(IntPtr detailPolyMesh);

	[DllImport("RustNative")]
	public static extern IntPtr CreateNavData(in NavMeshBuildParams buildParams, int tx, int ty, IntPtr polyMesh, IntPtr detailMesh, out int dataSize);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ValidateTileData(IntPtr data, int dataSize);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ValidateNavMesh(IntPtr navWrapper);

	public unsafe static int SetPolyFlagsInObb(IntPtr navWrapper, in Vector3 center, in Vector3 halfExtents, float axisXx, float axisXz, int area, ushort setFlags, ushort clearFlags, in Vector3 ownerPos, Vector3[] ownerPositions, int ownerCount, ulong[] outRefs, int maxOutRefs, out int outRefCount)
	{
		if (ownerCount > 0 && (ownerPositions == null || ownerCount > ownerPositions.Length))
		{
			throw new ArgumentException("Owner buffer must hold ownerCount entries.", "ownerPositions");
		}
		if (outRefs != null && maxOutRefs > outRefs.Length)
		{
			throw new ArgumentException("Reference buffer must hold maxOutRefs entries.", "outRefs");
		}
		fixed (Vector3* ownerPositions2 = ownerPositions)
		{
			fixed (ulong* outRefs2 = outRefs)
			{
				return SetPolyFlagsInObbInternal(navWrapper, in center, in halfExtents, axisXx, axisXz, area, setFlags, clearFlags, in ownerPos, ownerPositions2, ownerCount, outRefs2, maxOutRefs, out outRefCount);
			}
		}
	}

	[DllImport("RustNative", EntryPoint = "SetPolyFlagsInObb")]
	private unsafe static extern int SetPolyFlagsInObbInternal(IntPtr navWrapper, in Vector3 center, in Vector3 halfExtents, float axisXx, float axisXz, int area, ushort setFlags, ushort clearFlags, in Vector3 ownerPos, Vector3* ownerPositions, int ownerCount, ulong* outRefs, int maxOutRefs, out int outRefCount);

	[DllImport("RustNative")]
	public static extern int SetPolyFlagsForRefs(IntPtr navWrapper, [In] ulong[] refs, int refCount, ushort setFlags, ushort clearFlags);

	public unsafe static bool GetTilePolysEx(IntPtr navWrapper, int tx, int ty, Vector3[] outVertices, byte[] outAreas, ushort[] outFlags, int maxPolys, out int outPolyCount)
	{
		if (outVertices == null || maxPolys > outVertices.Length / 6)
		{
			throw new ArgumentException("Vertex buffer must hold maxPolys polygons.", "outVertices");
		}
		if (outAreas != null && maxPolys > outAreas.Length)
		{
			throw new ArgumentException("Area buffer must hold maxPolys entries.", "outAreas");
		}
		if (outFlags != null && maxPolys > outFlags.Length)
		{
			throw new ArgumentException("Flag buffer must hold maxPolys entries.", "outFlags");
		}
		fixed (Vector3* outVertices2 = outVertices)
		{
			fixed (byte* outAreas2 = outAreas)
			{
				fixed (ushort* outFlags2 = outFlags)
				{
					return GetTilePolysExInternal(navWrapper, tx, ty, outVertices2, outAreas2, outFlags2, maxPolys, out outPolyCount);
				}
			}
		}
	}

	[DllImport("RustNative", EntryPoint = "GetTilePolysEx")]
	[return: MarshalAs(UnmanagedType.U1)]
	private unsafe static extern bool GetTilePolysExInternal(IntPtr navWrapper, int tx, int ty, Vector3* outVertices, byte* outAreas, ushort* outFlags, int maxPolys, out int outPolyCount);

	[DllImport("RustNative")]
	public static extern int ResetPolyFlagsForArea(IntPtr navWrapper, int area, ushort setFlags, ushort clearFlags);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool SaveNavMesh(string path, IntPtr navWrapper, in NavMeshBuildParams buildParams, in Vector3 bmin, in Vector3 bmax, IntPtr managedBlob, int managedBlobSize, int flags, int threadCount);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool SaveNavMeshEx(string path, IntPtr navWrapper, in NavMeshBuildParams buildParams, in Vector3 bmin, in Vector3 bmax, IntPtr managedBlob, int managedBlobSize, int flags, int threadCount, out NavMeshSavePhases phases);

	public static bool SaveNavMeshWithPhases(string path, IntPtr navWrapper, in NavMeshBuildParams buildParams, in Vector3 bmin, in Vector3 bmax, IntPtr managedBlob, int managedBlobSize, int flags, int threadCount, out NavMeshSavePhases phases, out bool phasesValid)
	{
		if (!saveNavMeshExMissing)
		{
			try
			{
				bool result = SaveNavMeshEx(path, navWrapper, in buildParams, in bmin, in bmax, managedBlob, managedBlobSize, flags, threadCount, out phases);
				phasesValid = true;
				return result;
			}
			catch (EntryPointNotFoundException)
			{
				saveNavMeshExMissing = true;
				Debug.LogWarning((object)"RustNative has no SaveNavMeshEx, navmesh saves will report no phase breakdown until the library is rolled.");
			}
		}
		phases = default;
		phasesValid = false;
		return SaveNavMesh(path, navWrapper, in buildParams, in bmin, in bmax, managedBlob, managedBlobSize, flags, threadCount);
	}

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool AppendNavMeshDelta(string path, long expectedLength, IntPtr navWrapper, IntPtr dirtyCoords, int dirtyCount, IntPtr managedBlob, int managedBlobSize, int flags, int threadCount, out NavMeshSavePhases phases);

	public static bool TryAppendNavMeshDelta(string path, long expectedLength, IntPtr navWrapper, IntPtr dirtyCoords, int dirtyCount, IntPtr managedBlob, int managedBlobSize, int flags, int threadCount, out NavMeshSavePhases phases)
	{
		if (appendDeltaMissing)
		{
			phases = default;
			return false;
		}
		try
		{
			return AppendNavMeshDelta(path, expectedLength, navWrapper, dirtyCoords, dirtyCount, managedBlob, managedBlobSize, flags, threadCount, out phases);
		}
		catch (EntryPointNotFoundException)
		{
			appendDeltaMissing = true;
			Debug.LogWarning((object)"RustNative has no AppendNavMeshDelta, every navmesh save will write the whole file until the library is rolled.");
			phases = default;
			return false;
		}
	}

	[DllImport("RustNative")]
	public static extern IntPtr LoadNavMesh(string path, out IntPtr managedBlob, out int managedBlobSize, int threadCount);

	[DllImport("RustNative")]
	public static extern void FreeManagedBlob(IntPtr blob);

	[DllImport("RustNative")]
	public static extern int GetNavMeshTileCoords(IntPtr navWrapper, IntPtr outCoords, int maxPairs);
}
