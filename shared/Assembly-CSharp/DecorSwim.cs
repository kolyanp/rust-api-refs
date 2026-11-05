using UnityEngine;

public class DecorSwim : DecorComponent
{
	public override void ExpandBounds(ref float maxScale, ref float maxShift)
	{
	}

	public override void Apply(ref Vector3 pos, ref Quaternion rot, ref Vector3 scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		float num = WaterLevel.GetWaterSurface(pos, waves: false, volumes: false);
		if (num <= TerrainMeta.Position.y + 1f)
		{
			num = 0f;
		}
		ApplySwim(num, ref pos);
	}

	public void ApplySwim(float surface, ref Vector3 pos)
	{
		pos.y = surface;
	}
}
