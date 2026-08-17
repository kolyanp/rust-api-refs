using Rust.RenderPipeline.Runtime;
using UnityEngine;

[RustRendererFeatureCameraComponent(typeof(VolumeCloudsCamera), typeof(VolumeCloudsCameraContext))]
[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/VolumeCloudsRendererFeature")]
public class VolumeCloudsRendererFeature : RustRendererFeature
{
	private VolumeCloudsShadowPass _shadowPass;

	private VolumeCloudsDrawPass _drawPass;

	private VolumeCloudsUpscalePass _upscalePass;

	private VolumeCloudsAtmosphericScatteringPass _scatteringPass;

	public override void AddRenderPasses(RustRenderer renderer)
	{
		renderer.FrameData.Create<VolumeCloudsFrameData>();
		renderer.EnqueuePass((RustRenderPass)(object)_shadowPass);
		renderer.EnqueuePass((RustRenderPass)(object)_drawPass);
		renderer.EnqueuePass((RustRenderPass)(object)_upscalePass);
		renderer.EnqueuePass((RustRenderPass)(object)_scatteringPass);
	}

	public override void Create()
	{
		if (_shadowPass == null)
		{
			VolumeCloudsShadowPass volumeCloudsShadowPass = new VolumeCloudsShadowPass();
			((RustRenderPass)volumeCloudsShadowPass).renderPassEvent = (RenderPassEvent)5;
			_shadowPass = volumeCloudsShadowPass;
		}
		if (_drawPass == null)
		{
			VolumeCloudsDrawPass volumeCloudsDrawPass = new VolumeCloudsDrawPass();
			((RustRenderPass)volumeCloudsDrawPass).renderPassEvent = (RenderPassEvent)8;
			((RustRenderPass)volumeCloudsDrawPass).sort = 2;
			_drawPass = volumeCloudsDrawPass;
		}
		if (_upscalePass == null)
		{
			VolumeCloudsUpscalePass volumeCloudsUpscalePass = new VolumeCloudsUpscalePass();
			((RustRenderPass)volumeCloudsUpscalePass).renderPassEvent = (RenderPassEvent)8;
			((RustRenderPass)volumeCloudsUpscalePass).sort = 3;
			_upscalePass = volumeCloudsUpscalePass;
		}
		if (_scatteringPass == null)
		{
			VolumeCloudsAtmosphericScatteringPass volumeCloudsAtmosphericScatteringPass = new VolumeCloudsAtmosphericScatteringPass();
			((RustRenderPass)volumeCloudsAtmosphericScatteringPass).renderPassEvent = (RenderPassEvent)8;
			((RustRenderPass)volumeCloudsAtmosphericScatteringPass).sort = 4;
			_scatteringPass = volumeCloudsAtmosphericScatteringPass;
		}
	}

	protected override void Dispose(bool disposing)
	{
		((RustRendererFeature)this).Dispose(disposing);
		if (disposing)
		{
			_shadowPass = null;
			_drawPass = null;
			_upscalePass = null;
			_scatteringPass = null;
		}
	}
}
