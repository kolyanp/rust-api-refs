using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/FoliageDisplacement")]
public class FoliageDisplacementRendererFeature : RustRendererFeature
{
	[SerializeField]
	private Material clearDisplacementMaterial;

	[SerializeField]
	private RenderPassEvent cameraEvent;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}

	public FoliageDisplacementRendererFeature()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		cameraEvent = (RenderPassEvent)5;
		((RustRendererFeature)this)._002Ector();
	}
}
