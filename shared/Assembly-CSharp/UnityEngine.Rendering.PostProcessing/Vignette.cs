using System;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(UnityEngine.Rendering.PostProcessing.VignetteRenderer), "Unity/Vignette", true)]
public sealed class Vignette : PostProcessEffectSettings
{
	[Tooltip("Use the \"Classic\" mode for parametric controls. Use the \"Masked\" mode to use your own texture mask.")]
	public VignetteModeParameter mode;

	[Tooltip("Vignette color.")]
	public ColorParameter color;

	[Tooltip("Sets the vignette center point (screen center is [0.5, 0.5]).")]
	public Vector2Parameter center;

	[Tooltip("Amount of vignetting on screen.")]
	[Range(0f, 1f)]
	public FloatParameter intensity;

	[Range(0.01f, 1f)]
	[Tooltip("Smoothness of the vignette borders.")]
	public FloatParameter smoothness;

	[Range(0f, 1f)]
	[Tooltip("Lower values will make a square-ish vignette.")]
	public FloatParameter roundness;

	[Tooltip("Set to true to mark the vignette to be perfectly round. False will make its shape dependent on the current aspect ratio.")]
	public BoolParameter rounded;

	[Tooltip("A black and white mask to use as a vignette.")]
	public TextureParameter mask;

	[Tooltip("Mask opacity.")]
	[Range(0f, 1f)]
	public FloatParameter opacity;

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (enabled.value)
		{
			if (mode.value != VignetteMode.Classic || !(intensity.value > 0f))
			{
				if (mode.value == VignetteMode.Masked && opacity.value > 0f)
				{
					return (Object)(object)mask.value != (Object)null;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public Vignette()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		mode = new VignetteModeParameter
		{
			value = VignetteMode.Classic
		};
		color = new ColorParameter
		{
			value = new Color(0f, 0f, 0f, 1f)
		};
		center = new Vector2Parameter
		{
			value = new Vector2(0.5f, 0.5f)
		};
		intensity = new FloatParameter
		{
			value = 0f
		};
		smoothness = new FloatParameter
		{
			value = 0.2f
		};
		roundness = new FloatParameter
		{
			value = 1f
		};
		rounded = new BoolParameter
		{
			value = false
		};
		mask = new TextureParameter
		{
			value = null
		};
		opacity = new FloatParameter
		{
			value = 1f
		};
		base._002Ector();
	}
}
