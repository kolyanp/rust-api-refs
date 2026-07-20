using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Flee : FSMStateBase
{
	[SerializeField]
	public float desiredDistance = 50f;

	[SerializeField]
	public float distance = 20f;

	[SerializeField]
	protected RustNavMeshAgent.Speeds speed = RustNavMeshAgent.Speeds.Sprint;

	[SerializeField]
	private int maxAttempts = 3;

	private int attempts;

	protected float startDistance;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		base.Blackboard.Remove("HitByFire");
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Success;
		}
		attempts = 0;
		startDistance = Vector3.Distance(((Component)Owner).transform.position, targetPosition);
		return MoveAwayFromTarget();
	}

	public override void OnStateExit()
	{
		base.Agent.ResetPath();
		base.OnStateExit();
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (base.Agent.hasPath)
		{
			return base.OnStateUpdate(deltaTime);
		}
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Success;
		}
		if (Vector3.Distance(targetPosition, ((Component)Owner).transform.position) > desiredDistance + startDistance)
		{
			return EFSMStateStatus.Success;
		}
		attempts++;
		if (attempts >= maxAttempts)
		{
			return EFSMStateStatus.Success;
		}
		return MoveAwayFromTarget();
	}

	protected virtual EFSMStateStatus MoveAwayFromTarget()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Success;
		}
		PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
		try
		{
			Eqs.SamplePositionsInDonutShape(base.Agent.nextPosition, (List<Vector3>)(object)val, distance);
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				Vector3 val2 = Vector3Ex.NormalizeXZ(((Component)Owner).transform.position - targetPosition);
				foreach (Vector3 item3 in (List<Vector3>)(object)val)
				{
					Vector3 val3 = item3 - ((Component)Owner).transform.position;
					float item = Vector3.Dot(val2, ((Vector3)(ref val3)).normalized);
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
			((IDisposable)val)?.Dispose();
		}
	}
}
