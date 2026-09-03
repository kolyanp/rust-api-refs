using System;
using System.Collections.Generic;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_MoveToPointWithLosOnTarget : FSMStateBase
{
	public float searchRadius = 8f;

	private static RustNavMeshPath _path;

	private NpcZoneComponent _npcZoneComponent;

	private NpcShootingComponent _shooting;

	private NavVector3? lastChosenPeekNS;

	private static RustNavMeshPath Path => _path ?? (_path = new RustNavMeshPath());

	private NpcZoneComponent NpcZoneComponent => _npcZoneComponent ?? (_npcZoneComponent = ((Component)Owner).GetComponent<NpcZoneComponent>());

	private NpcShootingComponent Shooting => _shooting ?? (_shooting = ((Component)Owner).GetComponent<NpcShootingComponent>());

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTarget(out var target))
		{
			return EFSMStateStatus.Failure;
		}
		if (!base.Senses.FindLKP(target, out var lkp, applyHeightOffset: true))
		{
			return EFSMStateStatus.Failure;
		}
		bool flag = NpcZoneComponent.IsPointInsideZone(lkp) || (base.Senses.GetVisibilityStatus(target, out var status) && status.timeNotVisible < 30f);
		PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
		try
		{
			NavVector3 nextPosition = base.Agent.nextPosition;
			bool flag2 = ((!flag) ? Eqs.SampleNavigablePositions(base.Agent, nextPosition, (List<NavVector3>)(object)val, searchRadius, searchRadius, 4) : Eqs.SampleNavigablePositions(base.Agent, nextPosition, (List<NavVector3>)(object)val, searchRadius, searchRadius * 0.5f, 16));
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				foreach (NavVector3 item2 in (List<NavVector3>)(object)val)
				{
					float num = 0f;
					if (lastChosenPeekNS.HasValue)
					{
						num += Mathx.RemapValClamped(NavVector3.Distance(item2, lastChosenPeekNS.Value), 0f, searchRadius, 0f, 1f);
					}
					((List<(NavVector3, float)>)(object)pooledScoreList).Add((item2, num));
				}
				pooledScoreList.SortByScoreDesc(Owner);
				foreach (var item3 in (List<(NavVector3, float)>)(object)pooledScoreList)
				{
					NavVector3 item = item3.Item1;
					NavVector3 navVector = item;
					if (!flag2)
					{
						if (!base.Agent.SamplePosition(item, out var hitNS, 3.5f))
						{
							continue;
						}
						navVector = hitNS.position;
					}
					Vector3 val2 = base.Agent.NavToWorldSpace(navVector);
					if (NpcZoneComponent.IsPointInsideZone(val2) && (!lastChosenPeekNS.HasValue || !(NavVector3.Distance(navVector, lastChosenPeekNS.Value) < 2f)) && !base.Agent.IsInWater(val2) && Shooting.CanShootFromAt(val2 + base.Senses.EyeOffset, lkp, "navigation") && base.Agent.CalculatePath(navVector, Path) && (int)Path.status == 0 && !(Path.GetPathLength() > searchRadius * 3f) && base.Agent.SetDestinationWithParams(navVector, autoBraking: true, RustNavMeshAgent.Speeds.Walk))
					{
						lastChosenPeekNS = navVector;
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
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		base.Agent.ResetPath();
		base.OnStateExit();
	}
}
