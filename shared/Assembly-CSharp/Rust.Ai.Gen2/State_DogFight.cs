using System;
using System.Collections.Generic;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_DogFight : FSMStateBase
{
	private bool shouldGoRightNext;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if (CalculatePathDestination() == EFSMStateStatus.Failure)
		{
			return EFSMStateStatus.Failure;
		}
		shouldGoRightNext = Random.value > 0.5f;
		return base.OnStateEnter(payload);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if (base.Agent.hasPath)
		{
			return base.OnStateUpdate(deltaTime);
		}
		if (CalculatePathDestination() == EFSMStateStatus.Failure)
		{
			return EFSMStateStatus.Failure;
		}
		return EFSMStateStatus.None;
	}

	public override void OnStateExit()
	{
		base.Agent.ResetPath();
		base.OnStateExit();
	}

	private EFSMStateStatus CalculatePathDestination()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Failure;
		}
		Vector3 position = ((Component)Owner).transform.position;
		Vector3 val = targetPosition - position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 normalized2;
		if (!shouldGoRightNext)
		{
			val = Vector3.Cross(Vector3.up, normalized);
			normalized2 = ((Vector3)(ref val)).normalized;
		}
		else
		{
			val = Vector3.Cross(normalized, Vector3.up);
			normalized2 = ((Vector3)(ref val)).normalized;
		}
		Vector3 val2 = normalized2;
		shouldGoRightNext = !shouldGoRightNext;
		Vector3 directionWS = Quaternion.AngleAxis(Random.Range(-50f, 50f), Vector3.up) * val2;
		float num;
		RustNavMeshAgent.Speeds value;
		if (Random.value > 0.95f && Vector3.Distance(targetPosition, position) > 8f)
		{
			num = Random.Range(3f, 4f);
			value = RustNavMeshAgent.Speeds.Sprint;
		}
		else
		{
			num = Random.Range(1f, 2f);
			value = RustNavMeshAgent.Speeds.Walk;
		}
		NavVector3 nextPosition = base.Agent.nextPosition;
		NavVector3 aNS = base.Agent.WorldToNavDirection(directionWS);
		PooledList<NavVector3> val3 = Pool.Get<PooledList<NavVector3>>();
		try
		{
			bool flag = Eqs.SampleNavigablePositions(base.Agent, nextPosition, (List<NavVector3>)(object)val3, num, num, 8);
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				foreach (NavVector3 item3 in (List<NavVector3>)(object)val3)
				{
					float item = Mathx.RemapValClamped(NavVector3.Dot(aNS, (item3 - nextPosition).NormalizeXZ()), -1f, 1f, 0f, 1f);
					((List<(NavVector3, float)>)(object)pooledScoreList).Add((item3, item));
				}
				pooledScoreList.SortByScoreDesc(Owner);
				foreach (var item4 in (List<(NavVector3, float)>)(object)pooledScoreList)
				{
					NavVector3 item2 = item4.Item1;
					NavVector3 navVector = item2;
					if (!flag)
					{
						if (!base.Agent.SamplePosition(item2, out var hitNS, 3.5f))
						{
							continue;
						}
						navVector = hitNS.position;
					}
					Vector3 positionWS = base.Agent.NavToWorldSpace(navVector);
					if (!base.Agent.IsInWater(positionWS) && base.Agent.SetDestinationWithParams(navVector, autoBraking: false, value))
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
