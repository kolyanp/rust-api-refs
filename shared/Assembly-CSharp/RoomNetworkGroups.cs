using System;
using System.Collections.Generic;
using System.Text;
using ConVar;
using Development.Attributes;
using Facepunch;
using Network;
using Network.Visibility;
using UnityEngine;

[ResetStaticFields]
public class RoomNetworkGroups
{
	private struct Allocation
	{
		public int Index;

		public int LastSeenPartition;

		public ulong[] Anchors;
	}

	private struct RoomSpan
	{
		private ulong a;

		private ulong b;

		private List<ulong> extra;

		private bool failedOpen;

		public bool Add(Room room)
		{
			if (failedOpen)
			{
				return false;
			}
			if (room == null || room.EffectivelyOutside || room.Anchor == 0L)
			{
				failedOpen = true;
				return false;
			}
			if (a == 0L || room.Anchor == a)
			{
				a = room.Anchor;
				return true;
			}
			if (b == 0L || room.Anchor == b)
			{
				b = room.Anchor;
				return true;
			}
			if (extra == null)
			{
				extra = Pool.Get<List<ulong>>();
			}
			if (!extra.Contains(room.Anchor))
			{
				MultiRoomBlocks++;
				extra.Add(room.Anchor);
			}
			return true;
		}

		public int Resolve()
		{
			try
			{
				if (failedOpen)
				{
					return 0;
				}
				if (a == 0L)
				{
					return 0;
				}
				if (extra != null)
				{
					return BoundarySet(a, b, extra);
				}
				return Boundary(a, (b == 0L) ? a : b);
			}
			finally
			{
				if (extra != null)
				{
					Pool.FreeUnmanaged<ulong>(ref extra);
				}
			}
		}
	}

	public const int Grid = 0;

	private const int AnchorRetentionPartitions = 64;

	private static Dictionary<ulong, Allocation> contentsGroups = new Dictionary<ulong, Allocation>();

	private static Dictionary<(ulong, ulong), Allocation> boundaryGroups = new Dictionary<(ulong, ulong), Allocation>();

	private static Dictionary<string, Allocation> boundarySets = new Dictionary<string, Allocation>();

	private static Dictionary<ulong, List<int>> boundariesByAnchor = new Dictionary<ulong, List<int>>();

	private static int nextIndex = 1;

	private static bool exhaustionLogged;

	private static ListHashSet<ulong> liveAnchors = new ListHashSet<ulong>();

	public static int MultiRoomBlocks;

	public static int SpanningDeployables;

	public static int ContentsGroupCount => contentsGroups.Count;

	public static int BoundaryGroupCount => boundaryGroups.Count + boundarySets.Count;

	public static int AllocatedGroups => nextIndex - 1;

	public static bool IsEnabled
	{
		get
		{
			if (RoomOcclusion.occlusion >= 2 && RoomOcclusionManager.PartitionIsLive)
			{
				return !RoomOcclusionManager.FailedOpen;
			}
			return false;
		}
	}

	public static void Stamp(List<Room> rooms)
	{
		liveAnchors.Clear();
		foreach (Room room in rooms)
		{
			room.NetworkGroupIndex = 0;
			if (room.Anchor != 0L && !room.IsOutside)
			{
				liveAnchors.TryAdd(room.Anchor);
				int networkGroupIndex = Contents(room.Anchor);
				if (!room.EffectivelyOutside)
				{
					room.NetworkGroupIndex = networkGroupIndex;
				}
			}
		}
		Prune();
	}

	private static int GetOrMint<TKey>(Dictionary<TKey, Allocation> table, TKey key, List<ulong> anchors, out bool minted)
	{
		if (table.TryGetValue(key, out var value))
		{
			value.LastSeenPartition = RoomOcclusionManager.PartitionVersion;
			table[key] = value;
			minted = false;
			return value.Index;
		}
		int num = Mint();
		if (num == 0)
		{
			minted = false;
			return 0;
		}
		table[key] = new Allocation
		{
			Index = num,
			LastSeenPartition = RoomOcclusionManager.PartitionVersion,
			Anchors = anchors?.ToArray()
		};
		minted = true;
		return num;
	}

	private static int Contents(ulong anchor)
	{
		if (anchor == 0L)
		{
			return 0;
		}
		bool minted;
		return GetOrMint(contentsGroups, anchor, null, out minted);
	}

	private static int Boundary(ulong a, ulong b)
	{
		if (a == 0L || b == 0L)
		{
			return 0;
		}
		if (a == b)
		{
			return Contents(a);
		}
		(ulong, ulong) key = ((a < b) ? (a, b) : (b, a));
		int orMint = GetOrMint(boundaryGroups, key, null, out var minted);
		if (minted)
		{
			Link(key.Item1, orMint);
			Link(key.Item2, orMint);
		}
		return orMint;
	}

	private static int BoundarySet(ulong a, ulong b, List<ulong> extra)
	{
		List<ulong> list = Pool.Get<List<ulong>>();
		try
		{
			if (a != 0L)
			{
				list.Add(a);
			}
			if (b != 0L)
			{
				list.Add(b);
			}
			list.AddRange(extra);
			list.Sort();
			StringBuilder stringBuilder = new StringBuilder(list.Count * 17);
			foreach (ulong item in list)
			{
				stringBuilder.Append(item).Append('|');
			}
			string key = stringBuilder.ToString();
			int orMint = GetOrMint(boundarySets, key, list, out var minted);
			if (minted)
			{
				foreach (ulong item2 in list)
				{
					Link(item2, orMint);
				}
			}
			return orMint;
		}
		finally
		{
			Pool.FreeUnmanaged<ulong>(ref list);
		}
	}

	private static void Link(ulong anchor, int index)
	{
		if (!boundariesByAnchor.TryGetValue(anchor, out var value))
		{
			value = new List<int>();
			boundariesByAnchor[anchor] = value;
		}
		value.Add(index);
	}

	private static void Unlink(ulong anchor, int index)
	{
		if (boundariesByAnchor.TryGetValue(anchor, out var value))
		{
			value.Remove(index);
			if (value.Count == 0)
			{
				boundariesByAnchor.Remove(anchor);
			}
		}
	}

	private static int Mint()
	{
		if (nextIndex >= 6553600)
		{
			if (!exhaustionLogged)
			{
				exhaustionLogged = true;
				Debug.LogError((object)"Interior occlusion ran out of room network group ids - everything from here stays on the grid");
			}
			return 0;
		}
		return nextIndex++;
	}

	private static void Prune()
	{
		PruneContents();
		PruneBoundaries();
		PruneBoundarySets();
	}

	private static void Prune<TKey>(Dictionary<TKey, Allocation> table, Func<TKey, Allocation, bool> isLive, Action<TKey, Allocation> unlink)
	{
		if (table.Count == 0)
		{
			return;
		}
		List<TKey> list = Pool.Get<List<TKey>>();
		try
		{
			foreach (KeyValuePair<TKey, Allocation> item in table)
			{
				if (!isLive(item.Key, item.Value) && RoomOcclusionManager.PartitionVersion - item.Value.LastSeenPartition >= 64)
				{
					list.Add(item.Key);
				}
			}
			foreach (TKey item2 in list)
			{
				unlink?.Invoke(item2, table[item2]);
				table.Remove(item2);
			}
		}
		finally
		{
			Pool.FreeUnmanaged<TKey>(ref list);
		}
	}

	private static void PruneContents()
	{
		Prune(contentsGroups, (ulong anchor, Allocation _) => liveAnchors.Contains(anchor), null);
	}

	private static void PruneBoundaries()
	{
		Prune(boundaryGroups, ((ulong, ulong) key, Allocation _) => liveAnchors.Contains(key.Item1) && liveAnchors.Contains(key.Item2), ((ulong, ulong) key, Allocation allocation) =>
		{
			Unlink(key.Item1, allocation.Index);
			Unlink(key.Item2, allocation.Index);
		});
	}

	private static void PruneBoundarySets()
	{
		Prune(boundarySets, (string _, Allocation allocation) =>
		{
			ulong[] anchors = allocation.Anchors;
			foreach (ulong num in anchors)
			{
				if (!liveAnchors.Contains(num))
				{
					return false;
				}
			}
			return true;
		}, (string _, Allocation allocation) =>
		{
			ulong[] anchors = allocation.Anchors;
			for (int i = 0; i < anchors.Length; i++)
			{
				Unlink(anchors[i], allocation.Index);
			}
		});
	}

	public static int TargetGroup(BaseEntity entity)
	{
		if (!IsEnabled)
		{
			return 0;
		}
		if ((Object)(object)entity == (Object)null || entity.IsDestroyed)
		{
			return 0;
		}
		if (!entity.SupportsRoomOcclusion())
		{
			return 0;
		}
		if (entity is BuildingBlock)
		{
			return GroupForBlock(entity.faces);
		}
		if (!DeployablesEnabled(entity))
		{
			return 0;
		}
		BaseEntity baseEntity = RoomEntityBridge.FindPortalHost(entity);
		if ((Object)(object)baseEntity != (Object)null)
		{
			return GroupForBlock(baseEntity.faces);
		}
		List<Room> list = Pool.Get<List<Room>>();
		try
		{
			RoomEntityBridge.GetRoomsForDeployable(entity, list);
			return GroupForDeployable(list);
		}
		finally
		{
			Pool.FreeUnmanaged<Room>(ref list);
		}
	}

	private static bool DeployablesEnabled(BaseEntity entity)
	{
		int occlusion_deployables = RoomOcclusion.occlusion_deployables;
		if (occlusion_deployables <= 0)
		{
			return false;
		}
		if (entity is IOEntity)
		{
			return occlusion_deployables >= 2;
		}
		return true;
	}

	public static int GroupForBlock(List<BlockFace> faces)
	{
		if (faces == null || faces.Count == 0)
		{
			return 0;
		}
		RoomSpan roomSpan = default;
		foreach (BlockFace face in faces)
		{
			if (!roomSpan.Add(face.Room))
			{
				break;
			}
		}
		return roomSpan.Resolve();
	}

	public static int GroupForDeployable(List<Room> rooms)
	{
		if (rooms == null || rooms.Count == 0)
		{
			return 0;
		}
		RoomSpan roomSpan = default;
		foreach (Room room in rooms)
		{
			if (!roomSpan.Add(room))
			{
				break;
			}
		}
		return roomSpan.Resolve();
	}

	public static int GroupForDeployable(Room room)
	{
		if (room == null || room.EffectivelyOutside || room.Anchor == 0L)
		{
			return 0;
		}
		return Contents(room.Anchor);
	}

	public static void ReleaseAll(List<Room> rooms)
	{
		if (rooms == null)
		{
			return;
		}
		foreach (Room room in rooms)
		{
			room.NetworkGroupIndex = 0;
		}
	}

	public static Group Resolve(int index)
	{
		if (index == 0)
		{
			return null;
		}
		return Net.sv?.visibility?.Get(NetworkVisibilityGrid.RoomGroupId(index));
	}

	public static Group GroupFor(BaseEntity entity)
	{
		return Resolve(TargetGroup(entity));
	}

	public static bool IsRoomGroup(Group group)
	{
		if (group != null)
		{
			return NetworkVisibilityGrid.IsRoomGroupId(group.ID);
		}
		return false;
	}

	public static void GatherVisibleGroups(Room bodyRoom, Room remoteRoom, ListHashSet<Group> into)
	{
		ListHashSet<int> val = Pool.Get<ListHashSet<int>>();
		try
		{
			GatherVisibleIndices(bodyRoom, remoteRoom, val);
			Resolve(val, into);
		}
		finally
		{
			Pool.FreeUnmanaged<int>(ref val);
		}
	}

	public static void GatherVisibleIndices(Room bodyRoom, Room remoteRoom, ListHashSet<int> into)
	{
		AddComponent(bodyRoom, into);
		AddComponent(remoteRoom, into);
	}

	private static void AddComponent(Room viewer, ListHashSet<int> into)
	{
		if (viewer == null)
		{
			return;
		}
		List<Room> list = RoomVisibility.ComponentOf(viewer);
		if (list == null)
		{
			AddRoom(viewer, into);
			return;
		}
		foreach (Room item in list)
		{
			AddRoom(item, into);
		}
	}

	public static void GatherGroupsNear(Vector3 position, float range, ListHashSet<Group> into)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (range <= 0f)
		{
			return;
		}
		RoomSpatialIndex roomIndex = RoomOcclusionManager.RoomIndex;
		if (roomIndex == null)
		{
			return;
		}
		ListHashSet<Room> val = Pool.Get<ListHashSet<Room>>();
		ListHashSet<int> val2 = Pool.Get<ListHashSet<int>>();
		try
		{
			roomIndex.Gather(position, range, val);
			GatherNearbyIndices((IEnumerable<Room>)val, position, range, val2);
			Resolve(val2, into);
		}
		finally
		{
			Pool.FreeUnmanaged<Room>(ref val);
			Pool.FreeUnmanaged<int>(ref val2);
		}
	}

	public static void GatherNearbyIndices(IEnumerable<Room> rooms, Vector3 position, float range, ListHashSet<int> into)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (rooms == null || range <= 0f)
		{
			return;
		}
		float num = range * range;
		foreach (Room room in rooms)
		{
			if (!(room.Bounds.SqrDistance(position) > num))
			{
				AddRoom(room, into);
			}
		}
	}

	private static void AddRoom(Room room, ListHashSet<int> into)
	{
		Add(room.NetworkGroupIndex, into);
		AddBoundaries(room.Anchor, into);
	}

	private static void AddBoundaries(ulong anchor, ListHashSet<int> into)
	{
		if (anchor == 0L || !boundariesByAnchor.TryGetValue(anchor, out var value))
		{
			return;
		}
		foreach (int item in value)
		{
			Add(item, into);
		}
	}

	private static void Add(int index, ListHashSet<int> into)
	{
		if (index != 0)
		{
			into.TryAdd(index);
		}
	}

	private static void Resolve(ListHashSet<int> indices, ListHashSet<Group> into)
	{
		for (int i = 0; i < indices.Count; i++)
		{
			Group obj = Resolve(indices[i]);
			if (obj != null)
			{
				into.TryAdd(obj);
			}
		}
	}

	public static void Shutdown()
	{
		contentsGroups.Clear();
		boundaryGroups.Clear();
		boundarySets.Clear();
		boundariesByAnchor.Clear();
		liveAnchors.Clear();
		nextIndex = 1;
		exhaustionLogged = false;
		MultiRoomBlocks = 0;
		SpanningDeployables = 0;
	}
}
