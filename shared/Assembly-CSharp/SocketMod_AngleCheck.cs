using UnityEngine;

public class SocketMod_AngleCheck : SocketMod
{
	public bool wantsAngle;

	public Vector3 worldNormal;

	public float withinDegrees;

	public bool usePlacementNormal;

	protected override Phrase ErrorPhrase => ConstructionErrors.InvalidAngle;

	public override bool DoCheck(ref Construction.Placement place)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = (usePlacementNormal ? Vector3.forward : Vector3.up);
		float num = Vector3Ex.DotDegrees(worldNormal, place.rotation * val);
		if (!usePlacementNormal)
		{
			return num < withinDegrees;
		}
		return num >= withinDegrees;
	}

	public SocketMod_AngleCheck()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		wantsAngle = true;
		worldNormal = Vector3.up;
		withinDegrees = 45f;
		base._002Ector();
	}
}
