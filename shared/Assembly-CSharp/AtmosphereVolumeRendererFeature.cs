using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/AtmosphereVolumeRendererFeature")]
public class AtmosphereVolumeRendererFeature : RustRendererFeature
{
	public FogMode Mode = (FogMode)3;

	public bool DistanceFog = true;

	public bool HeightFog = true;

	public Shader fogVolumeShader;

	public override RustRendererFeatureCameraBase CreateCameraComponent()
	{
		return (RustRendererFeatureCameraBase)(object)new AtmosphereVolumeCamera();
	}

	public override RustRendererFeatureCameraContext CreateCameraContext()
	{
		return (RustRendererFeatureCameraContext)(object)new AtmosphereVolumeCameraContext();
	}

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}

	public AtmosphereVolumeRendererFeature()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
	}
}
