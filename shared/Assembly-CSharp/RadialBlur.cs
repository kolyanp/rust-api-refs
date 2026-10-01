using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(RadialBlurRenderer), PostProcessEvent.AfterStack, "Custom/RadialBlur", true)]
public class RadialBlur : PostProcessEffectSettings
{
	[Header("Radial Controls")]
	public Vector2Parameter center = new Vector2Parameter
	{
		value = new Vector2(0.5f, 0.5f)
	};

	[Range(0.1f, 2f)]
	public FloatParameter start = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 2f)]
	public FloatParameter amount = new FloatParameter
	{
		value = 0f
	};

	[Header("Blur Quality")]
	[Range(0f, 3f)]
	public FixedIntParameter downsample = new FixedIntParameter
	{
		value = 1
	};

	[Range(1f, 4f)]
	public FixedIntParameter iterations = new FixedIntParameter
	{
		value = 2
	};

	public RadialBlur()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
	}
}
