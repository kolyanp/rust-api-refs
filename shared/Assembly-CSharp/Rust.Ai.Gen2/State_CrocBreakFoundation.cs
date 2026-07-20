using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_CrocBreakFoundation : State_AttackWithTracking
{
	public const float attackRange = 3f;

	private static bool FindBuildingBlockNearby(RustNavMeshAgent agent, Vector3 position, out BuildingBlock buildingBlock)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (agent.SampleGroundPositionWithPhysics(position, out var hitInfoNS, 2f, BasePlayer.GetRadius(), 2097152) && RaycastHitEx.GetEntity(hitInfoNS) is BuildingBlock buildingBlock2)
		{
			buildingBlock = buildingBlock2;
			return true;
		}
		PooledList<BuildingBlock> val = Pool.Get<PooledList<BuildingBlock>>();
		try
		{
			Vis.Entities(position, 4f, (List<BuildingBlock>)(object)val, 2097152, (QueryTriggerInteraction)2);
			if (((List<BuildingBlock>)(object)val).Count > 0)
			{
				buildingBlock = ((List<BuildingBlock>)(object)val)[0];
				return true;
			}
			buildingBlock = null;
			return false;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static BuildingBlock FindNearestTwigFoundationOnTargetBuilding(RustNavMeshAgent agent, BasePlayer targetPlayer, float? maxDistance = null)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (!FindBuildingBlockNearby(agent, ((Component)targetPlayer).transform.position, out var buildingBlock))
		{
			return null;
		}
		BuildingManager.Building building = BuildingManager.server.GetBuilding(buildingBlock.buildingID);
		BuildingBlock result = null;
		float num = float.MaxValue;
		Enumerator<BuildingBlock> enumerator = building.buildingBlocks.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BuildingBlock current = enumerator.Current;
				if (current.grade == BuildingGrade.Enum.Twigs && (Object)(object)current.parentEntity.Get(serverside: true) == (Object)null && (current.ShortPrefabName == "foundation" || current.ShortPrefabName == "foundation.triangle"))
				{
					float num2 = current.Distance(((Component)agent).transform.position);
					if ((!maxDistance.HasValue || !(num2 > maxDistance.Value)) && num2 < num)
					{
						result = current;
						num = num2;
					}
				}
			}
			return result;
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	protected override void DoDamage()
	{
		if (base.Senses.FindTarget(out var target) && target.ToNonNpcPlayer(out var player))
		{
			BuildingBlock buildingBlock = FindNearestTwigFoundationOnTargetBuilding(base.Agent, player);
			if ((Object)(object)buildingBlock == (Object)null)
			{
				base.DoDamage();
			}
			else
			{
				buildingBlock.Kill(BaseNetworkable.DestroyMode.Gib);
			}
		}
	}
}
