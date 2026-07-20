using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Roam : FSMStateBase
{
	[SerializeField]
	private Vector2 distanceRange = new Vector2(10f, 20f);

	[SerializeField]
	private float homeRadius = 50f;

	[SerializeField]
	private RustNavMeshAgent.Speeds minSpeed;

	[SerializeField]
	private RustNavMeshAgent.Speeds maxSpeed = RustNavMeshAgent.Speeds.Sprint;

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
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
		try
		{
			float num = Random.Range(distanceRange.x, distanceRange.y);
			Eqs.SamplePositionsInDonutShape(base.Agent.nextPosition, (List<Vector3>)(object)val, num);
			bool flag = Vector3.Distance(spawnPosition.Value, ((Component)Owner).transform.position) > homeRadius;
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				Vector3 val2 = spawnPosition.Value - ((Component)Owner).transform.position;
				Vector3 normalized = ((Vector3)(ref val2)).normalized;
				foreach (Vector3 item2 in (List<Vector3>)(object)val)
				{
					float num2 = 0f;
					if (flag)
					{
						float num3 = num2;
						val2 = item2 - ((Component)Owner).transform.position;
						num2 = num3 + Mathx.RemapValClamped(Vector3.Dot(normalized, ((Vector3)(ref val2)).normalized), -1f, 1f, 0f, 1f);
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
					((List<(Vector3, float)>)(object)pooledScoreList).Add((item2, num2));
				}
				pooledScoreList.SortByScoreDesc(Owner);
				foreach (var item3 in (List<(Vector3, float)>)(object)pooledScoreList)
				{
					Vector3 item = item3.Item1;
					if (base.Agent.SamplePosition(item, out var hitNS, 10f) && (base.Agent.canSwim || !base.Agent.IsInWater(((NavMeshHit)(ref hitNS)).position)) && base.Agent.SetDestinationWithParams(((NavMeshHit)(ref hitNS)).position))
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
}
