using ConVar;
using Rust.Ai.Gen2;
using UnityEngine;

public class NPCNavigator : BaseNavigator
{
	public int DestroyOnFailedSampleCount = 5;

	private int sampleFailCount;

	public BaseNpc NPC { get; private set; }

	public override void Init(BaseCombatEntity entity, RustNavMeshAgent agent)
	{
		base.Init(entity, agent);
		NPC = entity as BaseNpc;
		sampleFailCount = 0;
	}

	public override void OnFailedToPlaceOnNavmesh()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		base.OnFailedToPlaceOnNavmesh();
		if ((Object)(object)SingletonComponent<DynamicNavMesh>.Instance == (Object)null || SingletonComponent<DynamicNavMesh>.Instance.IsBuilding)
		{
			return;
		}
		sampleFailCount++;
		if (DestroyOnFailedSampleCount > 0 && sampleFailCount >= DestroyOnFailedSampleCount)
		{
			Debug.LogWarning((object)("Failed to sample navmesh " + sampleFailCount + " times in a row at: " + ((object)((Component)this).transform.position/*cast due to constrained. prefix*/).ToString() + ". Destroying: " + ((Object)((Component)this).gameObject).name));
			if ((Object)(object)NPC != (Object)null && !NPC.IsDestroyed)
			{
				NPC.Kill();
			}
		}
	}

	public override void OnPlacedOnNavmesh()
	{
		base.OnPlacedOnNavmesh();
		sampleFailCount = 0;
	}

	protected override bool CanEnableNavMeshNavigation()
	{
		if (!base.CanEnableNavMeshNavigation())
		{
			return false;
		}
		return true;
	}

	protected override bool CanUpdateMovement()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (!base.CanUpdateMovement())
		{
			return false;
		}
		if ((Object)(object)NPC != (Object)null && (NPC.IsDormant || !NPC.syncPosition) && ((Behaviour)base.Agent).enabled)
		{
			SetDestination(NPC.ServerPosition);
			return false;
		}
		return true;
	}

	protected override void UpdatePositionAndRotation(Vector3 moveToPosition, float delta)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		base.UpdatePositionAndRotation(moveToPosition, delta);
		UpdateRotation(moveToPosition, delta);
	}

	private void UpdateRotation(Vector3 moveToPosition, float delta)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (overrideFacingDirectionMode != OverrideFacingDirectionMode.None)
		{
			return;
		}
		if (traversingNavMeshLink)
		{
			Vector3 val = base.Agent.destination - base.BaseEntity.ServerPosition;
			if (((Vector3)(ref val)).sqrMagnitude > 1f)
			{
				val = currentNavMeshLinkEndPos - base.BaseEntity.ServerPosition;
			}
			_ = ((Vector3)(ref val)).sqrMagnitude;
			_ = 0.001f;
			return;
		}
		Vector3 val2 = base.Agent.destination - base.BaseEntity.ServerPosition;
		if (((Vector3)(ref val2)).sqrMagnitude > 1f)
		{
			val2 = base.Agent.desiredVelocity;
			Vector3 normalized = ((Vector3)(ref val2)).normalized;
			if (((Vector3)(ref normalized)).sqrMagnitude > 0.001f)
			{
				base.BaseEntity.ServerRotation = Quaternion.LookRotation(normalized);
			}
		}
	}

	public override void ApplyFacingDirectionOverride()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.ApplyFacingDirectionOverride();
		base.BaseEntity.ServerRotation = Quaternion.LookRotation(base.FacingDirectionOverride);
	}

	public override bool IsSwimming()
	{
		if (!AI.npcswimming)
		{
			return false;
		}
		if ((Object)(object)NPC != (Object)null)
		{
			return NPC.swimming;
		}
		return false;
	}
}
