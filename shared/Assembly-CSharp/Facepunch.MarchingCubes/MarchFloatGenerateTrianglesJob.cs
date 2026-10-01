using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Facepunch.MarchingCubes;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct MarchFloatGenerateTrianglesJob : IJobParallelForBatch
{
	[NativeDisableContainerSafetyRestriction]
	public QuantizedFloatData3DArray sampler;

	public Writer edgeStream;

	public float iso;

	public float3 vertexOffset;

	public float scale;

	public int batchSize;

	[SkipLocalsInit]
	public void Execute(int startIndex, int count)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		int num = startIndex / batchSize;
		edgeStream.PatchMinMaxRange(num);
		edgeStream.BeginForEachIndex(num);
		Span<int3> corners = stackalloc int3[8];
		Span<int> cornerIndices = stackalloc int[8];
		Span<float> cornerSamples = stackalloc float[8];
		for (int i = startIndex; i < startIndex + count; i++)
		{
			int3 val = new int3(i % sampler.Width, i % sampler.WidthHeight / sampler.Width, i / sampler.WidthHeight);
			if (math.any(val > sampler.Bounds - new int3(2)))
			{
				continue;
			}
			corners[0] = val + new int3(0, 0, 0);
			corners[1] = val + new int3(1, 0, 0);
			corners[2] = val + new int3(1, 0, 1);
			corners[3] = val + new int3(0, 0, 1);
			corners[4] = val + new int3(0, 1, 0);
			corners[5] = val + new int3(1, 1, 0);
			corners[6] = val + new int3(1, 1, 1);
			corners[7] = val + new int3(0, 1, 1);
			cornerIndices[0] = i;
			cornerIndices[1] = i + 1;
			cornerIndices[2] = i + 1 + sampler.WidthHeight;
			cornerIndices[3] = i + sampler.WidthHeight;
			cornerIndices[4] = i + sampler.Width;
			cornerIndices[5] = i + 1 + sampler.Width;
			cornerIndices[6] = i + 1 + sampler.Width + sampler.WidthHeight;
			cornerIndices[7] = i + sampler.Width + sampler.WidthHeight;
			int num2 = 0;
			for (int j = 0; j < cornerIndices.Length; j++)
			{
				float num3 = sampler.Sample(cornerIndices[j]);
				cornerSamples[j] = num3;
				num2 |= math.select(0, 1 << j, num3 < iso);
			}
			int num4 = num2 * 16;
			for (int k = 0; k < 16; k += 3)
			{
				int num5 = MarchingCubeLookup.triTableFlat[num4 + k];
				if (num5 == -1)
				{
					break;
				}
				int edge = MarchingCubeLookup.triTableFlat[num4 + k + 1];
				int edge2 = MarchingCubeLookup.triTableFlat[num4 + k + 2];
				edgeStream.Write<EdgeKey>(MakeEdge(num5, corners, cornerSamples, cornerIndices));
				edgeStream.Write<EdgeKey>(MakeEdge(edge, corners, cornerSamples, cornerIndices));
				edgeStream.Write<EdgeKey>(MakeEdge(edge2, corners, cornerSamples, cornerIndices));
			}
		}
		edgeStream.EndForEachIndex();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private EdgeKey MakeEdge(int edge, Span<int3> corners, Span<float> cornerSamples, Span<int> cornerIndices)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		int num = MarchingCubeLookup.cornerIndexAFromEdge[edge];
		int num2 = MarchingCubeLookup.cornerIndexBFromEdge[edge];
		int num3 = cornerIndices[num];
		int num4 = cornerIndices[num2];
		bool flag = num3 < num4;
		int index = (flag ? num : num2);
		int index2 = (flag ? num2 : num);
		int num5 = (flag ? num3 : num4);
		int num6 = MarchingCubeLookup.axisFromEdge[edge];
		int edgeId = 3 * num5 + num6;
		return new EdgeKey(corners[index], corners[index2], cornerSamples[index], cornerSamples[index2], iso, vertexOffset, scale, edgeId);
	}
}
