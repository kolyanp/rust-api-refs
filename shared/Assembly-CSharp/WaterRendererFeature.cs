using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/WaterRendererFeature")]
[RustRendererFeatureCameraComponent(typeof(WaterRendererCamera), typeof(WaterRendererCameraContext))]
public class WaterRendererFeature : RustRendererFeature
{
	[SerializeField]
	private ComputeShader oceanVFaceComputeShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}
