using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class TexelDensityFrameData : ContextItem
{
	public TextureHandle densityTex;

	public TextureHandle densityDepthTex;

	public float opacity;

	public override void Reset()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		densityTex = TextureHandle.nullHandle;
		densityDepthTex = TextureHandle.nullHandle;
		opacity = 0f;
	}
}
