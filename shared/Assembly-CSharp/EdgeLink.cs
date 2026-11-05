using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class EdgeLink : IPooled
{
	private const float MinEdgeOverlap = 0.05f;

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
		BlockFace closestFace = null;
		int closestAngle = 400;
		float closestAngleDistance = 100f;
		for (int l = 0; l < connectedEdges.Count; l++)
		{
			Consider(connectedEdges[l].Face, ref closestFace, ref closestAngle, ref closestAngleDistance);
		}
		if (closestFace != null)
		{
			return closestFace;
		}
		for (int m = 0; m < OwnBlockFaces.Count; m++)
		{
			Consider(OwnBlockFaces[m], ref closestFace, ref closestAngle, ref closestAngleDistance);
		}
		return closestFace;
	}

	private void Consider(BlockFace candidate, ref BlockFace closestFace, ref int closestAngle, ref float closestAngleDistance)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		float? angle = GetAngle(Face, candidate);
		if (angle.HasValue && SharesEdge(Face, candidate))
		{
			int num = Mathf.RoundToInt(angle.Value);
			float num2 = Vector3.Distance(Face.WorldCenter, candidate.WorldCenter);
			if (num <= closestAngle && (num != closestAngle || (!(num2 > closestAngleDistance) && (num2 != closestAngleDistance || IsOrderedBefore(candidate, closestFace)))))
			{
				closestAngle = num;
				closestAngleDistance = num2;
				closestFace = candidate;
			}
		}
	}

	private static bool SharesEdge(BlockFace face, BlockFace candidate)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)face.Entity == (Object)null || (Object)(object)candidate.Entity == (Object)null || face.FacePrefab?.DebugPoints == null || candidate.FacePrefab?.DebugPoints == null)
		{
			return true;
		}
		if ((Object)(object)face.Entity == (Object)(object)candidate.Entity)
		{
			return true;
		}
		Vector3 line = Vector3.Cross(face.WorldNormal, candidate.WorldNormal);
		if (line.sqrMagnitude < 0.0001f)
		{
			line = SideFacing(face, candidate.WorldCenter);
		}
		if (line.sqrMagnitude < 0.0001f)
		{
			return true;
		}
		line.Normalize();
		ProjectOutline(face, line, out var min, out var max);
		ProjectOutline(candidate, line, out var min2, out var max2);
		return Mathf.Min(max, max2) - Mathf.Max(min, min2) > 0.05f;
	}

	private static Vector3 SideFacing(BlockFace face, Vector3 target)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)face.Entity).transform;
		Vector3[] debugPoints = face.FacePrefab.DebugPoints;
		Vector3 result = Vector3.zero;
		float num = float.MinValue;
		for (int i = 0; i < debugPoints.Length; i++)
		{
			Vector3 val = transform.TransformPoint(debugPoints[i]);
			Vector3 val2 = transform.TransformPoint(debugPoints[(i + 1) % debugPoints.Length]);
			Vector3 val3 = (val + val2) * 0.5f;
			Vector3 val4 = Vector3.Cross(val2 - val, face.WorldNormal);
			Vector3 val5 = val4.normalized;
			if (Vector3.Dot(val5, val3 - face.WorldCenter) < 0f)
			{
				val5 = -val5;
			}
			float num2 = Vector3.Dot(target - val3, val5);
			if (num2 > num)
			{
				num = num2;
				result = val2 - val;
			}
		}
		return result;
	}

	private static void ProjectOutline(BlockFace face, Vector3 line, out float min, out float max)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)face.Entity).transform;
		Vector3[] debugPoints = face.FacePrefab.DebugPoints;
		min = float.MaxValue;
		max = float.MinValue;
		for (int i = 0; i < debugPoints.Length; i++)
		{
			float num = Vector3.Dot(transform.TransformPoint(debugPoints[i]), line);
			min = Mathf.Min(min, num);
			max = Mathf.Max(max, num);
		}
	}

	private static bool IsOrderedBefore(BlockFace a, BlockFace b)
	{
		if (a.BlockId != b.BlockId)
		{
			return a.BlockId < b.BlockId;
		}
		return a.Key < b.Key;
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
