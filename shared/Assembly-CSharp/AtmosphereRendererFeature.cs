using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/AtmosphereRendererFeature")]
public class AtmosphereRendererFeature : RustRendererFeature
{
	public Texture DitheringTexture;

	public Shader scatteringShader;

	public override RustRendererFeatureCameraBase CreateCameraComponent()
	{
		return (RustRendererFeatureCameraBase)(object)new AtmosphereRendererCamera();
	}

	public override RustRendererFeatureCameraContext CreateCameraContext()
	{
		return (RustRendererFeatureCameraContext)(object)new AtmosphereRendererCameraContext();
	}

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}
