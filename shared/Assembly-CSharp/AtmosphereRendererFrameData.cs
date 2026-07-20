using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class AtmosphereRendererFrameData : ContextItem
{
	public BufferHandle ambientBuf;

	public override void Reset()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		ambientBuf = BufferHandle.nullHandle;
	}
}
