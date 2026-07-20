public static class PlayerInventoryErrors
{
	public static readonly Phrase InvalidItem = new Phrase("error_invaliditem", "Invalid item");

	public static readonly Phrase InvalidContainer = new Phrase("error_invalidcontainer", "Invalid container");

	public static readonly Phrase CannotMoveItem = new Phrase("error_cannotmoveitem", "Cannot move item!");

	public static readonly Phrase ContainerLocked = new Phrase("error_containerlocked", "Container locked");

	public static readonly Phrase DoesntAcceptPlayerItems = new Phrase("error_doesntacceptplayeritems", "Container does not accept player items!");

	public static readonly Phrase CannotEquipBroken = new Phrase("error_cannotequipbroken", "Cannot equip a broken item");

	public static readonly Phrase ActiveItemBroken = new Phrase("error_activeitembroken", "Your active item was broken!");

	public static readonly Phrase LootableDoesntExist = new Phrase("error_lootabledoesntexist", "The lootable container no longer exists!");

	public static readonly Phrase MoveItemFailedError = new Phrase("error.moveitemfailed", "Cannot move item: Inventory full!");

	public static readonly Phrase GiveItemFailedError = new Phrase("error.giveitemfailed", "Cannot take item: Inventory full!");

	public static readonly Phrase InventoryLockedError = new Phrase("error.inventorylocked", "Inventory locked!");
}
