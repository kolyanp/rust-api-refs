using UnityEngine;

public class HarborCrane : HarborProximityEntity
{
	public Transform CraneGrab;

	public Transform ArmRoot;

	public Transform ArmSupportLower;

	public Transform ArmSupportUpper;

	public TransformLineRenderer[] LineRenderers;

	protected void UpdateArmSupports(Vector3 fwd)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ArmSupportUpper == (Object)null) && !((Object)(object)ArmSupportLower == (Object)null))
		{
			Vector3 val = ArmSupportUpper.position - ArmSupportLower.position;
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			ArmSupportLower.rotation = Quaternion.LookRotation(fwd, normalized);
			ArmSupportUpper.rotation = Quaternion.LookRotation(fwd, normalized);
		}
	}
}
