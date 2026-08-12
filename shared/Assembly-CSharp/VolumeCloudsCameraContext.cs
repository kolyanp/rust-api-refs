using Rust.RenderPipeline.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

public class VolumeCloudsCameraContext : RustRendererFeatureCameraContext
{
	public int currUpdate;

	public bool firstFrame;

	public Matrix4x4 lastViewProj;

	public Matrix4x4 currViewProj;

	public Matrix4x4 projMat;

	public Matrix4x4 inverseViewMat;

	public Matrix4x4 projToWorld;

	public RenderTexture upscaledOutputRT;

	public RTHandle upscaledOutputRTHandle;

	public int outputSizeX;

	public int outputSizeY;

	public int lowResSizeX;

	public int lowResSizeY;

	public int sizeDivisor;

	public VolumeCloudsCameraContext()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		firstFrame = true;
		lastViewProj = Matrix4x4.identity;
		currViewProj = Matrix4x4.identity;
		projMat = Matrix4x4.identity;
		inverseViewMat = Matrix4x4.identity;
		projToWorld = Matrix4x4.identity;
		((RustRendererFeatureCameraContext)this)._002Ector();
	}
}
