using System.Linq;
using Unity.Collections;

public class GenerateRiverTopology : ProceduralComponent
{
	public override void Process(uint seed)
	{
		foreach (PathList item in TerrainMeta.Path.Rivers.AsEnumerable().Reverse())
		{
			item.AdjustTerrainTopology(scaleWidthWithLength: true);
		}
		MarkRiverside();
	}

	public void MarkRiverside()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		TerrainHeightMap heightmap = TerrainMeta.HeightMap;
		TerrainTopologyMap topomap = TerrainMeta.TopologyMap;
		NativeArray<int> map = topomap.dst;
		int res = topomap.res;
		ImageProcessing.Dilate2D(map, res, res, 49152, 6, delegate(int x, int y)
		{
			if ((map[x * res + y] & 0x31) != 0)
			{
				ref NativeArray<int> reference = ref map;
				int num = x * res + y;
				reference[num] |= 0x8000;
			}
			float normX = topomap.Coordinate(x);
			float normZ = topomap.Coordinate(y);
			if (heightmap.GetSlope(normX, normZ) > 40f)
			{
				ref NativeArray<int> reference = ref map;
				int num = x * res + y;
				reference[num] |= 2;
			}
		});
	}
}
