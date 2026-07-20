using System;
using ConVar;

public class GameModeHardcore : GameModeVanilla
{
	protected override void OnCreated()
	{
		base.OnCreated();
	}

	public override void InitShared()
	{
		base.InitShared();
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
	}

	protected override float GetCraftingCostConVar(CraftingCostConVar conVar)
	{
		if (conVar == CraftingCostConVar.HardcoreFirearmAmmunition)
		{
			return Server.hardcoreFirearmAmmunitionCraftingMultiplier;
		}
		return base.GetCraftingCostConVar(conVar);
	}

	public override void ServerInit()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is Recycler recycler)
				{
					recycler.UpdateInSafeZone();
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
