using UnityEngine;

public class DecorScale : DecorComponent
{
	public Vector3 MinScale;

	public Vector3 MaxScale;

	public override void Apply(ref Vector3 pos, ref Quaternion rot, ref Vector3 scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		uint num = SeedEx.Seed(pos, World.Seed) + 3;
		float num2 = SeedRandom.Value(ref num);
		scale.x *= Mathf.Lerp(MinScale.x, MaxScale.x, num2);
		scale.y *= Mathf.Lerp(MinScale.y, MaxScale.y, num2);
		scale.z *= Mathf.Lerp(MinScale.z, MaxScale.z, num2);
	}

	public DecorScale()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		MinScale = new Vector3(1f, 1f, 1f);
		MaxScale = new Vector3(2f, 2f, 2f);
		base._002Ector();
	}
}
