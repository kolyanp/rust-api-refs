using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Math;
using Network;
using Newtonsoft.Json;
using ProtoBuf;
using Rust;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;
using UnityEngine.Scripting;

namespace ConVar;

[Factory("global")]
public class Admin : ConsoleSystem
{
	private enum ChangeGradeMode
	{
		Upgrade,
		Downgrade
	}

	[Preserve]
	[JsonModel]
	public struct PlayerInfo
	{
		public string SteamID;

		public string OwnerSteamID;

		public string DisplayName;

		public int Ping;

		public string Address;

		public ulong EntityId;

		public int ConnectedSeconds;

		public float ViolationLevel;

		public float CurrentLevel;

		public float Health;

		public Vector3 Position;

		public bool IsMuted;

		public ulong TeamID;
	}

	[Preserve]
	[JsonModel]
	public struct PlayerIDInfo
	{
		public string SteamID;

		public string OwnerSteamID;

		public string DisplayName;

		public string Address;

		public ulong EntityId;
	}

	[JsonModel]
	[Preserve]
	public struct ServerInfoOutput
	{
		public string Hostname;

		public int MaxPlayers;

		public int Players;

		public int Queued;

		public int Joining;

		public int ReservedSlots;

		public int EntityCount;

		public string GameTime;

		public int Uptime;

		public string Map;

		public float Framerate;

		public int Memory;

		public int MemoryUsageSystem;

		public int Collections;

		public int NetworkIn;

		public int NetworkOut;

		public bool Restarting;

		public string SaveCreatedTime;

		public int Version;

		public string Protocol;
	}

	[Preserve]
	[JsonModel]
	public struct ServerConvarInfo
	{
		public string FullName;

		public string Value;

		public string Help;
	}

	[Preserve]
	[JsonModel]
	public struct ServerUGCInfo(IUGCBrowserEntity fromEntity)
	{
		public ulong entityId = fromEntity.UgcEntity.net.ID.Value;

		public uint[] crcs = fromEntity.GetContentCRCs;

		public UGCType contentType = fromEntity.ContentType;

		public uint entityPrefabID = fromEntity.UgcEntity.prefabID;

		public string shortPrefabName = fromEntity.UgcEntity.ShortPrefabName;

		public ulong[] playerIds = fromEntity.EditingHistory.ToArray();

		public string contentString = fromEntity.ContentString;
	}

	private struct EntityAssociation
	{
		public BaseEntity TargetEntity;

		public EntityAssociationType AssociationType;
	}

	private enum EntityAssociationType
	{
		Owner,
		Auth,
		LockGuest
	}

	[ReplicatedVar(Help = "Controls whether the in-game admin UI is displayed to admins")]
	public static bool allowAdminUI = true;

	[ServerVar(Help = "Include bots in the admin UI player list (debugging purpose only)")]
	public static bool showBotsInPlayerList = false;

	[ServerVar(Help = "Print out currently connected clients")]
	public static void status(Arg arg)
	{
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		string text = arg.GetString(0);
		if (text == "--json")
		{
			text = arg.GetString(1);
		}
		bool flag = arg.HasArg("--json");
		string text2 = string.Empty;
		if (!flag && text.Length == 0)
		{
			text2 = text2 + "hostname: " + Server.hostname + "\n";
			text2 = text2 + "version : " + 2632 + " secure (secure mode enabled, connected to Steam3)\n";
			text2 = text2 + "map     : " + Server.level + "\n";
			text2 += string.Format("players : {0} ({1} max) ({2} queued) ({3} joining)\n\n", new object[4]
			{
				((IEnumerable<BasePlayer>)BasePlayer.activePlayerList).Count(),
				Server.maxplayers,
				SingletonComponent<ServerMgr>.Instance.connectionQueue.Queued,
				SingletonComponent<ServerMgr>.Instance.connectionQueue.Joining
			});
		}
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.AddColumn("id");
			val.AddColumn("name");
			val.AddColumn("ping");
			val.AddColumn("connected");
			val.AddColumn("addr");
			val.AddColumn("owner");
			val.AddColumn("violation");
			val.AddColumn("kicks");
			val.AddColumn("entityId");
			Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BasePlayer current = enumerator.Current;
					try
					{
						if (!current.IsValid())
						{
							continue;
						}
						string userIDString = current.UserIDString;
						if (current.net.connection == null)
						{
							val.AddRow(new string[2] { userIDString, "NO CONNECTION" });
							continue;
						}
						string text3 = current.net.connection.ownerid.ToString();
						string text4 = StringExtensions.QuoteSafe(current.displayName);
						string text5 = Net.sv.GetAveragePing(current.net.connection).ToString();
						string text6 = current.net.connection.ipaddress;
						string text7 = current.net.ID.Value.ToString();
						string text8 = current.ViolationLevel.ToString("0.0");
						string text9 = current.GetAntiHackKicks().ToString();
						if (!arg.IsAdmin && !arg.IsRcon)
						{
							text6 = "xx.xxx.xx.xxx";
						}
						string text10 = current.net.connection.GetSecondsConnected() + "s";
						if (text.Length <= 0 || StringEx.Contains(text4, text, CompareOptions.IgnoreCase) || userIDString.Contains(text) || text3.Contains(text) || text6.Contains(text))
						{
							val.AddRow(new string[9]
							{
								userIDString,
								text4,
								text5,
								text10,
								text6,
								(text3 == userIDString) ? string.Empty : text3,
								text8,
								text9,
								text7
							});
						}
					}
					catch (Exception ex)
					{
						val.AddRow(new string[2]
						{
							current.UserIDString,
							StringExtensions.QuoteSafe(ex.Message)
						});
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			if (flag)
			{
				arg.ReplyWith(val.ToJson(true));
			}
			else
			{
				arg.ReplyWith(text2 + ((object)val).ToString());
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Print out stats of currently connected clients")]
	public static void stats(Arg arg)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.AddColumn("id");
			val.AddColumn("name");
			val.AddColumn("time");
			val.AddColumn("kills");
			val.AddColumn("deaths");
			val.AddColumn("suicides");
			val.AddColumn("player");
			val.AddColumn("building");
			val.AddColumn("entity");
			ulong uInt = arg.GetUInt64(0, 0uL);
			if (uInt == 0L)
			{
				string text = arg.GetString(0);
				Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						BasePlayer current = enumerator.Current;
						try
						{
							if (current.IsValid())
							{
								string text2 = StringExtensions.QuoteSafe(current.displayName);
								if (text.Length <= 0 || StringEx.Contains(text2, text, CompareOptions.IgnoreCase))
								{
									addRow(current.userID, text2, val);
								}
							}
						}
						catch (Exception ex)
						{
							val.AddRow(new string[2]
							{
								current.UserIDString,
								StringExtensions.QuoteSafe(ex.Message)
							});
						}
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
				}
			}
			else
			{
				string name = "N/A";
				BasePlayer basePlayer = BasePlayer.FindByID(uInt);
				if (Object.op_Implicit((Object)(object)basePlayer))
				{
					name = StringExtensions.QuoteSafe(basePlayer.displayName);
				}
				addRow(uInt, name, val);
			}
			arg.ReplyWith(flag ? val.ToJson(true) : ((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		static void addRow(ulong id, string text13, TextTable table)
		{
			ServerStatistics.Storage storage = ServerStatistics.Get(id);
			string text3 = TimeSpanEx.ToShortString(TimeSpan.FromSeconds((double)storage.Get("time")));
			string text4 = storage.Get("kill_player").ToString();
			string text5 = (storage.Get("deaths") - storage.Get("death_suicide")).ToString();
			string text6 = storage.Get("death_suicide").ToString();
			string text7 = storage.Get("hit_player_direct_los").ToString();
			string text8 = storage.Get("hit_player_indirect_los").ToString();
			string text9 = storage.Get("hit_building_direct_los").ToString();
			string text10 = storage.Get("hit_building_indirect_los").ToString();
			string text11 = storage.Get("hit_entity_direct_los").ToString();
			string text12 = storage.Get("hit_entity_indirect_los").ToString();
			table.AddRow(new string[9]
			{
				id.ToString(),
				text13,
				text3,
				text4,
				text5,
				text6,
				text7 + " / " + text8,
				text9 + " / " + text10,
				text11 + " / " + text12
			});
		}
	}

	[ServerVar(Help = "fillinventory <optional: category> - Fills your inventory with random items, can also specify a category (ammunition, weapon etc.)")]
	public static void fillInventory(Arg arg, string category)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Must be called from a player!");
			return;
		}
		bool flag = !string.IsNullOrEmpty(category);
		ItemCategory result = ItemCategory.Weapon;
		if (flag && (StringEx.IsNumeric(category) || !Enum.TryParse<ItemCategory>(category, ignoreCase: true, out result)))
		{
			arg.ReplyWith("'" + category + "' is not a valid item category!");
			return;
		}
		FillContainerInternal(basePlayer.inventory.containerBelt, flag, result, basePlayer);
		FillContainerInternal(basePlayer.inventory.containerMain, flag, result, basePlayer);
	}

	[ServerVar(Help = "fillcontainer <optional: category> - Fills the container you are looking at with random items, can also specify a category (ammunition, weapon etc.)")]
	public static void fillContainer(Arg arg, string category)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Must be called from a player!");
			return;
		}
		bool flag = !string.IsNullOrEmpty(category);
		ItemCategory result = ItemCategory.Weapon;
		if (flag && (StringEx.IsNumeric(category) || !Enum.TryParse<ItemCategory>(category, ignoreCase: true, out result)))
		{
			arg.ReplyWith("'" + category + "' is not a valid item category!");
			return;
		}
		BaseNetworkable baseNetworkable = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, basePlayer.eyes.HeadRay(), 0f, 6f, 1084293377, (QueryTriggerInteraction)0);
		if (baseNetworkable is IItemContainerEntity itemContainerEntity)
		{
			if (FillContainerInternal(itemContainerEntity.inventory, flag, result, basePlayer))
			{
				arg.ReplyWith($"Filled {baseNetworkable}.");
			}
			else
			{
				arg.ReplyWith($"Tried to fill {baseNetworkable}, but it couldn't accept some or all of the items.");
			}
		}
		else
		{
			arg.ReplyWith("Not looking at a container.");
		}
	}

	[ServerVar(Help = "fillcontainer_radius <radius> <optional: category> - Fills containers with random items within a radius, can also specify a category")]
	public static void fillContainer_radius(Arg arg, int radius, string category)
	{
		BasePlayer ply = ArgEx.Player(arg);
		if ((Object)(object)ply == (Object)null)
		{
			arg.ReplyWith("Must be called from a player!");
			return;
		}
		if (radius == 0)
		{
			arg.ReplyWith("Usage: fillcontainer_radius <radius> <optional: category>");
			return;
		}
		radius = Math.Min(radius, 50);
		bool useCategory = !string.IsNullOrEmpty(category);
		ItemCategory parsedCategory = ItemCategory.Weapon;
		if (useCategory && (StringEx.IsNumeric(category) || !Enum.TryParse<ItemCategory>(category, ignoreCase: true, out parsedCategory)))
		{
			arg.ReplyWith("'" + category + "' is not a valid item category!");
			return;
		}
		int foundAmount = 0;
		StringBuilder sb = new StringBuilder();
		RunInRadius(radius, ply, delegate(BaseCombatEntity entity)
		{
			if (entity.isServer && entity is IItemContainerEntity itemContainerEntity)
			{
				if (FillContainerInternal(itemContainerEntity.inventory, useCategory, parsedCategory, ply))
				{
					sb.AppendLine($"Filled {entity}.");
				}
				else
				{
					sb.AppendLine($"Tried to fill {entity}, but it couldn't accept some or all of the items.");
				}
				foundAmount++;
			}
		}, null, 1084293377);
		if (foundAmount == 0)
		{
			sb.AppendLine("Didn't find any containers in this radius.");
		}
		arg.ReplyWith(sb.ToString());
	}

	private static bool FillContainerInternal(ItemContainer container, bool useCategory, ItemCategory category, BasePlayer ply)
	{
		List<ItemDefinition> list = ItemManager.itemList.Where((ItemDefinition def) => !def.hidden && (!useCategory || def.category == category) && def.itemType != ItemContainer.ContentsType.Liquid).ToList();
		container.Clear();
		bool flag = false;
		int capacity = container.capacity;
		for (int num = 0; num < capacity; num++)
		{
			ItemDefinition random = ListEx.GetRandom<ItemDefinition>(list);
			Item item = ItemManager.CreateByItemID(random.itemid, random.stackable, 0uL, 0uL);
			item.OnVirginSpawn();
			item.SetItemOwnership(ply, ItemOwnershipPhrases.SpawnedPhrase);
			if (!item.MoveToContainer(container))
			{
				flag = true;
				item.Remove();
			}
		}
		return !flag;
	}

	[ServerVar(Help = "clearcontainer: Removes all items inside the container you're looking at")]
	public static void clearContainer(Arg arg)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Must be called from a player!");
			return;
		}
		BaseNetworkable baseNetworkable = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, basePlayer.eyes.HeadRay(), 0f, 6f, 1084293377, (QueryTriggerInteraction)0);
		if (baseNetworkable is IItemContainerEntity itemContainerEntity)
		{
			arg.ReplyWith($"Cleared {baseNetworkable}.");
			itemContainerEntity.inventory.Clear();
		}
		else
		{
			arg.ReplyWith("Not looking at a container.");
		}
	}

	[ServerVar(Help = "clearcontainer_radius <radius>: Removes all items inside a container within a radius")]
	public static void clearContainer_radius(Arg arg, int radius)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Must be called from a player!");
			return;
		}
		if (radius == 0)
		{
			arg.ReplyWith("Usage: clearContainer_radius <radius>");
			return;
		}
		int foundAmount = 0;
		StringBuilder sb = new StringBuilder();
		RunInRadius(radius, basePlayer, delegate(BaseCombatEntity entity)
		{
			if (entity.isServer && entity is IItemContainerEntity itemContainerEntity)
			{
				sb.AppendLine($"Emptied {entity}.");
				itemContainerEntity.inventory.Clear();
			}
			foundAmount++;
		}, null, 1084293377);
		if (foundAmount == 0)
		{
			arg.ReplyWith("Didn't find any containers in this radius");
		}
		else
		{
			arg.ReplyWith(sb.ToString());
		}
	}

	[ServerVar(Help = "upgrade_radius 'grade' 'radius'")]
	public static void upgrade_radius(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			arg.ReplyWith("Format is 'upgrade_radius {grade} {radius}'");
		}
		else
		{
			SkinRadiusInternal(arg, changeAnyGrade: true);
		}
	}

	[ServerVar(Help = "<grade>")]
	public static void upgrade_looking(Arg arg)
	{
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Format is 'upgrade_looking {grade}'");
		}
		else
		{
			SkinRaycastInternal(arg, changeAnyGrade: true);
		}
	}

	[ServerVar(Help = "skin_radius 'skin' 'radius'")]
	public static void skin_radius(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			arg.ReplyWith("Format is 'skin_radius {skin} {radius}'");
		}
		else
		{
			SkinRadiusInternal(arg, changeAnyGrade: false);
		}
	}

	[ServerVar(Help = "<skin>")]
	public static void skin_looking(Arg arg)
	{
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Format is 'skin_looking <skin>'");
		}
		else
		{
			SkinRaycastInternal(arg, changeAnyGrade: false);
		}
	}

	[ServerVar(Help = "<name/id> <radius> | Use print_wallpaper_skins for a list | 0 -> default, -1 -> random")]
	public static void add_wallpaper_radius(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			arg.ReplyWith("Format is 'add_wallpaper_radius {skin} {radius}' | Use print_wallpaper_skins for a list | 0 -> default, -1 -> random");
		}
		else
		{
			wallpaper_radius_internal(arg, addIfMissing: true);
		}
	}

	[ServerVar(Help = "<name/id> <radius> | Use print_wallpaper_skins for a list | 0 -> default, -1 -> random")]
	public static void change_wallpaper_radius(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			arg.ReplyWith("Format is 'change_wallpaper_radius {skin} {radius}' | Use print_wallpaper_skins for a list | 0 -> default, -1 -> random");
		}
		else
		{
			wallpaper_radius_internal(arg, addIfMissing: false);
		}
	}

	[ServerVar(Help = "clear_wallpaper_radius <radius>")]
	public static void clear_wallpaper_radius(Arg arg)
	{
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Format is 'clear_wallpaper_radius {radius}'");
			return;
		}
		RunInRadius(arg.GetFloat(0), ArgEx.Player(arg), delegate(BuildingBlock block)
		{
			if (block.HasWallpaper())
			{
				block.RemoveWallpaper(0);
				block.RemoveWallpaper(1);
			}
		}, null, 136314880);
	}

	public static BuildingGrade FindBuildingSkin(string name, out string error)
	{
		BuildingGrade buildingGrade = null;
		error = null;
		IEnumerable<BuildingGrade> source = from x in PrefabAttribute.server.FindAll<ConstructionGrade>(2194854973u)
			select x.gradeBase;
		switch (name)
		{
		case "twig":
		case "0":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => ((Object)x).name == "twigs");
			break;
		case "wood":
		case "1":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => ((Object)x).name == "wood");
			break;
		case "stone":
		case "2":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => ((Object)x).name == "stone");
			break;
		case "metal":
		case "sheetmetal":
		case "3":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => ((Object)x).name == "metal");
			break;
		case "hqm":
		case "armored":
		case "armoured":
		case "4":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => ((Object)x).name == "toptier");
			break;
		case "adobe":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => ((Object)x).name == "adobe");
			break;
		case "shipping":
		case "shippingcontainer":
		case "container":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => ((Object)x).name == "shipping_container");
			break;
		case "brutal":
		case "brutalist":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => ((Object)x).name == "brutalist");
			break;
		case "brick":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => ((Object)x).name == "brick");
			break;
		case "jungle":
		case "jungleruin":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => ((Object)x).name == "jungle");
			break;
		case "crypt":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => ((Object)x).name == "crypt");
			break;
		case "frontier":
		case "legacy":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => ((Object)x).name == "frontier");
			break;
		case "gingerbread":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => ((Object)x).name == "gingerbread");
			break;
		case "space":
		case "spacestation":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => ((Object)x).name == "space_station");
			break;
		default:
			error = "Valid skins are:\ntwig\nwood | frontier | gingerbread\nstone | adobe | brick | brutalist | jungle | crypt\nmetal | shipping\nhqm | space";
			return null;
		}
		if ((Object)(object)buildingGrade == (Object)null)
		{
			error = "Unable to find skin object for '" + name + "'";
		}
		return buildingGrade;
	}

	private static IEnumerable<BuildingBlock> SearchRadius(Vector3 position, float radius)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<BuildingBlock> list = new List<BuildingBlock>();
		global::Vis.Entities(position, radius, list, 2097152, (QueryTriggerInteraction)2);
		return list;
	}

	private static IEnumerable<BuildingBlock> SearchLookingAt(Vector3 position, Vector3 direction, float maxDistance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		BuildingBlock buildingBlock = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, new Ray(position, direction), 0f, maxDistance, 10485760, (QueryTriggerInteraction)1) as BuildingBlock;
		if ((Object)(object)buildingBlock == (Object)null)
		{
			return Array.Empty<BuildingBlock>();
		}
		return (IEnumerable<BuildingBlock>)(buildingBlock.GetBuilding()?.buildingBlocks);
	}

	private static void SkinRadiusInternal(Arg arg, bool changeAnyGrade)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		IEnumerable<BuildingBlock> blocks = SearchRadius(((Component)ArgEx.Player(arg)).transform.position, arg.GetFloat(1));
		ApplySkinInternal(arg, changeAnyGrade, blocks);
	}

	private static void SkinRaycastInternal(Arg arg, bool changeAnyGrade)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		IEnumerable<BuildingBlock> blocks = SearchLookingAt(basePlayer.eyes.position, basePlayer.eyes.BodyForward(), 100f);
		ApplySkinInternal(arg, changeAnyGrade, blocks);
	}

	private static void ApplySkinInternal(Arg arg, bool changeAnyGrade, IEnumerable<BuildingBlock> blocks)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("This must be called from the client");
			return;
		}
		arg.GetFloat(1);
		string text = arg.GetString(0);
		BuildingGrade buildingGrade = FindBuildingSkin(text, out var error);
		if ((Object)(object)buildingGrade == (Object)null)
		{
			arg.ReplyWith(error);
			return;
		}
		if (!buildingGrade.enabledInStandalone)
		{
			arg.ReplyWith("Skin " + text + " is not enabled in standalone yet");
			return;
		}
		if (blocks == null || blocks.Count() == 0)
		{
			arg.ReplyWith("No building blocks found");
			return;
		}
		uint shippingContainerBlockColourForPlayer = BuildingBlock.GetShippingContainerBlockColourForPlayer(basePlayer);
		foreach (BuildingBlock block in blocks)
		{
			if (!block.isClient && ((block.grade == buildingGrade.type) | changeAnyGrade))
			{
				block.ChangeGradeAndSkin(buildingGrade.type, buildingGrade.skin, playEffect: false, updateSkin: true, shippingContainerBlockColourForPlayer);
			}
		}
	}

	private static void wallpaper_radius_internal(Arg arg, bool addIfMissing)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("This must be called from the client");
			return;
		}
		float radius = arg.GetFloat(1);
		string text = arg.GetString(0);
		int skinIdParsed = -1;
		if (!int.TryParse(text, out skinIdParsed))
		{
			skinIdParsed = -1;
		}
		bool flag = false;
		string foundSkinName = "";
		foreach (ItemSkinDirectory.Skin item in WallpaperSettings.WallpaperItemDef.skins.Concat(WallpaperSettings.FlooringItemDef.skins).Concat(WallpaperSettings.CeilingItemDef.skins))
		{
			if (skinIdParsed != -1 && item.id == skinIdParsed)
			{
				flag = true;
				foundSkinName = item.invItem.displayName.english.Trim();
				break;
			}
			if (skinIdParsed == -1 && (item.invItem.displayName.english.Contains(text, StringComparison.InvariantCultureIgnoreCase) || ((Object)item.invItem).name.Contains(text, StringComparison.InvariantCultureIgnoreCase)))
			{
				flag = true;
				foundSkinName = item.invItem.displayName.english.Trim();
				skinIdParsed = item.id;
				break;
			}
		}
		if (skinIdParsed == 0)
		{
			flag = true;
		}
		if (!flag && skinIdParsed != -1)
		{
			arg.ReplyWith("Invalid skin");
			return;
		}
		RunInRadius(radius, basePlayer, delegate(BuildingBlock block)
		{
			bool flag2 = block.HasWallpaper();
			bool flag3 = flag2;
			if (addIfMissing && !flag2)
			{
				flag3 = WallpaperPlanner.Settings.CanUseWallpaper(block);
			}
			if (block.HasWallpaper() | flag3)
			{
				if (skinIdParsed == -1)
				{
					arg.ReplyWith("Applying random wallpaper");
					for (int i = 0; i < 2; i++)
					{
						ItemDefinition wallpaperItem = WallpaperPlanner.Settings.GetWallpaperItem(block, i);
						if ((Object)(object)wallpaperItem != (Object)null)
						{
							int id = ArrayEx.GetRandom(wallpaperItem.skins).id;
							block.SetWallpaper((ulong)id, i);
						}
					}
				}
				else if (skinIdParsed == 0)
				{
					arg.ReplyWith("Applying default wallpaper");
					block.SetWallpaper(0uL);
					block.SetWallpaper(0uL, 1);
				}
				else
				{
					arg.ReplyWith("Applying '" + foundSkinName + "' wallpaper to compatible blocks");
					for (int j = 0; j < 2; j++)
					{
						ItemDefinition wallpaperItem2 = WallpaperPlanner.Settings.GetWallpaperItem(block, j);
						if ((Object)(object)wallpaperItem2 != (Object)null && wallpaperItem2.skins.Any((ItemSkinDirectory.Skin x) => x.id == skinIdParsed))
						{
							block.SetWallpaper((ulong)skinIdParsed, j);
						}
					}
				}
				block.CheckWallpaper();
			}
		}, null, 136314880);
	}

	[ServerVar(Help = "Lists all wallpaper skins")]
	public static void print_wallpaper_skins(Arg arg)
	{
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumns(new string[3] { "Id", "Type", "Name" });
			ItemSkinDirectory.Skin[] skins = WallpaperSettings.WallpaperItemDef.skins;
			for (int i = 0; i < skins.Length; i++)
			{
				ItemSkinDirectory.Skin skin = skins[i];
				string[] array = new string[3];
				int id = skin.id;
				array[0] = id.ToString();
				array[1] = "Wall";
				array[2] = skin.invItem.displayName.english.Trim();
				val.AddRow(array);
			}
			skins = WallpaperSettings.FlooringItemDef.skins;
			for (int i = 0; i < skins.Length; i++)
			{
				ItemSkinDirectory.Skin skin2 = skins[i];
				string[] array2 = new string[3];
				int id = skin2.id;
				array2[0] = id.ToString();
				array2[1] = "Floor";
				array2[2] = skin2.invItem.displayName.english.Trim();
				val.AddRow(array2);
			}
			skins = WallpaperSettings.CeilingItemDef.skins;
			for (int i = 0; i < skins.Length; i++)
			{
				ItemSkinDirectory.Skin skin3 = skins[i];
				string[] array3 = new string[3];
				int id = skin3.id;
				array3[0] = id.ToString();
				array3[1] = "Ceiling";
				array3[2] = skin3.invItem.displayName.english.Trim();
				val.AddRow(array3);
			}
			arg.ReplyWith(((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "<gene string> - Applies the given genes (e.g. \"YYYGGG\") to the clone/seed in your hands")]
	public static void applygenes(Arg arg)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Must be called from a player");
			return;
		}
		Item activeItem = basePlayer.GetActiveItem();
		ItemModDeployable itemModDeployable = default(ItemModDeployable);
		if (activeItem == null || !((Component)activeItem.info).TryGetComponent<ItemModDeployable>(ref itemModDeployable) || (Object)(object)itemModDeployable.entityPrefab.Get().GetComponent<GrowableEntity>() == (Object)null)
		{
			arg.ReplyWith("Not holding a growable item");
			return;
		}
		string text = arg.GetString(0, "YYYGGG").ToUpper();
		if (text.Length != 6 || text.Any((char x) => !"XGHWY".Contains(x)))
		{
			arg.ReplyWith("Invalid gene string");
			return;
		}
		if (activeItem.instanceData == null)
		{
			activeItem.instanceData = new InstanceData
			{
				ShouldPool = false,
				dataInt = GrowableGeneEncoding.EncodeGeneStringToInt(text)
			};
		}
		else
		{
			activeItem.instanceData.dataInt = GrowableGeneEncoding.EncodeGeneStringToInt(text);
		}
		activeItem.MarkDirty();
		arg.ReplyWith("Applied genes to the held item");
	}

	[ServerVar(Help = "Kills all bee swarms")]
	public static void killbees(Arg arg)
	{
		int num = 0;
		BeeSwarmMaster[] array = BaseEntity.Util.FindAll<BeeSwarmMaster>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].AdminKill();
			num++;
		}
		BeeSwarmAI[] array2 = BaseEntity.Util.FindAll<BeeSwarmAI>();
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].AdminKill();
			num++;
		}
		arg.ReplyWith($"Killed {num} bee swarms");
	}

	[ServerVar(Help = "(Generated) Deals 1000 damage to the specified player (by name/SteamID/bot) killing them immediately; useful for testing death logic without console kill commands")]
	public static void killplayer(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (!Object.op_Implicit((Object)(object)basePlayer))
		{
			basePlayer = BasePlayer.FindBotClosestMatch(arg.GetString(0));
		}
		if (!Object.op_Implicit((Object)(object)basePlayer))
		{
			arg.ReplyWith("Player not found");
		}
		else
		{
			basePlayer.Hurt(1000f, DamageType.Suicide, basePlayer, useProtection: false);
		}
	}

	[ServerVar(Help = "(Generated) Deals lethal damage to every non-NPC player currently connected to the server; reports the number of players killed")]
	public static void killallplayers(Arg arg)
	{
		BasePlayer[] array = BaseEntity.Util.FindAll<BasePlayer>();
		int num = 0;
		BasePlayer[] array2 = array;
		foreach (BasePlayer basePlayer in array2)
		{
			if (!basePlayer.IsNpc)
			{
				basePlayer.Hurt(1000f, DamageType.Suicide, basePlayer, useProtection: false);
				num++;
			}
		}
		arg.ReplyWith($"Killed {num} players");
	}

	[ServerVar(Help = "(Generated) Puts the specified player into the wounded/downed state immediately without killing them; useful for testing the crawl/revive mechanics")]
	public static void injureplayer(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (!Object.op_Implicit((Object)(object)basePlayer))
		{
			basePlayer = BasePlayer.FindBotClosestMatch(arg.GetString(0));
		}
		if (!Object.op_Implicit((Object)(object)basePlayer))
		{
			arg.ReplyWith("Player not found");
		}
		else
		{
			Global.InjurePlayer(basePlayer);
		}
	}

	[ServerVar(Help = "(Generated) Recovers the specified player from the wounded state, standing them back up at minimum health")]
	public static void recoverplayer(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (!Object.op_Implicit((Object)(object)basePlayer))
		{
			basePlayer = BasePlayer.FindBotClosestMatch(arg.GetString(0));
		}
		if (!Object.op_Implicit((Object)(object)basePlayer))
		{
			arg.ReplyWith("Player not found");
		}
		else
		{
			Global.RecoverPlayer(basePlayer);
		}
	}

	[ServerVar(Help = "(Generated) Kicks the specified player from the server with an optional reason; broadcasts the kick to chat and places them through the queue on reconnect")]
	public static void kick(Arg arg)
	{
		BasePlayer player = ArgEx.GetPlayer(arg, 0);
		if (!Object.op_Implicit((Object)(object)player) || player.net == null || player.net.connection == null)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		string text = arg.GetString(1, "no reason given");
		arg.ReplyWith("Kicked: " + player.displayName);
		Chat.Broadcast("Kicking " + player.displayName + " (" + text + ")", "SERVER", "#eee", 0uL);
		player.Kick("Kicked: " + arg.GetString(1, "No Reason Given"), reserveSlot: false);
	}

	[ServerVar(Help = "(Generated) Silently kicks the specified player without broadcasting to chat; the kick is logged to RCON only")]
	public static void skick(Arg arg)
	{
		BasePlayer player = ArgEx.GetPlayer(arg, 0);
		if (!Object.op_Implicit((Object)(object)player) || player.net == null || player.net.connection == null)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		string text = arg.GetString(1, "no reason given");
		arg.ReplyWith("Kicked: " + player.displayName);
		Chat.Record(new Chat.ChatEntry
		{
			Channel = Chat.ChatChannel.Server,
			Message = "(SILENT) Kicking " + player.displayName + " (" + text + ")",
			UserId = "0",
			Username = "SERVER",
			Color = "#eee",
			Time = Epoch.Current
		});
		player.Kick("Kicked: " + arg.GetString(1, "No Reason Given"), reserveSlot: false);
	}

	[ServerVar(Help = "(Generated) Kicks all currently connected players from the server with an optional reason; useful for forcing a restart or clearing the server")]
	public static void kickall(Arg arg)
	{
		BasePlayer[] array = ((IEnumerable<BasePlayer>)BasePlayer.activePlayerList).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Kick("Kicked: " + arg.GetString(0, "No Reason Given"));
		}
	}

	[ServerVar(Help = "ban <player> <reason> [optional duration]")]
	public static void ban(Arg arg)
	{
		BasePlayer player = ArgEx.GetPlayer(arg, 0);
		if (!Object.op_Implicit((Object)(object)player) || player.net == null || player.net.connection == null)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		ServerUsers.User user = ServerUsers.Get(player.userID);
		if (user != null && user.group == ServerUsers.UserGroup.Banned)
		{
			arg.ReplyWith($"User {player.userID.Get()} is already banned");
			return;
		}
		string text = arg.GetString(1, "No Reason Given");
		if (TryGetBanExpiry(arg, 2, out var expiry, out var durationSuffix))
		{
			ServerUsers.Set(player.userID, ServerUsers.UserGroup.Banned, player.displayName, text, expiry);
			string text2 = "";
			if (player.IsConnected && player.net.connection.ownerid != 0L && player.net.connection.ownerid != player.net.connection.userid)
			{
				text2 += $" and also banned ownerid {player.net.connection.ownerid}";
				ServerUsers.Set(player.net.connection.ownerid, ServerUsers.UserGroup.Banned, player.displayName, arg.GetString(1, $"Family share owner of {player.net.connection.userid}"), -1L);
			}
			ServerUsers.Save();
			arg.ReplyWith(string.Format("Kickbanned User{0}: {1} - {2}{3}", new object[4]
			{
				durationSuffix,
				player.userID.Get(),
				player.displayName,
				text2
			}));
			Chat.Broadcast("Kickbanning " + player.displayName + durationSuffix + " (" + text + ")", "SERVER", "#eee", 0uL);
			Net.sv.Kick(player.net.connection, "Banned" + durationSuffix + ": " + text);
		}
	}

	[ServerVar(Help = "(Generated) Adds the specified Steam64 ID as a server moderator with optional name and reason; grants admin flag to the player if connected")]
	public static void moderatorid(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		string text = arg.GetString(1, "unnamed");
		string notes = arg.GetString(2, "no reason");
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user != null && user.group == ServerUsers.UserGroup.Moderator)
		{
			arg.ReplyWith("User " + uInt + " is already a Moderator");
			return;
		}
		ServerUsers.Set(uInt, ServerUsers.UserGroup.Moderator, text, notes, -1L);
		ServerUsers.Save();
		BasePlayer basePlayer = BasePlayer.FindByID(uInt);
		if ((Object)(object)basePlayer != (Object)null)
		{
			basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.IsAdmin, b: true);
			basePlayer.net.connection.authLevel = 1u;
			basePlayer.SendNetworkUpdate();
		}
		arg.ReplyWith("Added moderator " + text + ", steamid " + uInt);
	}

	[ServerVar(Help = "(Generated) Adds the specified Steam64 ID as a server owner (auth level 2) with optional name and reason; requires the caller to also be auth level 2")]
	public static void ownerid(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		string text = arg.GetString(1, "unnamed");
		string notes = arg.GetString(2, "no reason");
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			return;
		}
		if (arg.Connection != null && arg.Connection.authLevel < 2)
		{
			arg.ReplyWith("Moderators cannot run ownerid");
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user != null && user.group == ServerUsers.UserGroup.Owner)
		{
			arg.ReplyWith("User " + uInt + " is already an Owner");
			return;
		}
		ServerUsers.Set(uInt, ServerUsers.UserGroup.Owner, text, notes, -1L);
		ServerUsers.Save();
		BasePlayer basePlayer = BasePlayer.FindByID(uInt);
		if ((Object)(object)basePlayer != (Object)null)
		{
			basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.IsAdmin, b: true);
			basePlayer.net.connection.authLevel = 2u;
			basePlayer.SendNetworkUpdate();
		}
		arg.ReplyWith("Added owner " + text + ", steamid " + uInt);
	}

	[ServerVar(Help = "(Generated) Removes moderator status from the specified Steam64 ID; removes admin flag from the player if currently connected")]
	public static void removemoderator(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user == null || user.group != ServerUsers.UserGroup.Moderator)
		{
			arg.ReplyWith("User " + uInt + " isn't a moderator");
			return;
		}
		ServerUsers.Remove(uInt);
		ServerUsers.Save();
		BasePlayer basePlayer = BasePlayer.FindByID(uInt);
		if ((Object)(object)basePlayer != (Object)null)
		{
			basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.IsAdmin, b: false);
			basePlayer.net.connection.authLevel = 0u;
			basePlayer.SendNetworkUpdate();
		}
		arg.ReplyWith("Removed Moderator: " + uInt);
	}

	[ServerVar(Help = "(Generated) Removes owner status from the specified Steam64 ID; removes admin flag from the player if currently connected")]
	public static void removeowner(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user == null || user.group != ServerUsers.UserGroup.Owner)
		{
			arg.ReplyWith("User " + uInt + " isn't an owner");
			return;
		}
		ServerUsers.Remove(uInt);
		ServerUsers.Save();
		BasePlayer basePlayer = BasePlayer.FindByID(uInt);
		if ((Object)(object)basePlayer != (Object)null)
		{
			basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.IsAdmin, b: false);
			basePlayer.net.connection.authLevel = 0u;
			basePlayer.SendNetworkUpdate();
		}
		arg.ReplyWith("Removed Owner: " + uInt);
	}

	[ServerVar(Help = "banid <steamid> <username> <reason> [optional duration]")]
	public static void banid(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		string text = arg.GetString(1, "unnamed");
		string text2 = arg.GetString(2, "no reason");
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user != null && user.group == ServerUsers.UserGroup.Banned)
		{
			arg.ReplyWith("User " + uInt + " is already banned");
		}
		else
		{
			if (!TryGetBanExpiry(arg, 3, out var expiry, out var durationSuffix))
			{
				return;
			}
			string text3 = "";
			BasePlayer basePlayer = BasePlayer.FindByID(uInt);
			if ((Object)(object)basePlayer != (Object)null && basePlayer.IsConnected)
			{
				text = basePlayer.displayName;
				if (basePlayer.IsConnected && basePlayer.net.connection.ownerid != 0L && basePlayer.net.connection.ownerid != basePlayer.net.connection.userid)
				{
					text3 += $" and also banned ownerid {basePlayer.net.connection.ownerid}";
					ServerUsers.Set(basePlayer.net.connection.ownerid, ServerUsers.UserGroup.Banned, basePlayer.displayName, arg.GetString(1, $"Family share owner of {basePlayer.net.connection.userid}"), expiry);
				}
				Chat.Broadcast("Kickbanning " + basePlayer.displayName + durationSuffix + " (" + text2 + ")", "SERVER", "#eee", 0uL);
				Net.sv.Kick(basePlayer.net.connection, "Banned" + durationSuffix + ": " + text2);
			}
			ServerUsers.Set(uInt, ServerUsers.UserGroup.Banned, text, text2, expiry);
			arg.ReplyWith(string.Format("Banned User{0}: {1} - \"{2}\" for \"{3}\"{4}", new object[5] { durationSuffix, uInt, text, text2, text3 }));
		}
	}

	private static bool TryGetBanExpiry(Arg arg, int n, out long expiry, out string durationSuffix)
	{
		expiry = arg.GetTimestamp(n, -1L);
		durationSuffix = null;
		int current = Epoch.Current;
		if (expiry > 0 && expiry <= current)
		{
			arg.ReplyWith("Expiry time is in the past");
			return false;
		}
		durationSuffix = ((expiry > 0) ? (" for " + NumberExtensions.FormatSecondsLong(expiry - current)) : "");
		return true;
	}

	[ServerVar(Help = "(Generated) Removes the ban for the specified Steam64 ID from the server banlist, allowing the player to reconnect")]
	public static void unban(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith($"This doesn't appear to be a 64bit steamid: {uInt}");
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user == null || user.group != ServerUsers.UserGroup.Banned)
		{
			arg.ReplyWith($"User {uInt} isn't banned");
			return;
		}
		ServerUsers.Remove(uInt);
		arg.ReplyWith("Unbanned User: " + uInt);
	}

	[ServerVar(Help = "(Generated) Moves the specified Steam64 ID to the front of the connection queue so they connect immediately on next join")]
	public static void skipqueue(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
		}
		else
		{
			SingletonComponent<ServerMgr>.Instance.connectionQueue.SkipQueue(uInt);
		}
	}

	[ServerVar(Help = "Adds skip queue permissions to a SteamID")]
	public static void skipqueueid(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		string text = arg.GetString(1, "unnamed");
		string notes = arg.GetString(2, "no reason");
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user != null && (user.group == ServerUsers.UserGroup.Owner || user.group == ServerUsers.UserGroup.Moderator || user.group == ServerUsers.UserGroup.SkipQueue))
		{
			arg.ReplyWith($"User {uInt} will already skip the queue ({user.group})");
			return;
		}
		if (user != null && user.group == ServerUsers.UserGroup.Banned)
		{
			arg.ReplyWith($"User {uInt} is banned");
			return;
		}
		ServerUsers.Set(uInt, ServerUsers.UserGroup.SkipQueue, text, notes, -1L);
		arg.ReplyWith($"Added skip queue permission for {text} ({uInt})");
	}

	[ServerVar(Help = "Removes skip queue permission from a SteamID")]
	public static void removeskipqueue(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user != null && (user.group == ServerUsers.UserGroup.Owner || user.group == ServerUsers.UserGroup.Moderator))
		{
			arg.ReplyWith($"User is a {user.group}, cannot remove skip queue permission with this command");
			return;
		}
		if (user == null || user.group != ServerUsers.UserGroup.SkipQueue)
		{
			arg.ReplyWith("User does not have skip queue permission");
			return;
		}
		ServerUsers.Remove(uInt);
		arg.ReplyWith("Removed skip queue permission: " + uInt);
	}

	[ServerVar(Help = "Print out currently connected clients etc")]
	public static void players(Arg arg)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.ResizeColumns(5);
			val.AddColumn("id");
			val.AddColumn("name");
			val.AddColumn("ping");
			val.AddColumn("updt");
			val.AddColumn("dist");
			val.AddColumn("enId");
			val.ResizeRows(BasePlayer.activePlayerList.Count);
			Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BasePlayer current = enumerator.Current;
					string userIDString = current.UserIDString;
					val.AddValue(userIDString);
					string text = current.displayName;
					if (text.Length >= 14)
					{
						text = text.Substring(0, 14) + "..";
					}
					val.AddValue(text);
					int averagePing = Net.sv.GetAveragePing(current.net.connection);
					val.AddValue(averagePing);
					int queuedUpdateCount = current.GetQueuedUpdateCount(BasePlayer.NetworkQueue.Update);
					val.AddValue(queuedUpdateCount);
					int queuedUpdateCount2 = current.GetQueuedUpdateCount(BasePlayer.NetworkQueue.UpdateDistance);
					val.AddValue(queuedUpdateCount2);
					ulong value = current.net.ID.Value;
					val.AddValue(value);
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			arg.ReplyWith(flag ? val.ToJson(false) : ((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Sends a message in chat")]
	public static void say(Arg arg)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Chat.Broadcast((string)arg.FullString, "SERVER", "#eee", 0uL);
	}

	[ServerVar(Help = "Show user info for players on server.")]
	public static void users(Arg arg)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		string text = "<slot:userid:\"name\">\n";
		int num = 0;
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				text = text + current.userID.Get() + ":\"" + current.displayName + "\"\n";
				num++;
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		text = text + num + "users\n";
		arg.ReplyWith(text);
	}

	[ServerVar(Help = "Show user info for players on server.")]
	public static void sleepingusers(Arg arg)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		string text = "<slot:userid:\"name\">\n";
		int num = 0;
		Enumerator<BasePlayer> enumerator = BasePlayer.sleepingPlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				text += $"{current.userID.Get()}:{current.displayName}\n";
				num++;
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		text += $"{num} sleeping users\n";
		arg.ReplyWith(text);
	}

	[ServerVar(Help = "Show user info for sleeping players on server in range of the player.")]
	public static void sleepingusersinrange(Arg arg)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer fromPlayer = ArgEx.Player(arg);
		if ((Object)(object)fromPlayer == (Object)null)
		{
			return;
		}
		if (fromPlayer.IsSpectating() && (Object)(object)fromPlayer.SpectatingTarget != (Object)null)
		{
			fromPlayer = fromPlayer.SpectatingTarget;
		}
		float range = arg.GetFloat(0);
		string text = "<slot:userid:\"name\">\n";
		int num = 0;
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		Enumerator<BasePlayer> enumerator = BasePlayer.sleepingPlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				list.Add(current);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		list.RemoveAll((BasePlayer p) => p.Distance2D((BaseEntity)fromPlayer) > range);
		list.Sort((BasePlayer player, BasePlayer basePlayer) => (!(player.Distance2D((BaseEntity)fromPlayer) < basePlayer.Distance2D((BaseEntity)fromPlayer))) ? 1 : (-1));
		foreach (BasePlayer item in list)
		{
			text += $"{item.userID.Get()}:{item.displayName}:{item.Distance2D((BaseEntity)fromPlayer)}m\n";
			num++;
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
		text += $"{num} sleeping users within {range}m\n";
		arg.ReplyWith(text);
	}

	[ServerVar(Help = "Show user info for players on server in range of the player.")]
	public static void usersinrange(Arg arg)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer fromPlayer = ArgEx.Player(arg);
		if ((Object)(object)fromPlayer == (Object)null)
		{
			return;
		}
		if (fromPlayer.IsSpectating() && (Object)(object)fromPlayer.SpectatingTarget != (Object)null)
		{
			fromPlayer = fromPlayer.SpectatingTarget;
		}
		float range = arg.GetFloat(0);
		string text = "<slot:userid:\"name\">\n";
		int num = 0;
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				list.Add(current);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		list.RemoveAll((BasePlayer p) => p.Distance2D((BaseEntity)fromPlayer) > range);
		list.Sort((BasePlayer player, BasePlayer basePlayer) => (!(player.Distance2D((BaseEntity)fromPlayer) < basePlayer.Distance2D((BaseEntity)fromPlayer))) ? 1 : (-1));
		foreach (BasePlayer item in list)
		{
			text += $"{item.userID.Get()}:{item.displayName}:{item.Distance2D((BaseEntity)fromPlayer)}m\n";
			num++;
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
		text += $"{num} users within {range}m\n";
		arg.ReplyWith(text);
	}

	[ServerVar(Help = "Show user info for players on server in range of the supplied player (eg. Jim 50)")]
	public static void usersinrangeofplayer(Arg arg)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer targetPlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
		if ((Object)(object)targetPlayer == (Object)null)
		{
			return;
		}
		float range = arg.GetFloat(1);
		string text = "<slot:userid:\"name\">\n";
		int num = 0;
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				list.Add(current);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		list.RemoveAll((BasePlayer p) => p.Distance2D((BaseEntity)targetPlayer) > range);
		list.Sort((BasePlayer player, BasePlayer basePlayer) => (!(player.Distance2D((BaseEntity)targetPlayer) < basePlayer.Distance2D((BaseEntity)targetPlayer))) ? 1 : (-1));
		foreach (BasePlayer item in list)
		{
			text += $"{item.userID.Get()}:{item.displayName}:{item.Distance2D((BaseEntity)targetPlayer)}m\n";
			num++;
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
		text += $"{num} users within {range}m of {targetPlayer.displayName}\n";
		arg.ReplyWith(text);
	}

	[ServerVar(Help = "List of banned users (sourceds compat)")]
	public static void banlist(Arg arg)
	{
		arg.ReplyWith(ServerUsers.BanListString());
	}

	[ServerVar(Help = "List of banned users - shows reasons and usernames")]
	public static void banlistex(Arg arg)
	{
		arg.ReplyWith(ServerUsers.BanListStringEx());
	}

	[ServerVar(Help = "List of banned users, by ID (sourceds compat)")]
	public static void listid(Arg arg)
	{
		arg.ReplyWith(ServerUsers.BanListString(bHeader: true));
	}

	[ServerVar(Help = "(Generated) Mutes the specified connected player preventing them from using chat; optionally accepts a mute expiry timestamp for temporary mutes")]
	public static void mute(Arg arg)
	{
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (!Object.op_Implicit((Object)(object)playerOrSleeper) || playerOrSleeper.net == null || playerOrSleeper.net.connection == null)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		long timestamp = arg.GetTimestamp(1, 0L);
		if (timestamp > 0)
		{
			playerOrSleeper.State.chatMuteExpiryTimestamp = timestamp;
			string text = NumberExtensions.FormatSecondsLong(timestamp - DateTimeOffset.UtcNow.ToUnixTimeSeconds());
			playerOrSleeper.ChatMessage("You have been muted for " + text);
		}
		else
		{
			playerOrSleeper.ChatMessage("You have been permanently muted");
		}
		playerOrSleeper.State.chatMuted = true;
		playerOrSleeper.SetPlayerFlag(BasePlayer.PlayerFlags.ChatMute, b: true);
	}

	[ServerVar(Help = "(Generated) Removes the chat mute from the specified connected player, allowing them to send messages again")]
	public static void unmute(Arg arg)
	{
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (!Object.op_Implicit((Object)(object)playerOrSleeper) || playerOrSleeper.net == null || playerOrSleeper.net.connection == null)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		playerOrSleeper.State.chatMuted = false;
		playerOrSleeper.State.chatMuteExpiryTimestamp = 0.0;
		playerOrSleeper.SetPlayerFlag(BasePlayer.PlayerFlags.ChatMute, b: false);
		playerOrSleeper.ChatMessage("You have been unmuted");
	}

	[ServerVar(Help = "Print a list of currently muted players")]
	public static void mutelist(Arg arg)
	{
		var obj = from x in BasePlayer.allPlayerList
			where x.HasPlayerFlag(BasePlayer.PlayerFlags.ChatMute)
			select new
			{
				SteamId = x.UserIDString,
				Name = x.displayName
			};
		arg.ReplyWith(obj);
	}

	[ServerVar(Help = "(Generated) Requests a performance report from every connected client; supports legacy and JSON formats; used for monitoring client frame rates and memory usage")]
	public static void clientperf(Arg arg)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		string arg2 = arg.GetString(0, "legacy");
		int arg3 = arg.GetInt(1, Random.Range(int.MinValue, int.MaxValue));
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				current.ClientRPC(RpcTarget.Player("GetPerformanceReport", current), arg2, arg3);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ServerVar(Help = "Get information about all the cars in the world")]
	public static void carstats(Arg arg)
	{
		HashSet<ModularCar> allCarsList = ModularCar.allCarsList;
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumn("id");
			val.AddColumn("sockets");
			val.AddColumn("modules");
			val.AddColumn("complete");
			val.AddColumn("engine");
			val.AddColumn("health");
			val.AddColumn("location");
			int count = allCarsList.Count;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (ModularCar item in allCarsList)
			{
				string text = ((object)Unsafe.As<NetworkableId, NetworkableId>(ref item.net.ID)/*cast due to constrained. prefix*/).ToString();
				string text2 = item.TotalSockets.ToString();
				string text3 = item.NumAttachedModules.ToString();
				string text4;
				if (item.IsComplete())
				{
					text4 = "Complete";
					num++;
				}
				else
				{
					text4 = "Partial";
				}
				string text5;
				if (item.HasAnyWorkingEngines())
				{
					text5 = "Working";
					num2++;
				}
				else
				{
					text5 = "Broken";
				}
				string text6 = ((item.TotalMaxHealth() != 0f) ? $"{item.TotalHealth() / item.TotalMaxHealth():0%}" : "0");
				string text7;
				if (item.IsOutside())
				{
					text7 = "Outside";
				}
				else
				{
					text7 = "Inside";
					num3++;
				}
				val.AddRow(new string[7] { text, text2, text3, text4, text5, text6, text7 });
			}
			string text8 = "";
			text8 = ((count != 1) ? (text8 + $"\nThe world contains {count} modular cars.") : (text8 + "\nThe world contains 1 modular car."));
			text8 = ((num != 1) ? (text8 + $"\n{num} ({(float)num / (float)count:0%}) are in a completed state.") : (text8 + $"\n1 ({1f / (float)count:0%}) is in a completed state."));
			text8 = ((num2 != 1) ? (text8 + $"\n{num2} ({(float)num2 / (float)count:0%}) are driveable.") : (text8 + $"\n1 ({1f / (float)count:0%}) is driveable."));
			arg.ReplyWith(string.Concat(str1: (num3 != 1) ? (text8 + $"\n{num3} ({(float)num3 / (float)count:0%}) are sheltered indoors.") : (text8 + $"\n1 ({1f / (float)count:0%}) is sheltered indoors."), str0: ((object)val).ToString()));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of all members in the team of the specified player showing Steam ID, username, online status, and whether they are team leader; supports --json")]
	public static string teaminfo(Arg arg)
	{
		ulong num = ArgEx.GetPlayerOrSleeper(arg, 0)?.userID ?? ((EncryptedValue<ulong>)0uL);
		if (num == 0L)
		{
			num = arg.GetULong(0, 0uL);
		}
		if (!SingletonComponent<ServerMgr>.Instance.persistance.DoesPlayerExist(num))
		{
			return "Player not found";
		}
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(num);
		if (playerTeam == null)
		{
			return "Player is not in a team";
		}
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.ResizeColumns(4);
			val.AddColumn("steamID");
			val.AddColumn("username");
			val.AddColumn("online");
			val.AddColumn("leader");
			val.ResizeRows(playerTeam.members.Count);
			foreach (ulong memberId in playerTeam.members)
			{
				bool flag2 = Net.sv.connections.FirstOrDefault((Connection c) => c.connected && c.userid == memberId) != null;
				val.AddValue(memberId);
				val.AddValue(GetPlayerName(memberId));
				val.AddValue(flag2 ? "x" : "");
				val.AddValue((memberId == playerTeam.teamLeader) ? "x" : "");
			}
			return flag ? val.ToJson(true) : $"ID: {playerTeam.teamID}\n\n{val}";
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Authorises the specified player (or caller if none given) to all tool cupboards within the given radius around them")]
	public static void authradius(Arg arg)
	{
		float num = arg.GetFloat(0, -1f);
		if (num < 0f)
		{
			arg.ReplyWith("Format is 'authradius {radius} [user]'");
			return;
		}
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		list.Add(ArgEx.GetPlayer(arg, 1) ?? ArgEx.Player(arg));
		SetAuthInRadius(list[0], list, num, auth: true);
		Pool.FreeUnmanaged<BasePlayer>(ref list);
	}

	[ServerVar(Help = "(Generated) Authorises multiple specified players to all tool cupboards within the given radius around the calling admin")]
	public static void authradius_multi(Arg arg)
	{
		float num = arg.GetFloat(0, -1f);
		if (num < 0f)
		{
			arg.ReplyWith("Format is 'authradius {radius} [user, user, ...]'");
		}
		else
		{
			SetAuthInRadius(ArgEx.Player(arg), ArgEx.GetPlayerArgs(arg, 1), num, auth: true);
		}
	}

	[ServerVar(Help = "(Generated) Finds all players within playerRadius of the caller, then authorises each of them to TCs within authRadius of themselves")]
	public static void authradius_radius(Arg arg)
	{
		run_authradius_radius(arg, authFlag: true);
	}

	[ServerVar(Help = "(Generated) Finds all players within playerRadius of the caller, then deauthorises each of them from TCs within authRadius of themselves")]
	public static void deauthradius_radius(Arg arg)
	{
		run_authradius_radius(arg, authFlag: false);
	}

	private static void run_authradius_radius(Arg arg, bool authFlag)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		float num = arg.GetFloat(0, -1f);
		float num2 = arg.GetFloat(1, -1f);
		if (num < 0f || num2 < 0f)
		{
			arg.ReplyWith("Format is 'authradius_radius {playerRadius, authRadius }'");
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		global::Vis.Entities(((Component)basePlayer).transform.position, num, list, 131072, (QueryTriggerInteraction)2);
		for (int num3 = list.Count - 1; num3 >= 0; num3--)
		{
			BasePlayer basePlayer2 = list[num3];
			if ((Object)(object)basePlayer2 == (Object)null)
			{
				list.RemoveAt(num3);
			}
			else if (basePlayer2.isClient || Vector3.Distance(((Component)basePlayer2).transform.position, ((Component)basePlayer).transform.position) > num)
			{
				list.Remove(basePlayer2);
			}
		}
		SetAuthInRadius(basePlayer, list, num2, authFlag);
		Pool.FreeUnmanaged<BasePlayer>(ref list);
	}

	[ServerVar(Help = "(Generated) Removes authorisation for the specified player (or caller) from all tool cupboards within the given radius")]
	public static void deauthradius(Arg arg)
	{
		float num = arg.GetFloat(0, -1f);
		if (num < 0f)
		{
			arg.ReplyWith("Format is 'deauthradius {radius} [user]'");
			return;
		}
		List<BasePlayer> list = new List<BasePlayer>();
		list.Add(ArgEx.GetPlayer(arg, 1) ?? ArgEx.Player(arg));
		SetAuthInRadius(list[0], list, num, auth: false);
	}

	[ServerVar(Help = "(Generated) Removes authorisation for multiple specified players from all tool cupboards within the given radius around the calling admin")]
	public static void deauthradius_multi(Arg arg)
	{
		float num = arg.GetFloat(0, -1f);
		if (num < 0f)
		{
			arg.ReplyWith("Format is 'deauthradius {radius} [user, user, ...]'");
		}
		else
		{
			SetAuthInRadius(ArgEx.Player(arg), ArgEx.GetPlayerArgs(arg, 1), num, auth: false);
		}
	}

	private static void SetAuthInRadius(BasePlayer radiusTargetPlayer, List<BasePlayer> players, float radius, bool auth)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (players == null)
		{
			return;
		}
		if (players.Count == 0)
		{
			players.Add(radiusTargetPlayer);
		}
		List<BaseEntity> list = new List<BaseEntity>();
		global::Vis.Entities(((Component)radiusTargetPlayer).transform.position, radius, list, -1, (QueryTriggerInteraction)2);
		int num = 0;
		foreach (BaseEntity item in list)
		{
			if (!item.isServer)
			{
				continue;
			}
			bool flag = true;
			foreach (BasePlayer player in players)
			{
				bool flag2 = SetUserAuthorized(item, player.userID, auth);
				if (!flag2)
				{
					flag2 = SetUserAuthorized(item.GetSlot(BaseEntity.Slot.Lock), player.userID, auth);
				}
				if (flag)
				{
					num += (flag2 ? 1 : 0);
					flag = false;
				}
			}
		}
		Debug.Log((object)("Set auth: " + auth + " on " + players.Count + " players, for " + num + " entities."));
	}

	public static bool SetUserAuthorized(BaseEntity entity, ulong userId, bool state)
	{
		if ((Object)(object)entity == (Object)null)
		{
			return false;
		}
		if (entity is CodeLock codeLock)
		{
			if (state)
			{
				codeLock.whitelistPlayers.Add(userId);
			}
			else
			{
				codeLock.whitelistPlayers.Remove(userId);
				codeLock.guestPlayers.Remove(userId);
			}
			codeLock.SendNetworkUpdate();
		}
		else if (entity is AutoTurret autoTurret)
		{
			if (state)
			{
				autoTurret.authorizedPlayers.Add(userId);
			}
			else
			{
				autoTurret.authorizedPlayers.Remove(userId);
			}
			autoTurret.SendNetworkUpdate();
		}
		else if (entity is BuildingPrivlidge buildingPrivlidge)
		{
			if (state)
			{
				buildingPrivlidge.authorizedPlayers.Add(userId);
			}
			else
			{
				buildingPrivlidge.authorizedPlayers.Remove(userId);
			}
			if (entity.GetSlot(BaseEntity.Slot.Lock).IsValid())
			{
				SetUserAuthorized(entity.GetSlot(BaseEntity.Slot.Lock), userId, state);
			}
			buildingPrivlidge.SendNetworkUpdate();
		}
		else if (entity is Tugboat tugboat)
		{
			VehiclePrivilege componentInChildren = ((Component)tugboat).GetComponentInChildren<VehiclePrivilege>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				if (state)
				{
					componentInChildren.authorizedPlayers.Add(userId);
				}
				else
				{
					componentInChildren.authorizedPlayers.Remove(userId);
				}
				componentInChildren.SendNetworkUpdate();
			}
		}
		else if (entity is PlayerBoat playerBoat)
		{
			PlayerBoatPrivilege privilege = playerBoat.GetSteeringWheel().Privilege;
			if ((Object)(object)privilege != (Object)null)
			{
				if (state)
				{
					privilege.authorizedPlayers.Add(userId);
				}
				else
				{
					privilege.authorizedPlayers.Remove(userId);
				}
				privilege.SendNetworkUpdate();
			}
		}
		else
		{
			if (!(entity is ModularCar modularCar))
			{
				return false;
			}
			if (state)
			{
				modularCar.CarLock.TryAddPlayer(userId);
			}
			else
			{
				modularCar.CarLock.TryRemovePlayer(userId);
			}
			modularCar.SendNetworkUpdate();
		}
		return true;
	}

	[ServerVar(Help = "(Generated) Runs an admin command (kill, lock, unlock, etc.) on a specific entity by network ID; blocks operation on players and point entities")]
	public static void entid(Arg arg)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_078c: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07de: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0801: Unknown result type (might be due to invalid IL or missing references)
		//IL_0803: Unknown result type (might be due to invalid IL or missing references)
		//IL_0805: Unknown result type (might be due to invalid IL or missing references)
		//IL_0831: Unknown result type (might be due to invalid IL or missing references)
		//IL_084b: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(ArgEx.GetEntityID(arg, 1)) as BaseEntity;
		if ((Object)(object)baseEntity == (Object)null || baseEntity is BasePlayer || baseEntity is PointEntity)
		{
			return;
		}
		string text = arg.GetString(0);
		if ((Object)(object)ArgEx.Player(arg) != (Object)null)
		{
			Debug.Log((object)string.Format("[ENTCMD] {0}/{1} used *{2}* on ent [{3}/{4}] at position {5}", new object[6]
			{
				ArgEx.Player(arg).displayName,
				ArgEx.Player(arg).userID.Get(),
				text,
				((Object)baseEntity).name,
				baseEntity.net.ID,
				((Component)baseEntity).transform.position
			}));
		}
		switch (text)
		{
		case "kill":
			baseEntity.AdminKill();
			break;
		case "lock":
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope2 = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope2.Set(BaseEntity.Flags.Locked, b: true);
			break;
		}
		case "unlock":
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Locked, b: false);
			break;
		}
		case "open":
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope6 = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope6.Set(BaseEntity.Flags.Open, b: true);
			break;
		}
		case "close":
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope5 = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope5.Set(BaseEntity.Flags.Open, b: false);
			break;
		}
		case "debug":
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope4 = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope4.Set(BaseEntity.Flags.Debugging, b: true);
			break;
		}
		case "undebug":
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope3 = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope3.Set(BaseEntity.Flags.Debugging, b: false);
			break;
		}
		case "who":
			arg.ReplyWith(baseEntity.Admin_Who());
			break;
		case "auth":
			arg.ReplyWith(AuthList(baseEntity));
			break;
		case "upgrade":
			arg.ReplyWith(ChangeGrade(baseEntity, arg.GetInt(2, 1), 0, BuildingGrade.Enum.None, 0uL, arg.GetFloat(3)));
			break;
		case "downgrade":
			arg.ReplyWith(ChangeGrade(baseEntity, 0, arg.GetInt(2, 1), BuildingGrade.Enum.None, 0uL, arg.GetFloat(3)));
			break;
		case "setgrade":
		{
			BuildingGrade buildingGrade = FindBuildingSkin(arg.GetString(2), out var _);
			arg.ReplyWith(ChangeGrade(baseEntity, 0, 0, buildingGrade.type, buildingGrade.skin, arg.GetFloat(3)));
			break;
		}
		case "repair":
			RunInRadius(arg.GetFloat(2), baseEntity, delegate(BaseCombatEntity entity)
			{
				if (entity.repair.enabled)
				{
					entity.SetHealth(entity.MaxHealth());
				}
			});
			break;
		case "maxhp":
		{
			if (!(baseEntity is BaseCombatEntity baseCombatEntity))
			{
				arg.ReplyWith("Entity doesn't support max health!");
				break;
			}
			float num2 = arg.GetFloat(2);
			baseCombatEntity.OverrideMaxHealth(num2);
			if (num2 <= 0f)
			{
				arg.ReplyWith($"Removed max health override from {baseEntity}");
			}
			else
			{
				arg.ReplyWith($"Set max health to {num2}");
			}
			break;
		}
		case "dronetax":
		{
			List<MarketTerminal> list = new List<MarketTerminal>();
			if (baseEntity is Marketplace marketplace)
			{
				list.AddRange(from x in marketplace.terminalEntities
					select x.Get(serverside: true) into x
					where (Object)(object)x != (Object)null
					select x);
			}
			else
			{
				if (!(baseEntity is MarketTerminal item))
				{
					arg.ReplyWith("Entity is not a market terminal!");
					break;
				}
				list.Add(item);
			}
			{
				foreach (MarketTerminal item2 in list)
				{
					string text2 = arg.GetString(2);
					if (int.TryParse(text2, out var result) && result > 0)
					{
						item2.deliveryFeeAmount = result;
						item2.SendNetworkUpdate();
						arg.ReplyWith($"Set drone tax to '{result}'");
						continue;
					}
					ItemDefinition itemDefinition = ItemManager.FindDefinitionByPartialName(text2);
					if ((Object)(object)itemDefinition != (Object)null)
					{
						item2.deliveryFeeCurrency = itemDefinition;
						item2.SendNetworkUpdate();
						arg.ReplyWith("Set drone tax item to '" + itemDefinition.shortname + "'");
					}
					else
					{
						arg.ReplyWith("'" + text2 + "' is not a tax amount or valid item!");
					}
				}
				break;
			}
		}
		case "image":
		{
			if (!(baseEntity is ISignage signage))
			{
				arg.ReplyWith("Entity is not a sign");
				break;
			}
			uint[] textureCRCs = signage.GetTextureCRCs();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"{textureCRCs.Length} Image CRCs");
			uint[] array = textureCRCs;
			foreach (uint num4 in array)
			{
				stringBuilder.AppendLine(num4.ToString());
			}
			arg.ReplyWith(stringBuilder.ToString());
			break;
		}
		case "scale":
		{
			string text3 = arg.GetString(2);
			if (string.IsNullOrEmpty(text3))
			{
				arg.ReplyWith($"Scale: {((Component)baseEntity).transform.localScale}");
				break;
			}
			if (text3 == "default")
			{
				baseEntity.networkEntityScale = false;
				((Component)baseEntity).transform.localScale = Vector3.one;
				baseEntity.SendNetworkUpdate();
				arg.ReplyWith("Reset scale");
				break;
			}
			Vector3 val = Vector3.one;
			if (float.TryParse(text3, out var result2))
			{
				((Vector3)(ref val))._002Ector(result2, result2, result2);
			}
			else
			{
				val = Vector3Ex.Parse(text3);
				if (val == Vector3.zero)
				{
					arg.ReplyWith(text3 + " is not a valid scale");
					break;
				}
			}
			baseEntity.networkEntityScale = true;
			((Component)baseEntity).transform.localScale = val;
			baseEntity.SendNetworkUpdate();
			arg.ReplyWith($"Set scale to {((Component)baseEntity).transform.localScale}");
			break;
		}
		case "settime":
		{
			int num = arg.GetInt(2, -1);
			if (num == -1)
			{
				arg.ReplyWith("Time not provided");
			}
			else if (baseEntity is WipeLaptopEntity wipeLaptopEntity)
			{
				wipeLaptopEntity.SetTimeLeft(num);
				arg.ReplyWith($"Set time left to {num}");
			}
			else
			{
				arg.ReplyWith("Not looking at a laptop");
			}
			break;
		}
		default:
			arg.ReplyWith("Unknown command");
			break;
		}
	}

	private static string AuthList(BaseEntity ent)
	{
		List<ulong> list;
		if (!(ent is BuildingPrivlidge buildingPrivlidge))
		{
			if (!(ent is AutoTurret autoTurret))
			{
				if (ent is CodeLock codeLock)
				{
					return CodeLockAuthList(codeLock);
				}
				if (!(ent is KeyLock keyLock))
				{
					if (!(ent is LegacyShelter legacyShelter))
					{
						if (ent is BaseVehicleModule vehicleModule)
						{
							return CodeLockAuthList(vehicleModule);
						}
						if (!(ent is Tugboat tugboat))
						{
							if (!(ent is SteeringWheel steeringWheel))
							{
								return "Entity has no auth list";
							}
							list = new List<ulong>();
							PlayerBoatPrivilege componentInChildren = ((Component)steeringWheel).GetComponentInChildren<PlayerBoatPrivilege>();
							if ((Object)(object)componentInChildren != (Object)null)
							{
								foreach (ulong authorizedPlayer in componentInChildren.authorizedPlayers)
								{
									list.Add(authorizedPlayer);
								}
							}
						}
						else
						{
							list = new List<ulong>();
							VehiclePrivilege componentInChildren2 = ((Component)tugboat).GetComponentInChildren<VehiclePrivilege>();
							if ((Object)(object)componentInChildren2 != (Object)null)
							{
								foreach (ulong authorizedPlayer2 in componentInChildren2.authorizedPlayers)
								{
									list.Add(authorizedPlayer2);
								}
							}
						}
					}
					else
					{
						list = new List<ulong> { legacyShelter.OwnerID };
					}
				}
				else
				{
					list = new List<ulong> { keyLock.OwnerID };
				}
			}
			else
			{
				list = new List<ulong>();
				foreach (ulong authorizedPlayer3 in autoTurret.authorizedPlayers)
				{
					list.Add(authorizedPlayer3);
				}
			}
		}
		else
		{
			list = new List<ulong>();
			foreach (ulong authorizedPlayer4 in buildingPrivlidge.authorizedPlayers)
			{
				list.Add(authorizedPlayer4);
			}
		}
		if (list == null || list.Count == 0)
		{
			return "Nobody is authed to this entity";
		}
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumn("steamID");
			val.AddColumn("username");
			foreach (ulong item in list)
			{
				val.AddRow(new string[2]
				{
					item.ToString(),
					GetPlayerName(item)
				});
			}
			return ((object)val).ToString();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static string CodeLockAuthList(CodeLock codeLock)
	{
		if (codeLock.whitelistPlayers.Count == 0 && codeLock.guestPlayers.Count == 0)
		{
			return "Nobody is authed to this entity";
		}
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumn("steamID");
			val.AddColumn("username");
			val.AddColumn("isGuest");
			foreach (ulong whitelistPlayer in codeLock.whitelistPlayers)
			{
				val.AddRow(new string[3]
				{
					whitelistPlayer.ToString(),
					GetPlayerName(whitelistPlayer),
					""
				});
			}
			foreach (ulong guestPlayer in codeLock.guestPlayers)
			{
				val.AddRow(new string[3]
				{
					guestPlayer.ToString(),
					GetPlayerName(guestPlayer),
					"x"
				});
			}
			return ((object)val).ToString();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static string CodeLockAuthList(BaseVehicleModule vehicleModule)
	{
		if (!vehicleModule.IsOnAVehicle)
		{
			return "Nobody is authed to this entity";
		}
		ModularCar modularCar = vehicleModule.Vehicle as ModularCar;
		if ((Object)(object)modularCar == (Object)null || !modularCar.IsLockable || modularCar.CarLock.WhitelistPlayers.Count == 0)
		{
			return "Nobody is authed to this entity";
		}
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumn("steamID");
			val.AddColumn("username");
			foreach (ulong whitelistPlayer in modularCar.CarLock.WhitelistPlayers)
			{
				val.AddRow(new string[2]
				{
					whitelistPlayer.ToString(),
					GetPlayerName(whitelistPlayer)
				});
			}
			return ((object)val).ToString();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static string GetPlayerName(ulong steamId)
	{
		BasePlayer basePlayer = BasePlayer.allPlayerList.FirstOrDefault((BasePlayer p) => (ulong)p.userID == steamId);
		string text;
		if (!((Object)(object)basePlayer != (Object)null))
		{
			text = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(steamId);
			if (text == null)
			{
				return "[unknown]";
			}
		}
		else
		{
			text = basePlayer.displayName;
		}
		return text;
	}

	public static string ChangeGrade(BaseEntity entity, int increaseBy = 0, int decreaseBy = 0, BuildingGrade.Enum targetGrade = BuildingGrade.Enum.None, ulong skin = 0uL, float radius = 0f)
	{
		if ((Object)(object)(entity as BuildingBlock) == (Object)null)
		{
			return $"'{entity}' is not a building block";
		}
		int total = 0;
		RunInRadius(radius, entity, delegate(BuildingBlock block)
		{
			BuildingGrade.Enum grade = block.grade;
			if (targetGrade > BuildingGrade.Enum.None && targetGrade < BuildingGrade.Enum.Count)
			{
				grade = targetGrade;
			}
			else
			{
				grade = (BuildingGrade.Enum)Mathf.Min((int)(grade + increaseBy), 4);
				grade = (BuildingGrade.Enum)Mathf.Max((int)(grade - decreaseBy), 0);
			}
			if (grade != block.grade)
			{
				block.ChangeGradeAndSkin(targetGrade, skin);
				total++;
			}
		});
		return $"Upgraded/downgraded '{total}' building block(s)";
	}

	private static bool RunInRadius<T>(float radius, BaseEntity initial, Action<T> callback, Func<T, bool> filter = null, int layerMask = 2097152) where T : BaseEntity
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		List<T> list = Pool.Get<List<T>>();
		radius = Mathf.Clamp(radius, 0f, 200f);
		if (radius > 0f)
		{
			global::Vis.Entities(((Component)initial).transform.position, radius, list, layerMask, (QueryTriggerInteraction)2);
		}
		else if (initial is T item)
		{
			list.Add(item);
		}
		foreach (T item2 in list)
		{
			if (!item2.isClient)
			{
				try
				{
					callback(item2);
				}
				catch (Exception arg)
				{
					Debug.LogError((object)$"Exception while running callback in radius: {arg}");
					Pool.FreeUnmanaged<T>(ref list);
					return false;
				}
			}
		}
		Pool.FreeUnmanaged<T>(ref list);
		return true;
	}

	[ServerVar(Help = "Get a list of players")]
	public static PlayerInfo[] playerlist(Arg arg)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		bool showAddress = arg.Connection == null || arg.Connection.authLevel >= 2;
		List<PlayerInfo> list = ((IEnumerable<BasePlayer>)BasePlayer.activePlayerList).Select(delegate(BasePlayer x)
		{
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			return new PlayerInfo
			{
				SteamID = x.UserIDString,
				OwnerSteamID = x.OwnerID.ToString(),
				DisplayName = x.displayName,
				Ping = Net.sv.GetAveragePing(x.net.connection),
				Address = (showAddress ? x.net.connection.ipaddress : string.Empty),
				EntityId = x.net.ID.Value,
				ConnectedSeconds = (int)x.net.connection.GetSecondsConnected(),
				ViolationLevel = x.ViolationLevel,
				Health = x.Health(),
				Position = ((Component)x).transform.position,
				IsMuted = x.HasPlayerFlag(BasePlayer.PlayerFlags.ChatMute),
				TeamID = x.currentTeam
			};
		}).ToList();
		if (showBotsInPlayerList)
		{
			Enumerator<BasePlayer> enumerator = BasePlayer.bots.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BasePlayer current = enumerator.Current;
					if (!((Object)(object)current == (Object)null) && !current.IsDestroyed)
					{
						list.Add(new PlayerInfo
						{
							SteamID = current.UserIDString,
							OwnerSteamID = current.OwnerID.ToString(),
							DisplayName = current.displayName,
							Ping = 0,
							Address = string.Empty,
							EntityId = ((current.net != null) ? current.net.ID.Value : 0),
							ConnectedSeconds = 0,
							ViolationLevel = current.ViolationLevel,
							Health = current.Health(),
							Position = ((Component)current).transform.position,
							IsMuted = false,
							TeamID = current.currentTeam
						});
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
		return list.ToArray();
	}

	[ServerVar(Help = "Get a list of player's IDs")]
	public static PlayerIDInfo[] playerlistids(Arg arg)
	{
		bool showAddress = arg.Connection == null || arg.Connection.authLevel >= 2;
		return ((IEnumerable<BasePlayer>)BasePlayer.activePlayerList).Select((BasePlayer x) => new PlayerIDInfo
		{
			SteamID = x.UserIDString,
			OwnerSteamID = x.OwnerID.ToString(),
			DisplayName = x.displayName,
			Address = (showAddress ? x.net.connection.ipaddress : string.Empty),
			EntityId = x.net.ID.Value
		}).ToArray();
	}

	[ServerVar(Help = "List of banned users")]
	public static ServerUsers.User[] Bans()
	{
		return ServerUsers.GetAll(ServerUsers.UserGroup.Banned).ToArray();
	}

	[ServerVar(Help = "Get a list of information about the server")]
	public static ServerInfoOutput ServerInfo()
	{
		return new ServerInfoOutput
		{
			Hostname = Server.hostname,
			MaxPlayers = Server.maxplayers,
			Players = BasePlayer.activePlayerList.Count,
			Queued = SingletonComponent<ServerMgr>.Instance.connectionQueue.Queued,
			Joining = SingletonComponent<ServerMgr>.Instance.connectionQueue.Joining,
			ReservedSlots = SingletonComponent<ServerMgr>.Instance.connectionQueue.ReservedCount,
			EntityCount = BaseNetworkable.serverEntities.Count,
			GameTime = (((Object)(object)TOD_Sky.Instance != (Object)null) ? TOD_Sky.Instance.Cycle.DateTime.ToString() : DateTime.UtcNow.ToString()),
			Uptime = (int)Time.realtimeSinceStartup,
			Map = Server.level,
			Framerate = Performance.report.frameRate,
			Memory = (int)Performance.report.memoryAllocations,
			MemoryUsageSystem = (int)Performance.report.memoryUsageSystem,
			Collections = (int)Performance.report.memoryCollections,
			NetworkIn = (int)((Net.sv != null) ? Net.sv.GetStat(null, BaseNetwork.StatTypeLong.BytesReceived_LastSecond) : 0),
			NetworkOut = (int)((Net.sv != null) ? Net.sv.GetStat(null, BaseNetwork.StatTypeLong.BytesSent_LastSecond) : 0),
			Restarting = SingletonComponent<ServerMgr>.Instance.Restarting,
			SaveCreatedTime = SaveRestore.SaveCreatedTime.ToString(),
			Version = 2632,
			Protocol = Protocol.printable
		};
	}

	[ServerVar(Help = "Get information about this build")]
	public static BuildInfo BuildInfo()
	{
		return BuildInfo.Current;
	}

	[ServerVar(Help = "(Generated) Triggers a full refresh of the admin UI by requesting the player list, server info, convars, and UGC list all at once")]
	public static void AdminUI_FullRefresh(Arg arg)
	{
		AdminUI_RequestPlayerList(arg);
		AdminUI_RequestServerInfo(arg);
		AdminUI_RequestServerConvars(arg);
		AdminUI_RequestUGCList(arg);
	}

	[ServerVar(Help = "(Generated) Server-side handler that serialises and sends the current player list to the requesting admin client for display in the admin UI")]
	public static void AdminUI_RequestPlayerList(Arg arg)
	{
		if (allowAdminUI)
		{
			ConsoleNetwork.SendClientCommand(arg.Connection, "AdminUI_ReceivePlayerList", JsonConvert.SerializeObject((object)playerlist(arg)));
		}
	}

	[ServerVar(Help = "(Generated) Server-side handler that serialises and sends current server info (name, players, FPS, etc.) to the requesting admin client")]
	public static void AdminUI_RequestServerInfo(Arg arg)
	{
		if (allowAdminUI)
		{
			ConsoleNetwork.SendClientCommand(arg.Connection, "AdminUI_ReceiveServerInfo", JsonConvert.SerializeObject((object)ServerInfo()));
		}
	}

	[ServerVar(Help = "(Generated) Server-side handler that collects all ServerAdmin+ShowInAdminUI convars and sends them to the admin client for editing via the admin UI")]
	public static void AdminUI_RequestServerConvars(Arg arg)
	{
		if (!allowAdminUI)
		{
			return;
		}
		List<ServerConvarInfo> list = Pool.Get<List<ServerConvarInfo>>();
		Command[] all = Index.All;
		foreach (Command command in all)
		{
			if (command.Server && command.Variable && command.ServerAdmin && command.ShowInAdminUI && !command.RconOnly)
			{
				list.Add(new ServerConvarInfo
				{
					FullName = command.FullName,
					Value = command.GetOveride?.Invoke(),
					Help = command.Description
				});
			}
		}
		ConsoleNetwork.SendClientCommand(arg.Connection, "AdminUI_ReceiveCommands", JsonConvert.SerializeObject((object)list));
		Pool.FreeUnmanaged<ServerConvarInfo>(ref list);
	}

	[ServerVar(Help = "(Generated) Server-side handler that scans all entities for UGC content (images, patterns, vending names) and sends a serialised list to the admin client")]
	public static void AdminUI_RequestUGCList(Arg arg)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!allowAdminUI)
		{
			return;
		}
		List<ServerUGCInfo> list = Pool.Get<List<ServerUGCInfo>>();
		uint[] array = null;
		ulong[] array2 = null;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			IUGCBrowserEntity iUGCBrowserEntity = default(IUGCBrowserEntity);
			while (enumerator.MoveNext())
			{
				BaseNetworkable current = enumerator.Current;
				if (ObjectEx.IsUnityNull(current))
				{
					continue;
				}
				array = null;
				array2 = null;
				UGCType uGCType = UGCType.ImageJpg;
				string text = string.Empty;
				if (((Component)current).TryGetComponent<IUGCBrowserEntity>(ref iUGCBrowserEntity))
				{
					if ((Object)(object)iUGCBrowserEntity.UgcEntity == (Object)null)
					{
						continue;
					}
					array = iUGCBrowserEntity.GetContentCRCs;
					array2 = iUGCBrowserEntity.EditingHistory.ToArray();
					uGCType = iUGCBrowserEntity.ContentType;
					text = iUGCBrowserEntity.ContentString;
				}
				bool flag = false;
				if (array != null)
				{
					uint[] array3 = array;
					for (int i = 0; i < array3.Length; i++)
					{
						if (array3[i] != 0)
						{
							flag = true;
							break;
						}
					}
				}
				if (uGCType == UGCType.PatternBoomer)
				{
					flag = true;
					PatternFirework patternFirework = iUGCBrowserEntity as PatternFirework;
					if ((Object)(object)patternFirework != (Object)null && patternFirework.Design == null)
					{
						flag = false;
					}
				}
				if (uGCType == UGCType.VendingMachine && !string.IsNullOrEmpty(text))
				{
					flag = true;
				}
				if (flag)
				{
					list.Add(new ServerUGCInfo
					{
						entityId = current.net.ID.Value,
						crcs = array,
						contentType = uGCType,
						entityPrefabID = current.prefabID,
						shortPrefabName = current.ShortPrefabName,
						playerIds = array2,
						contentString = text
					});
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		ConsoleNetwork.SendClientCommand(arg.Connection, "AdminUI_ReceiveUGCList", JsonConvert.SerializeObject((object)list));
		Pool.FreeUnmanaged<ServerUGCInfo>(ref list);
	}

	[ServerVar(Help = "(Generated) Server-side handler that retrieves a specific UGC data blob by CRC, entity ID, and type and sends it to the requesting admin client")]
	public static void AdminUI_RequestUGCContent(Arg arg)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (allowAdminUI && !((Object)(object)ArgEx.Player(arg) == (Object)null))
		{
			uint uInt = arg.GetUInt(0);
			NetworkableId entityID = ArgEx.GetEntityID(arg, 1);
			FileStorage.Type type = (FileStorage.Type)arg.GetInt(2);
			uint uInt2 = arg.GetUInt(3);
			byte[] array = FileStorage.server.Get(uInt, type, entityID, uInt2);
			if (array != null)
			{
				SendInfo sendInfo = new SendInfo(arg.Connection);
				sendInfo.channel = 2;
				sendInfo.method = SendMethod.Reliable;
				SendInfo sendInfo2 = sendInfo;
				ArgEx.Player(arg).ClientRPC(RpcTarget.SendInfo("AdminReceivedUGC", sendInfo2), uInt, (uint)array.Length, array, uInt2, (byte)type);
			}
		}
	}

	[ServerVar(Help = "(Generated) Clears all UGC content (images, patterns) from the entity with the given network ID and notifies the IUGCBrowserEntity component")]
	public static void AdminUI_DeleteUGCContent(Arg arg)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!allowAdminUI)
		{
			return;
		}
		NetworkableId entityID = ArgEx.GetEntityID(arg, 0);
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(entityID);
		if ((Object)(object)baseNetworkable != (Object)null)
		{
			FileStorage.server.RemoveAllByEntity(entityID);
			IUGCBrowserEntity iUGCBrowserEntity = default(IUGCBrowserEntity);
			if (((Component)baseNetworkable).TryGetComponent<IUGCBrowserEntity>(ref iUGCBrowserEntity))
			{
				iUGCBrowserEntity.ClearContent();
			}
		}
	}

	[ServerVar(Help = "(Generated) Sends the firework pattern design data for the specified pattern firework entity to the requesting admin client")]
	public static void AdminUI_RequestFireworkPattern(Arg arg)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (allowAdminUI)
		{
			NetworkableId entityID = ArgEx.GetEntityID(arg, 0);
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(entityID);
			if ((Object)(object)baseNetworkable != (Object)null && baseNetworkable is PatternFirework { Design: not null } patternFirework)
			{
				SendInfo sendInfo = new SendInfo(arg.Connection);
				sendInfo.channel = 2;
				sendInfo.method = SendMethod.Reliable;
				SendInfo sendInfo2 = sendInfo;
				ArgEx.Player(arg).ClientRPC(RpcTarget.SendInfo("AdminReceivedPatternFirework", sendInfo2), entityID, ProtoStreamExtensions.ToProtoBytes((IProto)(object)patternFirework.Design));
			}
		}
	}

	[ServerVar(Help = "(Generated) Clears all UGC content from a single entity by network ID; reports success or failure")]
	public static void clearugcentity(Arg arg)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId entityID = ArgEx.GetEntityID(arg, 0);
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(entityID);
		IUGCBrowserEntity iUGCBrowserEntity = default(IUGCBrowserEntity);
		if ((Object)(object)baseNetworkable != (Object)null && ((Component)baseNetworkable).TryGetComponent<IUGCBrowserEntity>(ref iUGCBrowserEntity))
		{
			iUGCBrowserEntity.ClearContent();
			arg.ReplyWith($"Cleared content on {baseNetworkable.ShortPrefabName}/{entityID}");
		}
		else
		{
			arg.ReplyWith($"Could not find UGC entity with id {entityID}");
		}
	}

	[ServerVar(Help = "(Generated) Clears UGC content from all entities within the given radius of a world position; reports how many entities were cleared")]
	public static void clearugcentitiesinrange(Arg arg)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vector = arg.GetVector3(0);
		float num = arg.GetFloat(1);
		int num2 = 0;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			IUGCBrowserEntity iUGCBrowserEntity = default(IUGCBrowserEntity);
			while (enumerator.MoveNext())
			{
				BaseNetworkable current = enumerator.Current;
				if (((Component)current).TryGetComponent<IUGCBrowserEntity>(ref iUGCBrowserEntity) && Vector3.Distance(((Component)current).transform.position, vector) <= num)
				{
					iUGCBrowserEntity.ClearContent();
					num2++;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		arg.ReplyWith($"Cleared {num2} UGC entities within {num}m of {vector}");
	}

	[ServerVar(Help = "(Generated) Clears the custom name UGC from all vending machines whose content string contains the given search text (case/symbol insensitive)")]
	public static void clearVendingMachineNamesContaining(Arg arg)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		string text = arg.GetString(0);
		int num = 0;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			IUGCBrowserEntity iUGCBrowserEntity = default(IUGCBrowserEntity);
			while (enumerator.MoveNext())
			{
				if (((Component)enumerator.Current).TryGetComponent<IUGCBrowserEntity>(ref iUGCBrowserEntity) && iUGCBrowserEntity.ContentType == UGCType.VendingMachine && StringEx.Contains(iUGCBrowserEntity.ContentString, text, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols))
				{
					iUGCBrowserEntity.ClearContent();
					num++;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		arg.ReplyWith($"Cleared {num} vending machines containing {text}");
	}

	[ServerVar(Help = "(Generated) Clears UGC content from all entities that have the specified player (by name or Steam ID) in their editing history")]
	public static void clearUGCByPlayer(Arg arg)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		ulong num = (((Object)(object)playerOrSleeper == (Object)null) ? arg.GetULong(0, 0uL) : playerOrSleeper.userID.Get());
		int num2 = 0;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			IUGCBrowserEntity iUGCBrowserEntity = default(IUGCBrowserEntity);
			while (enumerator.MoveNext())
			{
				if (((Component)enumerator.Current).TryGetComponent<IUGCBrowserEntity>(ref iUGCBrowserEntity) && iUGCBrowserEntity.EditingHistory.Contains(num))
				{
					iUGCBrowserEntity.ClearContent();
					num2++;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		arg.ReplyWith($"Cleared {num2} UGC entities modified by {(((Object)(object)playerOrSleeper != (Object)null) ? playerOrSleeper.displayName : ((object)num))}");
	}

	[ServerVar(Help = "(Generated) Returns a JSON object containing the UGC info (CRCs, type, player history) for the entity with the given network ID")]
	public static void getugcinfo(Arg arg)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId entityID = ArgEx.GetEntityID(arg, 0);
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(entityID);
		IUGCBrowserEntity iUGCBrowserEntity = default(IUGCBrowserEntity);
		if ((Object)(object)baseNetworkable != (Object)null && ((Component)baseNetworkable).TryGetComponent<IUGCBrowserEntity>(ref iUGCBrowserEntity) && (Object)(object)iUGCBrowserEntity.UgcEntity != (Object)null)
		{
			ServerUGCInfo serverUGCInfo = new ServerUGCInfo(iUGCBrowserEntity);
			arg.ReplyWith(JsonConvert.SerializeObject((object)serverUGCInfo));
		}
		else
		{
			arg.ReplyWith($"Invalid entity id: {entityID}");
		}
	}

	[ServerVar(Help = "Returns all entities that the provided player is authed to (TC's, locks, etc), supports --json")]
	public static void authcount(Arg arg)
	{
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		ulong num = ArgEx.GetPlayerOrSleeper(arg, 0)?.userID ?? ((EncryptedValue<ulong>)0uL);
		if (num == 0L)
		{
			num = arg.GetULong(0, 0uL);
		}
		if (!SingletonComponent<ServerMgr>.Instance.persistance.DoesPlayerExist(num))
		{
			arg.ReplyWith("Please provide a valid player, unable to find '" + arg.GetString(0) + "'");
			return;
		}
		string playerName = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(num);
		string text = arg.GetString(1);
		if (text == "--json")
		{
			text = string.Empty;
		}
		List<EntityAssociation> list = Pool.Get<List<EntityAssociation>>();
		FindEntityAssociationsForPlayer(num, useOwnerId: false, useAuth: true, text, list);
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.AddColumns(new string[4] { "Prefab name", "Position", "ID", "Type" });
			foreach (EntityAssociation item in list)
			{
				val.AddRow(new string[4]
				{
					item.TargetEntity.ShortPrefabName,
					((object)((Component)item.TargetEntity).transform.position/*cast due to constrained. prefix*/).ToString(),
					((object)Unsafe.As<NetworkableId, NetworkableId>(ref item.TargetEntity.net.ID)/*cast due to constrained. prefix*/).ToString(),
					item.AssociationType.ToString()
				});
			}
			Pool.FreeUnmanaged<EntityAssociation>(ref list);
			if (flag)
			{
				arg.ReplyWith(val.ToJson(true));
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Found entities " + playerName + " is authed to");
			stringBuilder.AppendLine(((object)val).ToString());
			arg.ReplyWith(stringBuilder.ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Returns all entities that the provided player has placed, supports --json")]
	public static void entcount(Arg arg)
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		ulong num = ArgEx.GetPlayerOrSleeper(arg, 0)?.userID ?? ((EncryptedValue<ulong>)0uL);
		if (num == 0L)
		{
			num = arg.GetULong(0, 0uL);
		}
		if (!SingletonComponent<ServerMgr>.Instance.persistance.DoesPlayerExist(num))
		{
			arg.ReplyWith("Please provide a valid player, unable to find '" + arg.GetString(0) + "'");
			return;
		}
		string playerName = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(num);
		string text = arg.GetString(1);
		if (text == "--json")
		{
			text = string.Empty;
		}
		List<EntityAssociation> list = Pool.Get<List<EntityAssociation>>();
		FindEntityAssociationsForPlayer(num, useOwnerId: true, useAuth: false, text, list);
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.AddColumns(new string[3] { "Prefab name", "Position", "ID" });
			foreach (EntityAssociation item in list)
			{
				val.AddRow(new string[3]
				{
					item.TargetEntity.ShortPrefabName,
					((object)((Component)item.TargetEntity).transform.position/*cast due to constrained. prefix*/).ToString(),
					((object)Unsafe.As<NetworkableId, NetworkableId>(ref item.TargetEntity.net.ID)/*cast due to constrained. prefix*/).ToString()
				});
			}
			Pool.FreeUnmanaged<EntityAssociation>(ref list);
			if (flag)
			{
				arg.ReplyWith(val.ToJson(true));
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Found entities associated with " + playerName);
			stringBuilder.AppendLine(((object)val).ToString());
			arg.ReplyWith(stringBuilder.ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void FindEntityAssociationsForPlayer(ulong steamId, bool useOwnerId, bool useAuth, string filter, List<EntityAssociation> results)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		results.Clear();
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseNetworkable current = enumerator.Current;
				EntityAssociationType entityAssociationType = EntityAssociationType.Owner;
				if (!(current is BaseEntity baseEntity))
				{
					continue;
				}
				bool flag = false;
				if (useOwnerId && baseEntity.OwnerID == steamId)
				{
					flag = true;
				}
				if (useAuth && !flag)
				{
					if (!flag && baseEntity is BuildingPrivlidge buildingPrivlidge && buildingPrivlidge.IsAuthed(steamId))
					{
						flag = true;
					}
					if (!flag && baseEntity is SimplePrivilege simplePrivilege && simplePrivilege.IsAuthed(steamId))
					{
						flag = true;
					}
					if (!flag && baseEntity is KeyLock keyLock && keyLock.OwnerID == steamId)
					{
						flag = true;
					}
					else if (baseEntity is CodeLock codeLock)
					{
						if (codeLock.whitelistPlayers.Contains(steamId))
						{
							flag = true;
						}
						else if (codeLock.guestPlayers.Contains(steamId))
						{
							flag = true;
							entityAssociationType = EntityAssociationType.LockGuest;
						}
					}
					if (!flag && baseEntity is ModularCar { IsLockable: not false } modularCar && modularCar.CarLock.HasLockPermission(steamId))
					{
						flag = true;
					}
					if (!flag && baseEntity is AutoTurret autoTurret && autoTurret.IsAuthed(steamId))
					{
						flag = true;
					}
					if (flag && entityAssociationType == EntityAssociationType.Owner)
					{
						entityAssociationType = EntityAssociationType.Auth;
					}
				}
				if (flag && !string.IsNullOrEmpty(filter) && !StringEx.Contains(current.ShortPrefabName, filter, CompareOptions.IgnoreCase))
				{
					flag = false;
				}
				if (flag)
				{
					results.Add(new EntityAssociation
					{
						TargetEntity = baseEntity,
						AssociationType = entityAssociationType
					});
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
