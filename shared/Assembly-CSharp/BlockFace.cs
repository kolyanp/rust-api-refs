using System;
using System.Collections.Generic;
using System.Threading;
using Development.Attributes;
using Facepunch;
using UnityEngine;

[ResetStaticFields]
public class BlockFace : IPooled
{
	public BaseEntity Entity;

	public BlockFaceDefinition FacePrefab;

	public ulong Key;

	public ulong BlockId;

	private static long nextKey;

	public Room Room;

	public Vector3 WorldCenter;

	public Vector3 WorldNormal;

	public Dictionary<BlockFace, int> LinksPerFace = new Dictionary<BlockFace, int>();

	public List<EdgeLink> Edges = new List<EdgeLink>();

	public bool SealsRoom
	{
		get
		{
			if (!(FacePrefab == null))
			{
				return FacePrefab.SealsRoom;
			}
			return true;
		}
	}

	public static event Action<BlockFace> OnConnectionsChanged;

	public void Init(BaseEntity entity, BlockFaceDefinition facePrefab)
	{
		Entity = entity;
		BlockId = entity.net.ID.Value;
		FacePrefab = facePrefab;
		CaptureTransform();
		BlockEdgeDefinition[] edges = facePrefab.edges;
		foreach (BlockEdgeDefinition edgePrefab in edges)
		{
			EdgeLink edgeLink = Pool.Get<EdgeLink>();
			edgeLink.Init(entity, this, edgePrefab);
			Edges.Add(edgeLink);
		}
	}

	public void InitSynthetic(Vector3 worldCenter, Vector3 worldNormal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		WorldCenter = worldCenter;
		WorldNormal = worldNormal.normalized;
	}

	public void CaptureTransform()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Entity == (Object)null))
		{
			Transform transform = ((Component)Entity).transform;
			WorldCenter = transform.TransformPoint(FacePrefab.LocalCenter);
			WorldNormal = transform.TransformDirection(FacePrefab.LocalNormal);
		}
	}

	void IPooled.EnterPool()
	{
		Key = 0uL;
		BlockId = 0uL;
		Entity = null;
		FacePrefab = null;
		Room = null;
		LinksPerFace.Clear();
		Edges.Clear();
	}

	void IPooled.LeavePool()
	{
		Key = (ulong)Interlocked.Increment(ref nextKey);
	}

	public void RefreshFaceLinks()
	{
		List<BlockFace> list = Pool.Get<List<BlockFace>>();
		List<BlockFace> list2 = Pool.Get<List<BlockFace>>();
		try
		{
			list.Add(this);
			while (list.Count > 0)
			{
				int index = list.Count - 1;
				BlockFace blockFace = list[index];
				list.RemoveAt(index);
				list2.Clear();
				blockFace.RefreshOwnLinks(list2);
				for (int num = list2.Count - 1; num >= 0; num--)
				{
					list.Add(list2[num]);
				}
			}
		}
		finally
		{
			Pool.FreeUnmanaged<BlockFace>(ref list2);
			Pool.FreeUnmanaged<BlockFace>(ref list);
		}
	}

	private void RefreshOwnLinks(List<BlockFace> neighbours)
	{
		CaptureTransform();
		foreach (EdgeLink edge in Edges)
		{
			BlockFace closestFace = edge.ClosestFace;
			BlockFace blockFace = edge.FindClosestFace();
			if (closestFace == blockFace)
			{
				continue;
			}
			edge.ClosestFace = blockFace;
			if (closestFace != null)
			{
				closestFace.DisconnectFace(this);
				DisconnectFace(closestFace);
			}
			if (blockFace != null)
			{
				blockFace.ConnectFace(this);
				ConnectFace(blockFace);
				if (!neighbours.Contains(blockFace))
				{
					neighbours.Add(blockFace);
				}
			}
		}
	}

	public static void NotifyConnectionsChanged(BlockFace face)
	{
		OnConnectionsChanged?.Invoke(face);
	}

	public void ConnectFace(BlockFace face)
	{
		LinksPerFace.TryGetValue(face, out var value);
		value++;
		LinksPerFace[face] = value;
		OnConnectionsChanged?.Invoke(this);
	}

	public void DisconnectFace(BlockFace face)
	{
		if (!LinksPerFace.TryGetValue(face, out var value))
		{
			Debug.LogError((object)("Removing face connection between '" + FacePrefab?.localName + "' and '" + face.FacePrefab?.localName + "' but it isn't in the dictionary (ref counting mess up?)"));
		}
		else
		{
			value--;
			if (value == 0)
			{
				LinksPerFace.Remove(face);
			}
			else
			{
				LinksPerFace[face] = value;
			}
			OnConnectionsChanged?.Invoke(this);
		}
	}

	public void DDraw(BasePlayer player, Color color, float duration, bool drawNormal = true, string label = null)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		if (FacePrefab?.DebugPoints == null || (Object)(object)Entity == (Object)null)
		{
			return;
		}
		Vector3[] debugPoints = FacePrefab.DebugPoints;
		if (debugPoints.Length >= 2)
		{
			Transform transform = ((Component)Entity).transform;
			Vector3 val = WorldNormal * 0.2f;
			Vector3 start = OutlinePoint(transform, debugPoints[^1], val, 0.12f);
			for (int i = 0; i < debugPoints.Length; i++)
			{
				Vector3 val2 = OutlinePoint(transform, debugPoints[i], val, 0.12f);
				UnityEngine.DDraw.Line(player, start, val2, color, duration);
				start = val2;
			}
			if (drawNormal)
			{
				Vector3 val3 = WorldCenter + val;
				UnityEngine.DDraw.Arrow(player, val3, val3 + WorldNormal * 0.4f, color, duration, 0.08f);
			}
			if (!string.IsNullOrEmpty(label))
			{
				UnityEngine.DDraw.Text(player, WorldCenter + val + WorldNormal * 0.5f, label, color, duration, distanceFade: true, zTest: true, 1.5f);
			}
		}
	}

	private Vector3 OutlinePoint(Transform transform, Vector3 localPoint, Vector3 push, float inset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Lerp(transform.TransformPoint(localPoint), WorldCenter, inset) + push;
	}
}
