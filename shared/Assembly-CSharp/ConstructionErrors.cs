using System.Collections.Generic;
using Facepunch;
using Rust.Ai.Gen2;
using UnityEngine;

public static class ConstructionErrors
{
	public static readonly Phrase NoPermission;

	public static readonly Phrase StackPrivilege;

	public static readonly Phrase CantBuildWhileMoving;

	public static readonly Phrase ThroughRock;

	public static readonly Phrase ThroughWalls;

	public static readonly Phrase InsideObjects;

	public static readonly Phrase TooCloseToRoad;

	public static readonly Phrase TooFarAway;

	public static readonly Phrase BlockedBy;

	public static readonly Phrase BlockedByPlayer;

	public static readonly Phrase BlockedByVehicle;

	public static readonly Phrase TooCloseTo;

	public static readonly Phrase TooCloseToMonument;

	public static readonly Phrase BlockedByTree;

	public static readonly Phrase BlockedByTerrain;

	public static readonly Phrase BlockedByPiping;

	public static readonly Phrase SkinNotOwned;

	public static readonly Phrase CannotBuildInThisArea;

	public static readonly Phrase CannotBuildInTheDeepSea;

	public static readonly Phrase CannotBuildOnBoat;

	public static readonly Phrase BoatBuildingEntityLimit;

	public static readonly Phrase NotEnoughSpace;

	public static readonly Phrase NotStableEnough;

	public static readonly Phrase MustPlaceOnConstruction;

	public static readonly Phrase MustPlaceOnBoat;

	public static readonly Phrase CannotPlaceOnBoat;

	public static readonly Phrase RequiresHull;

	public static readonly Phrase CantPlaceOnNetting;

	public static readonly Phrase MustPlaceOnNetting;

	public static readonly Phrase CantPlaceOnConstruction;

	public static readonly Phrase CantPlaceOnMonument;

	public static readonly Phrase NotInTerrain;

	public static readonly Phrase MustPlaceOnRoad;

	public static readonly Phrase CantPlaceOnRoad;

	public static readonly Phrase InvalidAreaVehicleLarge;

	public static readonly Phrase InvalidAngle;

	public static readonly Phrase InvalidEntity;

	public static readonly Phrase InvalidEntityType;

	public static readonly Phrase WantsWater;

	public static readonly Phrase WantsWaterBody;

	public static readonly Phrase InWater;

	public static readonly Phrase TooDeep;

	public static readonly Phrase TooShallow;

	public static readonly Phrase CouldntFindConstruction;

	public static readonly Phrase CouldntFindEntity;

	public static readonly Phrase CouldntFindSocket;

	public static readonly Phrase Antihack;

	public static readonly Phrase AntihackWithReason;

	public static readonly Phrase CantDeployOnDoor;

	public static readonly Phrase DeployableMismatch;

	public static readonly Phrase LineOfSightBlocked;

	public static readonly Phrase ParentTooFar;

	public static readonly Phrase SocketOccupied;

	public static readonly Phrase SocketNotFemale;

	public static readonly Phrase WantsInside;

	public static readonly Phrase WantsOutside;

	public static readonly Phrase PlayerName;

	public static readonly Phrase HorseName;

	public static readonly Phrase ModularCarName;

	public static readonly Phrase TreeName;

	public static readonly Phrase DebrisName;

	public static readonly Phrase OreName;

	public static readonly Phrase DroppedItemName;

	public static readonly Phrase CannotAttachToUnauthorized;

	public static readonly Phrase CannotConnectTwoBuildings;

	public static readonly Phrase CantUpgradeRecentlyDamaged;

	public static readonly Phrase CantRotateAnymore;

	public static readonly Phrase CantDemolishAnymore;

	public static string GetTranslatedNameFromEntity(BaseEntity entity, BasePlayer fromPlayer = null)
	{
		if (entity is ModularCar || entity is BaseVehicleModule)
		{
			return ModularCarName.translated;
		}
		if (entity is BaseVehicleSeat && entity.parentEntity.Get(serverside: false) is RidableHorse)
		{
			return HorseName.translated;
		}
		if (entity is BaseNPC2 baseNPC)
		{
			return baseNPC.displayName;
		}
		if (entity is RidableHorse || entity is HorseSaddle)
		{
			return HorseName.translated;
		}
		if (entity is HumanNPC humanNPC)
		{
			return humanNPC.displayName;
		}
		if (entity is BasePlayer { displayName: var arg } basePlayer)
		{
			if ((Object)(object)fromPlayer != (Object)null)
			{
				arg = NameHelper.GetPlayerNameStreamSafe(fromPlayer, basePlayer);
			}
			return string.Format(PlayerName.translated, arg);
		}
		if (entity is BuildingBlock buildingBlock)
		{
			return PrefabAttribute.server.Find<Construction>(buildingBlock.prefabID).info.name.translated;
		}
		if (entity is DebrisEntity)
		{
			return DebrisName.translated;
		}
		if (entity is TreeEntity)
		{
			return TreeName.translated;
		}
		if (entity is OreResourceEntity)
		{
			return OreName.translated;
		}
		if (entity is DroppedItem)
		{
			return DroppedItemName.translated;
		}
		SprayCan.GetItemDefinitionForEntity(entity, out var def);
		if ((Object)(object)def != (Object)null)
		{
			return def.displayName.translated;
		}
		return string.Empty;
	}

	public static string GetBlockedByErrorFromEntity(BaseEntity entity, BasePlayer fromPlayer = null)
	{
		string translatedNameFromEntity = GetTranslatedNameFromEntity(entity, fromPlayer);
		if (!string.IsNullOrEmpty(translatedNameFromEntity))
		{
			return string.Format(BlockedBy.translated, translatedNameFromEntity);
		}
		return null;
	}

	public static string GetBlockedByErrorFromCollider(Collider col, BasePlayer fromPlayer = null)
	{
		if ((Object)(object)col == (Object)null)
		{
			return null;
		}
		PreventBuildingMonumentTag preventBuildingMonumentTag = GetPreventBuildingMonumentTag(col);
		if ((Object)(object)preventBuildingMonumentTag != (Object)null)
		{
			return string.Format(TooCloseToMonument.translated, preventBuildingMonumentTag.GetAttachedMonument().displayPhrase.translated);
		}
		ColliderInfo_Pipe colliderInfo_Pipe = default(ColliderInfo_Pipe);
		if (((Component)col).TryGetComponent<ColliderInfo_Pipe>(ref colliderInfo_Pipe))
		{
			return BlockedByPiping.translated;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(col);
		if ((Object)(object)baseEntity != (Object)null)
		{
			return GetBlockedByErrorFromEntity(baseEntity, fromPlayer);
		}
		if (col is TerrainCollider)
		{
			return BlockedByTerrain.translated;
		}
		return null;
	}

	public static bool IsBuildBlockedByMonument(Vector3 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapSphere(pos, 0.1f, list, 536870912, (QueryTriggerInteraction)2);
		PreventBuildingMonumentTag preventBuildingMonumentTag = default(PreventBuildingMonumentTag);
		foreach (Collider item in list)
		{
			if (((Component)item).TryGetComponent<PreventBuildingMonumentTag>(ref preventBuildingMonumentTag) && (Object)(object)preventBuildingMonumentTag.GetAttachedMonument() != (Object)null)
			{
				Pool.FreeUnmanaged<Collider>(ref list);
				return true;
			}
		}
		Pool.FreeUnmanaged<Collider>(ref list);
		return false;
	}

	public static PreventBuildingMonumentTag GetPreventBuildingMonumentTag(Collider col)
	{
		PreventBuildingMonumentTag preventBuildingMonumentTag = default(PreventBuildingMonumentTag);
		if ((Object)(object)col != (Object)null && ((Component)col).TryGetComponent<PreventBuildingMonumentTag>(ref preventBuildingMonumentTag) && (Object)(object)preventBuildingMonumentTag.GetAttachedMonument() != (Object)null && !((Component)preventBuildingMonumentTag).gameObject.HasCustomTag(GameObjectTag.BlockPlacement))
		{
			return preventBuildingMonumentTag;
		}
		return null;
	}

	public static void Log(BasePlayer player, string message)
	{
		if (!((Object)(object)player == (Object)null) && !string.IsNullOrEmpty(message) && player.isServer && player.net.connection != null && player.net.connection.info.GetBool("client.errortoasts_debug"))
		{
			player.ChatMessage(message);
		}
	}

	static ConstructionErrors()
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
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected O, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected O, but got Unknown
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected O, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Expected O, but got Unknown
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Expected O, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Expected O, but got Unknown
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Expected O, but got Unknown
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Expected O, but got Unknown
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Expected O, but got Unknown
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Expected O, but got Unknown
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Expected O, but got Unknown
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Expected O, but got Unknown
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Expected O, but got Unknown
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Expected O, but got Unknown
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Expected O, but got Unknown
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Expected O, but got Unknown
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Expected O, but got Unknown
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Expected O, but got Unknown
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Expected O, but got Unknown
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Expected O, but got Unknown
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Expected O, but got Unknown
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Expected O, but got Unknown
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Expected O, but got Unknown
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Expected O, but got Unknown
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Expected O, but got Unknown
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Expected O, but got Unknown
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Expected O, but got Unknown
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Expected O, but got Unknown
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Expected O, but got Unknown
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Expected O, but got Unknown
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Expected O, but got Unknown
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Expected O, but got Unknown
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Expected O, but got Unknown
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Expected O, but got Unknown
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Expected O, but got Unknown
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Expected O, but got Unknown
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Expected O, but got Unknown
		//IL_042e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Expected O, but got Unknown
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Expected O, but got Unknown
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Expected O, but got Unknown
		//IL_046a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Expected O, but got Unknown
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Expected O, but got Unknown
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Expected O, but got Unknown
		//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Expected O, but got Unknown
		//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Expected O, but got Unknown
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d8: Expected O, but got Unknown
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Expected O, but got Unknown
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Expected O, but got Unknown
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Expected O, but got Unknown
		//IL_051e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Expected O, but got Unknown
		//IL_0532: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Expected O, but got Unknown
		//IL_0546: Unknown result type (might be due to invalid IL or missing references)
		//IL_0550: Expected O, but got Unknown
		NoPermission = new Phrase("error_buildpermission", "You don't have permission to build here");
		StackPrivilege = new Phrase("error_stackprivilege", "Cannot stack building privileges");
		CantBuildWhileMoving = new Phrase("error_whilemoving", "You can't build this while moving");
		ThroughRock = new Phrase("error_throughrock", "Placing through rock");
		ThroughWalls = new Phrase("error_throughwalls", "Placing through walls");
		InsideObjects = new Phrase("error_insideobjects", "Can't deploy inside objects");
		TooCloseToRoad = new Phrase("error_tooclosetoroad", "Placing too close to road");
		TooFarAway = new Phrase("error_toofar", "Too far away");
		BlockedBy = new Phrase("error_blockedby", "Blocked by {0}");
		BlockedByPlayer = new Phrase("error_blockedbyplayer", "Blocked by Player {0}");
		BlockedByVehicle = new Phrase("error_blockedbyvehicle", "Blocked by Vehicle");
		TooCloseTo = new Phrase("error_toocloseto", "Too close to {0}");
		TooCloseToMonument = new Phrase("error_tooclosetomonument", "Cannot build this close to {0}");
		BlockedByTree = new Phrase("error_blockedbytree", "Blocked by tree");
		BlockedByTerrain = new Phrase("error_blockedbyterrain", "Blocked by terrain");
		BlockedByPiping = new Phrase("error_blockedbypiping", "Blocked by industrial piping");
		SkinNotOwned = new Phrase("error_skinnotowned", "Skin not owned");
		CannotBuildInThisArea = new Phrase("error_cannotbuildarea", "Cannot build in this area");
		CannotBuildInTheDeepSea = new Phrase("error_cannotbuilddeepsea", "Cannot build in the deep sea");
		CannotBuildOnBoat = new Phrase("error_cannotbuildboat", "Cannot be deployed on boats");
		BoatBuildingEntityLimit = new Phrase("error_boatbuilding_entity_limit", "Cannot deploy any more of this item.");
		NotEnoughSpace = new Phrase("error_notenoughspace", "Not enough space");
		NotStableEnough = new Phrase("error_notstableenough", "Not stable enough");
		MustPlaceOnConstruction = new Phrase("error_wantsconstruction", "Must be placed on a construction");
		MustPlaceOnBoat = new Phrase("error_wantsboat", "Must be placed on a player boat in edit mode");
		CannotPlaceOnBoat = new Phrase("error_no_boat", "Cannot be placed on boats");
		RequiresHull = new Phrase("error_requiresHull", "Must be placed on the boat hull");
		CantPlaceOnNetting = new Phrase("error_placement_no_netting", "Can't be placed on boat building netting");
		MustPlaceOnNetting = new Phrase("error_placement_needs_netting", "Must be placed on boat building netting");
		CantPlaceOnConstruction = new Phrase("error_doesnotwantconstruction", "Cannot be placed on constructions");
		CantPlaceOnMonument = new Phrase("error_cantplaceonmonument", "Cannot be placed on monument");
		NotInTerrain = new Phrase("error_notinterrain", "Not in terrain");
		MustPlaceOnRoad = new Phrase("error_placement_needs_road", "Must be placed on road");
		CantPlaceOnRoad = new Phrase("error_placement_no_road", "Cannot be placed on road");
		InvalidAreaVehicleLarge = new Phrase("error_invalidarea_vehiclelarge", "Cannot deploy near a large vehicle");
		InvalidAngle = new Phrase("error_invalidangle", "Invalid angle");
		InvalidEntity = new Phrase("error_invalidentitycheck", "Invalid entity");
		InvalidEntityType = new Phrase("error_invalidentitytype", "Invalid entity type");
		WantsWater = new Phrase("error_inwater_wants", "Must be placed in water");
		WantsWaterBody = new Phrase("error_inwater_wants_body", "Must be placed in a body of water");
		InWater = new Phrase("error_inwater", "Can't be placed in water");
		TooDeep = new Phrase("error_toodeep", "Water is too deep");
		TooShallow = new Phrase("error_shallow", "Water is too shallow");
		CouldntFindConstruction = new Phrase("error_counlndfindconstruction", "Couldn't find construction");
		CouldntFindEntity = new Phrase("error_counlndfindentity", "Couldn't find entity");
		CouldntFindSocket = new Phrase("error_counlndfindsocket", "Couldn't find socket");
		Antihack = new Phrase("error_antihack", "Anti hack!");
		AntihackWithReason = new Phrase("error_antihack_reason", "Anti hack! ({0})");
		CantDeployOnDoor = new Phrase("error_cantdeployondoor", "Can't deploy on door");
		DeployableMismatch = new Phrase("error_deployablemismatch", "Deployable mismatch!");
		LineOfSightBlocked = new Phrase("error_lineofsightblocked", "Line of sight blocked");
		ParentTooFar = new Phrase("error_parenttoofar", "Parent too far away");
		SocketOccupied = new Phrase("error_sockectoccupied", "Target socket is occupied");
		SocketNotFemale = new Phrase("error_socketnotfemale", "Target socket is not female");
		WantsInside = new Phrase("error_wantsinside", "Must be placed inside your base");
		WantsOutside = new Phrase("error_wantsoutside", "Can't be placed inside a base");
		PlayerName = new Phrase("error_name_player", "Player {0}");
		HorseName = new Phrase("error_name_horse", "Horse");
		ModularCarName = new Phrase("error_name_modularcar", "Modular Car");
		TreeName = new Phrase("error_name_tree", "Tree");
		DebrisName = new Phrase("error_name_debris", "Debris");
		OreName = new Phrase("error_name_ore", "Ore");
		DroppedItemName = new Phrase("error_dropped_item", "Dropped item");
		CannotAttachToUnauthorized = new Phrase("error_cannotattachtounauth", "Cannot attach to unauthorized building");
		CannotConnectTwoBuildings = new Phrase("error_connecttwobuildings", "Cannot connect two buildings with cupboards");
		CantUpgradeRecentlyDamaged = new Phrase("error_upgraderecentlydamaged", "Recently damaged, upgradable in {0} seconds");
		CantRotateAnymore = new Phrase("grade_rotationblocked", "Can't rotate this block anymore");
		CantDemolishAnymore = new Phrase("grade_demolishblocked", "Can't demolish this block anymore");
	}
}
