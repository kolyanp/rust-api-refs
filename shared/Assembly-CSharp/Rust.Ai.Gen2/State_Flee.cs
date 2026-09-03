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
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Success;
		}
		NavVector3 nextPosition = base.Agent.nextPosition;
		PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
		try
		{
			bool flag = Eqs.SampleNavigablePositions(base.Agent, nextPosition, (List<NavVector3>)(object)val, distance, distance, 8);
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				NavVector3 aNS = (nextPosition - base.Agent.WorldToNavSpace(targetPosition)).NormalizeXZ();
				foreach (NavVector3 item3 in (List<NavVector3>)(object)val)
				{
					float item = NavVector3.Dot(aNS, (item3 - nextPosition).NormalizeXZ());
					((List<(NavVector3, float)>)(object)pooledScoreList).Add((item3, item));
				}
				pooledScoreList.SortByScoreDesc(Owner);
				foreach (var item4 in (List<(NavVector3, float)>)(object)pooledScoreList)
				{
					NavVector3 item2 = item4.Item1;
					NavVector3 navVector = item2;
					if (!flag)
					{
						if (!base.Agent.SamplePosition(item2, out var hitNS, 10f))
						{
							continue;
						}
						navVector = hitNS.position;
					}
					if ((base.Agent.canSwim || !base.Agent.IsInWater(navVector)) && base.Agent.SetDestinationWithParams(navVector, autoBraking: false, speed))
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
