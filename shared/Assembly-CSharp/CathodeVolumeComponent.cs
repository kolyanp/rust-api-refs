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

	public CathodeVolumeComponent()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected Obj, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected Obj, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected Obj, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected Obj, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected Obj, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected Obj, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected Obj, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected Obj, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected Obj, but got Unknown
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected Obj, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected Obj, but got Unknown
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected Obj, but got Unknown
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected Obj, but got Unknown
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Expected Obj, but got Unknown
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Expected Obj, but got Unknown
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Expected Obj, but got Unknown
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Expected Obj, but got Unknown
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Expected Obj, but got Unknown
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Expected Obj, but got Unknown
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Expected Obj, but got Unknown
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Expected Obj, but got Unknown
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Expected Obj, but got Unknown
	}
}
