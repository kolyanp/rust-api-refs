using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTexturingJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct GenSamplePoints : IJob
{
	public int shoreMapSize;

	public float upscaleCoords;

	public Vector3 terrainPosition;

	[WriteOnly]
	public NativeArray<Vector3> positions;

	[WriteOnly]
	public NativeArray<int> indices;

	public void Execute()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		int num = 0;
		for (; i < shoreMapSize; i++)
		{
			float num2 = ((float)i + 0.5f) * upscaleCoords;
			int num3 = 0;
			while (num3 < shoreMapSize)
			{
				float num4 = ((float)num3 + 0.5f) * upscaleCoords;
				Vector3 val = new Vector3(terrainPosition.x, 0f, terrainPosition.z) + new Vector3(num4, 0f, num2);
				positions[num] = val;
				indices[num] = num;
				num3++;
				num++;
			}
		}
	}
}
