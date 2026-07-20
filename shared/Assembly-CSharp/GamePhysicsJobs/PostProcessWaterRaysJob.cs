using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct PostProcessWaterRaysJob : IJob
{
	public NativeArray<RaycastHit> hits;

	public NativeArray<Ray> rays;

	public NativeList<Vector2i> WaterIndices;

	public NativeArray<bool> hitsSub;

	public NativeArray<Vector3> positionsSub;

	public NativeArray<Vector3> normalsSub;

	public void Execute()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < WaterIndices.Length; i++)
		{
			Vector2i val = WaterIndices[i];
			int x = val.x;
			int y = val.y;
			int num2 = num++;
			RaycastHit val4;
			if (hitsSub[num2])
			{
				Vector3 val2 = positionsSub[num2];
				Ray val3 = rays[num2];
				val4 = default(RaycastHit);
				((RaycastHit)(ref val4)).point = val2;
				((RaycastHit)(ref val4)).normal = normalsSub[num2];
				Vector3 val5 = val2 - ((Ray)(ref val3)).origin;
				((RaycastHit)(ref val4)).distance = ((Vector3)(ref val5)).magnitude;
				RaycastHit val6 = val4;
				hits[x++] = val6;
			}
			if (x < y)
			{
				val4 = (hits[x] = default(RaycastHit));
			}
		}
	}
}
