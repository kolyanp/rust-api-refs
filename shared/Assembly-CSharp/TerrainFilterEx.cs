using UnityEngine;

public static class TerrainFilterEx
{
	public static bool ApplyTerrainFilters(this Transform transform, TerrainFilter[] filters, Vector3 pos, Quaternion rot, Vector3 scale, SpawnFilter globalFilter = null)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (filters.Length == 0)
		{
			return true;
		}
		foreach (TerrainFilter terrainFilter in filters)
		{
			Vector3 val = Vector3.Scale(terrainFilter.worldPosition, scale);
			val = rot * val;
			Vector3 val2 = pos + val;
			if (globalFilter != null && globalFilter.GetFactor(val2) == 0f)
			{
				return false;
			}
			if (!terrainFilter.Check(val2))
			{
				return false;
			}
		}
		return true;
	}

	public static bool CanPassTerrainFilters(TerrainFilter[] filters, Vector3 pos, DecorBounds bounds)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected I4, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (float.IsInfinity(bounds.maxScale) || float.IsInfinity(bounds.maxShift))
		{
			return true;
		}
		TerrainTopologyMap topologyMap = TerrainMeta.TopologyMap;
		float num = 2f * TerrainMeta.Size.x / (float)topologyMap.res;
		foreach (TerrainFilter terrainFilter in filters)
		{
			int num2 = (int)terrainFilter.Filter.TopologyAny;
			if (num2 != -1 && num2 != 0)
			{
				float radius = terrainFilter.worldPosition.magnitude * bounds.maxScale + bounds.maxShift + terrainFilter.CheckTopologyRadius + num;
				if (!topologyMap.HasTopology(pos, radius, num2))
				{
					return false;
				}
			}
		}
		return true;
	}
}
