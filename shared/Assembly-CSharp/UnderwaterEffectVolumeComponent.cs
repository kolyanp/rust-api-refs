using System;
using Rust.RenderPipeline.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(typeof(RustRenderPipelineAsset))]
[VolumeComponentMenu("RRP/Underwater Post Effect")]
public class UnderwaterEffectVolumeComponent : VolumeComponent, IPostProcessComponent
{
	[Header("Wiggle")]
	public BoolParameter wiggle;

	public FloatParameter speed;

	public FloatParameter scale;

	[Header("Water Line")]
	public ColorParameter waterLineColor;

	public IntParameter waterLineBlurIterations;

	public FloatParameter waterLineBlurSize;

	[Range(0f, 2f)]
	[Header("Blur")]
	public IntParameter downsample;

	[Range(1f, 4f)]
	public IntParameter blurIterations;

	[Range(0f, 10f)]
	public FloatParameter blurSize;

	public FloatParameter fadeToBlurDistance;

	[Header("General")]
	public BoolParameter effectActive;

	public bool IsActive()
	{
		if (base.active)
		{
			return ((VolumeParameter<bool>)(object)effectActive).value;
		}
		return false;
	}

	public UnderwaterEffectVolumeComponent()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		wiggle = new BoolParameter(true, false);
		speed = new FloatParameter(1f, false);
		scale = new FloatParameter(12f, false);
		waterLineColor = new ColorParameter(Color.white, false);
		waterLineBlurIterations = new IntParameter(1, false);
		waterLineBlurSize = new FloatParameter(0f, false);
		downsample = new IntParameter(0, false);
		blurIterations = new IntParameter(1, false);
		blurSize = new FloatParameter(0f, false);
		fadeToBlurDistance = new FloatParameter(0f, false);
		effectActive = new BoolParameter(false, false);
		((VolumeComponent)this)._002Ector();
	}
}
