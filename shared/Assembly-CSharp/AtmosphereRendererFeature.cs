using Rust.RenderPipeline.Runtime;
using UnityEngine;

[RustRendererFeatureCameraComponent(typeof(AtmosphereRendererCamera), typeof(AtmosphereRendererCameraContext))]
[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/AtmosphereRendererFeature")]
public class AtmosphereRendererFeature : RustRendererFeature
{
	public Texture DitheringTexture;

	public Shader scatteringShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}
