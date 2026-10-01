using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class EdgeLink : IPooled
{
	public BlockFace Face;

	public BlockEdgeDefinition EdgePrefab;

	public BlockFace ClosestFace;

	public List<EntityLink> Links = new List<EntityLink>();

	public List<BlockFace> OwnBlockFaces = new List<BlockFace>();

	private List<EdgeLink> connectedEdges = new List<EdgeLink>();

	public void Init(BaseEntity entity, BlockFace face, BlockEdgeDefinition edgePrefab)
	{
		Face = face;
		EdgePrefab = edgePrefab;
		if (edgePrefab.Sockets == null)
		{
			return;
		}
		List<EntityLink> entityLinks = entity.GetEntityLinks(linkToNeighbours: false);
		EdgeSocket[] sockets = edgePrefab.Sockets;
		foreach (EdgeSocket edgeSocket in sockets)
		{
			if (edgeSocket.SocketBase == null)
			{
				continue;
			}
			foreach (EntityLink item in entityLinks)
			{
				if (item.socket == edgeSocket.SocketBase)
				{
					Links.Add(item);
				}
			}
		}
	}

	public void ResolveOwnFaces(List<BlockFace> entityFaces)
	{
		OwnBlockFaces.Clear();
		if (EdgePrefab == null)
		{
			return;
		}
		for (int i = 0; i < entityFaces.Count; i++)
		{
			BlockFace blockFace = entityFaces[i];
			if (blockFace == Face || blockFace.FacePrefab?.edges == null)
			{
				continue;
			}
			BlockEdgeDefinition[] edges = blockFace.FacePrefab.edges;
			for (int j = 0; j < edges.Length; j++)
			{
				if (edges[j] == EdgePrefab)
				{
					OwnBlockFaces.Add(blockFace);
					break;
				}
			}
		}
	}

	public void InitSynthetic(BlockFace face)
	{
		Face = face;
	}

	void IPooled.EnterPool()
	{
		Face = null;
		EdgePrefab = null;
		ClosestFace = null;
		Links.Clear();
		OwnBlockFaces.Clear();
		connectedEdges.Clear();
	}

	void IPooled.LeavePool()
	{
	}

	public BlockFace FindClosestFace()
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		connectedEdges.Clear();
		for (int i = 0; i < Links.Count; i++)
		{
			List<EntityLink> connections = Links[i].connections;
			for (int j = 0; j < connections.Count; j++)
			{
				List<EdgeLink> edges = connections[j].edges;
				for (int k = 0; k < edges.Count; k++)
				{
					EdgeLink edgeLink = edges[k];
					if (edgeLink != this && !connectedEdges.Contains(edgeLink))
					{
						connectedEdges.Add(edgeLink);
					}
				}
			}
		}
		BlockFace blockFace = null;
		int num = 400;
		float num2 = 100f;
		for (int l = 0; l < connectedEdges.Count; l++)
		{
			BlockFace face = connectedEdges[l].Face;
			float? angle = GetAngle(Face, face);
			if (angle.HasValue)
			{
				int num3 = Mathf.RoundToInt(angle.Value);
				float num4 = Vector3.Distance(Face.WorldCenter, face.WorldCenter);
				if (num3 < num || (num3 == num && num4 < num2))
				{
					num = num3;
					num2 = num4;
					blockFace = face;
				}
			}
		}
		if (blockFace != null)
		{
			return blockFace;
		}
		for (int m = 0; m < OwnBlockFaces.Count; m++)
		{
			BlockFace blockFace2 = OwnBlockFaces[m];
			float? angle2 = GetAngle(Face, blockFace2);
			if (angle2.HasValue)
			{
				int num5 = Mathf.RoundToInt(angle2.Value);
				float num6 = Vector3.Distance(Face.WorldCenter, blockFace2.WorldCenter);
				if (num5 < num || (num5 == num && num6 < num2))
				{
					num = num5;
					num2 = num6;
					blockFace = blockFace2;
				}
			}
		}
		return blockFace;
	}

	public static float? GetAngle(BlockFace face1, BlockFace face2)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return AngleCalculator.Calculate(face1.WorldNormal, face2.WorldNormal, face1.WorldCenter, face2.WorldCenter);
	}
}
