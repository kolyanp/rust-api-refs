using UnityEngine;

namespace Carbon.Extensions;

public static class VectorEx
{
	public static string ToParsableString(this Vector3 vector)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return $"{vector.x} {vector.y} {vector.z}";
	}

	public static string ToParsableString(this Vector2 vector)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return $"{vector.x} {vector.y}";
	}
}
