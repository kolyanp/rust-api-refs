using System;
using System.Collections.Generic;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Roam : FSMStateBase
{
	[SerializeField]
	private Vector2 distanceRange;

	[SerializeField]
	private float homeRadius;

	[SerializeField]
	private RustNavMeshAgent.Speeds minSpeed;

	[SerializeField]
	private RustNavMeshAgent.Speeds maxSpeed;

	[SerializeField]
	protected bool favourWater;

	private Vector3? spawnPosition;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Reset();
		if (!spawnPosition.HasValue)
		{
			spawnPosition = ((Component)Owner).transform.position;
		}
		if (!TrySetRoamDestination())
		{
			return EFSMStateStatus.Failure;
		}
		return base.OnStateEnter(payload);
	}

	private bool TrySetRoamDestination()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		NavVector3 nextPosition = base.Agent.nextPosition;
		PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
		try
		{
			float num = Random.Range(distanceRange.x, distanceRange.y);
			bool flag = Eqs.SampleNavigablePositions(base.Agent, nextPosition, (List<NavVector3>)(object)val, num, num, 8);
			bool flag2 = Vector3.Distance(spawnPosition.Value, ((Component)Owner).transform.position) > homeRadius;
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				NavVector3 normalized = (base.Agent.WorldToNavSpace(spawnPosition.Value) - nextPosition).normalized;
				foreach (NavVector3 item2 in (List<NavVector3>)(object)val)
				{
					float num2 = 0f;
					if (flag2)
					{
						num2 += Mathx.RemapValClamped(NavVector3.Dot(normalized, (item2 - nextPosition).NormalizeXZ()), -1f, 1f, 0f, 1f);
						if (base.Agent.IsPositionOnFavoredTerrain(item2))
						{
							num2 += 0.25f;
						}
					}
					else
					{
						num2 += Random.value;
						if (base.Agent.IsPositionOnFavoredTerrain(item2))
						{
							num2 += 10f;
						}
					}
					((List<(NavVector3, float)>)(object)pooledScoreList).Add((item2, num2));
				}
				pooledScoreList.SortByScoreDesc(Owner);
				foreach (var item3 in (List<(NavVector3, float)>)(object)pooledScoreList)
				{
					NavVector3 item = item3.Item1;
					NavVector3 navVector = item;
					if (!flag)
					{
						if (!base.Agent.SamplePosition(item, out var hitNS, 10f))
						{
							continue;
						}
						navVector = hitNS.position;
					}
					if ((base.Agent.canSwim || !base.Agent.IsInWater(navVector)) && base.Agent.SetDestinationWithParams(navVector))
					{
						float ratio = Mathf.InverseLerp(0f, distanceRange.y, num);
						base.Agent.SetSpeedRatio(ratio, minSpeed, maxSpeed);
						return true;
					}
				}
				return false;
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

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if (!base.Agent.hasPath)
		{
			return EFSMStateStatus.Success;
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		base.Agent.ResetPath();
		base.OnStateExit();
	}

	private void Reset()
	{
		base.Senses.ClearTarget();
		base.Blackboard.Clear();
		if (Owner is BaseCombatEntity { healthFraction: <1f, SecondsSinceAttacked: >120f } baseCombatEntity)
		{
			baseCombatEntity.SetHealth(Owner.MaxHealth());
		}
	}

	public State_Roam()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		distanceRange = new Vector2(10f, 20f);
		homeRadius = 50f;
		maxSpeed = RustNavMeshAgent.Speeds.Sprint;
		base._002Ector();
	}
}
