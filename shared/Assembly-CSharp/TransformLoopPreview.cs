using UnityEngine;

[ExecuteAlways]
public class TransformLoopPreview : MonoBehaviour
{
	public enum MovementAxis
	{
		PositiveX,
		NegativeX,
		PositiveY,
		NegativeY,
		PositiveZ,
		NegativeZ
	}

	[SerializeField]
	[Header("Preview")]
	private bool previewMotion = true;

	[SerializeField]
	[Min(0.001f)]
	private float movementDistance = 10f;

	[Min(0f)]
	[SerializeField]
	private float movementSpeed = 5f;

	[SerializeField]
	[Tooltip("Uses the object's rotated local axes instead of world axes.")]
	private bool useLocalAxis = true;

	[SerializeField]
	private MovementAxis movementAxis = MovementAxis.PositiveZ;

	[HideInInspector]
	[SerializeField]
	private Vector3 originPosition;

	[SerializeField]
	[HideInInspector]
	private bool originCaptured;

	[SerializeField]
	[HideInInspector]
	private float travelledDistance;

	private void OnEnable()
	{
		if (!originCaptured)
		{
			CaptureOrigin();
		}
	}

	private void OnDisable()
	{
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			Move(Time.deltaTime);
		}
	}

	private void Move(float deltaTime)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (previewMotion && originCaptured && !(movementSpeed <= 0f) && !(movementDistance <= 0f))
		{
			travelledDistance += movementSpeed * deltaTime;
			if (travelledDistance >= movementDistance)
			{
				travelledDistance = 0f;
			}
			Vector3 movementDirection = GetMovementDirection();
			((Component)this).transform.position = originPosition + movementDirection * travelledDistance;
		}
	}

	private Vector3 GetMovementDirection()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = (Vector3)(movementAxis switch
		{
			MovementAxis.PositiveX => Vector3.right, 
			MovementAxis.NegativeX => Vector3.left, 
			MovementAxis.PositiveY => Vector3.up, 
			MovementAxis.NegativeY => Vector3.down, 
			MovementAxis.NegativeZ => Vector3.back, 
			_ => Vector3.forward, 
		});
		if (useLocalAxis)
		{
			val = ((Component)this).transform.TransformDirection(val);
		}
		return ((Vector3)(ref val)).normalized;
	}

	public void CaptureOrigin()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		originPosition = ((Component)this).transform.position;
		originCaptured = true;
		travelledDistance = 0f;
	}

	public void RestartFromOrigin()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!originCaptured)
		{
			CaptureOrigin();
		}
		travelledDistance = 0f;
		((Component)this).transform.position = originPosition;
	}

	public void StopAndReset()
	{
		previewMotion = false;
		RestartFromOrigin();
	}
}
