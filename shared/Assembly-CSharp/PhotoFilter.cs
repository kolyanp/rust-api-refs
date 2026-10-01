using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(PhotoFilterRenderer), PostProcessEvent.AfterStack, "Custom/PhotoFilter", true)]
public class PhotoFilter : PostProcessEffectSettings
{
	public ColorParameter color = new ColorParameter
	{
		value = Color.white
	};

	[Range(0f, 1f)]
	public FloatParameter density = new FloatParameter
	{
		value = 0f
	};

	public PhotoFilter()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
	}
}
