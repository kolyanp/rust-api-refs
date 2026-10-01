using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public static class BlockFaceOps
{
	private static bool IsAlive(BlockFace face)
	{
		if (!((Object)(object)face.Entity == (Object)null))
		{
			return !face.Entity.IsDestroyed;
		}
		return true;
	}

	public static void RefreshGraph(List<BlockFace> faces)
	{
		if (faces == null || faces.Count == 0)
		{
			return;
		}
		List<BlockFace> list = Pool.Get<List<BlockFace>>();
		GatherConnectedFaces(faces, list);
		GatherCandidateFaces(faces, list);
		foreach (BlockFace face in faces)
		{
			face.RefreshFaceLinks();
		}
		foreach (BlockFace item in list)
		{
			if (IsAlive(item))
			{
				item.RefreshFaceLinks();
			}
		}
		Pool.FreeUnmanaged<BlockFace>(ref list);
	}

	public static void Teardown(List<BlockFace> faces, string ownerName)
	{
		if (faces == null || faces.Count == 0)
		{
			return;
		}
		List<BlockFace> list = Pool.Get<List<BlockFace>>();
		GatherConnectedFaces(faces, list);
		foreach (BlockFace face in faces)
		{
			UnregisterEdges(face);
			foreach (EdgeLink edge in face.Edges)
			{
				if (edge.ClosestFace != null)
				{
					edge.ClosestFace.DisconnectFace(face);
					face.DisconnectFace(edge.ClosestFace);
					edge.ClosestFace = null;
				}
			}
		}
		foreach (BlockFace item in list)
		{
			if (IsAlive(item))
			{
				item.RefreshFaceLinks();
			}
		}
		Pool.FreeUnmanaged<BlockFace>(ref list);
		foreach (BlockFace face2 in faces)
		{
			if (face2.LinksPerFace.Count > 0)
			{
				Debug.LogError((object)$"Face '{face2.FacePrefab?.localName}' on '{ownerName}' still has {face2.LinksPerFace.Count} connections after teardown");
				foreach (KeyValuePair<BlockFace, int> item2 in face2.LinksPerFace)
				{
					BlockFace key = item2.Key;
					key.LinksPerFace.Remove(face2);
					foreach (EdgeLink edge2 in key.Edges)
					{
						if (edge2.ClosestFace == face2)
						{
							edge2.ClosestFace = null;
						}
					}
					BlockFace.NotifyConnectionsChanged(key);
				}
			}
			FreeFace(face2);
		}
		faces.Clear();
	}

	public static void Release(List<BlockFace> faces)
	{
		if (faces == null || faces.Count == 0)
		{
			return;
		}
		foreach (BlockFace face in faces)
		{
			UnregisterEdges(face);
			foreach (KeyValuePair<BlockFace, int> item in face.LinksPerFace)
			{
				item.Key.LinksPerFace.Remove(face);
			}
			FreeFace(face);
		}
		faces.Clear();
	}

	private static void UnregisterEdges(BlockFace face)
	{
		foreach (EdgeLink edge in face.Edges)
		{
			foreach (EntityLink link in edge.Links)
			{
				link.edges.Remove(edge);
			}
		}
	}

	private static void FreeFace(BlockFace face)
	{
		if (face.Room != null)
		{
			face.Room.Faces.Remove(face);
			face.Room = null;
		}
		for (int i = 0; i < face.Edges.Count; i++)
		{
			EdgeLink edgeLink = face.Edges[i];
			Pool.Free<EdgeLink>(ref edgeLink);
		}
		face.Edges.Clear();
		BlockFace blockFace = face;
		Pool.Free<BlockFace>(ref blockFace);
	}

	public static void GatherConnectedFaces(List<BlockFace> faces, List<BlockFace> results)
	{
		foreach (BlockFace face in faces)
		{
			foreach (KeyValuePair<BlockFace, int> item in face.LinksPerFace)
			{
				BlockFace key = item.Key;
				if (!faces.Contains(key) && !results.Contains(key))
				{
					results.Add(key);
				}
			}
		}
	}

	public static void GatherCandidateFaces(List<BlockFace> faces, List<BlockFace> results)
	{
		foreach (BlockFace face2 in faces)
		{
			foreach (EdgeLink edge in face2.Edges)
			{
				foreach (EntityLink link in edge.Links)
				{
					List<EntityLink> connections = link.connections;
					for (int i = 0; i < connections.Count; i++)
					{
						List<EdgeLink> edges = connections[i].edges;
						for (int j = 0; j < edges.Count; j++)
						{
							BlockFace face = edges[j].Face;
							if (face != null && !faces.Contains(face) && !results.Contains(face))
							{
								results.Add(face);
							}
						}
					}
				}
			}
		}
	}
}
