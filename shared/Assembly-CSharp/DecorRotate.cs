using UnityEngine;

public class DecorRotate : DecorComponent
{
	public Vector3 MinRotation = new Vector3(0f, -180f, 0f);

	public Vector3 MaxRotation = new Vector3(0f, 180f, 0f);

	public override void ExpandBounds(ref float maxScale, ref float maxShift)
	{
	}

	public override void Apply(ref Vector3 pos, ref Quaternion rot, ref Vector3 scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		uint num = SeedEx.Seed(pos, World.Seed) + 2;
		float rotX = SeedRandom.Range(ref num, MinRotation.x, MaxRotation.x);
		float rotY = SeedRandom.Range(ref num, MinRotation.y, MaxRotation.y);
		float rotZ = SeedRandom.Range(ref num, MinRotation.z, MaxRotation.z);
		ApplyRotation(rotX, rotY, rotZ, ref rot);
	}

	public void ApplyRotation(float rotX, float rotY, float rotZ, ref Quaternion rot)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		rot = Quaternion.Euler(rotX, rotY, rotZ) * rot;
	}

	public DecorRotate()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
	}
}
