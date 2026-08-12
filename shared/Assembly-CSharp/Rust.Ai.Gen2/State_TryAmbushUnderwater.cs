using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_TryAmbushUnderwater : FSMStateBase
{
	[SerializeField]
	private Vector2 distanceRange;

	[SerializeField]
	private float maxDistFromDivingPoint;

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
		if (!base.Agent.hasPath)
		{
			return FindNewUnderwaterWaitingPosition();
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		base.Agent.ResetPath();
		base.Agent.desiredSwimDepth.Reset();
		base.OnStateExit();
	}

	private EFSMStateStatus FindNewUnderwaterWaitingPosition()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
		try
		{
			float radius = Random.Range(distanceRange.x, distanceRange.y);
			Eqs.SamplePositionsInDonutShape(base.Agent.nextPosition, (List<Vector3>)(object)val, radius);
			if (Vector3.Distance(divePosition, ((Component)Owner).transform.position) > maxDistFromDivingPoint)
			{
				Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
				try
				{
					Vector3 val2 = divePosition - ((Component)Owner).transform.position;
					Vector3 normalized = ((Vector3)(ref val2)).normalized;
					foreach (Vector3 item2 in (List<Vector3>)(object)val)
					{
						val2 = item2 - ((Component)Owner).transform.position;
						float item = Vector3.Dot(normalized, ((Vector3)(ref val2)).normalized);
						((List<(Vector3, float)>)(object)pooledScoreList).Add((item2, item));
					}
					pooledScoreList.SortByScoreDesc(Owner);
					pooledScoreList.Reorder((List<Vector3>)(object)val);
				}
				finally
				{
					((IDisposable)(object)pooledScoreList)?.Dispose();
				}
			}
			else
			{
				ListEx.Shuffle<Vector3>((List<Vector3>)(object)val, (uint)Environment.TickCount);
			}
			foreach (Vector3 item3 in (List<Vector3>)(object)val)
			{
				if (base.Agent.SamplePosition(item3, out var hitNS, 10f) && base.Agent.IsInWater(((NavMeshHit)(ref hitNS)).position))
				{
					RustNavMeshAgent agent = base.Agent;
					Vector3 position = ((NavMeshHit)(ref hitNS)).position;
					RustNavMeshAgent.Speeds? gait = ((!base.Agent.IsSwimming) ? RustNavMeshAgent.Speeds.Run : RustNavMeshAgent.Speeds.Sneak);
					float? swimDepth = 3f;
					if (agent.SetDestinationWithParams(position, autoBraking: true, gait, null, null, null, swimDepth))
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
		distanceRange = new Vector2(10f, 20f);
		maxDistFromDivingPoint = 50f;
		base._002Ector();
	}
}
