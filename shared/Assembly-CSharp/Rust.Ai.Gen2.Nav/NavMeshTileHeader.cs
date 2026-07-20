using System.Runtime.InteropServices;

namespace Rust.Ai.Gen2.Nav;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NavMeshTileHeader
{
	public int tx;

	public int ty;

	public int dataSize;
}
