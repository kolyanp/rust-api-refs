using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace OceanSimulationJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct SmallDisplacementPlaneTraceJob : IJobParallelForDefer
{
	public Plane SeaPlane;

	[ReadOnly]
	public NativeList<int> Indices;

	[ReadOnly]
	public NativeArray<Ray> Rays;

	public ReadOnly<float> MaxDists;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<bool> HitResults;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Vector3> HitPositions;

	public void Execute(int indicesIndex)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		int num = Indices[indicesIndex];
		Ray val = Rays[num];
		float num2 = MaxDists[num];
		bool flag = false;
		Vector3 val2 = Vector3.zero;
		float num3 = default(float);
		if (((Plane)(ref SeaPlane)).Raycast(val, ref num3) && num3 < num2)
		{
			flag = true;
			val2 = ((Ray)(ref val)).GetPoint(num3);
		}
		HitResults[num] = flag;
		HitPositions[num] = val2;
	}
}
