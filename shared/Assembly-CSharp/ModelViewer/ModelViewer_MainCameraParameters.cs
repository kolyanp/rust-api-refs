using System;
using UnityEngine;

namespace ModelViewer;

[Serializable]
public class ModelViewer_MainCameraParameters
{
	public enum OverlayType
	{
		None,
		RuleOfThirds,
		GoldenRatio,
		CrossHair,
		AspectRatio,
		SafeFrames
	}

	public enum TonemappingMode
	{
		None,
		Neutral,
		ACES
	}

	[Header("Control")]
	public float orbitSpeed;

	public float mouseZoomSpeed;

	public float scrollZoomSpeed;

	public float panSpeed;

	public float moveSpeed;

	public float followLag;

	[Header("Lens")]
	[Range(0.01f, 360f)]
	[Header("                ")]
	public float fieldOfView;

	public float nearPlane;

	public float farPlane;

	public OverlayType overlayType;

	[Range(0.01f, 100f)]
	public float Aspect;

	[Range(0.01f, 100f)]
	public float Ratio;

	[Header("Focus")]
	[Header("                ")]
	public bool depthOfField;

	public float focalLength;

	public float focalSize;

	public float apeture;

	public float maxBlurSize;

	public bool debugMode;

	[Header("                ")]
	[Range(0f, 360f)]
	[Header("Motion Blur")]
	public float shutterAngle;

	[Header("                ")]
	[Range(-100f, 100f)]
	[Header("Distortion")]
	public float barrelAndPincushion;

	[Range(0f, 1f)]
	public float chromaticAbberation;

	[Header("                ")]
	[Header("Post Effects")]
	public TonemappingMode ToneMapping;

	public float Exposure;

	[Range(-100f, 100f)]
	public float Contrast;

	[Range(-100f, 100f)]
	public float Saturation;

	[Header("                ")]
	[Header("Sharpen")]
	public float Strength;

	public float limit;

	[Header("                ")]
	[Header("God Rays")]
	public bool GodRays;

	[Header("Bloom")]
	[Header("                ")]
	public float Brightness;

	public float Threshold;

	[Range(0f, 1f)]
	public float SoftKnee;

	public float Clamp;

	[Range(0f, 10f)]
	public float Diffusion;

	[Range(0f, 1f)]
	public float AnamorphicRatio;

	public Color Color;

	[Header("Vignette")]
	[Header("                ")]
	public float Darkness;

	public float Sharpness;

	[Header("                ")]
	[Header("Grain")]
	public bool Coloured;

	[Range(0f, 1f)]
	public float Intensity;

	[Range(0.3f, 3f)]
	public float Size;

	[Range(0f, 1f)]
	public float luminanceContribution;

	public ModelViewer_MainCameraParameters()
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		orbitSpeed = 4f;
		mouseZoomSpeed = 1.2f;
		scrollZoomSpeed = 0.1f;
		panSpeed = 0.1f;
		moveSpeed = 0.05f;
		fieldOfView = 15f;
		nearPlane = 0.05f;
		farPlane = 2500f;
		Aspect = 4f;
		Ratio = 3f;
		focalLength = 3f;
		focalSize = 0.1f;
		apeture = 80f;
		maxBlurSize = 7f;
		ToneMapping = TonemappingMode.Neutral;
		Exposure = 2.34f;
		GodRays = true;
		Brightness = 0.15f;
		Threshold = 1f;
		SoftKnee = 0.5f;
		Diffusion = 8f;
		AnamorphicRatio = 0.55f;
		Color = Color.white;
		Coloured = true;
		Size = 1f;
		luminanceContribution = 0.8f;
		base._002Ector();
	}
}
