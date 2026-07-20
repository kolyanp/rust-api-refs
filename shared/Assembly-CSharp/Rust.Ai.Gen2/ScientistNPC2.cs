using ConVar;
using Prefabs.Misc;
using UnityEngine;

namespace Rust.Ai.Gen2;

[SoftRequireComponent(typeof(Scientist2FSM))]
public class ScientistNPC2 : BaseNPC2
{
	public bool canBeHeadshot = true;

	public override bool IsAnimal => false;

	public override Vector3 ServerNavMeshPos
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			Vector3 serverWorldPosition = ServerWorldPosition;
			Matrix4x4 worldToNavMeshSpace = WorldToNavMeshSpace;
			return ((Matrix4x4)(ref worldToNavMeshSpace)).MultiplyPoint(serverWorldPosition);
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			Matrix4x4 navMeshToWorldSpace = NavMeshToWorldSpace;
			ServerWorldPosition = ((Matrix4x4)(ref navMeshToWorldSpace)).MultiplyPoint(value);
		}
	}

	public override Matrix4x4 WorldToNavMeshSpace
	{
		get
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			RustNavMeshAgent rustNavMeshAgent = default(RustNavMeshAgent);
			if (AI.useUnityNavmesh)
			{
				GhostShip ghostShip = GetParentEntity() as GhostShip;
				if (Object.op_Implicit((Object)(object)ghostShip))
				{
					return ghostShip.WorldToNavMeshSpace;
				}
			}
			else if (((Component)this).TryGetComponent<RustNavMeshAgent>(ref rustNavMeshAgent) && rustNavMeshAgent.HasValidIndependantNavmesh)
			{
				return rustNavMeshAgent.independantNavmesh.WorldToNavMatrix;
			}
			return base.WorldToNavMeshSpace;
		}
	}

	public override Matrix4x4 NavMeshToWorldSpace
	{
		get
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			RustNavMeshAgent rustNavMeshAgent = default(RustNavMeshAgent);
			if (AI.useUnityNavmesh)
			{
				GhostShip ghostShip = GetParentEntity() as GhostShip;
				if (Object.op_Implicit((Object)(object)ghostShip))
				{
					return ghostShip.NavMeshToWorldSpace;
				}
			}
			else if (((Component)this).TryGetComponent<RustNavMeshAgent>(ref rustNavMeshAgent) && rustNavMeshAgent.HasValidIndependantNavmesh)
			{
				return rustNavMeshAgent.independantNavmesh.NavToWorldMatrix;
			}
			return base.NavMeshToWorldSpace;
		}
	}

	public override string Categorize()
	{
		return "Scientist2";
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
	}

	public override void ScaleDamage(HitInfo info)
	{
		base.ScaleDamage(info);
		if (canBeHeadshot && Object.op_Implicit((Object)(object)info.damageProperties))
		{
			info.damageProperties.ScaleDamage(info);
		}
	}
}
