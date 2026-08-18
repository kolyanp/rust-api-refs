using System;
using Rust.RenderPipeline.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(typeof(RustRenderPipelineAsset))]
[VolumeComponentMenu("RRP/Cathode")]
public class CathodeVolumeComponent : VolumeComponent, IPostProcessComponent
{
	public ClampedFloatParameter intensity;

	public ClampedIntParameter downscale;

	public ClampedIntParameter downscaleTemporal;

	public ClampedFloatParameter horizontalBlur;

	public ClampedFloatParameter verticalBlur;

	public ClampedFloatParameter chromaSubsampling;

	public ClampedFloatParameter sharpen;

	public ClampedFloatParameter sharpenRadius;

	public ClampedFloatParameter colorNoise;

	public ClampedFloatParameter restlessFoot;

	public ClampedFloatParameter footAmplitude;

	public ClampedFloatParameter chromaIntensity;

	public ClampedFloatParameter chromaInstability;

	public ClampedFloatParameter chromaOffset;

	public ClampedFloatParameter responseCurve;

	public ClampedFloatParameter saturation;

	public ClampedFloatParameter cometTrailing;

	public ClampedFloatParameter burnIn;

	public ClampedFloatParameter tapeDust;

	public ClampedFloatParameter wobble;

	public Vector2Parameter blackWhiteLevels;

	public Vector2Parameter dynamicRange;

	public ClampedFloatParameter whiteBalance;

	public bool IsActive()
	{
		if (base.active)
		{
			return ((VolumeParameter<float>)(object)intensity).value > 0f;
		}
		return false;
	}

	public CathodeVolumeComponent()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected O, but got Unknown
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Expected O, but got Unknown
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Expected O, but got Unknown
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Expected O, but got Unknown
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Expected O, but got Unknown
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Expected O, but got Unknown
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Expected O, but got Unknown
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Expected O, but got Unknown
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Expected O, but got Unknown
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Expected O, but got Unknown
		intensity = new ClampedFloatParameter(0f, 0f, 1f, false);
		downscale = new ClampedIntParameter(1, 1, 16, false);
		downscaleTemporal = new ClampedIntParameter(1, 1, 16, false);
		horizontalBlur = new ClampedFloatParameter(1f, 0f, 3f, false);
		verticalBlur = new ClampedFloatParameter(1f, 0f, 3f, false);
		chromaSubsampling = new ClampedFloatParameter(1.7f, 0f, 5f, false);
		sharpen = new ClampedFloatParameter(1.2f, 0f, 5f, false);
		sharpenRadius = new ClampedFloatParameter(1.2f, 0f, 5f, false);
		colorNoise = new ClampedFloatParameter(0.05f, 0f, 0.5f, false);
		restlessFoot = new ClampedFloatParameter(0.2f, 0f, 5f, false);
		footAmplitude = new ClampedFloatParameter(0.02f, 0f, 0.1f, false);
		chromaIntensity = new ClampedFloatParameter(1f, 0f, 3f, false);
		chromaInstability = new ClampedFloatParameter(1f, 0f, 1f, false);
		chromaOffset = new ClampedFloatParameter(0.02f, 0f, 0.1f, false);
		responseCurve = new ClampedFloatParameter(0f, -2f, 2f, false);
		saturation = new ClampedFloatParameter(1f, -1f, 1f, false);
		cometTrailing = new ClampedFloatParameter(0.3f, 0f, 1f, false);
		burnIn = new ClampedFloatParameter(0.1f, 0f, 1f, false);
		tapeDust = new ClampedFloatParameter(0.1f, 0f, 1f, false);
		wobble = new ClampedFloatParameter(1f, 0f, 2f, false);
		blackWhiteLevels = new Vector2Parameter(new Vector2(0f, 1f), false);
		dynamicRange = new Vector2Parameter(new Vector2(0f, 1f), false);
		whiteBalance = new ClampedFloatParameter(0f, -1f, 1f, false);
		((VolumeComponent)this)._002Ector();
	}
}
