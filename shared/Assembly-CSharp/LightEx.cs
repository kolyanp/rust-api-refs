using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class LightEx : UpdateBehaviour, IClientComponent
{
	public bool alterColor;

	public float colorTimeScale = 1f;

	public Color colorA = Color.red;

	public Color colorB = Color.yellow;

	public AnimationCurve blendCurve = new AnimationCurve();

	public bool loopColor = true;

	public bool alterIntensity;

	public float intensityTimeScale = 1f;

	public AnimationCurve intenseCurve = new AnimationCurve();

	public float intensityCurveScale = 3f;

	public bool loopIntensity = true;

	public bool randomOffset;

	public float randomIntensityStartScale = -1f;

	public List<Light> syncLights = new List<Light>(0);

	protected void OnValidate()
	{
		CheckConflict(((Component)this).gameObject);
	}

	public static bool CheckConflict(GameObject go)
	{
		return false;
	}

	public LightEx()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected Obj, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected Obj, but got Unknown
	}
}
