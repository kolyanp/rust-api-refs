using UnityEngine;

public class ExplosionsScaleCurves : MonoBehaviour, IClientComponent
{
	public AnimationCurve ScaleCurveX;

	public AnimationCurve ScaleCurveY;

	public AnimationCurve ScaleCurveZ;

	public Vector3 GraphTimeMultiplier;

	public Vector3 GraphScaleMultiplier;

	public ExplosionsScaleCurves()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		ScaleCurveX = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		ScaleCurveY = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		ScaleCurveZ = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		GraphTimeMultiplier = Vector3.one;
		GraphScaleMultiplier = Vector3.one;
		((MonoBehaviour)this)._002Ector();
	}
}
