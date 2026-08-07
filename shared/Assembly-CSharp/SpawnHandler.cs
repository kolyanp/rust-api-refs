using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConVar;
using UnityEngine;

public class SpawnHandler : SingletonComponent<SpawnHandler>, SpawnPopulationBase.ISpawnHandler
{
	private class DebugSpawner : SpawnPopulationBase.ISpawnHandler
	{
		private SpawnHandler handler;

		private List<(Vector3, SpawnPopulationBase.Status)> samples;

		public DebugSpawner(SpawnHandler handler, List<(Vector3, SpawnPopulationBase.Status)> samples)
		{
			this.handler = handler;
			this.samples = samples;
		}

		SpawnPopulationBase.Status SpawnPopulationBase.ISpawnHandler.TrySpawn(SpawnPopulationBase pop, Prefab<Spawnable> prefab, Vector3 pos, Quaternion rot, out GameObject spawned)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			spawned = null;
			if (!handler.Validate(pop, prefab, pos, rot))
			{
				return SpawnPopulationBase.Status.PrefabRejected;
			}
			if ((Object)(object)((Component)prefab.Component).GetComponent<BaseEntity>() == (Object)null || ((Component)prefab.Component).CompareTag("CannotBeCreated"))
			{
				return SpawnPopulationBase.Status.InvalidEntity;
			}
			return SpawnPopulationBase.Status.Success;
		}

		void SpawnPopulationBase.ISpawnHandler.ReportAttempt(SpawnPopulationBase.Status status, Vector3 pos)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			samples.Add((pos, status));
		}
	}

	public float TickInterval = 60f;

	public int MinSpawnsPerTick = 100;

	public int MaxSpawnsPerTick = 100;

	public LayerMask PlacementMask;

	public LayerMask PlacementCheckMask;

	public float PlacementCheckHeight = 25f;

	public LayerMask RadiusCheckMask;

	public float RadiusCheckDistance = 5f;

	public LayerMask BoundsCheckMask;

	public SpawnFilter CharacterSpawn;

	public float CharacterSpawnCutoff;

	public SpawnPopulationBase[] SpawnPopulations;

	public SpawnDistribution[] SpawnDistributions;

	public SpawnDistribution CharDistribution;

	public ListHashSet<ISpawnGroup> SpawnGroups = new ListHashSet<ISpawnGroup>();

	internal List<SpawnIndividual> SpawnIndividuals = new List<SpawnIndividual>();

	[Header("Scientist Outfits")]
	public PlayerInventoryProperties[] JungleLoadouts;

	[ReadOnly]
	public SpawnPopulationBase[] ConvarSpawnPopulations;

	public Dictionary<SpawnPopulationBase, SpawnDistribution> population2distribution;

	private bool spawnTick;

	public SpawnPopulationBase[] AllSpawnPopulations;

	private static int PlayerCount
	{
		get
		{
			if (ConVar.Spawn.loot_population_test <= 0)
			{
				return BasePlayer.activePlayerList.Count;
			}
			return ConVar.Spawn.loot_population_test;
		}
	}

	protected void OnEnable()
	{
		AllSpawnPopulations = SpawnPopulations.Concat(ConvarSpawnPopulations).ToArray();
		((MonoBehaviour)this).StartCoroutine(SpawnTick());
		((MonoBehaviour)this).StartCoroutine(SpawnGroupTick());
		((MonoBehaviour)this).StartCoroutine(SpawnIndividualTick());
	}

	public static BasePlayer.SpawnPoint GetSpawnPoint()
	{
		if ((Object)(object)SingletonComponent<SpawnHandler>.Instance == (Object)null || SingletonComponent<SpawnHandler>.Instance.CharDistribution == null)
		{
			return null;
		}
		BasePlayer.SpawnPoint spawnPoint = new BasePlayer.SpawnPoint();
		if (!((WaterSystem.OceanLevel < 0.5f) ? GetSpawnPointStandard(spawnPoint) : FloodedSpawnHandler.GetSpawnPoint(spawnPoint, WaterSystem.OceanLevel + 1f)))
		{
			return null;
		}
		return spawnPoint;
	}

	public static BasePlayer.SpawnPoint GetSpawnPointForTeam(ulong teamId)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (teamId == 0L)
		{
			return null;
		}
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindTeam(teamId);
		if (playerTeam == null)
		{
			return null;
		}
		if (!playerTeam.usePartySpawn)
		{
			return null;
		}
		if (playerTeam.firstSpawnLocation == default(Vector3))
		{
			return null;
		}
		float num = 100000f;
		BasePlayer.SpawnPoint spawnPoint = null;
		for (int i = 0; i < party.maxpartyspawnattempts; i++)
		{
			BasePlayer.SpawnPoint spawnPoint2 = GetSpawnPoint();
			float num2 = Vector3Ex.Distance2D(spawnPoint2.pos, playerTeam.firstSpawnLocation);
			if (num2 < num || spawnPoint == null)
			{
				spawnPoint = spawnPoint2;
				num = num2;
			}
			if (num2 < (float)party.maxpartyspawndistance)
			{
				return spawnPoint2;
			}
		}
		return spawnPoint;
	}

	private static bool GetSpawnPointStandard(BasePlayer.SpawnPoint spawnPoint)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 60; i++)
		{
			if (!SingletonComponent<SpawnHandler>.Instance.CharDistribution.Sample(out spawnPoint.pos, out spawnPoint.rot, alignToNormal: false, 0f, 0.5f, SingletonComponent<SpawnHandler>.Instance.CharacterSpawn, SingletonComponent<SpawnHandler>.Instance.CharacterSpawnCutoff))
			{
				continue;
			}
			bool flag = true;
			if ((Object)(object)TerrainMeta.Path != (Object)null)
			{
				foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
				{
					if (monument.Distance(spawnPoint.pos) < 50f)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateDistributions()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (World.Size == 0)
		{
			return;
		}
		SpawnDistributions = new SpawnDistribution[AllSpawnPopulations.Length];
		population2distribution = new Dictionary<SpawnPopulationBase, SpawnDistribution>();
		Vector3 size = TerrainMeta.Size;
		Vector3 position = TerrainMeta.Position;
		int populationRes = Mathf.NextPowerOfTwo((int)((float)World.Size * 0.25f));
		for (int i = 0; i < AllSpawnPopulations.Length; i++)
		{
			SpawnPopulationBase spawnPopulationBase = AllSpawnPopulations[i];
			if (spawnPopulationBase == null)
			{
				Debug.LogError((object)"Spawn handler contains null spawn population.");
				continue;
			}
			byte[] baseMapValues = spawnPopulationBase.GetBaseMapValues(populationRes);
			SpawnDistribution value = (SpawnDistributions[i] = new SpawnDistribution(this, baseMapValues, position, size));
			population2distribution.Add(spawnPopulationBase, value);
		}
		int char_res = Mathf.NextPowerOfTwo((int)((float)World.Size * 0.5f));
		byte[] map = new byte[char_res * char_res];
		SpawnFilter filter = CharacterSpawn;
		float cutoff = CharacterSpawnCutoff;
		Parallel.For(0, char_res, delegate(int z)
		{
			for (int j = 0; j < char_res; j++)
			{
				float normX = ((float)j + 0.5f) / (float)char_res;
				float normZ = ((float)z + 0.5f) / (float)char_res;
				float factor = filter.GetFactor(normX, normZ);
				map[z * char_res + j] = (byte)((factor > cutoff) ? (255f * factor) : 0f);
			}
		});
		CharDistribution = new SpawnDistribution(this, map, position, size);
	}

	public void FillPopulations()
	{
		if (SpawnDistributions == null)
		{
			return;
		}
		for (int i = 0; i < AllSpawnPopulations.Length; i++)
		{
			if (!(AllSpawnPopulations[i] == null))
			{
				SpawnInitial(AllSpawnPopulations[i], SpawnDistributions[i]);
			}
		}
	}

	public void DeletePopulation(string name)
	{
		SpawnPopulationBase[] allSpawnPopulations = AllSpawnPopulations;
		foreach (SpawnPopulationBase spawnPopulationBase in allSpawnPopulations)
		{
			if (((Object)spawnPopulationBase).name == name)
			{
				spawnPopulationBase.DeleteEntities();
				break;
			}
		}
	}

	public void DeleteAllPopulations()
	{
		SpawnPopulationBase[] allSpawnPopulations = AllSpawnPopulations;
		for (int i = 0; i < allSpawnPopulations.Length; i++)
		{
			allSpawnPopulations[i].DeleteEntities();
		}
	}

	public void FillGroups()
	{
		for (int i = 0; i < SpawnGroups.Count; i++)
		{
			SpawnGroups[i].Fill();
		}
	}

	public void ClearGroups()
	{
		for (int i = 0; i < SpawnGroups.Count; i++)
		{
			SpawnGroups[i].Clear();
		}
	}

	public void ResetGroups()
	{
		ClearGroups();
		Invoke(FillGroups, 0f);
	}

	public void FillIndividuals()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < SpawnIndividuals.Count; i++)
		{
			SpawnIndividual spawnIndividual = SpawnIndividuals[i];
			Spawn(Prefab.Load<Spawnable>(spawnIndividual.PrefabID, (GameManager)null, (PrefabAttribute.Library)null), spawnIndividual.Position, spawnIndividual.Rotation);
		}
	}

	public void InitialSpawn()
	{
		if (ConVar.Spawn.respawn_populations && SpawnDistributions != null)
		{
			for (int i = 0; i < AllSpawnPopulations.Length; i++)
			{
				if (!(AllSpawnPopulations[i] == null))
				{
					SpawnInitial(AllSpawnPopulations[i], SpawnDistributions[i]);
				}
			}
		}
		if (ConVar.Spawn.respawn_groups)
		{
			for (int j = 0; j < SpawnGroups.Count; j++)
			{
				SpawnGroups[j].SpawnInitial();
			}
		}
	}

	public void StartSpawnTick()
	{
		spawnTick = true;
	}

	private IEnumerator SpawnTick()
	{
		while (true)
		{
			yield return CoroutineEx.waitForEndOfFrame;
			if (!spawnTick || !ConVar.Spawn.respawn_populations)
			{
				continue;
			}
			yield return CoroutineEx.waitForSeconds(ConVar.Spawn.tick_populations);
			for (int i = 0; i < AllSpawnPopulations.Length; i++)
			{
				SpawnPopulationBase spawnPopulationBase = AllSpawnPopulations[i];
				if (spawnPopulationBase == null)
				{
					continue;
				}
				SpawnDistribution spawnDistribution = SpawnDistributions[i];
				if (spawnDistribution == null)
				{
					continue;
				}
				try
				{
					if (SpawnDistributions != null)
					{
						SpawnRepeating(spawnPopulationBase, spawnDistribution);
					}
				}
				catch (Exception ex)
				{
					Debug.LogError((object)ex);
				}
				yield return CoroutineEx.waitForEndOfFrame;
			}
		}
	}

	private IEnumerator SpawnGroupTick()
	{
		while (true)
		{
			yield return CoroutineEx.waitForEndOfFrame;
			if (!spawnTick || !ConVar.Spawn.respawn_groups)
			{
				continue;
			}
			yield return CoroutineEx.waitForSeconds(1f);
			for (int i = 0; i < SpawnGroups.Count; i++)
			{
				ISpawnGroup spawnGroup = SpawnGroups[i];
				if (spawnGroup != null)
				{
					try
					{
						spawnGroup.SpawnRepeating();
					}
					catch (Exception ex)
					{
						Debug.LogError((object)ex);
					}
					yield return CoroutineEx.waitForEndOfFrame;
				}
			}
		}
	}

	private IEnumerator SpawnIndividualTick()
	{
		while (true)
		{
			yield return CoroutineEx.waitForEndOfFrame;
			if (!spawnTick || !ConVar.Spawn.respawn_individuals)
			{
				continue;
			}
			yield return CoroutineEx.waitForSeconds(ConVar.Spawn.tick_individuals);
			for (int i = 0; i < SpawnIndividuals.Count; i++)
			{
				SpawnIndividual spawnIndividual = SpawnIndividuals[i];
				try
				{
					Spawn(Prefab.Load<Spawnable>(spawnIndividual.PrefabID, (GameManager)null, (PrefabAttribute.Library)null), spawnIndividual.Position, spawnIndividual.Rotation);
				}
				catch (Exception ex)
				{
					Debug.LogError((object)ex);
				}
				yield return CoroutineEx.waitForEndOfFrame;
			}
		}
	}

	public void SpawnInitial(SpawnPopulationBase population, SpawnDistribution distribution)
	{
		int targetCount = population.GetTargetCount(distribution);
		int count = distribution.Count;
		int numToFill = targetCount - count;
		population.Fill(this, distribution, numToFill, initialSpawn: true);
	}

	public void SpawnRepeating(SpawnPopulationBase population, SpawnDistribution distribution)
	{
		int targetCount = population.GetTargetCount(distribution);
		int count = distribution.Count;
		int num = targetCount - count;
		num = Mathf.RoundToInt((float)num * population.GetCurrentSpawnRate());
		num = Random.Range(Mathf.Min(num, MinSpawnsPerTick), Mathf.Min(num, MaxSpawnsPerTick));
		population.Fill(this, distribution, num, initialSpawn: false);
	}

	public int EstimateMaxPopToSpawn(int toSpawn, SpawnPopulationBase pop)
	{
		toSpawn = Mathf.RoundToInt((float)toSpawn * pop.GetCurrentSpawnRate());
		return Mathf.Min(toSpawn, MaxSpawnsPerTick);
	}

	public bool Validate(SpawnPopulationBase population, Prefab<Spawnable> prefab, Vector3 pos, Quaternion rot)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		if (prefab == null)
		{
			return false;
		}
		if ((Object)(object)prefab.Component == (Object)null)
		{
			Debug.LogError((object)("[Spawn] Missing component 'Spawnable' on " + prefab.Name));
			return false;
		}
		Vector3 scale = Vector3.one;
		DecorComponent[] components = PrefabAttribute.server.FindAll<DecorComponent>(prefab.ID);
		prefab.Object.transform.ApplyDecorComponents(components, ref pos, ref rot, ref scale);
		if (!prefab.ApplyTerrainFilters(pos, rot, scale))
		{
			return false;
		}
		if (!prefab.ApplyWaterChecks(pos, rot, scale))
		{
			return false;
		}
		if (!prefab.ApplyTerrainAnchors(ref pos, rot, scale, TerrainAnchorMode.MinimizeMovement, population.GetSpawnFilter()))
		{
			return false;
		}
		if (!prefab.ApplyTerrainChecks(pos, rot, scale, population.GetSpawnFilter()))
		{
			return false;
		}
		if (!prefab.ApplyEnvironmentVolumeChecks(pos, rot, scale))
		{
			return false;
		}
		if (!prefab.ApplyBoundsChecks(pos, rot, scale, BoundsCheckMask))
		{
			return false;
		}
		if (!prefab.Component.CanSpawnInSafeZone && IsInSafeZone(pos))
		{
			return false;
		}
		return true;
	}

	private static bool IsInSafeZone(Vector3 pos)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		foreach (TriggerSafeZone allSafeZone in TriggerSafeZone.allSafeZones)
		{
			Collider val = allSafeZone?.triggerCollider;
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			Bounds bounds = val.bounds;
			if (((Bounds)(ref bounds)).Contains(pos))
			{
				Vector3 val2 = val.ClosestPoint(pos) - pos;
				if (((Vector3)(ref val2)).sqrMagnitude <= 0.0001f)
				{
					return true;
				}
			}
		}
		return false;
	}

	void SpawnPopulationBase.ISpawnHandler.ReportAttempt(SpawnPopulationBase.Status status, Vector3 pos)
	{
	}

	SpawnPopulationBase.Status SpawnPopulationBase.ISpawnHandler.TrySpawn(SpawnPopulationBase population, Prefab<Spawnable> prefab, Vector3 pos, Quaternion rot, out GameObject spawned)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		spawned = null;
		if (!Validate(population, prefab, pos, rot))
		{
			return SpawnPopulationBase.Status.PrefabRejected;
		}
		if (Global.developer > 1)
		{
			Debug.Log((object)("[Spawn] Spawning " + prefab.Name));
		}
		BaseEntity baseEntity = prefab.SpawnEntity(pos, rot, active: false);
		if ((Object)(object)baseEntity == (Object)null)
		{
			Debug.LogWarning((object)("[Spawn] Couldn't create prefab as entity - " + prefab.Name));
			return SpawnPopulationBase.Status.InvalidEntity;
		}
		Spawnable component = ((Component)baseEntity).GetComponent<Spawnable>();
		if (component.Population != population)
		{
			component.Population = population;
		}
		PoolableEx.AwakeFromInstantiate(((Component)baseEntity).gameObject);
		baseEntity.Spawn();
		spawned = ((Component)baseEntity).gameObject;
		return SpawnPopulationBase.Status.Success;
	}

	private GameObject Spawn(Prefab<Spawnable> prefab, Vector3 pos, Quaternion rot)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (!CheckBounds(prefab.Object, pos, rot, Vector3.one))
		{
			return null;
		}
		BaseEntity baseEntity = prefab.SpawnEntity(pos, rot);
		if ((Object)(object)baseEntity == (Object)null)
		{
			Debug.LogWarning((object)("[Spawn] Couldn't create prefab as entity - " + prefab.Name));
			return null;
		}
		baseEntity.Spawn();
		return ((Component)baseEntity).gameObject;
	}

	public bool CheckBounds(GameObject gameObject, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return CheckBounds(gameObject, pos, rot, scale, BoundsCheckMask);
	}

	public static bool CheckBounds(GameObject gameObject, Vector3 pos, Quaternion rot, Vector3 scale, LayerMask mask)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)gameObject == (Object)null)
		{
			return true;
		}
		if (LayerMask.op_Implicit(mask) != 0)
		{
			BaseEntity component = gameObject.GetComponent<BaseEntity>();
			if ((Object)(object)component != (Object)null && Physics.CheckBox(pos + rot * Vector3.Scale(((Bounds)(ref component.bounds)).center, scale), Vector3.Scale(((Bounds)(ref component.bounds)).extents, scale), rot, LayerMask.op_Implicit(mask)))
			{
				return false;
			}
		}
		return true;
	}

	public void EnforceLimits(bool forceAll = false)
	{
		if (SpawnDistributions == null)
		{
			return;
		}
		for (int i = 0; i < AllSpawnPopulations.Length; i++)
		{
			if (!(AllSpawnPopulations[i] == null))
			{
				SpawnPopulationBase spawnPopulationBase = AllSpawnPopulations[i];
				SpawnDistribution distribution = SpawnDistributions[i];
				if (forceAll || spawnPopulationBase.EnforcePopulationLimits)
				{
					EnforceLimits(spawnPopulationBase, distribution);
				}
			}
		}
	}

	public void EnforceLimits(SpawnPopulationBase population, SpawnDistribution distribution)
	{
		int targetCount = population.GetTargetCount(distribution);
		Spawnable[] array = FindAll(population);
		if (array.Length <= targetCount)
		{
			return;
		}
		Debug.Log((object)(((object)population)?.ToString() + " has " + array.Length + " objects, but max allowed is " + targetCount));
		int count = array.Length - targetCount;
		Debug.Log((object)(" - deleting " + count + " objects"));
		foreach (Spawnable item in array.Take(count))
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((Component)item).gameObject);
			if (baseEntity.IsValid())
			{
				baseEntity.Kill();
			}
			else
			{
				GameManager.Destroy(((Component)item).gameObject);
			}
		}
	}

	public Spawnable[] FindAll(SpawnPopulationBase population)
	{
		return (from x in Object.FindObjectsByType<Spawnable>((FindObjectsSortMode)0)
			where ((Component)x).gameObject.activeInHierarchy && x.Population == population
			select x).ToArray();
	}

	public void AddRespawn(SpawnIndividual individual)
	{
		SpawnIndividuals.Add(individual);
	}

	public void AddInstance(Spawnable spawnable)
	{
		if (spawnable.Population != null)
		{
			spawnable.Population.AddInstance(spawnable);
			if (!population2distribution.TryGetValue(spawnable.Population, out var value))
			{
				Debug.LogWarning((object)("[SpawnHandler] trying to add instance to invalid population: " + (object)spawnable.Population));
			}
			else
			{
				value.AddInstance(spawnable);
			}
		}
	}

	public void RemoveInstance(Spawnable spawnable)
	{
		if (spawnable.Population != null)
		{
			spawnable.Population.RemoveInstance(spawnable);
			if (!population2distribution.TryGetValue(spawnable.Population, out var value))
			{
				Debug.LogWarning((object)("[SpawnHandler] trying to remove instance from invalid population: " + (object)spawnable.Population));
			}
			else
			{
				value.RemoveInstance(spawnable);
			}
		}
	}

	public static float PlayerFraction()
	{
		float num = Mathf.Max(Server.maxplayers, 1);
		if (ConVar.Spawn.population_cap_rate > 0 && Server.maxplayers > ConVar.Spawn.population_cap_rate)
		{
			num = ConVar.Spawn.population_cap_rate;
		}
		return Mathf.Clamp01((float)PlayerCount / num);
	}

	public static float PlayerLerp(float min, float max)
	{
		return Mathf.Lerp(min, max, PlayerFraction());
	}

	public static float PlayerExcess()
	{
		float num = Mathf.Max(ConVar.Spawn.player_base, 1f);
		float num2 = PlayerCount;
		if (num2 > (float)ConVar.Spawn.population_cap_rate && ConVar.Spawn.population_cap_rate > 0)
		{
			num2 = ConVar.Spawn.population_cap_rate;
		}
		if (num2 <= num)
		{
			return 0f;
		}
		return (num2 - num) / num;
	}

	public static float PlayerScale(float scalar)
	{
		return Mathf.Max(1f, PlayerExcess() * scalar);
	}

	public void DumpReport(string filename)
	{
		File.AppendAllText(filename, "\r\n\r\nSpawnHandler Report:\r\n\r\n" + GetReport());
	}

	public string GetReport(bool detailed = true, string filter = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (AllSpawnPopulations == null)
		{
			stringBuilder.AppendLine("Spawn population array is null.");
		}
		if (SpawnDistributions == null)
		{
			stringBuilder.AppendLine("Spawn distribution array is null.");
		}
		if (AllSpawnPopulations != null && SpawnDistributions != null)
		{
			for (int i = 0; i < AllSpawnPopulations.Length; i++)
			{
				if (AllSpawnPopulations[i] == null)
				{
					continue;
				}
				SpawnPopulationBase spawnPopulationBase = AllSpawnPopulations[i];
				SpawnDistribution spawnDistribution = SpawnDistributions[i];
				if (filter != null && !((Object)spawnPopulationBase).name.Contains(filter))
				{
					continue;
				}
				if (spawnPopulationBase != null)
				{
					spawnPopulationBase.GetReportString(stringBuilder, detailed);
					if (spawnDistribution != null)
					{
						int count = spawnDistribution.Count;
						int targetCount = spawnPopulationBase.GetTargetCount(spawnDistribution);
						stringBuilder.AppendLine("- Population: " + count + "/" + targetCount);
						int toSpawn = targetCount - count;
						toSpawn = EstimateMaxPopToSpawn(toSpawn, spawnPopulationBase);
						toSpawn = spawnPopulationBase.EstimateMaxAttempts(toSpawn);
						stringBuilder.Append("- Max attempts for next tick: ");
						stringBuilder.Append(toSpawn);
						stringBuilder.AppendLine();
						int failedFillsInARow = spawnPopulationBase.FailedFillsInARow;
						if (failedFillsInARow > 0)
						{
							stringBuilder.Append("- Failed to reach target population in a row: ");
							stringBuilder.Append(failedFillsInARow);
							stringBuilder.Append("(Recent average spawns: ");
							stringBuilder.Append(spawnPopulationBase.AvgSpawnCount);
							stringBuilder.AppendLine(")");
						}
					}
					else
					{
						stringBuilder.AppendLine("- Distribution #" + i + " is not set.");
					}
				}
				else
				{
					stringBuilder.AppendLine("Population #" + i + " is not set.");
				}
				stringBuilder.AppendLine();
			}
		}
		return stringBuilder.ToString();
	}

	public void GenerateDebugMaps(string name, int simCount, out int spawned, out int attempts)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		spawned = 0;
		attempts = 0;
		SpawnPopulationBase spawnPopulationBase = null;
		SpawnDistribution distribution = null;
		for (int i = 0; i < AllSpawnPopulations.Length; i++)
		{
			SpawnPopulationBase spawnPopulationBase2 = AllSpawnPopulations[i];
			if (((Object)spawnPopulationBase2).name == name)
			{
				spawnPopulationBase = spawnPopulationBase2;
				distribution = SpawnDistributions[i];
				break;
			}
		}
		if (spawnPopulationBase == null)
		{
			return;
		}
		int num = Mathf.NextPowerOfTwo((int)((float)World.Size * 0.25f));
		ExportToPNG(spawnPopulationBase.GetBaseMapValues(num), num, (TextureFormat)63, ((Object)spawnPopulationBase).name + ".png");
		byte[] array = new byte[World.Size * World.Size * 3];
		List<(Vector3, SpawnPopulationBase.Status)> list = new List<(Vector3, SpawnPopulationBase.Status)>(simCount);
		DebugSpawner spawnHandler = new DebugSpawner(this, list);
		spawnPopulationBase.SubFill(spawnHandler, distribution, simCount, initialSpawn: false);
		attempts = list.Count;
		Color val2 = default(Color);
		foreach (var item3 in list)
		{
			Vector3 item = item3.Item1;
			SpawnPopulationBase.Status item2 = item3.Item2;
			Vector2i val = (Vector2i)(Vector3Ex.XZ2D(item) + new Vector2((float)(World.Size / 2), (float)(World.Size / 2)));
			long num2 = (val.y * World.Size + val.x) * 3;
			switch (item2)
			{
			case SpawnPopulationBase.Status.Success:
				val2 = Color.green;
				spawned++;
				break;
			case SpawnPopulationBase.Status.InvalidSample:
				val2 = Color.red;
				break;
			case SpawnPopulationBase.Status.PrefabRejected:
				val2 = Color.yellow;
				break;
			case SpawnPopulationBase.Status.PrefabPickFailed:
				val2 = Color.blue;
				break;
			case SpawnPopulationBase.Status.InvalidEntity:
				val2 = Color.cyan;
				break;
			case SpawnPopulationBase.Status.InvalidSpawnPosOverride:
				((Color)(ref val2))._002Ector(1f, 0.41f, 0f);
				break;
			case SpawnPopulationBase.Status.DensityOverflow:
				((Color)(ref val2))._002Ector(1f, 0f, 0.91f);
				break;
			default:
				val2 = Color.magenta;
				break;
			}
			array[num2] = (byte)(val2.r * 255f);
			array[num2 + 1] = (byte)(val2.g * 255f);
			array[num2 + 2] = (byte)(val2.b * 255f);
		}
		ExportToPNG(array, (int)World.Size, (TextureFormat)3, ((Object)spawnPopulationBase).name + "-samples.png");
	}

	public int GenerateOreNodeMap(out int inSafeZone)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		int size = (int)World.Size;
		byte[] array = new byte[size * size * 3];
		inSafeZone = 0;
		int num = 0;
		foreach (OreResourceEntity item in BaseNetworkable.serverEntities.OfType<OreResourceEntity>())
		{
			Vector3 position = ((Component)item).transform.position;
			bool flag = IsInSafeZone(position);
			Color val = (flag ? Color.red : Color.green);
			Vector2i val2 = (Vector2i)(Vector3Ex.XZ2D(position) + new Vector2((float)(size / 2), (float)(size / 2)));
			long num2 = (val2.y * size + val2.x) * 3;
			array[num2] = (byte)(val.r * 255f);
			array[num2 + 1] = (byte)(val.g * 255f);
			array[num2 + 2] = (byte)(val.b * 255f);
			num++;
			if (flag)
			{
				inSafeZone++;
			}
		}
		foreach (TriggerSafeZone allSafeZone in TriggerSafeZone.allSafeZones)
		{
			Collider val3 = allSafeZone?.triggerCollider;
			if (!((Object)(object)val3 == (Object)null))
			{
				Bounds bounds = val3.bounds;
				PlotCircle(array, size, ((Bounds)(ref bounds)).center, ((Bounds)(ref bounds)).extents.x, Color.blue);
			}
		}
		ExportToPNG(array, size, (TextureFormat)3, "ore-nodes.png");
		return num;
	}

	private static void ExportToPNG(byte[] data, int size, TextureFormat format, string filename)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		Texture2D val = new Texture2D(size, size, format, false);
		val.SetPixelData<byte>(data, 0, 0);
		val.Apply();
		byte[] bytes = ImageConversion.EncodeToPNG(val);
		if (!Directory.Exists(Server.rootFolder + "/debug"))
		{
			Directory.CreateDirectory(Server.rootFolder + "/debug");
		}
		File.WriteAllBytes(Server.rootFolder + "/debug/" + filename, bytes);
	}

	private static void PlotCircle(byte[] pixels, int size, Vector3 worldCenter, float worldRadius, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		Vector2i val = (Vector2i)(Vector3Ex.XZ2D(worldCenter) + new Vector2((float)(size / 2), (float)(size / 2)));
		int num = Mathf.RoundToInt(worldRadius);
		int num2 = Mathf.Max(64, num * 4);
		for (int i = 0; i < num2; i++)
		{
			float num3 = (float)i / (float)num2 * MathF.PI * 2f;
			int num4 = val.x + Mathf.RoundToInt(Mathf.Cos(num3) * (float)num);
			int num5 = val.y + Mathf.RoundToInt(Mathf.Sin(num3) * (float)num);
			if (num4 >= 0 && num4 < size && num5 >= 0 && num5 < size)
			{
				long num6 = ((long)num5 * (long)size + num4) * 3;
				pixels[num6] = (byte)(color.r * 255f);
				pixels[num6 + 1] = (byte)(color.g * 255f);
				pixels[num6 + 2] = (byte)(color.b * 255f);
			}
		}
	}
}
