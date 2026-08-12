using System;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(UnityEngine.Rendering.PostProcessing.AutoExposureRenderer), "Unity/Auto Exposure", true)]
public sealed class AutoExposure : PostProcessEffectSettings
{
	[MinMax(1f, 99f)]
	[DisplayName("Filtering (%)")]
	[Tooltip("Filters the bright and dark parts of the histogram when computing the average luminance. This is to avoid very dark pixels and very bright pixels from contributing to the auto exposure. Unit is in percent.")]
	public Vector2Parameter filtering;

	[Tooltip("Minimum average luminance to consider for auto exposure. Unit is EV.")]
	[DisplayName("Minimum (EV)")]
	[Range(-9f, 9f)]
	public FloatParameter minLuminance;

	[Tooltip("Maximum average luminance to consider for auto exposure. Unit is EV.")]
	[DisplayName("Maximum (EV)")]
	[Range(-9f, 9f)]
	public FloatParameter maxLuminance;

	[Min(0f)]
	[DisplayName("Exposure Compensation")]
	[Tooltip("Use this to scale the global exposure of the scene.")]
	public FloatParameter keyValue;

	[DisplayName("Type")]
	[Tooltip("Use \"Progressive\" if you want auto exposure to be animated. Use \"Fixed\" otherwise.")]
	public EyeAdaptationParameter eyeAdaptation;

	[Min(0f)]
	[Tooltip("Adaptation speed from a dark to a light environment.")]
	public FloatParameter speedUp;

	[Min(0f)]
	[Tooltip("Adaptation speed from a light to a dark environment.")]
	public FloatParameter speedDown;

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (enabled.value && SystemInfo.supportsComputeShaders && !RuntimeUtilities.isAndroidOpenGL && ((RenderTextureFormat)14).IsSupported() && Object.op_Implicit((Object)(object)context.resources.computeShaders.autoExposure))
		{
			return Object.op_Implicit((Object)(object)context.resources.computeShaders.exposureHistogram);
		}
		return false;
	}

	public AutoExposure()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		filtering = new Vector2Parameter
		{
			value = new Vector2(50f, 95f)
		};
		minLuminance = new FloatParameter
		{
			value = 0f
		};
		maxLuminance = new FloatParameter
		{
			value = 0f
		};
		keyValue = new FloatParameter
		{
			value = 1f
		};
		eyeAdaptation = new EyeAdaptationParameter
		{
			value = EyeAdaptation.Progressive
		};
		speedUp = new FloatParameter
		{
			value = 2f
		};
		speedDown = new FloatParameter
		{
			value = 1f
		};
		base._002Ector();
	}
}
