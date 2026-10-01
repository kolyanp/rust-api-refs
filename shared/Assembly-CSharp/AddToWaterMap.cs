using UnityEngine;

public class AddToWaterMap : ProceduralObject
{
	public bool automaticallyAddTerrainTopology;

	public override void Process()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		WaterBody component = ((Component)this).GetComponent<WaterBody>();
		Collider component2 = ((Component)this).GetComponent<Collider>();
		Bounds bounds = component2.bounds;
		int mask = (((Object)(object)component != (Object)null) ? component.GetTopologyMask() : 0);
		float num = TerrainMeta.NormalizeY(bounds.max.y);
		int num2 = TerrainMeta.WaterMap.Index(TerrainMeta.NormalizeX(bounds.min.x));
		int num3 = TerrainMeta.WaterMap.Index(TerrainMeta.NormalizeZ(bounds.max.x));
		int num4 = TerrainMeta.WaterMap.Index(TerrainMeta.NormalizeX(bounds.min.z));
		int num5 = TerrainMeta.WaterMap.Index(TerrainMeta.NormalizeZ(bounds.max.z));
		bool flag = component2 is BoxCollider && ((Component)this).transform.rotation == Quaternion.identity;
		RaycastHit val3 = default;
		for (int i = num4; i <= num5; i++)
		{
			float normZ = TerrainMeta.WaterMap.Coordinate(i);
			for (int j = num2; j <= num3; j++)
			{
				float normX = TerrainMeta.WaterMap.Coordinate(j);
				Vector3 val = new Vector3(TerrainMeta.DenormalizeX(normX), bounds.max.y + 1f, TerrainMeta.DenormalizeZ(normZ));
				if (!flag)
				{
					Ray val2 = new Ray(val, Vector3.down);
					if (!component2.Raycast(val2, ref val3, bounds.size.y + 1f + 1f))
					{
						continue;
					}
					num = TerrainMeta.NormalizeY(val3.point.y);
				}
				float height = TerrainMeta.HeightMap.GetHeight01(normX, normZ);
				if (num < height - TerrainMeta.OneOverSize.z)
				{
					continue;
				}
				if ((Object)(object)component != (Object)null)
				{
					if (automaticallyAddTerrainTopology)
					{
						TerrainMeta.TopologyMap.AddTopology(normX, normZ, mask);
					}
					if (component.Type == WaterBodyType.Lake && !TerrainMeta.TopologyMap.GetTopology(normX, normZ, 196608))
					{
						continue;
					}
				}
				if (!(TerrainMeta.WaterMap.GetHeight01(j, i) >= num))
				{
					TerrainMeta.WaterMap.SetHeight(j, i, num);
				}
			}
		}
		GameManager.Destroy((Component)(object)this);
	}
}
