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

	public BaseEntity[] buildableList;

	public Vector3 serverStartDurationPlacementPosition { get; private set; }

	public TimeSince serverStartDurationPlacementTime { get; private set; }

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
	[RPC_Server.CallsPerSecond(10uL)]
	[RPC_Server.IsActiveItem]
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
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
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
			ownerPlayer.ShowToast(GameTip.Styles.Error, ConstructionErrors.DeployableMismatch, false);
			return;
		}
		Construction.Target target = default(Construction.Target);
		if (((NetworkableId)(ref msg.entity)).IsValid)
		{
			target.entity = BaseNetworkable.serverEntities.Find(msg.entity) as BaseEntity;
			if ((Object)(object)target.entity == (Object)null)
			{
				ownerPlayer.ShowToast(GameTip.Styles.Error, ConstructionErrors.CouldntFindEntity, false);
				ConstructionErrors.Log(ownerPlayer, ((object)Unsafe.As<NetworkableId, NetworkableId>(ref msg.entity)/*cast due to constrained. prefix*/).ToString());
				return;
			}
			msg.ray = new Ray(((Component)target.entity).transform.TransformPoint(((Ray)(ref msg.ray)).origin), ((Component)target.entity).transform.TransformDirection(((Ray)(ref msg.ray)).direction));
			msg.position = ((Component)target.entity).transform.TransformPoint(msg.position);
			msg.normal = ((Component)target.entity).transform.TransformDirection(msg.normal);
			msg.rotation = ((Component)target.entity).transform.rotation * msg.rotation;
			if (msg.isSnapped)
			{
				msg.snappedPosition = ((Component)target.entity).transform.TransformPoint(msg.snappedPosition);
				Quaternion val2 = Quaternion.Euler(msg.snappedRotation);
				Quaternion val3 = ((Component)target.entity).transform.rotation * val2;
				msg.snappedRotation = ((Quaternion)(ref val3)).eulerAngles;
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
		BaseEntity baseEntity = DoBuild(target, construction);
		if ((Object)(object)baseEntity != (Object)null && baseEntity is DecayEntity decayEntity)
		{
			decayEntity.timePlaced = GetNetworkTime();
		}
		if ((Object)(object)baseEntity != (Object)null && baseEntity is BuildingBlock buildingBlock && ownerPlayer.IsInCreativeMode && Creative.freeBuild)
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
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
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
			if ((Object)(object)baseEntity != (Object)null && deployable != null)
			{
				if (ShouldParent(target.entity, deployable))
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
				if (deployable.wantsInstanceData && GetOwnerItem().instanceData != null)
				{
					(baseEntity as IInstanceDataReceiver).ReceiveInstanceData(GetOwnerItem().instanceData);
				}
				if (deployable.copyInventoryFromItem)
				{
					StorageContainer component2 = ((Component)baseEntity).GetComponent<StorageContainer>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						component2.ReceiveInventoryFromItem(GetOwnerItem());
					}
					if ((Object)(object)component2 == (Object)null)
					{
						ContainerIOEntity component3 = ((Component)baseEntity).GetComponent<ContainerIOEntity>();
						if (Object.op_Implicit((Object)(object)component3))
						{
							component3.ReceiveInventoryFromItem(GetOwnerItem());
						}
					}
				}
				ItemModDeployable modDeployable = GetModDeployable();
				if ((Object)(object)modDeployable != (Object)null)
				{
					modDeployable.OnDeployed(baseEntity, ownerPlayer);
				}
				baseEntity.OnDeployed(baseEntity.GetParentEntity(), ownerPlayer, GetOwnerItem());
				if (deployable.placeEffect.isValid)
				{
					if (Object.op_Implicit((Object)(object)target.entity) && target.socket != null)
					{
						Effect.server.Run(deployable.placeEffect.resourcePath, ((Component)target.entity).transform.TransformPoint(target.socket.worldPosition), ((Component)target.entity).transform.up);
					}
					else
					{
						Effect.server.Run(deployable.placeEffect.resourcePath, target.position, target.normal);
					}
				}
			}
			if ((Object)(object)baseEntity != (Object)null)
			{
				Facepunch.Rust.Analytics.Azure.OnEntityBuilt(baseEntity, ownerPlayer);
				if ((Object)(object)GetOwnerItemDefinition() != (Object)null)
				{
					ownerPlayer.ProcessMissionEvent(BaseMission.MissionEventType.DEPLOY, new BaseMission.MissionEventPayload
					{
						WorldPosition = ((Component)baseEntity).transform.position,
						UintIdentifier = baseEntity.prefabID,
						IntIdentifier = GetOwnerItemDefinition().itemid
					}, 1f);
				}
			}
			PayForPlacement(ownerPlayer, component);
			return baseEntity;
		}
		return null;
	}

	public GameObject DoPlacement(Construction.Target placement, Construction component)
	{
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
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
		float num = 1f;
		float num2 = 0f;
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null)
		{
			baseEntity.skinID = ownerItem.skin;
			if (ownerItem.hasCondition)
			{
				num = ownerItem.conditionNormalized;
			}
		}
		PoolableEx.AwakeFromInstantiate(((Component)baseEntity).gameObject);
		BuildingBlock buildingBlock = baseEntity as BuildingBlock;
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
		BaseCombatEntity baseCombatEntity = baseEntity as BaseCombatEntity;
		if (Object.op_Implicit((Object)(object)baseCombatEntity))
		{
			num2 = (((Object)(object)buildingBlock != (Object)null) ? buildingBlock.currentGrade.maxHealth : baseCombatEntity.startHealth);
			baseCombatEntity.ResetLifeStateOnSpawn = false;
			baseCombatEntity.InitializeHealth(num2 * num, baseCombatEntity.StartMaxHealth());
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
		baseEntity.OnPlaced(ownerPlayer);
		baseEntity.OwnerID = ownerPlayer.userID;
		baseEntity.Spawn();
		if (Object.op_Implicit((Object)(object)buildingBlock))
		{
			Effect.server.Run(buildingBlock.blockDefinition.placeEffect.isValid ? buildingBlock.blockDefinition.placeEffect.resourcePath : "assets/bundled/prefabs/fx/build/frame_place.prefab", baseEntity, 0u, Vector3.zero, Vector3.zero);
		}
		StabilityEntity stabilityEntity = baseEntity as StabilityEntity;
		if (Object.op_Implicit((Object)(object)stabilityEntity))
		{
			stabilityEntity.UpdateSurroundingEntities();
		}
		return ((Component)baseEntity).gameObject;
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
		Vector3 origin = ((Ray)(ref target.ray)).origin;
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
			val += ((Vector3)(ref target.normal)).normalized * num3;
		}
		if ((Object)(object)target.entity != (Object)null)
		{
			DeployShell deployShell = PrefabAttribute.server.Find<DeployShell>(target.entity.prefabID);
			if (deployShell != null)
			{
				val += ((Vector3)(ref target.normal)).normalized * deployShell.LineOfSightPadding();
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
		Vector3 origin = ((Ray)(ref target.ray)).origin;
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
			val += ((Vector3)(ref target.normal)).normalized * num3;
		}
		if ((Object)(object)target.entity != (Object)null)
		{
			DeployShell deployShell = PrefabAttribute.server.Find<DeployShell>(target.entity.prefabID);
			if (deployShell != null)
			{
				val += ((Vector3)(ref target.normal)).normalized * deployShell.LineOfSightPadding();
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
				if (!Physics.Raycast(origin, val3, ((Vector3)(ref val3)).magnitude, num4))
				{
					return true;
				}
			}
		}
		return flag;
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
