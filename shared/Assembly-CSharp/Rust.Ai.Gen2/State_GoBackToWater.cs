using System;
using System.Collections.Generic;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_GoBackToWater : State_MoveToTarget
{
	private NavVector3 nearestWaterPoint;

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
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
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
			PooledList<NavVector3> val3 = Pool.Get<PooledList<NavVector3>>();
			try
			{
				bool flag = Eqs.SampleNavigablePositions(base.Agent, base.Agent.WorldToNavSpace(val2), (List<NavVector3>)(object)val3, 10f, 10f, 8);
				ListEx.Shuffle<NavVector3>((List<NavVector3>)(object)val3, (uint)Environment.TickCount);
				nearestWaterPoint = base.Agent.WorldToNavSpace(val2);
				foreach (NavVector3 item3 in (List<NavVector3>)(object)val3)
				{
					NavVector3 positionNS = item3;
					if (!flag)
					{
						if (!base.Agent.SamplePosition(item3, out var hitNS, 10f))
						{
							continue;
						}
						positionNS = hitNS.position;
					}
					if (base.Agent.IsInWater(positionNS))
					{
						nearestWaterPoint = positionNS;
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

	protected override bool GetMoveDestination(out NavVector3 destination)
	{
		destination = nearestWaterPoint;
		return true;
	}
}
