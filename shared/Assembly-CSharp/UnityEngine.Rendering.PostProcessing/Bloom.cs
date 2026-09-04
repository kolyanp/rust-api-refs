using System;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(UnityEngine.Rendering.PostProcessing.BloomRenderer), "Unity/Bloom", true)]
public sealed class Bloom : PostProcessEffectSettings
{
	[Tooltip("Strength of the bloom filter. Values higher than 1 will make bloom contribute more energy to the final render.")]
	[Min(0f)]
	public FloatParameter intensity;

	[Tooltip("Filters out pixels under this level of brightness. Value is in gamma-space.")]
	[Min(0f)]
	public FloatParameter threshold;

	[Range(0f, 1f)]
	[Tooltip("Makes transitions between under/over-threshold gradual. 0 for a hard threshold, 1 for a soft threshold).")]
	public FloatParameter softKnee;

	[Tooltip("Clamps pixels to control the bloom amount. Value is in gamma-space.")]
	public FloatParameter clamp;

	[Range(1f, 10f)]
	[Tooltip("Changes the extent of veiling effects. For maximum quality, use integer values. Because this value changes the internal iteration count, You should not animating it as it may introduce issues with the perceived radius.")]
	public FloatParameter diffusion;

	[Tooltip("Distorts the bloom to give an anamorphic look. Negative values distort vertically, positive values distort horizontally.")]
	[Range(-1f, 1f)]
	public FloatParameter anamorphicRatio;

	[ColorUsage(false, true)]
	[Tooltip("Global tint of the bloom filter.")]
	public ColorParameter color;

	[Tooltip("Boost performance by lowering the effect quality. This settings is meant to be used on mobile and other low-end platforms but can also provide a nice performance boost on desktops and consoles.")]
	[FormerlySerializedAs("mobileOptimized")]
	public BoolParameter fastMode;

	[DisplayName("Texture")]
	[Tooltip("The lens dirt texture used to add smudges or dust to the bloom effect.")]
	public TextureParameter dirtTexture;

	[DisplayName("Intensity")]
	[Min(0f)]
	[Tooltip("The intensity of the lens dirtiness.")]
	public FloatParameter dirtIntensity;

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (enabled.value)
		{
			return intensity.value > 0f;
		}
		return false;
	}

	public Bloom()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		intensity = new FloatParameter
		{
			value = 0f
		};
		threshold = new FloatParameter
		{
			value = 1f
		};
		softKnee = new FloatParameter
		{
			value = 0.5f
		};
		clamp = new FloatParameter
		{
			value = 65472f
		};
		diffusion = new FloatParameter
		{
			value = 7f
		};
		anamorphicRatio = new FloatParameter
		{
			value = 0f
		};
		color = new ColorParameter
		{
			value = Color.white
		};
		fastMode = new BoolParameter
		{
			value = false
		};
		dirtTexture = new TextureParameter
		{
			value = null
		};
		dirtIntensity = new FloatParameter
		{
			value = 0f
		};
		base._002Ector();
	}
}
