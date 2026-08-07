using System;
using Rust.RenderPipeline.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(typeof(RustRenderPipelineAsset))]
[VolumeComponentMenu("RRP/Cathode")]
public class CathodeVolumeComponent : VolumeComponent, IPostProcessComponent
{
	public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f, false);

	public ClampedIntParameter downscale = new ClampedIntParameter(1, 1, 16, false);

	public ClampedIntParameter downscaleTemporal = new ClampedIntParameter(1, 1, 16, false);

	public ClampedFloatParameter horizontalBlur = new ClampedFloatParameter(1f, 0f, 3f, false);

	public ClampedFloatParameter verticalBlur = new ClampedFloatParameter(1f, 0f, 3f, false);

	public ClampedFloatParameter chromaSubsampling = new ClampedFloatParameter(1.7f, 0f, 5f, false);

	public ClampedFloatParameter sharpen = new ClampedFloatParameter(1.2f, 0f, 5f, false);

	public ClampedFloatParameter sharpenRadius = new ClampedFloatParameter(1.2f, 0f, 5f, false);

	public ClampedFloatParameter colorNoise = new ClampedFloatParameter(0.05f, 0f, 0.5f, false);

	public ClampedFloatParameter restlessFoot = new ClampedFloatParameter(0.2f, 0f, 5f, false);

	public ClampedFloatParameter footAmplitude = new ClampedFloatParameter(0.02f, 0f, 0.1f, false);

	public ClampedFloatParameter chromaIntensity = new ClampedFloatParameter(1f, 0f, 3f, false);

	public ClampedFloatParameter chromaInstability = new ClampedFloatParameter(1f, 0f, 1f, false);

	public ClampedFloatParameter chromaOffset = new ClampedFloatParameter(0.02f, 0f, 0.1f, false);

	public ClampedFloatParameter responseCurve = new ClampedFloatParameter(0f, -2f, 2f, false);

	public ClampedFloatParameter saturation = new ClampedFloatParameter(1f, -1f, 1f, false);

	public ClampedFloatParameter cometTrailing = new ClampedFloatParameter(0.3f, 0f, 1f, false);

	public ClampedFloatParameter burnIn = new ClampedFloatParameter(0.1f, 0f, 1f, false);

	public ClampedFloatParameter tapeDust = new ClampedFloatParameter(0.1f, 0f, 1f, false);

	public ClampedFloatParameter wobble = new ClampedFloatParameter(1f, 0f, 2f, false);

	public Vector2Parameter blackWhiteLevels = new Vector2Parameter(new Vector2(0f, 1f), false);

	public Vector2Parameter dynamicRange = new Vector2Parameter(new Vector2(0f, 1f), false);

	public ClampedFloatParameter whiteBalance = new ClampedFloatParameter(0f, -1f, 1f, false);

	public bool IsActive()
	{
		if (base.active)
		{
			return ((VolumeParameter<float>)(object)intensity).value > 0f;
		}
		return false;
	}
}
