using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class VolumeCloudsFrameData : ContextItem
{
	public TextureHandle shadowMap;

	public TextureHandle lowResImage;

	public TextureHandle depthBuffer;

	public TextureHandle upscaledImage;

	public TextureHandle atmosphericScattering;

	public override void Reset()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		shadowMap = TextureHandle.nullHandle;
		lowResImage = TextureHandle.nullHandle;
		depthBuffer = TextureHandle.nullHandle;
		upscaledImage = TextureHandle.nullHandle;
		atmosphericScattering = TextureHandle.nullHandle;
	}
}
