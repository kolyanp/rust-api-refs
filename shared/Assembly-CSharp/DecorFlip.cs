using UnityEngine;

public class DecorFlip : DecorComponent
{
	public enum AxisType
	{
		X,
		Y,
		Z
	}

	public AxisType FlipAxis = AxisType.Y;

	public override void ExpandBounds(ref float maxScale, ref float maxShift)
	{
	}

	public override void Apply(ref Vector3 pos, ref Quaternion rot, ref Vector3 scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		uint num = SeedEx.Seed(pos, World.Seed) + 4;
		ApplyFlip(SeedRandom.Value(ref num) <= 0.5f, ref rot);
	}

	public void ApplyFlip(bool flip, ref Quaternion rot)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (flip)
		{
			switch (FlipAxis)
			{
			case AxisType.X:
			case AxisType.Z:
				rot = Quaternion.AngleAxis(180f, rot * Vector3.up) * rot;
				break;
			case AxisType.Y:
				rot = Quaternion.AngleAxis(180f, rot * Vector3.forward) * rot;
				break;
			}
		}
	}
}
