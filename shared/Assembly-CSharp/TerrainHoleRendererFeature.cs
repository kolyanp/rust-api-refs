using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/TerrainHoleRendererFeature")]
public class TerrainHoleRendererFeature : RustRendererFeature
{
	[SerializeField]
	private RenderPassEvent cameraEvent;

	[SerializeField]
	private Material stencilMaterial;

	[SerializeField]
	private Material holeMapMaterial;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}

	public TerrainHoleRendererFeature()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		cameraEvent = (RenderPassEvent)5;
		((RustRendererFeature)this)._002Ector();
	}
}
