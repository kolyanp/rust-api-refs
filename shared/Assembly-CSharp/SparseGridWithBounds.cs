using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SparseGridWithBounds<T>
{
	private Dictionary<(int x, int z), HashSet<T>> grid = new Dictionary<(int, int), HashSet<T>>();

	private Dictionary<T, HashSet<(int x, int z)>> reverseLookup = new Dictionary<T, HashSet<(int, int)>>();

	private int cellSize;

	public SparseGridWithBounds(int cellSize = 32)
	{
		this.cellSize = cellSize;
	}

	public void Add(Bounds bounds, T item)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (reverseLookup.ContainsKey(item))
		{
			Debug.LogError((object)$"Item {item} is already in the grid. Remove it before adding it with new bounds.");
			return;
		}
		HashSet<(int, int)> hashSet = Pool.Get<HashSet<(int, int)>>();
		reverseLookup.Add(item, hashSet);
		(int, int) cellKey = GetCellKey(((Bounds)(ref bounds)).min);
		(int, int) cellKey2 = GetCellKey(((Bounds)(ref bounds)).max);
		var (i, _) = cellKey;
		for (; i <= cellKey2.Item1; i++)
		{
			for (int j = cellKey.Item2; j <= cellKey2.Item2; j++)
			{
				if (!grid.TryGetValue((i, j), out var value))
				{
					value = Pool.Get<HashSet<T>>();
					grid.Add((i, j), value);
				}
				value.Add(item);
				hashSet.Add((i, j));
			}
		}
	}

	public bool Remove(T item)
	{
		if (!reverseLookup.TryGetValue(item, out HashSet<(int, int)> value))
		{
			return false;
		}
		foreach (var item2 in value)
		{
			if (grid.TryGetValue(item2, out var value2))
			{
				value2.Remove(item);
				if (value2.Count == 0)
				{
					grid.Remove(item2);
					Pool.FreeUnmanaged<T>(ref value2);
				}
			}
			else
			{
				Debug.LogError((object)$"Inconsistent state: value {item} is mapped to cell {item2} in reverseLookup but that cell does not exist in grid.");
			}
		}
		reverseLookup.Remove(item);
		Pool.FreeUnmanaged<(int, int)>(ref value);
		return true;
	}

	public void FindAll(Bounds bounds, HashSet<T> foundItems)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		foundItems.Clear();
		(int, int) cellKey = GetCellKey(((Bounds)(ref bounds)).min);
		(int, int) cellKey2 = GetCellKey(((Bounds)(ref bounds)).max);
		var (i, _) = cellKey;
		for (; i <= cellKey2.Item1; i++)
		{
			for (int j = cellKey.Item2; j <= cellKey2.Item2; j++)
			{
				(int, int) key = (i, j);
				if (grid.TryGetValue(key, out var value))
				{
					foundItems.UnionWith(value);
				}
			}
		}
	}

	public (int x, int z) GetCellKey(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		int item = Mathf.FloorToInt(position.x / (float)cellSize);
		int item2 = Mathf.FloorToInt(position.z / (float)cellSize);
		return (x: item, z: item2);
	}

	public void Clear()
	{
		foreach (HashSet<T> value in grid.Values)
		{
			HashSet<T> current = value;
			Pool.FreeUnmanaged<T>(ref current);
		}
		grid.Clear();
		foreach (HashSet<(int, int)> value2 in reverseLookup.Values)
		{
			HashSet<(int, int)> current2 = value2;
			Pool.FreeUnmanaged<(int, int)>(ref current2);
		}
		reverseLookup.Clear();
	}
}
