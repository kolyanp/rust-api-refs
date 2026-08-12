using Oxide.Core;
using UnityEngine;

public class ItemModUnwrap : ItemMod
{
	public static readonly Phrase UnwrapGiftTitle;

	public static readonly Phrase UnwrapGiftDesc;

	public Phrase OwnershipPhrase;

	public LootSpawn revealList;

	public GameObjectRef successEffect;

	public int minTries = 1;

	public int maxTries = 1;

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		if (command == "unwrap" && item.amount > 0 && Interface.CallHook("OnItemUnwrap", item, player, this) == null)
		{
			item.UseItem();
			int num = Random.Range(minTries, maxTries + 1);
			ItemOwnershipShare ownership = default(ItemOwnershipShare);
			if (OwnershipPhrase != null && !string.IsNullOrEmpty(OwnershipPhrase.token))
			{
				ownership = new ItemOwnershipShare
				{
					username = player.displayName,
					reason = OwnershipPhrase.token
				};
			}
			for (int i = 0; i < num; i++)
			{
				revealList.SpawnIntoContainer(player.inventory.containerMain, ownership, player.inventory.containerBelt);
			}
			if (successEffect.isValid)
			{
				Effect.server.Run(successEffect.resourcePath, player.eyes.position);
			}
		}
	}

	static ItemModUnwrap()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		UnwrapGiftTitle = new Phrase("unwrap_gift", "Unwrap");
		UnwrapGiftDesc = new Phrase("unwrap_gift_desc", "Unwrap the gift");
	}
}
