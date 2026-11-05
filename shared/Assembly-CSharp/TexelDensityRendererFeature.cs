using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/TexelDensityRendererFeature")]
public class TexelDensityRendererFeature : RustRendererFeature
{
	[SerializeField]
	private Shader densityShader;

	[SerializeField]
	private Shader overlayShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}
