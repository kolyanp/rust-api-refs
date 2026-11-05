using UnityEngine;

public class DecorOffset : DecorComponent
{
	public Vector3 MinOffset = new Vector3(0f, 0f, 0f);

	public Vector3 MaxOffset = new Vector3(0f, 0f, 0f);

	public override void ExpandBounds(ref float maxScale, ref float maxShift)
	{
		float num = Mathf.Max(Mathf.Abs(MinOffset.x), Mathf.Abs(MaxOffset.x));
		float num2 = Mathf.Max(Mathf.Abs(MinOffset.z), Mathf.Abs(MaxOffset.z));
		maxShift += maxScale * Mathf.Sqrt(num * num + num2 * num2);
	}

	public override void Apply(ref Vector3 pos, ref Quaternion rot, ref Vector3 scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		uint num = SeedEx.Seed(pos, World.Seed) + 1;
		float offsetX = SeedRandom.Range(ref num, MinOffset.x, MaxOffset.x);
		float offsetY = SeedRandom.Range(ref num, MinOffset.y, MaxOffset.y);
		float offsetZ = SeedRandom.Range(ref num, MinOffset.z, MaxOffset.z);
		ApplyOffset(offsetX, offsetY, offsetZ, ref pos, scale);
	}

	public void ApplyOffset(float offsetX, float offsetY, float offsetZ, ref Vector3 pos, Vector3 scale)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		pos.x += scale.x * offsetX;
		pos.y += scale.y * offsetY;
		pos.z += scale.z * offsetZ;
	}

	public DecorOffset()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
	}
}
