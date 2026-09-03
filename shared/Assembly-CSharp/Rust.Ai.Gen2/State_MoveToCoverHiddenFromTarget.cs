using System;
using System.Collections.Generic;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_MoveToCoverHiddenFromTarget : FSMStateBase
{
	public float searchRadius = 8f;

	public float walkDurationBeforeSprint = 1f;

	public RustNavMeshAgent.Speeds speed = RustNavMeshAgent.Speeds.Walk;

	private static RustNavMeshPath _path;

	private NpcShootingComponent _shooting;

	private NpcZoneComponent _npcZoneComponent;

	private double? remainingWalkBeforeSprintTime;

	private NavVector3? lastChosenHidingSpotNS;

	private AIInformationZone _infoZone;

	private AICoverPoint heldCover;

	private static RustNavMeshPath Path => _path ?? (_path = new RustNavMeshPath());

	private NpcShootingComponent Shooting => _shooting ?? (_shooting = ((Component)Owner).GetComponent<NpcShootingComponent>());

	private NpcZoneComponent NpcZoneComponent => _npcZoneComponent ?? (_npcZoneComponent = ((Component)Owner).GetComponent<NpcZoneComponent>());

	private AIInformationZone InfoZone
	{
		get
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			return _infoZone ?? (_infoZone = AIInformationZone.GetForPoint(((Component)Owner).transform.position));
		}
	}

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)payload.entity != (Object)null)
		{
			base.Senses.TrySetTarget(payload.entity);
			base.Senses.ForgetAllNoises();
		}
		if (!base.Senses.FindTargetLKP(out var lkp, applyHeightOffset: true))
		{
			return EFSMStateStatus.Failure;
		}
		NavVector3 navVector = base.Agent.WorldToNavSpace(lkp);
		NavVector3 nextPosition = base.Agent.nextPosition;
		PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
		try
		{
			bool flag = Eqs.SampleNavigablePositions(base.Agent, nextPosition, (List<NavVector3>)(object)val, searchRadius, searchRadius * 0.5f, 16);
			float num = NavVector3.Distance(navVector, nextPosition);
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				RustNavMeshAgent rustNavMeshAgent = default(RustNavMeshAgent);
				foreach (NavVector3 item2 in (List<NavVector3>)(object)val)
				{
					float num2 = 0f;
					if (num < 20f)
					{
						num2 += Mathx.RemapValClamped(NavVector3.Distance(item2, navVector), 0f, searchRadius, 0f, 1f);
					}
					else if (num > 50f)
					{
						num2 += Mathx.RemapValClamped(NavVector3.Distance(item2, navVector), 0f, searchRadius, 0f, 1f);
					}
					else if (lastChosenHidingSpotNS.HasValue)
					{
						num2 += Mathx.RemapValClamped(NavVector3.Distance(item2, lastChosenHidingSpotNS.Value), 0f, searchRadius, 0f, 1f);
					}
					if (((Component)Owner).TryGetComponent<RustNavMeshAgent>(ref rustNavMeshAgent) && rustNavMeshAgent.FindClosestEdge(item2, out var hitNS) && NavVector3.Distance(hitNS.position, item2) < 1.5f)
					{
						num2 += 2f;
					}
					((List<(NavVector3, float)>)(object)pooledScoreList).Add((item2, num2));
				}
				pooledScoreList.SortByScoreDesc(Owner);
				foreach (var item3 in (List<(NavVector3, float)>)(object)pooledScoreList)
				{
					NavVector3 item = item3.Item1;
					NavVector3 navVector2 = item;
					if (!flag)
					{
						if (!base.Agent.SamplePosition(item, out var hitNS2, 3.5f))
						{
							continue;
						}
						navVector2 = hitNS2.position;
					}
					Vector3 val2 = base.Agent.NavToWorldSpace(navVector2);
					if (NpcZoneComponent.IsPointInsideZone(val2) && !base.Agent.IsInWater(val2) && !base.Senses.CanBeSeenAtFrom(val2 + 1.1f * Vector3.up, lkp, "navigation") && base.Agent.CalculatePath(navVector2, Path) && (int)Path.status == 0)
					{
						float pathLength = Path.GetPathLength();
						if (!(pathLength < 0.5f) && !(pathLength > searchRadius * 3f) && base.Agent.SetDestinationWithParams(navVector2, autoBraking: true, speed))
						{
							remainingWalkBeforeSprintTime = walkDurationBeforeSprint;
							lastChosenHidingSpotNS = navVector2;
							return base.OnStateEnter(payload);
						}
					}
				}
				if ((Object)(object)InfoZone == (Object)null)
				{
					return EFSMStateStatus.Failure;
				}
				AICoverPoint bestCoverPoint = InfoZone.GetBestCoverPoint(((Component)Owner).transform.position, lkp, 0f, searchRadius, Owner);
				if ((Object)(object)bestCoverPoint != (Object)null && NpcZoneComponent.IsPointInsideZone(((Component)bestCoverPoint).transform.position))
				{
					NavVector3 targetPositionNS = base.Agent.WorldToNavSpace(((Component)bestCoverPoint).transform.position);
					if (base.Agent.SetDestinationWithParams(targetPositionNS, autoBraking: true, speed))
					{
						heldCover = bestCoverPoint;
						heldCover.SetUsedBy(Owner);
						remainingWalkBeforeSprintTime = walkDurationBeforeSprint;
						return base.OnStateEnter(payload);
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

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if (!base.Agent.hasPath)
		{
			return EFSMStateStatus.Success;
		}
		if (remainingWalkBeforeSprintTime.HasValue)
		{
			remainingWalkBeforeSprintTime -= deltaTime;
			if (remainingWalkBeforeSprintTime <= 0.0)
			{
				base.Agent.SetGait(RustNavMeshAgent.Speeds.Sprint);
				remainingWalkBeforeSprintTime = null;
			}
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		if ((Object)(object)heldCover != (Object)null)
		{
			heldCover.ClearIfUsedBy(Owner);
			heldCover = null;
		}
		remainingWalkBeforeSprintTime = null;
		base.Agent.ResetPath();
		base.OnStateExit();
	}
}
