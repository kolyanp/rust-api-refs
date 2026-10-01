using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class RoomExteriorIndex
{
	private sealed class Component : IPooled
	{
		public readonly List<Room> Rooms = new List<Room>();

		public readonly List<ulong> Blocks = new List<ulong>();

		public bool FailedOpen;

		public void EnterPool()
		{
			Rooms.Clear();
			Blocks.Clear();
			FailedOpen = false;
		}

		public void LeavePool()
		{
		}
	}

	private readonly Dictionary<Room, Component> roomComponents = new Dictionary<Room, Component>();

	private readonly Dictionary<ulong, Component> blockComponents = new Dictionary<ulong, Component>();

	private readonly HashSet<Component> components = new HashSet<Component>();

	public bool IsBuilt { get; private set; }

	public int FailedOpenCount { get; private set; }

	public void Clear()
	{
		foreach (Component component in components)
		{
			Component current = component;
			Pool.Free<Component>(ref current);
		}
		components.Clear();
		roomComponents.Clear();
		blockComponents.Clear();
		FailedOpenCount = 0;
		IsBuilt = false;
	}

	public void Rebuild(List<Room> rooms)
	{
		using (TimeWarning.New("RoomOcclusion.LabelOutsideRooms"))
		{
			Clear();
			try
			{
				BuildComponents(rooms);
				IsBuilt = true;
			}
			catch
			{
				Clear();
				throw;
			}
		}
	}

	public void Repair(List<Room> created, List<Room> dissolved)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (!IsBuilt)
		{
			throw new InvalidOperationException("Build the exterior index before repairing it.");
		}
		using (TimeWarning.New("RoomOcclusion.RepairOutsideRooms"))
		{
			HashSet<Component> hashSet = Pool.Get<HashSet<Component>>();
			List<Room> list = Pool.Get<List<Room>>();
			try
			{
				foreach (Room item in dissolved)
				{
					if (roomComponents.TryGetValue(item, out var value))
					{
						hashSet.Add(value);
					}
					roomComponents.Remove(item);
				}
				foreach (Room item2 in created)
				{
					Enumerator<BlockFace> enumerator2 = item2.Faces.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							ulong blockId = enumerator2.Current.BlockId;
							if (blockId != 0L && blockComponents.TryGetValue(blockId, out var value2))
							{
								hashSet.Add(value2);
							}
						}
					}
					finally
					{
						((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
					}
				}
				foreach (Component item3 in hashSet)
				{
					foreach (Room room in item3.Rooms)
					{
						if (roomComponents.Remove(room))
						{
							list.Add(room);
						}
					}
					foreach (ulong block in item3.Blocks)
					{
						blockComponents.Remove(block);
					}
					if (item3.FailedOpen)
					{
						FailedOpenCount--;
					}
					components.Remove(item3);
					Component component = item3;
					Pool.Free<Component>(ref component);
				}
				list.AddRange(created);
				list.Sort((Room a, Room b) => a.Id.CompareTo(b.Id));
				BuildComponents(list);
			}
			catch
			{
				Clear();
				throw;
			}
			finally
			{
				Pool.FreeUnmanaged<Room>(ref list);
				Pool.FreeUnmanaged<Component>(ref hashSet);
			}
		}
	}

	private void BuildComponents(List<Room> rooms)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		List<int> parents = Pool.Get<List<int>>();
		Dictionary<ulong, int> dictionary = Pool.Get<Dictionary<ulong, int>>();
		Dictionary<int, Component> dictionary2 = Pool.Get<Dictionary<int, Component>>();
		try
		{
			for (int i = 0; i < rooms.Count; i++)
			{
				parents.Add(-1);
			}
			for (int j = 0; j < rooms.Count; j++)
			{
				Enumerator<BlockFace> enumerator = rooms[j].Faces.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						ulong blockId = enumerator.Current.BlockId;
						if (blockId == 0L)
						{
							continue;
						}
						if (!dictionary.TryGetValue(blockId, out var value))
						{
							dictionary.Add(blockId, j);
							continue;
						}
						int num = FindComponent(j);
						int num2 = FindComponent(value);
						if (num != num2)
						{
							if (parents[num] > parents[num2])
							{
								int num3 = num2;
								int num4 = num;
								num = num3;
								num2 = num4;
							}
							parents[num] += parents[num2];
							parents[num2] = num;
						}
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
				}
			}
			for (int k = 0; k < rooms.Count; k++)
			{
				int key = FindComponent(k);
				if (!dictionary2.TryGetValue(key, out var value2))
				{
					value2 = Pool.Get<Component>();
					components.Add(value2);
					dictionary2.Add(key, value2);
				}
				value2.Rooms.Add(rooms[k]);
				roomComponents.Add(rooms[k], value2);
			}
			foreach (KeyValuePair<ulong, int> item in dictionary)
			{
				Component component = roomComponents[rooms[item.Value]];
				component.Blocks.Add(item.Key);
				blockComponents.Add(item.Key, component);
			}
			foreach (Component value3 in dictionary2.Values)
			{
				value3.FailedOpen = Classify(value3);
				if (value3.FailedOpen)
				{
					FailedOpenCount++;
				}
			}
		}
		finally
		{
			Pool.FreeUnmanaged<int, Component>(ref dictionary2);
			Pool.FreeUnmanaged<ulong, int>(ref dictionary);
			Pool.FreeUnmanaged<int>(ref parents);
		}
		int FindComponent(int index)
		{
			int num5 = index;
			while (parents[num5] >= 0)
			{
				num5 = parents[num5];
			}
			while (index != num5)
			{
				int num6 = parents[index];
				parents[index] = num5;
				index = num6;
			}
			return num5;
		}
	}

	private static bool Classify(Component component)
	{
		Room room = null;
		float num = 0f;
		int num2 = 0;
		foreach (Room room2 in component.Rooms)
		{
			room2.IsOutside = false;
			if (room2.Faces.Count != 0)
			{
				float num3 = Outwardness(room2, out var outwardFaces);
				bool flag = room == null || num3 > num;
				if (room != null && num3 >= 0f && num >= 0f)
				{
					flag = outwardFaces > num2 || (outwardFaces == num2 && num3 > num);
				}
				if (flag)
				{
					room = room2;
					num = num3;
					num2 = outwardFaces;
				}
			}
		}
		if (room == null)
		{
			return false;
		}
		if (num < 0f)
		{
			foreach (Room room3 in component.Rooms)
			{
				room3.IsOutside = room3.Faces.Count > 0;
			}
			return true;
		}
		room.IsOutside = true;
		return false;
	}

	public static float Outwardness(Room room)
	{
		int outwardFaces;
		return Outwardness(room, out outwardFaces);
	}

	private static float Outwardness(Room room, out int outwardFaces)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		outwardFaces = 0;
		if (room.Faces.Count == 0)
		{
			return 0f;
		}
		Vector3 val = Vector3.zero;
		Enumerator<BlockFace> enumerator = room.Faces.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BlockFace current = enumerator.Current;
				val += current.WorldCenter;
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		val /= (float)room.Faces.Count;
		float num = 0f;
		enumerator = room.Faces.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BlockFace current2 = enumerator.Current;
				float num2 = Vector3.Dot(current2.WorldNormal, current2.WorldCenter - val);
				num += num2;
				if (num2 > 0f)
				{
					outwardFaces++;
				}
			}
			return num;
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
