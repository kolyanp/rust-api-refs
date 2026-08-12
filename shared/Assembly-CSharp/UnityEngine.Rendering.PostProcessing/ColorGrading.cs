using System;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(UnityEngine.Rendering.PostProcessing.ColorGradingRenderer), "Unity/Color Grading", true)]
public sealed class ColorGrading : PostProcessEffectSettings
{
	[DisplayName("Mode")]
	[Tooltip("Select a color grading mode that fits your dynamic range and workflow. Use HDR if your camera is set to render in HDR and your target platform supports it. Use LDR for low-end mobiles or devices that don't support HDR. Use External if you prefer authoring a Log LUT in an external software.")]
	public GradingModeParameter gradingMode;

	[DisplayName("Lookup Texture")]
	[Tooltip("A custom 3D log-encoded texture.")]
	public TextureParameter externalLut;

	[DisplayName("Mode")]
	[Tooltip("Select a tonemapping algorithm to use at the end of the color grading process.")]
	public TonemapperParameter tonemapper;

	[DisplayName("Toe Strength")]
	[Range(0f, 1f)]
	[Tooltip("Affects the transition between the toe and the mid section of the curve. A value of 0 means no toe, a value of 1 means a very hard transition.")]
	public FloatParameter toneCurveToeStrength;

	[Range(0f, 1f)]
	[Tooltip("Affects how much of the dynamic range is in the toe. With a small value, the toe will be very short and quickly transition into the linear section, with a larger value, the toe will be longer.")]
	[DisplayName("Toe Length")]
	public FloatParameter toneCurveToeLength;

	[Range(0f, 1f)]
	[Tooltip("Affects the transition between the mid section and the shoulder of the curve. A value of 0 means no shoulder, a value of 1 means a very hard transition.")]
	[DisplayName("Shoulder Strength")]
	public FloatParameter toneCurveShoulderStrength;

	[Tooltip("Affects how many F-stops (EV) to add to the dynamic range of the curve.")]
	[Min(0f)]
	[DisplayName("Shoulder Length")]
	public FloatParameter toneCurveShoulderLength;

	[Tooltip("Affects how much overshoot to add to the shoulder.")]
	[Range(0f, 1f)]
	[DisplayName("Shoulder Angle")]
	public FloatParameter toneCurveShoulderAngle;

	[Tooltip("Applies a gamma function to the curve.")]
	[Min(0.001f)]
	[DisplayName("Gamma")]
	public FloatParameter toneCurveGamma;

	[DisplayName("Lookup Texture")]
	[Tooltip("Custom lookup texture (strip format, for example 256x16) to apply before the rest of the color grading operators. If none is provided, a neutral one will be generated internally.")]
	public TextureParameter ldrLut;

	[DisplayName("Contribution")]
	[Range(0f, 1f)]
	[Tooltip("How much of the lookup texture will contribute to the color grading effect.")]
	public FloatParameter ldrLutContribution;

	[Tooltip("Sets the white balance to a custom color temperature.")]
	[Range(-100f, 100f)]
	[DisplayName("Temperature")]
	public FloatParameter temperature;

	[Range(-100f, 100f)]
	[Tooltip("Sets the white balance to compensate for a green or magenta tint.")]
	[DisplayName("Tint")]
	public FloatParameter tint;

	[DisplayName("Color Filter")]
	[ColorUsage(false, true)]
	[Tooltip("Tint the render by multiplying a color.")]
	public ColorParameter colorFilter;

	[DisplayName("Hue Shift")]
	[Range(-180f, 180f)]
	[Tooltip("Shift the hue of all colors.")]
	public FloatParameter hueShift;

	[Tooltip("Pushes the intensity of all colors.")]
	[DisplayName("Saturation")]
	[Range(-100f, 100f)]
	public FloatParameter saturation;

	[DisplayName("Brightness")]
	[Range(-100f, 100f)]
	[Tooltip("Makes the image brighter or darker.")]
	public FloatParameter brightness;

	[DisplayName("Post-exposure (EV)")]
	[Tooltip("Adjusts the overall exposure of the scene in EV units. This is applied after the HDR effect and right before tonemapping so it won't affect previous effects in the chain.")]
	public FloatParameter postExposure;

	[DisplayName("Contrast")]
	[Range(-100f, 100f)]
	[Tooltip("Expands or shrinks the overall range of tonal values.")]
	public FloatParameter contrast;

	[DisplayName("Mode")]
	[Tooltip("Select masking type to avoid applying grading to certain areas.")]
	public MaskingModeParameter maskMode;

	[Tooltip("Mask intensity.")]
	[DisplayName("Intensity")]
	[Range(0f, 10f)]
	public FloatParameter maskIntensity;

	[Range(-200f, 200f)]
	[DisplayName("Red")]
	[Tooltip("Modify influence of the red channel in the overall mix.")]
	public FloatParameter mixerRedOutRedIn;

	[Range(-200f, 200f)]
	[DisplayName("Green")]
	[Tooltip("Modify influence of the green channel in the overall mix.")]
	public FloatParameter mixerRedOutGreenIn;

	[DisplayName("Blue")]
	[Tooltip("Modify influence of the blue channel in the overall mix.")]
	[Range(-200f, 200f)]
	public FloatParameter mixerRedOutBlueIn;

	[Range(-200f, 200f)]
	[Tooltip("Modify influence of the red channel in the overall mix.")]
	[DisplayName("Red")]
	public FloatParameter mixerGreenOutRedIn;

	[DisplayName("Green")]
	[Range(-200f, 200f)]
	[Tooltip("Modify influence of the green channel in the overall mix.")]
	public FloatParameter mixerGreenOutGreenIn;

	[Range(-200f, 200f)]
	[DisplayName("Blue")]
	[Tooltip("Modify influence of the blue channel in the overall mix.")]
	public FloatParameter mixerGreenOutBlueIn;

	[DisplayName("Red")]
	[Range(-200f, 200f)]
	[Tooltip("Modify influence of the red channel in the overall mix.")]
	public FloatParameter mixerBlueOutRedIn;

	[Tooltip("Modify influence of the green channel in the overall mix.")]
	[Range(-200f, 200f)]
	[DisplayName("Green")]
	public FloatParameter mixerBlueOutGreenIn;

	[DisplayName("Blue")]
	[Range(-200f, 200f)]
	[Tooltip("Modify influence of the blue channel in the overall mix.")]
	public FloatParameter mixerBlueOutBlueIn;

	[DisplayName("Lift")]
	[Tooltip("Controls the darkest portions of the render.")]
	[Trackball(TrackballAttribute.Mode.Lift)]
	public Vector4Parameter lift;

	[DisplayName("Gamma")]
	[Tooltip("Power function that controls mid-range tones.")]
	[Trackball(TrackballAttribute.Mode.Gamma)]
	public Vector4Parameter gamma;

	[DisplayName("Gain")]
	[Tooltip("Controls the lightest portions of the render.")]
	[Trackball(TrackballAttribute.Mode.Gain)]
	public Vector4Parameter gain;

	public SplineParameter masterCurve;

	public SplineParameter redCurve;

	public SplineParameter greenCurve;

	public SplineParameter blueCurve;

	public SplineParameter hueVsHueCurve;

	public SplineParameter hueVsSatCurve;

	public SplineParameter satVsSatCurve;

	public SplineParameter lumVsSatCurve;

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (gradingMode.value == GradingMode.External && (!SystemInfo.supports3DRenderTextures || !SystemInfo.supportsComputeShaders))
		{
			return false;
		}
		return enabled.value;
	}

	public ColorGrading()
	{
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Expected O, but got Unknown
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Expected O, but got Unknown
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Expected O, but got Unknown
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Expected O, but got Unknown
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_050b: Expected O, but got Unknown
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0531: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Expected O, but got Unknown
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0561: Unknown result type (might be due to invalid IL or missing references)
		//IL_056b: Expected O, but got Unknown
		//IL_057c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0591: Unknown result type (might be due to invalid IL or missing references)
		//IL_059b: Expected O, but got Unknown
		gradingMode = new GradingModeParameter
		{
			value = GradingMode.HighDefinitionRange
		};
		externalLut = new TextureParameter
		{
			value = null
		};
		tonemapper = new TonemapperParameter
		{
			value = Tonemapper.None
		};
		toneCurveToeStrength = new FloatParameter
		{
			value = 0f
		};
		toneCurveToeLength = new FloatParameter
		{
			value = 0.5f
		};
		toneCurveShoulderStrength = new FloatParameter
		{
			value = 0f
		};
		toneCurveShoulderLength = new FloatParameter
		{
			value = 0.5f
		};
		toneCurveShoulderAngle = new FloatParameter
		{
			value = 0f
		};
		toneCurveGamma = new FloatParameter
		{
			value = 1f
		};
		ldrLut = new TextureParameter
		{
			value = null,
			defaultState = TextureParameterDefault.Lut2D
		};
		ldrLutContribution = new FloatParameter
		{
			value = 1f
		};
		temperature = new FloatParameter
		{
			value = 0f
		};
		tint = new FloatParameter
		{
			value = 0f
		};
		colorFilter = new ColorParameter
		{
			value = Color.white
		};
		hueShift = new FloatParameter
		{
			value = 0f
		};
		saturation = new FloatParameter
		{
			value = 0f
		};
		brightness = new FloatParameter
		{
			value = 0f
		};
		postExposure = new FloatParameter
		{
			value = 0f
		};
		contrast = new FloatParameter
		{
			value = 0f
		};
		maskMode = new MaskingModeParameter
		{
			value = MaskingMode.None
		};
		maskIntensity = new FloatParameter
		{
			value = 1f
		};
		mixerRedOutRedIn = new FloatParameter
		{
			value = 100f
		};
		mixerRedOutGreenIn = new FloatParameter
		{
			value = 0f
		};
		mixerRedOutBlueIn = new FloatParameter
		{
			value = 0f
		};
		mixerGreenOutRedIn = new FloatParameter
		{
			value = 0f
		};
		mixerGreenOutGreenIn = new FloatParameter
		{
			value = 100f
		};
		mixerGreenOutBlueIn = new FloatParameter
		{
			value = 0f
		};
		mixerBlueOutRedIn = new FloatParameter
		{
			value = 0f
		};
		mixerBlueOutGreenIn = new FloatParameter
		{
			value = 0f
		};
		mixerBlueOutBlueIn = new FloatParameter
		{
			value = 100f
		};
		lift = new Vector4Parameter
		{
			value = new Vector4(1f, 1f, 1f, 0f)
		};
		gamma = new Vector4Parameter
		{
			value = new Vector4(1f, 1f, 1f, 0f)
		};
		gain = new Vector4Parameter
		{
			value = new Vector4(1f, 1f, 1f, 0f)
		};
		masterCurve = new SplineParameter
		{
			value = new Spline(new AnimationCurve((Keyframe[])(object)new Keyframe[2]
			{
				new Keyframe(0f, 0f, 1f, 1f),
				new Keyframe(1f, 1f, 1f, 1f)
			}), 0f, loop: false, new Vector2(0f, 1f))
		};
		redCurve = new SplineParameter
		{
			value = new Spline(new AnimationCurve((Keyframe[])(object)new Keyframe[2]
			{
				new Keyframe(0f, 0f, 1f, 1f),
				new Keyframe(1f, 1f, 1f, 1f)
			}), 0f, loop: false, new Vector2(0f, 1f))
		};
		greenCurve = new SplineParameter
		{
			value = new Spline(new AnimationCurve((Keyframe[])(object)new Keyframe[2]
			{
				new Keyframe(0f, 0f, 1f, 1f),
				new Keyframe(1f, 1f, 1f, 1f)
			}), 0f, loop: false, new Vector2(0f, 1f))
		};
		blueCurve = new SplineParameter
		{
			value = new Spline(new AnimationCurve((Keyframe[])(object)new Keyframe[2]
			{
				new Keyframe(0f, 0f, 1f, 1f),
				new Keyframe(1f, 1f, 1f, 1f)
			}), 0f, loop: false, new Vector2(0f, 1f))
		};
		hueVsHueCurve = new SplineParameter
		{
			value = new Spline(new AnimationCurve(), 0.5f, loop: true, new Vector2(0f, 1f))
		};
		hueVsSatCurve = new SplineParameter
		{
			value = new Spline(new AnimationCurve(), 0.5f, loop: true, new Vector2(0f, 1f))
		};
		satVsSatCurve = new SplineParameter
		{
			value = new Spline(new AnimationCurve(), 0.5f, loop: false, new Vector2(0f, 1f))
		};
		lumVsSatCurve = new SplineParameter
		{
			value = new Spline(new AnimationCurve(), 0.5f, loop: false, new Vector2(0f, 1f))
		};
		base._002Ector();
	}
}
