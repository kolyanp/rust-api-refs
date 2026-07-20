using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;
using UnityEngine.AI;

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
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
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
		Vector3 val3 = Quaternion.AngleAxis(Random.Range(-50f, 50f), Vector3.up) * val2;
		float radius;
		RustNavMeshAgent.Speeds value;
		if (Random.value > 0.95f && Vector3.Distance(targetPosition, position) > 8f)
		{
			radius = Random.Range(3f, 4f);
			value = RustNavMeshAgent.Speeds.Sprint;
		}
		else
		{
			radius = Random.Range(1f, 2f);
			value = RustNavMeshAgent.Speeds.Walk;
		}
		PooledList<Vector3> val4 = Pool.Get<PooledList<Vector3>>();
		try
		{
			Eqs.SamplePositionsInDonutShape(position, (List<Vector3>)(object)val4, radius);
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				foreach (Vector3 item3 in (List<Vector3>)(object)val4)
				{
					val = item3 - position;
					float item = Mathx.RemapValClamped(Vector3.Dot(val3, ((Vector3)(ref val)).normalized), -1f, 1f, 0f, 1f);
					((List<(Vector3, float)>)(object)pooledScoreList).Add((item3, item));
				}
				pooledScoreList.SortByScoreDesc(Owner);
				Matrix4x4 worldToNavMeshSpace = Owner.WorldToNavMeshSpace;
				Matrix4x4 navMeshToWorldSpace = Owner.NavMeshToWorldSpace;
				foreach (var item4 in (List<(Vector3, float)>)(object)pooledScoreList)
				{
					Vector3 item2 = item4.Item1;
					Vector3 positionNS = ((Matrix4x4)(ref worldToNavMeshSpace)).MultiplyPoint(item2);
					if (base.Agent.SamplePosition(positionNS, out var hitNS, 3.5f))
					{
						Vector3 position2 = ((Matrix4x4)(ref navMeshToWorldSpace)).MultiplyPoint(((NavMeshHit)(ref hitNS)).position);
						if (!base.Agent.IsInWater(position2) && base.Agent.SetDestinationWithParams(((NavMeshHit)(ref hitNS)).position, autoBraking: false, value))
						{
							return EFSMStateStatus.None;
						}
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
			((IDisposable)val4)?.Dispose();
		}
	}
}
