using System;
using AmplifyOcclusion;
using UnityEngine;

[AddComponentMenu("")]
public class AmplifyOcclusionBase : MonoBehaviour
{
	[Header("Ambient Occlusion")]
	public ApplicationMethod ApplyMethod;

	[Tooltip("Number of samples per pass.")]
	public SampleCountLevel SampleCount;

	public PerPixelNormalSource PerPixelNormals;

	[Range(0f, 1f)]
	[Tooltip("Final applied intensity of the occlusion effect.")]
	public float Intensity;

	public Color Tint;

	[Range(0f, 32f)]
	[Tooltip("Radius spread of the occlusion.")]
	public float Radius;

	[NonSerialized]
	[Range(32f, 1024f)]
	[Tooltip("Max sampling range in pixels.")]
	public int PixelRadiusLimit;

	[NonSerialized]
	[Range(0f, 2f)]
	[Tooltip("Occlusion contribution amount on relation to radius.")]
	public float RadiusIntensity;

	[Tooltip("Power exponent attenuation of the occlusion.")]
	[Range(0f, 16f)]
	public float PowerExponent;

	[Tooltip("Controls the initial occlusion contribution offset.")]
	[Range(0f, 0.99f)]
	public float Bias;

	[Tooltip("Controls the thickness occlusion contribution.")]
	[Range(0f, 1f)]
	public float Thickness;

	[Tooltip("Compute the Occlusion and Blur at half of the resolution.")]
	public bool Downsample;

	[Tooltip("Control parameters at faraway.")]
	[Header("Distance Fade")]
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

	[Range(0f, 1f)]
	[Tooltip("Final Thickness parameter.")]
	public float FadeToThickness;

	[Header("Bilateral Blur")]
	public bool BlurEnabled;

	[Tooltip("Radius in screen pixels.")]
	[Range(1f, 4f)]
	public int BlurRadius;

	[Tooltip("Number of times that the Blur will repeat.")]
	[Range(1f, 4f)]
	public int BlurPasses;

	[Tooltip("0 - Blured, 1 - Sharpened.")]
	[Range(0f, 20f)]
	public float BlurSharpness;

	[Tooltip("Accumulates the effect over the time.")]
	[Header("Temporal Filter")]
	public bool FilterEnabled;

	[Range(0f, 1f)]
	[Tooltip("Controls the accumulation decayment. 0 - Faster update, more flicker. 1 - Slow update (ghosting on moving objects), less flicker.")]
	public float FilterBlending;

	[Tooltip("Controls the discard sensibility based on the motion of the scene and objects. 0 - Discard less, reuse more (more ghost effect). 1 - Discard more, reuse less (less ghost effect).")]
	[Range(0f, 1f)]
	public float FilterResponse;

	[NonSerialized]
	[Tooltip("Enables directional variations.")]
	public bool TemporalDirections;

	[NonSerialized]
	[Tooltip("Enables offset variations.")]
	public bool TemporalOffsets;

	[NonSerialized]
	[Tooltip("Reduces ghosting effect near the objects's edges while moving.")]
	public bool TemporalDilation;

	[NonSerialized]
	[Tooltip("Uses the object movement information for calc new areas of occlusion.")]
	public bool UseMotionVectors;

	public AmplifyOcclusionBase()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		SampleCount = SampleCountLevel.Medium;
		PerPixelNormals = PerPixelNormalSource.Camera;
		Intensity = 1f;
		Tint = Color.black;
		Radius = 2f;
		PixelRadiusLimit = 512;
		RadiusIntensity = 1f;
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
		BlurEnabled = true;
		BlurRadius = 3;
		BlurPasses = 1;
		BlurSharpness = 10f;
		FilterEnabled = true;
		FilterBlending = 0.5f;
		FilterResponse = 0.5f;
		TemporalDirections = true;
		TemporalOffsets = true;
		UseMotionVectors = true;
		((MonoBehaviour)this)._002Ector();
	}
}
