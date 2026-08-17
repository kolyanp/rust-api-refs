using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/AtmosphereVolumeRendererFeature")]
[RustRendererFeatureCameraComponent(typeof(AtmosphereVolumeCamera), typeof(AtmosphereVolumeCameraContext))]
public class AtmosphereVolumeRendererFeature : RustRendererFeature
{
	public FogMode Mode;

	public bool DistanceFog;

	public bool HeightFog;

	public Shader fogVolumeShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}

	public AtmosphereVolumeRendererFeature()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Mode = (FogMode)3;
		DistanceFog = true;
		HeightFog = true;
		((RustRendererFeature)this)._002Ector();
	}
}
