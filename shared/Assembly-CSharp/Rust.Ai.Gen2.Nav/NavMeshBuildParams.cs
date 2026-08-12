using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Rust.Ai.Gen2.Nav;

[Serializable]
public struct NavMeshBuildParams(bool dummy = true)
{
	public enum EPartitionType : uint
	{
		Watershed,
		Monotone,
		Layers
	}

	[Tooltip("The xz-plane cell size to use for fields. [Limit: > 0] [Units: wu]")]
	[Min(0f)]
	public float cellSize = agentRadius / 3f;

	[Tooltip("The y-axis cell size to use for fields. [Limit: > 0] [Units: wu]")]
	[Min(0f)]
	public float cellHeight = cellSize;

	[Tooltip("Agent height. Needs to be a multiple of cellHeight")]
	[Min(0f)]
	public float agentHeight = 1.7f;

	[Tooltip("Agent radius. Needs to be a multiple of walkableRadius")]
	[Min(0f)]
	public float agentRadius = 0.25f;

	[Min(0f)]
	[Tooltip("Maximum climb height for agent. Needs to be a multiple of cellHeight")]
	public float agentMaxClimb = 0.4f;

	[Range(0f, 90f)]
	[Tooltip("The maximum slope that is considered walkable. [Limits: 0 <= value < 90] [Units: Degrees]")]
	public float agentMaxSlope = 45f;

	[Tooltip("The width/height size of tile's on the xz-plane. [Limit: >= 0] [Units: vx]")]
	[Range(16f, 1024f)]
	public float tileSize = 512f;

	[Tooltip("The type of partitioning used for NavMesh generation")]
	public EPartitionType partitionType = EPartitionType.Watershed;

	[Range(0f, 65535f)]
	public int maxNodes = 2048;

	[Min(0f)]
	public float minRegionSizeMeters = 5.76f;

	[MarshalAs(UnmanagedType.U1)]
	[Tooltip("Removes small obstacles and rasterization artifacts that the agent would be able to walk over")]
	public bool filterLowHangingObstacles = true;

	[MarshalAs(UnmanagedType.U1)]
	[Tooltip("Remove regions hanging in the air over ledges")]
	public bool filterLedgeSpans = true;

	[MarshalAs(UnmanagedType.U1)]
	[Tooltip("Marks walkable spans as not walkable if the clearance above the span is less than the specified walkableHeight")]
	public bool filterWalkableLowHeightSpans = true;

	[MarshalAs(UnmanagedType.U1)]
	public bool buildDetailMesh = true;
}
