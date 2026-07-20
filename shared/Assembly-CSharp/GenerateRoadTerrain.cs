using System.Linq;
using UnityEngine;

public class GenerateRoadTerrain : ProceduralComponent
{
	public const int SmoothenLoops = 2;

	public const int SmoothenIterations = 8;

	public const int SmoothenY = 16;

	public const int SmoothenXZ = 4;

	private float SmoothenFilter(PathList path, int index)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		int topology = TerrainMeta.TopologyMap.GetTopology(path.Path.Points[index]);
		if ((topology & 0x80400) != 0)
		{
			return 0f;
		}
		if ((topology & 0x100000) != 0)
		{
			return 0.5f;
		}
		if ((topology & 0x4000) != 0)
		{
			return 0.1f;
		}
		if ((topology & 0x8000) != 0)
		{
			return 0.3f;
		}
		return 1f;
	}

	public override void Process(uint seed)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		TerrainTopologyMap topologyMap = TerrainMeta.TopologyMap;
		for (int i = 0; i < 2; i++)
		{
			foreach (PathList road in TerrainMeta.Path.Roads.AsEnumerable().Reverse())
			{
				PathInterpolator path = road.Path;
				Vector3[] points = path.Points;
				for (int j = 0; j < points.Length; j++)
				{
					Vector3 val = points[j];
					float num = heightMap.GetHeight(val);
					if ((topologyMap.GetTopology(val) & 0xC000) != 0)
					{
						num = Mathf.Max(num, WaterLevel.RaycastWaterColliders(val) + 2f);
					}
					val.y = num;
					points[j] = val;
				}
				path.Smoothen(8, Vector3.up, (int index) => SmoothenFilter(road, index));
				path.RecalculateTangents();
			}
			foreach (PathList item in TerrainMeta.Path.Roads.AsEnumerable().Reverse())
			{
				heightMap.Push();
				float intensity = 1f;
				float fade = Mathf.InverseLerp(2f, 0f, (float)i);
				item.AdjustTerrainHeight(intensity, fade);
				heightMap.Pop();
			}
			foreach (PathList item2 in TerrainMeta.Path.Rails.AsEnumerable().Reverse())
			{
				heightMap.Push();
				float intensity2 = 1f;
				float num2 = Mathf.InverseLerp(2f, 0f, (float)i);
				item2.AdjustTerrainHeight(intensity2, num2 / 4f);
				heightMap.Pop();
			}
		}
		foreach (PathList road2 in TerrainMeta.Path.Roads)
		{
			PathInterpolator path2 = road2.Path;
			Vector3[] points2 = path2.Points;
			for (int num3 = 0; num3 < points2.Length; num3++)
			{
				Vector3 val2 = points2[num3];
				val2.y = heightMap.GetHeight(val2);
				points2[num3] = val2;
			}
			path2.RecalculateTangents();
		}
	}
}
