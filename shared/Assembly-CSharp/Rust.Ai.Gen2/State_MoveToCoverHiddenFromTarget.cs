using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;
using UnityEngine.AI;

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

	private Vector3? lastChosenHidingSpotNS;

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
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)payload.entity != (Object)null)
		{
			base.Senses.TrySetTarget(payload.entity);
			base.Senses.ForgetAllNoises();
		}
		if (!base.Senses.FindTargetLKP(out var lkp, applyHeightOffset: true))
		{
			return EFSMStateStatus.Failure;
		}
		Matrix4x4 worldToNavMeshSpace = Owner.WorldToNavMeshSpace;
		Vector3 val = ((Matrix4x4)(ref worldToNavMeshSpace)).MultiplyPoint(lkp);
		Vector3 serverNavMeshPos = Owner.ServerNavMeshPos;
		PooledList<Vector3> val2 = Pool.Get<PooledList<Vector3>>();
		try
		{
			Eqs.SamplePositionsInMultiDonutShape(serverNavMeshPos, (List<Vector3>)(object)val2, searchRadius * 0.5f, searchRadius, 2);
			float num = Vector3.Distance(val, serverNavMeshPos);
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				RustNavMeshAgent rustNavMeshAgent = default(RustNavMeshAgent);
				foreach (Vector3 item2 in (List<Vector3>)(object)val2)
				{
					float num2 = 0f;
					if (num < 20f)
					{
						num2 += Mathx.RemapValClamped(Vector3.Distance(item2, val), 0f, searchRadius, 0f, 1f);
					}
					else if (num > 50f)
					{
						num2 += Mathx.RemapValClamped(Vector3.Distance(item2, val), 0f, searchRadius, 0f, 1f);
					}
					else if (lastChosenHidingSpotNS.HasValue)
					{
						num2 += Mathx.RemapValClamped(Vector3.Distance(item2, lastChosenHidingSpotNS.Value), 0f, searchRadius, 0f, 1f);
					}
					if (((Component)Owner).TryGetComponent<RustNavMeshAgent>(ref rustNavMeshAgent) && rustNavMeshAgent.FindClosestEdge(out var hitNS) && Vector3.Distance(((NavMeshHit)(ref hitNS)).position, Owner.ServerNavMeshPos) < 1.5f)
					{
						num2 += 2f;
					}
					((List<(Vector3, float)>)(object)pooledScoreList).Add((item2, num2));
				}
				pooledScoreList.SortByScoreDesc(Owner);
				Matrix4x4 navMeshToWorldSpace = Owner.NavMeshToWorldSpace;
				foreach (var item3 in (List<(Vector3, float)>)(object)pooledScoreList)
				{
					Vector3 item = item3.Item1;
					if (!base.Agent.SamplePosition(item, out var hitNS2, 3.5f))
					{
						continue;
					}
					Vector3 val3 = ((Matrix4x4)(ref navMeshToWorldSpace)).MultiplyPoint(((NavMeshHit)(ref hitNS2)).position);
					if (NpcZoneComponent.IsPointInsideZone(val3) && !base.Agent.IsInWater(val3) && !base.Senses.CanBeSeenAtFrom(val3 + 1.1f * Vector3.up, lkp, "navigation") && base.Agent.CalculatePath(((NavMeshHit)(ref hitNS2)).position, Path) && (int)Path.status == 0)
					{
						float pathLength = Path.GetPathLength();
						if (!(pathLength < 0.5f) && !(pathLength > searchRadius * 3f) && base.Agent.SetDestinationWithParams(((NavMeshHit)(ref hitNS2)).position, autoBraking: true, speed))
						{
							remainingWalkBeforeSprintTime = walkDurationBeforeSprint;
							lastChosenHidingSpotNS = ((NavMeshHit)(ref hitNS2)).position;
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
					Vector3 targetPositionNS = ((Matrix4x4)(ref worldToNavMeshSpace)).MultiplyPoint(((Component)bestCoverPoint).transform.position);
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
			((IDisposable)val2)?.Dispose();
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
