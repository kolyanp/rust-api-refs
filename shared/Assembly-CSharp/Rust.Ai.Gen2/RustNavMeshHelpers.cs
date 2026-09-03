using ConVar;
using Rust.Ai.Gen2.Nav;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

public static class RustNavMeshHelpers
{
	private static readonly Vector3[] cornerBuffer = (Vector3[])(object)new Vector3[256];

	public const int AllAreas = -1;

	public static int GetAreaFromName(string areaName)
	{
		return NavMesh.GetAreaFromName(areaName);
	}

	public static bool SamplePosition(Vector3 sourcePositionWS, out NavMeshHit hitWS, float maxDistance, int areaMask)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		hitWS = default(NavMeshHit);
		if (AI.useUnityNavmesh)
		{
			return NavMesh.SamplePosition(sourcePositionWS, ref hitWS, maxDistance, areaMask);
		}
		IndependantNavmesh independantNavmesh = IndependantNavmesh.FindNavmeshAtPosition(sourcePositionWS);
		NavHit hit;
		if ((Object)(object)independantNavmesh != (Object)null && independantNavmesh.Navmesh != null && independantNavmesh.Navmesh.IsBuilt())
		{
			NavVector3 position = independantNavmesh.TransformPointFromWorldSpaceToNavSpace(sourcePositionWS);
			if (!independantNavmesh.Navmesh.SamplePosition(position, out hit, Vector3.one * maxDistance))
			{
				return false;
			}
			hitWS = hit.ToUnity();
			((NavMeshHit)(ref hitWS)).position = independantNavmesh.TransformPointFromNavSpaceToWorldSpace(hit.position);
			((NavMeshHit)(ref hitWS)).normal = independantNavmesh.TransformDirectionFromNavSpaceToWorldSpace(hit.normal);
			return true;
		}
		if (!RustNavigation.Instance.IsDefaultNavmeshBuilt())
		{
			if (AI.logIssues)
			{
				RustNavigation.LogError("Trying to sample position on the navmesh before it's built. This will always fail. Make sure to check IsDefaultNavmeshBuilt() before sampling positions.");
			}
			hitWS = default(NavMeshHit);
			return false;
		}
		if (!RustNavigation.Instance.DefaultNavmesh.SamplePosition(new NavVector3(sourcePositionWS), out hit, Vector3.one * maxDistance))
		{
			return false;
		}
		hitWS = hit.ToUnity();
		return true;
	}

	public static bool Raycast(Vector3 sourcePositionWS, Vector3 targetPositionWS, out NavMeshHit hitWS, int areaMask)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		hitWS = default(NavMeshHit);
		if (AI.useUnityNavmesh)
		{
			return NavMesh.Raycast(sourcePositionWS, targetPositionWS, ref hitWS, areaMask);
		}
		IndependantNavmesh independantNavmesh = IndependantNavmesh.FindNavmeshAtPosition(sourcePositionWS);
		if ((Object)(object)independantNavmesh != (Object)null && independantNavmesh.Navmesh != null && independantNavmesh.Navmesh.IsBuilt())
		{
			NavVector3 startPos = independantNavmesh.TransformPointFromWorldSpaceToNavSpace(sourcePositionWS);
			NavVector3 endPos = independantNavmesh.TransformPointFromWorldSpaceToNavSpace(targetPositionWS);
			if (!independantNavmesh.Navmesh.Raycast(startPos, endPos, out var hit))
			{
				return false;
			}
			hitWS = hit.ToUnity();
			((NavMeshHit)(ref hitWS)).position = independantNavmesh.TransformPointFromNavSpaceToWorldSpace(hit.position);
			((NavMeshHit)(ref hitWS)).normal = independantNavmesh.TransformDirectionFromNavSpaceToWorldSpace(hit.normal);
			return true;
		}
		if (!RustNavigation.Instance.IsDefaultNavmeshBuilt())
		{
			if (AI.logIssues)
			{
				RustNavigation.LogError("Trying to raycast on the navmesh before it's built. This will always fail. Make sure to check IsDefaultNavmeshBuilt() before raycasting.");
			}
			hitWS = default(NavMeshHit);
			return false;
		}
		if (!RustNavigation.Instance.DefaultNavmesh.Raycast(new NavVector3(sourcePositionWS), new NavVector3(targetPositionWS), out var hit2))
		{
			return false;
		}
		hitWS = hit2.ToUnity();
		return true;
	}

	public static bool CalculatePath(Vector3 sourcePositionWS, Vector3 targetPositionWS, NavMeshQueryFilter filter, RustNavMeshPath pathNS)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return CalculatePath(sourcePositionWS, targetPositionWS, ((NavMeshQueryFilter)(ref filter)).areaMask, pathNS);
	}

	public static bool CalculatePath(Vector3 sourcePositionWS, Vector3 targetPositionWS, int areaMask, RustNavMeshPath pathNS)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		pathNS.corners.Clear();
		if (AI.useUnityNavmesh)
		{
			if (pathNS.unityPath == null)
			{
				pathNS.unityPath = new NavMeshPath();
			}
			bool result = NavMesh.CalculatePath(sourcePositionWS, targetPositionWS, areaMask, pathNS.unityPath);
			pathNS.status = pathNS.unityPath.status;
			int cornersNonAlloc = pathNS.unityPath.GetCornersNonAlloc(cornerBuffer);
			for (int i = 0; i < cornersNonAlloc; i++)
			{
				pathNS.corners.Add(new NavVector3(cornerBuffer[i]));
			}
			return result;
		}
		IndependantNavmesh independantNavmesh = IndependantNavmesh.FindNavmeshAtPosition(sourcePositionWS);
		if ((Object)(object)independantNavmesh != (Object)null && independantNavmesh.Navmesh != null && independantNavmesh.Navmesh.IsBuilt())
		{
			NavVector3 start = independantNavmesh.TransformPointFromWorldSpaceToNavSpace(sourcePositionWS);
			NavVector3 end = independantNavmesh.TransformPointFromWorldSpaceToNavSpace(targetPositionWS);
			if (!independantNavmesh.Navmesh.CalculatePath(start, end, pathNS))
			{
				return false;
			}
			return true;
		}
		if (!RustNavigation.Instance.IsDefaultNavmeshBuilt())
		{
			if (AI.logIssues)
			{
				RustNavigation.LogError("Trying to calculate a path on the navmesh before it's built. This will always fail. Make sure to check IsDefaultNavmeshBuilt() before calculating paths.");
			}
			pathNS.Reset();
			return false;
		}
		return RustNavigation.Instance.DefaultNavmesh.CalculatePath(new NavVector3(sourcePositionWS), new NavVector3(targetPositionWS), pathNS);
	}
}
