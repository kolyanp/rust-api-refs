using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class RoomSpatialIndex
{
	private struct CellRange
	{
		public int MinX;

		public int MinZ;

		public int MaxX;

		public int MaxZ;
	}

	public const float CellSize = 32f;

	private readonly Dictionary<long, List<Room>> cells = new Dictionary<long, List<Room>>();

	private readonly Dictionary<Room, CellRange> ranges = new Dictionary<Room, CellRange>();

	public int Count => ranges.Count;

	public bool Contains(Room room)
	{
		return ranges.ContainsKey(room);
	}

	public void Add(Room room)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (room == null || room.Faces.Count == 0 || ranges.ContainsKey(room))
		{
			return;
		}
		CellRange value = RangeOf(room.Bounds);
		ranges[room] = value;
		for (int i = value.MinX; i <= value.MaxX; i++)
		{
			for (int j = value.MinZ; j <= value.MaxZ; j++)
			{
				long key = Key(i, j);
				if (!cells.TryGetValue(key, out var value2))
				{
					value2 = Pool.Get<List<Room>>();
					cells[key] = value2;
				}
				value2.Add(room);
			}
		}
	}

	public void Remove(Room room)
	{
		if (room == null || !ranges.TryGetValue(room, out var value))
		{
			return;
		}
		ranges.Remove(room);
		for (int i = value.MinX; i <= value.MaxX; i++)
		{
			for (int j = value.MinZ; j <= value.MaxZ; j++)
			{
				long key = Key(i, j);
				if (cells.TryGetValue(key, out var value2))
				{
					value2.Remove(room);
					if (value2.Count <= 0)
					{
						cells.Remove(key);
						Pool.FreeUnmanaged<Room>(ref value2);
					}
				}
			}
		}
	}

	public void Clear()
	{
		foreach (KeyValuePair<long, List<Room>> cell in cells)
		{
			List<Room> value = cell.Value;
			Pool.FreeUnmanaged<Room>(ref value);
		}
		cells.Clear();
		ranges.Clear();
	}

	public void Gather(Vector3 position, float range, ListHashSet<Room> into)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (range <= 0f)
		{
			return;
		}
		CellRange cellRange = RangeOf(new Bounds(position, Vector3.one * (range * 2f)));
		for (int i = cellRange.MinX; i <= cellRange.MaxX; i++)
		{
			for (int j = cellRange.MinZ; j <= cellRange.MaxZ; j++)
			{
				if (!cells.TryGetValue(Key(i, j), out var value))
				{
					continue;
				}
				foreach (Room item in value)
				{
					into.TryAdd(item);
				}
			}
		}
	}

	private static CellRange RangeOf(Bounds bounds)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		return new CellRange
		{
			MinX = Cell(bounds.min.x),
			MaxX = Cell(bounds.max.x),
			MinZ = Cell(bounds.min.z),
			MaxZ = Cell(bounds.max.z)
		};
	}

	private static int Cell(float coordinate)
	{
		return Mathf.FloorToInt(coordinate / 32f);
	}

	private static long Key(int x, int z)
	{
		return ((long)x << 32) | (uint)z;
	}
}
