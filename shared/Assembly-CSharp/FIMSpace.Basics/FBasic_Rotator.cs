using UnityEngine;

namespace FIMSpace.Basics;

public class FBasic_Rotator : MonoBehaviour
{
	public Vector3 RotationAxis;

	public float RotationSpeed;

	public bool UnscaledDeltaTime;

	protected virtual void Update()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		float num = ((!UnscaledDeltaTime) ? Time.deltaTime : Time.unscaledDeltaTime);
		Transform transform = ((Component)this).transform;
		transform.localRotation *= Quaternion.AngleAxis(num * RotationSpeed, RotationAxis);
	}

	public FBasic_Rotator()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		RotationAxis = new Vector3(0f, 1f, 0f);
		RotationSpeed = 100f;
		((MonoBehaviour)this)._002Ector();
	}
}
