using System;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects;

[AddComponentMenu("Image Effects/Cinematic/Tonemapping and Color Grading")]
[ImageEffectAllowedInSceneView]
[ExecuteInEditMode]
public class TonemappingColorGrading : MonoBehaviour
{
	[AttributeUsage(AttributeTargets.Field)]
	public class SettingsGroup : Attribute
	{
	}

	public class IndentedGroup : PropertyAttribute
	{
	}

	public class ChannelMixer : PropertyAttribute
	{
	}

	public class ColorWheelGroup : PropertyAttribute
	{
		public int minSizePerWheel = 60;

		public int maxSizePerWheel = 150;

		public ColorWheelGroup()
		{
		}

		public ColorWheelGroup(int minSizePerWheel, int maxSizePerWheel)
		{
			this.minSizePerWheel = minSizePerWheel;
			this.maxSizePerWheel = maxSizePerWheel;
		}
	}

	public class Curve : PropertyAttribute
	{
		public Color color;

		public Curve()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			color = Color.white;
			((PropertyAttribute)this)._002Ector();
		}

		public Curve(float r, float g, float b, float a)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			color = Color.white;
			((PropertyAttribute)this)._002Ector();
			color = new Color(r, g, b, a);
		}
	}

	[Serializable]
	public struct EyeAdaptationSettings
	{
		public bool enabled;

		[Tooltip("Midpoint Adjustment.")]
		[Min(0f)]
		public float middleGrey;

		[Tooltip("The lowest possible exposure value; adjust this value to modify the brightest areas of your level.")]
		public float min;

		[Tooltip("The highest possible exposure value; adjust this value to modify the darkest areas of your level.")]
		public float max;

		[Tooltip("Speed of linear adaptation. Higher is faster.")]
		[Min(0f)]
		public float speed;

		[Tooltip("Displays a luminosity helper in the GameView.")]
		public bool showDebug;

		public static EyeAdaptationSettings defaultSettings => new EyeAdaptationSettings
		{
			enabled = false,
			showDebug = false,
			middleGrey = 0.5f,
			min = -3f,
			max = 3f,
			speed = 1.5f
		};
	}

	public enum Tonemapper
	{
		ACES,
		Curve,
		Hable,
		HejlDawson,
		Photographic,
		Reinhard,
		Neutral
	}

	[Serializable]
	public struct TonemappingSettings
	{
		public bool enabled;

		[Tooltip("Tonemapping technique to use. ACES is the recommended one.")]
		public Tonemapper tonemapper;

		[Min(0f)]
		[Tooltip("Adjusts the overall exposure of the scene.")]
		public float exposure;

		[Tooltip("Custom tonemapping curve.")]
		public AnimationCurve curve;

		[Range(-0.1f, 0.1f)]
		public float neutralBlackIn;

		[Range(1f, 20f)]
		public float neutralWhiteIn;

		[Range(-0.09f, 0.1f)]
		public float neutralBlackOut;

		[Range(1f, 19f)]
		public float neutralWhiteOut;

		[Range(0.1f, 20f)]
		public float neutralWhiteLevel;

		[Range(1f, 10f)]
		public float neutralWhiteClip;

		public static TonemappingSettings defaultSettings => new TonemappingSettings
		{
			enabled = false,
			tonemapper = Tonemapper.Neutral,
			exposure = 1f,
			curve = CurvesSettings.defaultCurve,
			neutralBlackIn = 0.02f,
			neutralWhiteIn = 10f,
			neutralBlackOut = 0f,
			neutralWhiteOut = 10f,
			neutralWhiteLevel = 5.3f,
			neutralWhiteClip = 10f
		};
	}

	[Serializable]
	public struct LUTSettings
	{
		public bool enabled;

		[Tooltip("Custom lookup texture (strip format, e.g. 256x16).")]
		public Texture texture;

		[Range(0f, 1f)]
		[Tooltip("Blending factor.")]
		public float contribution;

		public static LUTSettings defaultSettings => new LUTSettings
		{
			enabled = false,
			texture = null,
			contribution = 1f
		};
	}

	[Serializable]
	public struct ColorWheelsSettings
	{
		[ColorUsage(false)]
		public Color shadows;

		[ColorUsage(false)]
		public Color midtones;

		[ColorUsage(false)]
		public Color highlights;

		public static ColorWheelsSettings defaultSettings
		{
			get
			{
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				return new ColorWheelsSettings
				{
					shadows = Color.white,
					midtones = Color.white,
					highlights = Color.white
				};
			}
		}
	}

	[Serializable]
	public struct BasicsSettings
	{
		[Tooltip("Sets the white balance to a custom color temperature.")]
		[Range(-2f, 2f)]
		public float temperatureShift;

		[Range(-2f, 2f)]
		[Tooltip("Sets the white balance to compensate for a green or magenta tint.")]
		public float tint;

		[Space]
		[Range(-0.5f, 0.5f)]
		[Tooltip("Shift the hue of all colors.")]
		public float hue;

		[Range(0f, 2f)]
		[Tooltip("Pushes the intensity of all colors.")]
		public float saturation;

		[Tooltip("Adjusts the saturation so that clipping is minimized as colors approach full saturation.")]
		[Range(-1f, 1f)]
		public float vibrance;

		[Tooltip("Brightens or darkens all colors.")]
		[Range(0f, 10f)]
		public float value;

		[Tooltip("Expands or shrinks the overall range of tonal values.")]
		[Range(0f, 2f)]
		[Space]
		public float contrast;

		[Tooltip("Contrast gain curve. Controls the steepness of the curve.")]
		[Range(0.01f, 5f)]
		public float gain;

		[Tooltip("Applies a pow function to the source.")]
		[Range(0.01f, 5f)]
		public float gamma;

		public static BasicsSettings defaultSettings => new BasicsSettings
		{
			temperatureShift = 0f,
			tint = 0f,
			contrast = 1f,
			hue = 0f,
			saturation = 1f,
			value = 1f,
			vibrance = 0f,
			gain = 1f,
			gamma = 1f
		};
	}

	[Serializable]
	public struct ChannelMixerSettings
	{
		public int currentChannel;

		public Vector3[] channels;

		public static ChannelMixerSettings defaultSettings
		{
			get
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Unknown result type (might be due to invalid IL or missing references)
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0064: Unknown result type (might be due to invalid IL or missing references)
				return new ChannelMixerSettings
				{
					currentChannel = 0,
					channels = (Vector3[])(object)new Vector3[3]
					{
						new Vector3(1f, 0f, 0f),
						new Vector3(0f, 1f, 0f),
						new Vector3(0f, 0f, 1f)
					}
				};
			}
		}
	}

	[Serializable]
	public struct CurvesSettings
	{
		[Curve]
		public AnimationCurve master;

		[Curve(1f, 0f, 0f, 1f)]
		public AnimationCurve red;

		[Curve(0f, 1f, 0f, 1f)]
		public AnimationCurve green;

		[Curve(0f, 1f, 1f, 1f)]
		public AnimationCurve blue;

		public static CurvesSettings defaultSettings => new CurvesSettings
		{
			master = defaultCurve,
			red = defaultCurve,
			green = defaultCurve,
			blue = defaultCurve
		};

		public static AnimationCurve defaultCurve
		{
			get
			{
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_003c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_004c: Expected O, but got Unknown
				return new AnimationCurve((Keyframe[])(object)new Keyframe[2]
				{
					new Keyframe(0f, 0f, 1f, 1f),
					new Keyframe(1f, 1f, 1f, 1f)
				});
			}
		}
	}

	public enum ColorGradingPrecision
	{
		Normal = 0x10,
		High = 0x20
	}

	[Serializable]
	public struct ColorGradingSettings
	{
		public bool enabled;

		[Tooltip("Internal LUT precision. \"Normal\" is 256x16, \"High\" is 1024x32. Prefer \"Normal\" on mobile devices.")]
		public ColorGradingPrecision precision;

		[Space]
		[ColorWheelGroup]
		public ColorWheelsSettings colorWheels;

		[Space]
		[IndentedGroup]
		public BasicsSettings basics;

		[ChannelMixer]
		[Space]
		public ChannelMixerSettings channelMixer;

		[Space]
		[IndentedGroup]
		public CurvesSettings curves;

		[Space]
		[Tooltip("Use dithering to try and minimize color banding in dark areas.")]
		public bool useDithering;

		[Tooltip("Displays the generated LUT in the top left corner of the GameView.")]
		public bool showDebug;

		public static ColorGradingSettings defaultSettings => new ColorGradingSettings
		{
			enabled = false,
			useDithering = false,
			showDebug = false,
			precision = ColorGradingPrecision.Normal,
			colorWheels = ColorWheelsSettings.defaultSettings,
			basics = BasicsSettings.defaultSettings,
			channelMixer = ChannelMixerSettings.defaultSettings,
			curves = CurvesSettings.defaultSettings
		};

		internal void Reset()
		{
			curves = CurvesSettings.defaultSettings;
		}
	}

	[SettingsGroup]
	[SerializeField]
	private EyeAdaptationSettings m_EyeAdaptation = EyeAdaptationSettings.defaultSettings;

	[SettingsGroup]
	[SerializeField]
	private TonemappingSettings m_Tonemapping = TonemappingSettings.defaultSettings;

	[SettingsGroup]
	[SerializeField]
	private ColorGradingSettings m_ColorGrading = ColorGradingSettings.defaultSettings;

	[SerializeField]
	[SettingsGroup]
	private LUTSettings m_Lut = LUTSettings.defaultSettings;

	[SerializeField]
	private Shader m_Shader;
}
