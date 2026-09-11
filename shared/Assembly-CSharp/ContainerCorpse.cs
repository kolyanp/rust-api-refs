using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class ContainerCorpse : ConstructableEntity
{
	[Header("Container Corpse")]
	public Collider mainCollider;

	public const Flags Flag_Empty = Flags.Reserved13;

	private CodeLock codelockData;

	private KeyLock keylockData;

	private ulong lockOwnerId;

	private BuildingPrivlidge _cachedTc;

	private float _cacheTimeout;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ContainerCorpse.OnRpcMessage"))
		{
			if (rpc == 1735184033 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_RequestOwnerData"));
				}
				using (TimeWarning.New("SERVER_RequestOwnerData"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1735184033u, "SERVER_RequestOwnerData", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1735184033u, "SERVER_RequestOwnerData", this, player, 3f))
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
							SERVER_RequestOwnerData(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_RequestOwnerData");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static bool IsValidPointForEntity(uint prefabID, Vector3 point, Quaternion rotation, BaseEntity ignoredEntity = null, int mask = 536870912, bool ignoreChildrenOfEntity = false)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		DeployVolume[] array = null;
		array = PrefabAttribute.server.FindAll<DeployVolume>(prefabID);
		PooledList<Type> val = Pool.Get<PooledList<Type>>();
		try
		{
			((List<Type>)(object)val).Add(typeof(DebrisEntity));
			if (DeployVolume.Check(point, rotation, array, (List<Type>)(object)val, DeployVolume.TypeFilterMode.Ignore, ignoredEntity, mask, ignoreChildrenOfEntity))
			{
				return false;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		Socket_Base[] array2 = PrefabAttribute.server.FindAll<Socket_Base>(prefabID);
		Construction.Target target = new Construction.Target
		{
			position = point,
			rotation = ((Quaternion)(ref rotation)).eulerAngles,
			onTerrain = (Mathf.Abs(TerrainMeta.HeightMap.GetHeight(point) - point.y) < 0.05f)
		};
		Construction.Placement placement = new Construction.Placement(target)
		{
			position = target.position,
			rotation = rotation
		};
		Socket_Base[] array3 = array2;
		foreach (Socket_Base socket_Base in array3)
		{
			if (socket_Base.male && !socket_Base.CheckSocketMods(ref placement))
			{
				return false;
			}
		}
		return true;
	}

	public bool InValidPosition()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		int mask = (IsNearlyBuilt() ? 537001984 : 536870912);
		return IsValidPointForEntity(entityToSpawn.resourceID, ((Component)this).transform.position, ((Component)this).transform.rotation, this, mask);
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		bool flag = false;
		if (base.isServer)
		{
			flag = IsOwner(player);
		}
		if (flag && pickup.enabled)
		{
			if (pickup.requireHammer)
			{
				return player.IsHoldingEntity<Hammer>();
			}
			return true;
		}
		return false;
	}

	public override bool HasSlot(Slot slot)
	{
		if (slot == Slot.Lock)
		{
			return false;
		}
		return base.HasSlot(slot);
	}

	public override void OnInventoryFirstCreated(ItemContainer container)
	{
		base.OnInventoryFirstCreated(container);
		base.inventory.SetFlag(ItemContainer.Flag.NoItemInput, b: true);
	}

	public void TakeFrom(ItemContainer[] source, float savePercent = 0f)
	{
		DroppedItemContainer.TakeFractionOfItems(source, base.inventory, savePercent);
		base.inventory.capacity = base.inventory.itemList.Count;
	}

	protected override bool CanRepair(BasePlayer player)
	{
		if (!RunBuildingChecks(player))
		{
			return false;
		}
		bool flag = IsOwner(player);
		if (flag && !InValidPosition())
		{
			if ((Object)(object)DeployVolume.LastDeployHit != (Object)null)
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(DeployVolume.LastDeployHit);
				if ((Object)(object)baseEntity != (Object)null)
				{
					player.ShowBlockedByEntityToast(baseEntity, Construction.lastPlacementError);
				}
			}
			else
			{
				player.ShowToast(GameTip.Styles.Error, Construction.lastPlacementError, false);
			}
			return false;
		}
		if (flag)
		{
			return base.CanRepair(player);
		}
		return false;
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (isLootable)
		{
			return IsOwner(player);
		}
		return false;
	}

	private bool RunBuildingChecks(BasePlayer player)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = entityToSpawn.Get();
		BaseEntity baseEntity = ((obj != null) ? obj.GetComponent<BaseEntity>() : null);
		if ((Object)(object)baseEntity == (Object)null)
		{
			Debug.LogError((object)("Prefab not found for '" + entityToSpawn.resourcePath + "'"));
			return false;
		}
		Construction construction = PrefabAttribute.server.Find<Construction>(baseEntity.prefabID);
		if (construction == null)
		{
			Debug.LogError((object)("Construction not found on '" + entityToSpawn.resourcePath + "'"));
			return false;
		}
		if (construction.isBuildingPrivilege)
		{
			BuildingPrivlidge componentInChildren = ((Component)this).GetComponentInChildren<BuildingPrivlidge>();
			if (!player.CanPlaceBuildingPrivilege(((Component)this).transform.position, ((Component)this).transform.rotation, construction.bounds, componentInChildren))
			{
				player.ShowToast(GameTip.Styles.Red_Normal, Phrase.op_Implicit("Can't stack building privileges"), false);
				return false;
			}
		}
		return true;
	}

	public override void OnRepairFinished(BasePlayer player)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = GameManager.server.CreateEntity(entityToSpawn.resourcePath, ((Component)this).transform.position, ((Component)this).transform.rotation);
		if (HasParent())
		{
			baseEntity.SetParent(GetParentEntity(), worldPositionStays: true);
		}
		baseEntity.OwnerID = base.OwnerID;
		baseEntity.skinID = skinID;
		baseEntity.Spawn();
		if (baseEntity is StorageContainer storageContainer)
		{
			storageContainer.MoveAllInventoryItems(base.inventory);
		}
		else if (baseEntity is ContainerIOEntity containerIOEntity)
		{
			containerIOEntity.MoveAllInventoryItems(base.inventory);
		}
		if (baseEntity is BuildingPrivlidge buildingPrivlidge)
		{
			BuildingPrivlidge componentInChildren = ((Component)this).GetComponentInChildren<BuildingPrivlidge>();
			if ((Object)(object)componentInChildren == (Object)null)
			{
				Debug.LogError((object)("Can't copy auth list from corpse to TC: no BuildingPrivilege found in '" + base.PrefabName + "'"));
			}
			else
			{
				buildingPrivlidge.SetAuthListFrom(componentInChildren);
			}
		}
		if (baseEntity is DecayEntity decayEntity)
		{
			decayEntity.AttachToBuilding(null);
		}
		SpawnLock(baseEntity);
		Kill();
		baseEntity.SendNetworkUpdateImmediate();
		if (spawnEffect.isValid)
		{
			Effect.server.Run(spawnEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
		}
	}

	public void SaveLock(BaseLock lockEntity)
	{
		SaveInfo info = new SaveInfo
		{
			forDisk = true,
			cachedTime = ThreadSafeTime.TakeSnapshot()
		};
		Entity val = (info.msg = Pool.Get<Entity>());
		try
		{
			lockEntity.Save(info);
			CodeLock codeLock = info.msg.codeLock;
			codelockData = ((codeLock != null) ? codeLock.Copy() : null);
			KeyLock keyLock = info.msg.keyLock;
			keylockData = ((keyLock != null) ? keyLock.Copy() : null);
			lockOwnerId = lockEntity.OwnerID;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void SpawnLock(BaseEntity parent)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		BaseEntity baseEntity;
		if (codelockData != null)
		{
			baseEntity = GameManager.server.CreateEntity("Assets/Prefabs/Locks/Keypad/lock.code.prefab") as CodeLock;
			flag = codelockData.pv != null && !string.IsNullOrEmpty(codelockData.pv.code);
		}
		else
		{
			if (keylockData == null)
			{
				return;
			}
			baseEntity = GameManager.server.CreateEntity("Assets/Prefabs/Locks/keylock/lock.key.prefab") as KeyLock;
			flag = true;
		}
		baseEntity.SetParent(parent, parent.GetSlotAnchorName(Slot.Lock));
		baseEntity.OwnerID = lockOwnerId;
		if (baseEntity is CodeLock codeLock && codelockData != null)
		{
			codeLock.LoadCodelockPrivateData(codelockData);
		}
		else if (baseEntity is KeyLock keyLock && keylockData != null)
		{
			keyLock.LoadKeylockData(keylockData);
		}
		baseEntity.SetFlagLocal(Flags.Locked, flag);
		baseEntity.Spawn();
		parent.SetSlot(Slot.Lock, baseEntity);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if (base.inventory.IsEmpty())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved13, b: true);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	private void SERVER_RequestOwnerData(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && IsOwner(player))
		{
			SendOwnerData(player);
		}
	}

	public virtual void SendOwnerData(BasePlayer player)
	{
		ClientRPC(RpcTarget.Player("CLIENT_ReceiveOwnerData", player));
	}

	public bool IsOwner(BasePlayer player)
	{
		if ((ulong)player.userID == base.OwnerID)
		{
			return true;
		}
		BuildingPrivlidge cachedTc = GetCachedTc();
		if ((Object)(object)cachedTc != (Object)null && cachedTc.IsAuthed(player))
		{
			return true;
		}
		return false;
	}

	private BuildingPrivlidge GetCachedTc()
	{
		if ((Object)(object)_cachedTc != (Object)null && _cachedTc.IsDestroyed)
		{
			_cachedTc = null;
		}
		if ((Object)(object)_cachedTc == (Object)null || Time.realtimeSinceStartup > _cacheTimeout)
		{
			_cachedTc = null;
			BuildingManager.Building building = GetBuilding();
			if (building != null)
			{
				_cachedTc = building.GetDominatingBuildingPrivilege();
			}
			if ((Object)(object)_cachedTc == (Object)null)
			{
				return GetNearestBuildingPrivilege(cached: true, 3f);
			}
			_cacheTimeout = Time.realtimeSinceStartup + 3f;
		}
		return _cachedTc;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			if (base.inventory != null)
			{
				info.msg.storageBox = Pool.Get<StorageBox>();
				info.msg.storageBox.contents = base.inventory.Save();
			}
			ContainerCorpseData val = Pool.Get<ContainerCorpseData>();
			info.msg.containerCorpse = val;
			val.codeLock = codelockData;
			val.keyLock = keylockData;
			val.lockOwnerId = lockOwnerId;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.containerCorpse != null)
		{
			CodeLock codeLock = info.msg.containerCorpse.codeLock;
			codelockData = ((codeLock != null) ? codeLock.Copy() : null);
			KeyLock keyLock = info.msg.containerCorpse.keyLock;
			keylockData = ((keyLock != null) ? keyLock.Copy() : null);
			lockOwnerId = info.msg.containerCorpse.lockOwnerId;
		}
	}
}
