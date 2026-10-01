using UnityEngine;

public abstract class BaseFootstepEffect : MonoBehaviour, IClientComponent
{
	public LayerMask validImpactLayers = LayerMask.op_Implicit(-1);

	protected BaseFootstepEffect()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
	}
}
