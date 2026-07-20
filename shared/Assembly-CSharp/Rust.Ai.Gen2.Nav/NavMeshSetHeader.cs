using System.Runtime.InteropServices;
using UnityEngine;

namespace Rust.Ai.Gen2.Nav;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NavMeshSetHeader
{
	public int magic;

	public int version;

	public int numTiles;

	public NavMeshBuildParams buildParams;

	public NavMeshBuildParams buildParamsHiRes;

	public Bounds currentNavmeshBounds;

	public Vector2Int tileNum;
}
