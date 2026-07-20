using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class VolumetricFogFrameData : ContextItem
{
	public TextureHandle shadowMap;

	public TextureHandle fogVolume;

	public TextureHandle skyFogTexture;

	public override void Reset()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		shadowMap = TextureHandle.nullHandle;
		fogVolume = TextureHandle.nullHandle;
		skyFogTexture = TextureHandle.nullHandle;
	}
}
