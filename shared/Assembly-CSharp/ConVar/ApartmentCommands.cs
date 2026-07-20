using System.Linq;
using UnityEngine;

namespace ConVar;

[Factory("apartment")]
public class ApartmentCommands : ConsoleSystem
{
	[ReplicatedVar(Name = "breakinseconds", Help = "How long a player needs to hold the break in interaction on an apartment door with a master key")]
	public static float breakinseconds = 30f;

	[ReplicatedVar(Name = "apartmentevictiondelay", Help = "How long should we wait before evicting a player from their apartment if they don't pay rent?")]
	public static float apartmentevictiondelay = 86400f;

	[ReplicatedVar(Help = "Should an invisible blocker prevent guests from entering apartments that they don't have access to?")]
	public static bool apartmentinvisibleblocker = true;

	[ReplicatedVar(Help = "Should combat be allowed inside apartment rooms outside of the break-in period?")]
	public static bool allowcombatoutsideofbreakin = true;

	[ReplicatedVar(Help = "How much scrap the apartment security NPC charges for a master key")]
	public static int masterkeyprice = 1000;

	[ServerVar(Help = "How many hours of scrap upkeep does the apartments spawn with (so players don't see 'Eviction' vital right after renting an apartment")]
	public static float apartmentfreerenthours = 4f;

	[ServerVar(Name = "intruderauthseconds", Help = "How long a player stays authorized on an apartment room after breaking in with a master key")]
	public static float intruderauthseconds = 300f;

	[ServerVar(Name = "rentscaling", Help = "Should the rent scale based on the items you have stored inside your apartment?")]
	public static float rentscaling = 0f;

	[ServerVar(Name = "npcsecuritydooropentime", Help = "How long should the apartment security NPC keep the door open for after being paid?")]
	public static float apartmentsecurityaccesstime = 300f;

	[ServerVar(Name = "printitemtax", Help = "Print out a list of all items that apartments will tax")]
	public static void PrintItemTax(Arg arg)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		TextTable val = new TextTable();
		val.AddColumns(new string[3] { "Item", "Tax Per Stack", "Stacksize" });
		foreach (ItemDefinition item in from x in ItemManager.itemList
			where x.ApartmentTaxPerStack > 0f
			orderby x.ApartmentTaxPerStack descending
			select x)
		{
			val.AddRow(new string[3]
			{
				item.shortname,
				item.ApartmentTaxPerStack.ToString("0.##"),
				item.stackable.ToString()
			});
		}
		arg.ReplyWith(((object)val).ToString());
	}

	private static ApartmentBuilding GetApartmentBuilding()
	{
		return BaseNetworkable.serverEntities.OfType<ApartmentBuilding>().FirstOrDefault();
	}

	[ServerVar(Name = "rentroom")]
	public static void RentApartment(Arg arg)
	{
		string text = arg.GetString(0);
		BasePlayer player = ArgEx.Player(arg);
		ApartmentBuilding apartmentBuilding = GetApartmentBuilding();
		if ((Object)(object)apartmentBuilding == (Object)null)
		{
			arg.ReplyWith("No apartment building found");
			return;
		}
		ApartmentRoom apartmentRoom = apartmentBuilding.FindByRoomNumber(text);
		if ((Object)(object)apartmentRoom == (Object)null)
		{
			arg.ReplyWith("No room found with number '" + text + "'");
		}
		else if ((Object)(object)apartmentBuilding.GetPlayerApartment(player) != (Object)null)
		{
			arg.ReplyWith("You already have an apartment!");
		}
		else
		{
			apartmentBuilding.GiveRoomToPlayer(player, apartmentRoom);
		}
	}

	[ServerVar(Name = "fakerentroom")]
	public static void fakerentroom(Arg arg)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		string text = arg.GetString(0);
		ApartmentBuilding apartmentBuilding = GetApartmentBuilding();
		if ((Object)(object)apartmentBuilding == (Object)null)
		{
			arg.ReplyWith("No apartment building found");
			return;
		}
		ApartmentRoom apartmentRoom = apartmentBuilding.FindByRoomNumber(text);
		if ((Object)(object)apartmentRoom == (Object)null)
		{
			arg.ReplyWith("No room found with number '" + text + "'");
			return;
		}
		if (apartmentRoom.IsCurrentlyRented())
		{
			arg.ReplyWith("Room '" + apartmentRoom.RoomNumber + "' is already rented");
			return;
		}
		BasePlayer basePlayer = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", ((Component)apartmentRoom.TeleportAnchor).transform.position, Quaternion.identity) as BasePlayer;
		basePlayer.Spawn();
		apartmentBuilding.GiveRoomToPlayer(basePlayer, apartmentRoom);
		arg.ReplyWith("Rented room '" + apartmentRoom.RoomNumber + "' to fake player");
	}

	[ServerVar(Name = "rentallrooms")]
	public static void RentAllRooms(Arg arg)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		ArgEx.Player(arg);
		ApartmentBuilding apartmentBuilding = GetApartmentBuilding();
		if ((Object)(object)apartmentBuilding == (Object)null)
		{
			arg.ReplyWith("No apartment building found");
			return;
		}
		foreach (ApartmentRoom room in apartmentBuilding.Rooms)
		{
			if (!room.IsCurrentlyRented())
			{
				BasePlayer basePlayer = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", ((Component)room.TeleportAnchor).transform.position, Quaternion.identity) as BasePlayer;
				basePlayer.Spawn();
				apartmentBuilding.GiveRoomToPlayer(basePlayer, room);
			}
		}
		arg.ReplyWith("Rented every single room out");
	}

	[ServerVar(Name = "rentallroomsoftype")]
	public static void RentAllRoomsOfType(Arg arg)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		ArgEx.Player(arg);
		ApartmentBuilding apartmentBuilding = GetApartmentBuilding();
		if ((Object)(object)apartmentBuilding == (Object)null)
		{
			arg.ReplyWith("No apartment building found");
			return;
		}
		int num = arg.GetInt(0);
		if (num < 1 || num > 3)
		{
			arg.ReplyWith($"Failed to get a room type from arg {num}");
			return;
		}
		ApartmentSize apartmentSize = (ApartmentSize)num;
		foreach (ApartmentRoom room in apartmentBuilding.Rooms)
		{
			if (!room.IsCurrentlyRented() && room.Size == apartmentSize)
			{
				BasePlayer basePlayer = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", ((Component)room.TeleportAnchor).transform.position, Quaternion.identity) as BasePlayer;
				basePlayer.Spawn();
				apartmentBuilding.GiveRoomToPlayer(basePlayer, room);
			}
		}
		arg.ReplyWith($"Rented every single room of type {apartmentSize} out");
	}

	[ServerVar(Name = "checkoutroom")]
	public static void CheckoutRoom(Arg arg)
	{
		string text = arg.GetString(0);
		BasePlayer player = ArgEx.Player(arg);
		ApartmentBuilding apartmentBuilding = GetApartmentBuilding();
		if ((Object)(object)apartmentBuilding == (Object)null)
		{
			arg.ReplyWith("No apartment building found");
			return;
		}
		if (!string.IsNullOrEmpty(text))
		{
			ApartmentRoom apartmentRoom = apartmentBuilding.FindByRoomNumber(text);
			if ((Object)(object)apartmentRoom == (Object)null)
			{
				arg.ReplyWith("No room found with number '" + text + "'");
				return;
			}
			apartmentBuilding.Checkout(apartmentRoom);
			arg.ReplyWith("You checked out room '" + apartmentRoom.RoomNumber + "'");
			return;
		}
		ApartmentRoom playerApartment = apartmentBuilding.GetPlayerApartment(player);
		if ((Object)(object)playerApartment == (Object)null)
		{
			arg.ReplyWith("You don't have an apartment room to checkout from!");
		}
		else if (apartmentBuilding.TryCheckout(player))
		{
			arg.ReplyWith("You have checked out of room '" + playerApartment.RoomNumber + "'");
		}
		else
		{
			arg.ReplyWith($"Failed to checkout of room '{playerApartment}'");
		}
	}

	[ServerVar(Help = "Checkout every room in the apartment complex")]
	public static void checkoutallrooms(Arg arg)
	{
		ArgEx.Player(arg);
		ApartmentBuilding apartmentBuilding = GetApartmentBuilding();
		if ((Object)(object)apartmentBuilding == (Object)null)
		{
			arg.ReplyWith("No apartment building found");
			return;
		}
		foreach (ApartmentRoom room in apartmentBuilding.Rooms)
		{
			if (room.IsCurrentlyRented())
			{
				apartmentBuilding.Checkout(room);
			}
		}
		arg.ReplyWith("Checked out every room in the apartment complex");
	}

	[ServerVar(Help = "Test triggering the apartment security door")]
	public static void testapartmentsecuritydoor(Arg arg)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		NPCApartmentSecurity.OnPaidToll(basePlayer, ((Component)basePlayer).transform.position, doPayment: false);
	}

	[ServerVar(Help = "Test triggering the scheduled death in safezones")]
	public static void scheduleddeath(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Must be ran from client");
		}
		else
		{
			basePlayer.ScheduledDeath();
		}
	}
}
