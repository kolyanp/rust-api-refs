using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class NpcLevelScript : TriggerBase, IServerComponent
{
	public List<NpcLevelTrigger> linkedTriggers = new List<NpcLevelTrigger>();

	public List<NpcPositionHint> positionHints = new List<NpcPositionHint>();

	public void OnDrawGizmosSelected()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		BoxCollider val = default(BoxCollider);
		foreach (NpcLevelTrigger linkedTrigger in linkedTriggers)
		{
			if (((Behaviour)linkedTrigger).isActiveAndEnabled && ((Component)linkedTrigger).TryGetComponent<BoxCollider>(ref val))
			{
				Gizmos.color = Color.cyan;
				Matrix4x4 matrix = Gizmos.matrix;
				Gizmos.matrix = Matrix4x4.TRS(((Component)val).transform.position, ((Component)val).transform.rotation, ((Component)val).transform.lossyScale);
				Gizmos.DrawWireCube(val.center, val.size);
				Gizmos.matrix = matrix;
			}
		}
		Collider val3 = default(Collider);
		foreach (NpcPositionHint positionHint in positionHints)
		{
			if ((Object)(object)positionHint == (Object)null || !((Behaviour)positionHint).isActiveAndEnabled || positionHint is NpcGrenadePositionHint)
			{
				continue;
			}
			Vector3? val2 = null;
			float num = float.PositiveInfinity;
			foreach (NpcLevelTrigger linkedTrigger2 in linkedTriggers)
			{
				if (!((Object)(object)linkedTrigger2 == (Object)null) && ((Behaviour)linkedTrigger2).isActiveAndEnabled && ((Component)linkedTrigger2).TryGetComponent<Collider>(ref val3))
				{
					Vector3 val4 = val3.ClosestPoint(((Component)positionHint).transform.position);
					Vector3 val5 = val4 - ((Component)positionHint).transform.position;
					float sqrMagnitude = ((Vector3)(ref val5)).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						val2 = val4;
					}
				}
			}
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(((Component)positionHint).transform.position, 0.2f);
			Gizmos.DrawLine(((Component)positionHint).transform.position, ((Component)positionHint).transform.position + 1.8f * Vector3.up);
			if (val2.HasValue)
			{
				Gizmos.DrawLine(((Component)positionHint).transform.position + 1.8f * Vector3.up, val2.Value);
			}
		}
		foreach (NpcPositionHint positionHint2 in positionHints)
		{
			if ((Object)(object)positionHint2 == (Object)null || !((Behaviour)positionHint2).isActiveAndEnabled || !(positionHint2 is NpcGrenadePositionHint npcGrenadePositionHint))
			{
				continue;
			}
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(((Component)positionHint2).transform.position, 0.2f);
			Gizmos.DrawLine(((Component)positionHint2).transform.position, ((Component)positionHint2).transform.position + 1.8f * Vector3.up);
			if (!((Object)(object)npcGrenadePositionHint.landingPoint == (Object)null))
			{
				Vector3 val6 = ((Component)npcGrenadePositionHint).transform.position + 1.8f * Vector3.up;
				Vector3 position = npcGrenadePositionHint.landingPoint.position;
				Vector3 val7 = Vector3Ex.WithY((val6 + position) * 0.5f, Mathf.Max(val6.y, position.y) + npcGrenadePositionHint.apexHeight);
				int num2 = 20;
				Vector3 val8 = val6;
				for (int i = 1; i <= num2; i++)
				{
					float num3 = (float)i / (float)num2;
					Vector3 val9 = Vector3.Lerp(Vector3.Lerp(val6, val7, num3), Vector3.Lerp(val7, position, num3), num3);
					Gizmos.DrawLine(val8, val9);
					val8 = val9;
				}
			}
		}
	}
}
