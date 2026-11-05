using System;
using System.Collections.Generic;
using System.Diagnostics;
using ConVar;
using Development.Attributes;
using Facepunch;
using Facepunch.Rust.Profiling;
using UnityEngine;

[ResetStaticFields]
public class RoomOcclusionManager
{
	private static RoomSolver solver;

	private static ListHashSet<BlockFace> dirty;

	private static bool subscribed;

	private static bool facesEnabled;

	private static bool bringUpFailed;

	private static RoomSpatialIndex roomIndex = new RoomSpatialIndex();

	private static RoomExteriorIndex exteriorIndex = new RoomExteriorIndex();

	private static bool fillersDirty;

	public static int PortalStateVersion;

	private static ListHashSet<Portal> changedPortals = new ListHashSet<Portal>();

	public static int PartitionVersion;

	private static int visibilityPortalVersion;

	private static readonly Stopwatch timer = new Stopwatch();

	public static bool FailedOpen;

	private static bool partitionActive;

	private static Dictionary<ulong, int> debugRoomIds = new Dictionary<ulong, int>();

	private static int nextDebugRoomId = 1;

	public static bool FacesEnabled => facesEnabled;

	public static int FailedOpenBuildings => exteriorIndex.FailedOpenCount;

	public static RoomSolver Solver => solver;

	public static RoomSpatialIndex RoomIndex => roomIndex;

	public static bool PartitionIsLive => partitionActive;

	private static bool ShouldMaintainPartition => RoomOcclusion.occlusion != 0;

	public static void GatherPortals(List<Portal> into)
	{
		if (solver == null)
		{
			return;
		}
		foreach (Room room in solver.Rooms)
		{
			foreach (Portal portal in room.Portals)
			{
				if (portal.RoomA == room)
				{
					into.Add(portal);
				}
			}
		}
	}

	public static void Initialize()
	{
		if (!subscribed)
		{
			solver = Pool.Get<RoomSolver>();
			dirty = Pool.Get<ListHashSet<BlockFace>>();
			BlockFace.OnConnectionsChanged -= OnFaceChanged;
			BlockFace.OnConnectionsChanged += OnFaceChanged;
			subscribed = true;
		}
	}

	public static void Shutdown()
	{
		TearDown();
		bringUpFailed = false;
		RoomNetworkGroups.Shutdown();
	}

	private static void OnFaceChanged(BlockFace face)
	{
		dirty.TryAdd(face);
	}

	public static void OnFacesRefreshed(BaseEntity entity)
	{
		if (entity.faces == null || !subscribed)
		{
			return;
		}
		foreach (BlockFace face in entity.faces)
		{
			dirty.TryAdd(face);
		}
		if (RoomEntityBridge.RegisterBlock(entity) && RoomEntityBridge.AddPortals(entity))
		{
			fillersDirty = true;
		}
	}

	public static void OnEntitySpawned(BaseEntity entity)
	{
		OnFillerChanged(entity);
	}

	public static void OnEntityDestroyed(BaseEntity entity)
	{
		if (subscribed)
		{
			OnFillerChanged(entity);
			RoomEntityBridge.UnregisterBlock(entity);
			RoomEntityBridge.ForgetEntity(entity);
			RoomOcclusionDelivery.ForgetEntity(entity);
			if (entity is BasePlayer player)
			{
				RoomOcclusionDelivery.ForgetPlayer(player);
			}
		}
	}

	public static void OnPortalStateChanged(BaseEntity portal, bool opened)
	{
		PortalStateVersion++;
		try
		{
			QueueHostPortals(portal);
			if (opened)
			{
				OnPortalOpened(portal);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
	}

	public static void OnPortalChanged(Portal portal)
	{
		PortalStateVersion++;
		changedPortals.TryAdd(portal);
	}

	private static void QueueHostPortals(BaseEntity filler)
	{
		if (!partitionActive)
		{
			return;
		}
		BaseEntity baseEntity = RoomEntityBridge.FindPortalHost(filler);
		if (baseEntity?.faces == null)
		{
			return;
		}
		foreach (BlockFace face in baseEntity.faces)
		{
			if (face.Portal != null)
			{
				changedPortals.TryAdd(face.Portal);
			}
		}
	}

	private static void OnPortalOpened(BaseEntity portal)
	{
		if (partitionActive && ShouldMaintainPartition && RoomOcclusion.occlusion_door_immediate && PortalStateVersion != visibilityPortalVersion && !RoomOcclusionDelivery.InCycle)
		{
			timer.Restart();
			Repartition();
			RoomOcclusionDelivery.RefreshForOpenedPortal(portal);
			RuntimeProfiler.RoomOcclusion += timer.Elapsed;
		}
	}

	private static void OnFillerChanged(BaseEntity filler)
	{
		if (subscribed && RoomEntityBridge.IsVisionBlockingFiller(filler))
		{
			fillersDirty = true;
			PortalStateVersion++;
		}
	}

	public static void RebuildForDebug()
	{
		if (!partitionActive)
		{
			BringUp();
		}
		else
		{
			RebuildEverything();
		}
	}

	public static void Cycle()
	{
		if (!ShouldMaintainPartition)
		{
			bringUpFailed = false;
			if (partitionActive)
			{
				timer.Restart();
				TearDown();
				RuntimeProfiler.RoomOcclusion += timer.Elapsed;
			}
			return;
		}
		timer.Restart();
		if (!partitionActive)
		{
			if (!bringUpFailed)
			{
				BringUp();
			}
		}
		else if (dirty.Count > 0 || solver.HasEmptiedRooms || fillersDirty || PortalStateVersion != visibilityPortalVersion)
		{
			Repartition();
		}
		if (partitionActive)
		{
			RoomOcclusionDelivery.Cycle();
			RoomOcclusion.DrawLiveTick();
		}
		RuntimeProfiler.RoomOcclusion += timer.Elapsed;
	}

	private static void BringUp()
	{
		using (TimeWarning.New("RoomOcclusion.BringUp"))
		{
			Initialize();
			facesEnabled = true;
			try
			{
				CreateAllFaces();
				ConnectAllFaces();
			}
			catch (Exception arg)
			{
				Debug.LogError((object)$"Room occlusion could not build its face graph: disabling. {arg}");
				TearDown();
				bringUpFailed = true;
				return;
			}
			RebuildEverything();
			partitionActive = true;
		}
	}

	private static void TearDown()
	{
		using (TimeWarning.New("RoomOcclusion.TearDown"))
		{
			facesEnabled = false;
			partitionActive = false;
			RoomOcclusionDelivery.Shutdown();
			RoomEntityBridge.ReleaseAllPortals();
			RoomEntityBridge.Clear();
			roomIndex.Clear();
			exteriorIndex.Clear();
			RoomVisibility.Clear();
			if (subscribed)
			{
				BlockFace.OnConnectionsChanged -= OnFaceChanged;
				subscribed = false;
			}
			if (solver != null)
			{
				Pool.Free<RoomSolver>(ref solver);
			}
			if (dirty != null)
			{
				Pool.FreeUnmanaged<BlockFace>(ref dirty);
			}
			ReleaseAllFaces();
			changedPortals.Clear();
			fillersDirty = false;
			PortalStateVersion = 0;
			visibilityPortalVersion = 0;
			FailedOpen = false;
			debugRoomIds.Clear();
			nextDebugRoomId = 1;
			RoomNetworkGroups.Changed();
		}
	}

	private static void CreateAllFaces()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is BaseEntity { IsDestroyed: false } baseEntity)
				{
					baseEntity.InitFaces();
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private static void ConnectAllFaces()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		List<BlockFace> list = Pool.Get<List<BlockFace>>();
		try
		{
			Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current is BaseEntity { IsDestroyed: false, faces: not null } baseEntity)
					{
						baseEntity.GetEntityLinks();
						list.AddRange(baseEntity.faces);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			BlockFace.RefreshFaceLinks(list);
		}
		finally
		{
			Pool.FreeUnmanaged<BlockFace>(ref list);
		}
	}

	private static void ReleaseAllFaces()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is BaseEntity baseEntity)
				{
					baseEntity.ReleaseFaces();
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private static void Repartition()
	{
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RoomOcclusion.Repartition"))
		{
			List<Room> list = Pool.Get<List<Room>>();
			try
			{
				if (!exteriorIndex.IsBuilt)
				{
					RebuildEverything();
					return;
				}
				bool num = dirty.Count > 0 || solver.HasEmptiedRooms;
				int nextRoomId = solver.NextRoomId;
				if (num)
				{
					for (int num2 = dirty.Count - 1; num2 >= 0; num2--)
					{
						if (dirty[num2].Edges.Count == 0)
						{
							dirty.RemoveAt(num2);
						}
					}
					solver.Repair(dirty);
					RoomPortalBuilder.Sync((IList<BlockFace>)solver.Moved);
					OnRoomsChanging(solver.Changed);
					exteriorIndex.Repair(solver.Created, solver.Dissolved, solver.Changed, list);
					list.AddRange(solver.Dissolved);
					StampRoomMetadata(solver.Created);
					StampRoomMetadata(solver.Changed);
					PartitionVersion++;
					OnRoomsDissolved();
					OnRoomsReflooded(solver.Created);
					OnRoomsReflooded(solver.Changed);
					foreach (Room item in solver.Created)
					{
						OnRegionChanged(item.Bounds);
					}
					foreach (Room item2 in solver.Changed)
					{
						OnRegionChanged(item2.Bounds);
					}
				}
				if (fillersDirty)
				{
					RoomEntityBridge.RefreshFillers();
				}
				RefreshVisibility(list, nextRoomId);
			}
			catch (Exception e)
			{
				FailOpen(e);
			}
			finally
			{
				dirty.Clear();
				changedPortals.Clear();
				fillersDirty = false;
				Pool.FreeUnmanaged<Room>(ref list);
			}
		}
	}

	private static void OnRoomsChanging(List<Room> rooms)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		foreach (Room room in rooms)
		{
			roomIndex.Remove(room);
			OnRegionChanged(room.Bounds);
			if (!room.IsOutside)
			{
				RoomOcclusionDelivery.InvalidateRegion(room.Bounds);
			}
		}
	}

	private static void OnRoomsDissolved()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		foreach (Room item in solver.Dissolved)
		{
			roomIndex.Remove(item);
			OnRegionChanged(item.Bounds);
			if (!item.IsOutside)
			{
				RoomOcclusionDelivery.InvalidateRegion(item.Bounds);
			}
		}
	}

	private static void OnRoomsReflooded(List<Room> rooms)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		foreach (Room room in rooms)
		{
			roomIndex.Add(room);
			RoomOcclusionDelivery.InvalidateRoom(room);
			if (!room.IsOutside)
			{
				RoomOcclusionDelivery.InvalidateRegion(room.Bounds);
			}
		}
	}

	private static void RebuildVisibility()
	{
		using (TimeWarning.New("RoomOcclusion.RebuildVisibility"))
		{
			RoomVisibility.Rebuild(solver.Rooms);
			visibilityPortalVersion = PortalStateVersion;
			FailedOpen = false;
			RoomNetworkGroups.Stamp(solver.Rooms);
		}
	}

	private static void RefreshVisibility(List<Room> seeds, int firstNewRoomId)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RoomOcclusion.RefreshVisibility"))
		{
			List<Room> list = Pool.Get<List<Room>>();
			try
			{
				RoomVisibility.Refresh(seeds, (IList<Portal>)changedPortals, list);
				visibilityPortalVersion = PortalStateVersion;
				FailedOpen = false;
				RoomNetworkGroups.Restamp(seeds);
				RoomNetworkGroups.Restamp(list);
				foreach (Room item in list)
				{
					if (item.Id < firstNewRoomId)
					{
						RoomOcclusionDelivery.InvalidateRoom(item);
						RoomOcclusionDelivery.InvalidateRegion(item.Bounds);
					}
				}
			}
			finally
			{
				Pool.FreeUnmanaged<Room>(ref list);
			}
		}
	}

	private static void OnRegionChanged(Bounds bounds)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		RoomEntityBridge.OnRegionChanged(bounds);
		RoomNetworkGroups.OnRegionChanged(bounds);
	}

	private static void FailOpen(Exception e)
	{
		Debug.LogError((object)$"Room occlusion failed and is failing open (everything visible): {e}");
		FailedOpen = true;
		RoomNetworkGroups.Changed();
		exteriorIndex.Clear();
		RoomVisibility.FailOpen(solver?.Rooms);
		visibilityPortalVersion = PortalStateVersion;
		RoomNetworkGroups.ReleaseAll(solver?.Rooms);
	}

	private static void RebuildEverything()
	{
		if (!subscribed)
		{
			return;
		}
		using (TimeWarning.New("RoomOcclusion.RebuildEverything"))
		{
			List<BlockFace> faces = Pool.Get<List<BlockFace>>();
			try
			{
				RoomEntityBridge.ReleaseAllPortals();
				GatherLiveFaces(faces);
				roomIndex.Clear();
				exteriorIndex.Clear();
				solver.Solve(faces);
				exteriorIndex.Rebuild(solver.Rooms);
				StampRoomMetadata(solver.Rooms);
				PartitionVersion++;
				RoomEntityBridge.ForgetRooms();
				OnRoomsReflooded(solver.Rooms);
				RoomEntityBridge.CreateAllPortals();
				RebuildVisibility();
			}
			catch (Exception e)
			{
				FailOpen(e);
			}
			finally
			{
				dirty.Clear();
				changedPortals.Clear();
				fillersDirty = false;
				Pool.FreeUnmanaged<BlockFace>(ref faces);
				RoomNetworkGroups.Changed();
			}
		}
	}

	private static void GatherLiveFaces(List<BlockFace> faces)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is BaseEntity { faces: not null } baseEntity)
				{
					faces.AddRange(baseEntity.faces);
					RoomEntityBridge.RegisterBlock(baseEntity);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void AssignAnchors(List<Room> rooms)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		foreach (Room room in rooms)
		{
			ulong num = 0uL;
			Enumerator<BlockFace> enumerator2 = room.Faces.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					BlockFace current2 = enumerator2.Current;
					if (current2.Key != 0L && (num == 0L || current2.Key < num))
					{
						num = current2.Key;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
			room.Anchor = num;
		}
	}

	private static void AssignDebugIds(List<Room> rooms)
	{
		foreach (Room room in rooms)
		{
			if (room.Anchor == 0L)
			{
				room.DebugId = 0;
				continue;
			}
			if (!debugRoomIds.TryGetValue(room.Anchor, out var value))
			{
				value = nextDebugRoomId++;
				debugRoomIds[room.Anchor] = value;
			}
			room.DebugId = value;
		}
		if (debugRoomIds.Count > 4096)
		{
			debugRoomIds.Clear();
		}
	}

	private static void StampRoomMetadata(List<Room> rooms)
	{
		AssignAnchors(rooms);
		AssignDebugIds(rooms);
		foreach (Room room in rooms)
		{
			room.BuildingId = ((room.Faces.Count != 0) ? GetBuildingId(room.Faces[0]) : 0u);
		}
		StampBounds(rooms);
	}

	public static void StampBounds(List<Room> rooms)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		foreach (Room room in rooms)
		{
			if (room.Faces.Count == 0)
			{
				room.Bounds = default;
				continue;
			}
			Bounds bounds = new Bounds(room.Faces[0].WorldCenter, Vector3.zero);
			for (int i = 1; i < room.Faces.Count; i++)
			{
				bounds.Encapsulate(room.Faces[i].WorldCenter);
			}
			bounds.Expand(3f);
			room.Bounds = bounds;
		}
	}

	private static uint GetBuildingId(BlockFace face)
	{
		if (!(face.Entity is DecayEntity decayEntity))
		{
			return 0u;
		}
		return decayEntity.buildingID;
	}
}
