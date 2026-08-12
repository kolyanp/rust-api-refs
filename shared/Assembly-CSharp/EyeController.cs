using UnityEngine;

public class EyeController : MonoBehaviour
{
	public const float MaxLookDot = 0.8f;

	public bool debug;

	public Transform LeftEye;

	public Transform RightEye;

	public Transform LeftEyelid;

	public Transform RightEyelid;

	public Transform EyeTransform;

	public Vector3 Fudge;

	public Vector3 RightEyeFudge;

	public Vector3 FlickerRange;

	private Transform Focus;

	private float FocusUpdateTime;

	public EyeController()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Fudge = new Vector3(0f, 90f, 0f);
		RightEyeFudge = new Vector3(180f, 180f, 0f);
		((MonoBehaviour)this)._002Ector();
	}
}
