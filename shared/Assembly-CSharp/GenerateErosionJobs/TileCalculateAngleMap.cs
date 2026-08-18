using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct TileCalculateAngleMap : IJobParallelFor
{
	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<float> AngleMap;

	[ReadOnly]
	public ReadOnly<float> TerrainHeightMapSrcFloat;

	public float NormY;

	public int Res;

	public int NumXTiles;

	public int TileSizeX;

	public int TileSizeZ;

	public void Execute(int index)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		int num = index % NumXTiles;
		int num2 = index / NumXTiles;
		int num3 = math.max(num * TileSizeX, 1);
		int num4 = math.max(num2 * TileSizeZ, 1);
		int num5 = math.min(num3 + TileSizeX, Res - 1);
		int num6 = math.min(num4 + TileSizeZ, Res - 1);
		int4 val = default(int4);
		((int4)(ref val))._002Ector(Res);
		int2 val2 = default(int2);
		((int2)(ref val2))._002Ector(1, -1);
		for (int i = num4; i < num6; i++)
		{
			int2 val3 = new int2(i) + val2;
			int4 val4 = ((int2)(ref val3)).yyxy * val;
			for (int j = num3; j < num5; j++)
			{
				float4 val5 = float4.op_Implicit(val4 + new int4(j + 1, j - 1, j - 1, j - 1));
				float num7 = (TerrainHeightMapSrcFloat[(int)val5.x] - TerrainHeightMapSrcFloat[(int)val5.y]) * -0.5f;
				float num8 = (TerrainHeightMapSrcFloat[(int)val5.z] - TerrainHeightMapSrcFloat[(int)val5.w]) * -0.5f;
				float3 val6 = math.normalize(new float3(num7, NormY, num8));
				float num9 = math.dot(val6, val6);
				AngleMap[i * Res + j] = math.acos(math.clamp(math.dot(math.up(), math.normalize(val6)) / num9, -1f, 1f));
			}
		}
	}
}
