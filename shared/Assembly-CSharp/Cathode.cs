using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(CathodeRenderer), PostProcessEvent.AfterStack, "Custom/Cathode - Analogue Video", true)]
public class Cathode : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter intensity;

	[Range(1f, 16f)]
	public IntParameter downscale;

	[Range(1f, 16f)]
	public IntParameter downscaleTemporal;

	[Range(0f, 3f)]
	public FloatParameter horizontalBlur;

	[Range(0f, 3f)]
	public FloatParameter verticalBlur;

	[Range(0f, 5f)]
	public FloatParameter chromaSubsampling;

	[Range(0f, 5f)]
	public FloatParameter sharpen;

	[Range(0f, 5f)]
	public FloatParameter sharpenRadius;

	[Range(0f, 0.5f)]
	public FloatParameter colorNoise;

	[Range(0f, 5f)]
	public FloatParameter restlessFoot;

	[Range(0f, 0.1f)]
	public FloatParameter footAmplitude;

	[Range(0f, 3f)]
	public FloatParameter chromaIntensity;

	[Range(0f, 1f)]
	public FloatParameter chromaInstability;

	[Range(0f, 0.1f)]
	public FloatParameter chromaOffset;

	[Range(-2f, 2f)]
	public FloatParameter responseCurve;

	[Range(-1f, 1f)]
	public FloatParameter saturation;

	[Range(0f, 1f)]
	public FloatParameter cometTrailing;

	[Range(0f, 1f)]
	public FloatParameter burnIn;

	[Range(0f, 1f)]
	public FloatParameter tapeDust;

	[Range(0f, 2f)]
	public FloatParameter wobble;

	[Range(0f, 1f)]
	public Vector2Parameter blackWhiteLevels;

	[Range(0f, 1f)]
	public Vector2Parameter dynamicRange;

	[Range(-1f, 1f)]
	public FloatParameter whiteBallance;

	public Cathode()
	{
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		intensity = new FloatParameter
		{
			value = 0f
		};
		downscale = new IntParameter
		{
			value = 1
		};
		downscaleTemporal = new IntParameter
		{
			value = 1
		};
		horizontalBlur = new FloatParameter
		{
			value = 1f
		};
		verticalBlur = new FloatParameter
		{
			value = 1f
		};
		chromaSubsampling = new FloatParameter
		{
			value = 1.7f
		};
		sharpen = new FloatParameter
		{
			value = 1.2f
		};
		sharpenRadius = new FloatParameter
		{
			value = 1.2f
		};
		colorNoise = new FloatParameter
		{
			value = 0.05f
		};
		restlessFoot = new FloatParameter
		{
			value = 0.2f
		};
		footAmplitude = new FloatParameter
		{
			value = 0.02f
		};
		chromaIntensity = new FloatParameter
		{
			value = 1f
		};
		chromaInstability = new FloatParameter
		{
			value = 1f
		};
		chromaOffset = new FloatParameter
		{
			value = 0.02f
		};
		responseCurve = new FloatParameter
		{
			value = 0f
		};
		saturation = new FloatParameter
		{
			value = 1f
		};
		cometTrailing = new FloatParameter
		{
			value = 0.3f
		};
		burnIn = new FloatParameter
		{
			value = 0.1f
		};
		tapeDust = new FloatParameter
		{
			value = 0.1f
		};
		wobble = new FloatParameter
		{
			value = 1f
		};
		blackWhiteLevels = new Vector2Parameter
		{
			value = new Vector2(0f, 1f)
		};
		dynamicRange = new Vector2Parameter
		{
			value = new Vector2(0f, 1f)
		};
		whiteBallance = new FloatParameter
		{
			value = 0f
		};
		base._002Ector();
	}
}
