using UnityEngine;

[ExecuteInEditMode]
public class BindJellyfishLag : MonoBehaviour
{
	[Tooltip("How quickly the tendrils catch up to the body. Lower trails further behind.")]
	public float followSpeed = 4f;

	[Tooltip("Longest distance, in world units, the tendrils are allowed to trail behind the body.")]
	public float maxLagDistance = 0.5f;
}
