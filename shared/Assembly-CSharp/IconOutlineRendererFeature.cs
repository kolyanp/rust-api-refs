using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/IconOutlineRendererFeature")]
public class IconOutlineRendererFeature : RustRendererFeature
{
	public Shader iconOutlineShader;

	public override RustRendererFeatureCameraBase CreateCameraComponent()
	{
		return (RustRendererFeatureCameraBase)(object)new IconOutlineCamera();
	}

	public override RustRendererFeatureCameraContext CreateCameraContext()
	{
		return (RustRendererFeatureCameraContext)(object)new IconOutlineCameraContext();
	}

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}
