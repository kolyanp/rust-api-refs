using System.Collections.Generic;
using Development.Attributes;
using Facepunch;

[ResetStaticFields]
public class RoomVisibility
{
	private static List<List<Room>> components = new List<List<Room>>();

	public static void Rebuild(List<Room> rooms, List<Room> changed = null)
	{
		Clear();
		foreach (Room room3 in rooms)
		{
			room3.VisibilityGroup = 0;
		}
		List<Room> list = Pool.Get<List<Room>>();
		try
		{
			int num = 1;
			foreach (Room room4 in rooms)
			{
				if (room4.VisibilityGroup != 0)
				{
					continue;
				}
				int visibilityGroup = num++;
				bool flag = false;
				List<Room> list2 = Pool.Get<List<Room>>();
				components.Add(list2);
				list.Clear();
				room4.VisibilityGroup = visibilityGroup;
				list.Add(room4);
				while (list.Count > 0)
				{
					Room room = list[list.Count - 1];
					list.RemoveAt(list.Count - 1);
					list2.Add(room);
					flag |= room.IsOutside;
					foreach (Portal portal in room.Portals)
					{
						if (portal.IsOpen())
						{
							Room room2 = portal.OtherRoom(room);
							if (room2 != null && room2.VisibilityGroup == 0)
							{
								room2.VisibilityGroup = visibilityGroup;
								list.Add(room2);
							}
						}
					}
				}
				foreach (Room item in list2)
				{
					bool effectivelyOutside = item.EffectivelyOutside;
					item.EffectivelyOutside = flag;
					if (changed != null && effectivelyOutside != flag)
					{
						changed.Add(item);
					}
				}
			}
		}
		finally
		{
			Pool.FreeUnmanaged<Room>(ref list);
		}
	}

	public static void FailOpen(List<Room> rooms)
	{
		Clear();
		if (rooms == null)
		{
			return;
		}
		List<Room> list = Pool.Get<List<Room>>();
		components.Add(list);
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
		if (num < 0 || num >= components.Count)
		{
			return null;
		}
		return components[num];
	}

	public static void Clear()
	{
		foreach (List<Room> component in components)
		{
			List<Room> current = component;
			Pool.FreeUnmanaged<Room>(ref current);
		}
		components.Clear();
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
