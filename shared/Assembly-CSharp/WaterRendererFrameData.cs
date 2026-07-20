using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class WaterRendererFrameData : ContextItem
{
	public TextureHandle surfaceTex;

	public TextureHandle surfaceMotionTex;

	public TextureHandle surfaceMaskTex;

	public TextureHandle preFogBackgroundTex;

	public TextureHandle causticsTex;

	public TextureHandle ssrReflectionTex;

	public TextureHandle backgroundColorTex;

	public TextureHandle combinedDynamicsTex;

	public BufferHandle oceanVFaceBuf;

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
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		surfaceTex = TextureHandle.nullHandle;
		surfaceMotionTex = TextureHandle.nullHandle;
		surfaceMaskTex = TextureHandle.nullHandle;
		preFogBackgroundTex = TextureHandle.nullHandle;
		backgroundColorTex = TextureHandle.nullHandle;
		causticsTex = TextureHandle.nullHandle;
		ssrReflectionTex = TextureHandle.nullHandle;
		oceanVFaceBuf = BufferHandle.nullHandle;
		combinedDynamicsTex = TextureHandle.nullHandle;
	}
}
