using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Facepunch.MarchingCubes;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct WriteMeshDataJob : IJob
{
	[ReadOnly]
	public NativeArray<float3> vertices;

	[ReadOnly]
	public NativeArray<int> indices;

	public MeshData meshData;

	public bool withNormals;

	public void Execute()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		int length = vertices.Length;
		int length2 = indices.Length;
		NativeArray<VertexAttributeDescriptor> val = new NativeArray<VertexAttributeDescriptor>((!withNormals) ? 1 : 2, (Allocator)2, (NativeArrayOptions)1);
		val[0] = new VertexAttributeDescriptor((VertexAttribute)0, (VertexAttributeFormat)0, 3, 0);
		if (withNormals)
		{
			val[1] = new VertexAttributeDescriptor((VertexAttribute)1, (VertexAttributeFormat)0, 3, 1);
		}
		meshData.SetVertexBufferParams(length, val);
		meshData.GetVertexData<float3>(0).CopyFrom(vertices);
		bool flag = length <= 65535;
		meshData.SetIndexBufferParams(length2, (IndexFormat)(!flag));
		if (flag)
		{
			NativeArray<ushort> indexData = meshData.GetIndexData<ushort>();
			for (int i = 0; i < length2; i++)
			{
				indexData[i] = (ushort)indices[i];
			}
		}
		else
		{
			meshData.GetIndexData<int>().CopyFrom(indices);
		}
		if (withNormals)
		{
			WriteNormals();
		}
		meshData.subMeshCount = 1;
		meshData.SetSubMesh(0, new SubMeshDescriptor(0, length2, (MeshTopology)0), (MeshUpdateFlags)13);
	}

	private void WriteNormals()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		NativeArray<float3> array = meshData.GetVertexData<float3>(1);
		array.MemClear<float3>();
		for (int i = 0; i < indices.Length; i += 3)
		{
			int num = indices[i];
			int num2 = indices[i + 1];
			int num3 = indices[i + 2];
			float3 val = vertices[num];
			float3 val2 = math.cross(vertices[num2] - val, vertices[num3] - val);
			ref NativeArray<float3> reference = ref array;
			int num4 = num;
			reference[num4] += val2;
			reference = ref array;
			num4 = num2;
			reference[num4] += val2;
			reference = ref array;
			num4 = num3;
			reference[num4] += val2;
		}
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = math.normalizesafe(array[j], default(float3));
		}
	}
}
