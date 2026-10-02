using UnityEngine;

namespace Rust.Ai.Gen2;

public readonly struct LivestockFootprint
{
	public readonly Vector3 Centre;

	public readonly Vector3 Right;

	public readonly Vector3 Forward;

	public readonly Vector2 Extents;

	public LivestockFootprint(Vector3 centre, Vector3 forward, Vector2 extents)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3Ex.WithY(forward, 0f);
		Vector3 forward2;
		if (!(val.sqrMagnitude > 0.0001f))
		{
			forward2 = Vector3.forward;
		}
		else
		{
			val = Vector3Ex.WithY(forward, 0f);
			forward2 = val.normalized;
		}
		Forward = forward2;
		Right = new Vector3(Forward.z, 0f, 0f - Forward.x);
		Centre = Vector3Ex.WithY(centre, 0f);
		Extents = extents;
	}

	public static LivestockFootprint Of(BaseEntity entity, Vector3 position, Vector3 forward)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = entity.bounds;
		LivestockFootprint livestockFootprint = new LivestockFootprint(position, forward, Vector2.zero);
		return new LivestockFootprint(position + livestockFootprint.Right * bounds.center.x + livestockFootprint.Forward * bounds.center.z, forward, new Vector2(bounds.extents.x, bounds.extents.z));
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
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 offset = b.Centre - a.Centre;
		return Mathf.Min(Mathf.Min(Mathf.Min(Overlap(a.Right, offset, in a, in b), Overlap(a.Forward, offset, in a, in b)), Overlap(b.Right, offset, in a, in b)), Overlap(b.Forward, offset, in a, in b));
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
}
