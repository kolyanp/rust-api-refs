using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Prefabs.Misc;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_IsNavmeshReady : FSMTransitionBase
{
	private readonly int humanoid = BaseNavigator.GetNavMeshAgentID("Humanoid");

	private readonly int animal = BaseNavigator.GetNavMeshAgentID("Animal");

	private MonumentNavMesh cachedMonumentNavMesh;

	public override void OnStateEnter()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		base.OnStateEnter();
		if (!AI.useUnityNavmesh || base.Agent.agentTypeID != humanoid)
		{
			return;
		}
		if (DeepSeaManager.IsInsideDeepSea(((Component)Owner).transform.position))
		{
			if (!BaseNetworkableEx.Is<GhostShip>((Object)(object)Owner.GetParentEntity(), out GhostShip _))
			{
				PooledList<DeepSeaIsland> val = Pool.Get<PooledList<DeepSeaIsland>>();
				try
				{
					Vis.Entities(((Component)Owner).transform.position, 10f, (List<DeepSeaIsland>)(object)val, 8454145, (QueryTriggerInteraction)2);
					DeepSeaIsland deepSeaIsland = ((((List<DeepSeaIsland>)(object)val).Count > 0) ? ((List<DeepSeaIsland>)(object)val)[0] : null);
					cachedMonumentNavMesh = (((Object)(object)deepSeaIsland != (Object)null) ? deepSeaIsland.monumentNavMesh : null);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
		else
		{
			if ((Object)(object)TerrainMeta.TopologyMap == (Object)null || !TerrainMeta.TopologyMap.GetTopology(((Component)Owner).transform.position, 1024) || (Object)(object)TerrainMeta.Path == (Object)null)
			{
				return;
			}
			foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
			{
				if (monument.HasNavmesh && BaseNetworkableEx.Is<MonumentNavMesh>((Object)(object)monument.GetMonumentNavMesh(), out MonumentNavMesh castedUnityObject2) && monument.IsInBounds(((Component)Owner).transform.position))
				{
					cachedMonumentNavMesh = castedUnityObject2;
					break;
				}
			}
		}
	}

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_IsNavmeshReady"))
		{
			if (!AI.move)
			{
				return false;
			}
			if (AI.useUnityNavmesh)
			{
				if (base.Agent.agentTypeID == animal)
				{
					if ((Object)(object)SingletonComponent<DynamicNavMesh>.Instance == (Object)null || SingletonComponent<DynamicNavMesh>.Instance.IsBuilding)
					{
						return false;
					}
				}
				else if (base.Agent.agentTypeID == humanoid)
				{
					if ((Object)(object)cachedMonumentNavMesh != (Object)null && cachedMonumentNavMesh.IsBuilding)
					{
						return false;
					}
					if (BaseNetworkableEx.Is<GhostShip>((Object)(object)Owner.GetParentEntity(), out GhostShip _))
					{
						return true;
					}
					if (!DungeonNavmesh.NavReady())
					{
						return false;
					}
				}
			}
			NavMeshHit hitNS;
			return base.Agent.SamplePosition(((Component)Owner).transform.position, out hitNS, 0.5f);
		}
	}
}
