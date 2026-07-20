using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_FleeToHide : State_Flee
{
	public const string HitDuringChargeKey = "HitDuringCharge";

	private bool clockWise;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		base.Blackboard.Remove("HitDuringCharge");
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Success;
		}
		Vector3 val = Vector3Ex.NormalizeXZ(targetPosition - ((Component)Owner).transform.position);
		clockWise = Vector3.Dot(((Component)Owner).transform.right, val) > 0f;
		return base.OnStateEnter(payload);
	}

	protected override EFSMStateStatus MoveAwayFromTarget()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Success;
		}
		Vector3 val = ((Component)Owner).transform.position - targetPosition;
		float magnitude = ((Vector3)(ref val)).magnitude;
		Vector3 val2 = ((Component)Owner).transform.forward;
		float num = 15f;
		if (magnitude > 6f)
		{
			val2 = Vector3Ex.NormalizeXZ(((Component)Owner).transform.position - targetPosition);
			num = 55f;
		}
		val2 = Quaternion.AngleAxis(num * (clockWise ? 1f : (-1f)), Vector3.up) * val2;
		PooledList<Vector3> val3 = Pool.Get<PooledList<Vector3>>();
		try
		{
			Eqs.SamplePositionsInDonutShape(base.Agent.nextPosition, (List<Vector3>)(object)val3, distance);
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				foreach (Vector3 item3 in (List<Vector3>)(object)val3)
				{
					Vector3 val4 = val2;
					Vector3 val5 = item3 - ((Component)Owner).transform.position;
					float item = Vector3.Dot(val4, ((Vector3)(ref val5)).normalized);
					((List<(Vector3, float)>)(object)pooledScoreList).Add((item3, item));
				}
				pooledScoreList.SortByScoreDesc(Owner);
				foreach (var item4 in (List<(Vector3, float)>)(object)pooledScoreList)
				{
					Vector3 item2 = item4.Item1;
					if (base.Agent.SamplePosition(item2, out var hitNS, 10f) && (base.Agent.canSwim || !base.Agent.IsInWater(((NavMeshHit)(ref hitNS)).position)) && base.Agent.SetDestinationWithParams(((NavMeshHit)(ref hitNS)).position, autoBraking: false, speed))
					{
						return EFSMStateStatus.None;
					}
				}
				return EFSMStateStatus.Failure;
			}
			finally
			{
				((IDisposable)(object)pooledScoreList)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}
}
