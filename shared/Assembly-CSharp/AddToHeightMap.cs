using UnityEngine;

public class AddToHeightMap : ProceduralObject
{
	public bool DestroyGameObject;

	public void Apply(bool useTerrainSamplingSettings = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		Collider component = ((Component)this).GetComponent<Collider>();
		Bounds bounds = component.bounds;
		int num = TerrainMeta.HeightMap.Index(TerrainMeta.NormalizeX(((Bounds)(ref bounds)).min.x));
		int num2 = TerrainMeta.HeightMap.Index(TerrainMeta.NormalizeX(((Bounds)(ref bounds)).max.x));
		int num3 = TerrainMeta.HeightMap.Index(TerrainMeta.NormalizeZ(((Bounds)(ref bounds)).min.z));
		int num4 = TerrainMeta.HeightMap.Index(TerrainMeta.NormalizeZ(((Bounds)(ref bounds)).max.z));
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		Vector3 position = TerrainMeta.Position;
		Vector3 size = TerrainMeta.Size;
		Vector3 val = default(Vector3);
		Ray val2 = default(Ray);
		RaycastHit val3 = default(RaycastHit);
		for (int i = num3; i <= num4; i++)
		{
			float normZ = TerrainMeta.HeightMap.Coordinate(i);
			for (int j = num; j <= num2; j++)
			{
				float normX = TerrainMeta.HeightMap.Coordinate(j);
				bool flag;
				float height;
				if (useTerrainSamplingSettings)
				{
					flag = heightMap.TrySampleColliderHeight01(1 << ((Component)component).gameObject.layer, position, size, normX, normZ, out height, component);
				}
				else
				{
					((Vector3)(ref val))._002Ector(TerrainMeta.DenormalizeX(normX), ((Bounds)(ref bounds)).max.y, TerrainMeta.DenormalizeZ(normZ));
					((Ray)(ref val2))._002Ector(val, Vector3.down);
					flag = component.Raycast(val2, ref val3, ((Bounds)(ref bounds)).size.y);
					height = (flag ? TerrainMeta.NormalizeY(((RaycastHit)(ref val3)).point.y) : 0f);
				}
				if (flag)
				{
					float height2 = TerrainMeta.HeightMap.GetHeight01(j, i);
					if (height > height2)
					{
						TerrainMeta.HeightMap.SetHeight(j, i, height);
					}
				}
			}
		}
	}

	public override void Process()
	{
		Apply();
		if (DestroyGameObject)
		{
			GameManager.Destroy(((Component)this).gameObject);
		}
		else
		{
			GameManager.Destroy((Component)(object)this);
		}
	}
}
