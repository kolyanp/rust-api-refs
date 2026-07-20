using System.Collections.Generic;
using Facepunch;
using Rust;
using UnityEngine;

public class Igniter : IOEntity
{
	[Space]
	public float IgniteRange = 5f;

	public float IgniteFrequency = 1f;

	public float IgniteStartDelay;

	public Transform LineOfSightEyes;

	public float SelfDamagePerIgnite = 0.5f;

	[Space]
	public int PowerConsumption = 2;

	public override int ConsumptionAmount()
	{
		return PowerConsumption;
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		base.UpdateFromInput(inputAmount, inputSlot);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (inputAmount >= ConsumptionAmount() && CanIgnite())
		{
			InvokeRepeating(IgniteInRange, IgniteStartDelay, IgniteFrequency);
			flagsUpdateScope.Set(Flags.On, b: true);
			return;
		}
		if (IsInvoking(IgniteInRange))
		{
			CancelInvoke(IgniteInRange);
		}
		flagsUpdateScope.Set(Flags.On, b: false);
	}

	public override int DesiredPower(int inputIndex = 0)
	{
		if (!CanIgnite())
		{
			return 0;
		}
		return base.DesiredPower(inputIndex);
	}

	public override void OnRepair()
	{
		base.OnRepair();
		if (CanIgnite())
		{
			SendChangedToRoot(forceUpdate: true);
		}
	}

	public bool CanIgnite()
	{
		return base.healthFraction >= 0.1f;
	}

	private void IgniteInRange()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		Vis.Entities(LineOfSightEyes.position, IgniteRange, list, 1237019409, (QueryTriggerInteraction)2);
		foreach (BaseEntity item in list)
		{
			if (!item.HasFlag(Flags.On) && item.IsVisible(LineOfSightEyes.position))
			{
				if (item.isServer && item is BaseOven)
				{
					(item as BaseOven).StartCooking();
				}
				else if (item.isServer && item is IIgniteable igniteable && igniteable.CanIgnite())
				{
					igniteable.Ignite(((Component)this).transform.position);
				}
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
		Hurt(SelfDamagePerIgnite, DamageType.ElectricShock, this, useProtection: false);
		if (!CanIgnite())
		{
			SendChangedToRoot(forceUpdate: true);
		}
	}
}
