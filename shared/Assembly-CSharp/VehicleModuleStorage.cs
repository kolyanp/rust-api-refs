using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class VehicleModuleStorage : VehicleModuleSeating
{
	[Serializable]
	public class Storage
	{
		public GameObjectRef storageUnitPrefab;

		public Transform storageUnitPoint;
	}

	[SerializeField]
	private Storage storage;

	[SerializeField]
	private GameObjectRef crudeExplosionEntity;

	[SerializeField]
	private GameObjectRef crudeExplosionEffect;

	[SerializeField]
	private float explosionThreshold = 50f;

	[SerializeField]
	private float minimumTimeBetweenExplosions = 5f;

	[SerializeField]
	private int oilToConsumePerExplosion = 100;

	private EntityRef storageUnitInstance;

	private static ItemDefinition _crudeItem;

	public static readonly Phrase StorageCantBeMovedError;

	private TimeSince lastExplosionSpawned;

	public static ItemDefinition CrudeItem => _crudeItem ?? (_crudeItem = ItemManager.FindItemDefinition("crude.oil"));

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("VehicleModuleStorage.OnRpcMessage"))
		{
			if (rpc == 4254195175u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Open"));
				}
				using (TimeWarning.New("RPC_Open"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4254195175u, "RPC_Open", this, player, 3f))
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
							RPC_Open(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_Open");
					}
				}
				return true;
			}
			if (rpc == 425471188 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_TryOpenWithKeycode"));
				}
				using (TimeWarning.New("RPC_TryOpenWithKeycode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(425471188u, "RPC_TryOpenWithKeycode", this, player, 3f))
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
							RPC_TryOpenWithKeycode(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_TryOpenWithKeycode");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public IItemContainerEntity GetContainer()
	{
		BaseEntity baseEntity = storageUnitInstance.Get(base.isServer);
		if ((Object)(object)baseEntity != (Object)null && baseEntity.IsValid())
		{
			return baseEntity as IItemContainerEntity;
		}
		return null;
	}

	public override void Load(LoadInfo info)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		storageUnitInstance.uid = info.msg.simpleUID.uid;
	}

	public BaseEntity GetStorageUnitInstance()
	{
		return storageUnitInstance.Get(base.isServer);
	}

	public override void Spawn()
	{
		base.Spawn();
		if (!Application.isLoadingSave && ((Component)storage.storageUnitPoint).gameObject.activeSelf)
		{
			CreateStorageEntity();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		IItemContainerEntity container = GetContainer();
		if (!ObjectEx.IsUnityNull(container))
		{
			InitStorageEvents(container.inventory);
		}
	}

	private void OnItemAddedRemoved(Item item, bool add)
	{
		AssociatedItemInstance?.LockUnlock(!CanBeMovedNowOnVehicle());
	}

	internal override void DoServerDestroy()
	{
		if (vehicle.vehiclesdroploot)
		{
			IItemContainerEntity container = GetContainer();
			if (!ObjectEx.IsUnityNull(container))
			{
				container.DropItems();
			}
		}
		base.DoServerDestroy();
	}

	public override void Save(SaveInfo info)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.simpleUID = Pool.Get<SimpleUID>();
		info.msg.simpleUID.uid = storageUnitInstance.uid;
	}

	[UnityEvent]
	public void CreateStorageEntity()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (IsFullySpawned() && base.isServer && !storageUnitInstance.IsValid(base.isServer))
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(storage.storageUnitPrefab.resourcePath, storage.storageUnitPoint.localPosition, storage.storageUnitPoint.localRotation);
			storageUnitInstance.Set(baseEntity);
			baseEntity.SetParent(this);
			baseEntity.Spawn();
			InitStorageEvents(GetContainer().inventory);
		}
	}

	protected virtual void InitStorageEvents(ItemContainer container)
	{
		container.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(container.onItemAddedRemoved, new Action<Item, bool>(OnItemAddedRemoved));
	}

	[UnityEvent]
	public void DestroyStorageEntity()
	{
		if (!IsFullySpawned() || !base.isServer)
		{
			return;
		}
		BaseEntity baseEntity = storageUnitInstance.Get(base.isServer);
		if (baseEntity.IsValid())
		{
			if (baseEntity is BaseCombatEntity baseCombatEntity)
			{
				baseCombatEntity.Die();
			}
			else
			{
				baseEntity.Kill();
			}
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_Open(RPCMessage msg)
	{
		TryOpen(msg.player);
	}

	private bool TryOpen(BasePlayer player)
	{
		if (!player.IsValid() || !CanBeLooted(player))
		{
			return false;
		}
		IItemContainerEntity container = GetContainer();
		if (!ObjectEx.IsUnityNull(container))
		{
			container.PlayerOpenLoot(player);
		}
		else
		{
			Debug.LogError((object)(((object)this).GetType().Name + ": No container component found."));
		}
		return true;
	}

	protected override bool CanBeMovedNowOnVehicle()
	{
		IItemContainerEntity container = GetContainer();
		if (!ObjectEx.IsUnityNull(container) && !container.inventory.IsEmpty())
		{
			return false;
		}
		return true;
	}

	public override Phrase CannotBeMovedNowReason()
	{
		return StorageCantBeMovedError;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_TryOpenWithKeycode(RPCMessage msg)
	{
		if (!base.IsOnACar)
		{
			return;
		}
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null))
		{
			string codeEntered = msg.read.String();
			if (base.Car.CarLock.TryOpenWithCode(player, codeEntered))
			{
				TryOpen(player);
			}
			else
			{
				base.Car.ClientRPC(RpcTarget.NetworkGroup("CodeEntryFailed"));
			}
		}
	}

	public override void Hurt(HitInfo info)
	{
		TrySpawnExplosion(info);
		base.Hurt(info);
	}

	private void TrySpawnExplosion(HitInfo info)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		if (!(TimeSince.op_Implicit(lastExplosionSpawned) > minimumTimeBetweenExplosions) || !(info.damageTypes.Get(DamageType.Explosion) > explosionThreshold) || !(storageUnitInstance.Get(serverside: true) is LiquidContainer liquidContainer))
		{
			return;
		}
		Item liquidItem = liquidContainer.GetLiquidItem();
		if (liquidItem == null || !(liquidItem.info.shortname == "crude.oil") || liquidItem.amount < oilToConsumePerExplosion)
		{
			return;
		}
		liquidItem.UseItem(oilToConsumePerExplosion);
		lastExplosionSpawned = TimeSince.op_Implicit(0f);
		if (!crudeExplosionEntity.isValid)
		{
			return;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(crudeExplosionEntity.resourcePath, ((Component)this).transform.position, ((Component)this).transform.rotation);
		if (!((Object)(object)baseEntity == (Object)null))
		{
			ServerProjectile component = ((Component)baseEntity).GetComponent<ServerProjectile>();
			baseEntity.Spawn();
			TimedExplosive timedExplosive = default(TimedExplosive);
			if ((Object)(object)component != (Object)null && ((Component)component).TryGetComponent<TimedExplosive>(ref timedExplosive))
			{
				timedExplosive.creatorEntity = creatorEntity;
				timedExplosive.Explode();
			}
			if (crudeExplosionEffect.isValid)
			{
				Effect.server.Run(crudeExplosionEffect.resourcePath, ((Component)baseEntity).transform.position);
			}
		}
	}

	static VehicleModuleStorage()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		_crudeItem = null;
		StorageCantBeMovedError = new Phrase("error.itemsinstorage", "Cannot move item: Storage contains items!");
	}
}
