using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public static class RoomPortalBuilder
{
	public struct PortalHost
	{
		public BlockFace FaceA;

		public BlockFace FaceB;

		public bool PermanentlyOpen;

		public BaseEntity Filler;

		public bool FillerBlocksVision;
	}

	public static void ClearPortals(List<Portal> portals)
	{
		foreach (Portal portal2 in portals)
		{
			portal2.RoomA?.Portals.Remove(portal2);
			portal2.RoomB?.Portals.Remove(portal2);
			Portal portal = portal2;
			Pool.Free<Portal>(ref portal);
		}
		portals.Clear();
	}

	public static void Build(List<PortalHost> hosts, List<Portal> results)
	{
		foreach (PortalHost host in hosts)
		{
			Room room = host.FaceA?.Room;
			Room room2 = host.FaceB?.Room;
			if (room != null && room2 != null && room != room2)
			{
				Portal portal = Pool.Get<Portal>();
				portal.RoomA = room;
				portal.RoomB = room2;
				portal.FaceA = host.FaceA;
				portal.FaceB = host.FaceB;
				portal.PermanentlyOpen = host.PermanentlyOpen;
				portal.Filler = host.Filler;
				portal.FillerBlocksVision = host.FillerBlocksVision;
				room.Portals.Add(portal);
				room2.Portals.Add(portal);
				results.Add(portal);
			}
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
