using UnityEngine;

namespace Rust.Ai.Gen2;

public readonly struct LivestockFootprint
{
	private const float SeparateFloors = 2f;

	public readonly Vector3 Centre;

	public readonly Vector3 Right;

	public readonly Vector3 Forward;

	public readonly Vector2 Extents;

	public LivestockFootprint(Vector3 centre, Vector3 forward, Vector2 extents)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Forward = FlatForward(forward);
		Right = RightOf(Forward);
		Centre = centre;
		Extents = extents;
	}

	public static LivestockFootprint Of(BaseEntity entity, Vector3 position, Vector3 forward)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Of(entity.bounds, position, forward);
	}

	public static LivestockFootprint Of(Bounds bounds, Vector3 position, Vector3 forward)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = FlatForward(forward);
		return new LivestockFootprint(position + RightOf(val) * bounds.center.x + val * bounds.center.z, forward, new Vector2(bounds.extents.x, bounds.extents.z));
	}

	public static LivestockFootprint Of(BaseEntity entity)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return Of(entity, ((Component)entity).transform.position, ((Component)entity).transform.forward);
	}

	public static float Penetration(in LivestockFootprint a, in LivestockFootprint b)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = b.Centre - a.Centre;
		if (Mathf.Abs(val.y) > 2f)
		{
			return float.NegativeInfinity;
		}
		return Mathf.Min(Mathf.Min(Mathf.Min(Overlap(a.Right, val, in a, in b), Overlap(a.Forward, val, in a, in b)), Overlap(b.Right, val, in a, in b)), Overlap(b.Forward, val, in a, in b));
	}

	private static float Overlap(Vector3 axis, Vector3 offset, in LivestockFootprint a, in LivestockFootprint b)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return a.Reach(axis) + b.Reach(axis) - Mathf.Abs(Vector3.Dot(offset, axis));
	}

	private float Reach(Vector3 axis)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return Extents.x * Mathf.Abs(Vector3.Dot(Right, axis)) + Extents.y * Mathf.Abs(Vector3.Dot(Forward, axis));
	}

	private static Vector3 FlatForward(Vector3 forward)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3Ex.WithY(forward, 0f);
		if (!(val.sqrMagnitude > 0.0001f))
		{
			return Vector3.forward;
		}
		return val.normalized;
	}

	private static Vector3 RightOf(Vector3 forward)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(forward.z, 0f, 0f - forward.x);
	}
}
