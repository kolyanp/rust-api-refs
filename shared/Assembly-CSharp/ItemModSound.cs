using Rust;
using UnityEngine;

public class ItemModSound : ItemMod
{
	public enum Type
	{
		OnAttachToWeapon
	}

	public GameObjectRef effect = new GameObjectRef();

	public Type actionType;

	public override void OnParentChanged(Item item)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isLoadingSave || actionType != Type.OnAttachToWeapon || item.parentItem == null || item.parentItem.info.category != ItemCategory.Weapon)
		{
			return;
		}
		ItemContainer rootContainer = item.parentItem.GetRootContainer();
		if (rootContainer != null)
		{
			BasePlayer ownerPlayer = rootContainer.GetOwnerPlayer();
			if (!((Object)(object)ownerPlayer == (Object)null) && !ownerPlayer.IsNpc)
			{
				Effect.server.Run(effect.resourcePath, ownerPlayer, 0u, Vector3.zero, Vector3.zero);
			}
		}
	}
}
