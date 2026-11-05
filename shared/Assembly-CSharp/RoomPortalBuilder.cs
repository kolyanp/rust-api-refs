using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public static class RoomPortalBuilder
{
	public static bool AddHost(List<BlockFace> hostFaces, bool permanentlyOpen)
	{
		List<(BlockFace, BlockFace)> list = Pool.Get<List<(BlockFace, BlockFace)>>();
		bool result = false;
		try
		{
			GatherOpposingPairs(hostFaces, list);
			foreach (var (blockFace, blockFace2) in list)
			{
				if (blockFace.Portal == null && blockFace2.Portal == null)
				{
					Portal portal = Pool.Get<Portal>();
					portal.FaceA = blockFace;
					portal.FaceB = blockFace2;
					portal.PermanentlyOpen = permanentlyOpen;
					blockFace.Portal = portal;
					blockFace2.Portal = portal;
					Sync(portal);
					result = true;
				}
			}
			return result;
		}
		finally
		{
			Pool.FreeUnmanaged<(BlockFace, BlockFace)>(ref list);
		}
	}

	public static void RemoveHost(List<BlockFace> hostFaces)
	{
		foreach (BlockFace hostFace in hostFaces)
		{
			if (hostFace.Portal != null)
			{
				Release(hostFace.Portal);
			}
		}
	}

	public static void Sync(IList<BlockFace> moved)
	{
		for (int i = 0; i < moved.Count; i++)
		{
			Portal portal = moved[i].Portal;
			if (portal != null)
			{
				Sync(portal);
			}
		}
	}

	public static void Sync(Portal portal)
	{
		Room room = portal.FaceA.Room;
		Room room2 = portal.FaceB.Room;
		if (room == null || room2 == null || room == room2)
		{
			room = null;
			room2 = null;
		}
		if (portal.RoomA != room || portal.RoomB != room2)
		{
			Detach(portal);
			if (room != null)
			{
				portal.RoomA = room;
				portal.RoomB = room2;
				room.Portals.Add(portal);
				room2.Portals.Add(portal);
			}
		}
	}

	public static void SetFiller(List<BlockFace> hostFaces, BaseEntity filler, RoomVisionBlocker visionBlocker)
	{
		bool flag = visionBlocker != null;
		bool flag2 = visionBlocker != null && visionBlocker.AlwaysClosed;
		foreach (BlockFace hostFace in hostFaces)
		{
			Portal portal = hostFace.Portal;
			if (portal != null && (!((Object)(object)portal.Filler == (Object)(object)filler) || portal.FillerBlocksVision != flag || portal.FillerAlwaysClosed != flag2))
			{
				portal.Filler = filler;
				portal.FillerBlocksVision = flag;
				portal.FillerAlwaysClosed = flag2;
				RoomOcclusionManager.OnPortalChanged(portal);
			}
		}
	}

	public static void Release(Portal portal)
	{
		Detach(portal);
		portal.FaceA.Portal = null;
		portal.FaceB.Portal = null;
		Pool.Free<Portal>(ref portal);
	}

	private static void Detach(Portal portal)
	{
		if (portal.RoomA != null)
		{
			portal.RoomA.Portals.Remove(portal);
			portal.RoomB.Portals.Remove(portal);
			portal.RoomA = null;
			portal.RoomB = null;
		}
	}

	public static void GatherOpposingPairs(List<BlockFace> faces, List<(BlockFace a, BlockFace b)> pairs)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < faces.Count; i++)
		{
			for (int j = i + 1; j < faces.Count; j++)
			{
				BlockFace blockFace = faces[i];
				BlockFace blockFace2 = faces[j];
				if (!(Vector3.Distance(blockFace.WorldCenter, blockFace2.WorldCenter) > 0.1f) && !(Vector3.Dot(blockFace.WorldNormal, blockFace2.WorldNormal) > -0.99f))
				{
					pairs.Add((blockFace, blockFace2));
				}
			}
		}
	}
}
