using System;
using System.Collections.Generic;
using Rust.Ai.Gen2.Nav;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

public class RustNavMeshPath
{
	public readonly List<NavVector3> corners;

	public NavMeshPathStatus status;

	public NavMeshPath unityPath;

	public readonly ulong[] polyRefs;

	public int polyRefCount;

	public float GetPathLength()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshPath.GetPathLength"))
		{
			float num = 0f;
			if (corners.Count < 2)
			{
				return num;
			}
			for (int i = 0; i < corners.Count - 1; i++)
			{
				num += Vector3.Distance(corners[i].Value, corners[i + 1].Value);
			}
			return num;
		}
	}

	public NavVector3 GetDestinationNS()
	{
		if (corners.Count < 1)
		{
			return NavVector3.zero;
		}
		List<NavVector3> list = corners;
		return list[list.Count - 1];
	}

	public void Reset()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		corners.Clear();
		status = (NavMeshPathStatus)2;
		unityPath = null;
		polyRefCount = 0;
	}

	public void CopyFrom(RustNavMeshPath other)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		corners.Clear();
		corners.AddRange(other.corners);
		status = other.status;
		unityPath = other.unityPath;
		Array.Copy(other.polyRefs, polyRefs, other.polyRefCount);
		polyRefCount = other.polyRefCount;
	}

	public RustNavMeshPath()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		corners = new List<NavVector3>();
		status = (NavMeshPathStatus)2;
		polyRefs = new ulong[256];
		base._002Ector();
	}
}
