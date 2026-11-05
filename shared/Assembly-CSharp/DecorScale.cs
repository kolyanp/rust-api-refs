using UnityEngine;

public class DecorScale : DecorComponent
{
	public Vector3 MinScale = new Vector3(1f, 1f, 1f);

	public Vector3 MaxScale = new Vector3(2f, 2f, 2f);

	public override void ExpandBounds(ref float maxScale, ref float maxShift)
	{
		maxScale *= Mathf.Max(new float[6]
		{
			Mathf.Abs(MinScale.x),
			Mathf.Abs(MinScale.y),
			Mathf.Abs(MinScale.z),
			Mathf.Abs(MaxScale.x),
			Mathf.Abs(MaxScale.y),
			Mathf.Abs(MaxScale.z)
		});
	}

	public override void Apply(ref Vector3 pos, ref Quaternion rot, ref Vector3 scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		uint num = SeedEx.Seed(pos, World.Seed) + 3;
		ApplyScale(SeedRandom.Value(ref num), ref scale);
	}

	public void ApplyScale(float lerp, ref Vector3 scale)
	{
		scale.x *= Mathf.Lerp(MinScale.x, MaxScale.x, lerp);
		scale.y *= Mathf.Lerp(MinScale.y, MaxScale.y, lerp);
		scale.z *= Mathf.Lerp(MinScale.z, MaxScale.z, lerp);
	}

	public DecorScale()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
	}
}
