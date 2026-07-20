using UnityEngine;

namespace ConVar;

[Factory("craft")]
public class Craft : ConsoleSystem
{
	[ServerVar(Help = "(Generated) When enabled, all crafting completes instantly with no time delay; useful for testing crafting recipes or quickly equipping items in development")]
	public static bool instant;

	[ServerUserVar]
	public static void add(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!Object.op_Implicit((Object)(object)basePlayer) || basePlayer.IsDead())
		{
			return;
		}
		int num = args.GetInt(0);
		int num2 = args.GetInt(1, 1);
		int num3 = (int)args.GetUInt64(2, 0uL);
		bool flag = args.GetBool(3);
		int num4 = (int)args.GetUInt64(4, 0uL);
		if (num2 < 1)
		{
			return;
		}
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(num);
		if ((Object)(object)itemDefinition == (Object)null)
		{
			args.ReplyWith("Item not found");
			return;
		}
		ItemBlueprint itemBlueprint = ItemManager.FindBlueprint(itemDefinition);
		if (!Object.op_Implicit((Object)(object)itemBlueprint))
		{
			args.ReplyWith("Blueprint not found");
			return;
		}
		if (!itemBlueprint.userCraftable)
		{
			args.ReplyWith("Item is not craftable");
			return;
		}
		if (!basePlayer.blueprints.CanCraft(num, num3, basePlayer))
		{
			num3 = 0;
			if (0 == 0 && !basePlayer.blueprints.CanCraft(num, num3, basePlayer))
			{
				args.ReplyWith("You can't craft this item");
				return;
			}
			args.ReplyWith("You don't have permission to use this skin, so crafting unskinned");
		}
		bool flag2 = ItemSkinDirectory.FindByInventoryDefinitionId(num4).invItem is AccessoryItem;
		if (num4 != 0 && (!flag2 || !basePlayer.blueprints.CheckSkinOwnership(num4, basePlayer)))
		{
			args.ReplyWith("You don't have permission to use that attachment, removing...");
			num4 = 0;
		}
		int num5 = num2;
		int num6 = num2;
		if (flag)
		{
			num5 = Mathf.Min(num2, 5);
			num6 = 1;
		}
		for (int num7 = num5; num7 >= num6; num7--)
		{
			if (basePlayer.inventory.crafting.CraftItem(itemBlueprint, basePlayer, null, num7, num3, null, free: false, num4))
			{
				return;
			}
		}
		args.ReplyWith("Couldn't craft!");
	}

	[ServerUserVar]
	public static void canceltask(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer) && !basePlayer.IsDead())
		{
			int iID = args.GetInt(0);
			if (!basePlayer.inventory.crafting.CancelTask(iID))
			{
				args.ReplyWith("Couldn't cancel task!");
			}
		}
	}

	[ServerUserVar]
	public static void cancel(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer) && !basePlayer.IsDead())
		{
			int itemid = args.GetInt(0);
			basePlayer.inventory.crafting.CancelBlueprint(itemid);
		}
	}

	[ServerUserVar]
	public static void fasttracktask(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer) && !basePlayer.IsDead())
		{
			int taskID = args.GetInt(0);
			if (!basePlayer.inventory.crafting.FastTrackTask(taskID))
			{
				args.ReplyWith("Couldn't fast track task!");
			}
		}
	}
}
