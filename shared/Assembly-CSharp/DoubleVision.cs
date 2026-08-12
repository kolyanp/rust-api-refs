using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(DoubleVisionRenderer), PostProcessEvent.AfterStack, "Custom/DoubleVision", true)]
public class DoubleVision : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public Vector2Parameter displace;

	[Range(0f, 1f)]
	public FloatParameter amount;

	public DoubleVision()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		displace = new Vector2Parameter
		{
			value = Vector2.zero
		};
		amount = new FloatParameter
		{
			value = 0f
		};
		base._002Ector();
	}
}
