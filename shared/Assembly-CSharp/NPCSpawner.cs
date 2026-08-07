using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

public class NPCSpawner : SpawnGroup
{
	public int AdditionalLOSBlockingLayer;

	public MonumentNavMesh monumentNavMesh;

	public bool shouldFillOnSpawn;

	[Header("InfoZone Config")]
	public AIInformationZone VirtualInfoZone;

	[Header("Navigator Config")]
	public AIMovePointPath Path;

	public BasePath AStarGraph;

	[Header("Human Stat Replacements")]
	public bool UseStatModifiers;

	public float SenseRange = 30f;

	public bool CheckLOS = true;

	public float TargetLostRange = 50f;

	public float AttackRangeMultiplier = 1f;

	public float ListenRange = 10f;

	public float CanUseHealingItemsChance;

	[Header("Loadout Replacements")]
	public PlayerInventoryProperties[] Loadouts;

	[Header("Parenting")]
	public BaseEntity attachToParent;

	public override void SpawnInitial()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (!AI.npc_spawn_on_cargo_ship && GameObjectEx.ToBaseEntity(((Component)this).transform.root) is CargoShip)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		if (!AI.npc_spawn_on_junkpile && GameObjectEx.ToBaseEntity(((Component)this).transform.root) is JunkPile)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		if (DeepSeaManager.IsInsideDeepSea(((Component)this).transform.position))
		{
			List<DeepSeaIsland> list = Pool.Get<List<DeepSeaIsland>>();
			Vis.Entities(((Component)this).transform.position, 10f, list, 8454145, (QueryTriggerInteraction)2);
			DeepSeaIsland deepSeaIsland = ((list.Count > 0) ? list[0] : null);
			Pool.FreeUnmanaged<DeepSeaIsland>(ref list);
			if ((Object)(object)deepSeaIsland != (Object)null)
			{
				monumentNavMesh = deepSeaIsland.monumentNavMesh;
				if (!AI.npc_spawn_on_deep_sea_islands)
				{
					((Behaviour)this).enabled = false;
					return;
				}
			}
		}
		fillOnSpawn = shouldFillOnSpawn;
		if (WaitingForNavMesh())
		{
			Invoke(LateSpawn, 10f);
		}
		else
		{
			base.SpawnInitial();
		}
	}

	public bool WaitingForNavMesh()
	{
		if ((Object)(object)monumentNavMesh != (Object)null)
		{
			return monumentNavMesh.IsBuilding;
		}
		if (!AI.useUnityNavmesh && !RustNavigation.Instance.IsDefaultNavmeshBuilt())
		{
			return true;
		}
		if (!DungeonNavmesh.NavReady())
		{
			return true;
		}
		return !AI.move;
	}

	public void LateSpawn()
	{
		if (!WaitingForNavMesh())
		{
			SpawnInitial();
			if (AI.logIssues)
			{
				string recursiveName = TransformEx.GetRecursiveName(((Component)this).transform);
				Debug.Log((object)("SpawnGroup spawning: \"" + recursiveName + "\""));
			}
		}
		else
		{
			Invoke(LateSpawn, 5f);
		}
	}

	protected override void PostSpawnProcess(BaseEntity entity, BaseSpawnPoint spawnPoint)
	{
		base.PostSpawnProcess(entity, spawnPoint);
		BaseNavigator component = ((Component)entity).GetComponent<BaseNavigator>();
		if (AdditionalLOSBlockingLayer != 0 && (Object)(object)entity != (Object)null && entity is HumanNPC humanNPC)
		{
			humanNPC.AdditionalLosBlockingLayer = AdditionalLOSBlockingLayer;
		}
		HumanNPC humanNPC2 = entity as HumanNPC;
		if ((Object)(object)humanNPC2 != (Object)null)
		{
			if (Loadouts != null && Loadouts.Length != 0)
			{
				humanNPC2.EquipLoadout(Loadouts);
			}
			ModifyHumanBrainStats(humanNPC2.Brain);
		}
		if ((Object)(object)VirtualInfoZone != (Object)null)
		{
			if (VirtualInfoZone.Virtual)
			{
				NPCPlayer nPCPlayer = entity as NPCPlayer;
				if ((Object)(object)nPCPlayer != (Object)null)
				{
					nPCPlayer.VirtualInfoZone = VirtualInfoZone;
					if ((Object)(object)humanNPC2 != (Object)null)
					{
						humanNPC2.VirtualInfoZone.RegisterSleepableEntity(humanNPC2.Brain);
					}
				}
			}
			else
			{
				Debug.LogError((object)"NPCSpawner trying to set a virtual info zone without the Virtual property!");
			}
		}
		if ((Object)(object)component != (Object)null)
		{
			component.Path = Path;
			component.AStarGraph = AStarGraph;
		}
		if (Object.op_Implicit((Object)(object)attachToParent))
		{
			entity.SetParent(attachToParent, worldPositionStays: true);
		}
	}

	private void ModifyHumanBrainStats(BaseAIBrain brain)
	{
		if (UseStatModifiers && !((Object)(object)brain == (Object)null))
		{
			brain.SenseRange = SenseRange;
			brain.TargetLostRange *= TargetLostRange;
			brain.AttackRangeMultiplier = AttackRangeMultiplier;
			brain.ListenRange = ListenRange;
			brain.CheckLOS = CheckLOS;
			if (CanUseHealingItemsChance > 0f)
			{
				brain.CanUseHealingItems = Random.Range(0f, 1f) <= CanUseHealingItemsChance;
			}
		}
	}
}
