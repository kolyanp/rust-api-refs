public class ItemModOpenWrapped : ItemMod
{
	public GameObjectRef successEffect;

	public static Phrase open_wrapped_gift;

	public static Phrase open_wrapped_gift_desc;

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if (!(command == "open") || item.amount <= 0)
		{
			return;
		}
		Item slot = item.contents.GetSlot(0);
		if (slot != null)
		{
			int position = item.position;
			ItemContainer rootContainer = item.GetRootContainer();
			item.RemoveFromContainer();
			if (!slot.MoveToContainer(rootContainer, position))
			{
				player.GiveItem(slot);
			}
			item.Remove();
			if (successEffect.isValid)
			{
				Effect.server.Run(successEffect.resourcePath, player.eyes.position);
			}
		}
	}

	static ItemModOpenWrapped()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		open_wrapped_gift = new Phrase("open_wrapped_gift", "Unwrap");
		open_wrapped_gift_desc = new Phrase("open_wrapped_gift_desc", "Unwrap the gift and reveal its contents");
	}
}
