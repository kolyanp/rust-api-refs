using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Facepunch.MarchingCubes;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct ProcessTrianglesJob : IJob
{
	public Reader edgeStream;

	public NativeList<float3> vertices;

	public NativeList<int> indices;

	public int edgeArraySize;

	public unsafe void Execute()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		vertices.Clear();
		indices.Clear();
		int num = ((Reader)(ref edgeStream)).Count();
		if (indices.Capacity < num)
		{
			indices.SetCapacity(num);
		}
		if (vertices.Capacity < num)
		{
			vertices.SetCapacity(num);
		}
		NativeArray<int> val = default(NativeArray<int>);
		val._002Ector(edgeArraySize, (Allocator)2, (NativeArrayOptions)0);
		UnsafeUtility.MemSet(NativeArrayUnsafeUtility.GetUnsafePtr<int>(val), byte.MaxValue, (long)val.Length * 4L);
		int generatedVertices = 0;
		for (int i = 0; i < ((Reader)(ref edgeStream)).ForEachCount; i++)
		{
			((Reader)(ref edgeStream)).BeginForEachIndex(i);
			while (((Reader)(ref edgeStream)).RemainingItemCount > 0)
			{
				ProcessEdge(in ((Reader)(ref edgeStream)).Read<Facepunch.MarchingCubes.EdgeKey>(), val, ref generatedVertices);
			}
			((Reader)(ref edgeStream)).EndForEachIndex();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessEdge(in Facepunch.MarchingCubes.EdgeKey edge, NativeArray<int> vertexByEdge, ref int generatedVertices)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		int num = vertexByEdge[edge.edgeId];
		if (num != -1)
		{
			indices.AddNoResize(num);
			return;
		}
		int num2 = generatedVertices++;
		vertices.AddNoResize(edge.vertex);
		indices.AddNoResize(num2);
		vertexByEdge[edge.edgeId] = num2;
	}
}
