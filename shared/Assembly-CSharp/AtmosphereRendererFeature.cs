using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/AtmosphereRendererFeature")]
[RustRendererFeatureCameraComponent(typeof(AtmosphereRendererCamera), typeof(AtmosphereRendererCameraContext))]
public class AtmosphereRendererFeature : RustRendererFeature
{
	public Texture DitheringTexture;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}
