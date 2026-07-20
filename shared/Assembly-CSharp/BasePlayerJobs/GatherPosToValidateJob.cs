using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BasePlayerJobs;

[BurstCompile]
internal struct GatherPosToValidateJob : IJob
{
	[WriteOnly]
	public NativeArray<BasePlayer.PositionChange> Changes;

	[WriteOnly]
	public NativeList<int> ToValidate;

	[ReadOnly]
	public TickInterpolatorCache.ReadOnlyState TickCache;

	[ReadOnly]
	public ReadOnly<int> Indices;

	public void Execute()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			Vector3 startPoint = TickInterpolatorCache.GetStartPoint(TickCache, num);
			Vector3 endPoint = TickInterpolatorCache.GetEndPoint(TickCache, num);
			bool num2 = startPoint != endPoint;
			Changes[num] = BasePlayer.PositionChange.Same;
			if (num2)
			{
				ToValidate.AddNoResize(num);
			}
		}
	}
}
