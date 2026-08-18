using System;
using System.Linq;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class SprayCan : HeldEntity
{
	public struct ReskinPreserveInfo
	{
		public BaseEntityPreserveInfo baseEntityPreserve;

		public BaseCombatEntity.BaseCombatEntityPreserveInfo baseCombatEntityPreserve;

		public DecayEntity.DecayEntityPreserveInfo decayEntityPreserve;

		public CodeLock.CodeLockPreserveInfo codeLockPreserve;

		public PlanterBox.PlanterBoxPreserveInfo planterBoxPreserve;

		public BuildingPrivlidge.BuildingPrivilegePreserveInfo buildingPrivilegePreserve;

		public IItemContainerEntity.ContainerPreserveInfo containerPreserve;

		public IOEntity.IOEntityPreserveInfo ioEntityPreserve;

		public AutoTurret.AutoTurretPreserveInfo autoTurretPreserve;

		public ComputerStation.ComputerStationPreserveInfo computerStationPreserve;

		public ElectricOven.ElectricOvenPreserveInfo electricOvenPreserve;
	}

	public const float MaxFreeSprayDistanceFromStart = 10f;

	public const float MaxFreeSprayStartingDistance = 3f;

	private SprayCanSpray_Freehand paintingLine;

	public const Flags IsFreeSpraying = Flags.Reserved1;

	public static Phrase LastReskinError;

	public static BaseEntity LastReskinErrorEntity;

	public static string LastReskinErrorArgString;

	public static readonly Phrase FreeSprayNamePhrase;

	public static readonly Phrase FreeSprayDescPhrase;

	public static readonly Phrase BuildingSkinColourPhrase;

	public static readonly Phrase BuildingSkinColourDescPhrase;

	public static readonly Phrase EntityChangeSkinPhrase;

	public static readonly Phrase EntityChangeSkinDescPhrase;

	public static readonly Phrase EntityChangeColourPhrase;

	public static readonly Phrase EntityChangeColourDescPhrase;

	public static readonly Phrase DoorMustBeClosed;

	public static readonly Phrase NeedDoorAccess;

	public static readonly Phrase CannotReskinThatDoor;

	public static readonly Phrase RecentlyDamaged;

	public static readonly Phrase ExplosivesActive;

	public static readonly Phrase PlayerInAir;

	public static readonly Phrase BlockedByPlayer;

	public static readonly Phrase BlockedBySomething;

	public static readonly Phrase PlayerIsMounted;

	public static readonly Phrase CannotReskinInMonument;

	public static readonly Phrase NeedLockAccess;

	public static readonly Phrase NotAuthorized;

	public SoundDefinition SpraySound;

	public GameObjectRef SkinSelectPanel;

	public float SprayCooldown = 2f;

	public float ConditionLossPerSpray = 10f;

	public float ConditionLossPerReskin = 10f;

	public GameObjectRef LinePrefab;

	public Color[] SprayColours = (Color[])(object)new Color[0];

	public float[] SprayWidths = new float[3] { 0.1f, 0.2f, 0.3f };

	public ParticleSystem worldSpaceSprayFx;

	public GameObjectRef ReskinEffect;

	public ItemDefinition SprayDecalItem;

	public GameObjectRef SprayDecalEntityRef;

	public SteamInventoryItem FreeSprayUnlockItem;

	public MinMaxGradient DecalSprayGradient;

	public SoundDefinition SprayLoopDef;

	[FormerlySerializedAs("ShippingCOntainerColourLookup")]
	public ConstructionSkin_ColourLookup ShippingContainerColourLookup;

	public const string ENEMY_BASE_STAT = "sprayed_enemy_base";

	private Phrase lastSprayError;

	private Action _actionClearBusy;

	private Action actionClearBusy => ClearBusy;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0455: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SprayCan.OnRpcMessage"))
		{
			if (rpc == 3490735573u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - BeginFreehandSpray"));
				}
				using (TimeWarning.New("BeginFreehandSpray"))
				{
					using (TimeWarning.New("Conditions"))
					{
						long position = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Read<int>();
						msg.read.Read<int>();
						msg.read.Position = position;
						if (!RPC_Server.IsActiveItem.Test(3490735573u, "BeginFreehandSpray", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							BeginFreehandSpray(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in BeginFreehandSpray");
					}
				}
				return true;
			}
			if (rpc == 151738090 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ChangeItemSkin"));
				}
				using (TimeWarning.New("ChangeItemSkin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(151738090u, "ChangeItemSkin", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(151738090u, "ChangeItemSkin", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ChangeItemSkin(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in ChangeItemSkin");
					}
				}
				return true;
			}
			if (rpc == 688080035 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ChangeWallpaper"));
				}
				using (TimeWarning.New("ChangeWallpaper"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(688080035u, "ChangeWallpaper", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(688080035u, "ChangeWallpaper", this, player))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(688080035u, "ChangeWallpaper", this, player, 5f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ChangeWallpaper(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in ChangeWallpaper");
					}
				}
				return true;
			}
			if (rpc == 396000799 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - CreateSpray"));
				}
				using (TimeWarning.New("CreateSpray"))
				{
					using (TimeWarning.New("Conditions"))
					{
						long position2 = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Read<int>();
						msg.read.Position = position2;
						if (!RPC_Server.IsActiveItem.Test(396000799u, "CreateSpray", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							CreateSpray(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in CreateSpray");
					}
				}
				return true;
			}
			if (rpc == 3288478393u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_SetEntityColour"));
				}
				using (TimeWarning.New("Server_SetEntityColour"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3288478393u, "Server_SetEntityColour", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(3288478393u, "Server_SetEntityColour", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg6 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_SetEntityColour(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in Server_SetEntityColour");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.InputValidation(new Type[]
	{
		typeof(Vector3),
		typeof(Vector3),
		typeof(int),
		typeof(int)
	})]
	private void BeginFreehandSpray(RPCMessage msg)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if (IsBusy() || !CanSprayFreehand(msg.player))
		{
			return;
		}
		Vector3 val = msg.read.Vector3();
		Vector3 atNormal = msg.read.Vector3();
		int num = msg.read.Int32();
		int num2 = msg.read.Int32();
		if (num < 0 || num >= SprayColours.Length || num2 < 0 || num2 >= SprayWidths.Length || Vector3.Distance(val, ((Component)GetOwnerPlayer()).transform.position) > 3f)
		{
			return;
		}
		SprayCanSpray_Freehand sprayCanSpray_Freehand = GameManager.server.CreateEntity(LinePrefab.resourcePath, val, Quaternion.identity) as SprayCanSpray_Freehand;
		sprayCanSpray_Freehand.AddInitialPoint(atNormal);
		sprayCanSpray_Freehand.SetColour(SprayColours[num]);
		sprayCanSpray_Freehand.SetWidth(SprayWidths[num2]);
		sprayCanSpray_Freehand.EnableChanges(msg.player);
		sprayCanSpray_Freehand.Spawn();
		paintingLine = sprayCanSpray_Freehand;
		ClientRPC(RpcTarget.NetworkGroup("Client_ChangeSprayColour"), num);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: true);
		flagsUpdateScope.Set(Flags.Reserved1, b: true);
	}

	public void ClearPaintingLine(bool allowNewSprayImmediately)
	{
		paintingLine = null;
		if (!base.UsingInfiniteAmmoCheat)
		{
			LoseCondition(ConditionLossPerSpray);
		}
		if (allowNewSprayImmediately)
		{
			ClearBusy();
		}
		else
		{
			Invoke(ClearBusy, 0.1f);
		}
	}

	public bool CanSprayFreehand(BasePlayer player)
	{
		if ((Object)(object)FreeSprayUnlockItem != (Object)null)
		{
			if (!player.blueprints.steamInventory.HasItem(FreeSprayUnlockItem.id))
			{
				return FreeSprayUnlockItem.HasUnlocked(player);
			}
			return true;
		}
		return false;
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(2uL)]
	private void ChangeItemSkin(RPCMessage msg)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId uid = msg.read.EntityID();
		int num = msg.read.Int32();
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(uid);
		BasePlayer player = msg.player;
		BaseEntity baseEntity = baseNetworkable as BaseEntity;
		if (baseEntity == null)
		{
			return;
		}
		LastReskinError = Phrase.op_Implicit(string.Empty);
		LastReskinErrorEntity = null;
		if (!ValidateReskin(player, baseEntity, num))
		{
			ShowLastReskinError(player);
		}
		else
		{
			if (!GetItemDefinitionForEntity(baseEntity, out var def, useRedirect: false))
			{
				return;
			}
			ulong num2 = ItemDefinition.FindSkin(((Object)(object)def.isRedirectOf != (Object)null) ? def.isRedirectOf.itemid : def.itemid, num);
			if (Interface.CallHook("OnEntityReskin", baseEntity, num2, msg.player) != null)
			{
				return;
			}
			if (!TryFindTargetRedirect(def, num, out var targetRedirect))
			{
				baseEntity.skinID = num2;
			}
			else
			{
				if (!ValidateRedirectSwap(baseEntity, targetRedirect, player))
				{
					ShowLastReskinError(player);
					return;
				}
				if (!GetEntityPrefabPath(targetRedirect, out var resourcePath))
				{
					Debug.LogError((object)("Cannot find resource path of redirect entity to spawn! " + ((Object)((Component)targetRedirect).gameObject).name));
					return;
				}
				baseEntity = DoRedirectSwap(baseEntity, resourcePath, num2);
			}
			if (baseEntity is IReskinCallback reskinCallback)
			{
				reskinCallback.OnReskinned(player);
			}
			baseEntity.SendNetworkUpdate();
			Interface.CallHook("OnEntityReskinned", baseEntity, num2, msg.player);
			ClientRPC(RpcTarget.NetworkGroup("Client_ReskinResult"), 1, baseEntity.net.ID);
			if (!base.UsingInfiniteAmmoCheat)
			{
				LoseCondition(ConditionLossPerReskin);
			}
			Facepunch.Rust.Analytics.Azure.OnEntitySkinChanged(player, baseEntity, num);
			ClientRPC(RpcTarget.NetworkGroup("Client_ChangeSprayColour"), -1);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: true);
			}
			Invoke(actionClearBusy, SprayCooldown);
		}
	}

	private BaseEntity DoRedirectSwap(BaseEntity entity, string newResourcePath, ulong targetSkinID)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		ReskinPreserveInfo preserveInfo = default(ReskinPreserveInfo);
		entity.Reskin_Preserve(ref preserveInfo);
		Vector3 pos = default(Vector3);
		Quaternion rot = default(Quaternion);
		((Component)entity).transform.GetPositionAndRotation(ref pos, ref rot);
		entity.Kill();
		BaseEntity baseEntity = GameManager.server.CreateEntity(newResourcePath, pos, rot);
		baseEntity.Spawn();
		baseEntity.Reskin_Restore(ref preserveInfo);
		if (GetItemDefinitionForEntity(baseEntity, out var def, useRedirect: false) && (Object)(object)def.isRedirectOf == (Object)null)
		{
			baseEntity.skinID = targetSkinID;
		}
		return baseEntity;
	}

	private bool ValidateReskin(BasePlayer player, BaseEntity targetEnt, int targetSkin)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (IsBusy())
		{
			return false;
		}
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (!player.IsOnGround() && !player.IsFlying)
		{
			LastReskinError = PlayerInAir;
			return false;
		}
		if (targetSkin != 0 && !player.blueprints.CheckSkinOwnership(targetSkin, player))
		{
			LastReskinError = ConstructionErrors.SkinNotOwned;
			return false;
		}
		OBB val = targetEnt.WorldSpaceBounds();
		Vector3 position = ((OBB)(ref val)).ClosestPoint(player.eyes.position);
		if (!player.IsVisible(position, 3f))
		{
			LastReskinError = ConstructionErrors.LineOfSightBlocked;
			return false;
		}
		if (!player.CanBuild())
		{
			return false;
		}
		if (player.IsBuildBlockedByMonument())
		{
			LastReskinError = CannotReskinInMonument;
			return false;
		}
		return targetEnt.CanBeReskinned(player);
	}

	private bool ValidateRedirectSwap(BaseEntity entity, ItemDefinition targetRedirect, BasePlayer player)
	{
		if (!entity.CanBeRedirectSwapped(player))
		{
			return false;
		}
		if (global::SimpleUpgrade.IsUpgradeBlocked(entity, targetRedirect, player))
		{
			if ((Object)(object)DeployVolume.LastDeployHit != (Object)null)
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(DeployVolume.LastDeployHit);
				if ((Object)(object)baseEntity != (Object)null && !string.IsNullOrEmpty(ConstructionErrors.GetTranslatedNameFromEntity(baseEntity)))
				{
					LastReskinError = ConstructionErrors.BlockedBy;
					LastReskinErrorEntity = baseEntity;
				}
				else
				{
					LastReskinError = BlockedBySomething;
				}
			}
			else
			{
				LastReskinError = Construction.lastPlacementError;
			}
			return false;
		}
		return true;
	}

	public void ShowLastReskinError(BasePlayer player)
	{
		if (!LastReskinError.IsEmpty())
		{
			if ((Object)(object)LastReskinErrorEntity != (Object)null)
			{
				player.ShowBlockedByEntityToast(LastReskinErrorEntity, BlockedBySomething);
			}
			else if (!string.IsNullOrEmpty(LastReskinErrorArgString))
			{
				player.ShowToast(GameTip.Styles.Error, LastReskinError, false, LastReskinErrorArgString);
			}
			else
			{
				player.ShowToast(GameTip.Styles.Error, LastReskinError, false);
			}
		}
	}

	[RPC_Server.MaxDistance(5f)]
	[RPC_Server.IsActiveItem]
	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server]
	private void ChangeWallpaper(RPCMessage msg)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId uid = msg.read.EntityID();
		int targetSkin = msg.read.Int32();
		int side = ((!msg.read.Bool()) ? 1 : 0);
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(uid);
		if (baseNetworkable is BuildingBlock buildingBlock && buildingBlock.HasWallpaper(side) && ValidateWallpaperReskin(msg.player, baseNetworkable as BuildingBlock, side, targetSkin))
		{
			ulong id = ItemDefinition.FindSkin(WallpaperSettings.GetItemDefForCategory(WallpaperPlanner.Settings.GetCategory(buildingBlock, side)).itemid, targetSkin);
			buildingBlock.SetWallpaper(id, side);
			Facepunch.Rust.Analytics.Azure.OnWallpaperPlaced(msg.player, buildingBlock, id, side, reskin: true);
			ClientRPC(RpcTarget.NetworkGroup("Client_ReskinResult"), 1, buildingBlock.net.ID);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: true);
			}
			Invoke(actionClearBusy, SprayCooldown);
		}
	}

	private bool ValidateWallpaperReskin(BasePlayer player, BuildingBlock block, int side, int targetSkin)
	{
		if ((Object)(object)player == (Object)null || !player.CanBuild())
		{
			return false;
		}
		if (!player.IsOnGround())
		{
			player.ShowToast(GameTip.Styles.Error, PlayerInAir, false);
			return false;
		}
		if (targetSkin != 0 && !player.blueprints.CheckSkinOwnership(targetSkin, player))
		{
			player.ShowToast(GameTip.Styles.Error, ConstructionErrors.SkinNotOwned, false);
			return false;
		}
		if (!block.HasWallpaper(side))
		{
			return false;
		}
		if (!block.CanSeeWallpaperSocket(player, side))
		{
			return false;
		}
		return true;
	}

	public static bool GetItemDefinitionForEntity(BaseEntity be, out ItemDefinition def, bool useRedirect = true)
	{
		def = null;
		if (be is BaseCombatEntity baseCombatEntity)
		{
			if (baseCombatEntity.pickup.enabled && (Object)(object)baseCombatEntity.pickup.itemTarget != (Object)null)
			{
				def = baseCombatEntity.pickup.itemTarget;
			}
			else if (baseCombatEntity.repair.enabled && (Object)(object)baseCombatEntity.repair.itemTarget != (Object)null)
			{
				def = baseCombatEntity.repair.itemTarget;
			}
		}
		if (be is HeldEntity heldEntity)
		{
			def = heldEntity.GetCachedItem()?.info;
		}
		if (be is CodeLock codeLock)
		{
			def = codeLock.itemType;
		}
		if (useRedirect && (Object)(object)def != (Object)null && (Object)(object)def.isRedirectOf != (Object)null)
		{
			def = def.isRedirectOf;
		}
		return (Object)(object)def != (Object)null;
	}

	private bool TryFindTargetRedirect(ItemDefinition itemDef, int targetSkin, out ItemDefinition targetRedirect)
	{
		targetRedirect = null;
		if ((((Object)(object)itemDef.isRedirectOf != (Object)null) ? itemDef.isRedirectOf : itemDef).skins.FirstOrDefault((ItemSkinDirectory.Skin x) => x.id == targetSkin).invItem is ItemSkin itemSkin)
		{
			if ((Object)(object)itemSkin.Redirect != (Object)null)
			{
				targetRedirect = itemSkin.Redirect;
			}
			else if ((Object)(object)itemDef.isRedirectOf != (Object)null)
			{
				targetRedirect = itemDef.isRedirectOf;
			}
		}
		else if ((Object)(object)itemDef.isRedirectOf != (Object)null)
		{
			targetRedirect = itemDef.isRedirectOf;
		}
		return (Object)(object)targetRedirect != (Object)null;
	}

	private bool GetEntityPrefabPath(ItemDefinition def, out string resourcePath)
	{
		resourcePath = string.Empty;
		ItemModDeployable itemModDeployable = default(ItemModDeployable);
		if (((Component)def).TryGetComponent<ItemModDeployable>(ref itemModDeployable))
		{
			resourcePath = itemModDeployable.entityPrefab.resourcePath;
			return true;
		}
		ItemModEntity itemModEntity = default(ItemModEntity);
		if (((Component)def).TryGetComponent<ItemModEntity>(ref itemModEntity))
		{
			resourcePath = itemModEntity.entityPrefab.resourcePath;
			return true;
		}
		ItemModEntityReference itemModEntityReference = default(ItemModEntityReference);
		if (((Component)def).TryGetComponent<ItemModEntityReference>(ref itemModEntityReference))
		{
			resourcePath = itemModEntityReference.entityPrefab.resourcePath;
			return true;
		}
		return false;
	}

	private bool IsSprayBlockedByTrigger(Vector3 pos)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer == (Object)null)
		{
			return true;
		}
		TriggerNoSpray triggerNoSpray = ownerPlayer.FindTrigger<TriggerNoSpray>();
		if ((Object)(object)triggerNoSpray == (Object)null)
		{
			return false;
		}
		return !triggerNoSpray.IsPositionValid(pos);
	}

	[RPC_Server.InputValidation(new Type[]
	{
		typeof(Vector3),
		typeof(Vector3),
		typeof(Vector3),
		typeof(int)
	})]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void CreateSpray(RPCMessage msg)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		if (IsBusy())
		{
			return;
		}
		ClientRPC(RpcTarget.NetworkGroup("Client_ChangeSprayColour"), -1);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: true);
		}
		Invoke(actionClearBusy, SprayCooldown);
		Vector3 val = msg.read.Vector3();
		Vector3 val2 = msg.read.Vector3();
		Vector3 val3 = msg.read.Vector3();
		int num = msg.read.Int32();
		if (Vector3.Distance(val, ((Component)this).transform.position) > 4.5f)
		{
			return;
		}
		Plane val4 = default(Plane);
		((Plane)(ref val4))._002Ector(val2, val);
		Vector3 val5 = ((Plane)(ref val4)).ClosestPointOnPlane(val3) - val;
		Quaternion val6 = Quaternion.LookRotation(((Vector3)(ref val5)).normalized, val2);
		val6 *= Quaternion.Euler(0f, 0f, 90f);
		if (num != 0 && !msg.player.blueprints.CheckSkinOwnership(num, msg.player))
		{
			Debug.Log((object)$"SprayCan.ChangeItemSkin player does not have item :{num}:");
		}
		else
		{
			if (Interface.CallHook("OnSprayCreate", this, val, val6) != null)
			{
				return;
			}
			ulong num2 = ItemDefinition.FindSkin(SprayDecalItem.itemid, num);
			BaseEntity baseEntity = GameManager.server.CreateEntity(SprayDecalEntityRef.resourcePath, val, val6);
			baseEntity.skinID = num2;
			baseEntity.OnDeployed(null, GetOwnerPlayer(), GetItem());
			baseEntity.networkEntityScale = true;
			Vector3 one = Vector3.one;
			ItemSkinDirectory.Skin[] skins = SprayDecalItem.skins;
			for (int i = 0; i < skins.Length; i++)
			{
				ItemSkinDirectory.Skin skin = skins[i];
				if ((ulong)skin.id == num2 && skin.invItem.SprayScale > 0f)
				{
					one.y = skin.invItem.SprayScale;
					one.z = skin.invItem.SprayScale;
				}
			}
			((Component)baseEntity).transform.localScale = one;
			baseEntity.Spawn();
			if (!base.UsingInfiniteAmmoCheat)
			{
				LoseCondition(ConditionLossPerSpray);
			}
		}
	}

	private void LoseCondition(float amount)
	{
		GetOwnerItem()?.LoseCondition(amount);
	}

	public void ClearBusy()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: false);
		flagsUpdateScope.Set(Flags.Reserved1, b: false);
	}

	public override void OnHeldChanged()
	{
		if (IsDisabled())
		{
			ClearBusy();
			if ((Object)(object)paintingLine != (Object)null)
			{
				paintingLine.Kill();
			}
			paintingLine = null;
		}
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void Server_SetEntityColour(RPCMessage msg)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId uid = msg.read.EntityID();
		uint num = msg.read.UInt32();
		BasePlayer player = msg.player;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: true);
		}
		Invoke(actionClearBusy, 0.1f);
		if ((Object)(object)player == (Object)null || !player.CanBuild())
		{
			return;
		}
		BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(uid) as BaseEntity;
		if (!((Object)(object)baseEntity != (Object)null) || Vector3.SqrMagnitude(((Component)player).transform.position - ((Component)baseEntity).transform.position) > 16f)
		{
			return;
		}
		if (baseEntity is BuildingBlock { customColour: var customColour } buildingBlock)
		{
			buildingBlock.SetCustomColour(num);
			Facepunch.Rust.Analytics.Azure.OnEntityColorChanged(player, buildingBlock, customColour, num);
			return;
		}
		int i = 0;
		for (int count = baseEntity.Components.Count; i < count; i++)
		{
			EntityComponentBase entityComponentBase = baseEntity.Components[i];
			if (entityComponentBase is SprayCanColorChangeEntityComponent { currentColorIndex: var currentColorIndex } sprayCanColorChangeEntityComponent)
			{
				sprayCanColorChangeEntityComponent.Server_UpdateColor(num);
				Facepunch.Rust.Analytics.Azure.OnEntityColorChanged(player, entityComponentBase.GetBaseEntity(), currentColorIndex, num);
				break;
			}
		}
	}

	static SprayCan()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Expected O, but got Unknown
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Expected O, but got Unknown
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Expected O, but got Unknown
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Expected O, but got Unknown
		LastReskinError = Phrase.op_Implicit(string.Empty);
		LastReskinErrorEntity = null;
		LastReskinErrorArgString = string.Empty;
		FreeSprayNamePhrase = new Phrase("freespray_radial", "Free Spray");
		FreeSprayDescPhrase = new Phrase("freespray_radial_desc", "Spray shapes freely with various colors");
		BuildingSkinColourPhrase = new Phrase("buildingskin_colour", "Set colour");
		BuildingSkinColourDescPhrase = new Phrase("buildingskin_colour_desc", "Set the block to the highlighted colour");
		EntityChangeSkinPhrase = new Phrase("entity_changeskin", "Change skin");
		EntityChangeSkinDescPhrase = new Phrase("entity_changeskin_desc", "Open skin selection");
		EntityChangeColourPhrase = new Phrase("entity_changecolour", "Change colour");
		EntityChangeColourDescPhrase = new Phrase("entity_changecolour_desc", "Open colour selection");
		DoorMustBeClosed = new Phrase("error_doormustbeclosed", "Door must be closed");
		NeedDoorAccess = new Phrase("error_needdooraccess", "Need door access");
		CannotReskinThatDoor = new Phrase("error_cannotreskindoor", "Cannot reskin that door");
		RecentlyDamaged = new Phrase("error_reskin_recentlydamaged", "Recently damaged, reskinnable in {0} seconds");
		ExplosivesActive = new Phrase("error_explosivesactive", "Cannot reskin an object with explosives attached");
		PlayerInAir = new Phrase("error_playerinair", "You must be on the ground");
		BlockedByPlayer = new Phrase("error_blockedbyplayer_reskin", "Blocked by intersecting player");
		BlockedBySomething = new Phrase("error_blockedbysomething", "Blocked by something");
		PlayerIsMounted = new Phrase("error_playerismounted", "Player {0} is mounted");
		CannotReskinInMonument = new Phrase("error_reskin_monument", "Cannot reskin objects inside a monument");
		NeedLockAccess = new Phrase("error_needlockaccess", "Need lock access");
		NotAuthorized = new Phrase("error_notauthorized", "You are not authorized");
	}
}
