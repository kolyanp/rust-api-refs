using UnityEngine;

public class ItemModEntity : ItemMod
{
	public GameObjectRef entityPrefab = new GameObjectRef();

	public string defaultBone;

	public bool playerOnlyEntity;

	public bool destroyEntityWhenBroken;

	public override void OnChanged(Item item)
	{
		HeldEntity heldEntity = item.GetHeldEntity() as HeldEntity;
		if ((Object)(object)heldEntity != (Object)null)
		{
			heldEntity.OnItemChanged(item);
		}
		base.OnChanged(item);
	}

	public override void OnItemCreated(Item item)
	{
		if ((Object)(object)item.GetHeldEntity() == (Object)null && !playerOnlyEntity)
		{
			CreateEntity(item);
		}
	}

	private void CreateEntity(Item item)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!destroyEntityWhenBroken || !item.isBroken)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(entityPrefab.resourcePath);
			if ((Object)(object)baseEntity == (Object)null)
			{
				Debug.LogWarning((object)("Couldn't create item entity " + item.info.displayName.english + " (" + entityPrefab.resourcePath + ")"));
			}
			else
			{
				baseEntity.skinID = item.skin;
				baseEntity.attachmentID = item.attachment;
				baseEntity.limitNetworking = true;
				baseEntity.Spawn();
				item.SetHeldEntity(baseEntity);
			}
		}
	}

	public override void OnRemove(Item item)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity heldEntity = item.GetHeldEntity();
		if (!((Object)(object)heldEntity == (Object)null))
		{
			if (heldEntity.prefabID != entityPrefab.resourceID)
			{
				Debug.LogError((object)string.Format("Held entity for item '{0}' has prefab '{1}' ({2}) but expected '{3}'!!", new object[4]
				{
					item.info.shortname,
					heldEntity.PrefabName,
					heldEntity.net.ID,
					entityPrefab.resourcePath
				}), (Object)(object)heldEntity);
			}
			heldEntity.Kill();
			item.SetHeldEntity(null);
		}
	}

	private bool ParentToParent(Item item, BaseEntity ourEntity)
	{
		Item parentItem = item.parentItem;
		if (parentItem == null)
		{
			return false;
		}
		if (parentItem.IsBackpack())
		{
			return false;
		}
		BaseEntity baseEntity = parentItem.GetWorldEntity();
		if ((Object)(object)baseEntity == (Object)null)
		{
			baseEntity = item.parentItem.GetHeldEntity();
		}
		using (BaseEntity.FlagsUpdateScope flagsUpdateScope = ourEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(BaseEntity.Flags.Disabled, b: false);
		}
		ourEntity.limitNetworking = false;
		ourEntity.SetParent(baseEntity, defaultBone);
		return true;
	}

	private bool ParentToPlayer(Item item, BaseEntity ourEntity)
	{
		HeldEntity heldEntity = ourEntity as HeldEntity;
		if ((Object)(object)heldEntity == (Object)null)
		{
			return false;
		}
		BasePlayer basePlayer = item.GetRootContainer()?.GetOwnerPlayer();
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			heldEntity.SetOwnerPlayer(basePlayer);
			return true;
		}
		heldEntity.ClearOwnerPlayer();
		return true;
	}

	public override void OnParentChanged(Item item)
	{
		BaseEntity baseEntity = item.GetHeldEntity();
		if (playerOnlyEntity)
		{
			BasePlayer ownerPlayer = item.GetOwnerPlayer();
			if ((Object)(object)ownerPlayer == (Object)null && (Object)(object)baseEntity != (Object)null)
			{
				baseEntity.Kill();
				baseEntity = null;
				item.SetHeldEntity(null);
			}
			else if ((Object)(object)ownerPlayer != (Object)null && (Object)(object)baseEntity == (Object)null)
			{
				CreateEntity(item);
				baseEntity = item.GetHeldEntity();
			}
		}
		if ((Object)(object)baseEntity == (Object)null || ParentToParent(item, baseEntity) || ParentToPlayer(item, baseEntity))
		{
			return;
		}
		baseEntity.SetParent(null);
		baseEntity.limitNetworking = true;
		using BaseEntity.FlagsUpdateScope flagsUpdateScope = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(BaseEntity.Flags.Disabled, b: true);
	}

	public override void CollectedForCrafting(Item item, BasePlayer crafter)
	{
		BaseEntity heldEntity = item.GetHeldEntity();
		if (!((Object)(object)heldEntity == (Object)null))
		{
			HeldEntity heldEntity2 = heldEntity as HeldEntity;
			if (!((Object)(object)heldEntity2 == (Object)null))
			{
				heldEntity2.CollectedForCrafting(item, crafter);
			}
		}
	}

	public override void ReturnedFromCancelledCraft(Item item, BasePlayer crafter)
	{
		BaseEntity heldEntity = item.GetHeldEntity();
		if (!((Object)(object)heldEntity == (Object)null))
		{
			HeldEntity heldEntity2 = heldEntity as HeldEntity;
			if (!((Object)(object)heldEntity2 == (Object)null))
			{
				heldEntity2.ReturnedFromCancelledCraft(item, crafter);
			}
		}
	}
}
