using Rust.RenderPipeline.Runtime;
using UnityEngine;

[RustRendererFeatureCameraComponent(typeof(AtmosphereVolumeCamera), typeof(AtmosphereVolumeCameraContext))]
[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/AtmosphereVolumeRendererFeature")]
public class AtmosphereVolumeRendererFeature : RustRendererFeature
{
	public FogMode Mode = (FogMode)3;

	public bool DistanceFog = true;

	public bool HeightFog = true;

	public Shader fogVolumeShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}
