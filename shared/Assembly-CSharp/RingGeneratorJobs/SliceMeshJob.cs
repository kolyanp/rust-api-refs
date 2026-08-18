using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RingGeneratorJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct SliceMeshJob : IJobParallelForBatch
{
	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> Vertices;

	[NativeDisableParallelForRestriction]
	[ReadOnly]
	public NativeArray<int> Indices;

	public float4x4 ToLocal;

	public float PlaneY;

	public ParallelWriter<float4> Segments;

	private const float PlaneEpsilon = 0.0001f;

	public void Execute(int startIndex, int count)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		NativeList<float4> val = default(NativeList<float4>);
		val._002Ector(count, AllocatorHandle.op_Implicit((Allocator)2));
		float3 val5 = default(float3);
		for (int i = startIndex; i < startIndex + count; i++)
		{
			int num = i * 3;
			float3 val2 = math.transform(ToLocal, Vertices[Indices[num]]);
			float3 val3 = math.transform(ToLocal, Vertices[Indices[num + 1]]);
			float3 val4 = math.transform(ToLocal, Vertices[Indices[num + 2]]);
			float num2 = val2.y - PlaneY;
			float num3 = val3.y - PlaneY;
			float num4 = val4.y - PlaneY;
			((float3)(ref val5))._002Ector(num2, num3, num4);
			if (!math.all(val5 > 0.0001f) && !math.all(val5 < -0.0001f) && !math.all(math.abs(val5) <= 0.0001f))
			{
				float2 p = default(float2);
				float2 p2 = default(float2);
				int n = 0;
				AddCrossing(val2, val3, num2, num3, ref p, ref p2, ref n);
				AddCrossing(val3, val4, num3, num4, ref p, ref p2, ref n);
				AddCrossing(val4, val2, num4, num2, ref p, ref p2, ref n);
				if (n == 2)
				{
					float4 val6 = new float4(p, p2);
					val.Add(ref val6);
				}
			}
		}
		Segments.AddRangeNoResize(val);
	}

	private static void AddCrossing(float3 a, float3 b, float da, float db, ref float2 p0, ref float2 p1, ref int n)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (math.abs(da) <= 0.0001f)
		{
			AddPoint(((float3)(ref a)).xz, ref p0, ref p1, ref n);
		}
		else if (da > 0f != db > 0f && math.abs(db) > 0.0001f)
		{
			float3 val = math.lerp(a, b, math.saturate(da / (da - db)));
			AddPoint(((float3)(ref val)).xz, ref p0, ref p1, ref n);
		}
	}

	private static void AddPoint(float2 q, ref float2 p0, ref float2 p1, ref int n)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (n == 0)
		{
			p0 = q;
			n = 1;
		}
		else if (n == 1 && math.lengthsq(q - p0) > 1E-08f)
		{
			p1 = q;
			n = 2;
		}
	}
}
