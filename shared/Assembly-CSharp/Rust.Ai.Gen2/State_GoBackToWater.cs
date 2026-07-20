using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_GoBackToWater : State_MoveToTarget
{
	private Vector3 nearestWaterPoint;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		if (base.Agent.IsSwimming)
		{
			return EFSMStateStatus.Success;
		}
		using (TimeWarning.New("State_GoBackToWater GetCoarseVectorToShore and GetHeight"))
		{
			(Vector3 shoreDir, float shoreDist) coarseVectorToShore = TerrainTexturing.Instance.GetCoarseVectorToShore(((Component)Owner).transform.position);
			Vector3 item = coarseVectorToShore.shoreDir;
			float item2 = coarseVectorToShore.shoreDist;
			Vector3 val = item * item2;
			Vector3 val2 = ((Component)Owner).transform.position + ((Vector3)(ref val)).normalized * (((Vector3)(ref val)).magnitude + 10f);
			val2.y = TerrainMeta.HeightMap.GetHeight(val2);
			PooledList<Vector3> val3 = Pool.Get<PooledList<Vector3>>();
			try
			{
				Eqs.SamplePositionsInDonutShape(val2, (List<Vector3>)(object)val3);
				ListEx.Shuffle<Vector3>((List<Vector3>)(object)val3, (uint)Environment.TickCount);
				nearestWaterPoint = val2;
				foreach (Vector3 item3 in (List<Vector3>)(object)val3)
				{
					if (base.Agent.SamplePosition(item3, out var hitNS, 10f) && base.Agent.IsInWater(((NavMeshHit)(ref hitNS)).position))
					{
						nearestWaterPoint = ((NavMeshHit)(ref hitNS)).position;
						break;
					}
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		return base.OnStateEnter(payload);
	}

	protected override bool GetMoveDestination(out Vector3 destination)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		destination = nearestWaterPoint;
		return true;
	}
}
