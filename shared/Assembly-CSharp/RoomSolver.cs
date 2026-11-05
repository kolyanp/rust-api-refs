using System;
using System.Collections.Generic;
using Facepunch;

public class RoomSolver : IPooled
{
	private sealed class Search
	{
		public int Parent;

		public bool Finished;

		public readonly List<BlockFace> Visited = new List<BlockFace>();

		public readonly List<BlockFace> Frontier = new List<BlockFace>();
	}

	public List<Room> Rooms = new List<Room>();

	private int nextRoomId = 1;

	public List<Room> Dissolved = new List<Room>();

	public List<Room> Created = new List<Room>();

	public List<Room> Changed = new List<Room>();

	public ListHashSet<BlockFace> Moved = new ListHashSet<BlockFace>();

	private readonly List<Room> emptied = new List<Room>();

	private Stack<BlockFace> floodStack = new Stack<BlockFace>();

	private readonly List<Search> searches = new List<Search>();

	private int searchCount;

	public bool HasEmptiedRooms => emptied.Count > 0;

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
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		FreeDissolved();
		emptied.Clear();
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
		Created.Clear();
		Changed.Clear();
		Moved.Clear();
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
		Created.Clear();
		Changed.Clear();
		Moved.Clear();
		ListHashSet<BlockFace> val = Pool.Get<ListHashSet<BlockFace>>();
		try
		{
			for (int i = 0; i < dirtyFaces.Count; i++)
			{
				BlockFace blockFace = dirtyFaces[i];
				if (blockFace.Room != null)
				{
					MarkChanged(blockFace.Room);
				}
				val.TryAdd(blockFace);
			}
			MergeAcrossLinks(val);
			SplitWhereDisconnected(val);
			foreach (Room item in emptied)
			{
				if (item.Faces.Count <= 0 && item.SolverIndex >= 0)
				{
					RemoveRoom(item);
					Retire(item);
				}
			}
			emptied.Clear();
		}
		finally
		{
			Pool.FreeUnmanaged<BlockFace>(ref val);
		}
	}

	private void MergeAcrossLinks(ListHashSet<BlockFace> pending)
	{
		for (int i = 0; i < pending.Count; i++)
		{
			BlockFace blockFace = pending[i];
			if (blockFace.Room == null)
			{
				AddFace(blockFace, CreateRoom());
			}
			foreach (KeyValuePair<BlockFace, int> item in blockFace.LinksPerFace)
			{
				BlockFace key = item.Key;
				if (key.Room == null)
				{
					AddFace(key, blockFace.Room);
					pending.TryAdd(key);
				}
				else
				{
					Merge(blockFace.Room, key.Room);
				}
			}
		}
	}

	private void Merge(Room a, Room b)
	{
		if (a != b)
		{
			Room room = ((a.Faces.Count >= b.Faces.Count) ? a : b);
			Room room2 = ((room == a) ? b : a);
			for (int i = 0; i < room2.Faces.Count; i++)
			{
				BlockFace blockFace = room2.Faces[i];
				blockFace.Room = room;
				room.Faces.Add(blockFace);
				Moved.TryAdd(blockFace);
			}
			room2.Faces.Clear();
			MarkChanged(room);
			RemoveRoom(room2);
			Retire(room2);
		}
	}

	private void SplitWhereDisconnected(ListHashSet<BlockFace> pending)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<Room, int> dictionary = Pool.Get<Dictionary<Room, int>>();
		List<BlockFace> list = Pool.Get<List<BlockFace>>();
		try
		{
			Enumerator<BlockFace> enumerator = pending.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BlockFace current = enumerator.Current;
					dictionary.TryGetValue(current.Room, out var value);
					dictionary[current.Room] = value + 1;
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			foreach (KeyValuePair<Room, int> item in dictionary)
			{
				if (item.Value < 2)
				{
					continue;
				}
				list.Clear();
				enumerator = pending.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						BlockFace current3 = enumerator.Current;
						if (current3.Room == item.Key)
						{
							list.Add(current3);
						}
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
				}
				Split(item.Key, list);
			}
		}
		finally
		{
			Pool.FreeUnmanaged<BlockFace>(ref list);
			Pool.FreeUnmanaged<Room, int>(ref dictionary);
		}
	}

	private void Split(Room room, List<BlockFace> seeds)
	{
		Dictionary<BlockFace, int> searchOf = Pool.Get<Dictionary<BlockFace, int>>();
		try
		{
			searchCount = 0;
			foreach (BlockFace seed in seeds)
			{
				Visit(seed, TakeSearch(), searchOf);
			}
			while (GrowSearches(searchOf))
			{
			}
			SplitOffFinishedPieces(room);
		}
		finally
		{
			Pool.FreeUnmanaged<BlockFace, int>(ref searchOf);
		}
	}

	private bool GrowSearches(Dictionary<BlockFace, int> searchOf)
	{
		int num = 0;
		for (int i = 0; i < searchCount; i++)
		{
			Search search = searches[i];
			if (search.Parent != i || search.Finished)
			{
				continue;
			}
			if (search.Frontier.Count == 0)
			{
				search.Finished = true;
				continue;
			}
			num++;
			BlockFace blockFace = search.Frontier[search.Frontier.Count - 1];
			search.Frontier.RemoveAt(search.Frontier.Count - 1);
			foreach (KeyValuePair<BlockFace, int> item in blockFace.LinksPerFace)
			{
				BlockFace key = item.Key;
				int num2 = Find(i);
				if (!searchOf.TryGetValue(key, out var value))
				{
					Visit(key, num2, searchOf);
					continue;
				}
				int num3 = Find(value);
				if (num3 != num2)
				{
					Join(num2, num3);
				}
			}
		}
		return num > 1;
	}

	private void SplitOffFinishedPieces(Room room)
	{
		int num = -1;
		for (int i = 0; i < searchCount; i++)
		{
			Search search = searches[i];
			if (search.Parent == i)
			{
				if (!search.Finished)
				{
					num = i;
					break;
				}
				if (num < 0 || search.Visited.Count > searches[num].Visited.Count)
				{
					num = i;
				}
			}
		}
		for (int j = 0; j < searchCount; j++)
		{
			Search search2 = searches[j];
			if (search2.Parent != j || j == num)
			{
				continue;
			}
			Room room2 = CreateRoom();
			foreach (BlockFace item in search2.Visited)
			{
				room.Faces.Remove(item);
				item.Room = room2;
				room2.Faces.Add(item);
				Moved.TryAdd(item);
			}
		}
	}

	private int TakeSearch()
	{
		if (searchCount == searches.Count)
		{
			searches.Add(new Search());
		}
		Search search = searches[searchCount];
		search.Parent = searchCount;
		search.Finished = false;
		search.Visited.Clear();
		search.Frontier.Clear();
		return searchCount++;
	}

	private void Visit(BlockFace face, int search, Dictionary<BlockFace, int> searchOf)
	{
		searchOf[face] = search;
		searches[search].Visited.Add(face);
		searches[search].Frontier.Add(face);
	}

	private int Find(int index)
	{
		while (searches[index].Parent != index)
		{
			Search search = searches[index];
			search.Parent = searches[search.Parent].Parent;
			index = search.Parent;
		}
		return index;
	}

	private void Join(int a, int b)
	{
		if (searches[a].Visited.Count < searches[b].Visited.Count)
		{
			int num = b;
			int num2 = a;
			a = num;
			b = num2;
		}
		Search search = searches[a];
		Search search2 = searches[b];
		search.Visited.AddRange(search2.Visited);
		search.Frontier.AddRange(search2.Frontier);
		search2.Visited.Clear();
		search2.Frontier.Clear();
		search2.Parent = a;
	}

	private void AddFace(BlockFace face, Room room)
	{
		face.Room = room;
		room.Faces.Add(face);
		Moved.TryAdd(face);
		MarkChanged(room);
	}

	private void MarkChanged(Room room)
	{
		if (!Created.Contains(room) && !Changed.Contains(room))
		{
			Changed.Add(room);
		}
	}

	private void Retire(Room room)
	{
		Changed.Remove(room);
		if (Created.Remove(room))
		{
			Pool.Free<Room>(ref room);
		}
		else
		{
			Dissolved.Add(room);
		}
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

	public void OnFaceRemoved(Room room)
	{
		if (room.Faces.Count == 0)
		{
			emptied.Add(room);
		}
	}

	private Room NewRoom()
	{
		Room room = Pool.Get<Room>();
		room.Id = nextRoomId++;
		room.Solver = this;
		room.SolverIndex = Rooms.Count;
		Rooms.Add(room);
		return room;
	}

	private void RemoveRoom(Room room)
	{
		int index = Rooms.Count - 1;
		Room room2 = Rooms[index];
		Rooms[room.SolverIndex] = room2;
		room2.SolverIndex = room.SolverIndex;
		Rooms.RemoveAt(index);
		room.SolverIndex = -1;
	}

	private Room CreateRoom()
	{
		Room room = NewRoom();
		Created.Add(room);
		return room;
	}

	private void FloodFrom(BlockFace seed)
	{
		Room room = NewRoom();
		floodStack.Clear();
		floodStack.Push(seed);
		seed.Room = room;
		room.Faces.Add(seed);
		while (floodStack.Count > 0)
		{
			foreach (KeyValuePair<BlockFace, int> item in floodStack.Pop().LinksPerFace)
			{
				BlockFace key = item.Key;
				if (key.Room == null)
				{
					key.Room = room;
					room.Faces.Add(key);
					floodStack.Push(key);
				}
			}
		}
	}
}
