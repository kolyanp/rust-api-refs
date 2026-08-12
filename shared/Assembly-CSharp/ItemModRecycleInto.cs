using UnityEngine;

public class ItemModRecycleInto : ItemMod
{
	public static readonly Phrase RecycleIntoTitle;

	public static readonly Phrase RecycleIntoDesc;

	public ItemDefinition recycleIntoItem;

	public int numRecycledItemMin = 1;

	public int numRecycledItemMax = 1;

	public GameObjectRef successEffect;

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (!(command == "recycle_item"))
		{
			return;
		}
		int num = Random.Range(numRecycledItemMin, numRecycledItemMax + 1);
		item.UseItem();
		if (num > 0)
		{
			Item item2 = ItemManager.Create(recycleIntoItem, num, 0uL, isServerSide: true, 0uL);
			item2.SetItemOwnership(player, ItemOwnershipPhrases.Recycler);
			if (!item2.MoveToContainer(player.inventory.containerMain))
			{
				item2.Drop(player.GetDropPosition(), player.GetDropVelocity());
			}
			if (successEffect.isValid)
			{
				Effect.server.Run(successEffect.resourcePath, player.eyes.position);
			}
		}
	}

	static ItemModRecycleInto()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		RecycleIntoTitle = new Phrase("recycle_into", "MISSING RECYCLE INTO PHRASE");
		RecycleIntoDesc = new Phrase("recycle_into_desc", "MISSING RECYCLE INTO DESC PHRASE");
	}
}
