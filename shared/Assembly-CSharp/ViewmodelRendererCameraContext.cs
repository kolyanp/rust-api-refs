using Rust.RenderPipeline.Runtime;
using UnityEngine;

public class ViewmodelRendererCameraContext : RustRendererFeatureCameraContext
{
	public Matrix4x4 prevViewProj = Matrix4x4.identity;

	public ViewmodelRendererCameraContext()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
