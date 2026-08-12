using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(GreyScaleRenderer), PostProcessEvent.AfterStack, "Custom/GreyScale", true)]
public class GreyScale : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter redLuminance;

	[Range(0f, 1f)]
	public FloatParameter greenLuminance;

	[Range(0f, 1f)]
	public FloatParameter blueLuminance;

	[Range(0f, 1f)]
	public FloatParameter amount;

	[ColorUsage(false, true)]
	public ColorParameter color;

	public GreyScale()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		redLuminance = new FloatParameter
		{
			value = 0f
		};
		greenLuminance = new FloatParameter
		{
			value = 0f
		};
		blueLuminance = new FloatParameter
		{
			value = 0f
		};
		amount = new FloatParameter
		{
			value = 0f
		};
		color = new ColorParameter
		{
			value = Color.white
		};
		base._002Ector();
	}
}
