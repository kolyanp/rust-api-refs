using System;
using System.Collections.Generic;
using UnityEngine;

public class PlanterBoxStatic : PlanterBox
{
	[ServerVar(Help = "Chance of a favourable gene being picked [0-1]. Setting this to 0 does not ensure no favourable genes are picked up, but it greatly reduces the chances.")]
	public static float FavourableGeneChance = 0.5f;

	public List<GameObjectRef> staticPlantsSpawnlist;

	public bool randomPerSlot;

	public float respawnCheckTimer = 30f;

	[ServerVar(Help = "(Generated) Interval in seconds between respawn checks for growable plants in static planter boxes inside the deep sea zone; default 600s")]
	public static float DeepSeaRespawnCheckTimer = 600f;

	private TimeSince lastDeepSeaSpawn;

	private static ListHashSet<PlanterBoxStatic> AllStaticPlanters = new ListHashSet<PlanterBoxStatic>();

	private bool DeepSeaMode => DeepSeaManager.IsInsideDeepSea((BaseNetworkable)this);

	public override void SetupTimeCaches()
	{
	}

	public override void RefreshGrowables(GrowableEntity ignoreEntity = null)
	{
	}

	public static void OnDeepSeaSpawned()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<PlanterBoxStatic> enumerator = AllStaticPlanters.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				PlanterBoxStatic current = enumerator.Current;
				if (current.DeepSeaMode)
				{
					current.lastDeepSeaSpawn = TimeSince.op_Implicit(float.MaxValue);
					current.CreateStaticPlants();
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(CreateStaticPlants, 1f, DeepSeaMode ? 120f : respawnCheckTimer);
		AllStaticPlanters.Add(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		AllStaticPlanters.Remove(this);
	}

	public void CreateStaticPlants()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		soilSaturation = soilSaturationMax;
		GameObjectRef randomStaticPlant = GetRandomStaticPlant();
		Socket_Base[] array = PrefabAttribute.server.FindAll<Socket_Base>(prefabID);
		bool deepSeaMode = DeepSeaMode;
		if ((deepSeaMode && TimeSince.op_Implicit(lastDeepSeaSpawn) < DeepSeaRespawnCheckTimer) || (deepSeaMode && (Object)(object)DeepSeaManager.Get(base.isServer) != (Object)null && DeepSeaManager.Get(base.isServer).IsBusy()))
		{
			return;
		}
		foreach (Socket_Base socket_Base in array)
		{
			if (!(socket_Base is Socket_Specific_Female) || !IsSpawnPointFreeSearch(socket_Base.localPosition))
			{
				continue;
			}
			if (randomPerSlot)
			{
				randomStaticPlant = GetRandomStaticPlant();
			}
			Vector3 pos = ((Component)this).transform.TransformPoint(socket_Base.localPosition);
			BaseEntity baseEntity = GameManager.server.CreateEntity(randomStaticPlant.resourcePath, pos, Quaternion.identity);
			baseEntity.SetParent(this, worldPositionStays: true);
			baseEntity.Spawn();
			GrowableEntity growableEntity = baseEntity as GrowableEntity;
			if ((Object)(object)growableEntity != (Object)null)
			{
				growableEntity.Fertilize();
				growableEntity.SetGodQuality(qual: true);
				growableEntity.SetMaxGrowingConditions();
				growableEntity.Genes.GenerateFavourableGenes(growableEntity);
				if (deepSeaMode)
				{
					growableEntity.ChangeState(PlantProperties.State.Ripe, resetAge: false);
				}
				growableEntity.SendNetworkUpdate();
				OnPlantInserted(growableEntity, null);
				lastDeepSeaSpawn = TimeSince.op_Implicit(0f);
			}
		}
	}

	private GameObjectRef GetRandomStaticPlant()
	{
		if (staticPlantsSpawnlist == null || staticPlantsSpawnlist.Count == 0)
		{
			return null;
		}
		int index = Random.Range(0, staticPlantsSpawnlist.Count);
		return staticPlantsSpawnlist[index];
	}

	private bool IsSpawnPointFreeSearch(Vector3 localPos)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		foreach (BaseEntity child in children)
		{
			if (child is GrowableEntity && Vector3.Distance(((Component)child).transform.localPosition, localPos) < 0.05f)
			{
				return false;
			}
		}
		return true;
	}
}
