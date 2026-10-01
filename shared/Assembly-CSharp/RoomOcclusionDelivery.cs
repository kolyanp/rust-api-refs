using System;
using System.Collections.Generic;
using ConVar;
using Development.Attributes;
using Facepunch;
using Network;
using Network.Visibility;
using UnityEngine;

[ResetStaticFields]
public class RoomOcclusionDelivery
{
	private static ListHashSet<BasePlayer> tracked = new ListHashSet<BasePlayer>();

	private static Dictionary<BasePlayer, Dictionary<Group, float>> lastVisibleByPlayer = new Dictionary<BasePlayer, Dictionary<Group, float>>();

	public const int Off = 0;

	public const int Shadow = 1;

	public const int Enforce = 2;

	private static ListHashSet<BaseEntity> grouped = new ListHashSet<BaseEntity>();

	private static int sweepCursor;

	private const int SweepPerFrame = 64;

	private static int deliveryLevel;

	private static ListHashSet<Room> dirtyRooms = new ListHashSet<Room>();

	private static List<Bounds> dirtyRegions = new List<Bounds>();

	private static bool assignAll = true;

	private static bool failed;

	private static int failedLevel;

	private static bool releasing;

	private static bool inCycle;

	public static bool Failed => failed;

	public static int GroupedEntities => grouped.Count;

	public static bool InCycle => inCycle;

	public static bool IsEnforcing
	{
		get
		{
			if (deliveryLevel >= 2 && !failed)
			{
				return !releasing;
			}
			return false;
		}
	}

	private static void RebuildAll()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RoomOcclusion.RebuildPlayers"))
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			tracked.Clear();
			Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BasePlayer current = enumerator.Current;
					if (!((Object)(object)current == (Object)null) && !current.IsDestroyed && current.IsConnected)
					{
						tracked.TryAdd(current);
						Rebuild(current, realtimeSinceStartup);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			enumerator = BasePlayer.activePlayerList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BasePlayer current2 = enumerator.Current;
					if (!((Object)(object)current2 == (Object)null) && current2.net?.roomGroups != null && !tracked.Contains(current2))
					{
						Clear(current2);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static void Rebuild(BasePlayer player, float now)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		ListHashSet<Group> val = Pool.Get<ListHashSet<Group>>();
		ListHashSet<Group> val2 = Pool.Get<ListHashSet<Group>>();
		try
		{
			if (SeesEverything(player))
			{
				RoomNetworkGroups.GatherGroupsNear(((Component)player).transform.position, RoomOcclusion.occlusion_range, val);
			}
			else
			{
				RoomNetworkGroups.GatherVisibleGroups(RoomEntityBridge.GetRoomForPlayer(player), null, val);
				GatherNearbyGroups(player, val);
			}
			GatherRemoteGroups(player, val);
			ApplyLinger(player, val, now, val2);
			if (!Same(player.net.roomGroups, val2))
			{
				ListHashSet<Group> roomGroups = player.net.roomGroups;
				player.net.roomGroups = val2;
				val2 = null;
				if (roomGroups != null)
				{
					Pool.FreeUnmanaged<Group>(ref roomGroups);
				}
				player.net.SetUpdateSubscriptions(shouldUpdate: true);
				player.net.UpdateSubscriptions(0, int.MaxValue);
			}
		}
		finally
		{
			if (val2 != null)
			{
				Pool.FreeUnmanaged<Group>(ref val2);
			}
			Pool.FreeUnmanaged<Group>(ref val);
		}
	}

	private static void GatherNearbyGroups(BasePlayer player, ListHashSet<Group> into)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		float occlusion_nearby_range = RoomOcclusion.occlusion_nearby_range;
		if (!(occlusion_nearby_range <= 0f))
		{
			RoomNetworkGroups.GatherGroupsNear(((Component)player).transform.position, occlusion_nearby_range, into);
		}
	}

	private static void ApplyLinger(BasePlayer player, ListHashSet<Group> visible, float now, ListHashSet<Group> into)
	{
		float occlusion_linger = RoomOcclusion.occlusion_linger;
		if (occlusion_linger <= 0f)
		{
			for (int i = 0; i < visible.Count; i++)
			{
				into.TryAdd(visible[i]);
			}
			ClearLingerClock(player);
			return;
		}
		if (!lastVisibleByPlayer.TryGetValue(player, out var value))
		{
			value = Pool.Get<Dictionary<Group, float>>();
			lastVisibleByPlayer[player] = value;
		}
		ApplyLinger(visible, value, now, occlusion_linger, into);
	}

	public static void ApplyLinger(ListHashSet<Group> visible, Dictionary<Group, float> lastVisible, float now, float linger, ListHashSet<Group> into)
	{
		for (int i = 0; i < visible.Count; i++)
		{
			into.TryAdd(visible[i]);
		}
		List<Group> list = Pool.Get<List<Group>>();
		try
		{
			foreach (KeyValuePair<Group, float> item in lastVisible)
			{
				if (!visible.Contains(item.Key))
				{
					if (now - item.Value < linger)
					{
						into.TryAdd(item.Key);
					}
					else
					{
						list.Add(item.Key);
					}
				}
			}
			foreach (Group item2 in list)
			{
				lastVisible.Remove(item2);
			}
			for (int j = 0; j < visible.Count; j++)
			{
				lastVisible[visible[j]] = now;
			}
		}
		finally
		{
			Pool.FreeUnmanaged<Group>(ref list);
		}
	}

	private static bool Same(ListHashSet<Group> current, ListHashSet<Group> next)
	{
		if (current == null)
		{
			return next.Count == 0;
		}
		if (current.Count != next.Count)
		{
			return false;
		}
		for (int i = 0; i < next.Count; i++)
		{
			if (!current.Contains(next[i]))
			{
				return false;
			}
		}
		return true;
	}

	private static bool SeesEverything(BasePlayer player)
	{
		if (player.IsSpectating())
		{
			return true;
		}
		if (player.isInvisible)
		{
			return true;
		}
		if (RoomOcclusion.occlusion_admin_bypass && (player.IsAdmin || player.IsDeveloper))
		{
			return true;
		}
		return false;
	}

	private static void GatherRemoteGroups(BasePlayer player, ListHashSet<Group> into)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (player.GetMounted() is ComputerStation computerStation)
		{
			BaseEntity baseEntity = computerStation.currentlyControllingEnt.Get(serverside: true);
			if (!((Object)(object)baseEntity == (Object)null) && !baseEntity.IsDestroyed)
			{
				RoomNetworkGroups.GatherGroupsNear(((Component)baseEntity).transform.position, RoomOcclusion.occlusion_range, into);
			}
		}
	}

	private static void Clear(BasePlayer player)
	{
		ClearLingerClock(player);
		Networkable net = player.net;
		if (net?.roomGroups != null)
		{
			ListHashSet<Group> roomGroups = net.roomGroups;
			net.roomGroups = null;
			Pool.FreeUnmanaged<Group>(ref roomGroups);
			net.SetUpdateSubscriptions(shouldUpdate: true);
		}
	}

	private static void ClearLingerClock(BasePlayer player)
	{
		if (lastVisibleByPlayer.TryGetValue(player, out var value))
		{
			lastVisibleByPlayer.Remove(player);
			Pool.FreeUnmanaged<Group, float>(ref value);
		}
	}

	private static void ClearAllLingerClocks()
	{
		foreach (KeyValuePair<BasePlayer, Dictionary<Group, float>> item in lastVisibleByPlayer)
		{
			Dictionary<Group, float> value = item.Value;
			Pool.FreeUnmanaged<Group, float>(ref value);
		}
		lastVisibleByPlayer.Clear();
	}

	public static void ForgetPlayer(BasePlayer player)
	{
		Clear(player);
		tracked.Remove(player);
	}

	public static bool TryGetRoomGroup(BaseEntity entity, out Group group)
	{
		group = null;
		if (!IsEnforcing)
		{
			return false;
		}
		group = RoomNetworkGroups.GroupFor(entity);
		if (group == null)
		{
			return false;
		}
		grouped.TryAdd(entity);
		return true;
	}

	public static bool Cycle(List<BaseEntity> transitions = null)
	{
		if (inCycle)
		{
			return false;
		}
		int num = RoomOcclusion.occlusion;
		if (RoomOcclusionManager.FailedOpen || !RoomOcclusionManager.PartitionIsLive)
		{
			num = 0;
		}
		if (failed)
		{
			if (num == failedLevel)
			{
				return true;
			}
			failed = false;
		}
		if (num == 0 && deliveryLevel == 0 && grouped.Count == 0)
		{
			return true;
		}
		using (TimeWarning.New("RoomOcclusion.Delivery"))
		{
			inCycle = true;
			try
			{
				if (num != deliveryLevel)
				{
					ReleaseAll();
					deliveryLevel = num;
				}
				if (num < 2)
				{
					return true;
				}
				Deliver(transitions);
			}
			catch (Exception arg)
			{
				Debug.LogError((object)$"Room occlusion delivery failed and is failing open: {arg}");
				try
				{
					ReleaseAll();
				}
				catch (Exception arg2)
				{
					Debug.LogError((object)$"Room occlusion could not even release, entities may be stranded: {arg2}");
				}
				failed = true;
				failedLevel = num;
				deliveryLevel = num;
			}
			finally
			{
				inCycle = false;
			}
		}
		return true;
	}

	public static void RefreshForOpenedPortal(BaseEntity portal)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		bool occlusion_door_immediate = RoomOcclusion.occlusion_door_immediate;
		List<BaseEntity> list = (occlusion_door_immediate ? Pool.Get<List<BaseEntity>>() : null);
		try
		{
			bool flag = Cycle(list);
			if (!occlusion_door_immediate || !flag || !IsEnforcing)
			{
				return;
			}
			foreach (BaseEntity item in list)
			{
				if (!((Object)(object)item == (Object)null) && !item.IsDestroyed)
				{
					item.SendNetworkUpdateImmediate();
				}
			}
			List<Connection> list2 = portal?.net?.group?.subscribers;
			if (list2 == null)
			{
				return;
			}
			Vector3 position = ((Component)portal).transform.position;
			float occlusion_door_immediate_range = RoomOcclusion.occlusion_door_immediate_range;
			foreach (Connection item2 in list2)
			{
				if (item2?.player is BasePlayer { IsConnected: not false, net: var net } basePlayer && net?.subscriber != null && SendsImmediately(position, ((Component)basePlayer).transform.position, immediate: true, occlusion_door_immediate_range))
				{
					basePlayer.SendAllQueuedSnapshots();
				}
			}
		}
		finally
		{
			if (list != null)
			{
				Pool.FreeUnmanaged<BaseEntity>(ref list);
			}
		}
	}

	public static bool SendsImmediately(Vector3 portalPosition, Vector3 viewerPosition, bool immediate, float range)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!immediate)
		{
			return false;
		}
		if (range <= 0f)
		{
			return true;
		}
		Vector3 val = portalPosition - viewerPosition;
		return val.sqrMagnitude <= range * range;
	}

	public static void InvalidateRoom(Room room)
	{
		if (IsEnforcing && room != null)
		{
			dirtyRooms.TryAdd(room);
		}
	}

	public static void InvalidateRegion(Bounds bounds)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (IsEnforcing)
		{
			dirtyRegions.Add(bounds);
		}
	}

	private static void Deliver(List<BaseEntity> transitions)
	{
		ListHashSet<BaseEntity> val = Pool.Get<ListHashSet<BaseEntity>>();
		try
		{
			GatherCandidates(val);
			AllocateGroups(val);
			RebuildAll();
			AssignEntities(val, transitions);
		}
		finally
		{
			Pool.FreeUnmanaged<BaseEntity>(ref val);
		}
	}

	private static void GatherCandidates(ListHashSet<BaseEntity> into)
	{
		RoomSolver solver = RoomOcclusionManager.Solver;
		if (solver != null)
		{
			if (assignAll)
			{
				GatherAllRooms(solver.Rooms, into);
			}
			else
			{
				GatherDirtyRooms(into);
			}
		}
	}

	private static void GatherAllRooms(List<Room> rooms, ListHashSet<BaseEntity> into)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		assignAll = false;
		dirtyRooms.Clear();
		dirtyRegions.Clear();
		foreach (Room room in rooms)
		{
			if (!room.IsOutside)
			{
				GatherBlocks(room, into);
				GatherContents(room.Bounds, into);
			}
		}
	}

	private static void GatherDirtyRooms(ListHashSet<BaseEntity> into)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<Room> enumerator = dirtyRooms.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				GatherBlocks(enumerator.Current, into);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		foreach (Bounds dirtyRegion in dirtyRegions)
		{
			GatherContents(dirtyRegion, into);
		}
		dirtyRooms.Clear();
		dirtyRegions.Clear();
	}

	private static void AllocateGroups(ListHashSet<BaseEntity> candidates)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RoomOcclusion.AllocateGroups"))
		{
			Enumerator<BaseEntity> enumerator = candidates.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					RoomNetworkGroups.TargetGroup(enumerator.Current);
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static void AssignEntities(ListHashSet<BaseEntity> candidates, List<BaseEntity> transitions)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RoomOcclusion.AssignEntities"))
		{
			Enumerator<BaseEntity> enumerator = candidates.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Assign(enumerator.Current, transitions);
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			SweepGrouped(transitions);
		}
	}

	private static void GatherBlocks(Room room, ListHashSet<BaseEntity> into)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BlockFace> enumerator = room.Faces.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BlockFace current = enumerator.Current;
				if ((Object)(object)current.Entity != (Object)null)
				{
					into.TryAdd(current.Entity);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private static void GatherContents(Bounds bounds, ListHashSet<BaseEntity> into)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		try
		{
			Vector3 center = bounds.center;
			Vector3 extents = bounds.extents;
			Vis.Entities(center, extents.magnitude, list, 2097408, (QueryTriggerInteraction)1);
			foreach (BaseEntity item in list)
			{
				into.TryAdd(item);
			}
		}
		finally
		{
			Pool.FreeUnmanaged<BaseEntity>(ref list);
		}
	}

	private static void SweepGrouped(List<BaseEntity> transitions)
	{
		if (grouped.Count == 0)
		{
			sweepCursor = 0;
			return;
		}
		int num = Mathf.Min(64, grouped.Count);
		for (int i = 0; i < num; i++)
		{
			if (sweepCursor >= grouped.Count)
			{
				sweepCursor = 0;
			}
			BaseEntity baseEntity = grouped[sweepCursor];
			if ((Object)(object)baseEntity == (Object)null || baseEntity.IsDestroyed)
			{
				grouped.RemoveAt(sweepCursor);
			}
			else if (Assign(baseEntity, transitions))
			{
				sweepCursor++;
			}
		}
	}

	private static bool Assign(BaseEntity entity, List<BaseEntity> transitions = null)
	{
		if ((Object)(object)entity == (Object)null || entity.IsDestroyed || entity.isClient)
		{
			return true;
		}
		if (entity.net == null)
		{
			return true;
		}
		if (!entity.SupportsRoomOcclusion())
		{
			return true;
		}
		Group obj = entity.net.group;
		entity.UpdateNetworkGroup();
		if (transitions != null && entity.net.group != obj)
		{
			transitions.Add(entity);
		}
		if (RoomNetworkGroups.IsRoomGroup(entity.net.group))
		{
			grouped.TryAdd(entity);
			return true;
		}
		if (!grouped.Contains(entity))
		{
			return true;
		}
		grouped.Remove(entity);
		return false;
	}

	public static void ForgetEntity(BaseEntity entity)
	{
		if (grouped.Count != 0)
		{
			grouped.Remove(entity);
		}
	}

	private static void ReleaseAll()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		releasing = true;
		try
		{
			list.AddRange((IEnumerable<BaseEntity>)grouped);
			grouped.Clear();
			sweepCursor = 0;
			foreach (BaseEntity item in list)
			{
				if (!((Object)(object)item == (Object)null) && !item.IsDestroyed && item.net != null)
				{
					item.UpdateNetworkGroup();
				}
			}
		}
		finally
		{
			releasing = false;
			Pool.FreeUnmanaged<BaseEntity>(ref list);
		}
		Enumerator<BasePlayer> enumerator2 = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator2.MoveNext())
			{
				BasePlayer current2 = enumerator2.Current;
				if (!((Object)(object)current2 == (Object)null))
				{
					Clear(current2);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
		}
		ClearAllLingerClocks();
		tracked.Clear();
		dirtyRooms.Clear();
		dirtyRegions.Clear();
		assignAll = true;
	}

	public static void Shutdown()
	{
		ReleaseAll();
		deliveryLevel = 0;
		failed = false;
		failedLevel = 0;
	}
}
