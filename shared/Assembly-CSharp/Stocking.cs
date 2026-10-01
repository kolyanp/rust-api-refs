using Oxide.Core;
using Rust;
using UnityEngine;

public class Stocking : LootContainer
{
	public static ListHashSet<Stocking> stockings;

	public override void ServerInit()
	{
		base.ServerInit();
		if (stockings == null)
		{
			stockings = new ListHashSet<Stocking>();
		}
		stockings.Add(this);
	}

	internal override void DoServerDestroy()
	{
		stockings.Remove(this);
		base.DoServerDestroy();
	}

	public bool IsEmpty()
	{
		if (inventory == null)
		{
			return false;
		}
		for (int num = inventory.itemList.Count - 1; num >= 0; num--)
		{
			if (inventory.itemList[num] != null)
			{
				return false;
			}
		}
		return true;
	}

	public override void SpawnLoot()
	{
		if (inventory == null)
		{
			Debug.Log((object)("CONTACT DEVELOPERS! Stocking::PopulateLoot has null inventory!!! " + ((Object)this).name));
		}
		else if (IsEmpty() && Interface.CallHook("OnXmasStockingFill", this) == null)
		{
			base.SpawnLoot();
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: true);
			}
			Hurt(MaxHealth() * 0.1f, DamageType.Generic, null, useProtection: false);
		}
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		base.PlayerStoppedLooting(player);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		if (IsEmpty() && healthFraction <= 0.1f)
		{
			Hurt(health, DamageType.Generic, this, useProtection: false);
		}
	}
}
