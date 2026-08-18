using Rust.RenderPipeline.Runtime;
using UnityEngine;

[RustRendererFeatureCameraComponent(typeof(ViewmodelRendererCamera), typeof(ViewmodelRendererCameraContext))]
[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/ViewmodelRendererFeature")]
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
