using System;

namespace Rust.Ai.Gen2.Nav;

public class Tile
{
	public int tx;

	public int ty;

	public IntPtr tileBytes;

	public int dataSize;

	public bool wasBuiltOnce;

	public Tile(int tx, int ty)
	{
		this.tx = tx;
		this.ty = ty;
	}
}
