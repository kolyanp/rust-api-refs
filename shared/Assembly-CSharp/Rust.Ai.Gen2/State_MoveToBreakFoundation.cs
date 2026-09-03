using System;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_MoveToBreakFoundation : FSMStateBase
{
	private const float maxHorizontalDist = 10f;

	private bool FindReachableLocation(out Vector3 location)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		location = default(Vector3);
		if (!base.Senses.FindTarget(out var target) || !target.ToNonNpcPlayer(out var player))
		{
			return false;
		}
		Vector3 position = ((Component)target).transform.position;
		if (Vector3.Distance(((Component)Owner).transform.position, position) > 50f)
		{
			return false;
		}
		if (BaseNetworkableEx.Is<BuildingBlock>((Object)(object)State_CrocBreakFoundation.FindNearestTwigFoundationOnTargetBuilding(base.Agent, player), out BuildingBlock castedUnityObject) && base.Agent.SamplePosition(castedUnityObject.ClosestPoint(((Component)Owner).transform.position), out var hitWS, 10f))
		{
			location = ((NavMeshHit)(ref hitWS)).position;
			return true;
		}
		if (base.Agent.SamplePosition(position, out var hitWS2, 3f))
		{
			location = ((NavMeshHit)(ref hitWS2)).position;
			return true;
		}
		return false;
	}

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTarget(out var target) || !target.ToNonNpcPlayer(out var _))
		{
			return EFSMStateStatus.Failure;
		}
		if (!FindReachableLocation(out var location))
		{
			return EFSMStateStatus.Failure;
		}
		Vector3 val = location + ((Bounds)(ref Owner.bounds)).extents.y * Vector3.up;
		Vector3 val2 = target.CenterPoint() - val;
		if (GamePhysics.Trace(new Ray(val, val2), 0f, out var _, ((Vector3)(ref val2)).magnitude, 1503731969, (QueryTriggerInteraction)0))
		{
			return EFSMStateStatus.Failure;
		}
		if (!base.Agent.SetDestinationWithParams(location, autoBraking: true, RustNavMeshAgent.Speeds.Run))
		{
			return EFSMStateStatus.Failure;
		}
		return base.OnStateEnter(payload);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if (!base.Agent.hasPath)
		{
			return EFSMStateStatus.Success;
		}
		return base.OnStateUpdate(deltaTime);
	}
}
