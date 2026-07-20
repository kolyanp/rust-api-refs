using System;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshCache
{
	[Serializable]
	public class Data
	{
		public Mesh mesh;

		public Vector3[] vertices;

		public Vector3[] normals;

		public Vector4[] tangents;

		public Color32[] colors32;

		public int[] triangles;

		public int[] remainingTriangles;

		public Vector2[] uv;

		public Vector2[] uv2;

		public Vector2[] uv3;

		public Vector2[] uv4;

		public Bounds bounds;

		public string meshName;

		public SubMeshDescriptor[] submeshes;
	}

	public static ConcurrentDictionary<int, Data> dictionary = new ConcurrentDictionary<int, Data>();

	public static Data Get(Mesh mesh)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		int instanceID = ((Object)mesh).GetInstanceID();
		if (!TryGet(instanceID, out var data))
		{
			data = new Data();
			data.mesh = mesh;
			data.vertices = mesh.vertices;
			data.normals = mesh.normals;
			data.tangents = mesh.tangents;
			data.colors32 = mesh.colors32;
			data.triangles = mesh.triangles;
			data.uv = mesh.uv;
			data.uv2 = mesh.uv2;
			data.uv3 = mesh.uv3;
			data.uv4 = mesh.uv4;
			data.bounds = mesh.bounds;
			data.meshName = ((Object)mesh).name;
			data.submeshes = (SubMeshDescriptor[])(object)new SubMeshDescriptor[mesh.subMeshCount];
			for (int i = 0; i < data.submeshes.Length; i++)
			{
				data.submeshes[i] = mesh.GetSubMesh(i);
			}
			dictionary.TryAdd(instanceID, data);
		}
		return data;
	}

	public static bool TryGet(int meshInstanceId, out Data data)
	{
		return dictionary.TryGetValue(meshInstanceId, out data);
	}
}
