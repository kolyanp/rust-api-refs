using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/WaterRendererFeature")]
[RustRendererFeatureCameraComponent(typeof(WaterRendererCamera), typeof(WaterRendererCameraContext))]
public class WaterRendererFeature : RustRendererFeature
{
	[SerializeField]
	private ComputeShader oceanVFaceComputeShader;

	[SerializeField]
	private Shader interiorShader;

	[SerializeField]
	private Shader oceanUnderWaterFillShader;

	[SerializeField]
	private Shader multiCopyShader;

	[SerializeField]
	private Shader reflectionShader;

	[SerializeField]
	private Shader interactionShader;

	[SerializeField]
	private Shader combineInteractionsShader;

	[SerializeField]
	private Shader underwaterShader;

	[SerializeField]
	private Shader underwaterEffectShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}
