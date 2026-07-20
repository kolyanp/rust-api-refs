using ProtoBuf;
using UnityEngine;

public class ItemModPaintballOveralls : ItemModSpriteConfig
{
	public override void OnParentChanged(Item item)
	{
		if (!item.isServer)
		{
			return;
		}
		ItemContainer rootContainer = item.GetRootContainer();
		if (rootContainer != null)
		{
			BasePlayer ownerPlayer = rootContainer.GetOwnerPlayer();
			if (!((Object)(object)ownerPlayer == (Object)null) && (Object)(object)ownerPlayer.inventory != (Object)null && ownerPlayer.inventory.containerWear != null && ownerPlayer.inventory.containerWear == item.parent)
			{
				OnWorn(item, ownerPlayer);
			}
		}
	}

	private void OnWorn(Item item, BasePlayer player)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		if (player.TryGetHeldEntity(out PaintballGun _))
		{
			if (item.instanceData == null)
			{
				item.instanceData = new InstanceData();
				item.instanceData.ShouldPool = false;
			}
			item.instanceData.dataInt = player.server_paintballColor;
		}
		else
		{
			player.Server_UpdatePaintballColor(item.instanceData?.dataInt ?? 0);
		}
		item.MarkDirty();
	}
}
