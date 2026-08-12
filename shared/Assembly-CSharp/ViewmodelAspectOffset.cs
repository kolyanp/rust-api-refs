using UnityEngine;

public class ViewmodelAspectOffset : MonoBehaviour
{
	public Vector3 OffsetAmount;

	[Tooltip("What aspect ratio should we start moving the viewmodel? 16:9 = 1.7, 21:9 = 2.3")]
	public float aspectCutoff;

	public ViewmodelAspectOffset()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		OffsetAmount = Vector3.zero;
		aspectCutoff = 2f;
		((MonoBehaviour)this)._002Ector();
	}
}
