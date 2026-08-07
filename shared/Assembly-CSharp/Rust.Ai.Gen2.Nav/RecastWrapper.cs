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

	private const string DLLName = "RustNative";

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool GetTilePolys(IntPtr navWrapper, int tx, int ty, [In][Out] Vector3[] outVertices, int maxPolys, out int outPolyCount);

	[DllImport("RustNative")]
	public static extern void DestroyNavMesh(IntPtr navMesh);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool SamplePosition(IntPtr navMesh, in Vector3 position, in Vector3 extents, out Vector3 nearestPosition, out ulong nearestPolyRef);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool Raycast(IntPtr navMesh, in Vector3 startPos, in Vector3 endPos, out Vector3 hitLocation, out Vector3 hitNormal);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool Move(IntPtr navMesh, ulong startRef, in Vector3 startPos, in Vector3 endPos, out ulong outRef, out Vector3 movedPos);

	[DllImport("RustNative")]
	public static extern DtStatus FindPath(IntPtr navMesh, in Vector3 start, in Vector3 end, [Out] Vector3[] path, out int pathLength, int maxIterations);

	[DllImport("RustNative")]
	public static extern void SetLogCallback(LogCallback callback);

	[DllImport("RustNative")]
	public static extern IntPtr CreateEmptyNavMesh(in NavMeshBuildParams buildParams, in Vector3 bmin, in Vector3 bmax);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool AddTileToNavMesh(IntPtr navMeshWrapper, in NavMeshBuildParams buildParams, IntPtr verts, int vertCount, IntPtr tris, int triCount, int tx, int ty);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool RemoveTileFromNavMesh(IntPtr navMeshWrapper, int tx, int ty);

	[DllImport("RustNative")]
	public static extern IntPtr AllocateTileData(int dataSize);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreeTileData(IntPtr tileData);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool SaveAll(string path, IntPtr navMeshWrapper);

	[DllImport("RustNative")]
	public static extern IntPtr LoadAll(in NavMeshBuildParams buildParams, string path);

	[DllImport("RustNative")]
	public static extern IntPtr PrebuildTile(IntPtr navMeshWrapper, in NavMeshBuildParams buildParams, IntPtr verts, int vertCount, IntPtr tris, int triCount, int tx, int ty, out int dataSize);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool AddPrebuiltTileToNavMesh(IntPtr navMeshWrapper, int tx, int ty, IntPtr tileData, int dataSize);

	[DllImport("RustNative")]
	public static extern void PrebuildAndAddAllTiles(IntPtr navMeshWrapper, in NavMeshBuildParams buildParams, IntPtr verts, int vertCount, IntPtr tris, int triCount, int txChunk, int tyChunk, int tileNumX, int tileNumZ, int parallelBuildTileChunkSize);

	[DllImport("RustNative")]
	public static extern IntPtr CreateChunkyMesh(IntPtr verts, IntPtr tris, int triCount, int trisPerChunk);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreeChunkyMesh(IntPtr chunkyMesh);

	[DllImport("RustNative")]
	public static extern IntPtr CreateHeightField(in NavMeshBuildParams buildParams, IntPtr chunkyMesh, IntPtr verts, int nverts, in Vector3 bmin, in Vector3 bmax);

	[DllImport("RustNative")]
	public static extern IntPtr CreateHeightFieldRaw(in NavMeshBuildParams buildParams, IntPtr verts, int nverts, IntPtr tris, int triCount, in Vector3 bmin, in Vector3 bmax);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreeHeightField(IntPtr heightfield);

	[DllImport("RustNative")]
	public static extern IntPtr CreateCompactHeightField(in NavMeshBuildParams buildParams, IntPtr heightField);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreeCompactHeightField(IntPtr compactHeightfield);

	[DllImport("RustNative")]
	public static extern IntPtr CreatePolymesh(in NavMeshBuildParams buildParams, IntPtr compactHeightField);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreePolymesh(IntPtr polymesh);

	[DllImport("RustNative")]
	public static extern IntPtr CreateDetailPolymesh(in NavMeshBuildParams buildParams, IntPtr polyMesh, IntPtr compactHeightField);

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
}
