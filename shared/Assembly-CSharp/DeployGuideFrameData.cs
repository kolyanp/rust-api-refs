using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class DeployGuideFrameData : ContextItem
{
	public TextureHandle depthHandle;

	public TextureHandle depthBackHandle;

	public TextureHandle normalsHandle;

	public TextureHandle normalsBackHandle;

	public DeployGuideRendererFeature rendererFeature;

	public override void Reset()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		depthHandle = (depthBackHandle = TextureHandle.nullHandle);
		normalsHandle = (normalsBackHandle = TextureHandle.nullHandle);
		rendererFeature = null;
	}
}
