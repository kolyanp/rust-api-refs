using System;
using UnityEngine;

[Serializable]
public sealed class ByteQuadtree
{
	public struct Element(ByteQuadtree source, int x, int y, int level) : IEquatable<Element>
	{
		private ByteQuadtree source = source;

		private int x = x;

		private int y = y;

		private int level = level;

		public bool IsLeaf => level == 0;

		public bool IsRoot => level == source.levels - 1;

		public int ByteMap => level;

		public uint Value => source.GetValue(level, x, y);

		public Vector2 Coords
		{
			get
			{
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				return new Vector2((float)x, (float)y);
			}
		}

		public int Depth => source.levels - level - 1;

		public Element Parent
		{
			get
			{
				if (IsRoot)
				{
					throw new Exception("Element is the root and therefore has no parent.");
				}
				return new Element(source, x / 2, y / 2, level + 1);
			}
		}

		public Element Child1
		{
			get
			{
				if (IsLeaf)
				{
					throw new Exception("Element is a leaf and therefore has no children.");
				}
				return new Element(source, x * 2, y * 2, level - 1);
			}
		}

		public Element Child2
		{
			get
			{
				if (IsLeaf)
				{
					throw new Exception("Element is a leaf and therefore has no children.");
				}
				return new Element(source, x * 2 + 1, y * 2, level - 1);
			}
		}

		public Element Child3
		{
			get
			{
				if (IsLeaf)
				{
					throw new Exception("Element is a leaf and therefore has no children.");
				}
				return new Element(source, x * 2, y * 2 + 1, level - 1);
			}
		}

		public Element Child4
		{
			get
			{
				if (IsLeaf)
				{
					throw new Exception("Element is a leaf and therefore has no children.");
				}
				return new Element(source, x * 2 + 1, y * 2 + 1, level - 1);
			}
		}

		public Element MaxChild
		{
			get
			{
				Element child = Child1;
				Element child2 = Child2;
				Element child3 = Child3;
				Element child4 = Child4;
				uint value = child.Value;
				uint value2 = child2.Value;
				uint value3 = child3.Value;
				uint value4 = child4.Value;
				if (value >= value2 && value >= value3 && value >= value4)
				{
					return child;
				}
				if (value2 >= value3 && value2 >= value4)
				{
					return child2;
				}
				if (value3 >= value4)
				{
					return child3;
				}
				return child4;
			}
		}

		public Element RandChild
		{
			get
			{
				Element child = Child1;
				Element child2 = Child2;
				Element child3 = Child3;
				Element child4 = Child4;
				uint value = child.Value;
				uint value2 = child2.Value;
				uint value3 = child3.Value;
				uint value4 = child4.Value;
				float num = value + value2 + value3 + value4;
				float value5 = Random.value;
				if ((float)value / num >= value5)
				{
					return child;
				}
				if ((float)(value + value2) / num >= value5)
				{
					return child2;
				}
				if ((float)(value + value2 + value3) / num >= value5)
				{
					return child3;
				}
				return child4;
			}
		}

		public bool Equals(Element other)
		{
			if (source == other.source && x == other.x && y == other.y)
			{
				return level == other.level;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Element other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(source, x, y, level);
		}
	}

	private const int TileLevel = 4;

	private const int TileSize = 16;

	private const int TileArea = 256;

	private const int TileSumCount = 84;

	private static readonly int[] TileSumOffsets = new int[4] { 0, 0, 64, 80 };

	[SerializeField]
	private int size;

	[SerializeField]
	private int levels;

	[SerializeField]
	private ByteMap[] values;

	[SerializeField]
	private int[] tileIndexes;

	[SerializeField]
	private byte[] tileValues;

	[SerializeField]
	private ushort[] tileSums;

	public int Size => size;

	public Element Root => new Element(this, 0, 0, levels - 1);

	public void UpdateValues(byte[] baseValues)
	{
		size = Mathf.RoundToInt(Mathf.Sqrt((float)baseValues.Length));
		levels = Mathf.RoundToInt(Mathf.Max(Mathf.Log((float)size, 2f), 0f)) + 1;
		values = new ByteMap[levels];
		if (size < 16)
		{
			throw new ArgumentException($"ByteQuadtree needs at least {16}x{16} base values");
		}
		UpdateTiles(baseValues);
		for (int i = 4; i < levels; i++)
		{
			ByteMap byteMap = (values[i] = CreateLevel(i));
			for (int j = 0; j < byteMap.Size; j++)
			{
				for (int k = 0; k < byteMap.Size; k++)
				{
					byteMap[k, j] = SumChildren(i, k, j);
				}
			}
		}
	}

	private ByteMap CreateLevel(int level)
	{
		int num = 1 << levels - level - 1;
		int bytes = 1 + (level + 3) / 4;
		return new ByteMap(num, bytes);
	}

	private void UpdateTiles(byte[] baseValues)
	{
		int num = size >> 4;
		tileIndexes = new int[num * num];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int tileStart = GetTileStart(j, i);
				tileIndexes[i * num + j] = (IsUniformTile(baseValues, tileStart) ? (-(baseValues[tileStart] + 1)) : num2++);
			}
		}
		tileValues = new byte[num2 * 256];
		tileSums = new ushort[num2 * 84];
		for (int k = 0; k < num; k++)
		{
			for (int l = 0; l < num; l++)
			{
				int num3 = tileIndexes[k * num + l];
				if (num3 >= 0)
				{
					CopyTileValues(baseValues, GetTileStart(l, k), num3);
					UpdateTileSums(num3, l, k);
				}
			}
		}
	}

	private int GetTileStart(int tileX, int tileY)
	{
		return tileY * 16 * size + tileX * 16;
	}

	private bool IsUniformTile(byte[] baseValues, int start)
	{
		byte b = baseValues[start];
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				if (baseValues[start + i * size + j] != b)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void CopyTileValues(byte[] baseValues, int start, int tile)
	{
		for (int i = 0; i < 16; i++)
		{
			Buffer.BlockCopy(baseValues, start + i * size, tileValues, tile * 256 + i * 16, 16);
		}
	}

	private void UpdateTileSums(int tile, int tileX, int tileY)
	{
		for (int i = 1; i < 4; i++)
		{
			int num = 16 >> i;
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num; k++)
				{
					tileSums[GetTileSumIndex(tile, i, k, j)] = (ushort)SumChildren(i, tileX * num + k, tileY * num + j);
				}
			}
		}
	}

	private uint SumChildren(int level, int x, int y)
	{
		return GetValue(level - 1, 2 * x, 2 * y) + GetValue(level - 1, 2 * x + 1, 2 * y) + GetValue(level - 1, 2 * x, 2 * y + 1) + GetValue(level - 1, 2 * x + 1, 2 * y + 1);
	}

	private uint GetValue(int level, int x, int y)
	{
		if (level >= 4)
		{
			return values[level][x, y];
		}
		int num = 4 - level;
		int num2 = (1 << num) - 1;
		int num3 = size >> 4;
		int tile = tileIndexes[(y >> num) * num3 + (x >> num)];
		return GetTileValue(tile, level, x & num2, y & num2);
	}

	private uint GetTileValue(int tile, int level, int x, int y)
	{
		if (tile < 0)
		{
			return (uint)(-tile - 1 << 2 * level);
		}
		if (level == 0)
		{
			return tileValues[tile * 256 + y * 16 + x];
		}
		return tileSums[GetTileSumIndex(tile, level, x, y)];
	}

	private static int GetTileSumIndex(int tile, int level, int x, int y)
	{
		return tile * 84 + TileSumOffsets[level] + y * (16 >> level) + x;
	}
}
