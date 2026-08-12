using UnityEngine;

namespace Rust.Ai.Gen2;

public readonly struct NavVector3
{
	public readonly Transform Parent;

	public readonly Vector3 LocalPosition;

	public NavVector3(Transform parent, Vector3 localPosition)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Parent = parent;
		LocalPosition = localPosition;
	}

	public static NavVector3 FromLocal(Transform parent, Vector3 localPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return new NavVector3(parent, localPosition);
	}

	public static NavVector3 FromWorld(Vector3 worldPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return new NavVector3(null, worldPosition);
	}
}
