using Unity.Collections;

public class GenerateClutterTopology : ProceduralComponent
{
	public override void Process(uint seed)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		NativeArray<int> map = TerrainMeta.TopologyMap.dst;
		int res = TerrainMeta.TopologyMap.res;
		ImageProcessing.Dilate2D(map, res, res, 16777728, 3, delegate(int x, int y)
		{
			if ((map[x * res + y] & 0x200) == 0)
			{
				ref NativeArray<int> reference = ref map;
				int num = x * res + y;
				reference[num] |= 0x1000000;
			}
		});
	}
}
