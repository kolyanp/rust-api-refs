using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

public static class NPCFlankSpot
{
	private static readonly List<Quaternion> sampleRotations;

	public static bool Find(RustNavMeshAgent agent, Vector3 enemyPositionNs, RustNavMeshPath directPath, RustNavMeshPath pathToFlank, RustNavMeshPath pathFromFlankToEnemy, float flankWidth = 15f, float sampleRadius = 3.5f, float minAngle = 30f, float minSimilarity = 0.25f)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isPlaying || !Application.isEditor)
		{
			agent.GetBaseEntity();
		}
		if (directPath.corners.Count < 2)
		{
			return false;
		}
		Vector3 val = Vector3Ex.NormalizeXZ(enemyPositionNs - agent.nextPosition) * flankWidth;
		for (int i = 0; i < sampleRotations.Count; i++)
		{
			Vector3 positionNS = enemyPositionNs + sampleRotations[i] * val;
			if (!agent.SamplePosition(positionNS, out var hitNS, sampleRadius))
			{
				continue;
			}
			Vector3 position = ((NavMeshHit)(ref hitNS)).position;
			if (!agent.CalculatePath(position, enemyPositionNs, pathFromFlankToEnemy) || (int)pathFromFlankToEnemy.status != 0 || pathFromFlankToEnemy.corners.Count < 2)
			{
				continue;
			}
			List<Vector3> corners = directPath.corners;
			Vector3 val2 = corners[corners.Count - 1];
			List<Vector3> corners2 = directPath.corners;
			Vector3 val3 = Vector3Ex.NormalizeXZ(val2 - corners2[corners2.Count - 2]);
			List<Vector3> corners3 = pathFromFlankToEnemy.corners;
			Vector3 val4 = corners3[corners3.Count - 1];
			List<Vector3> corners4 = pathFromFlankToEnemy.corners;
			if (Vector3.Angle(val3, Vector3Ex.NormalizeXZ(val4 - corners4[corners4.Count - 2])) < minAngle || !agent.CalculatePath(position, pathToFlank) || (int)pathToFlank.status != 0 || State_Flank.ComputePathsInitialSimilarity(directPath, pathToFlank) > minSimilarity)
			{
				continue;
			}
			Vector3? val5 = null;
			if (pathToFlank.corners.Count < 2)
			{
				continue;
			}
			for (int j = 0; j < pathToFlank.corners.Count - 1; j++)
			{
				for (int num = pathFromFlankToEnemy.corners.Count - 1; num >= 1; num--)
				{
					if (Vector3.Distance(pathToFlank.corners[j], pathFromFlankToEnemy.corners[num]) < 2f && !agent.Raycast(pathToFlank.corners[j], pathFromFlankToEnemy.corners[num], out var _))
					{
						val5 = pathToFlank.corners[j];
						break;
					}
				}
				if (val5.HasValue)
				{
					break;
				}
			}
			if (val5.HasValue)
			{
				position = val5.Value;
				if (!agent.CalculatePath(position, enemyPositionNs, pathFromFlankToEnemy) || (int)pathFromFlankToEnemy.status != 0)
				{
					return false;
				}
				if (pathFromFlankToEnemy.corners.Count < 2)
				{
					return false;
				}
				List<Vector3> corners5 = directPath.corners;
				Vector3 val6 = corners5[corners5.Count - 1];
				List<Vector3> corners6 = directPath.corners;
				Vector3 val7 = Vector3Ex.NormalizeXZ(val6 - corners6[corners6.Count - 2]);
				List<Vector3> corners7 = pathFromFlankToEnemy.corners;
				Vector3 val8 = corners7[corners7.Count - 1];
				List<Vector3> corners8 = pathFromFlankToEnemy.corners;
				if (Vector3.Angle(val7, Vector3Ex.NormalizeXZ(val8 - corners8[corners8.Count - 2])) < minAngle)
				{
					return false;
				}
				if (!agent.CalculatePath(position, pathToFlank) || (int)pathToFlank.status != 0)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	static NPCFlankSpot()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		sampleRotations = new List<Quaternion>
		{
			Quaternion.Euler(0f, 90f, 0f),
			Quaternion.Euler(0f, -90f, 0f),
			Quaternion.Euler(0f, 45f, 0f),
			Quaternion.Euler(0f, -45f, 0f)
		};
	}
}
