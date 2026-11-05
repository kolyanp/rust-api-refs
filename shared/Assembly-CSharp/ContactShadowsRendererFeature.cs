using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/ContactShadowsRendererFeature")]
public class ContactShadowsRendererFeature : RustRendererFeature
{
	public ContactShadowsPreset preset;

	public Shader screenSpaceShadowsShader;

	public override RustRendererFeatureCameraBase CreateCameraComponent()
	{
		return (RustRendererFeatureCameraBase)(object)new ContactShadowsCamera();
	}

	public override RustRendererFeatureCameraContext CreateCameraContext()
	{
		return (RustRendererFeatureCameraContext)(object)new ContactShadowsCameraContext();
	}

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}
