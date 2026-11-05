using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class RoomExteriorIndex
{
	private sealed class RoomCluster : IPooled
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

	private readonly Dictionary<Room, RoomCluster> roomClusters = new Dictionary<Room, RoomCluster>();

	private readonly Dictionary<ulong, RoomCluster> blockClusters = new Dictionary<ulong, RoomCluster>();

	private readonly HashSet<RoomCluster> clusters = new HashSet<RoomCluster>();

	public bool IsBuilt { get; private set; }

	public int FailedOpenCount { get; private set; }

	public void Clear()
	{
		foreach (RoomCluster cluster in clusters)
		{
			RoomCluster current = cluster;
			Pool.Free<RoomCluster>(ref current);
		}
		clusters.Clear();
		roomClusters.Clear();
		blockClusters.Clear();
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
				Measure(rooms);
				BuildClusters(rooms);
				IsBuilt = true;
			}
			catch
			{
				Clear();
				throw;
			}
		}
	}

	public void Repair(List<Room> created, List<Room> dissolved, List<Room> changed, List<Room> relabelled)
	{
		if (!IsBuilt)
		{
			throw new InvalidOperationException("Build the exterior index before repairing it.");
		}
		using (TimeWarning.New("RoomOcclusion.RepairOutsideRooms"))
		{
			HashSet<RoomCluster> hashSet = Pool.Get<HashSet<RoomCluster>>();
			List<Room> list = Pool.Get<List<Room>>();
			try
			{
				Measure(created);
				Measure(changed);
				LeaveClusters(dissolved, hashSet);
				LeaveClusters(changed, hashSet);
				FindJoinedClusters(created, hashSet);
				FindJoinedClusters(changed, hashSet);
				foreach (RoomCluster item in hashSet)
				{
					foreach (Room room in item.Rooms)
					{
						if (roomClusters.Remove(room))
						{
							list.Add(room);
						}
					}
					foreach (ulong block in item.Blocks)
					{
						blockClusters.Remove(block);
					}
					if (item.FailedOpen)
					{
						FailedOpenCount--;
					}
					clusters.Remove(item);
					RoomCluster roomCluster = item;
					Pool.Free<RoomCluster>(ref roomCluster);
				}
				list.AddRange(created);
				list.AddRange(changed);
				list.Sort((Room a, Room b) => a.Id.CompareTo(b.Id));
				BuildClusters(list);
				relabelled.AddRange(list);
			}
			catch
			{
				Clear();
				throw;
			}
			finally
			{
				Pool.FreeUnmanaged<Room>(ref list);
				Pool.FreeUnmanaged<RoomCluster>(ref hashSet);
			}
		}
	}

	private void LeaveClusters(List<Room> rooms, HashSet<RoomCluster> affected)
	{
		foreach (Room room in rooms)
		{
			if (roomClusters.TryGetValue(room, out var value))
			{
				affected.Add(value);
			}
			roomClusters.Remove(room);
		}
	}

	private void FindJoinedClusters(List<Room> rooms, HashSet<RoomCluster> affected)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		foreach (Room room in rooms)
		{
			Enumerator<BlockFace> enumerator2 = room.Faces.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					ulong blockId = enumerator2.Current.BlockId;
					if (blockId != 0L && blockClusters.TryGetValue(blockId, out var value))
					{
						affected.Add(value);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private void BuildClusters(List<Room> rooms)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		List<int> parents = Pool.Get<List<int>>();
		Dictionary<ulong, int> dictionary = Pool.Get<Dictionary<ulong, int>>();
		Dictionary<int, RoomCluster> dictionary2 = Pool.Get<Dictionary<int, RoomCluster>>();
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
						int num = FindCluster(j);
						int num2 = FindCluster(value);
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
				int key = FindCluster(k);
				if (!dictionary2.TryGetValue(key, out var value2))
				{
					value2 = Pool.Get<RoomCluster>();
					clusters.Add(value2);
					dictionary2.Add(key, value2);
				}
				value2.Rooms.Add(rooms[k]);
				roomClusters.Add(rooms[k], value2);
			}
			foreach (KeyValuePair<ulong, int> item in dictionary)
			{
				RoomCluster roomCluster = roomClusters[rooms[item.Value]];
				roomCluster.Blocks.Add(item.Key);
				blockClusters.Add(item.Key, roomCluster);
			}
			foreach (RoomCluster value3 in dictionary2.Values)
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
			Pool.FreeUnmanaged<int, RoomCluster>(ref dictionary2);
			Pool.FreeUnmanaged<ulong, int>(ref dictionary);
			Pool.FreeUnmanaged<int>(ref parents);
		}
		int FindCluster(int index)
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

	private static bool Classify(RoomCluster cluster)
	{
		Room room = null;
		float num = 0f;
		int num2 = 0;
		foreach (Room room2 in cluster.Rooms)
		{
			room2.IsOutside = false;
			if (room2.Faces.Count != 0)
			{
				float outwardness = room2.Outwardness;
				int outwardFaces = room2.OutwardFaces;
				bool flag = room == null || outwardness > num;
				if (room != null && outwardness >= 0f && num >= 0f)
				{
					flag = outwardFaces > num2 || (outwardFaces == num2 && outwardness > num);
				}
				if (flag)
				{
					room = room2;
					num = outwardness;
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
			foreach (Room room3 in cluster.Rooms)
			{
				room3.IsOutside = room3.Faces.Count > 0;
			}
			return true;
		}
		room.IsOutside = true;
		return false;
	}

	private static void Measure(List<Room> rooms)
	{
		foreach (Room room in rooms)
		{
			room.Outwardness = Outwardness(room, out room.OutwardFaces);
		}
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
