using UnityEngine;

public class Chicken : BaseAnimalNPC
{
	public class EggDropWorkQueue : ObjectWorkQueue<Chicken>
	{
		protected override void RunJob(Chicken entity)
		{
			if (((ObjectWorkQueue<Chicken>)this).ShouldAdd(entity))
			{
				entity.CheckEggDrop();
			}
		}

		protected override bool ShouldAdd(Chicken entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	[ServerVar(Help = "Population active on the server, per square km", ShowInAdminUI = true)]
	public static float Population = 3f;

	public ItemDefinition EggDefinition;

	public float EggDropFrequency = 30f;

	public Vector3 EggDropLocalPos;

	public static EggDropWorkQueue EggWorkQueue = new EggDropWorkQueue();

	public override float RealisticMass => 3f;

	public override TraitFlag Traits => TraitFlag.Alive | TraitFlag.Animal | TraitFlag.Food | TraitFlag.Meat;

	public override void ServerInit()
	{
		base.ServerInit();
		if ((Object)(object)EggDefinition != (Object)null)
		{
			InvokeRandomized(QueueEggDropCheck, EggDropFrequency, EggDropFrequency, EggDropFrequency * 0.5f);
		}
	}

	private void QueueEggDropCheck()
	{
		((ObjectWorkQueue<Chicken>)EggWorkQueue).Add(this);
	}

	public override bool WantsToEat(BaseEntity best)
	{
		if (best.HasTrait(TraitFlag.Alive))
		{
			return false;
		}
		if (best.HasTrait(TraitFlag.Meat))
		{
			return false;
		}
		CollectibleEntity collectibleEntity = best as CollectibleEntity;
		if ((Object)(object)collectibleEntity != (Object)null)
		{
			ItemAmount[] itemList = collectibleEntity.itemList;
			for (int i = 0; i < itemList.Length; i++)
			{
				if (itemList[i].itemDef.category == ItemCategory.Food)
				{
					return true;
				}
			}
		}
		return base.WantsToEat(best);
	}

	private void CheckEggDrop()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer && Random.Range(0, 100) > 50 && (Object)(object)((Component)this).transform != (Object)null && BaseNetworkable.HasCloseConnections(((Component)this).transform.position, 100f))
		{
			SpawnEgg();
		}
	}

	public void SpawnEgg()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)EggDefinition != (Object)null)
		{
			ItemManager.Create(EggDefinition, 1, 0uL, isServerSide: true, 0uL).Drop(((Component)this).transform.TransformPoint(EggDropLocalPos), -((Component)this).transform.forward);
		}
	}

	public override string Categorize()
	{
		return "Chicken";
	}
}
