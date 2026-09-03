using System;
using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class Beehive : StorageContainer, IHeatSourceListener, ISplashable
{
	[Header("Beehive Settings")]
	public ItemDefinition HoneyCombDefinition;

	public ItemDefinition BeeNucleusDefinition;

	public float growthRate = 0.05f;

	public float beeStingTime = 2f;

	[Header("References")]
	public TriggerHurtEx hurtTrigger;

	public GameObjectRef masterSwarm;

	public const Flags HasNucleus = Flags.Reserved12;

	public const Flags HasBees = Flags.Reserved13;

	public const Flags AngryBees = Flags.Reserved14;

	[ServerVar(Help = "How long before a Beehive will update")]
	public static float updateHiveInterval;

	[ServerVar(Help = "How long before the Beehive will perform temperature and inside checks")]
	public static float updateHiveStatsInterval;

	[ServerVar(Help = "How much the Nucleus's XP should be increased per honeycomb generated")]
	public static int xpIncreasePerHoneycomb;

	private static Vector3[] outsideLookupDirs;

	private bool hasNucleus;

	private float createNewCombAccumulator;

	private float honeyCombProductionMultiplier = 2f;

	private TimeSince timeSinceAngryBees;

	private TimeCachedValue<float> temperatureExposure;

	private TimeCachedValue<float> humidityExposure;

	private TimeCachedValue<bool> outsideCheck;

	private float serverHumidity;

	private float serverTemperature;

	private bool serverOutside;

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (HasFlag(Flags.Reserved13) || HasFlag(Flags.Reserved12))
		{
			pickupErrorToFormat = (format: PickupErrors.ItemMustBeEmpty, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	public bool IsOutsideAccurate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return SocketMod_Inside.IsOutside(((Component)this).transform.position + Vector3.up * 0.2f, Quaternion.identity, outsideLookupDirs);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && info.fromDisk && info.msg.beehive != null)
		{
			createNewCombAccumulator = info.msg.beehive.currentProgress;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.beehive = Pool.Get<Beehive>();
		info.msg.beehive.currentProgress = createNewCombAccumulator;
		if (!info.forDisk)
		{
			info.msg.beehive.temperature = serverTemperature;
			info.msg.beehive.inside = serverOutside;
			info.msg.beehive.humidity = serverHumidity;
		}
	}

	public override void OnItemRemovedFromStack(Item item, int amount)
	{
		base.OnItemRemovedFromStack(item, amount);
		OnItemAddedOrRemoved(item, added: false);
	}

	public override void OnItemAddedToStack(Item item, int amount)
	{
		base.OnItemAddedToStack(item, amount);
		OnItemAddedOrRemoved(item, added: true);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		base.OnItemAddedOrRemoved(item, added);
		CheckNucleus();
		Flags flags = base.flags;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
		{
			float num = base.inventory.GetAmount(HoneyCombDefinition.itemid);
			flagsUpdateScope.Set(Flags.Reserved13, num > 0f && hasNucleus);
			if (!added && (Object)(object)item.info == (Object)(object)HoneyCombDefinition)
			{
				BasePlayer basePlayer = BasePlayer.FindByID(base.LastLootedBy);
				if ((Object)(object)basePlayer != (Object)null && basePlayer.IsAlive() && !basePlayer.IsNpc && basePlayer.isServer)
				{
					timeSinceAngryBees = TimeSince.op_Implicit(0f);
					flagsUpdateScope.Set(Flags.Reserved14, b: true);
				}
			}
		}
		if (base.inventory.IsFull(checkForPartialStacks: true))
		{
			StopHive();
		}
		else if (flags != base.flags)
		{
			SendNetworkUpdate();
		}
	}

	private void OnPhysicsNeighbourChanged()
	{
		using (TimeWarning.New("Beehive.OnPhysicsNeighbourChanged"))
		{
			CalculateQualifiers(force: true);
			SendNetworkUpdate();
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		if (!base.isClient)
		{
			CheckNucleus();
			Sprinkler.SplashableGrid.RegisterEntity(this);
		}
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		Sprinkler.SplashableGrid.OnParentChanged(this, oldParent, newParent);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		CalculateQualifiers(force: true);
		CheckNucleus();
		InvokeRepeating(HiveUpdateTick, 0f, 1f);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		Sprinkler.SplashableGrid.DeregisterEntity(this);
	}

	public void OnHeatSourceChanged()
	{
		CalculateQualifiers(force: true);
		SendNetworkUpdate();
	}

	private void HiveUpdateTick()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (TimeSince.op_Implicit(timeSinceAngryBees) > beeStingTime)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved14, b: false);
			}
		}
	}

	private void GenerateHoneyComb()
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		float num = base.inventory.GetAmount(HoneyCombDefinition.itemid);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved13, num > 0f && hasNucleus);
		}
		createNewCombAccumulator += growthRate * honeyCombProductionMultiplier;
		if (!(createNewCombAccumulator >= 1f))
		{
			return;
		}
		createNewCombAccumulator = 0f;
		if (hasNucleus)
		{
			Item slot = base.inventory.GetSlot(0);
			if (slot != null)
			{
				int dataInt = slot.instanceData.dataInt;
				if (NucleusGrading.XpToGrade(dataInt) != NucleusGrading.NucleusGrade.Grade1)
				{
					dataInt += xpIncreasePerHoneycomb;
					SetNucleusData(slot, dataInt);
				}
			}
		}
		Item item = ItemManager.Create(HoneyCombDefinition, 1, 0uL, isServerSide: true, 0uL);
		if (!item.MoveToContainer(base.inventory))
		{
			StopHive();
			item.Drop(base.inventory.dropPosition, base.inventory.dropVelocity);
		}
	}

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		if (targetSlot == 0)
		{
			return item.info.shortname.Equals(allowedItem.shortname);
		}
		if (targetSlot > 0)
		{
			return item.info.shortname.Equals(allowedItem2.shortname);
		}
		return base.ItemFilter(player, item, targetSlot);
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		return (Object)(object)splashType == (Object)(object)WaterTypes.RadioactiveWaterItemDef;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		if ((Object)(object)splashType == (Object)(object)WaterTypes.RadioactiveWaterItemDef)
		{
			Item slot = base.inventory.GetSlot(0);
			if (slot != null)
			{
				hasNucleus = (Object)(object)((Component)slot.info).GetComponent<ItemModBeehiveNucleus>() != (Object)null;
				if (hasNucleus)
				{
					base.inventory.Remove(slot);
					slot.Remove();
				}
			}
			return amount;
		}
		return amount;
	}

	private void SetNucleusData(Item targetItem, int xp)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		if (targetItem != null)
		{
			targetItem.instanceData = new InstanceData
			{
				ShouldPool = false,
				dataInt = xp
			};
		}
	}

	private void CheckNucleus()
	{
		if (base.inventory == null)
		{
			return;
		}
		Item slot = base.inventory.GetSlot(0);
		if (slot != null)
		{
			hasNucleus = (Object)(object)((Component)slot.info).GetComponent<ItemModBeehiveNucleus>() != (Object)null;
			if (slot == null || slot.instanceData == null || (slot.instanceData.dataInt == 0 && slot.instanceData.dataFloat == 0f))
			{
				SetNucleusData(slot, 0);
			}
			createNewCombAccumulator = 0f;
		}
		else
		{
			hasNucleus = false;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved12, hasNucleus);
		}
		if (hasNucleus)
		{
			StartHive();
		}
		else
		{
			StopHive();
		}
	}

	private void StartHive()
	{
		if (!IsInvoking(UpdateGrowthRate))
		{
			InvokeRepeating(UpdateGrowthRate, 0f, updateHiveInterval);
		}
		if (!IsInvoking(GenerateHoneyComb))
		{
			InvokeRepeating(GenerateHoneyComb, updateHiveInterval, updateHiveInterval);
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: true);
		}
		CalculateQualifiers(force: true);
		SendNetworkUpdate();
	}

	private void StopHive()
	{
		if (IsInvoking(UpdateGrowthRate))
		{
			CancelInvoke(UpdateGrowthRate);
		}
		if (IsInvoking(GenerateHoneyComb))
		{
			CancelInvoke(GenerateHoneyComb);
		}
		SetFlagLocal(Flags.On, b: false);
		SendNetworkUpdate();
	}

	public float CalculateRain()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			SingletonComponent<NpcFireManager>.Instance.GetFiresAround(((Component)this).transform.position, 2f, (List<BaseEntity>)(object)val);
			if (((List<BaseEntity>)(object)val).Count > 0)
			{
				return 0f;
			}
			if (!IsOutside())
			{
				return 0f;
			}
			return Climate.GetRain(((Component)this).transform.position);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public float CalculateTemperature()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		float temperature = Climate.GetTemperature(((Component)this).transform.position);
		float num = GrowableEntity.CalculateArtificialTemperature(((Component)this).transform);
		bool flag = num >= 10f;
		bool flag2 = temperature < 10f;
		bool flag3 = temperature < 16f && !flag2;
		if (flag)
		{
			if (flag3)
			{
				return 20f;
			}
			if (flag2)
			{
				return 16f;
			}
		}
		return temperature + num;
	}

	private void CalculateQualifiers(bool force = false)
	{
		using (TimeWarning.New("Beehive.CalculateQualifiers"))
		{
			if (temperatureExposure == null)
			{
				temperatureExposure = new TimeCachedValue<float>
				{
					refreshCooldown = updateHiveStatsInterval,
					refreshRandomRange = 5f,
					updateValue = CalculateTemperature
				};
			}
			if (outsideCheck == null)
			{
				outsideCheck = new TimeCachedValue<bool>
				{
					refreshCooldown = updateHiveStatsInterval,
					refreshRandomRange = 5f,
					updateValue = IsOutsideAccurate
				};
			}
			if (humidityExposure == null)
			{
				humidityExposure = new TimeCachedValue<float>
				{
					refreshCooldown = updateHiveStatsInterval,
					refreshRandomRange = 5f,
					updateValue = CalculateRain
				};
			}
			serverHumidity = humidityExposure.Get(force);
			serverTemperature = temperatureExposure.Get(force);
			serverOutside = outsideCheck.Get(force);
		}
	}

	private void UpdateGrowthRate()
	{
		using (TimeWarning.New("Beehive.UpdateGrowthRate"))
		{
			CalculateQualifiers();
			float num = serverTemperature;
			float num2 = ((num < 28f) ? ((num < 10f) ? 0.010000001f : ((!(num < 16f)) ? 0.1f : 0.05f)) : ((!(num < 40f)) ? 0.010000001f : 0.05f));
			growthRate = num2;
			Item slot = base.inventory.GetSlot(0);
			if (slot != null)
			{
				switch (NucleusGrading.XpToGrade(slot.instanceData.dataInt))
				{
				case NucleusGrading.NucleusGrade.Grade2:
					growthRate *= 2f;
					break;
				case NucleusGrading.NucleusGrade.Grade1:
					growthRate *= 3f;
					break;
				}
			}
			if (serverHumidity >= 0.5f)
			{
				growthRate *= 0.5f;
			}
			if (!serverOutside)
			{
				growthRate = 0f;
			}
			SendNetworkUpdate();
		}
	}

	public override void DropItems(BaseEntity initiator = null)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		int index = -1;
		for (int i = 0; i < base.inventory.itemList.Count; i++)
		{
			if ((Object)(object)base.inventory.itemList[i].info == (Object)(object)BeeNucleusDefinition)
			{
				flag = true;
				index = i;
			}
		}
		if (flag && base.inventory.Remove(base.inventory.itemList[index]))
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(masterSwarm.resourcePath, ((Component)this).transform.position + Vector3.up * 1.5f, Quaternion.identity);
			if (creatorEntity is BasePlayer basePlayer)
			{
				baseEntity.creatorEntity = basePlayer;
				baseEntity.OwnerID = basePlayer.userID;
			}
			baseEntity.Spawn();
		}
		base.DropItems(initiator);
	}

	static Beehive()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		updateHiveInterval = 120f;
		updateHiveStatsInterval = 120f;
		xpIncreasePerHoneycomb = 2;
		Vector3[] array = new Vector3[5];
		Vector3 val = new Vector3(0f, 1f, 0f);
		array[0] = ((Vector3)(ref val)).normalized;
		val = new Vector3(1f, 0f, 0f);
		array[1] = ((Vector3)(ref val)).normalized;
		val = new Vector3(0f, 0f, 1f);
		array[2] = ((Vector3)(ref val)).normalized;
		val = new Vector3(-1f, 0f, 0f);
		array[3] = ((Vector3)(ref val)).normalized;
		val = new Vector3(0f, 0f, -1f);
		array[4] = ((Vector3)(ref val)).normalized;
		outsideLookupDirs = (Vector3[])(object)array;
	}
}
