using System.Collections.Generic;
using Development.Attributes;
using Facepunch;

[ResetStaticFields]
public class RoomVisibility
{
	private static List<List<Room>> roomsConnected = new List<List<Room>>();

	private static List<int> freeGroups = new List<int>();

	private static List<int> stamps = new List<int>();

	private static int nextStamp;

	public static void Rebuild(List<Room> rooms, List<Room> changed = null)
	{
		Clear();
		foreach (Room room in rooms)
		{
			room.VisibilityGroup = 0;
		}
		List<Room> stack = Pool.Get<List<Room>>();
		try
		{
			foreach (Room room2 in rooms)
			{
				if (room2.VisibilityGroup == 0)
				{
					Flood(room2, stack, changed);
				}
			}
		}
		finally
		{
			Pool.FreeUnmanaged<Room>(ref stack);
		}
	}

	public static void Refresh(List<Room> seeds, IList<Portal> portals, List<Room> changed = null)
	{
		List<Room> list = Pool.Get<List<Room>>();
		List<Room> stack = Pool.Get<List<Room>>();
		try
		{
			foreach (Room seed in seeds)
			{
				Release(seed, list);
			}
			for (int i = 0; i < portals.Count; i++)
			{
				Portal portal = portals[i];
				if (portal.RoomA != null && !portal.IsOpen())
				{
					Release(portal.RoomA, list);
					Release(portal.RoomB, list);
				}
			}
			foreach (Room item in list)
			{
				if (item.VisibilityGroup == 0 && item.Faces.Count != 0)
				{
					Flood(item, stack, changed);
				}
			}
			for (int j = 0; j < portals.Count; j++)
			{
				Portal portal2 = portals[j];
				if (portal2.RoomA != null && portal2.IsOpen())
				{
					Merge(portal2.RoomA, portal2.RoomB, changed);
				}
			}
		}
		finally
		{
			Pool.FreeUnmanaged<Room>(ref list);
			Pool.FreeUnmanaged<Room>(ref stack);
		}
	}

	private static void Flood(Room start, List<Room> stack, List<Room> changed)
	{
		List<Room> list = Allocate(out var group);
		stack.Clear();
		start.VisibilityGroup = group;
		stack.Add(start);
		while (stack.Count > 0)
		{
			Room room = stack[stack.Count - 1];
			stack.RemoveAt(stack.Count - 1);
			list.Add(room);
			Touch(group);
			foreach (Portal portal in room.Portals)
			{
				if (!portal.IsOpen())
				{
					continue;
				}
				Room room2 = portal.OtherRoom(room);
				if (room2 != null && room2.VisibilityGroup != group)
				{
					if (ComponentOf(room2) != null)
					{
						Join(list, group, room2.VisibilityGroup);
						continue;
					}
					room2.VisibilityGroup = group;
					stack.Add(room2);
				}
			}
		}
		SetSeesOutside(list, changed);
	}

	private static void Merge(Room a, Room b, List<Room> changed)
	{
		List<Room> list = ComponentOf(a);
		List<Room> list2 = ComponentOf(b);
		if (list != null && list2 != null && list != list2)
		{
			if (list.Count < list2.Count)
			{
				Room room = b;
				Room room2 = a;
				List<Room> list3 = list2;
				a = room;
				b = room2;
				list = list3;
			}
			Join(list, a.VisibilityGroup, b.VisibilityGroup);
			SetSeesOutside(list, changed);
		}
	}

	private static void Join(List<Room> component, int group, int from)
	{
		foreach (Room item in roomsConnected[from - 1])
		{
			item.VisibilityGroup = group;
			component.Add(item);
		}
		Touch(group);
		Free(from);
	}

	private static void Touch(int group)
	{
		stamps[group - 1] = ++nextStamp;
	}

	public static int StampOf(Room room)
	{
		if (room == null)
		{
			return 0;
		}
		int num = room.VisibilityGroup - 1;
		if (num < 0 || num >= stamps.Count)
		{
			return 0;
		}
		return stamps[num];
	}

	private static void SetSeesOutside(List<Room> component, List<Room> changed)
	{
		bool flag = false;
		foreach (Room item in component)
		{
			flag |= item.IsOutside;
		}
		foreach (Room item2 in component)
		{
			bool effectivelyOutside = item2.EffectivelyOutside;
			item2.EffectivelyOutside = flag;
			if (changed != null && effectivelyOutside != flag)
			{
				changed.Add(item2);
			}
		}
	}

	private static void Release(Room room, List<Room> pending)
	{
		List<Room> list = ComponentOf(room);
		if (list == null)
		{
			room.VisibilityGroup = 0;
			pending.Add(room);
			return;
		}
		int visibilityGroup = room.VisibilityGroup;
		foreach (Room item in list)
		{
			item.VisibilityGroup = 0;
			pending.Add(item);
		}
		Free(visibilityGroup);
	}

	private static List<Room> Allocate(out int group)
	{
		if (freeGroups.Count > 0)
		{
			group = freeGroups[freeGroups.Count - 1];
			freeGroups.RemoveAt(freeGroups.Count - 1);
		}
		else
		{
			roomsConnected.Add(null);
			stamps.Add(0);
			group = roomsConnected.Count;
		}
		List<Room> list = Pool.Get<List<Room>>();
		roomsConnected[group - 1] = list;
		Touch(group);
		return list;
	}

	private static void Free(int group)
	{
		List<Room> list = roomsConnected[group - 1];
		Pool.FreeUnmanaged<Room>(ref list);
		roomsConnected[group - 1] = null;
		freeGroups.Add(group);
		Touch(group);
	}

	public static void FailOpen(List<Room> rooms)
	{
		Clear();
		if (rooms == null)
		{
			return;
		}
		List<Room> list = Pool.Get<List<Room>>();
		roomsConnected.Add(list);
		stamps.Add(++nextStamp);
		foreach (Room room in rooms)
		{
			room.VisibilityGroup = 1;
			room.EffectivelyOutside = true;
			list.Add(room);
		}
	}

	public static List<Room> ComponentOf(Room room)
	{
		if (room == null)
		{
			return null;
		}
		int num = room.VisibilityGroup - 1;
		if (num < 0 || num >= roomsConnected.Count)
		{
			return null;
		}
		return roomsConnected[num];
	}

	public static void Clear()
	{
		foreach (List<Room> item in roomsConnected)
		{
			if (item != null)
			{
				List<Room> list = item;
				Pool.FreeUnmanaged<Room>(ref list);
			}
		}
		roomsConnected.Clear();
		freeGroups.Clear();
		stamps.Clear();
	}

	public static bool CanSee(Room viewer, Room target)
	{
		if (viewer == null || target == null)
		{
			return true;
		}
		return viewer.VisibilityGroup == target.VisibilityGroup;
	}

	public static bool CanSeeFrom(Room viewerRoom, Room targetRoom)
	{
		if (targetRoom == null)
		{
			return true;
		}
		if (targetRoom.EffectivelyOutside)
		{
			return true;
		}
		if (viewerRoom == null)
		{
			return false;
		}
		return viewerRoom.VisibilityGroup == targetRoom.VisibilityGroup;
	}
}
