using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/ViewmodelRendererFeature")]
[RustRendererFeatureCameraComponent(typeof(ViewmodelRendererCamera), typeof(ViewmodelRendererCameraContext))]
public class ViewmodelRendererFeature : RustRendererFeature
{
	public Shader viewModelShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}
