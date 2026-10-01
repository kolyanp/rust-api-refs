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

	private static List<Portal> portals = new List<Portal>();

	private static RoomSpatialIndex roomIndex = new RoomSpatialIndex();

	private static RoomExteriorIndex exteriorIndex = new RoomExteriorIndex();

	private static bool portalsDirty;

	public static int PortalStateVersion;

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

	public static List<Portal> Portals => portals;

	public static RoomSpatialIndex RoomIndex => roomIndex;

	public static bool PartitionIsLive => partitionActive;

	private static bool ShouldMaintainPartition => RoomOcclusion.occlusion != 0;

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
		RoomEntityBridge.RegisterBlock(entity);
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
		if (!opened)
		{
			return;
		}
		try
		{
			OnPortalOpened(portal);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
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
			portalsDirty = true;
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
		else if (dirty.Count > 0 || portalsDirty || PortalStateVersion != visibilityPortalVersion)
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
			RoomPortalBuilder.ClearPortals(portals);
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
			portalsDirty = false;
			PortalStateVersion = 0;
			visibilityPortalVersion = 0;
			FailedOpen = false;
			debugRoomIds.Clear();
			nextDebugRoomId = 1;
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
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is BaseEntity { IsDestroyed: false, faces: not null } baseEntity)
				{
					baseEntity.GetEntityLinks();
					BlockFaceOps.RefreshGraph(baseEntity.faces);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
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
		using (TimeWarning.New("RoomOcclusion.Repartition"))
		{
			try
			{
				if (!exteriorIndex.IsBuilt)
				{
					RebuildEverything();
					return;
				}
				bool flag = dirty.Count > 0;
				bool flag2 = flag || portalsDirty;
				int nextRoomId = solver.NextRoomId;
				if (flag)
				{
					for (int num = dirty.Count - 1; num >= 0; num--)
					{
						if (dirty[num].Edges.Count == 0)
						{
							dirty.RemoveAt(num);
						}
					}
					solver.Repair(dirty);
					List<Room> list = Pool.Get<List<Room>>();
					try
					{
						int num2 = solver.Rooms.Count - 1;
						while (num2 >= 0 && solver.Rooms[num2].Id >= nextRoomId)
						{
							list.Add(solver.Rooms[num2]);
							num2--;
						}
						list.Reverse();
						exteriorIndex.Repair(list, solver.Dissolved);
						StampRoomMetadata(list);
						PartitionVersion++;
						OnRoomsDissolved();
						OnRoomsReflooded(list);
					}
					finally
					{
						Pool.FreeUnmanaged<Room>(ref list);
					}
				}
				if (flag2)
				{
					RebuildPortals();
				}
				RebuildVisibility(nextRoomId);
			}
			catch (Exception e)
			{
				FailOpen(e);
			}
			finally
			{
				dirty.Clear();
				portalsDirty = false;
			}
		}
	}

	private static void OnRoomsDissolved()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		foreach (Room item in solver.Dissolved)
		{
			roomIndex.Remove(item);
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

	private static void RebuildVisibility(int firstNewRoomId)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RoomOcclusion.RebuildVisibility"))
		{
			List<Room> list = Pool.Get<List<Room>>();
			try
			{
				RoomVisibility.Rebuild(solver.Rooms, list);
				visibilityPortalVersion = PortalStateVersion;
				FailedOpen = false;
				RoomNetworkGroups.Stamp(solver.Rooms);
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

	private static void FailOpen(Exception e)
	{
		Debug.LogError((object)$"Room occlusion failed and is failing open (everything visible): {e}");
		FailedOpen = true;
		exteriorIndex.Clear();
		RoomVisibility.FailOpen(solver?.Rooms);
		visibilityPortalVersion = PortalStateVersion;
		RoomNetworkGroups.ReleaseAll(solver?.Rooms);
	}

	private static void RebuildPortals()
	{
		using (TimeWarning.New("RoomOcclusion.RebuildPortals"))
		{
			RoomPortalBuilder.ClearPortals(portals);
			List<RoomPortalBuilder.PortalHost> hosts = Pool.Get<List<RoomPortalBuilder.PortalHost>>();
			try
			{
				RoomEntityBridge.GatherPortalHosts(hosts);
				RoomPortalBuilder.Build(hosts, portals);
			}
			finally
			{
				Pool.FreeUnmanaged<RoomPortalBuilder.PortalHost>(ref hosts);
			}
		}
	}

	private static void RebuildEverything()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!subscribed)
		{
			return;
		}
		using (TimeWarning.New("RoomOcclusion.RebuildEverything"))
		{
			List<BlockFace> list = Pool.Get<List<BlockFace>>();
			try
			{
				Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current is BaseEntity { faces: not null } baseEntity)
						{
							list.AddRange(baseEntity.faces);
							RoomEntityBridge.RegisterBlock(baseEntity);
						}
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
				}
				int nextRoomId = solver.NextRoomId;
				roomIndex.Clear();
				exteriorIndex.Clear();
				solver.Solve(list);
				exteriorIndex.Rebuild(solver.Rooms);
				StampRoomMetadata(solver.Rooms);
				PartitionVersion++;
				OnRoomsReflooded(solver.Rooms);
				RebuildPortals();
				RebuildVisibility(nextRoomId);
			}
			catch (Exception e)
			{
				FailOpen(e);
			}
			finally
			{
				dirty.Clear();
				portalsDirty = false;
				Pool.FreeUnmanaged<BlockFace>(ref list);
			}
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
