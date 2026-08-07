using Rust.RenderPipeline.Runtime;
using UnityEngine;

public class AtmosphereVolumeCamera : RustRendererFeatureCamera<AtmosphereVolumeCameraContext>
{
	public override bool OnBeginRendering()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Shader.SetGlobalVector("_SceneFogMode", Vector4.zero);
		return true;
	}

	public override void OnEndRendering()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Shader.SetGlobalVector("_SceneFogMode", Vector4.zero);
	}
}
