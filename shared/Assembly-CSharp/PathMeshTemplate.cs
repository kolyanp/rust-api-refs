using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PathMeshTemplate
{
	private static Quaternion rot90;

	public MeshCache.Data[] srcData;

	public float normalSmoothing;

	public bool snapToTerrain;

	public bool snapStartToTerrain;

	public bool snapEndToTerrain;

	public bool scaleWidthWithLength;

	public bool topAligned;

	public int roundVertices;

	public PathList PathList;

	public TerrainHeightMap heightmap;

	public Vector3 Position;

	public Vector3 origin;

	public int stepIndex;

	public int segmentCount;

	public float stepSize;

	public MeshDataArray[] dstData;

	public Mesh[] outputMeshes;

	public void DestroyMeshes()
	{
		for (int i = 0; i < outputMeshes.Length; i++)
		{
			Mesh val = outputMeshes[i];
			if (!((Object)(object)val == (Object)null))
			{
				Object.Destroy((Object)(object)val);
				outputMeshes[i] = null;
			}
		}
	}

	public void IntegrateMainThread()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < outputMeshes.Length; i++)
		{
			Mesh val = outputMeshes[i];
			if (!((Object)(object)val == (Object)null))
			{
				Mesh.ApplyAndDisposeWritableMeshData(dstData[i], val, (MeshUpdateFlags)0);
				val.RecalculateBounds();
				val.RecalculateUVDistributionMetrics(1E-09f);
			}
		}
	}

	private void PreJob(int[] filter)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < outputMeshes.Length; i++)
		{
			if (filter == null || filter.Length == 0 || Array.IndexOf(filter, i) != -1)
			{
				Mesh val = new Mesh();
				outputMeshes[i] = val;
				dstData[i] = Mesh.AllocateWritableMeshData(srcData[i].submeshes.Length);
			}
		}
	}

	public void Generate()
	{
		GenerateCertainLODs();
	}

	public void GenerateCertainLODs(params int[] filter)
	{
		PreJob(filter);
		GenerateImpl();
		IntegrateMainThread();
	}

	private void GenerateImpl()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0572: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = srcData[srcData.Length - 1].bounds;
		Vector3 min = ((Bounds)(ref bounds)).min;
		Vector3 size = ((Bounds)(ref bounds)).size;
		_ = PathList.Width / ((Bounds)(ref bounds)).size.x;
		float randomScale = PathList.RandomScale;
		float meshOffset = PathList.MeshOffset;
		float baseRadius = PathList.Width * 0.5f;
		Vector3 val16 = default(Vector3);
		for (int i = 0; i < srcData.Length; i++)
		{
			if ((Object)(object)outputMeshes[i] == (Object)null)
			{
				continue;
			}
			MeshCache.Data data = srcData[i];
			MeshData val = ((MeshDataArray)(ref dstData[i]))[0];
			int num = data.vertices.Length;
			int num2 = data.triangles.Length;
			int num3 = segmentCount * num;
			int num4 = segmentCount * num2;
			IndexFormat val2 = (IndexFormat)(num3 > 65535);
			((MeshData)(ref val)).SetVertexBufferParams(num3, (VertexAttributeDescriptor[])(object)new VertexAttributeDescriptor[4]
			{
				new VertexAttributeDescriptor((VertexAttribute)0, (VertexAttributeFormat)0, 3, 0),
				new VertexAttributeDescriptor((VertexAttribute)1, (VertexAttributeFormat)0, 3, 1),
				new VertexAttributeDescriptor((VertexAttribute)2, (VertexAttributeFormat)0, 4, 2),
				new VertexAttributeDescriptor((VertexAttribute)4, (VertexAttributeFormat)0, 2, 3)
			});
			((MeshData)(ref val)).SetIndexBufferParams(num4, val2);
			NativeArray<Vector3> vertexData = ((MeshData)(ref val)).GetVertexData<Vector3>(0);
			NativeArray<Vector3> vertexData2 = ((MeshData)(ref val)).GetVertexData<Vector3>(1);
			NativeArray<Vector4> vertexData3 = ((MeshData)(ref val)).GetVertexData<Vector4>(2);
			NativeArray<Vector2> vertexData4 = ((MeshData)(ref val)).GetVertexData<Vector2>(3);
			NativeArray<ushort> val3 = default(NativeArray<ushort>);
			NativeArray<uint> val4 = default(NativeArray<uint>);
			if ((int)val2 == 0)
			{
				val3 = ((MeshData)(ref val)).GetIndexData<ushort>();
			}
			else
			{
				val4 = ((MeshData)(ref val)).GetIndexData<uint>();
			}
			for (int j = 0; j < segmentCount; j++)
			{
				float num5 = (float)(stepIndex + j) * stepSize;
				int num6 = j * num;
				int num7 = j * num2;
				for (int k = 0; k < num; k++)
				{
					Vector2 val5 = data.uv[k];
					Vector3 val6 = data.vertices[k];
					Vector3 val7 = data.normals[k];
					Vector4 val8 = data.tangents[k];
					float num8 = (val6.x - min.x) / size.x;
					float num9 = val6.y - min.y;
					if (topAligned)
					{
						num9 -= size.y;
					}
					float num10 = (val6.z - min.z) / size.z;
					float num11 = num5 + num10 * stepSize;
					Vector3 val9 = (PathList.Spline ? PathList.Path.GetPointCubicHermite(num11) : PathList.Path.GetPoint(num11));
					Vector3 tangent = PathList.Path.GetTangent(num11);
					Vector3 val10 = Vector3Ex.XZ3D(tangent);
					Vector3 normalized = ((Vector3)(ref val10)).normalized;
					Vector3 val11 = rot90 * normalized;
					Vector3 val12 = Vector3.Cross(tangent, val11);
					Quaternion val13 = Quaternion.LookRotation(normalized, val12);
					float radius = PathList.GetRadius(num11, PathList.Path.Length, baseRadius, randomScale, scaleWidthWithLength);
					Vector3 val14 = val9 - val11 * radius;
					Vector3 val15 = val9 + val11 * radius;
					if (snapToTerrain)
					{
						val14.y = heightmap.GetHeight(val14);
						val15.y = heightmap.GetHeight(val15);
					}
					val14 += val12 * meshOffset;
					val15 += val12 * meshOffset;
					val6 = Vector3.Lerp(val14, val15, num8);
					if ((snapStartToTerrain && num11 < 0.1f) || (snapEndToTerrain && num11 > PathList.Path.Length - 0.1f))
					{
						val6.y = heightmap.GetHeight(val6);
					}
					else
					{
						val6.y += num9;
					}
					val6 -= origin;
					val7 = val13 * val7;
					((Vector3)(ref val16))._002Ector(val8.x, val8.y, val8.z);
					val16 = val13 * val16;
					((Vector4)(ref val8)).Set(val16.x, val16.y, val16.z, val8.w);
					if (normalSmoothing > 0f)
					{
						val7 = Vector3.Slerp(val7, Vector3.up, normalSmoothing);
					}
					if (roundVertices > 0)
					{
						val6.x = (float)Math.Round(val6.x, roundVertices);
						val6.y = (float)Math.Round(val6.y, roundVertices);
						val6.z = (float)Math.Round(val6.z, roundVertices);
					}
					vertexData[num6 + k] = val6;
					vertexData2[num6 + k] = val7;
					vertexData3[num6 + k] = val8;
					vertexData4[num6 + k] = val5;
				}
				if ((int)val2 == 0)
				{
					for (int l = 0; l < num2; l++)
					{
						val3[num7 + l] = (ushort)(num6 + data.triangles[l]);
					}
				}
				else
				{
					for (int m = 0; m < num2; m++)
					{
						val4[num7 + m] = (uint)(num6 + data.triangles[m]);
					}
				}
			}
			((MeshData)(ref val)).subMeshCount = 1;
			((MeshData)(ref val)).SetSubMesh(0, new SubMeshDescriptor(0, num4, (MeshTopology)0), (MeshUpdateFlags)0);
		}
	}

	static PathMeshTemplate()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		rot90 = Quaternion.Euler(0f, 90f, 0f);
	}
}
