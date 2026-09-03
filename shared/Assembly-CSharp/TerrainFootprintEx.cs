using UnityEngine;

public static class TerrainFootprintEx
{
	public static bool CheckTerrainFootprint(this Transform transform, TerrainFootprint footprint, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!footprint || footprint.RejectAboveGap <= 0f)
		{
			return true;
		}
		return footprint.MeasureGap(pos, rot, scale) <= footprint.RejectAboveGap;
	}

	public static void FillTerrainFootprint(this Transform transform, TerrainFootprint footprint, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if ((bool)footprint)
		{
			footprint.Fill(pos, rot, scale);
		}
	}
}
