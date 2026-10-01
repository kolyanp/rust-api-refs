using UnityEngine;

namespace Rust.Ai.Gen2;

[SoftRequireComponent(typeof(BasicAnimalFsm))]
public class Chicken : BaseNPC2
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

	private void CheckEggDrop()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (isServer && Random.Range(0, 100) > 50 && (Object)(object)((Component)this).transform != (Object)null && BaseNetworkable.HasCloseConnections(((Component)this).transform.position, 100f))
		{
			SpawnEgg();
		}
	}

	public void SpawnEgg()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)EggDefinition != (Object)null)
		{
			ItemManager.Create(EggDefinition, 1, 0uL, isServerSide: true, 0uL).DropAtRest(((Component)this).transform.TransformPoint(EggDropLocalPos));
		}
	}

	public override string Categorize()
	{
		return "Chicken";
	}
}
