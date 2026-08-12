using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

public class RustNavMeshPath
{
	public readonly List<Vector3> corners;

	public NavMeshPathStatus status;

	public NavMeshPath unityPath;

	public float GetPathLength()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshPath.GetPathLength"))
		{
			float num = 0f;
			if (corners.Count < 2)
			{
				return num;
			}
			for (int i = 0; i < corners.Count - 1; i++)
			{
				num += Vector3.Distance(corners[i], corners[i + 1]);
			}
			return num;
		}
	}

	public Vector3 GetOrigin()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (corners.Count < 1)
		{
			return Vector3.zero;
		}
		return corners[0];
	}

	public Vector3 GetDestinationNS()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (corners.Count < 1)
		{
			return Vector3.zero;
		}
		List<Vector3> list = corners;
		return list[list.Count - 1];
	}

	public void Reset()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		corners.Clear();
		status = (NavMeshPathStatus)2;
		unityPath = null;
	}

	public void CopyFrom(RustNavMeshPath other)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		corners.Clear();
		corners.AddRange(other.corners);
		status = other.status;
		unityPath = other.unityPath;
	}

	public RustNavMeshPath()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		corners = new List<Vector3>();
		status = (NavMeshPathStatus)2;
		base._002Ector();
	}
}
