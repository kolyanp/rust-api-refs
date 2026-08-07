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
	public PostProcessLayer.Antialiasing antialiasing = PostProcessLayer.Antialiasing.TemporalAntialiasing;

	[Header("Shadows")]
	[Range(1f, 4f)]
	public int shadowCascades = 4;

	[Range(1f, 3f)]
	public int shadowLights = 3;

	[Range(1f, 4f)]
	public int shadowMode = 4;

	public float shadowDistance = 50f;

	[Range(0f, 3f)]
	public int shadowQuality = 3;

	[Range(0f, 0.02f)]
	public float shadowSoftness = 0.01f;

	[Range(0f, 2f)]
	public float sunShadowBias = 0.01f;

	[Header("                ")]
	[Header("Contact Shadows")]
	public bool enableContactShadows = true;

	[Range(0f, 1f)]
	public float blendStrength = 1f;

	[Range(0f, 1f)]
	public float accumulation = 0.9f;

	[Range(0.1f, 5f)]
	public float lengthFade = 0.7f;

	[Range(0.01f, 5f)]
	public float range = 0.7f;

	[Range(0f, 1f)]
	public float zThickness = 0.1f;

	[Range(2f, 92f)]
	public int samples = 32;

	[Range(0.5f, 4f)]
	public float nearSampleQuality = 1.5f;

	[Range(0f, 1f)]
	public float traceBias = 0.03f;

	[Header("Ambient Occlusion")]
	[Header("                ")]
	public bool enableAmbientOcclusion = true;

	public SampleCountLevel SampleCount = SampleCountLevel.Medium;

	public float Intensity = 1f;

	public Color Tint = Color.black;

	[Tooltip("Radius spread of the occlusion.")]
	[Range(0f, 32f)]
	public float Radius = 2f;

	[Tooltip("Power exponent attenuation of the occlusion.")]
	[Range(0f, 16f)]
	public float PowerExponent = 1.8f;

	[Range(0f, 0.99f)]
	[Tooltip("Controls the initial occlusion contribution offset.")]
	public float Bias = 0.05f;

	[Range(0f, 1f)]
	[Tooltip("Controls the thickness occlusion contribution.")]
	public float Thickness = 1f;

	[Tooltip("Compute the Occlusion and Blur at half of the resolution.")]
	public bool Downsample = true;

	[Tooltip("Control parameters at faraway.")]
	public bool FadeEnabled;

	[Tooltip("Distance in Unity unities that start to fade.")]
	public float FadeStart = 100f;

	[Tooltip("Length distance to performe the transition.")]
	public float FadeLength = 50f;

	[Tooltip("Final Intensity parameter.")]
	[Range(0f, 1f)]
	public float FadeToIntensity;

	public Color FadeToTint = Color.black;

	[Range(0f, 32f)]
	[Tooltip("Final Radius parameter.")]
	public float FadeToRadius = 2f;

	[Tooltip("Final PowerExponent parameter.")]
	[Range(0f, 16f)]
	public float FadeToPowerExponent = 1.8f;

	[Range(0f, 1f)]
	[Tooltip("Final Thickness parameter.")]
	public float FadeToThickness = 1f;

	[Header("                ")]
	[Header("Reflections")]
	public ReflectionProbeMode Type = (ReflectionProbeMode)1;

	public ReflectionProbeRefreshMode RefreshMode = (ReflectionProbeRefreshMode)1;

	public ReflectionProbeTimeSlicingMode TimeSlicing = (ReflectionProbeTimeSlicingMode)1;

	public renderResolution resolution;

	public float reflectionShadowDistance = 100f;

	public LayerMask cullingMask;

	public float reflectionClippingPlaneNear = 0.1f;

	public float reflectionClippingPlaneFar = 100f;
}
