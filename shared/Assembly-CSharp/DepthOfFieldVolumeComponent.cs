using System;
using Rust.RenderPipeline.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(typeof(RustRenderPipelineAsset))]
[VolumeComponentMenu("RRP/Depth of Field")]
public class DepthOfFieldVolumeComponent : VolumeComponent, IPostProcessComponent
{
	public FloatParameter focalLength = new FloatParameter(10f, false);

	public FloatParameter focalSize = new FloatParameter(0.05f, false);

	public FloatParameter aperture = new FloatParameter(11.5f, false);

	[Range(0f, 3f)]
	public FloatParameter anamorphicSqueeze = new FloatParameter(0f, false);

	[Range(0f, 1f)]
	public FloatParameter anamorphicBarrel = new FloatParameter(0f, false);

	public FloatParameter maxBlurSize = new FloatParameter(2f, false);

	public BoolParameter highResolution = new BoolParameter(true, false);

	public BoolParameter enabled = new BoolParameter(false, false);

	public DOFBlurSampleCountParameter_RRP blurSampleCount = new DOFBlurSampleCountParameter_RRP
	{
		value = DOFBlurSampleCount_RRP.Low
	};

	public Transform focalTransform;

	public bool IsActive()
	{
		if (base.active)
		{
			return ((VolumeParameter<bool>)(object)enabled).value;
		}
		return false;
	}
}
