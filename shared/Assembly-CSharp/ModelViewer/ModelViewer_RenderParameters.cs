using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace ModelViewer;

[Serializable]
public class ModelViewer_RenderParameters
{
	public enum SampleCountLevel
	{
		Low,
		Medium,
		High,
		VeryHigh
	}

	public enum renderResolution
	{
		_16,
		_32,
		_64,
		_128,
		_256,
		_512,
		_1024,
		_2048
	}

	[Header("Anti Aliasing")]
	public PostProcessLayer.Antialiasing antialiasing;

	[Range(1f, 4f)]
	[Header("Shadows")]
	public int shadowCascades;

	[Range(1f, 3f)]
	public int shadowLights;

	[Range(1f, 4f)]
	public int shadowMode;

	public float shadowDistance;

	[Range(0f, 3f)]
	public int shadowQuality;

	[Range(0f, 0.02f)]
	public float shadowSoftness;

	[Range(0f, 2f)]
	public float sunShadowBias;

	[Header("                ")]
	[Header("Contact Shadows")]
	public bool enableContactShadows;

	[Range(0f, 1f)]
	public float blendStrength;

	[Range(0f, 1f)]
	public float accumulation;

	[Range(0.1f, 5f)]
	public float lengthFade;

	[Range(0.01f, 5f)]
	public float range;

	[Range(0f, 1f)]
	public float zThickness;

	[Range(2f, 92f)]
	public int samples;

	[Range(0.5f, 4f)]
	public float nearSampleQuality;

	[Range(0f, 1f)]
	public float traceBias;

	[Header("                ")]
	[Header("Ambient Occlusion")]
	public bool enableAmbientOcclusion;

	public SampleCountLevel SampleCount;

	public float Intensity;

	public Color Tint;

	[Range(0f, 32f)]
	[Tooltip("Radius spread of the occlusion.")]
	public float Radius;

	[Range(0f, 16f)]
	[Tooltip("Power exponent attenuation of the occlusion.")]
	public float PowerExponent;

	[Range(0f, 0.99f)]
	[Tooltip("Controls the initial occlusion contribution offset.")]
	public float Bias;

	[Tooltip("Controls the thickness occlusion contribution.")]
	[Range(0f, 1f)]
	public float Thickness;

	[Tooltip("Compute the Occlusion and Blur at half of the resolution.")]
	public bool Downsample;

	[Tooltip("Control parameters at faraway.")]
	public bool FadeEnabled;

	[Tooltip("Distance in Unity unities that start to fade.")]
	public float FadeStart;

	[Tooltip("Length distance to performe the transition.")]
	public float FadeLength;

	[Tooltip("Final Intensity parameter.")]
	[Range(0f, 1f)]
	public float FadeToIntensity;

	public Color FadeToTint;

	[Tooltip("Final Radius parameter.")]
	[Range(0f, 32f)]
	public float FadeToRadius;

	[Tooltip("Final PowerExponent parameter.")]
	[Range(0f, 16f)]
	public float FadeToPowerExponent;

	[Tooltip("Final Thickness parameter.")]
	[Range(0f, 1f)]
	public float FadeToThickness;

	[Header("                ")]
	[Header("Reflections")]
	public ReflectionProbeMode Type;

	public ReflectionProbeRefreshMode RefreshMode;

	public ReflectionProbeTimeSlicingMode TimeSlicing;

	public renderResolution resolution;

	public float reflectionShadowDistance;

	public LayerMask cullingMask;

	public float reflectionClippingPlaneNear;

	public float reflectionClippingPlaneFar;

	public ModelViewer_RenderParameters()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		antialiasing = PostProcessLayer.Antialiasing.TemporalAntialiasing;
		shadowCascades = 4;
		shadowLights = 3;
		shadowMode = 4;
		shadowDistance = 50f;
		shadowQuality = 3;
		shadowSoftness = 0.01f;
		sunShadowBias = 0.01f;
		enableContactShadows = true;
		blendStrength = 1f;
		accumulation = 0.9f;
		lengthFade = 0.7f;
		range = 0.7f;
		zThickness = 0.1f;
		samples = 32;
		nearSampleQuality = 1.5f;
		traceBias = 0.03f;
		enableAmbientOcclusion = true;
		SampleCount = SampleCountLevel.Medium;
		Intensity = 1f;
		Tint = Color.black;
		Radius = 2f;
		PowerExponent = 1.8f;
		Bias = 0.05f;
		Thickness = 1f;
		Downsample = true;
		FadeStart = 100f;
		FadeLength = 50f;
		FadeToTint = Color.black;
		FadeToRadius = 2f;
		FadeToPowerExponent = 1.8f;
		FadeToThickness = 1f;
		Type = (ReflectionProbeMode)1;
		RefreshMode = (ReflectionProbeRefreshMode)1;
		TimeSlicing = (ReflectionProbeTimeSlicingMode)1;
		reflectionShadowDistance = 100f;
		reflectionClippingPlaneNear = 0.1f;
		reflectionClippingPlaneFar = 100f;
		base._002Ector();
	}
}
