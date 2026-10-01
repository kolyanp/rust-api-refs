namespace UnityEngine;

public static class RayEx
{
	public static Vector3 ClosestPoint(this Ray ray, Vector3 pos)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return ray.origin + Vector3.Dot(pos - ray.origin, ray.direction) * ray.direction;
	}

	public static float Distance(this Ray ray, Vector3 pos)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Cross(ray.direction, pos - ray.origin);
		return val.magnitude;
	}

	public static float SqrDistance(this Ray ray, Vector3 pos)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Cross(ray.direction, pos - ray.origin);
		return val.sqrMagnitude;
	}

	public static bool IsNaNOrInfinity(this Ray r)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!Vector3Ex.IsNaNOrInfinity(r.origin))
		{
			return Vector3Ex.IsNaNOrInfinity(r.direction);
		}
		return true;
	}
}
