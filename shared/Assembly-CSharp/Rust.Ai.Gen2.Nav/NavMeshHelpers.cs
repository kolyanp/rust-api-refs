using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai.Gen2.Nav;

public static class NavMeshHelpers
{
	public static bool FindNextWayPointIndex(List<Vector3> path, Vector3 currentPosition, int curWaypointIndex, out int newWaypointIndex, out bool reachedEnd, float stoppingDistance = 0.1f)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavigation.FindNextWayPointIndex"))
		{
			newWaypointIndex = curWaypointIndex;
			reachedEnd = false;
			if (path == null || path.Count == 0)
			{
				return false;
			}
			if (path.Count == 1)
			{
				newWaypointIndex = 0;
				reachedEnd = true;
				return true;
			}
			curWaypointIndex = Mathf.Clamp(curWaypointIndex, 1, path.Count - 1);
			Vector3 val = path[curWaypointIndex - 1];
			Vector3 val2 = path[curWaypointIndex];
			if (Vector3.Dot(Vector3Ex.NormalizeXZ(currentPosition - val2), Vector3Ex.NormalizeXZ(val2 - val)) >= 0f)
			{
				if (curWaypointIndex >= path.Count - 1)
				{
					reachedEnd = true;
				}
				else
				{
					newWaypointIndex = curWaypointIndex + 1;
				}
				return true;
			}
			if (curWaypointIndex == path.Count - 1 && Vector3.Distance(currentPosition, val2) <= stoppingDistance)
			{
				reachedEnd = true;
				return true;
			}
			return true;
		}
	}

	public static bool CalculateRemainingPathLength(List<Vector3> path, Vector3 currentPosition, int curWaypointIndex, out float length)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavigation.CalculateRemainingPathLength"))
		{
			length = 0f;
			if (path == null || path.Count == 0)
			{
				return false;
			}
			if (path.Count == 1)
			{
				length = Vector3.Distance(currentPosition, path[0]);
				return true;
			}
			curWaypointIndex = Mathf.Clamp(curWaypointIndex, 1, path.Count - 1);
			Vector3 val = path[curWaypointIndex];
			length += Vector3.Distance(currentPosition, val);
			for (int i = curWaypointIndex; i < path.Count - 1; i++)
			{
				length += Vector3.Distance(path[i], path[i + 1]);
			}
			return true;
		}
	}
}
