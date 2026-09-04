using UnityEngine;

[ExecuteAlways]
public class FakePhysicsRope : FacepunchBehaviour, IClientComponent
{
	public enum RenderMode
	{
		LineRenderer2D,
		TubeRenderer3D,
		Both
	}

	[Header("References")]
	public Transform startPoint;

	public Transform endPoint;

	public Transform leadPoint;

	public Vector3 endPointOffset;

	[Range(2f, 100f)]
	[Header("Settings")]
	public int linePoints = 10;

	[Tooltip("Value highly dependent on use case, a metal cable would have high stiffness, a rubber rope would have a low one")]
	public float stiffness = 350f;

	[Tooltip("0 is no damping, 50 is a lot")]
	public float damping = 15f;

	[Tooltip("How long is the rope. It will hang more or less from starting point to end point depending on this value")]
	public float ropeLength = 15f;

	[Tooltip("The Rope width set at start (changing this value during run time will produce no effect)")]
	public float ropeWidth = 0.1f;

	[Range(1f, 15f)]
	[Tooltip("Adjust the middle control point weight for the Rational Bezier curve")]
	public float midPointWeight = 1f;

	[Tooltip("Use local positions instead of world positions (relative to this object)")]
	public bool useLocalPositions;

	[Header("Rendering")]
	public RenderMode renderMode;

	[Header("Wind")]
	public bool AddFakeWind;

	public float windFrequency;

	public float windAmplitude;

	protected Vector3 EndPointPosition
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			return endPoint.position + endPointOffset;
		}
	}

	public static Vector3 GetRationalBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t, float w0 = 1f, float w1 = 1f, float w2 = 1f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = w0 * p0;
		Vector3 val2 = w1 * p1;
		Vector3 val3 = w2 * p2;
		float num = w0 * Mathf.Pow(1f - t, 2f) + 2f * w1 * (1f - t) * t + w2 * Mathf.Pow(t, 2f);
		return (val * Mathf.Pow(1f - t, 2f) + val2 * 2f * (1f - t) * t + val3 * Mathf.Pow(t, 2f)) / num;
	}
}
