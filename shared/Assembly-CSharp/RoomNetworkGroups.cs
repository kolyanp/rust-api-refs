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

	private struct BoundaryLink
	{
		public int Index;

		public ulong Other;

		public string Set;
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

	private static Dictionary<ulong, List<BoundaryLink>> boundariesByAnchor = new Dictionary<ulong, List<BoundaryLink>>();

	private static int nextIndex = 1;

	private static bool exhaustionLogged;

	private static HashSet<ulong> liveAnchors = new HashSet<ulong>();

	private static ListHashSet<ulong> unclaimed = new ListHashSet<ulong>();

	public static int MultiRoomBlocks;

	public static int SpanningDeployables;

	private static List<Bounds> regrouped = new List<Bounds>();

	private static int regroupedBase;

	private const int MaxRegrouped = 1024;

	public static int Version { get; private set; }

	public static int Regrouped => regroupedBase + regrouped.Count;

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

	public static void Changed()
	{
		Version++;
	}

	public static bool RegroupedNear(Vector3 position, float reach, int since)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (since < regroupedBase)
		{
			return true;
		}
		for (int i = since - regroupedBase; i < regrouped.Count; i++)
		{
			Bounds val = regrouped[i];
			if (val.SqrDistance(position) <= reach * reach)
			{
				return true;
			}
		}
		return false;
	}

	private static void LogRegrouped(Room room)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		OnRegionChanged(room.Bounds);
	}

	public static void OnRegionChanged(Bounds bounds)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (regrouped.Count >= 1024)
		{
			int num = regrouped.Count / 2;
			regrouped.RemoveRange(0, num);
			regroupedBase += num;
		}
		regrouped.Add(bounds);
	}

	public static void Stamp(List<Room> rooms)
	{
		HashSet<ulong> hashSet = liveAnchors;
		liveAnchors = Pool.Get<HashSet<ulong>>();
		foreach (Room room in rooms)
		{
			Claim(room);
		}
		foreach (ulong item in hashSet)
		{
			if (!liveAnchors.Contains(item))
			{
				Release(item);
			}
		}
		Pool.FreeUnmanaged<ulong>(ref hashSet);
		Prune();
	}

	public static void Restamp(List<Room> rooms)
	{
		foreach (Room room in rooms)
		{
			if (room.ClaimedAnchor != 0L && room.ClaimedAnchor != Claimable(room))
			{
				liveAnchors.Remove(room.ClaimedAnchor);
				Release(room.ClaimedAnchor);
				room.ClaimedAnchor = 0uL;
			}
		}
		foreach (Room room2 in rooms)
		{
			Claim(room2);
		}
		Prune();
	}

	private static void Claim(Room room)
	{
		room.ClaimedAnchor = (Assign(room) ? room.Anchor : 0);
		if (room.ClaimedAnchor != 0L)
		{
			liveAnchors.Add(room.ClaimedAnchor);
		}
	}

	private static void Release(ulong anchor)
	{
		unclaimed.TryAdd(anchor);
		if (contentsGroups.TryGetValue(anchor, out var value))
		{
			value.LastSeenPartition = RoomOcclusionManager.PartitionVersion;
			contentsGroups[anchor] = value;
		}
	}

	private static void Track(ulong anchor)
	{
		if (!liveAnchors.Contains(anchor))
		{
			unclaimed.TryAdd(anchor);
		}
	}

	private static ulong Claimable(Room room)
	{
		if (!room.IsOutside && room.Faces.Count != 0)
		{
			return room.Anchor;
		}
		return 0uL;
	}

	private static bool Assign(Room room)
	{
		int networkGroupIndex = room.NetworkGroupIndex;
		room.NetworkGroupIndex = 0;
		ulong num = Claimable(room);
		if (num != 0L)
		{
			int networkGroupIndex2 = Contents(num);
			if (!room.EffectivelyOutside)
			{
				room.NetworkGroupIndex = networkGroupIndex2;
			}
		}
		if (room.NetworkGroupIndex != networkGroupIndex)
		{
			LogRegrouped(room);
		}
		return num != 0;
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
		int orMint = GetOrMint(contentsGroups, anchor, null, out var minted);
		if (minted)
		{
			Track(anchor);
		}
		return orMint;
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
		(ulong, ulong) tuple = ((a < b) ? (a, b) : (b, a));
		int orMint = GetOrMint(boundaryGroups, tuple, null, out var minted);
		if (minted)
		{
			Link(tuple.Item1, new BoundaryLink
			{
				Index = orMint,
				Other = tuple.Item2
			});
			ulong item = tuple.Item2;
			BoundaryLink link = new BoundaryLink
			{
				Index = orMint
			};
			(link.Other, _) = tuple;
			Link(item, link);
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
			string text = stringBuilder.ToString();
			int orMint = GetOrMint(boundarySets, text, list, out var minted);
			if (minted)
			{
				foreach (ulong item2 in list)
				{
					Link(item2, new BoundaryLink
					{
						Index = orMint,
						Set = text
					});
				}
			}
			return orMint;
		}
		finally
		{
			Pool.FreeUnmanaged<ulong>(ref list);
		}
	}

	private static void Link(ulong anchor, BoundaryLink link)
	{
		if (!boundariesByAnchor.TryGetValue(anchor, out var value))
		{
			value = new List<BoundaryLink>();
			boundariesByAnchor[anchor] = value;
		}
		value.Add(link);
		Track(anchor);
		Version++;
	}

	private static void Unlink(ulong anchor, int index)
	{
		if (!boundariesByAnchor.TryGetValue(anchor, out var value))
		{
			return;
		}
		for (int i = 0; i < value.Count; i++)
		{
			if (value[i].Index == index)
			{
				value.RemoveAt(i);
				break;
			}
		}
		if (value.Count == 0)
		{
			boundariesByAnchor.Remove(anchor);
		}
		Version++;
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
		for (int num = unclaimed.Count - 1; num >= 0; num--)
		{
			ulong num2 = unclaimed[num];
			if (liveAnchors.Contains(num2) || PruneAnchor(num2))
			{
				unclaimed.RemoveAt(num);
			}
		}
	}

	private static bool PruneAnchor(ulong anchor)
	{
		if (contentsGroups.TryGetValue(anchor, out var value) && IsStale(value))
		{
			contentsGroups.Remove(anchor);
		}
		if (boundariesByAnchor.TryGetValue(anchor, out var value2))
		{
			for (int num = value2.Count - 1; num >= 0; num--)
			{
				BoundaryLink boundaryLink = value2[num];
				if (boundaryLink.Set != null)
				{
					Allocation allocation = boundarySets[boundaryLink.Set];
					if (IsStale(allocation))
					{
						boundarySets.Remove(boundaryLink.Set);
						ulong[] anchors = allocation.Anchors;
						for (int i = 0; i < anchors.Length; i++)
						{
							Unlink(anchors[i], boundaryLink.Index);
						}
					}
				}
				else
				{
					(ulong, ulong) key = ((anchor < boundaryLink.Other) ? (anchor, boundaryLink.Other) : (boundaryLink.Other, anchor));
					if (IsStale(boundaryGroups[key]))
					{
						boundaryGroups.Remove(key);
						Unlink(key.Item1, boundaryLink.Index);
						Unlink(key.Item2, boundaryLink.Index);
					}
				}
			}
		}
		if (!contentsGroups.ContainsKey(anchor))
		{
			return !boundariesByAnchor.ContainsKey(anchor);
		}
		return false;
	}

	private static bool IsStale(Allocation allocation)
	{
		return RoomOcclusionManager.PartitionVersion - allocation.LastSeenPartition >= 64;
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
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		GatherGroupsNear(position, range, into, out var _);
	}

	public static void GatherGroupsNear(Vector3 position, float range, ListHashSet<Group> into, out float margin)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		margin = float.PositiveInfinity;
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
			margin = RoomSpatialIndex.Slack(position, range);
			GatherNearbyIndices((IList<Room>)val, position, range, val2, ref margin);
			Resolve(val2, into);
		}
		finally
		{
			Pool.FreeUnmanaged<Room>(ref val);
			Pool.FreeUnmanaged<int>(ref val2);
		}
	}

	public static void GatherNearbyIndices(IList<Room> rooms, Vector3 position, float range, ListHashSet<int> into)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		float margin = float.PositiveInfinity;
		GatherNearbyIndices(rooms, position, range, into, ref margin);
	}

	public static void GatherNearbyIndices(IList<Room> rooms, Vector3 position, float range, ListHashSet<int> into, ref float margin)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (rooms == null || range <= 0f)
		{
			return;
		}
		float num = range * range;
		for (int i = 0; i < rooms.Count; i++)
		{
			Room room = rooms[i];
			float num2 = room.Bounds.SqrDistance(position);
			if (room.NetworkGroupIndex != 0 || (room.Anchor != 0L && boundariesByAnchor.ContainsKey(room.Anchor)))
			{
				margin = Mathf.Min(margin, Mathf.Abs(Mathf.Sqrt(num2) - range));
			}
			if (!(num2 > num))
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
		foreach (BoundaryLink item in value)
		{
			Add(item.Index, into);
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
		unclaimed.Clear();
		regrouped.Clear();
		regroupedBase = 0;
		nextIndex = 1;
		exhaustionLogged = false;
		Version++;
		MultiRoomBlocks = 0;
		SpanningDeployables = 0;
	}
}
