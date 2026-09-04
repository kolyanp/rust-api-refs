using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public static class QueryVisJobs
{
	[BurstCompile]
	public struct ConstructCommandsJob : IJobParallelForTransform
	{
		[NativeDisableParallelForRestriction]
		public Vector3 cameraPosition;

		[NativeDisableParallelForRestriction]
		public QueryParameters queryParameters;

		[WriteOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<RaycastCommand> commands;

		public void Execute(int index, TransformAccess transform)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			if (((TransformAccess)(ref transform)).isValid)
			{
				Vector3 val = cameraPosition;
				Vector3 position = ((TransformAccess)(ref transform)).position;
				Vector3 val2 = position - val;
				Vector3 normalized = ((Vector3)(ref val2)).normalized;
				float num = Vector3.Distance(val, position);
				commands[index] = new RaycastCommand(val, normalized, queryParameters, num);
			}
		}
	}

	[BurstCompile]
	public struct CheckWaterLevelVisibilityJob : IJobParallelForTransform
	{
		[WriteOnly]
		public NativeArray<bool> blockedByWaterLevel;

		public float waterLevelHeight;

		public bool cameraAboveWater;

		public void Execute(int index, TransformAccess transform)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			bool flag = ((TransformAccess)(ref transform)).position.y - 0.5f >= waterLevelHeight;
			bool flag2 = ((TransformAccess)(ref transform)).position.y + 0.5f < waterLevelHeight;
			blockedByWaterLevel[index] = (flag2 && cameraAboveWater) || (flag && !cameraAboveWater);
		}
	}

	public const float WaterLevelForgiveness = 0.5f;
}
