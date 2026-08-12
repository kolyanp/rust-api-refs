using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class ChickenCoop : StorageContainer
{
	public struct AnimalStatus
	{
		public EntityRef<FarmableAnimal> SpawnedAnimal;

		public TimeUntil TimeUntilHatch;

		public void CopyTo(ChickenStatus status, ThreadSafeTime time)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			status.spawnedChicken = SpawnedAnimal.uid;
			status.timeUntilHatch = ((TimeUntil)(ref TimeUntilHatch)).LeftFrom(time.Time);
		}

		public void CopyFrom(ChickenStatus status)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			SpawnedAnimal.uid = status.spawnedChicken;
			TimeUntilHatch = TimeUntil.op_Implicit(status.timeUntilHatch);
		}
	}

	public class ChickenCoopWorkQueue : ObjectWorkQueue<ChickenCoop>
	{
		protected override void RunJob(ChickenCoop entity)
		{
			entity.QueuedWorkJob();
		}
	}

	public Transform[] SpawnPoints;

	public GameObjectRef ChickenPrefab;

	public int MaxChickens = 4;

	public float ChickenHatchTimeMinutes = 30f;

	public float SunCheckRate = 10f;

	public Transform SunSampler;

	public List<AnimalStatus> Animals = new List<AnimalStatus>();

	public const Flags Hatching = Flags.Reserved1;

	public const Flags Full = Flags.Reserved3;

	public const int EggInsertSlot = 0;

	public const int FoodSlot = 1;

	public const int WaterSlot = 2;

	public const int FoodProductionSlot = 3;

	public GameObjectRef hatchEffect;

	private static ItemDefinition _eggDef;

	private float currentSunValue;

	public Plane MovementPlane;

	private Func<Item, int, bool> reservedSlotCallback;

	public static ChickenCoopWorkQueue CoopWorkQueue = new ChickenCoopWorkQueue();

	private static ItemDefinition EggDef
	{
		get
		{
			if ((Object)(object)_eggDef == (Object)null)
			{
				_eggDef = ItemManager.FindItemDefinition("egg");
			}
			return _eggDef;
		}
	}

	public bool IsInSun => currentSunValue > 0f;

	public bool IsOnTerrain { get; private set; }

	public Item CurrentFoodItem => base.inventory?.GetSlot(1);

	public Item CurrentWaterItem => base.inventory?.GetSlot(2);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ChickenCoop.OnRpcMessage"))
		{
			if (rpc == 3418655327u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RequestAnimalStats"));
				}
				using (TimeWarning.New("RequestAnimalStats"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3418655327u, "RequestAnimalStats", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3418655327u, "RequestAnimalStats", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RequestAnimalStats(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RequestAnimalStats");
					}
				}
				return true;
			}
			if (rpc == 1409078750 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SubmitEggForHatching"));
				}
				using (TimeWarning.New("SubmitEggForHatching"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1409078750u, "SubmitEggForHatching", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SubmitEggForHatching(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SubmitEggForHatching");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		InvokeRepeating(ScheduleWorkQueue, Random.Range(0f, SunCheckRate), SunCheckRate);
		if (GamePhysics.Trace(new Ray(((Component)this).transform.position + ((Component)this).transform.up, -((Component)this).transform.up), 0f, out var hitInfo, 1.1f, 10485760, (QueryTriggerInteraction)0))
		{
			IsOnTerrain = ColliderEx.IsOnLayer(((RaycastHit)(ref hitInfo)).collider, (Layer)23);
			UpdateMovementPlane();
		}
		if (reservedSlotCallback == null)
		{
			reservedSlotCallback = SlotIsReserved;
		}
		base.inventory.slotIsReserved = reservedSlotCallback;
	}

	public void UpdateMovementPlane()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!IsOnTerrain)
		{
			MovementPlane = new Plane(((Component)this).transform.up, ((Component)this).transform.position);
		}
	}

	public void SpawnFoodProduction(ItemDefinition def, int count)
	{
		try
		{
			base.inventory.slotIsReserved = null;
			Item item = ItemManager.Create(def, 1, 0uL, isServerSide: true, 0uL);
			if (!item.MoveToContainer(base.inventory, 3))
			{
				item.Remove();
			}
		}
		finally
		{
			base.inventory.slotIsReserved = reservedSlotCallback;
		}
	}

	private bool SlotIsReserved(Item item, int slot)
	{
		if (slot == 3)
		{
			return true;
		}
		return false;
	}

	private void ScheduleWorkQueue()
	{
		((ObjectWorkQueue<ChickenCoop>)CoopWorkQueue).Add(this);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void SubmitEggForHatching(RPCMessage msg)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (HasFlag(Flags.Reserved3) || HasFlag(Flags.Reserved1))
		{
			return;
		}
		Item slot = base.inventory.GetSlot(0);
		if (slot != null && !((Object)(object)slot.info != (Object)(object)EggDef) && !((Object)(object)msg.player.inventory.loot.entitySource != (Object)(object)this))
		{
			slot.UseItem();
			Animals.Add(new AnimalStatus
			{
				TimeUntilHatch = TimeUntil.op_Implicit(ChickenHatchTimeMinutes * 60f)
			});
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: true);
				flagsUpdateScope.Set(Flags.Reserved3, Animals.Count >= MaxChickens);
			}
			if (!IsInvoking(CheckEggHatchState))
			{
				InvokeRepeating(CheckEggHatchState, 10f, 10f);
			}
			SendNetworkUpdate();
		}
	}

	private void CheckEggHatchState()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		for (int i = 0; i < Animals.Count; i++)
		{
			AnimalStatus value = Animals[i];
			if (!value.SpawnedAnimal.IsSet && TimeUntil.op_Implicit(value.TimeUntilHatch) <= 0f)
			{
				FarmableAnimal farmableAnimal = SpawnChicken(i);
				value.SpawnedAnimal.Set(farmableAnimal);
				flag = true;
				Animals[i] = value;
				if (hatchEffect != null && hatchEffect.isValid)
				{
					Effect.server.Run(hatchEffect.resourcePath, ((Component)farmableAnimal).transform.position);
				}
			}
		}
		if (flag)
		{
			SetFlagLocal(Flags.Reserved1, b: false);
			CancelInvoke(CheckEggHatchState);
			SendNetworkUpdate();
		}
	}

	private FarmableAnimal SpawnChicken(int index)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		FarmableAnimal obj = base.gameManager.CreateEntity(ChickenPrefab.resourcePath, ((Component)this).transform.TransformPoint(GetRandomMovePoint()), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)) as FarmableAnimal;
		obj.SetParent(this, worldPositionStays: true);
		string text = RandomUsernames.Get(Random.Range(0, 1000));
		text = text[0].ToString().ToUpper() + text.Substring(1);
		obj.ApplyStartingStats(text);
		obj.Spawn();
		return obj;
	}

	public Vector3 GetRandomMovePoint()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (ConVar.Server.farmChickenLocalAvoidance)
		{
			int num = 10;
			for (int i = 0; i < num; i++)
			{
				Vector3 val = ((Component)this).transform.InverseTransformPoint(SpawnPoints[Random.Range(0, SpawnPoints.Length)].position);
				if (IsLocationClear(val, 0.25f))
				{
					return val;
				}
			}
		}
		return ((Component)this).transform.InverseTransformPoint(SpawnPoints[Random.Range(0, SpawnPoints.Length)].position);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.chickenCoop = Pool.Get<ChickenCoop>();
		info.msg.chickenCoop.chickens = Pool.Get<List<ChickenStatus>>();
		foreach (AnimalStatus animal in Animals)
		{
			ChickenStatus val = Pool.Get<ChickenStatus>();
			animal.CopyTo(val, info.cachedTime);
			info.msg.chickenCoop.chickens.Add(val);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (HasFlag(Flags.Reserved1) && !IsInvoking(CheckEggHatchState))
		{
			InvokeRepeating(CheckEggHatchState, 10f, 10f);
		}
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		if (targetSlot == 0 && (Object)(object)item.info != (Object)(object)EggDef)
		{
			return false;
		}
		if (targetSlot == 1 && !IsValidFoodItem(item))
		{
			return false;
		}
		switch (targetSlot)
		{
		case 2:
			return item.info.shortname == "water";
		case 3:
			if ((Object)(object)item.info != (Object)(object)EggDef)
			{
				return false;
			}
			break;
		}
		return base.ItemFilter(item, targetSlot);
	}

	private void QueuedWorkJob()
	{
		if (Animals.Count != 0)
		{
			UpdateSunValue();
		}
	}

	private void UpdateSunValue()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (TOD_Sky.Instance.IsNight)
		{
			currentSunValue = 0f;
			return;
		}
		Vector3 sunDirection = TOD_Sky.Instance.SunDirection;
		float num = Vector3.Dot(SunSampler.forward, sunDirection);
		currentSunValue = Mathf.InverseLerp(0.1f, 0.6f, num);
		if (currentSunValue > 0f && !CanSee(SunSampler.position, SunSampler.position + sunDirection * 100f))
		{
			currentSunValue = 0f;
		}
	}

	public void DebugFillCoop()
	{
		while (Animals.Count < MaxChickens)
		{
			AnimalStatus item = default(AnimalStatus);
			FarmableAnimal entity = SpawnChicken(Animals.Count);
			item.SpawnedAnimal.Set(entity);
			Animals.Add(item);
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, b: true);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	private void RequestAnimalStats(RPCMessage msg)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		ChickenCoopStatusUpdate val = Pool.Get<ChickenCoopStatusUpdate>();
		try
		{
			val.animals = Pool.Get<List<FarmableAnimalStatus>>();
			foreach (AnimalStatus animal in Animals)
			{
				FarmableAnimalStatus val2 = Pool.Get<FarmableAnimalStatus>();
				val2.data = Pool.Get<FarmableAnimal>();
				EntityRef<FarmableAnimal> spawnedAnimal = animal.SpawnedAnimal;
				if ((Object)(object)spawnedAnimal.Get(serverside: true) != (Object)null)
				{
					spawnedAnimal = animal.SpawnedAnimal;
					spawnedAnimal.Get(serverside: true).SaveToData(val2.data);
					spawnedAnimal = animal.SpawnedAnimal;
					val2.animal = spawnedAnimal.uid;
					val.animals.Add(val2);
				}
			}
			ClientRPC(RpcTarget.Player("OnReceivedChickenStats", player), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void OnAnimalDied(FarmableAnimal animal)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Animals.Count; i++)
		{
			if (Animals[i].SpawnedAnimal.uid == animal.net.ID)
			{
				Animals.RemoveAt(i);
				break;
			}
		}
		SetFlagLocal(Flags.Reserved3, Animals.Count >= MaxChickens);
		SendNetworkUpdate();
	}

	public override void DropItems(BaseEntity initiator = null)
	{
		base.inventory.GetSlot(2)?.Remove();
		base.DropItems(initiator);
	}

	public bool IsLocationClear(Vector3 pos, float radius, FarmableAnimal ignoreAnimal = null)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		foreach (AnimalStatus animal in Animals)
		{
			FarmableAnimal farmableAnimal = animal.SpawnedAnimal.Get(serverside: true);
			if ((Object)(object)farmableAnimal != (Object)null && (Object)(object)farmableAnimal != (Object)(object)ignoreAnimal)
			{
				Vector3 val = ((Component)farmableAnimal).transform.localPosition - pos;
				if (((Vector3)(ref val)).sqrMagnitude < radius * radius)
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool IsValidFoodItem(Item item)
	{
		ItemModConsumable itemModConsumable = default(ItemModConsumable);
		if (item != null && ((Component)item.info).TryGetComponent<ItemModConsumable>(ref itemModConsumable))
		{
			return itemModConsumable.chickenCoopFood;
		}
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		Animals.Clear();
		if (info.msg.chickenCoop == null || info.msg.chickenCoop.chickens == null)
		{
			return;
		}
		foreach (ChickenStatus chicken in info.msg.chickenCoop.chickens)
		{
			AnimalStatus item = default(AnimalStatus);
			item.CopyFrom(chicken);
			Animals.Add(item);
		}
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (Animals.Count != 0)
		{
			pickupErrorToFormat = (format: PickupErrors.ItemMustBeEmpty, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}
}
