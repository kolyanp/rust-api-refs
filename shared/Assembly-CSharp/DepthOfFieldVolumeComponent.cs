using System;
using Rust.RenderPipeline.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("RRP/Depth of Field")]
[SupportedOnRenderPipeline(typeof(RustRenderPipelineAsset))]
public class DepthOfFieldVolumeComponent : VolumeComponent, IPostProcessComponent
{
	public FloatParameter focalLength;

	public FloatParameter focalSize;

	public FloatParameter aperture;

	[Range(0f, 3f)]
	public FloatParameter anamorphicSqueeze;

	[Range(0f, 1f)]
	public FloatParameter anamorphicBarrel;

	public FloatParameter maxBlurSize;

	public BoolParameter highResolution;

	public BoolParameter enabled;

	public DOFBlurSampleCountParameter_RRP blurSampleCount;

	public Transform focalTransform;

	public bool IsActive()
	{
		if (base.active)
		{
			return ((VolumeParameter<bool>)(object)enabled).value;
		}
		return false;
	}

	public DepthOfFieldVolumeComponent()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		focalLength = new FloatParameter(10f, false);
		focalSize = new FloatParameter(0.05f, false);
		aperture = new FloatParameter(11.5f, false);
		anamorphicSqueeze = new FloatParameter(0f, false);
		anamorphicBarrel = new FloatParameter(0f, false);
		maxBlurSize = new FloatParameter(2f, false);
		highResolution = new BoolParameter(true, false);
		enabled = new BoolParameter(false, false);
		blurSampleCount = new DOFBlurSampleCountParameter_RRP
		{
			value = DOFBlurSampleCount_RRP.Low
		};
		((VolumeComponent)this)._002Ector();
	}
}
