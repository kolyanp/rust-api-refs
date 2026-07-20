using UnityEngine;

public class MortarServerProjectile : ServerProjectile
{
	private Vector3 lateralAcceleration;

	private float curveTimeRemaining;

	public void StartLateralCurve(Vector3 acceleration, float duration)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		lateralAcceleration = acceleration;
		curveTimeRemaining = duration;
	}

	public override Vector3 GetVelocityStep()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = base.GetVelocityStep();
		if (curveTimeRemaining > 0f)
		{
			float num = Time.fixedDeltaTime * Time.timeScale;
			val += lateralAcceleration * num;
			curveTimeRemaining -= num;
		}
		return val;
	}
}
