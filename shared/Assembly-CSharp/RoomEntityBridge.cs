using System;
using System.Collections.Generic;
using Development.Attributes;
using Facepunch;
using UnityEngine;

[ResetStaticFields]
public class RoomEntityBridge
{
	private struct CachedRoom
	{
		public Room Room;

		public int Generation;

		public Vector3 Position;

		public int Regions;

		public Room[] Spanned;

		public int[] SpannedGenerations;
	}

	private struct CachedPlayerRoom
	{
		public Room Room;

		public int Generation;

		public int PartitionVersion;

		public int Regions;

		public Vector3 Position;
	}

	private static ListHashSet<BaseEntity> portalHosts = new ListHashSet<BaseEntity>();

	private static Dictionary<BaseEntity, CachedRoom> deployableRooms = new Dictionary<BaseEntity, CachedRoom>();

	private static Dictionary<BasePlayer, CachedPlayerRoom> playerRooms = new Dictionary<BasePlayer, CachedPlayerRoom>();

	private const float PlayerRoomRecheckDistance = 0.5f;

	public const float RoomProbeRadius = 4f;

	private const float RoomProbeMargin = 3f;

	private static readonly List<BaseEntity> probeEntities = new List<BaseEntity>();

	private static readonly List<BlockFace> probeFaces = new List<BlockFace>();

	private static List<Bounds> changedRegions = new List<Bounds>();

	private static int regionsBase;

	private const int MaxChangedRegions = 1024;

	private const float RegionReach = 7f;

	private static int RegionCount => regionsBase + changedRegions.Count;

	public static bool RegisterBlock(BaseEntity entity)
	{
		if (entity.faces == null)
		{
			return false;
		}
		bool flag = PrefabAttribute.server.Find<RoomPortalDefinition>(entity.prefabID) != null;
		if (!flag)
		{
			foreach (BlockFace face in entity.faces)
			{
				if (!face.SealsRoom)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			portalHosts.TryAdd(entity);
		}
		return flag;
	}

	public static void UnregisterBlock(BaseEntity entity)
	{
		portalHosts.Remove(entity);
	}

	public static void Clear()
	{
		portalHosts.Clear();
		ForgetRooms();
	}

	public static bool IsVisionBlockingFiller(BaseEntity entity)
	{
		if ((Object)(object)entity != (Object)null)
		{
			return PrefabAttribute.server.Find<RoomVisionBlocker>(entity.prefabID) != null;
		}
		return false;
	}

	public static bool IsPortalFiller(BaseEntity entity)
	{
		return (Object)(object)FindPortalHost(entity) != (Object)null;
	}

	public static BaseEntity FindPortalHost(BaseEntity entity)
	{
		List<EntityLink> entityLinks = entity.GetEntityLinks(linkToNeighbours: false);
		for (int i = 0; i < entityLinks.Count; i++)
		{
			List<EntityLink> connections = entityLinks[i].connections;
			for (int j = 0; j < connections.Count; j++)
			{
				EntityLink entityLink = connections[j];
				BaseEntity owner = entityLink.owner;
				if ((Object)(object)owner == (Object)null || owner.IsDestroyed)
				{
					continue;
				}
				RoomPortalDefinition roomPortalDefinition = PrefabAttribute.server.Find<RoomPortalDefinition>(owner.prefabID);
				if (roomPortalDefinition?.FillerSocketNames == null)
				{
					continue;
				}
				string[] fillerSocketNames = roomPortalDefinition.FillerSocketNames;
				foreach (string value in fillerSocketNames)
				{
					if (entityLink.socket.socketName.Contains(value))
					{
						return owner;
					}
				}
			}
		}
		return null;
	}

	public static bool AddPortals(BaseEntity host)
	{
		bool permanentlyOpen = PrefabAttribute.server.Find<RoomPortalDefinition>(host.prefabID) == null;
		return RoomPortalBuilder.AddHost(host.faces, permanentlyOpen);
	}

	public static void CreateAllPortals()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BaseEntity> enumerator = portalHosts.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseEntity current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && !current.IsDestroyed && current.faces != null)
				{
					AddPortals(current);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		RefreshFillers();
	}

	public static void ReleaseAllPortals()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BaseEntity> enumerator = portalHosts.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseEntity current = enumerator.Current;
				if ((Object)(object)current != (Object)null && current.faces != null)
				{
					RoomPortalBuilder.RemoveHost(current.faces);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void RefreshFillers()
	{
		for (int num = portalHosts.Count - 1; num >= 0; num--)
		{
			BaseEntity baseEntity = portalHosts[num];
			if ((Object)(object)baseEntity == (Object)null || baseEntity.IsDestroyed || baseEntity.faces == null)
			{
				portalHosts.RemoveAt(num);
			}
			else
			{
				RoomPortalDefinition roomPortalDefinition = PrefabAttribute.server.Find<RoomPortalDefinition>(baseEntity.prefabID);
				if (!(roomPortalDefinition == null))
				{
					BaseEntity filler = FindFiller(baseEntity, roomPortalDefinition, out var visionBlocker);
					RoomPortalBuilder.SetFiller(baseEntity.faces, filler, visionBlocker);
				}
			}
		}
	}

	public static Room GetRoomForDeployable(BaseEntity deployable)
	{
		return Resolve(deployable).Room;
	}

	public static void GetRoomsForDeployable(BaseEntity deployable, List<Room> into)
	{
		CachedRoom cachedRoom = Resolve(deployable);
		if (cachedRoom.Room == null)
		{
			return;
		}
		into.Add(cachedRoom.Room);
		if (cachedRoom.Spanned == null)
		{
			return;
		}
		Room[] spanned = cachedRoom.Spanned;
		foreach (Room room in spanned)
		{
			if (room != null && !into.Contains(room))
			{
				into.Add(room);
			}
		}
	}

	private static CachedRoom Resolve(BaseEntity deployable)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (deployableRooms.TryGetValue(deployable, out var value) && IsCacheLive(in value))
		{
			return value;
		}
		Vector3 position = ProbePosition(deployable);
		Room room = ResolveRoomNear(position);
		value = new CachedRoom
		{
			Room = room,
			Generation = (room?.Generation ?? 0),
			Position = position,
			Regions = RegionCount
		};
		if (room != null)
		{
			CaptureSpannedRooms(deployable, room, ref value);
		}
		deployableRooms[deployable] = value;
		return value;
	}

	private static bool IsCacheLive(in CachedRoom cached)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (ChangedNear(cached.Position, cached.Regions))
		{
			return false;
		}
		if (!IsRoomLive(cached.Room, cached.Generation))
		{
			return false;
		}
		if (cached.Spanned != null)
		{
			for (int i = 0; i < cached.Spanned.Length; i++)
			{
				if (!IsRoomLive(cached.Spanned[i], cached.SpannedGenerations[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool IsRoomLive(Room room, int generation)
	{
		if (room != null)
		{
			if (room.Generation == generation)
			{
				return room.Faces.Count > 0;
			}
			return false;
		}
		return true;
	}

	private static void CaptureSpannedRooms(BaseEntity deployable, Room room, ref CachedRoom cached)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (room.Portals.Count == 0)
		{
			return;
		}
		List<Room> list = Pool.Get<List<Room>>();
		try
		{
			OBB bounds = deployable.WorldSpaceBounds();
			foreach (Portal portal in room.Portals)
			{
				Room room2 = portal.OtherRoom(room);
				if (room2 != null && room2 != room && !list.Contains(room2) && portal.Spans(bounds))
				{
					list.Add(room2);
				}
			}
			if (list.Count != 0)
			{
				cached.Spanned = list.ToArray();
				cached.SpannedGenerations = new int[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					cached.SpannedGenerations[i] = list[i].Generation;
				}
				RoomNetworkGroups.SpanningDeployables++;
			}
		}
		finally
		{
			Pool.FreeUnmanaged<Room>(ref list);
		}
	}

	public static void ForgetEntity(BaseEntity entity)
	{
		if (deployableRooms.Count > 0)
		{
			deployableRooms.Remove(entity);
		}
		if (playerRooms.Count > 0 && entity is BasePlayer key)
		{
			playerRooms.Remove(key);
		}
	}

	public static Room GetRoomForPlayer(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)player).transform.position;
		if (playerRooms.TryGetValue(player, out var value) && (value.Room == null || (value.Room.Generation == value.Generation && value.Room.Faces.Count > 0)))
		{
			Vector3 val = value.Position - position;
			if (val.sqrMagnitude < 0.25f)
			{
				if (value.PartitionVersion == RoomOcclusionManager.PartitionVersion)
				{
					return value.Room;
				}
				if (value.Position == position && !ChangedNear(ProbePosition(player), value.Regions))
				{
					value.PartitionVersion = RoomOcclusionManager.PartitionVersion;
					value.Regions = RegionCount;
					playerRooms[player] = value;
					return value.Room;
				}
			}
		}
		Room room = ResolveRoomNear((BaseEntity)player, 4f);
		playerRooms[player] = new CachedPlayerRoom
		{
			Room = room,
			Generation = (room?.Generation ?? 0),
			PartitionVersion = RoomOcclusionManager.PartitionVersion,
			Regions = RegionCount,
			Position = position
		};
		return room;
	}

	public static Room GetRoomForCamera(BaseEntity camera)
	{
		return ResolveRoomNear(camera);
	}

	public static Room ResolveRoomAt(Vector3 position, List<BlockFace> candidates)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Room result = null;
		float num = float.MaxValue;
		foreach (BlockFace candidate in candidates)
		{
			if (candidate.Room == null)
			{
				continue;
			}
			Vector3 val = position - candidate.WorldCenter;
			if (!(Vector3.Dot(val, candidate.WorldNormal) <= 0f))
			{
				float sqrMagnitude = val.sqrMagnitude;
				if (!(sqrMagnitude >= num))
				{
					num = sqrMagnitude;
					result = candidate.Room;
				}
			}
		}
		return result;
	}

	private static Vector3 ProbePosition(BaseEntity entity)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)entity).transform.TransformPoint(entity.bounds.center);
	}

	private static Room ResolveRoomNear(BaseEntity entity, float radius = 4f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ResolveRoomNear(ProbePosition(entity), radius);
	}

	private static Room ResolveRoomNear(Vector3 position, float radius = 4f)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (!RoomOcclusionManager.RoomIndex.AnyNear(position, radius + 3f))
		{
			return null;
		}
		probeEntities.Clear();
		probeFaces.Clear();
		Vis.Entities(position, radius, probeEntities, 2097152, (QueryTriggerInteraction)1);
		foreach (BaseEntity probeEntity in probeEntities)
		{
			if (!((Object)(object)probeEntity == (Object)null) && !probeEntity.isClient && probeEntity.faces != null)
			{
				probeFaces.AddRange(probeEntity.faces);
			}
		}
		return ResolveRoomAt(position, probeFaces);
	}

	public static void OnRegionChanged(Bounds bounds)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (deployableRooms.Count != 0 || playerRooms.Count != 0)
		{
			if (changedRegions.Count >= 1024)
			{
				int num = changedRegions.Count / 2;
				changedRegions.RemoveRange(0, num);
				regionsBase += num;
			}
			changedRegions.Add(bounds);
		}
	}

	public static void ForgetRooms()
	{
		deployableRooms.Clear();
		playerRooms.Clear();
		changedRegions.Clear();
		regionsBase = 0;
	}

	private static bool ChangedNear(Vector3 position, int since)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (since < regionsBase)
		{
			return true;
		}
		for (int i = since - regionsBase; i < changedRegions.Count; i++)
		{
			Bounds val = changedRegions[i];
			if (val.SqrDistance(position) <= 49f)
			{
				return true;
			}
		}
		return false;
	}

	public static BaseEntity FindFiller(BaseEntity host, RoomPortalDefinition portalDef, out RoomVisionBlocker visionBlocker)
	{
		visionBlocker = null;
		if (portalDef.FillerSocketNames == null)
		{
			return null;
		}
		BaseEntity baseEntity = null;
		List<EntityLink> entityLinks = host.GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			EntityLink entityLink = entityLinks[i];
			bool flag = false;
			string[] fillerSocketNames = portalDef.FillerSocketNames;
			foreach (string value in fillerSocketNames)
			{
				if (entityLink.socket.socketName.Contains(value))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			List<EntityLink> connections = entityLink.connections;
			for (int k = 0; k < connections.Count; k++)
			{
				BaseEntity owner = connections[k].owner;
				if (!((Object)(object)owner == (Object)null) && !owner.IsDestroyed)
				{
					visionBlocker = PrefabAttribute.server.Find<RoomVisionBlocker>(owner.prefabID);
					if (visionBlocker != null)
					{
						return owner;
					}
					if ((Object)(object)baseEntity == (Object)null)
					{
						baseEntity = owner;
					}
				}
			}
		}
		return baseEntity;
	}
}
