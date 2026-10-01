using System;
using System.Collections.Generic;
using Facepunch;

public class RoomSolver : IPooled
{
	public List<Room> Rooms = new List<Room>();

	private int nextRoomId = 1;

	public List<Room> Dissolved = new List<Room>();

	private Stack<BlockFace> floodStack = new Stack<BlockFace>();

	public int NextRoomId => nextRoomId;

	void IPooled.EnterPool()
	{
		Clear();
		nextRoomId = 1;
	}

	void IPooled.LeavePool()
	{
	}

	public void Clear()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		FreeDissolved();
		foreach (Room room2 in Rooms)
		{
			Enumerator<BlockFace> enumerator2 = room2.Faces.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					enumerator2.Current.Room = null;
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
			Room room = room2;
			Pool.Free<Room>(ref room);
		}
		Rooms.Clear();
	}

	public void Solve(List<BlockFace> faces)
	{
		Clear();
		foreach (BlockFace face in faces)
		{
			face.Room = null;
		}
		foreach (BlockFace face2 in faces)
		{
			if (face2.Room == null)
			{
				FloodFrom(face2);
			}
		}
	}

	public void Repair(ListHashSet<BlockFace> dirtyFaces)
	{
		FreeDissolved();
		ListHashSet<BlockFace> val = Pool.Get<ListHashSet<BlockFace>>();
		try
		{
			for (int i = 0; i < dirtyFaces.Count; i++)
			{
				BlockFace blockFace = dirtyFaces[i];
				if (blockFace.Room != null)
				{
					DissolveRoom(blockFace.Room, val);
				}
				else
				{
					val.TryAdd(blockFace);
				}
			}
			for (int j = 0; j < val.Count; j++)
			{
				BlockFace blockFace2 = val[j];
				if (blockFace2.Room == null)
				{
					FloodFrom(blockFace2);
				}
			}
			for (int num = Rooms.Count - 1; num >= 0; num--)
			{
				if (Rooms[num].Faces.Count <= 0)
				{
					Room room = Rooms[num];
					Rooms.RemoveAt(num);
					Retire(room);
				}
			}
		}
		finally
		{
			Pool.FreeUnmanaged<BlockFace>(ref val);
		}
	}

	private void Retire(Room room)
	{
		Dissolved.Add(room);
	}

	private void FreeDissolved()
	{
		foreach (Room item in Dissolved)
		{
			Room current = item;
			Pool.Free<Room>(ref current);
		}
		Dissolved.Clear();
	}

	private void DissolveRoom(Room room, ListHashSet<BlockFace> seeds)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BlockFace> enumerator = room.Faces.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BlockFace current = enumerator.Current;
				current.Room = null;
				seeds.TryAdd(current);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Rooms.Remove(room);
		Retire(room);
	}

	private void FloodFrom(BlockFace seed)
	{
		Room room = Pool.Get<Room>();
		room.Id = nextRoomId++;
		Rooms.Add(room);
		floodStack.Clear();
		floodStack.Push(seed);
		seed.Room = room;
		room.Faces.Add(seed);
		while (floodStack.Count > 0)
		{
			foreach (KeyValuePair<BlockFace, int> item in floodStack.Pop().LinksPerFace)
			{
				BlockFace key = item.Key;
				if (key.Room != room)
				{
					if (key.Room != null)
					{
						Absorb(key.Room, room);
						continue;
					}
					key.Room = room;
					room.Faces.Add(key);
					floodStack.Push(key);
				}
			}
		}
	}

	private void Absorb(Room other, Room into)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BlockFace> enumerator = other.Faces.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BlockFace current = enumerator.Current;
				current.Room = into;
				into.Faces.Add(current);
				floodStack.Push(current);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		if (other.IsOutside)
		{
			into.IsOutside = true;
		}
		Rooms.Remove(other);
		Retire(other);
	}
}
