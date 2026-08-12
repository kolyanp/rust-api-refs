using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class LightEx : UpdateBehaviour, IClientComponent
{
	public bool alterColor;

	public float colorTimeScale;

	public Color colorA;

	public Color colorB;

	public AnimationCurve blendCurve;

	public bool loopColor;

	public bool alterIntensity;

	public float intensityTimeScale;

	public AnimationCurve intenseCurve;

	public float intensityCurveScale;

	public bool loopIntensity;

	public bool randomOffset;

	public float randomIntensityStartScale;

	public List<Light> syncLights;

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
		//IL_002c: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		colorTimeScale = 1f;
		colorA = Color.red;
		colorB = Color.yellow;
		blendCurve = new AnimationCurve();
		loopColor = true;
		intensityTimeScale = 1f;
		intenseCurve = new AnimationCurve();
		intensityCurveScale = 3f;
		loopIntensity = true;
		randomIntensityStartScale = -1f;
		syncLights = new List<Light>(0);
		base._002Ector();
	}
}
