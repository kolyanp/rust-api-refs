using UnityEngine;

public class IndependentScale : MonoBehaviour, IClientComponent
{
	public Transform scaleParent;

	public Vector3 initialScale;

	public IndependentScale()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		initialScale = Vector3.one;
		((MonoBehaviour)this)._002Ector();
	}
}
