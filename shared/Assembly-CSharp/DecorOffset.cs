using UnityEngine;

public class DecorOffset : DecorComponent
{
	public Vector3 MinOffset;

	public Vector3 MaxOffset;

	public override void Apply(ref Vector3 pos, ref Quaternion rot, ref Vector3 scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		uint num = SeedEx.Seed(pos, World.Seed) + 1;
		pos.x += scale.x * SeedRandom.Range(ref num, MinOffset.x, MaxOffset.x);
		pos.y += scale.y * SeedRandom.Range(ref num, MinOffset.y, MaxOffset.y);
		pos.z += scale.z * SeedRandom.Range(ref num, MinOffset.z, MaxOffset.z);
	}

	public DecorOffset()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		MinOffset = new Vector3(0f, 0f, 0f);
		MaxOffset = new Vector3(0f, 0f, 0f);
		base._002Ector();
	}
}
