using System;
using UnityEngine;

public class CatapultAmmoContainer : StorageContainer
{
	[NonSerialized]
	public Catapult catapult;

	public override void OnItemAddedOrRemoved(Item item, bool added, BasePlayer sourcePlayer)
	{
		base.OnItemAddedOrRemoved(item, added, sourcePlayer);
		catapult.UpdateLoadedAmmo(item, added);
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if ((Object)(object)catapult != (Object)null)
		{
			return catapult.CanBeLooted(player);
		}
		return false;
	}
}
