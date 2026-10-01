using UnityEngine;

public class ExplosionsScaleCurves : MonoBehaviour, IClientComponent
{
	public AnimationCurve ScaleCurveX = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve ScaleCurveY = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve ScaleCurveZ = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public Vector3 GraphTimeMultiplier = Vector3.one;

	public Vector3 GraphScaleMultiplier = Vector3.one;

	public ExplosionsScaleCurves()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
	}
}
