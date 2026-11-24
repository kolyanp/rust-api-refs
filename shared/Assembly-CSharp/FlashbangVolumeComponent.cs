using System;
using Rust.RenderPipeline.Runtime;
using UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(typeof(RustRenderPipelineAsset))]
[VolumeComponentMenu("RRP/Flashbang")]
public class FlashbangVolumeComponent : VolumeComponent, IPostProcessComponent
{
	private const float ActivationThreshold = 0.001f;

	public ClampedFloatParameter burnIntensity = new ClampedFloatParameter(0f, 0f, 1f, false);

	public ClampedFloatParameter whiteoutIntensity = new ClampedFloatParameter(0f, 0f, 1f, false);

	public bool IsActive()
	{
		if (base.active)
		{
			if (!(((VolumeParameter<float>)(object)burnIntensity).value > 0.001f))
			{
				return ((VolumeParameter<float>)(object)whiteoutIntensity).value > 0.001f;
			}
			return true;
		}
		return false;
	}

	public FlashbangVolumeComponent()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected Obj, but got Unknown
	}
}
