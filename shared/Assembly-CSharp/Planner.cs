using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Planner : HeldEntity
{
	public struct CanBuildResult
	{
		public bool Result;

		public Phrase Phrase;

		public string[] Arguments;
	}

	public const int MaxUpgradeChainLength = 8;

	public BaseEntity[] buildableList;

	public Vector3 serverStartDurationPlacementPosition
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public TimeSince serverStartDurationPlacementTime
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public virtual bool isTypeDeployable => (Object)(object)GetModDeployable() != (Object)null;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Planner.OnRpcMessage"))
		{
			if (rpc == 1872774636 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DoPlace"));
				}
				using (TimeWarning.New("DoPlace"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(1872774636u, "DoPlace", this, player))
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
							DoPlace(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in DoPlace");
					}
				}
				return true;
			}
			if (rpc == 3892284151u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - StartDurationPlace"));
				}
				using (TimeWarning.New("StartDurationPlace"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3892284151u, "StartDurationPlace", this, player, 10uL))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(3892284151u, "StartDurationPlace", this, player))
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
							StartDurationPlace(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in StartDurationPlace");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void DoPlace(RPCMessage msg)
	{
		if (!msg.player.CanInteract())
		{
			return;
		}
		CreateBuilding val = msg.read.Proto<CreateBuilding>((CreateBuilding)null);
		try
		{
			DoBuild(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.CallsPerSecond(10uL)]
	private void StartDurationPlace(RPCMessage msg)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (Object.op_Implicit((Object)(object)ownerPlayer))
		{
			serverStartDurationPlacementPosition = ((Component)ownerPlayer).transform.position;
			serverStartDurationPlacementTime = TimeSince.op_Implicit(0f);
		}
	}

	public Socket_Base FindSocket(string name, uint prefabIDToFind)
	{
		Socket_Base[] array = PrefabAttribute.server.FindAll<Socket_Base>(prefabIDToFind);
		foreach (Socket_Base socket_Base in array)
		{
			if (socket_Base.socketName == name)
			{
				return socket_Base;
			}
		}
		return null;
	}

	public virtual void DoBuild(CreateBuilding msg)
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!Object.op_Implicit((Object)(object)ownerPlayer))
		{
			return;
		}
		Construction construction = PrefabAttribute.server.Find<Construction>(msg.blockID);
		if (construction == null)
		{
			ownerPlayer.ShowToast(GameTip.Styles.Error, ConstructionErrors.CouldntFindConstruction, false);
			ConstructionErrors.Log(ownerPlayer, msg.blockID.ToString());
			return;
		}
		if (!CanAffordToPlace(construction))
		{
			ItemAmountList val = Pool.Get<ItemAmountList>();
			try
			{
				val.amount = Pool.Get<List<float>>();
				val.itemID = Pool.Get<List<int>>();
				GetConstructionCost(val, construction);
				ownerPlayer.ClientRPC(RpcTarget.Player("Client_OnRepairFailedResources", ownerPlayer), val);
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if (ConVar.AntiHack.objectplacement && ownerPlayer.TriggeredMovementAntiHack())
		{
			AntiHack.PlayerState playerState = AntiHack.PlayerStates[ownerPlayer.ActivePlayerInd];
			ownerPlayer.ShowToast(GameTip.Styles.Error, ConstructionErrors.AntihackWithReason, false, playerState.LastViolationType.ToString());
			return;
		}
		Deployable deployable = GetDeployable(msg.entity);
		if (construction.deployable != deployable)
		{
			BaseEntity baseEntity = (msg.entity.IsValid ? (BaseNetworkable.serverEntities.Find(msg.entity) as BaseEntity) : null);
			if (construction.deployable == null || GetUpgradeDeployable(deployable, baseEntity) != construction.deployable)
			{
				ownerPlayer.ShowToast(GameTip.Styles.Error, ConstructionErrors.DeployableMismatch, false);
			}
			else
			{
				DoUpgrade(ownerPlayer, baseEntity, construction);
			}
			return;
		}
		Construction.Target target = default;
		if (msg.entity.IsValid)
		{
			target.entity = BaseNetworkable.serverEntities.Find(msg.entity) as BaseEntity;
			if ((Object)(object)target.entity == (Object)null)
			{
				ownerPlayer.ShowToast(GameTip.Styles.Error, ConstructionErrors.CouldntFindEntity, false);
				ConstructionErrors.Log(ownerPlayer, ((object)msg.entity/*cast due to constrained. prefix*/).ToString());
				return;
			}
			msg.ray = new Ray(((Component)target.entity).transform.TransformPoint(msg.ray.origin), ((Component)target.entity).transform.TransformDirection(msg.ray.direction));
			msg.position = ((Component)target.entity).transform.TransformPoint(msg.position);
			msg.normal = ((Component)target.entity).transform.TransformDirection(msg.normal);
			msg.rotation = ((Component)target.entity).transform.rotation * msg.rotation;
			if (msg.isSnapped)
			{
				msg.snappedPosition = ((Component)target.entity).transform.TransformPoint(msg.snappedPosition);
				Quaternion val2 = Quaternion.Euler(msg.snappedRotation);
				Quaternion val3 = ((Component)target.entity).transform.rotation * val2;
				msg.snappedRotation = val3.eulerAngles;
			}
			if (msg.socket != 0)
			{
				string text = StringPool.Get(msg.socket);
				if (text != "")
				{
					target.socket = FindSocket(text, target.entity.prefabID);
				}
				if (target.socket == null)
				{
					ownerPlayer.ShowToast(GameTip.Styles.Error, ConstructionErrors.CouldntFindSocket, false);
					ConstructionErrors.Log(ownerPlayer, msg.socket.ToString());
					return;
				}
			}
			else if (target.entity is Door)
			{
				ownerPlayer.ShowToast(GameTip.Styles.Error, ConstructionErrors.CantDeployOnDoor, false);
				return;
			}
		}
		target.ray = msg.ray;
		target.onTerrain = msg.onterrain;
		target.position = msg.position;
		target.normal = msg.normal;
		target.rotation = msg.rotation;
		target.player = ownerPlayer;
		target.isHoldingShift = msg.isHoldingShift;
		target.snappingMode = msg.snappingMode;
		target.isSnapped = msg.isSnapped;
		target.snappedPosition = msg.snappedPosition;
		target.snappedRotation = msg.snappedRotation;
		target.valid = true;
		if (Interface.CallHook("CanBuild", this, construction, target) != null)
		{
			return;
		}
		target.shouldParent = ShouldParent(target.entity, deployable);
		if (target.shouldParent)
		{
			PlayerBoat parentPlayerBoat = PlayerBoat.GetParentPlayerBoat(target.entity, includeEntityItself: true);
			if ((Object)(object)parentPlayerBoat != (Object)null && (!parentPlayerBoat.VerifyDeployablePlacement(ownerPlayer) || (!construction.canBypassBuildingPermission && !parentPlayerBoat.IsAuthedForBuilding(ownerPlayer))))
			{
				return;
			}
			Vector3 position = ((target.socket != null) ? target.GetWorldPosition() : target.position);
			float num = target.entity.Distance(position);
			if (num > 1f)
			{
				ownerPlayer.ShowToast(GameTip.Styles.Error, ConstructionErrors.ParentTooFar, false);
				ConstructionErrors.Log(ownerPlayer, num.ToString());
				return;
			}
		}
		if (!construction.canBypassBuildingPermission && !ownerPlayer.CanBuild())
		{
			ownerPlayer.ShowToast(GameTip.Styles.Error, ConstructionErrors.NoPermission, false);
			return;
		}
		BaseEntity baseEntity2 = DoBuild(target, construction);
		if ((Object)(object)baseEntity2 != (Object)null && baseEntity2 is DecayEntity decayEntity)
		{
			decayEntity.timePlaced = GetNetworkTime();
		}
		if ((Object)(object)baseEntity2 != (Object)null && baseEntity2 is BuildingBlock buildingBlock && ownerPlayer.IsInCreativeMode && Creative.freeBuild)
		{
			ConstructionGrade constructionGrade = construction.grades[msg.setToGrade];
			if (buildingBlock.currentGrade != constructionGrade)
			{
				buildingBlock.ChangeGradeAndSkin(constructionGrade.gradeBase.type, constructionGrade.gradeBase.skin);
			}
		}
	}

	public virtual BaseEntity DoBuild(Construction.Target target, Construction component)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!Object.op_Implicit((Object)(object)ownerPlayer))
		{
			return null;
		}
		if (RayEx.IsNaNOrInfinity(target.ray))
		{
			return null;
		}
		if (Vector3Ex.IsNaNOrInfinity(target.position))
		{
			return null;
		}
		if (Vector3Ex.IsNaNOrInfinity(target.normal))
		{
			return null;
		}
		Construction.lastPlacementError = Phrase.op_Implicit("");
		Construction.lastPlacementErrorDebug = "";
		Construction.lastBuildingBlockError = null;
		Construction.lastPlacementErrorIsDetailed = false;
		if (target.socket != null)
		{
			if (!target.socket.female)
			{
				ownerPlayer.ShowToast(GameTip.Styles.Error, ConstructionErrors.SocketNotFemale, false);
				Construction.lastPlacementErrorDebug = target.socket.socketName;
				return null;
			}
			if ((Object)(object)target.entity != (Object)null && target.entity.IsOccupied(target.socket))
			{
				ownerPlayer.ShowToast(GameTip.Styles.Error, ConstructionErrors.SocketOccupied, false);
				Construction.lastPlacementErrorDebug = target.socket.socketName;
				return null;
			}
			if (target.onTerrain)
			{
				Construction.lastPlacementErrorDebug = "Target on terrain is not allowed when attaching to socket (" + target.socket.socketName + ")";
				return null;
			}
		}
		Vector3 deployPos = (((Object)(object)target.entity != (Object)null && target.socket != null) ? target.GetWorldPosition() : target.position);
		if (AntiHack.TestIsBuildingInsideSomething(target, deployPos))
		{
			ownerPlayer.ShowToast(GameTip.Styles.Error, ConstructionErrors.InsideObjects, false);
			return null;
		}
		if (ConVar.AntiHack.eye_protection >= 2 && !component.HasAlternativeLOSChecks() && !HasLineOfSight(ownerPlayer, deployPos, target, component))
		{
			ownerPlayer.ShowToast(GameTip.Styles.Error, ConstructionErrors.LineOfSightBlocked, false);
			return null;
		}
		if (ConVar.Server.max_sleeping_bags > 0 && component.isSleepingBag)
		{
			CanBuildResult? result = SleepingBag.CanBuildBed(ownerPlayer, component);
			if (HandleCanBuild(result, ownerPlayer))
			{
				return null;
			}
		}
		GameObject val = DoPlacement(target, component);
		if ((Object)(object)val == (Object)null)
		{
			if (!string.IsNullOrEmpty(Construction.lastPlacementError.translated))
			{
				ownerPlayer.ShowToast(GameTip.Styles.Error, Construction.lastPlacementError, false);
			}
			ConstructionErrors.Log(ownerPlayer, Construction.lastPlacementErrorDebug);
		}
		if ((Object)(object)val != (Object)null)
		{
			Interface.CallHook("OnEntityBuilt", this, val);
			Deployable deployable = GetDeployable();
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(val);
			if ((Object)(object)baseEntity != (Object)null && deployable != null && ShouldParent(target.entity, deployable))
			{
				if (target.socket is Socket_Specific_Female socket_Specific_Female)
				{
					if (socket_Specific_Female.parentToBone)
					{
						baseEntity.SetParent(target.entity, socket_Specific_Female.boneName, worldPositionStays: true);
					}
					else
					{
						baseEntity.SetParent(target.entity, worldPositionStays: true);
					}
				}
				else
				{
					baseEntity.SetParent(target.entity, worldPositionStays: true);
				}
			}
			Vector3 effectPosition = ((Object.op_Implicit((Object)(object)target.entity) && target.socket != null) ? ((Component)target.entity).transform.TransformPoint(target.socket.worldPosition) : target.position);
			Vector3 effectNormal = ((Object.op_Implicit((Object)(object)target.entity) && target.socket != null) ? ((Component)target.entity).transform.up : target.normal);
			OnConstructionBuilt(baseEntity, deployable, ownerPlayer, component, effectPosition, effectNormal);
			return baseEntity;
		}
		return null;
	}

	private void OnConstructionBuilt(BaseEntity objEnt, Deployable dep, BasePlayer player, Construction component, Vector3 effectPosition, Vector3 effectNormal)
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)objEnt != (Object)null && dep != null)
		{
			if (dep.wantsInstanceData && GetOwnerItem().instanceData != null)
			{
				(objEnt as IInstanceDataReceiver).ReceiveInstanceData(GetOwnerItem().instanceData);
			}
			if (dep.copyInventoryFromItem)
			{
				StorageContainer component2 = ((Component)objEnt).GetComponent<StorageContainer>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					component2.ReceiveInventoryFromItem(GetOwnerItem());
				}
				if ((Object)(object)component2 == (Object)null)
				{
					ContainerIOEntity component3 = ((Component)objEnt).GetComponent<ContainerIOEntity>();
					if (Object.op_Implicit((Object)(object)component3))
					{
						component3.ReceiveInventoryFromItem(GetOwnerItem());
					}
				}
			}
			ItemModDeployable modDeployable = GetModDeployable();
			if ((Object)(object)modDeployable != (Object)null)
			{
				modDeployable.OnDeployed(objEnt, player);
			}
			objEnt.OnDeployed(objEnt.GetParentEntity(), player, GetOwnerItem());
			if (dep.placeEffect.isValid)
			{
				Effect.server.Run(dep.placeEffect.resourcePath, effectPosition, effectNormal);
			}
		}
		if ((Object)(object)objEnt != (Object)null)
		{
			Facepunch.Rust.Analytics.Azure.OnEntityBuilt(objEnt, player);
			if ((Object)(object)GetOwnerItemDefinition() != (Object)null)
			{
				player.ProcessMissionEvent(BaseMission.MissionEventType.DEPLOY, new BaseMission.MissionEventPayload
				{
					WorldPosition = ((Component)objEnt).transform.position,
					UintIdentifier = objEnt.prefabID,
					IntIdentifier = GetOwnerItemDefinition().itemid
				}, 1f);
			}
		}
		PayForPlacement(player, component);
	}

	public GameObject DoPlacement(Construction.Target placement, Construction component)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!Object.op_Implicit((Object)(object)ownerPlayer))
		{
			return null;
		}
		BaseEntity baseEntity = component.CreateConstruction(placement, bNeedsValidPlacement: true);
		if (!Object.op_Implicit((Object)(object)baseEntity))
		{
			return null;
		}
		if (Interface.CallHook("OnConstructionPlace", baseEntity, component, placement, ownerPlayer) != null)
		{
			if (baseEntity.IsValid())
			{
				baseEntity.KillMessage();
			}
			else
			{
				DecayEntity decayEntity = baseEntity as DecayEntity;
				if (Object.op_Implicit((Object)(object)decayEntity))
				{
					decayEntity.DoServerDestroy();
				}
				baseEntity.TerminateOnServer();
				baseEntity.EntityDestroy();
			}
			return null;
		}
		return SpawnConstruction(baseEntity, ownerPlayer, 0uL);
	}

	private GameObject SpawnConstruction(BaseEntity constructed, BasePlayer player, ulong inheritedSkinID = 0uL, float inheritedHealthFraction = 1f)
	{
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		float num = inheritedHealthFraction;
		float num2 = 0f;
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null)
		{
			constructed.skinID = ownerItem.skin;
			if (ownerItem.hasCondition)
			{
				num *= ownerItem.conditionNormalized;
			}
		}
		if (inheritedSkinID != 0L)
		{
			constructed.skinID = inheritedSkinID;
		}
		PoolableEx.AwakeFromInstantiate(((Component)constructed).gameObject);
		BuildingBlock buildingBlock = constructed as BuildingBlock;
		if (Object.op_Implicit((Object)(object)buildingBlock))
		{
			buildingBlock.blockDefinition = PrefabAttribute.server.Find<Construction>(buildingBlock.prefabID);
			if (!buildingBlock.blockDefinition)
			{
				Debug.LogError((object)"Placing a building block that has no block definition!");
				return null;
			}
			buildingBlock.SetGrade(buildingBlock.blockDefinition.defaultGrade.gradeBase.type);
		}
		BaseCombatEntity baseCombatEntity = constructed as BaseCombatEntity;
		if (Object.op_Implicit((Object)(object)baseCombatEntity))
		{
			num2 = (((Object)(object)buildingBlock != (Object)null) ? buildingBlock.currentGrade.maxHealth : baseCombatEntity.startHealth);
			baseCombatEntity.ResetLifeStateOnSpawn = false;
			baseCombatEntity.InitializeHealth(num2 * num, baseCombatEntity.StartMaxHealth());
		}
		constructed.OnPlaced(player);
		constructed.OwnerID = player.userID;
		constructed.Spawn();
		if (Object.op_Implicit((Object)(object)buildingBlock))
		{
			Effect.server.Run(buildingBlock.blockDefinition.placeEffect.isValid ? buildingBlock.blockDefinition.placeEffect.resourcePath : "assets/bundled/prefabs/fx/build/frame_place.prefab", constructed, 0u, Vector3.zero, Vector3.zero);
		}
		StabilityEntity stabilityEntity = constructed as StabilityEntity;
		if (Object.op_Implicit((Object)(object)stabilityEntity))
		{
			stabilityEntity.UpdateSurroundingEntities();
		}
		return ((Component)constructed).gameObject;
	}

	public virtual void PayForPlacement(BasePlayer player, Construction component)
	{
		if (Interface.CallHook("OnPayForPlacement", player, this, component) != null || (player.IsInCreativeMode && Creative.freeBuild))
		{
			return;
		}
		if (player.IsInTutorial)
		{
			TutorialIsland currentTutorialIsland = player.GetCurrentTutorialIsland();
			if ((Object)(object)currentTutorialIsland != (Object)null)
			{
				currentTutorialIsland.OnPlayerBuiltConstruction(player);
			}
		}
		if (isTypeDeployable)
		{
			GetItem().UseItem();
			return;
		}
		List<Item> list = Pool.Get<List<Item>>();
		foreach (ItemAmount item in component.defaultGrade.CostToBuild())
		{
			player.inventory.Take(list, item.itemDef.itemid, (int)item.amount);
			player.Command("note.inv", item.itemDef.itemid, item.amount * -1f);
			Facepunch.Rust.Analytics.Azure.LogResource(Facepunch.Rust.Analytics.Azure.ResourceMode.Consumed, "build_block", item.itemDef.shortname, (int)item.amount, null, null, safezone: false, null, player.userID, null, null, null, 0uL);
		}
		foreach (Item item2 in list)
		{
			item2.Remove();
		}
		Pool.Free<Item>(ref list, false);
	}

	public virtual bool CanAffordToPlace(Construction component)
	{
		if (isTypeDeployable)
		{
			return true;
		}
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!Object.op_Implicit((Object)(object)ownerPlayer))
		{
			return false;
		}
		object obj = Interface.CallHook("CanAffordToPlace", ownerPlayer, this, component);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (ownerPlayer.IsInCreativeMode && Creative.freeBuild)
		{
			return true;
		}
		foreach (ItemAmount item in component.defaultGrade.CostToBuild())
		{
			if ((float)ownerPlayer.inventory.GetAmount(item.itemDef.itemid) < item.amount)
			{
				return false;
			}
		}
		return true;
	}

	protected virtual void GetConstructionCost(ItemAmountList list, Construction component)
	{
		list.amount.Clear();
		list.itemID.Clear();
		foreach (ItemAmount item in component.defaultGrade.CostToBuild())
		{
			list.itemID.Add(item.itemDef.itemid);
			list.amount.Add((int)item.amount);
		}
	}

	private bool ShouldParent(BaseEntity targetEntity, Deployable deployable)
	{
		if ((Object)(object)targetEntity != (Object)null && targetEntity.SupportsChildDeployables() && (targetEntity.ForceDeployableSetParent() || (deployable != null && deployable.setSocketParent)))
		{
			return true;
		}
		return false;
	}

	private bool HandleCanBuild(CanBuildResult? result, BasePlayer player)
	{
		if (result.HasValue)
		{
			if (result.Value.Phrase != null && !player.IsInTutorial)
			{
				player.ShowToast((!result.Value.Result) ? GameTip.Styles.Red_Normal : GameTip.Styles.Blue_Long, result.Value.Phrase, overlay: false, result.Value.Arguments);
			}
			if (!result.Value.Result)
			{
				return true;
			}
		}
		return false;
	}

	protected virtual bool HasLineOfSight(BasePlayer player, Vector3 deployPos, Construction.Target target, Construction component)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 center = player.eyes.center;
		Vector3 position = player.eyes.position;
		Vector3 origin = target.ray.origin;
		Vector3 val = deployPos;
		int num = 2097152;
		int num2 = 2162688;
		if (ConVar.AntiHack.build_terraincheck)
		{
			num2 |= 0x800000;
		}
		if (ConVar.AntiHack.build_vehiclecheck)
		{
			num2 |= 0x8000000;
		}
		float num3 = ConVar.AntiHack.build_losradius;
		float padding = ConVar.AntiHack.build_losradius + 0.01f;
		int layerMask = num2;
		if (target.socket != null)
		{
			num3 = 0f;
			padding = 0.5f;
			layerMask = num;
		}
		if (component.isSleepingBag)
		{
			num3 = ConVar.AntiHack.build_losradius_sleepingbag;
			padding = ConVar.AntiHack.build_losradius_sleepingbag + 0.01f;
			layerMask = num2;
		}
		if (num3 > 0f)
		{
			val += target.normal.normalized * num3;
		}
		if ((Object)(object)target.entity != (Object)null)
		{
			DeployShell deployShell = PrefabAttribute.server.Find<DeployShell>(target.entity.prefabID);
			if (deployShell != null)
			{
				val += target.normal.normalized * deployShell.LineOfSightPadding();
			}
		}
		if (GamePhysics.LineOfSightRadius(center, position, layerMask, num3) && GamePhysics.LineOfSightRadius(position, origin, layerMask, num3))
		{
			return GamePhysics.LineOfSightRadius(origin, val, layerMask, num3, 0f, padding);
		}
		return false;
	}

	public static bool HasLineOfSight(ref Construction.Placement placement, Construction construction, Construction.Target target)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = target.player;
		Vector3 center = player.eyes.center;
		Vector3 position = player.eyes.position;
		Vector3 origin = target.ray.origin;
		Vector3 val = (((Object)(object)target.entity != (Object)null && target.socket != null) ? target.GetWorldPosition() : target.position);
		int num = 2097152;
		int num2 = 2162688;
		if (ConVar.AntiHack.build_terraincheck)
		{
			num2 |= 0x800000;
		}
		if (ConVar.AntiHack.build_vehiclecheck)
		{
			num2 |= 0x8000000;
		}
		float num3 = ConVar.AntiHack.build_losradius;
		float padding = ConVar.AntiHack.build_losradius + 0.01f;
		int num4 = num2;
		if (target.socket != null)
		{
			num3 = 0f;
			padding = 0.5f;
			num4 = num;
		}
		if (construction.isSleepingBag)
		{
			num3 = ConVar.AntiHack.build_losradius_sleepingbag;
			padding = ConVar.AntiHack.build_losradius_sleepingbag + 0.01f;
			num4 = num2;
		}
		if (num3 > 0f)
		{
			val += target.normal.normalized * num3;
		}
		if ((Object)(object)target.entity != (Object)null)
		{
			DeployShell deployShell = PrefabAttribute.server.Find<DeployShell>(target.entity.prefabID);
			if (deployShell != null)
			{
				val += target.normal.normalized * deployShell.LineOfSightPadding();
			}
		}
		if (!GamePhysics.LineOfSightRadius(center, position, num4, num3) || !GamePhysics.LineOfSightRadius(position, origin, num4, num3))
		{
			return false;
		}
		bool flag = GamePhysics.LineOfSightRadius(origin, val, num4, num3, 0f, padding);
		if (!flag && target.socket != null && construction.HasAlternativeLOSChecks())
		{
			Vector3[] alternativeLOSPositions = construction.alternativeLOSPositions;
			foreach (Vector3 val2 in alternativeLOSPositions)
			{
				Vector3 val3 = placement.position + placement.rotation * val2 - origin;
				if (!Physics.Raycast(origin, val3, val3.magnitude, num4))
				{
					return true;
				}
			}
		}
		return flag;
	}

	public static Deployable GetUpgradeDeployable(Deployable itemDeployable, BaseEntity target)
	{
		if (itemDeployable == null || (Object)(object)target == (Object)null || !itemDeployable.upgradesTo.isValid)
		{
			return null;
		}
		Deployable deployable = itemDeployable;
		for (int i = 0; i < 8; i++)
		{
			if (!(deployable != null))
			{
				break;
			}
			Deployable deployable2 = (deployable.upgradesTo.isValid ? target.prefabAttribute.Find<Deployable>(deployable.upgradesTo.resourceID) : null);
			if (deployable.prefabID == target.prefabID)
			{
				return deployable2;
			}
			deployable = deployable2;
		}
		return null;
	}

	public bool CanUpgradeInPlace(BasePlayer player, BaseEntity replaced, Construction upgrade)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)replaced).transform.position;
		Quaternion rotation = ((Component)replaced).transform.rotation;
		float num = upgrade.maxplaceDistance + 1f;
		if (isServer && replaced is BaseCombatEntity baseCombatEntity)
		{
			float num2 = baseCombatEntity.GetDamageRepairCooldown();
			if (player.IsInCreativeMode && Creative.freeRepair)
			{
				num2 = 0f;
			}
			if (baseCombatEntity.SecondsSinceAttacked <= num2)
			{
				Construction.lastPlacementError = Phrase.op_Implicit(string.Format(ConstructionErrors.CantUpgradeRecentlyDamaged.translated, (num2 - baseCombatEntity.SecondsSinceAttacked).ToString("N0")));
				return false;
			}
		}
		if (replaced.Distance(player.eyes.position) > num)
		{
			Construction.lastPlacementError = ConstructionErrors.TooFarAway;
			return false;
		}
		if (!replaced.IsVisible(player.eyes.position, num))
		{
			Construction.lastPlacementError = ConstructionErrors.LineOfSightBlocked;
			return false;
		}
		return PassesTierPlacement(player, upgrade, position, rotation, replaced);
	}

	private bool PassesTierPlacement(BasePlayer player, Construction tier, Vector3 position, Quaternion rotation, BaseEntity replaced)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (!tier.canBypassBuildingPermission && player.IsBuildingBlocked(position, rotation, tier.bounds, cached: false))
		{
			Construction.lastPlacementError = ConstructionErrors.NoPermission;
			return false;
		}
		DeployVolume[] volumes = prefabAttribute.FindAll<DeployVolume>(tier.prefabID);
		if (DeployVolume.Check(position, rotation, volumes, null, DeployVolume.TypeFilterMode.Include, replaced))
		{
			if ((Object)(object)DeployVolume.LastDeployHit != (Object)null)
			{
				Construction.lastPlacementErrorDebug = ((Object)DeployVolume.LastDeployHit).name;
				string blockedByErrorFromCollider = ConstructionErrors.GetBlockedByErrorFromCollider(DeployVolume.LastDeployHit, player);
				if (!string.IsNullOrEmpty(blockedByErrorFromCollider))
				{
					Construction.lastPlacementError = Phrase.op_Implicit(blockedByErrorFromCollider);
					Construction.lastPlacementErrorIsDetailed = true;
					return false;
				}
			}
			Construction.lastPlacementError = ConstructionErrors.NotEnoughSpace;
			return false;
		}
		return PassesSocketMods(tier, position, rotation, replaced);
	}

	public bool CanPlaceRemainingUpgrades(BasePlayer player, Vector3 position, Quaternion rotation, BaseEntity replaced, Deployable tier)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Phrase lastPlacementError = Construction.lastPlacementError;
		string lastPlacementErrorDebug = Construction.lastPlacementErrorDebug;
		bool lastPlacementErrorIsDetailed = Construction.lastPlacementErrorIsDetailed;
		bool flag = true;
		for (int i = 0; (i < 8) & flag; i++)
		{
			if (!(tier != null))
			{
				break;
			}
			if (!tier.upgradesTo.isValid)
			{
				break;
			}
			tier = prefabAttribute.Find<Deployable>(tier.upgradesTo.resourceID);
			if (tier == null)
			{
				break;
			}
			Construction construction = prefabAttribute.Find<Construction>(tier.prefabID);
			if (!(construction == null))
			{
				flag = PassesTierPlacement(player, construction, position, rotation, replaced);
			}
		}
		Construction.lastPlacementError = lastPlacementError;
		Construction.lastPlacementErrorDebug = lastPlacementErrorDebug;
		Construction.lastPlacementErrorIsDetailed = lastPlacementErrorIsDetailed;
		return flag;
	}

	private static bool PassesSocketMods(Construction construction, Vector3 position, Quaternion rotation, BaseEntity replaced)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Socket_Base[] allSockets = construction.allSockets;
		foreach (Socket_Base socket_Base in allSockets)
		{
			if (socket_Base is Socket_Free)
			{
				Construction.Placement placement = new Construction.Placement
				{
					position = position,
					rotation = rotation,
					isPopulated = true,
					ignoredEntity = replaced
				};
				if (!socket_Base.CheckSocketMods(ref placement))
				{
					return false;
				}
			}
		}
		return true;
	}

	public BaseEntity DoUpgrade(BasePlayer player, BaseEntity replaced, Construction component)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		Construction.lastPlacementError = Phrase.op_Implicit("");
		Construction.lastPlacementErrorDebug = "";
		Construction.lastPlacementErrorIsDetailed = false;
		if (!CanUpgradeInPlace(player, replaced, component))
		{
			if (!string.IsNullOrEmpty(Construction.lastPlacementError.translated))
			{
				player.ShowToast(GameTip.Styles.Error, Construction.lastPlacementError, false);
			}
			ConstructionErrors.Log(player, Construction.lastPlacementErrorDebug);
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(GameManager.server.CreatePrefab(component.fullName, ((Component)replaced).transform.position, ((Component)replaced).transform.rotation, active: false));
		if ((Object)(object)baseEntity == (Object)null)
		{
			return null;
		}
		if (baseEntity is DecayEntity decayEntity)
		{
			decayEntity.AttachToBuilding(replaced as DecayEntity);
		}
		float inheritedHealthFraction = ((ConVar.Server.upgrade_keeps_health && replaced is BaseCombatEntity baseCombatEntity) ? baseCombatEntity.healthFraction : 1f);
		GameObject val = SpawnConstruction(baseEntity, player, replaced.skinID, inheritedHealthFraction);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		BaseEntity baseEntity2 = GameObjectEx.ToBaseEntity(val);
		if (replaced.HasParent())
		{
			baseEntity2.SetParent(replaced.GetParentEntity(), worldPositionStays: true);
		}
		OnConstructionBuilt(baseEntity2, component.deployable, player, component, ((Component)baseEntity2).transform.position, Vector3.up);
		replaced.Kill();
		if (baseEntity2 is DecayEntity decayEntity2)
		{
			decayEntity2.timePlaced = GetNetworkTime();
		}
		return baseEntity2;
	}

	public ItemModDeployable GetModDeployable()
	{
		ItemDefinition ownerItemDefinition = GetOwnerItemDefinition();
		if ((Object)(object)ownerItemDefinition == (Object)null)
		{
			return null;
		}
		return ((Component)ownerItemDefinition).GetComponent<ItemModDeployable>();
	}

	public virtual Deployable GetDeployable()
	{
		ItemModDeployable modDeployable = GetModDeployable();
		if ((Object)(object)modDeployable == (Object)null)
		{
			return null;
		}
		return modDeployable.GetDeployable(this);
	}

	public virtual Deployable GetDeployable(NetworkableId entityId)
	{
		return GetDeployable();
	}
}
