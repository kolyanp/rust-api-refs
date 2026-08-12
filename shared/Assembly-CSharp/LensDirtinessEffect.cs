using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(LensDirtinessRenderer), PostProcessEvent.AfterStack, "Custom/LensDirtiness", true)]
public class LensDirtinessEffect : PostProcessEffectSettings
{
	public TextureParameter dirtinessTexture;

	public BoolParameter sceneTintsBloom;

	public FloatParameter gain;

	public FloatParameter threshold;

	public FloatParameter bloomSize;

	public FloatParameter dirtiness;

	public ColorParameter bloomColor;

	public LensDirtinessEffect()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		dirtinessTexture = new TextureParameter();
		sceneTintsBloom = new BoolParameter
		{
			value = false
		};
		gain = new FloatParameter
		{
			value = 1f
		};
		threshold = new FloatParameter
		{
			value = 1f
		};
		bloomSize = new FloatParameter
		{
			value = 5f
		};
		dirtiness = new FloatParameter
		{
			value = 1f
		};
		bloomColor = new ColorParameter
		{
			value = Color.white
		};
		base._002Ector();
	}
}
