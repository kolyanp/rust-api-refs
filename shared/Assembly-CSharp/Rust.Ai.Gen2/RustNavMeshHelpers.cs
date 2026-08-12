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

	public static bool SamplePosition(Vector3 sourcePositionNS, out NavMeshHit hitNS, float maxDistance, int areaMask)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh)
		{
			return NavMesh.SamplePosition(sourcePositionNS, ref hitNS, maxDistance, areaMask);
		}
		IndependantNavmesh independantNavmesh = IndependantNavmesh.FindNavmeshAtPosition(sourcePositionNS);
		if ((Object)(object)independantNavmesh != (Object)null && independantNavmesh.Navmesh != null && independantNavmesh.Navmesh.IsBuilt())
		{
			return independantNavmesh.Navmesh.SamplePosition(sourcePositionNS, out hitNS, Vector3.one * maxDistance);
		}
		if (!RustNavigation.Instance.IsDefaultNavmeshBuilt())
		{
			if (AI.logIssues)
			{
				RustNavigation.LogError("Trying to sample position on the navmesh before it's built. This will always fail. Make sure to check IsDefaultNavmeshBuilt() before sampling positions.");
			}
			hitNS = default(NavMeshHit);
			return false;
		}
		return RustNavigation.Instance.DefaultNavmesh.SamplePosition(sourcePositionNS, out hitNS, Vector3.one * maxDistance);
	}

	public static bool SamplePositionWS(BaseEntity entity, Vector3 sourcePositionWS, out NavMeshHit hitWS, float maxDistance, int areaMask)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh)
		{
			if ((Object)(object)entity == (Object)null)
			{
				return NavMesh.SamplePosition(sourcePositionWS, ref hitWS, maxDistance, areaMask);
			}
			Matrix4x4 val = entity.WorldToNavMeshSpace;
			NavMeshHit val2 = default(NavMeshHit);
			bool num = NavMesh.SamplePosition(((Matrix4x4)(ref val)).MultiplyPoint(sourcePositionWS), ref val2, maxDistance, areaMask);
			hitWS = val2;
			if (num)
			{
				val = entity.NavMeshToWorldSpace;
				((NavMeshHit)(ref hitWS)).position = ((Matrix4x4)(ref val)).MultiplyPoint(((NavMeshHit)(ref val2)).position);
			}
			return num;
		}
		IndependantNavmesh independantNavmesh = IndependantNavmesh.FindNavmeshAtPosition(sourcePositionWS);
		if ((Object)(object)independantNavmesh != (Object)null && independantNavmesh.Navmesh != null && independantNavmesh.Navmesh.IsBuilt())
		{
			Vector3 position = independantNavmesh.TransformPointFromWorldSpaceToNavSpace(sourcePositionWS);
			bool num2 = independantNavmesh.Navmesh.SamplePosition(position, out var hit, Vector3.one * maxDistance);
			hitWS = hit;
			if (num2)
			{
				((NavMeshHit)(ref hitWS)).position = independantNavmesh.TransformPointFromNavSpaceToWorldSpace(((NavMeshHit)(ref hit)).position);
			}
			return num2;
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
		return RustNavigation.Instance.DefaultNavmesh.SamplePosition(sourcePositionWS, out hitWS, Vector3.one * maxDistance);
	}

	public static bool Raycast(Vector3 sourcePositionNS, Vector3 targetPositionNS, out NavMeshHit hitWS, int areaMask)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh)
		{
			return NavMesh.Raycast(sourcePositionNS, targetPositionNS, ref hitWS, areaMask);
		}
		IndependantNavmesh independantNavmesh = IndependantNavmesh.FindNavmeshAtPosition(sourcePositionNS);
		if ((Object)(object)independantNavmesh != (Object)null && independantNavmesh.Navmesh != null && independantNavmesh.Navmesh.IsBuilt())
		{
			return independantNavmesh.Navmesh.Raycast(sourcePositionNS, targetPositionNS, out hitWS);
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
		return RustNavigation.Instance.DefaultNavmesh.Raycast(sourcePositionNS, targetPositionNS, out hitWS);
	}

	public static bool CalculatePath(Vector3 sourcePositionNS, Vector3 targetPositionNS, NavMeshQueryFilter filter, RustNavMeshPath pathNS)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return CalculatePath(sourcePositionNS, targetPositionNS, ((NavMeshQueryFilter)(ref filter)).areaMask, pathNS);
	}

	public static bool CalculatePath(Vector3 sourcePositionNS, Vector3 targetPositionNS, int areaMask, RustNavMeshPath pathNS)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		pathNS.corners.Clear();
		if (AI.useUnityNavmesh)
		{
			if (pathNS.unityPath == null)
			{
				pathNS.unityPath = new NavMeshPath();
			}
			bool result = NavMesh.CalculatePath(sourcePositionNS, targetPositionNS, areaMask, pathNS.unityPath);
			pathNS.status = pathNS.unityPath.status;
			int cornersNonAlloc = pathNS.unityPath.GetCornersNonAlloc(cornerBuffer);
			for (int i = 0; i < cornersNonAlloc; i++)
			{
				pathNS.corners.Add(cornerBuffer[i]);
			}
			return result;
		}
		IndependantNavmesh independantNavmesh = IndependantNavmesh.FindNavmeshAtPosition(sourcePositionNS);
		if ((Object)(object)independantNavmesh != (Object)null && independantNavmesh.Navmesh != null && independantNavmesh.Navmesh.IsBuilt())
		{
			return independantNavmesh.Navmesh.CalculatePath(sourcePositionNS, targetPositionNS, pathNS);
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
		return RustNavigation.Instance.DefaultNavmesh.CalculatePath(sourcePositionNS, targetPositionNS, pathNS);
	}
}
