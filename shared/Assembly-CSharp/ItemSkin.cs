using Rust.Workshop;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Skins/ItemSkin")]
public class ItemSkin : SteamInventoryItem
{
	public Skinnable Skinnable;

	public Material[] Materials;

	[Tooltip("If set, whenever we make an item with this skin, we'll spawn this item without a skin instead")]
	public ItemDefinition Redirect;

	[Tooltip("(overriden by Redirect) If set, this is the icon that will be used in-game (spray can, crafting menu, repair bench). Allows you to have different icons for the store and in-game (see halloween wallpapers as an example)")]
	public Sprite inGameIcon;

	public bool UnlockedByDefault;

	public void ApplySkin(GameObject obj)
	{
		if (!((Object)(object)Skinnable == (Object)null))
		{
			Skin.Apply(obj, Skinnable, Materials);
		}
	}

	public override bool HasUnlocked(BasePlayer player)
	{
		if (UnlockedByDefault)
		{
			return true;
		}
		if (!player.DefaultSkinAccess)
		{
			return player.AllSkinsUnlocked;
		}
		if ((Object)(object)Redirect != (Object)null && (Object)(object)Redirect.isRedirectOf != (Object)null && (Object)(object)Redirect.isRedirectOf.steamItem != (Object)null && (Object)(object)player != (Object)null && player.blueprints.CheckSkinOwnership(Redirect.isRedirectOf.steamItem.id, player))
		{
			return true;
		}
		return base.HasUnlocked(player);
	}
}
