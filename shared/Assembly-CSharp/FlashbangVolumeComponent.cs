using System;
using Rust.RenderPipeline.Runtime;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("RRP/Flashbang")]
[SupportedOnRenderPipeline(typeof(RustRenderPipelineAsset))]
public class FlashbangVolumeComponent : VolumeComponent, IPostProcessComponent
{
	private const float ActivationThreshold = 0.001f;

	public ClampedFloatParameter burnIntensity;

	public ClampedFloatParameter whiteoutIntensity;

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
		//IL_001b: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		burnIntensity = new ClampedFloatParameter(0f, 0f, 1f, false);
		whiteoutIntensity = new ClampedFloatParameter(0f, 0f, 1f, false);
		((VolumeComponent)this)._002Ector();
	}
}
