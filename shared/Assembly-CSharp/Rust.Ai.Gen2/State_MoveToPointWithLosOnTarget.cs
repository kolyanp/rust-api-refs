using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_MoveToPointWithLosOnTarget : FSMStateBase
{
	public float searchRadius = 8f;

	private static RustNavMeshPath _path;

	private NpcZoneComponent _npcZoneComponent;

	private NpcShootingComponent _shooting;

	private Vector3? lastChosenPeekNS;

	private static RustNavMeshPath Path => _path ?? (_path = new RustNavMeshPath());

	private NpcZoneComponent NpcZoneComponent => _npcZoneComponent ?? (_npcZoneComponent = ((Component)Owner).GetComponent<NpcZoneComponent>());

	private NpcShootingComponent Shooting => _shooting ?? (_shooting = ((Component)Owner).GetComponent<NpcShootingComponent>());

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTarget(out var target))
		{
			return EFSMStateStatus.Failure;
		}
		if (!base.Senses.FindLKP(target, out var lkp, applyHeightOffset: true))
		{
			return EFSMStateStatus.Failure;
		}
		bool flag = NpcZoneComponent.IsPointInsideZone(lkp) || (base.Senses.GetVisibilityStatus(target, out var status) && status.timeNotVisible < 30f);
		PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
		try
		{
			Vector3 serverNavMeshPos = Owner.ServerNavMeshPos;
			if (flag)
			{
				Eqs.SamplePositionsInMultiDonutShape(serverNavMeshPos, (List<Vector3>)(object)val, searchRadius * 0.5f, searchRadius, 2);
			}
			else
			{
				Eqs.SamplePositionsInMultiDonutShape(serverNavMeshPos, (List<Vector3>)(object)val, searchRadius, searchRadius, 1, 4);
			}
			Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
			try
			{
				foreach (Vector3 item2 in (List<Vector3>)(object)val)
				{
					float num = 0f;
					if (lastChosenPeekNS.HasValue)
					{
						num += Mathx.RemapValClamped(Vector3.Distance(item2, lastChosenPeekNS.Value), 0f, searchRadius, 0f, 1f);
					}
					((List<(Vector3, float)>)(object)pooledScoreList).Add((item2, num));
				}
				pooledScoreList.SortByScoreDesc(Owner);
				_ = Owner.WorldToNavMeshSpace;
				Matrix4x4 navMeshToWorldSpace = Owner.NavMeshToWorldSpace;
				foreach (var item3 in (List<(Vector3, float)>)(object)pooledScoreList)
				{
					Vector3 item = item3.Item1;
					if (base.Agent.SamplePosition(item, out var hitNS, 3.5f))
					{
						Vector3 val2 = ((Matrix4x4)(ref navMeshToWorldSpace)).MultiplyPoint(((NavMeshHit)(ref hitNS)).position);
						if (NpcZoneComponent.IsPointInsideZone(val2) && (!lastChosenPeekNS.HasValue || !(Vector3.Distance(((NavMeshHit)(ref hitNS)).position, lastChosenPeekNS.Value) < 2f)) && !base.Agent.IsInWater(val2) && Shooting.CanShootFromAt(val2 + base.Senses.EyeOffset, lkp, "navigation") && base.Agent.CalculatePath(((NavMeshHit)(ref hitNS)).position, Path) && (int)Path.status == 0 && !(Path.GetPathLength() > searchRadius * 3f) && base.Agent.SetDestinationWithParams(((NavMeshHit)(ref hitNS)).position, autoBraking: true, RustNavMeshAgent.Speeds.Walk))
						{
							lastChosenPeekNS = ((NavMeshHit)(ref hitNS)).position;
							return base.OnStateEnter(payload);
						}
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
