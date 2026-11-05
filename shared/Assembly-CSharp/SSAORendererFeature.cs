using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/AmbientOcclusion")]
public class SSAORendererFeature : RustRendererFeature
{
	public SSAOPreset preset;

	[Header("Shaders")]
	public Shader occlusionShader;

	public Shader blurShader;

	public Shader occlusionApplyShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}
