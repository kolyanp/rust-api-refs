using System;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(UnityEngine.Rendering.PostProcessing.AmbientOcclusionRenderer), "Unity/Ambient Occlusion", true)]
public sealed class AmbientOcclusion : PostProcessEffectSettings
{
	[Tooltip("The ambient occlusion method to use. \"Multi Scale Volumetric Obscurance\" is higher quality and faster on desktop & console platforms but requires compute shader support.")]
	public AmbientOcclusionModeParameter mode;

	[Range(0f, 4f)]
	[Tooltip("The degree of darkness added by ambient occlusion. Higher values produce darker areas.")]
	public FloatParameter intensity;

	[ColorUsage(false)]
	[Tooltip("The custom color to use for the ambient occlusion. The default is black.")]
	public ColorParameter color;

	[Tooltip("Check this box to mark this Volume as to only affect ambient lighting. This mode is only available with the Deferred rendering path and HDR rendering. Objects rendered with the Forward rendering path won't get any ambient occlusion.")]
	public BoolParameter ambientOnly;

	[Range(-8f, 0f)]
	public FloatParameter noiseFilterTolerance;

	[Range(-8f, -1f)]
	public FloatParameter blurTolerance;

	[Range(-12f, -1f)]
	public FloatParameter upsampleTolerance;

	[Range(1f, 10f)]
	[Tooltip("This modifies the thickness of occluders. It increases the size of dark areas and also introduces a dark halo around objects.")]
	public FloatParameter thicknessModifier;

	[Range(0f, 1f)]
	[Tooltip("Modifies the influence of direct lighting on ambient occlusion.")]
	public FloatParameter directLightingStrength;

	[Tooltip("The radius of sample points. This affects the size of darkened areas.")]
	public FloatParameter radius;

	[Tooltip("The number of sample points. This affects both quality and performance. For \"Lowest\", \"Low\", and \"Medium\", passes are downsampled. For \"High\" and \"Ultra\", they are not and therefore you should only \"High\" and \"Ultra\" on high-end hardware.")]
	public AmbientOcclusionQualityParameter quality;

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		bool flag = enabled.value && intensity.value > 0f;
		if (mode.value == AmbientOcclusionMode.ScalableAmbientObscurance)
		{
			flag &= !RuntimeUtilities.scriptableRenderPipelineActive;
			if (context != null)
			{
				flag &= Object.op_Implicit((Object)(object)context.resources.shaders.scalableAO) && context.resources.shaders.scalableAO.isSupported;
			}
		}
		else if (mode.value == AmbientOcclusionMode.MultiScaleVolumetricObscurance)
		{
			if (context != null)
			{
				flag &= Object.op_Implicit((Object)(object)context.resources.shaders.multiScaleAO) && context.resources.shaders.multiScaleAO.isSupported && Object.op_Implicit((Object)(object)context.resources.computeShaders.multiScaleAODownsample1) && Object.op_Implicit((Object)(object)context.resources.computeShaders.multiScaleAODownsample2) && Object.op_Implicit((Object)(object)context.resources.computeShaders.multiScaleAORender) && Object.op_Implicit((Object)(object)context.resources.computeShaders.multiScaleAOUpsample);
			}
			flag &= SystemInfo.supportsComputeShaders && !RuntimeUtilities.isAndroidOpenGL && ((RenderTextureFormat)14).IsSupported() && ((RenderTextureFormat)15).IsSupported() && ((RenderTextureFormat)16).IsSupported();
		}
		return flag;
	}

	public AmbientOcclusion()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		mode = new AmbientOcclusionModeParameter
		{
			value = AmbientOcclusionMode.MultiScaleVolumetricObscurance
		};
		intensity = new FloatParameter
		{
			value = 0f
		};
		color = new ColorParameter
		{
			value = Color.black
		};
		ambientOnly = new BoolParameter
		{
			value = true
		};
		noiseFilterTolerance = new FloatParameter
		{
			value = 0f
		};
		blurTolerance = new FloatParameter
		{
			value = -4.6f
		};
		upsampleTolerance = new FloatParameter
		{
			value = -12f
		};
		thicknessModifier = new FloatParameter
		{
			value = 1f
		};
		directLightingStrength = new FloatParameter
		{
			value = 0f
		};
		radius = new FloatParameter
		{
			value = 0.25f
		};
		quality = new AmbientOcclusionQualityParameter
		{
			value = AmbientOcclusionQuality.Medium
		};
		base._002Ector();
	}
}
