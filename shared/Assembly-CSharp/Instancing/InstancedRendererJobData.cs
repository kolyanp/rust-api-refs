using UnityEngine.Rendering;

namespace Instancing;

public struct InstancedRendererJobData
{
	public int Id;

	public int DrawCallCount;

	public float MinDistance;

	public float MaxDistance;

	public ShadowCastingMode ShadowMode;

	public bool HasMesh
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			return (int)ShadowMode != 3;
		}
	}

	public bool HasShadow
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			return (int)ShadowMode > 0;
		}
	}
}
