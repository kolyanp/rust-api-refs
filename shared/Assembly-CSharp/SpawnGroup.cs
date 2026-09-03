using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.AI;

public class SpawnGroup : BaseMonoBehaviour, IServerComponent, ISpawnPointUser, ISpawnGroup
{
	[Serializable]
	public class SpawnEntry
	{
		public GameObjectRef prefab;

		public int weight = 1;

		public bool mobile;
	}

	[InspectorFlags]
	public MonumentTier Tier = (MonumentTier)(-1);

	public List<SpawnEntry> prefabs;

	public int maxPopulation = 5;

	public int numToSpawnPerTickMin = 1;

	public int numToSpawnPerTickMax = 2;

	public float respawnDelayMin = 10f;

	public float respawnDelayMax = 20f;

	public bool wantsInitialSpawn = true;

	public bool temporary;

	public bool forceInitialSpawn;

	public bool preventDuplicates;

	public bool isSpawnerActive = true;

	public bool shouldBlockSpawnedEntitySaving = true;

	public SpawnGroupResetBehavior resetBehavior;

	public BoxCollider setFreeIfMovedBeyond;

	public string category;

	[NonSerialized]
	public MonumentInfo Monument;

	public bool fillOnSpawn;

	public BaseSpawnPoint[] spawnPoints;

	public List<SpawnPointInstance> spawnInstances = new List<SpawnPointInstance>();

	public LocalClock spawnClock = new LocalClock();

	public int currentPopulation => spawnInstances.Count;

	public List<SpawnPointInstance> SpawnInstances => spawnInstances;

	public int ObjectsAdded { get; private set; }

	public int ObjectsRemoved { get; private set; }

	public int ObjectsActive => spawnInstances.Count;

	public int SpawnPointCount => spawnPoints.Length;

	protected virtual bool BlockSpawnedEntitySaving => shouldBlockSpawnedEntitySaving;

	protected virtual bool AllowOverlappingSpawns => false;

	public bool DoesGroupContainNPCs()
	{
		foreach (SpawnEntry prefab in prefabs)
		{
			GameObject val = prefab.prefab?.Get();
			if (!((Object)(object)val == (Object)null))
			{
				BaseCombatEntity component = val.GetComponent<BaseCombatEntity>();
				if (!((Object)(object)component == (Object)null) && component.IsNpc)
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool WantsInitialSpawn()
	{
		return wantsInitialSpawn;
	}

	public virtual bool WantsTimedSpawn()
	{
		return respawnDelayMax != float.PositiveInfinity;
	}

	public float GetSpawnDelta()
	{
		return (respawnDelayMax + respawnDelayMin) * 0.5f / SpawnHandler.PlayerScale(ConVar.Spawn.player_scale);
	}

	public float GetSpawnVariance()
	{
		return (respawnDelayMax - respawnDelayMin) * 0.5f / SpawnHandler.PlayerScale(ConVar.Spawn.player_scale);
	}

	protected void Awake()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TerrainMeta.TopologyMap == (Object)null)
		{
			return;
		}
		int topology = TerrainMeta.TopologyMap.GetTopology(((Component)this).transform.position);
		int num = 469762048;
		int num2 = MonumentInfo.TierToMask(Tier);
		if (num2 == num || (num2 & topology) != 0)
		{
			spawnPoints = ((Component)this).GetComponentsInChildren<BaseSpawnPoint>();
			if (WantsTimedSpawn())
			{
				spawnClock.Add(GetSpawnDelta(), GetSpawnVariance(), Spawn);
			}
			if (!temporary && Object.op_Implicit((Object)(object)SingletonComponent<SpawnHandler>.Instance))
			{
				SingletonComponent<SpawnHandler>.Instance.SpawnGroups.Add((ISpawnGroup)this);
			}
			if (forceInitialSpawn)
			{
				Invoke(SpawnInitial, 1f);
			}
			Monument = FindMonument();
		}
	}

	protected void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)SingletonComponent<SpawnHandler>.Instance))
		{
			SingletonComponent<SpawnHandler>.Instance.SpawnGroups.Remove((ISpawnGroup)this);
		}
		else if (AI.logIssues)
		{
			Debug.LogWarning((object)(((object)this).GetType().Name + ": SpawnHandler instance not found."));
		}
	}

	public void Fill()
	{
		if (isSpawnerActive)
		{
			Spawn(maxPopulation);
		}
	}

	public void Clear()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		for (int num = spawnInstances.Count - 1; num >= 0; num--)
		{
			SpawnPointInstance spawnPointInstance = spawnInstances[num];
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((Component)spawnPointInstance).gameObject);
			if ((Object)(object)setFreeIfMovedBeyond != (Object)null)
			{
				Bounds bounds = ((Collider)setFreeIfMovedBeyond).bounds;
				if (!((Bounds)(ref bounds)).Contains(((Component)baseEntity).transform.position))
				{
					spawnPointInstance.Retire();
					continue;
				}
			}
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				baseEntity.Kill();
			}
		}
		spawnInstances.Clear();
	}

	public bool HasSpawnedAny()
	{
		return spawnInstances.Count > 0;
	}

	public bool HasSpawned(uint prefabID)
	{
		foreach (SpawnPointInstance spawnInstance in spawnInstances)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((Component)spawnInstance).gameObject);
			if (Object.op_Implicit((Object)(object)baseEntity) && baseEntity.prefabID == prefabID)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void SpawnInitial()
	{
		if (wantsInitialSpawn && isSpawnerActive)
		{
			if (fillOnSpawn)
			{
				Spawn(maxPopulation);
			}
			else
			{
				Spawn();
			}
		}
	}

	public void SpawnRepeating()
	{
		for (int i = 0; i < spawnClock.events.Count; i++)
		{
			LocalClock.TimedEvent value = spawnClock.events[i];
			if (Time.time > value.time)
			{
				value.delta = GetSpawnDelta();
				value.variance = GetSpawnVariance();
				spawnClock.events[i] = value;
			}
		}
		spawnClock.Tick();
	}

	public void ObjectSpawned(SpawnPointInstance instance)
	{
		spawnInstances.Add(instance);
		ObjectsAdded++;
	}

	public void ObjectRetired(SpawnPointInstance instance)
	{
		spawnInstances.Remove(instance);
		ObjectsRemoved++;
	}

	public void DelayedSpawn()
	{
		Invoke(Spawn, 1f);
	}

	public void Spawn()
	{
		if (isSpawnerActive)
		{
			Spawn(Random.Range(numToSpawnPerTickMin, numToSpawnPerTickMax + 1));
		}
	}

	protected virtual void Spawn(int numToSpawn)
	{
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		if (!AI.scientist_spawners_enabled && prefabs != null)
		{
			foreach (SpawnEntry prefab2 in prefabs)
			{
				if (prefab2?.prefab != null && prefab2.prefab.isValid)
				{
					BaseEntity entity = prefab2.prefab.GetEntity();
					if (entity is ScientistNPC || entity is TunnelDweller || entity is UnderwaterDweller || entity is ScientistNPC2)
					{
						((Behaviour)this).enabled = false;
						return;
					}
				}
			}
		}
		numToSpawn = Mathf.Min(numToSpawn, maxPopulation - currentPopulation);
		for (int i = 0; i < numToSpawn; i++)
		{
			GameObjectRef prefab = GetPrefab();
			if (prefab == null || !prefab.isValid)
			{
				continue;
			}
			BaseSpawnPoint spawnPoint = GetSpawnPoint(prefab, out var pos, out var rot);
			if (!Object.op_Implicit((Object)(object)spawnPoint))
			{
				continue;
			}
			BaseEntity baseEntity = GameManager.server.CreateEntity(prefab.resourcePath, pos, rot, startActive: false);
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				if (baseEntity.enableSaving && BlockSpawnedEntitySaving && !(spawnPoint is SpaceCheckingSpawnPoint))
				{
					baseEntity.enableSaving = false;
				}
				PoolableEx.AwakeFromInstantiate(((Component)baseEntity).gameObject);
				baseEntity.Spawn();
				PostSpawnProcess(baseEntity, spawnPoint);
				SpawnPointInstance spawnPointInstance = ((Component)baseEntity).gameObject.AddComponent<SpawnPointInstance>();
				spawnPointInstance.parentSpawnPointUser = this;
				spawnPointInstance.parentSpawnPoint = spawnPoint;
				spawnPointInstance.Entity = baseEntity;
				spawnPointInstance.Notify();
			}
		}
	}

	protected virtual void PostSpawnProcess(BaseEntity entity, BaseSpawnPoint spawnPoint)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (entity is HumanNPC humanNPC)
		{
			Vector3 position = ((Component)spawnPoint).transform.position;
			bool num = TerrainMeta.BiomeMap.GetBiomeMaxType(position) == 16;
			bool flag = EnvironmentManager.Check(position, EnvironmentType.TrainTunnels | EnvironmentType.UnderwaterLab | EnvironmentType.Submarine);
			bool topology = TerrainMeta.TopologyMap.GetTopology(position, 128);
			if (num && !flag && !topology && SingletonComponent<SpawnHandler>.Instance.JungleLoadouts != null && SingletonComponent<SpawnHandler>.Instance.JungleLoadouts.Length != 0)
			{
				humanNPC.EquipLoadout(SingletonComponent<SpawnHandler>.Instance.JungleLoadouts);
			}
		}
	}

	protected GameObjectRef GetPrefab()
	{
		float num = prefabs.Sum((SpawnEntry x) => (!preventDuplicates || !HasSpawned(x.prefab.resourceID)) ? x.weight : 0);
		if (num == 0f)
		{
			return null;
		}
		float num2 = Random.Range(0f, num);
		foreach (SpawnEntry prefab in prefabs)
		{
			int num3 = ((!preventDuplicates || !HasSpawned(prefab.prefab.resourceID)) ? prefab.weight : 0);
			if ((num2 -= (float)num3) <= 0f)
			{
				return prefab.prefab;
			}
		}
		return prefabs[prefabs.Count - 1].prefab;
	}

	protected virtual BaseSpawnPoint GetSpawnPoint(GameObjectRef prefabRef, out Vector3 pos, out Quaternion rot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		pos = Vector3.zero;
		rot = Quaternion.identity;
		bool flag = DoesRequireNavmeshToSpawn(prefabRef.Get());
		GameObjectEx.ToBaseEntity(((Component)this).transform);
		int num = Random.Range(0, spawnPoints.Length);
		for (int i = 0; i < spawnPoints.Length; i++)
		{
			BaseSpawnPoint baseSpawnPoint = spawnPoints[(num + i) % spawnPoints.Length];
			if (!((Object)(object)baseSpawnPoint == (Object)null) && (baseSpawnPoint.IsAvailableTo(prefabRef.Get()) || AllowOverlappingSpawns) && !baseSpawnPoint.HasPlayersIntersecting())
			{
				baseSpawnPoint.GetLocation(out pos, out rot);
				if (!flag)
				{
					return baseSpawnPoint;
				}
				if (RustNavMeshHelpers.SamplePosition(pos, out var hitWS, 2f, -1))
				{
					pos = ((NavMeshHit)(ref hitWS)).position;
					return baseSpawnPoint;
				}
				if (AI.logIssues)
				{
					Debug.LogWarning((object)$"Failed to spawn {prefabRef.Get()} at {pos} - no navmesh found");
				}
			}
		}
		return null;
	}

	private static bool DoesRequireNavmeshToSpawn(GameObject prefab)
	{
		if (!AI.npc_check_spawner_is_on_navmesh)
		{
			return false;
		}
		BaseNavigator baseNavigator = default(BaseNavigator);
		if (prefab.TryGetComponent<BaseNavigator>(ref baseNavigator) && !baseNavigator.CanUseNavMesh)
		{
			return false;
		}
		RustNavMeshAgent rustNavMeshAgent = default(RustNavMeshAgent);
		if (prefab.TryGetComponent<RustNavMeshAgent>(ref rustNavMeshAgent))
		{
			return true;
		}
		return false;
	}

	private MonumentInfo FindMonument()
	{
		return ((Component)this).GetComponentInParent<MonumentInfo>();
	}

	protected virtual void OnDrawGizmos()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = new Color(1f, 1f, 0f, 1f);
		Gizmos.DrawSphere(((Component)this).transform.position, 0.25f);
	}

	public void SetIsSpawningActive(bool isActive)
	{
		isSpawnerActive = isActive;
	}
}
