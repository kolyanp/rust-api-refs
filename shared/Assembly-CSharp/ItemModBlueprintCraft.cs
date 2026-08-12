using UnityEngine;

public class ItemModBlueprintCraft : ItemMod
{
	public static readonly Phrase CraftItemTitle;

	public static readonly Phrase CraftItemDesc;

	public static readonly Phrase CraftAllTitle;

	public static readonly Phrase CraftAllDesc;

	public GameObjectRef successEffect;

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)item.GetOwnerPlayer() != (Object)(object)player)
		{
			return;
		}
		if (command == "craft")
		{
			if (!item.IsBlueprint() || !player.inventory.crafting.CanCraft(item.blueprintTargetDef.Blueprint))
			{
				return;
			}
			Item fromTempBlueprint = item;
			if (item.amount > 1)
			{
				fromTempBlueprint = item.SplitItem(1);
			}
			player.inventory.crafting.CraftItem(item.blueprintTargetDef.Blueprint, player, null, 1, 0, fromTempBlueprint);
			if (successEffect.isValid)
			{
				Effect.server.Run(successEffect.resourcePath, player.eyes.position);
			}
		}
		if (command == "craft_all" && item.IsBlueprint() && player.inventory.crafting.CanCraft(item.blueprintTargetDef.Blueprint, item.amount))
		{
			player.inventory.crafting.CraftItem(item.blueprintTargetDef.Blueprint, player, null, item.amount, 0, item);
			if (successEffect.isValid)
			{
				Effect.server.Run(successEffect.resourcePath, player.eyes.position);
			}
		}
	}

	static ItemModBlueprintCraft()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		CraftItemTitle = new Phrase("craft_item", "Craft");
		CraftItemDesc = new Phrase("craft_item_desc", "Create the item the blueprint is referring to");
		CraftAllTitle = new Phrase("craft_all", "Craft All");
		CraftAllDesc = new Phrase("craft_all_desc", "Craft all available blueprints into items");
	}
}
