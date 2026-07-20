using System;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Flank : FSMStateBase
{
	public RustNavMeshAgent.Speeds speed;

	private NPCHumanoidAnimController _clientAnim;

	private NpcBarkComponent _barkComponent;

	private RustNavMeshPath _pathToLkp;

	private RustNavMeshPath _pathToFlank;

	private RustNavMeshPath _pathFromFlankToEnemy;

	private bool isOnSecondPath;

	private NPCHumanoidAnimController ClientAnim => _clientAnim ?? (_clientAnim = ((Component)Owner).GetComponentInChildren<NPCHumanoidAnimController>());

	private NpcBarkComponent BarkComponent => _barkComponent ?? (_barkComponent = ((Component)Owner).GetComponent<NpcBarkComponent>());

	private RustNavMeshPath PathToLkp => _pathToLkp ?? (_pathToLkp = new RustNavMeshPath());

	private RustNavMeshPath PathToFlank => _pathToFlank ?? (_pathToFlank = new RustNavMeshPath());

	private RustNavMeshPath PathFromFlankToEnemy => _pathFromFlankToEnemy ?? (_pathFromFlankToEnemy = new RustNavMeshPath());

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTargetLKP(out var lkp, applyHeightOffset: false, predict: true))
		{
			return EFSMStateStatus.Failure;
		}
		Matrix4x4 worldToNavMeshSpace = Owner.WorldToNavMeshSpace;
		Vector3 positionNS = ((Matrix4x4)(ref worldToNavMeshSpace)).MultiplyPoint(lkp);
		if (!base.Agent.SamplePosition(positionNS, out var hitNS, 3.5f))
		{
			return EFSMStateStatus.Failure;
		}
		if (!base.Agent.CalculatePath(((NavMeshHit)(ref hitNS)).position, PathToLkp) && (int)PathToLkp.status != 0)
		{
			return EFSMStateStatus.Failure;
		}
		if (!NPCFlankSpot.Find(base.Agent, ((NavMeshHit)(ref hitNS)).position, PathToLkp, PathToFlank, PathFromFlankToEnemy))
		{
			return EFSMStateStatus.Failure;
		}
		if (!base.Agent.SetPath(PathToFlank))
		{
			return EFSMStateStatus.Failure;
		}
		if (speed == RustNavMeshAgent.Speeds.Sneak)
		{
			ClientAnim.IsCrouching = true;
		}
		base.Agent.speed = base.Agent.GetSpeedForGait(speed);
		isOnSecondPath = false;
		BarkComponent.PlayVoicelineFromCategory(ENPCVoicelineCategory.Flank);
		return base.OnStateEnter(payload);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if (!base.Agent.hasPath)
		{
			if (isOnSecondPath)
			{
				return EFSMStateStatus.Success;
			}
			isOnSecondPath = true;
			if (!base.Senses.FindTarget(out var _))
			{
				return EFSMStateStatus.Failure;
			}
			if (!base.Agent.SetPath(PathFromFlankToEnemy))
			{
				return EFSMStateStatus.Failure;
			}
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		ClientAnim.IsCrouching = false;
		base.Agent.ResetPath();
		base.OnStateExit();
	}

	public static float ComputePathsInitialSimilarity(RustNavMeshPath pathA, RustNavMeshPath pathB)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ComputePathDistance"))
		{
			int num = Mathf.Min(pathA.corners.Count, pathB.corners.Count);
			float num2 = 0f;
			for (int i = 0; i + 1 < num && !(pathA.corners[i] != pathB.corners[i]) && !(pathA.corners[i + 1] != pathB.corners[i + 1]); i++)
			{
				num2 += Vector3.Distance(pathA.corners[i], pathA.corners[i + 1]);
			}
			return num2 / Mathf.Min(pathA.GetPathLength(), pathB.GetPathLength());
		}
	}
}
