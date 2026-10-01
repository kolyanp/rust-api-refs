using System;
using System.Collections.Generic;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_TryAmbushUnderwater : FSMStateBase
{
	[SerializeField]
	private Vector2 distanceRange = new Vector2(10f, 20f);

	[SerializeField]
	private float maxDistFromDivingPoint = 50f;

	private const float desiredDepth = 3f;

	private Vector3 divePosition;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		divePosition = ((Component)Owner).transform.position;
		return FindNewUnderwaterWaitingPosition();
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if (!Agent.hasPath)
		{
			return FindNewUnderwaterWaitingPosition();
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		Agent.ResetPath();
		Agent.desiredSwimDepth.Reset();
		base.OnStateExit();
	}

	private EFSMStateStatus FindNewUnderwaterWaitingPosition()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		NavVector3 nextPosition = Agent.nextPosition;
		PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
		try
		{
			float num = Random.Range(distanceRange.x, distanceRange.y);
			bool flag = Eqs.SampleNavigablePositions(Agent, nextPosition, (List<NavVector3>)(object)val, num, num, 8);
			if (Vector3.Distance(divePosition, ((Component)Owner).transform.position) > maxDistFromDivingPoint)
			{
				Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
				try
				{
					NavVector3 normalized = (Agent.WorldToNavSpace(divePosition) - nextPosition).normalized;
					foreach (NavVector3 item2 in (List<NavVector3>)(object)val)
					{
						float item = NavVector3.Dot(normalized, (item2 - nextPosition).NormalizeXZ());
						((List<(NavVector3, float)>)(object)pooledScoreList).Add((item2, item));
					}
					pooledScoreList.SortByScoreDesc(Owner);
					pooledScoreList.Reorder((List<NavVector3>)(object)val);
				}
				finally
				{
					((IDisposable)(object)pooledScoreList)?.Dispose();
				}
			}
			else
			{
				ListEx.Shuffle<NavVector3>((List<NavVector3>)(object)val, (uint)Environment.TickCount);
			}
			foreach (NavVector3 item3 in (List<NavVector3>)(object)val)
			{
				NavVector3 navVector = item3;
				if (!flag)
				{
					if (!Agent.SamplePosition(item3, out var hitNS, 10f))
					{
						continue;
					}
					navVector = hitNS.position;
				}
				if (Agent.IsInWater(navVector))
				{
					RustNavMeshAgent agent = Agent;
					NavVector3 targetPositionNS = navVector;
					RustNavMeshAgent.Speeds? gait = ((!Agent.IsSwimming) ? RustNavMeshAgent.Speeds.Run : RustNavMeshAgent.Speeds.Sneak);
					float? swimDepth = 3f;
					if (agent.SetDestinationWithParams(targetPositionNS, autoBraking: true, gait, null, null, null, swimDepth))
					{
						return EFSMStateStatus.None;
					}
				}
			}
			return EFSMStateStatus.Failure;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public State_TryAmbushUnderwater()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
	}
}
