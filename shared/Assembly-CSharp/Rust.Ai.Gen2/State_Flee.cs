using System;
using System.Collections.Generic;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

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

	[SerializeField]
	public WaterAvoidance waterAvoidance;

	[SerializeField]
	public bool clearSensesTargetOnExit;

	private int attempts;

	protected float startDistance;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Blackboard.Remove("HitByFire");
		if (!Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Success;
		}
		attempts = 0;
		startDistance = Vector3.Distance(((Component)Owner).transform.position, targetPosition);
		return MoveAwayFromTarget();
	}

	public override void OnStateExit()
	{
		Agent.ResetPath();
		if (clearSensesTargetOnExit)
		{
			Senses.ClearTarget();
		}
		base.OnStateExit();
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Senses.FindTargetPosition(out var targetPosition);
		if (flag && Vector3.Distance(targetPosition, ((Component)Owner).transform.position) > desiredDistance + startDistance)
		{
			return EFSMStateStatus.Success;
		}
		if (Agent.hasPath)
		{
			return base.OnStateUpdate(deltaTime);
		}
		if (!flag)
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

	protected virtual NavVector3 GetFleeDirection(NavVector3 posNS, Vector3 targetPosition)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return (posNS - Agent.WorldToNavSpace(targetPosition)).NormalizeXZ();
	}

	protected virtual EFSMStateStatus MoveAwayFromTarget()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (!Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Success;
		}
		NavVector3 nextPosition = Agent.nextPosition;
		PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
		try
		{
			bool flag = Eqs.SampleNavigablePositions(Agent, nextPosition, (List<NavVector3>)(object)val, distance, distance, 8);
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				NavVector3 fleeDirection = GetFleeDirection(nextPosition, targetPosition);
				foreach (NavVector3 item2 in (List<NavVector3>)(object)val)
				{
					float num = NavVector3.Dot(fleeDirection, (item2 - nextPosition).NormalizeXZ());
					if (waterAvoidance != WaterAvoidance.None && WaterLevel.GetOverallWaterDepth(Agent.NavToWorldSpace(item2), waves: false, volumes: false, Owner) > 0.01f)
					{
						switch (waterAvoidance)
						{
						case WaterAvoidance.Prefer:
							num += 0.5f;
							break;
						case WaterAvoidance.Avoid:
							num--;
							break;
						case WaterAvoidance.Refuse:
							continue;
						}
					}
					((List<(NavVector3, float)>)(object)pooledScoreList).Add((item2, num));
				}
				pooledScoreList.SortByScoreDesc(Owner);
				foreach (var item3 in (List<(NavVector3, float)>)(object)pooledScoreList)
				{
					NavVector3 item = item3.Item1;
					NavVector3 navVector = item;
					if (!flag)
					{
						if (!Agent.SamplePosition(item, out var hitNS, 10f))
						{
							continue;
						}
						navVector = hitNS.position;
					}
					if ((Agent.canSwim || !Agent.IsInWater(navVector)) && Agent.SetDestinationWithParams(navVector, autoBraking: false, speed))
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
