using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct PreProcessWaterSpheresJob : IJob
{
	public ReadOnly<RaycastHit> hits;

	public ReadOnly<SpherecastCommand> rays;

	public int maxHitsPerTrace;

	public NativeList<Vector2i> WaterIndices;

	public NativeList<Ray> WaterRays;

	public NativeArray<float> WaterMaxDists;

	public NativeList<int> DeepIndices;

	public NativeList<int> MainIndices;

	public Bounds DeepSeaBounds;

	public void Execute()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Ray val2 = default(Ray);
		for (int i = 0; i < rays.Length; i++)
		{
			SpherecastCommand val = rays[i];
			if ((val.queryParameters.layerMask & 0x10) == 0)
			{
				continue;
			}
			int num2 = GamePhysicsJobs.Util.FindFreeSlot(i, in hits, maxHitsPerTrace, out var endInd);
			if (num2 != endInd)
			{
				int num3 = num++;
				((Ray)(ref val2))._002Ector(((SpherecastCommand)(ref val)).origin, ((SpherecastCommand)(ref val)).direction);
				WaterRays.Add(ref val2);
				if (((Bounds)(ref DeepSeaBounds)).Contains(((Ray)(ref val2)).origin))
				{
					DeepIndices.Add(ref num3);
				}
				else
				{
					MainIndices.Add(ref num3);
				}
				WaterMaxDists[num3] = ((SpherecastCommand)(ref val)).distance;
				ref NativeList<Vector2i> waterIndices = ref WaterIndices;
				Vector2i val3 = new Vector2i(num2, endInd);
				waterIndices.Add(ref val3);
			}
		}
	}
}
