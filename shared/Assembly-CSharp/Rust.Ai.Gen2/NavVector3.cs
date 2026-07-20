using UnityEngine;

namespace Rust.Ai.Gen2;

public readonly struct NavVector3(Transform parent, Vector3 localPosition)
{
	public readonly Transform Parent = parent;

	public readonly Vector3 LocalPosition = localPosition;

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
