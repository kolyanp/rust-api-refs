using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/GeometryClipmapTerrainDepthPrepass")]
public class GeometryClipmapTerrainDepthPrepassRendererFeature : RustRendererFeature
{
	public RenderPassEvent passEvent;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}

	public GeometryClipmapTerrainDepthPrepassRendererFeature()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		passEvent = (RenderPassEvent)5;
		((RustRendererFeature)this)._002Ector();
	}
}
