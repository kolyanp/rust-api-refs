using Rust;
using UnityEngine;

public class CookingWorkbench : MixingTable
{
	public GameObjectRef SubOvenPrefab;

	public Transform SubOvenPosition;

	public const Flags OvenCooking = Flags.Reserved9;

	public const Flags MixingTea = Flags.Reserved10;

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (!isServer || (next & Flags.On) == Flags.On == ((old & Flags.On) == Flags.On))
		{
			return;
		}
		ItemDefinition itemDefinition = (((Object)(object)currentRecipe != (Object)null) ? currentRecipe.ProducedItem : currentProductionItem);
		if ((Object)(object)itemDefinition == (Object)null)
		{
			return;
		}
		bool flag = GetChildBbq().IsOn();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		ItemModCookingWorkbench itemModCookingWorkbench = default;
		if ((Object)(object)itemDefinition != (Object)null && ((Component)itemDefinition).TryGetComponent<ItemModCookingWorkbench>(ref itemModCookingWorkbench))
		{
			flagsUpdateScope.Set(Flags.Reserved9, ((next & Flags.On) == Flags.On) | flag);
		}
		else
		{
			flagsUpdateScope.Set(Flags.Reserved10, (next & Flags.On) == Flags.On);
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		SpawnOven();
	}

	private void SpawnOven()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)GetChildBbq() != (Object)null) && SubOvenPrefab.isValid)
		{
			BaseEntity baseEntity = gameManager.CreateEntity(SubOvenPrefab.resourcePath, SubOvenPosition.position, SubOvenPosition.rotation);
			baseEntity.SetParent(this, worldPositionStays: true);
			((Component)baseEntity).transform.localPosition = SubOvenPosition.localPosition;
			((Component)baseEntity).transform.localRotation = SubOvenPosition.localRotation;
			baseEntity.OwnerID = OwnerID;
			baseEntity.Spawn();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!Application.isLoadingSave)
		{
			SpawnOven();
		}
	}

	private CookingWorkbenchBbq GetChildBbq()
	{
		foreach (BaseEntity child in children)
		{
			if (child is CookingWorkbenchBbq result)
			{
				return result;
			}
		}
		return null;
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		CookingWorkbenchBbq childBbq = GetChildBbq();
		if ((Object)(object)childBbq != (Object)null && childBbq.inventory != null && !childBbq.inventory.IsEmpty())
		{
			pickupErrorToFormat = (format: PickupErrors.ItemInventoryMustBeEmpty, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	internal override void DoServerDestroy()
	{
		CookingWorkbenchBbq childBbq = GetChildBbq();
		if ((Object)(object)childBbq != (Object)null)
		{
			childBbq.DropItems();
			childBbq.Kill();
		}
		base.DoServerDestroy();
	}
}
