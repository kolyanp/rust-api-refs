public class ItemModWrap : ItemMod
{
	public GameObjectRef successEffect;

	public ItemDefinition wrappedDefinition;

	public static Phrase wrap_gift;

	public static Phrase wrap_gift_desc;

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (!(command == "wrap") || item.amount <= 0)
		{
			return;
		}
		Item slot = item.contents.GetSlot(0);
		if (slot != null)
		{
			int position = item.position;
			ItemContainer parent = item.parent;
			item.RemoveFromContainer();
			Item item2 = ItemManager.Create(wrappedDefinition, 1, 0uL, isServerSide: true, 0uL);
			item2.SetItemOwnership(player, ItemOwnershipPhrases.Wrap);
			slot.MoveToContainer(item2.contents);
			item2.MoveToContainer(parent, position);
			item.Remove();
			if (successEffect.isValid)
			{
				Effect.server.Run(successEffect.resourcePath, player.eyes.position);
			}
		}
	}

	static ItemModWrap()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		wrap_gift = new Phrase("wrap_gift", "Wrap Gift");
		wrap_gift_desc = new Phrase("wrap_gift_desc", "Wrap this item and turn it in to an openable gift");
	}
}
