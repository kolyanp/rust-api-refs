using System.Collections.Generic;
using Rust.Ai.Gen2.Nav;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

public static class NPCOverwatchSpot
{
	public static (NavVector3 loc, Vector3 dir)? Find(List<NavVector3> corners)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		if (corners.Count < 3)
		{
			return null;
		}
		NavMeshHit val = default(NavMeshHit);
		RaycastHit val2 = default(RaycastHit);
		RaycastHit val3 = default(RaycastHit);
		for (int num = corners.Count - 1; num >= 2; num--)
		{
			NavVector3 navVector = corners[num];
			NavVector3 navVector2 = corners[num - 1];
			NavVector3 navVector3 = corners[num - 2];
			NavVector3 navVector4 = (navVector - navVector2).NormalizeXZ();
			NavVector3 navVector5 = (navVector3 - navVector2).NormalizeXZ();
			NavVector3 navVector6 = (navVector4 + navVector5).NormalizeXZ() * -1f * 0.01f;
			navVector += navVector6;
			navVector2 += navVector6;
			NavVector3 navVector7 = (navVector2 - navVector).NormalizeXZ() * 100f;
			if (NavMesh.Raycast(navVector.Value, (navVector + navVector7).Value, ref val, -1))
			{
				NavVector3 navVector8 = new NavVector3(((NavMeshHit)(ref val)).position);
				if (((NavMeshHit)(ref val)).distance >= 7f)
				{
					Vector3 value = corners[corners.Count - 1].Value;
					Vector3 position = ((NavMeshHit)(ref val)).position;
					if (Physics.Linecast(value + 1.7f * Vector3.up, position + 1.7f * Vector3.up, ref val2, 1218652417) && Physics.Linecast(value + 0.2f * Vector3.up, position + 0.2f * Vector3.up, ref val3, 1218652417))
					{
						return (navVector8, NavVector3.LookDirection(navVector8, navVector));
					}
				}
			}
		}
		return null;
	}
}
