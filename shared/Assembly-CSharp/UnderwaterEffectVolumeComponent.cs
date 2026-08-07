using System;
using Rust.RenderPipeline.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("RRP/Underwater Post Effect")]
[SupportedOnRenderPipeline(typeof(RustRenderPipelineAsset))]
public class UnderwaterEffectVolumeComponent : VolumeComponent, IPostProcessComponent
{
	[Header("Wiggle")]
	public BoolParameter wiggle = new BoolParameter(true, false);

	public FloatParameter speed = new FloatParameter(1f, false);

	public FloatParameter scale = new FloatParameter(12f, false);

	[Header("Water Line")]
	public ColorParameter waterLineColor = new ColorParameter(Color.white, false);

	public IntParameter waterLineBlurIterations = new IntParameter(1, false);

	public FloatParameter waterLineBlurSize = new FloatParameter(0f, false);

	[Header("Blur")]
	[Range(0f, 2f)]
	public IntParameter downsample = new IntParameter(0, false);

	[Range(1f, 4f)]
	public IntParameter blurIterations = new IntParameter(1, false);

	[Range(0f, 10f)]
	public FloatParameter blurSize = new FloatParameter(0f, false);

	public FloatParameter fadeToBlurDistance = new FloatParameter(0f, false);

	[Header("General")]
	public BoolParameter effectActive = new BoolParameter(false, false);

	public bool IsActive()
	{
		if (base.active)
		{
			return ((VolumeParameter<bool>)(object)effectActive).value;
		}
		return false;
	}
}
