using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class BuildingBlock : StabilityEntity
{
	public static class BlockFlags
	{
		public const Flags CanRotate = Flags.Reserved1;
	}

	public class UpdateSkinWorkQueue : ObjectWorkQueue<BuildingBlock>
	{
		protected override void RunJob(BuildingBlock entity)
		{
			if (((ObjectWorkQueue<BuildingBlock>)this).ShouldAdd(entity))
			{
				entity.UpdateSkin(force: true);
			}
		}

		protected override bool ShouldAdd(BuildingBlock entity)
		{
			return entity.IsValid();
		}
	}

	private bool forceSkinRefresh;

	private ulong lastSkinID;

	public ulong lastModelState;

	private uint lastCustomColour;

	public uint playerCustomColourToApply;

	public BuildingGrade.Enum grade;

	public BuildingGrade.Enum lastGrade = BuildingGrade.Enum.None;

	protected ConstructionSkin currentSkin;

	private DeferredAction skinChange;

	private MeshRenderer placeholderRenderer;

	private MeshCollider placeholderCollider;

	public static UpdateSkinWorkQueue updateSkinQueueServer = new UpdateSkinWorkQueue();

	public static readonly Phrase RotateTitle = new Phrase("rotate", "Rotate");

	public static readonly Phrase RotateDesc = new Phrase("rotate_building_desc", "Rotate or flip this block to face a different direction");

	private bool globalNetworkCooldown;

	public bool CullBushes;

	public bool CheckForPipesOnModelChange;

	public OBBComponent AlternativePipeBounds;

	public bool useCastNoClipChecks;

	public const int WALLPAPER_MAXHEALTH = 100;

	[HideInInspector]
	public float wallpaperHealth = -1f;

	[HideInInspector]
	public float wallpaperRotation;

	[HideInInspector]
	public float wallpaperHealth2 = -1f;

	[HideInInspector]
	public float wallpaperRotation2;

	public ProtectionProperties wallpaperProtection;

	[NonSerialized]
	public Construction blockDefinition;

	private static Vector3[] outsideLookupOffsets;

	public ulong modelState { get; set; }

	public uint customColour { get; private set; }

	public ConstructionGrade currentGrade
	{
		get
		{
			if (blockDefinition == null)
			{
				Debug.LogWarning((object)$"blockDefinition is null for {base.ShortPrefabName} {grade} {skinID}");
				return null;
			}
			ConstructionGrade constructionGrade = blockDefinition.GetGrade(grade, skinID);
			if (constructionGrade == null)
			{
				Debug.LogWarning((object)$"currentGrade is null for {base.ShortPrefabName} {grade} {skinID}");
				return null;
			}
			return constructionGrade;
		}
	}

	public ulong wallpaperID { get; private set; }

	public ulong wallpaperID2 { get; private set; }

	public override bool IsDemolishSupported => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BuildingBlock.OnRpcMessage"))
		{
			if (rpc == 1956645865 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DoRotation"));
				}
				using (TimeWarning.New("DoRotation"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1956645865u, "DoRotation", this, player, 3f))
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
							DoRotation(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in DoRotation");
					}
				}
				return true;
			}
			if (rpc == 3746288057u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DoUpgradeToGrade"));
				}
				using (TimeWarning.New("DoUpgradeToGrade"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3746288057u, "DoUpgradeToGrade", this, player, 3f))
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
							DoUpgradeToGrade(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in DoUpgradeToGrade");
					}
				}
				return true;
			}
			if (rpc == 526349102 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_PickupWallpaperStart"));
				}
				using (TimeWarning.New("RPC_PickupWallpaperStart"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(526349102u, "RPC_PickupWallpaperStart", this, player, 3f))
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
							RPC_PickupWallpaperStart(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_PickupWallpaperStart");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void SetConditionalModel(ulong state)
	{
		if (state != modelState)
		{
			modelState = state;
			if (base.isServer)
			{
				GlobalNetworkHandler.server?.TrySendNetworkUpdate(this);
			}
		}
	}

	public bool GetConditionalModel(int index)
	{
		return (modelState & (ulong)(1L << index)) != 0;
	}

	public bool CanChangeToGrade(BuildingGrade.Enum iGrade, ulong iSkin, BasePlayer player)
	{
		object obj = Interface.CallHook("CanChangeGrade", player, this, iGrade, iSkin);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (player.IsInCreativeMode && Creative.freeBuild)
		{
			return true;
		}
		if (HasUpgradePrivilege(iGrade, iSkin, player))
		{
			return !IsUpgradeBlocked();
		}
		return false;
	}

	public bool HasUpgradePrivilege(BuildingGrade.Enum iGrade, ulong iSkin, BasePlayer player)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (player.IsInCreativeMode && Creative.freeBuild)
		{
			return true;
		}
		if (iGrade < grade)
		{
			return false;
		}
		if (iGrade == grade && iSkin == skinID)
		{
			return false;
		}
		if (iGrade <= BuildingGrade.Enum.None)
		{
			return false;
		}
		if (iGrade >= BuildingGrade.Enum.Count)
		{
			return false;
		}
		return !player.IsBuildingBlocked(((Component)this).transform.position, ((Component)this).transform.rotation, bounds);
	}

	public bool IsUpgradeBlocked()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (!blockDefinition.checkVolumeOnUpgrade)
		{
			return false;
		}
		DeployVolume[] volumes = PrefabAttribute.server.FindAll<DeployVolume>(prefabID);
		return DeployVolume.Check(((Component)this).transform.position, ((Component)this).transform.rotation, volumes, ~(1 << ((Component)this).gameObject.layer));
	}

	public bool CanAffordUpgrade(BuildingGrade.Enum iGrade, ulong iSkin, BasePlayer player)
	{
		object obj = Interface.CallHook("CanAffordUpgrade", player, this, iGrade, iSkin);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if ((Object)(object)player != (Object)null && player.IsInCreativeMode && Creative.freeBuild)
		{
			return true;
		}
		if (!ConVar.Decay.CanUpgradeToGrade(iGrade))
		{
			return false;
		}
		foreach (ItemAmount item in blockDefinition.GetGrade(iGrade, iSkin).CostToBuild(grade))
		{
			if ((float)player.inventory.GetAmount(item.itemid) < item.amount)
			{
				return false;
			}
		}
		return true;
	}

	public void SetGrade(BuildingGrade.Enum iGrade)
	{
		if (blockDefinition.grades == null || iGrade <= BuildingGrade.Enum.None || iGrade >= BuildingGrade.Enum.Count)
		{
			Debug.LogError((object)("Tried to set to undefined grade! " + blockDefinition.fullName), (Object)(object)((Component)this).gameObject);
			return;
		}
		grade = iGrade;
		grade = currentGrade.gradeBase.type;
		UpdateGrade();
	}

	public void UpdateGrade()
	{
		baseProtection = currentGrade.gradeBase.damageProtecton;
	}

	protected override void OnSkinChanged(ulong oldSkinID, ulong newSkinID)
	{
		if (oldSkinID != newSkinID)
		{
			skinID = newSkinID;
		}
	}

	protected override void OnSkinPreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
	}

	public void SetHealthToMax()
	{
		base.health = MaxHealth();
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void DoUpgradeToGrade(RPCMessage msg)
	{
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		if (!msg.player.CanInteract())
		{
			return;
		}
		ConstructionGrade constructionGrade = blockDefinition.GetGrade((BuildingGrade.Enum)msg.read.Int32(), msg.read.UInt64());
		if (constructionGrade == null)
		{
			return;
		}
		if (!CanChangeToGrade(constructionGrade.gradeBase.type, constructionGrade.gradeBase.skin, msg.player))
		{
			if (!((Object)(object)DeployVolume.LastDeployHit != (Object)null))
			{
				return;
			}
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(DeployVolume.LastDeployHit);
			if ((Object)(object)baseEntity != (Object)null && baseEntity is BasePlayer basePlayer)
			{
				ulong currentTeam = msg.player.currentTeam;
				if (currentTeam != 0L && currentTeam == basePlayer.currentTeam)
				{
					string playerNameStreamSafe = NameHelper.GetPlayerNameStreamSafe(msg.player, basePlayer);
					msg.player.ShowToast(GameTip.Styles.Error, ConstructionErrors.BlockedByPlayer, false, playerNameStreamSafe);
				}
			}
		}
		else
		{
			if (Interface.CallHook("OnStructureUpgrade", this, msg.player, constructionGrade.gradeBase.type, constructionGrade.gradeBase.skin) != null || !CanAffordUpgrade(constructionGrade.gradeBase.type, constructionGrade.gradeBase.skin, msg.player))
			{
				return;
			}
			if (base.SecondsSinceAttacked < 30f)
			{
				msg.player.ShowToast(GameTip.Styles.Error, ConstructionErrors.CantUpgradeRecentlyDamaged, false, (30f - base.SecondsSinceAttacked).ToString("N0"));
				return;
			}
			if (!constructionGrade.gradeBase.alwaysUnlock && constructionGrade.gradeBase.skin != 0L && !msg.player.blueprints.steamInventory.HasItem((int)constructionGrade.gradeBase.skin))
			{
				msg.player.ShowToast(GameTip.Styles.Error, ConstructionErrors.SkinNotOwned, false);
				return;
			}
			PayForUpgrade(constructionGrade, msg.player);
			if ((Object)(object)msg.player != (Object)null)
			{
				playerCustomColourToApply = GetShippingContainerBlockColourForPlayer(msg.player);
			}
			ClientRPC(RpcTarget.NetworkGroup("DoUpgradeEffect"), (int)constructionGrade.gradeBase.type, constructionGrade.gradeBase.skin);
			BuildingGrade.Enum obj = grade;
			OnSkinChanged(skinID, constructionGrade.gradeBase.skin);
			ChangeGrade(constructionGrade.gradeBase.type, playEffect: true);
			Facepunch.Rust.Analytics.Azure.OnBuildingBlockUpgraded(msg.player, this, constructionGrade.gradeBase.type, playerCustomColourToApply, constructionGrade.gradeBase.skin, (Object)(object)((Component)this).GetComponentInChildren<ConstructionSkin_CustomDetail>() != (Object)null);
			if ((Object)(object)msg.player != (Object)null && obj != constructionGrade.gradeBase.type)
			{
				msg.player.ProcessMissionEvent(BaseMission.MissionEventType.UPGRADE_BUILDING_GRADE, new BaseMission.MissionEventPayload
				{
					NetworkIdentifier = net.ID,
					IntIdentifier = (int)constructionGrade.gradeBase.type
				}, 1f);
			}
			Interface.CallHook("OnStructureUpgraded", this, msg.player, constructionGrade.gradeBase.type, constructionGrade.gradeBase.skin);
			timePlaced = GetNetworkTime();
		}
	}

	public static uint GetShippingContainerBlockColourForPlayer(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return 0u;
		}
		int infoInt = player.GetInfoInt("client.SelectedShippingContainerBlockColour", 0);
		if (infoInt >= 0)
		{
			return (uint)infoInt;
		}
		return 0u;
	}

	public void ChangeGradeAndSkin(BuildingGrade.Enum targetGrade, ulong skin, bool playEffect = false, bool updateSkin = true, uint? color = null)
	{
		OnSkinChanged(skinID, skin);
		ChangeGrade(targetGrade, playEffect, updateSkin);
		customColour = (color.HasValue ? currentSkin.GetStartingDetailColour(color.Value) : 0u);
	}

	public void ChangeGrade(BuildingGrade.Enum targetGrade, bool playEffect = false, bool updateSkin = true)
	{
		SetGrade(targetGrade);
		if (grade != lastGrade)
		{
			SetHealthToMax();
			StartBeingRotatable();
		}
		if (updateSkin)
		{
			UpdateSkin();
		}
		SendNetworkUpdate();
		ResetUpkeepTime();
		UpdateSurroundingEntities();
		GlobalNetworkHandler.server.TrySendNetworkUpdate(this);
		BuildingManager.server.GetBuilding(buildingID)?.Dirty();
	}

	public void PayForUpgrade(ConstructionGrade g, BasePlayer player)
	{
		if (Interface.CallHook("OnPayForUpgrade", player, this, g) != null || (player.IsInCreativeMode && Creative.freeBuild))
		{
			return;
		}
		List<Item> list = new List<Item>();
		foreach (ItemAmount item in g.CostToBuild(grade))
		{
			player.inventory.Take(list, item.itemid, (int)item.amount);
			ItemDefinition itemDefinition = ItemManager.FindItemDefinition(item.itemid);
			Facepunch.Rust.Analytics.Azure.LogResource(Facepunch.Rust.Analytics.Azure.ResourceMode.Consumed, "upgrade_block", itemDefinition.shortname, (int)item.amount, this, null, safezone: false, null, player.userID, null, null, null, 0uL);
			player.Command("note.inv " + item.itemid + " " + item.amount * -1f);
		}
		foreach (Item item2 in list)
		{
			item2.Remove();
		}
	}

	public void SetCustomColour(uint newColour)
	{
		if (newColour != customColour)
		{
			customColour = newColour;
			SendNetworkUpdateImmediate();
			ClientRPC(RpcTarget.NetworkGroup("RefreshSkin"));
			GlobalNetworkHandler.server.TrySendNetworkUpdate(this);
		}
	}

	public bool NeedsSkinChange()
	{
		if (!((Object)(object)currentSkin == (Object)null) && !forceSkinRefresh && lastGrade == grade && lastModelState == modelState)
		{
			return lastSkinID != skinID;
		}
		return true;
	}

	public void UpdateSkin(bool force = false)
	{
		if (force)
		{
			forceSkinRefresh = true;
		}
		if (!NeedsSkinChange())
		{
			return;
		}
		if (cachedStability <= 0f || base.isServer)
		{
			ChangeSkin();
			return;
		}
		if (!skinChange)
		{
			skinChange = new DeferredAction((Object)(object)this, ChangeSkin);
		}
		if (skinChange.Idle)
		{
			skinChange.Invoke();
		}
	}

	private void DestroySkin()
	{
		if ((Object)(object)currentSkin != (Object)null)
		{
			currentSkin.Destroy(this);
			currentSkin = null;
		}
	}

	public void RefreshNeighbours(bool linkToNeighbours)
	{
		List<EntityLink> entityLinks = GetEntityLinks(linkToNeighbours);
		for (int i = 0; i < entityLinks.Count; i++)
		{
			EntityLink entityLink = entityLinks[i];
			for (int j = 0; j < entityLink.connections.Count; j++)
			{
				BuildingBlock buildingBlock = entityLink.connections[j].owner as BuildingBlock;
				if (!((Object)(object)buildingBlock == (Object)null))
				{
					if (Application.isLoading)
					{
						buildingBlock.UpdateSkin(force: true);
					}
					else
					{
						((ObjectWorkQueue<BuildingBlock>)updateSkinQueueServer).Add(buildingBlock);
					}
				}
			}
		}
	}

	private void UpdatePlaceholder(bool state)
	{
		if (Object.op_Implicit((Object)(object)placeholderRenderer))
		{
			((Renderer)placeholderRenderer).enabled = state;
		}
		if (Object.op_Implicit((Object)(object)placeholderCollider))
		{
			((Collider)placeholderCollider).enabled = state;
		}
	}

	protected void ChangeSkin()
	{
		if (base.IsDestroyed)
		{
			return;
		}
		ConstructionGrade constructionGrade = currentGrade;
		if (currentGrade == null)
		{
			Debug.LogWarning((object)"CurrentGrade is null!");
			return;
		}
		if (constructionGrade.skinObject.isValid)
		{
			ChangeSkin(constructionGrade.skinObject);
			return;
		}
		ConstructionGrade defaultGrade = blockDefinition.defaultGrade;
		if (defaultGrade.skinObject.isValid)
		{
			ChangeSkin(defaultGrade.skinObject);
		}
		else
		{
			Debug.LogWarning((object)("No skins found for " + (object)((Component)this).gameObject));
		}
	}

	protected virtual void OnSkinRefresh()
	{
	}

	public void ChangeSkin(GameObjectRef prefab)
	{
		bool flag = lastGrade != grade || lastSkinID != skinID;
		lastGrade = grade;
		lastSkinID = skinID;
		if (flag)
		{
			if ((Object)(object)currentSkin == (Object)null)
			{
				UpdatePlaceholder(state: false);
			}
			else
			{
				DestroySkin();
			}
			GameObject val = base.gameManager.CreatePrefab(prefab.resourcePath, ((Component)this).transform);
			currentSkin = val.GetComponent<ConstructionSkin>();
			if ((Object)(object)currentSkin != (Object)null && base.isServer && !Application.isLoading)
			{
				customColour = currentSkin.GetStartingDetailColour(playerCustomColourToApply);
			}
			Model component = ((Component)currentSkin).GetComponent<Model>();
			SetModel(component);
			Assert.IsTrue((Object)(object)model == (Object)(object)component, "Didn't manage to set model successfully!");
		}
		if (base.isServer)
		{
			SetConditionalModel(currentSkin.DetermineConditionalModelState(this));
		}
		bool flag2 = lastModelState != modelState;
		lastModelState = modelState;
		bool flag3 = lastCustomColour != customColour;
		lastCustomColour = customColour;
		if (flag || flag2 || forceSkinRefresh || flag3)
		{
			currentSkin.Refresh(this);
			OnSkinRefresh();
			if (base.isServer && flag2)
			{
				CheckForPipes();
			}
			forceSkinRefresh = false;
		}
		if (base.isServer)
		{
			if (flag)
			{
				RefreshNeighbours(linkToNeighbours: true);
			}
			if (flag2)
			{
				SendNetworkUpdate();
			}
			timePlaced = GetNetworkTime();
		}
	}

	public override bool ShouldBlockProjectiles()
	{
		return grade != BuildingGrade.Enum.Twigs;
	}

	[ContextMenu("Check for pipes")]
	public void CheckForPipes()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!CheckForPipesOnModelChange || !ConVar.Server.enforcePipeChecksOnBuildingBlockChanges || Application.isLoading)
		{
			return;
		}
		List<ColliderInfo_Pipe> list = Pool.Get<List<ColliderInfo_Pipe>>();
		Bounds val = bounds;
		((Bounds)(ref val)).extents = ((Bounds)(ref val)).extents * 0.97f;
		Vis.Components<ColliderInfo_Pipe>((OBB)(((Object)(object)AlternativePipeBounds != (Object)null) ? AlternativePipeBounds.GetObb() : new OBB(((Component)this).transform, val)), list, 536870912, (QueryTriggerInteraction)2);
		foreach (ColliderInfo_Pipe item in list)
		{
			if (!((Object)(object)item == (Object)null) && ((Component)item).gameObject.activeInHierarchy && item.HasFlag(ColliderInfo.Flags.OnlyBlockBuildingBlock) && (Object)(object)item.ParentEntity != (Object)null && item.ParentEntity.isServer)
			{
				WireTool.AttemptClearSlot(item.ParentEntity, null, item.OutputSlotIndex, isInput: false);
			}
		}
		Pool.FreeUnmanaged<ColliderInfo_Pipe>(ref list);
	}

	private void OnHammered()
	{
	}

	public override float MaxHealth()
	{
		if (maxHealthOverride > 0f)
		{
			return maxHealthOverride;
		}
		return currentGrade.maxHealth;
	}

	public override List<ItemAmount> BuildCost()
	{
		return currentGrade.CostToBuild();
	}

	public override void OnHealthChanged(float oldvalue, float newvalue)
	{
		base.OnHealthChanged(oldvalue, newvalue);
		if (base.isServer && Mathf.RoundToInt(oldvalue) != Mathf.RoundToInt(newvalue))
		{
			SendNetworkUpdate(BasePlayer.NetworkQueue.UpdateDistance);
		}
	}

	public override float RepairCostFraction()
	{
		return 1f;
	}

	protected virtual bool CanRotate(BasePlayer player)
	{
		if (IsRotatable() && HasRotationPrivilege(player))
		{
			return !IsRotationBlocked();
		}
		return false;
	}

	public bool IsRotatable()
	{
		if (blockDefinition.grades == null)
		{
			return false;
		}
		if (!blockDefinition.canRotateAfterPlacement)
		{
			return false;
		}
		if (!HasRotateFlag())
		{
			return false;
		}
		return true;
	}

	public virtual bool HasRotateFlag()
	{
		return HasFlag(Flags.Reserved1);
	}

	public bool IsRotationBlocked()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (children != null)
		{
			foreach (BaseEntity child in children)
			{
				if (child is TimedExplosive)
				{
					return true;
				}
			}
		}
		if (!blockDefinition.checkVolumeOnRotate)
		{
			return false;
		}
		DeployVolume[] volumes = PrefabAttribute.server.FindAll<DeployVolume>(prefabID);
		return DeployVolume.Check(((Component)this).transform.position, ((Component)this).transform.rotation, volumes, ~(1 << ((Component)this).gameObject.layer));
	}

	public bool HasRotationPrivilege(BasePlayer player)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return !player.IsBuildingBlocked(((Component)this).transform.position, ((Component)this).transform.rotation, bounds);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void DoRotation(RPCMessage msg)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (msg.player.CanInteract() && CanRotate(msg.player) && blockDefinition.canRotateAfterPlacement && Interface.CallHook("OnStructureRotate", this, msg.player) == null)
		{
			Transform transform = ((Component)this).transform;
			transform.localRotation *= Quaternion.Euler(blockDefinition.rotationAmount);
			RefreshEntityLinks();
			UpdateSurroundingEntities();
			UpdateSkin(force: true);
			RefreshNeighbours(linkToNeighbours: false);
			SendNetworkUpdateImmediate();
			ClientRPC(RpcTarget.NetworkGroup("RefreshSkin"));
			if (!globalNetworkCooldown)
			{
				globalNetworkCooldown = true;
				GlobalNetworkHandler.server.TrySendNetworkUpdate(this);
				CancelInvoke(ResetGlobalNetworkCooldown);
				Invoke(ResetGlobalNetworkCooldown, 15f);
			}
		}
	}

	private void ResetGlobalNetworkCooldown()
	{
		globalNetworkCooldown = false;
		GlobalNetworkHandler.server.TrySendNetworkUpdate(this);
	}

	public void StopBeingRotatable()
	{
		SetFlagLocal(Flags.Reserved1, b: false);
		SendNetworkUpdate();
	}

	public void StartBeingRotatable()
	{
		if (blockDefinition.grades != null && blockDefinition.canRotateAfterPlacement)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: true);
			}
			Invoke(StopBeingRotatable, 600f);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.buildingBlock = Pool.Get<BuildingBlock>();
		info.msg.buildingBlock.model = modelState;
		info.msg.buildingBlock.grade = (int)grade;
		info.msg.buildingBlock.wallpaperID = wallpaperID;
		info.msg.buildingBlock.wallpaperID2 = wallpaperID2;
		info.msg.buildingBlock.wallpaperHealth = wallpaperHealth;
		info.msg.buildingBlock.wallpaperHealth2 = wallpaperHealth2;
		info.msg.buildingBlock.wallpaperRotation = wallpaperRotation;
		info.msg.buildingBlock.wallpaperRotation2 = wallpaperRotation2;
		if (customColour != 0)
		{
			info.msg.simpleUint = Pool.Get<SimpleUInt>();
			info.msg.simpleUint.value = customColour;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		customColour = 0u;
		if (info.msg.simpleUint != null)
		{
			customColour = info.msg.simpleUint.value;
		}
		if (info.msg.buildingBlock != null)
		{
			wallpaperID = info.msg.buildingBlock.wallpaperID;
			wallpaperID2 = info.msg.buildingBlock.wallpaperID2;
			wallpaperHealth = info.msg.buildingBlock.wallpaperHealth;
			wallpaperHealth2 = info.msg.buildingBlock.wallpaperHealth2;
			wallpaperRotation = info.msg.buildingBlock.wallpaperRotation;
			wallpaperRotation2 = info.msg.buildingBlock.wallpaperRotation2;
			SetConditionalModel(info.msg.buildingBlock.model);
			SetGrade((BuildingGrade.Enum)info.msg.buildingBlock.grade);
		}
		if (info.fromDisk)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: false);
			}
			UpdateSkin();
		}
	}

	public override void AttachToBuilding(DecayEntity other)
	{
		if ((Object)(object)other != (Object)null && other is BuildingBlock)
		{
			AttachToBuilding(other.buildingID);
			BuildingManager.server.CheckMerge(this);
		}
		else
		{
			AttachToBuilding(BuildingManager.server.NewBuildingID());
		}
	}

	public override void ServerInit()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		blockDefinition = PrefabAttribute.server.Find<Construction>(prefabID);
		if (blockDefinition == null)
		{
			Debug.LogError((object)("Couldn't find Construction for prefab " + prefabID));
		}
		base.ServerInit();
		UpdateSkin();
		if (HasFlag(Flags.Reserved1) || !Application.isLoadingSave)
		{
			StartBeingRotatable();
		}
		if (!CullBushes || Application.isLoadingSave)
		{
			return;
		}
		List<BushEntity> list = Pool.Get<List<BushEntity>>();
		Vis.Entities(WorldSpaceBounds(), list, 67108864, (QueryTriggerInteraction)2);
		foreach (BushEntity item in list)
		{
			if (item.isServer)
			{
				item.Kill();
			}
		}
		Pool.FreeUnmanaged<BushEntity>(ref list);
	}

	public override void Hurt(HitInfo info)
	{
		if (ConVar.Server.pve && Object.op_Implicit((Object)(object)info.Initiator) && info.Initiator is BasePlayer)
		{
			if (Interface.CallHook("OnPlayerPveDamage", info.Initiator, info, this) == null)
			{
				(info.Initiator as BasePlayer).Hurt(info.damageTypes.Total(), DamageType.Generic);
			}
		}
		else
		{
			if (Object.op_Implicit((Object)(object)info.Initiator) && info.Initiator is BasePlayer { IsInTutorial: not false })
			{
				return;
			}
			if (HasWallpaper())
			{
				DamageType majorityDamageType = info.damageTypes.GetMajorityDamageType();
				bool flag = info.damageTypes.Contains(DamageType.Explosion);
				DamageTypeList damageTypeList = info.damageTypes.Clone();
				if (wallpaperProtection != null)
				{
					wallpaperProtection.Scale(damageTypeList);
				}
				float totalDamage = damageTypeList.Total();
				if (majorityDamageType == DamageType.Decay || flag || majorityDamageType == DamageType.Heat)
				{
					DamageWallpaper(totalDamage);
					DamageWallpaper(totalDamage, 1);
				}
				else
				{
					bool flag2 = false;
					for (int i = 0; i < propDirection.Length; i++)
					{
						if (propDirection[i].IsWeakspot(((Component)this).transform, info))
						{
							flag2 = true;
							break;
						}
					}
					DamageWallpaper(totalDamage, (!flag2) ? 1 : 0);
				}
			}
			base.Hurt(info);
		}
	}

	public override bool ShouldAlwaysBlockNoClipChecks()
	{
		return true;
	}

	public override bool ShouldUseCastNoClipChecks()
	{
		if (!useCastNoClipChecks)
		{
			return base.ShouldUseCastNoClipChecks();
		}
		return true;
	}

	public bool HasWallpaper()
	{
		if (!(wallpaperHealth > 0f))
		{
			return wallpaperHealth2 > 0f;
		}
		return true;
	}

	public bool HasWallpaper(int side)
	{
		if (side != 0)
		{
			return wallpaperHealth2 > 0f;
		}
		return wallpaperHealth > 0f;
	}

	public ulong GetWallpaperSkin(int side)
	{
		if (side != 0)
		{
			return wallpaperID2;
		}
		return wallpaperID;
	}

	public float GetWallpaperRotation(int side)
	{
		if (side != 0)
		{
			return wallpaperRotation2;
		}
		return wallpaperRotation;
	}

	public void SetWallpaper(ulong id, int side = 0, float rotation = 0f)
	{
		if (Interface.CallHook("OnWallpaperSet", this, id, side, rotation) != null)
		{
			return;
		}
		if (side == 0)
		{
			if (HasWallpaper(side) && wallpaperID == id && wallpaperRotation == rotation)
			{
				return;
			}
			wallpaperID = id;
			wallpaperHealth = 100f;
			wallpaperRotation = rotation;
		}
		else
		{
			if (HasWallpaper(side) && wallpaperID2 == id && wallpaperRotation2 == rotation)
			{
				return;
			}
			wallpaperID2 = id;
			wallpaperHealth2 = 100f;
			wallpaperRotation2 = rotation;
		}
		if (base.isServer)
		{
			SetConditionalModel(currentSkin.DetermineConditionalModelState(this));
			SendNetworkUpdateImmediate();
			ClientRPC(RpcTarget.NetworkGroup("RefreshSkin"));
		}
	}

	public void RemoveWallpaper(int side)
	{
		if (Interface.CallHook("OnWallpaperRemove", this, side) == null)
		{
			switch (side)
			{
			case 0:
				wallpaperHealth = -1f;
				wallpaperID = 0uL;
				wallpaperRotation = 0f;
				break;
			case 1:
				wallpaperHealth2 = -1f;
				wallpaperID2 = 0uL;
				wallpaperRotation2 = 0f;
				break;
			}
			if (base.isServer)
			{
				SetConditionalModel(currentSkin.DetermineConditionalModelState(this));
				SendNetworkUpdateImmediate();
				ClientRPC(RpcTarget.NetworkGroup("RefreshSkin"));
			}
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void RPC_PickupWallpaperStart(RPCMessage msg)
	{
		if (msg.player.CanInteract() && ShouldDisplayPickupOption(msg.player) && CanCompletePickup(msg.player))
		{
			bool flag = msg.read.Bool();
			if (HasWallpaper((!flag) ? 1 : 0))
			{
				Item item = ItemManager.Create(WallpaperPlanner.Settings.PlacementPrice.itemDef, (int)WallpaperPlanner.Settings.PlacementPrice.amount, 0uL, isServerSide: true, 0uL);
				msg.player.GiveItem(item, GiveItemReason.PickedUp);
				RemoveWallpaper((!flag) ? 1 : 0);
			}
		}
	}

	private void DamageWallpaper(float totalDamage, int side = 0)
	{
		switch (side)
		{
		case 0:
			wallpaperHealth -= totalDamage;
			if (wallpaperHealth <= 0f)
			{
				RemoveWallpaper(0);
			}
			break;
		case 1:
			wallpaperHealth2 -= totalDamage;
			if (wallpaperHealth2 <= 0f)
			{
				RemoveWallpaper(1);
			}
			break;
		}
	}

	public override void StabilityCheck()
	{
		base.StabilityCheck();
		if (HasWallpaper())
		{
			Invoke(CheckWallpaper, 0.5f);
		}
	}

	public override void OnDecay(Decay decay, float decayDeltaTime)
	{
		base.OnDecay(decay, decayDeltaTime);
		if (HasWallpaper())
		{
			CheckWallpaper();
		}
	}

	public void CheckWallpaper()
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		if (Creative.allUsers && Creative.freePlacement)
		{
			return;
		}
		int side = WallpaperPlanner.Settings.GetSideThatMustBeInside(this);
		if (side == -1 || !HasWallpaper(side))
		{
			return;
		}
		Construction construction = WallpaperPlanner.Settings?.GetConstruction(this, 0);
		if (construction == null)
		{
			construction = WallpaperPlanner.Settings?.GetConstruction(this, 1);
			if (construction == null)
			{
				return;
			}
		}
		Socket_Base socket_Base = PrefabAttribute.server.FindAll<Socket_Base>(prefabID).FirstOrDefault((Socket_Base s) => s.socketName.Contains("wallpaper") && s.socketName.EndsWith((side == 0) ? "1" : "2"));
		if (socket_Base == null)
		{
			return;
		}
		SocketMod[] array = PrefabAttribute.server.FindAll<SocketMod>(construction.prefabID);
		for (int num = 0; num < array.Length; num++)
		{
			if (array[num] is SocketMod_Inside socketMod_Inside)
			{
				Construction.Placement place = new Construction.Placement(default(Construction.Target));
				place.position = ((Component)this).transform.position + ((Component)this).transform.rotation * socket_Base.localPosition;
				place.rotation = ((Component)this).transform.rotation * socket_Base.localRotation;
				if (!socketMod_Inside.DoCheck(ref place))
				{
					RemoveWallpaper(side);
					break;
				}
			}
		}
	}

	public bool CanSeeWallpaperSocket(BasePlayer player, int side = 0)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		Construction construction = WallpaperPlanner.Settings?.GetConstruction(this, side);
		if (construction == null)
		{
			return false;
		}
		Vector3 center = player.eyes.center;
		Vector3 position = player.eyes.position;
		if (!GamePhysics.LineOfSightRadius(center, position, 136380416, 0f))
		{
			return false;
		}
		Socket_Base socket_Base = PrefabAttribute.server.FindAll<Socket_Base>(prefabID).FirstOrDefault((Socket_Base s) => s.socketName.Contains("wallpaper") && s.socketName.EndsWith((side == 0) ? "1" : "2"));
		if (socket_Base == null)
		{
			return false;
		}
		Transform deployOffset = construction.deployOffset;
		Vector3 val = ((deployOffset != null) ? deployOffset.localPosition : Vector3.zero);
		Vector3 val2 = socket_Base.rotation * val;
		Vector3 val3 = ((Component)this).transform.position + ((Component)this).transform.rotation * val2;
		bool flag = false;
		Ray val4 = player.eyes.HeadRay();
		Vector3 val5 = val3 - ((Ray)(ref val4)).origin;
		val4 = player.eyes.HeadRay();
		if (!Physics.Raycast(new Ray(((Ray)(ref val4)).origin, ((Vector3)(ref val5)).normalized), ((Vector3)(ref val5)).magnitude, 136314880))
		{
			flag = true;
		}
		if (!flag && construction.HasAlternativeLOSChecks())
		{
			Vector3[] alternativeLOSPositions = construction.alternativeLOSPositions;
			foreach (Vector3 val6 in alternativeLOSPositions)
			{
				Vector3 val7 = ((Component)this).transform.position + ((Component)this).transform.rotation * socket_Base.localPosition;
				Quaternion val8 = ((Component)this).transform.rotation * socket_Base.localRotation;
				Vector3 val9 = val7 + val8 * val6 - center;
				if (!Physics.Raycast(center, val9, ((Vector3)(ref val9)).magnitude, 136314880))
				{
					flag = true;
					break;
				}
			}
		}
		return flag;
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (!HasWallpaper())
		{
			return false;
		}
		if (player.IsHoldingEntity<Hammer>() && player.CanBuild())
		{
			if (!HasWallpaper(0) || !CanSeeWallpaperSocket(player))
			{
				if (HasWallpaper(1))
				{
					return CanSeeWallpaperSocket(player, 1);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override void ResetState()
	{
		base.ResetState();
		blockDefinition = null;
		forceSkinRefresh = false;
		modelState = 0uL;
		lastModelState = 0uL;
		wallpaperID = 0uL;
		wallpaperID2 = 0uL;
		wallpaperHealth = -1f;
		wallpaperHealth2 = -1f;
		wallpaperRotation = 0f;
		wallpaperRotation2 = 0f;
		grade = BuildingGrade.Enum.Twigs;
		lastGrade = BuildingGrade.Enum.None;
		DestroySkin();
		UpdatePlaceholder(state: true);
	}

	public override void InitShared()
	{
		base.InitShared();
		placeholderRenderer = ((Component)this).GetComponent<MeshRenderer>();
		placeholderCollider = ((Component)this).GetComponent<MeshCollider>();
	}

	public override void PostInitShared()
	{
		baseProtection = currentGrade.gradeBase.damageProtecton;
		grade = currentGrade.gradeBase.type;
		base.PostInitShared();
	}

	public override void DestroyShared()
	{
		if (base.isServer)
		{
			RefreshNeighbours(linkToNeighbours: false);
		}
		base.DestroyShared();
	}

	public override string Categorize()
	{
		return "building";
	}

	public override float AntiHackPadding()
	{
		return 1f;
	}

	public override bool IsOutside()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		float outside_test_range = ConVar.Decay.outside_test_range;
		Vector3 val = PivotPoint();
		for (int i = 0; i < outsideLookupOffsets.Length; i++)
		{
			Vector3 val2 = outsideLookupOffsets[i];
			Vector3 val3 = val + val2 * outside_test_range;
			if (!Physics.Raycast(new Ray(val3, -val2), outside_test_range - 0.5f, 2097152))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool Interactable()
	{
		return true;
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	public override bool CanReturnEmptyBuildingPrivilege()
	{
		return true;
	}

	static BuildingBlock()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = new Vector3[5];
		Vector3 val = new Vector3(0f, 1f, 0f);
		array[0] = ((Vector3)(ref val)).normalized;
		val = new Vector3(1f, 1f, 0f);
		array[1] = ((Vector3)(ref val)).normalized;
		val = new Vector3(-1f, 1f, 0f);
		array[2] = ((Vector3)(ref val)).normalized;
		val = new Vector3(0f, 1f, 1f);
		array[3] = ((Vector3)(ref val)).normalized;
		val = new Vector3(0f, 1f, -1f);
		array[4] = ((Vector3)(ref val)).normalized;
		outsideLookupOffsets = (Vector3[])(object)array;
	}
}
