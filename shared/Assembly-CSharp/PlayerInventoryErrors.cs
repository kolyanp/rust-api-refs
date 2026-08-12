public static class PlayerInventoryErrors
{
	public static readonly Phrase InvalidItem;

	public static readonly Phrase InvalidContainer;

	public static readonly Phrase CannotMoveItem;

	public static readonly Phrase ContainerLocked;

	public static readonly Phrase DoesntAcceptPlayerItems;

	public static readonly Phrase CannotEquipBroken;

	public static readonly Phrase ActiveItemBroken;

	public static readonly Phrase LootableDoesntExist;

	public static readonly Phrase MoveItemFailedError;

	public static readonly Phrase GiveItemFailedError;

	public static readonly Phrase InventoryLockedError;

	static PlayerInventoryErrors()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		InvalidItem = new Phrase("error_invaliditem", "Invalid item");
		InvalidContainer = new Phrase("error_invalidcontainer", "Invalid container");
		CannotMoveItem = new Phrase("error_cannotmoveitem", "Cannot move item!");
		ContainerLocked = new Phrase("error_containerlocked", "Container locked");
		DoesntAcceptPlayerItems = new Phrase("error_doesntacceptplayeritems", "Container does not accept player items!");
		CannotEquipBroken = new Phrase("error_cannotequipbroken", "Cannot equip a broken item");
		ActiveItemBroken = new Phrase("error_activeitembroken", "Your active item was broken!");
		LootableDoesntExist = new Phrase("error_lootabledoesntexist", "The lootable container no longer exists!");
		MoveItemFailedError = new Phrase("error.moveitemfailed", "Cannot move item: Inventory full!");
		GiveItemFailedError = new Phrase("error.giveitemfailed", "Cannot take item: Inventory full!");
		InventoryLockedError = new Phrase("error.inventorylocked", "Inventory locked!");
	}
}
