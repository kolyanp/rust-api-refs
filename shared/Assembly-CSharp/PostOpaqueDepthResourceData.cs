using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class PostOpaqueDepthResourceData : ContextItem
{
	public TextureHandle postOpaqueDepthHandle;

	public override void Reset()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		postOpaqueDepthHandle = TextureHandle.nullHandle;
	}
}
