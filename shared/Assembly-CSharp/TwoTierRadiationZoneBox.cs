using UnityEngine;

public class TwoTierRadiationZoneBox : TwoTierRadiationZone
{
	public BoxCollider Inner;

	public BoxCollider Outer;

	public override void Apply(Bounds inner, Bounds outer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		base.Apply(inner, outer);
		Inner.center = ((Bounds)(ref inner)).center;
		Inner.size = ((Bounds)(ref inner)).size;
		Outer.center = ((Bounds)(ref outer)).center;
		Outer.size = ((Bounds)(ref outer)).size;
	}
}
