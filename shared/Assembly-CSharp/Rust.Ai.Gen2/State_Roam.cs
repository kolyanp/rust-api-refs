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
	private Vector2 distanceRange = new Vector2(10f, 20f);

	[SerializeField]
	private float homeRadius = 50f;

	[NonSerialized]
	private float authoredHomeRadius = -1f;

	[SerializeField]
	private RustNavMeshAgent.Speeds minSpeed;

	[SerializeField]
	private RustNavMeshAgent.Speeds maxSpeed = RustNavMeshAgent.Speeds.Sprint;

	[Tooltip("When determining the speed to roam at, does the interpolation start from the minimum distance range (true), or from 0 (false)")]
	[SerializeField]
	private bool useDistanceRangeMinforMinSpeed;

	[SerializeField]
	protected WaterAvoidance waterAvoidance;

	private Vector3? spawnPosition;

	protected virtual Vector3 HomePosition
	{
		get
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			return spawnPosition ?? ((Component)Owner).transform.position;
		}
	}

	protected virtual bool HealsWhenUndisturbed => true;

	public void SetHomeRadiusScale(float scale)
	{
		if (authoredHomeRadius < 0f)
		{
			authoredHomeRadius = homeRadius;
		}
		homeRadius = authoredHomeRadius * Mathf.Clamp01(scale);
	}

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
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		NavVector3 nextPosition = Agent.nextPosition;
		PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
		try
		{
			float num = Random.Range(distanceRange.x, distanceRange.y);
			bool flag = Eqs.SampleNavigablePositions(Agent, nextPosition, (List<NavVector3>)(object)val, num, num, 8);
			Vector3 homePosition = HomePosition;
			bool flag2 = Vector3.Distance(homePosition, ((Component)Owner).transform.position) > homeRadius;
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				NavVector3 normalized = (Agent.WorldToNavSpace(homePosition) - nextPosition).normalized;
				foreach (NavVector3 item2 in (List<NavVector3>)(object)val)
				{
					float num2 = 0f;
					if (flag2)
					{
						num2 += Mathx.RemapValClamped(NavVector3.Dot(normalized, (item2 - nextPosition).NormalizeXZ()), -1f, 1f, 0f, 1f);
						if (Agent.IsPositionOnFavoredTerrain(item2))
						{
							num2 += 0.25f;
						}
					}
					else
					{
						num2 += Random.value;
						if (Agent.IsPositionOnFavoredTerrain(item2))
						{
							num2 += 10f;
						}
					}
					if (waterAvoidance != WaterAvoidance.None && WaterLevel.GetOverallWaterDepth(Agent.NavToWorldSpace(item2), waves: false, volumes: false, Owner) > 0.01f)
					{
						switch (waterAvoidance)
						{
						case WaterAvoidance.Prefer:
							num2 += 0.5f;
							break;
						case WaterAvoidance.Avoid:
							num2--;
							break;
						case WaterAvoidance.Refuse:
							continue;
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
						if (!Agent.SamplePosition(item, out var hitNS, 10f))
						{
							continue;
						}
						navVector = hitNS.position;
					}
					if ((Agent.canSwim || !Agent.IsInWater(navVector)) && Agent.SetDestinationWithParams(navVector))
					{
						float ratio = Mathf.InverseLerp(useDistanceRangeMinforMinSpeed ? distanceRange.x : 0f, distanceRange.y, num);
						Agent.SetSpeedRatio(ratio, minSpeed, maxSpeed);
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
		if (!Agent.hasPath)
		{
			return EFSMStateStatus.Success;
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		Agent.ResetPath();
		base.OnStateExit();
	}

	private void Reset()
	{
		Senses.ClearTarget();
		Blackboard.Clear();
		if (HealsWhenUndisturbed && Owner is BaseCombatEntity { healthFraction: <1f, SecondsSinceAttacked: >120f } baseCombatEntity)
		{
			baseCombatEntity.SetHealth(Owner.MaxHealth());
		}
	}

	public State_Roam()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
	}
}
