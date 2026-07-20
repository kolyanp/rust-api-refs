using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshRendererData
{
	public List<List<int>> triangles;

	public List<Vector3> vertices;

	public List<Vector3> normals;

	public List<Vector4> tangents;

	public List<Color32> colors32;

	public List<Vector2> uv;

	public List<Vector2> uv2;

	public List<Vector4> positions;

	public void Alloc()
	{
		if (triangles == null)
		{
			triangles = Pool.Get<List<List<int>>>();
		}
		if (vertices == null)
		{
			vertices = Pool.Get<List<Vector3>>();
		}
		if (normals == null)
		{
			normals = Pool.Get<List<Vector3>>();
		}
		if (tangents == null)
		{
			tangents = Pool.Get<List<Vector4>>();
		}
		if (colors32 == null)
		{
			colors32 = Pool.Get<List<Color32>>();
		}
		if (uv == null)
		{
			uv = Pool.Get<List<Vector2>>();
		}
		if (uv2 == null)
		{
			uv2 = Pool.Get<List<Vector2>>();
		}
		if (positions == null)
		{
			positions = Pool.Get<List<Vector4>>();
		}
	}

	public void Free()
	{
		if (triangles != null)
		{
			foreach (List<int> triangle in triangles)
			{
				List<int> current = triangle;
				Pool.FreeUnmanaged<int>(ref current);
			}
			Pool.FreeUnmanaged<List<int>>(ref triangles);
		}
		if (vertices != null)
		{
			Pool.FreeUnmanaged<Vector3>(ref vertices);
		}
		if (normals != null)
		{
			Pool.FreeUnmanaged<Vector3>(ref normals);
		}
		if (tangents != null)
		{
			Pool.FreeUnmanaged<Vector4>(ref tangents);
		}
		if (colors32 != null)
		{
			Pool.FreeUnmanaged<Color32>(ref colors32);
		}
		if (uv != null)
		{
			Pool.FreeUnmanaged<Vector2>(ref uv);
		}
		if (uv2 != null)
		{
			Pool.FreeUnmanaged<Vector2>(ref uv2);
		}
		if (positions != null)
		{
			Pool.FreeUnmanaged<Vector4>(ref positions);
		}
	}

	public void Clear()
	{
		if (triangles != null)
		{
			foreach (List<int> triangle in triangles)
			{
				List<int> current = triangle;
				Pool.FreeUnmanaged<int>(ref current);
			}
			triangles.Clear();
		}
		if (vertices != null)
		{
			vertices.Clear();
		}
		if (normals != null)
		{
			normals.Clear();
		}
		if (tangents != null)
		{
			tangents.Clear();
		}
		if (colors32 != null)
		{
			colors32.Clear();
		}
		if (uv != null)
		{
			uv.Clear();
		}
		if (uv2 != null)
		{
			uv2.Clear();
		}
		if (positions != null)
		{
			positions.Clear();
		}
	}

	public void Apply(Mesh mesh, MeshRendererBatch batch)
	{
		mesh.Clear();
		mesh.subMeshCount = ((triangles == null) ? 1 : triangles.Count);
		if (vertices != null)
		{
			mesh.SetVertices(vertices);
		}
		if (triangles != null)
		{
			for (int i = 0; i < triangles.Count; i++)
			{
				mesh.SetTriangles(triangles[i], i);
			}
		}
		if (normals != null)
		{
			if (normals.Count == vertices.Count)
			{
				mesh.SetNormals(normals);
			}
			else if (normals.Count > 0 && Batching.verbose > 0)
			{
				Debug.LogWarning((object)"Skipping renderer normals because some meshes were missing them.");
			}
		}
		if (tangents != null)
		{
			if (tangents.Count == vertices.Count)
			{
				mesh.SetTangents(tangents);
			}
			else if (tangents.Count > 0 && Batching.verbose > 0)
			{
				Debug.LogWarning((object)"Skipping renderer tangents because some meshes were missing them.");
			}
		}
		if (colors32 != null)
		{
			if (colors32.Count == vertices.Count)
			{
				mesh.SetColors(colors32);
			}
			else if (colors32.Count > 0 && Batching.verbose > 0)
			{
				Debug.LogWarning((object)"Skipping renderer colors because some meshes were missing them.", (Object)(object)batch);
			}
		}
		if (uv != null)
		{
			if (uv.Count == vertices.Count)
			{
				mesh.SetUVs(0, uv);
			}
			else if (uv.Count > 0 && Batching.verbose > 0)
			{
				Debug.LogWarning((object)"Skipping renderer uvs because some meshes were missing them.");
			}
		}
		if (uv2 != null)
		{
			if (uv2.Count == vertices.Count)
			{
				mesh.SetUVs(1, uv2);
			}
			else if (uv2.Count > 0 && Batching.verbose > 0)
			{
				Debug.LogWarning((object)"Skipping renderer uv2s because some meshes were missing them.");
			}
		}
		if (positions != null)
		{
			mesh.SetUVs(2, positions);
		}
	}

	public void Combine(MeshRendererGroup meshGroup, MeshRendererLookup rendererLookup)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val4 = default(Vector3);
		for (int i = 0; i < ((List<MeshRendererInstance>)(object)meshGroup).Count; i++)
		{
			MeshRendererInstance instance = ((List<MeshRendererInstance>)(object)meshGroup)[i];
			Matrix4x4 val = Matrix4x4.TRS(instance.position, instance.rotation, instance.scale);
			MeshCache.Data data = instance.data;
			int num = data.submeshes.Length;
			for (int j = 0; j < num; j++)
			{
				if (triangles.Count <= j)
				{
					triangles.Add(Pool.Get<List<int>>());
				}
				SubMeshDescriptor val2 = data.submeshes[j];
				int num2 = vertices.Count - ((SubMeshDescriptor)(ref val2)).firstVertex;
				int indexCount = ((SubMeshDescriptor)(ref val2)).indexCount;
				int vertexCount = ((SubMeshDescriptor)(ref val2)).vertexCount;
				int num3 = ((data.normals.Length != 0) ? vertexCount : 0);
				int num4 = ((data.tangents.Length != 0) ? vertexCount : 0);
				int num5 = ((data.colors32.Length != 0) ? vertexCount : 0);
				int num6 = vertexCount;
				int num7 = vertexCount;
				List<int> list = triangles[j];
				for (int k = 0; k < indexCount; k++)
				{
					int num8 = data.triangles[k + ((SubMeshDescriptor)(ref val2)).indexStart];
					list.Add(num2 + num8);
				}
				for (int l = 0; l < vertexCount; l++)
				{
					vertices.Add(((Matrix4x4)(ref val)).MultiplyPoint3x4(data.vertices[l + ((SubMeshDescriptor)(ref val2)).firstVertex]));
					positions.Add(Vector4.op_Implicit(instance.position));
				}
				for (int m = 0; m < num3; m++)
				{
					normals.Add(((Matrix4x4)(ref val)).MultiplyVector(data.normals[m + ((SubMeshDescriptor)(ref val2)).firstVertex]));
				}
				for (int n = 0; n < num4; n++)
				{
					Vector4 val3 = data.tangents[n + ((SubMeshDescriptor)(ref val2)).firstVertex];
					((Vector3)(ref val4))._002Ector(val3.x, val3.y, val3.z);
					Vector3 val5 = ((Matrix4x4)(ref val)).MultiplyVector(val4);
					tangents.Add(new Vector4(val5.x, val5.y, val5.z, val3.w));
				}
				if (data.colors32.Length == 0)
				{
					for (int num9 = 0; num9 < vertexCount; num9++)
					{
						colors32.Add(Color32.op_Implicit(Color.white));
					}
				}
				else
				{
					for (int num10 = 0; num10 < num5; num10++)
					{
						colors32.Add(data.colors32[num10 + ((SubMeshDescriptor)(ref val2)).firstVertex]);
					}
				}
				if (data.uv.Length == 0)
				{
					for (int num11 = 0; num11 < num6; num11++)
					{
						uv.Add(Vector2.zero);
					}
				}
				else
				{
					for (int num12 = 0; num12 < num6; num12++)
					{
						uv.Add(data.uv[num12 + ((SubMeshDescriptor)(ref val2)).firstVertex]);
					}
				}
				if (data.uv2.Length == 0)
				{
					for (int num13 = 0; num13 < num7; num13++)
					{
						uv2.Add(Vector2.zero);
					}
				}
				else
				{
					for (int num14 = 0; num14 < num7; num14++)
					{
						uv2.Add(data.uv2[num14 + ((SubMeshDescriptor)(ref val2)).firstVertex]);
					}
				}
			}
			rendererLookup.Add(instance);
		}
	}
}
