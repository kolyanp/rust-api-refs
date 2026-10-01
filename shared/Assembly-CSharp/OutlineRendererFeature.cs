using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/OutlineRendererFeature")]
public class OutlineRendererFeature : RustRendererFeature
{
	public Material glowSolidMaterial;

	public Material blendGlowMaterial;

	public Shader blurShader;

	public override RustRendererFeatureCameraBase CreateCameraComponent()
	{
		return (RustRendererFeatureCameraBase)(object)new OutlineCamera();
	}

	public override RustRendererFeatureCameraContext CreateCameraContext()
	{
		return (RustRendererFeatureCameraContext)(object)new OutlineCameraContext();
	}

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}
