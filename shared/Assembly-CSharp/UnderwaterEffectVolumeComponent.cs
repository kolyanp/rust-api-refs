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

	[Range(0f, 2f)]
	[Header("Blur")]
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

	public UnderwaterEffectVolumeComponent()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected Obj, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected Obj, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected Obj, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected Obj, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected Obj, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected Obj, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected Obj, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected Obj, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected Obj, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected Obj, but got Unknown
	}
}
