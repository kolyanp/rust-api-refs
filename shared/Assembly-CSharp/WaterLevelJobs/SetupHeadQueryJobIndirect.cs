using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct SetupHeadQueryJobIndirect : IJob
{
	public NativeArray<int> Indices;

	[WriteOnly]
	public NativeReference<int> QueryIndexCount;

	[WriteOnly]
	public NativeArray<Vector3> QueryStarts;

	[WriteOnly]
	public NativeArray<float> QueryRadii;

	[ReadOnly]
	public ReadOnly<bool> ValidInfos;

	[ReadOnly]
	public ReadOnly<Vector3> Starts;

	[ReadOnly]
	public ReadOnly<Vector3> Ends;

	[ReadOnly]
	public ReadOnly<float> Radii;

	public void Execute()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		int value = 0;
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			if (ValidInfos[num])
			{
				Vector3 val = Starts[num];
				Vector3 val2 = Ends[num];
				float num2 = Radii[num];
				float num3 = Mathf.Min(val.y, val2.y) - num2;
				float num4 = Mathf.Max(val.y, val2.y) + num2;
				Vector3 val3 = Vector3Ex.WithY((val + val2) * 0.5f, Mathf.Lerp(num3, num4, 0.75f));
				Indices[value++] = num;
				QueryStarts[num] = val3;
				QueryRadii[num] = 0.01f;
			}
		}
		QueryIndexCount.Value = value;
	}
}
