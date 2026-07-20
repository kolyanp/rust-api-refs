using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Ai.Gen2.Nav;
using UnityEngine;
using UnityEngine.Assertions;

public class DecayEntity : BaseCombatEntity
{
	[Serializable]
	public struct DebrisPosition
	{
		public Vector3 Position;

		public Vector3 Rotation;

		public bool dropToTerrain;
	}

	public struct DecayEntityPreserveInfo
	{
		public bool canBeDemolished;
	}

	public static readonly Phrase CancelTitle = new Phrase("cancel", "Cancel");

	public static readonly Phrase CancelDesc = new Phrase("cancel_desc", "");

	public static readonly Phrase DemolishTitle = new Phrase("demolish", "Demolish");

	public static readonly Phrase DemolishDesc = new Phrase("demolish_desc", "Slowly and automatically dismantle this block");

	[ServerVar(Help = "(Generated) Time window in seconds after placement during which a player can demolish their own building block; default 600s (10 minutes)")]
	public static int demolish_seconds = 600;

	public const Flags DemolishFlag = Flags.Reserved2;

	[Header("Demolish")]
	public bool canBeDemolished;

	public GameObjectRef debrisPrefab;

	public Vector3 debrisRotationOffset = Vector3.zero;

	public DebrisPosition[] DebrisPositions;

	[NonSerialized]
	public uint buildingID;

	public float timePlaced;

	public float decayTimer;

	public float upkeepTimer;

	public Upkeep upkeep;

	[ServerVar(Help = "(Generated) When enabled, logs detailed debug output for building privilege (tool cupboard auth) checks during decay calculations")]
	public static bool DebugGetPrivilege = false;

	public Decay decay;

	public DecayPoint[] decayPoints;

	public float lastDecayTick;

	public float decayVariance = 1f;

	public virtual bool IsDemolishSupported => canBeDemolished;

	public Upkeep Upkeep => upkeep;

	public virtual bool BypassInsideDecayMultiplier => false;

	public virtual bool AllowOnCargoShip => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("DecayEntity.OnRpcMessage"))
		{
			if (rpc == 2858062413u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DoDemolish"));
				}
				using (TimeWarning.New("DoDemolish"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2858062413u, "DoDemolish", this, player, 3f))
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
							DoDemolish(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in DoDemolish");
					}
				}
				return true;
			}
			if (rpc == 216608990 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DoImmediateDemolish"));
				}
				using (TimeWarning.New("DoImmediateDemolish"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(216608990u, "DoImmediateDemolish", this, player, 3f))
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
							DoImmediateDemolish(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in DoImmediateDemolish");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool CanDemolish(BasePlayer player)
	{
		object obj = Interface.CallHook("CanDemolish", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (IsDemolishSupported && IsDemolishable())
		{
			return HasDemolishPrivilege(player);
		}
		return false;
	}

	public virtual bool IsDemolishable()
	{
		if (!ConVar.Server.pve && !HasFlag(Flags.Reserved2))
		{
			return false;
		}
		return true;
	}

	public virtual bool HasDemolishPrivilege(BasePlayer player)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return player.IsBuildingAuthed(((Component)this).transform.position, ((Component)this).transform.rotation, bounds);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void DoDemolish(RPCMessage msg)
	{
		if (msg.player.CanInteract() && CanDemolish(msg.player) && Interface.CallHook("OnStructureDemolish", this, msg.player, false) == null)
		{
			StabilityEntity stabilityEntity = this as StabilityEntity;
			if ((Object)(object)stabilityEntity != (Object)null)
			{
				Facepunch.Rust.Analytics.Azure.OnBuildingBlockDemolished(msg.player, stabilityEntity);
			}
			Kill(DestroyMode.Gib);
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void DoImmediateDemolish(RPCMessage msg)
	{
		if (msg.player.CanInteract() && msg.player.IsAdmin && Interface.CallHook("OnStructureDemolish", this, msg.player, true) == null)
		{
			StabilityEntity stabilityEntity = this as StabilityEntity;
			if ((Object)(object)stabilityEntity != (Object)null)
			{
				Facepunch.Rust.Analytics.Azure.OnBuildingBlockDemolished(msg.player, stabilityEntity);
			}
			Kill(DestroyMode.Gib);
		}
	}

	public void StopBeingDemolishable()
	{
		SetFlagLocal(Flags.Reserved2, b: false);
		SendNetworkUpdate();
	}

	public void StartBeingDemolishable()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved2, b: true);
		}
		Invoke(StopBeingDemolishable, demolish_seconds);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.decayEntity = Pool.Get<DecayEntity>();
		info.msg.decayEntity.buildingID = buildingID;
		if (info.forDisk)
		{
			info.msg.decayEntity.decayTimer = decayTimer;
			info.msg.decayEntity.upkeepTimer = upkeepTimer;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.decayEntity != null)
		{
			decayTimer = info.msg.decayEntity.decayTimer;
			upkeepTimer = info.msg.decayEntity.upkeepTimer;
			if (buildingID != info.msg.decayEntity.buildingID)
			{
				AttachToBuilding(info.msg.decayEntity.buildingID);
				if (info.fromDisk)
				{
					BuildingManager.server.LoadBuildingID(buildingID);
				}
			}
		}
		if (info.fromDisk && IsDemolishSupported)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved2, b: false);
			}
		}
	}

	public override void ResetState()
	{
		base.ResetState();
		buildingID = 0u;
		if (base.isServer)
		{
			decayTimer = 0f;
		}
	}

	public void AttachToBuilding(uint id)
	{
		if (base.isServer)
		{
			BuildingManager.server.Remove(this);
			buildingID = id;
			BuildingManager.server.Add(this);
			SendNetworkUpdate();
		}
	}

	public BuildingManager.Building GetBuilding()
	{
		if (base.isServer)
		{
			return BuildingManager.server.GetBuilding(buildingID);
		}
		return null;
	}

	public override BuildingPrivlidge GetBuildingPrivilege()
	{
		BuildingManager.Building building = GetBuilding();
		if (building != null)
		{
			BuildingPrivlidge dominatingBuildingPrivilege = building.GetDominatingBuildingPrivilege();
			if ((Object)(object)dominatingBuildingPrivilege != (Object)null || CanReturnEmptyBuildingPrivilege())
			{
				return dominatingBuildingPrivilege;
			}
		}
		return base.GetBuildingPrivilege();
	}

	public virtual IPrivilege GetPrivilege(bool useFallbackVisEntities = true)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		BuildingManager.Building building = GetBuilding();
		BaseBoat parentBoat2;
		if (building != null)
		{
			if (HasParentBoat(out var parentBoat))
			{
				VehiclePrivilege childPrivilege = parentBoat.GetChildPrivilege();
				DebugDrawGetPrivilege(childPrivilege, slowPath: false);
				return childPrivilege;
			}
			if (building.IsABoatBuilding())
			{
				BoatBuildingStation stationOverlappingPosition = BoatBuildingStation.GetStationOverlappingPosition(((Component)this).transform.position, base.isServer);
				if ((Object)(object)stationOverlappingPosition != (Object)null)
				{
					SteeringWheel steeringWheel = stationOverlappingPosition.GetSteeringWheel();
					if ((Object)(object)steeringWheel != (Object)null)
					{
						DebugDrawGetPrivilege(steeringWheel.Privilege, slowPath: false);
						return steeringWheel.Privilege;
					}
				}
			}
			BuildingPrivlidge dominatingBuildingPrivilege = building.GetDominatingBuildingPrivilege();
			if ((Object)(object)dominatingBuildingPrivilege != (Object)null || CanReturnEmptyBuildingPrivilege())
			{
				DebugDrawGetPrivilege(dominatingBuildingPrivilege, slowPath: false);
				return dominatingBuildingPrivilege;
			}
		}
		else if (HasParentBoat(out parentBoat2))
		{
			VehiclePrivilege childPrivilege2 = parentBoat2.GetChildPrivilege();
			DebugDrawGetPrivilege(childPrivilege2, slowPath: false);
			return childPrivilege2;
		}
		if (useFallbackVisEntities)
		{
			BuildingPrivlidge nearestBuildingPrivilege = GetNearestBuildingPrivilege(PrivilegeCacheDefaultValue());
			DebugDrawGetPrivilege(nearestBuildingPrivilege, slowPath: true);
			return nearestBuildingPrivilege;
		}
		return null;
	}

	private void DebugDrawGetPrivilege(IPrivilege privilege, bool slowPath)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		if (DebugGetPrivilege)
		{
			if (slowPath)
			{
				ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(((Component)this).transform.position, 5f, Color.red, 16f));
				ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Text(((Component)this).transform.position, 5f, Color.red, "GetPrivilege Vis.Entities"));
			}
			if (privilege is BaseEntity baseEntity)
			{
				ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Line(((Component)this).transform.position, ((Component)baseEntity).transform.position, 2f, Color.green));
				ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Text(Vector3.Lerp(((Component)this).transform.position, ((Component)baseEntity).transform.position, 0.2f), 2f, Color.green, $"{baseEntity.ShortPrefabName} ({baseEntity.net.ID})", 1f));
			}
		}
	}

	public virtual bool CanReturnEmptyBuildingPrivilege()
	{
		return false;
	}

	public void CalculateUpkeepCostAmounts(List<ItemAmount> itemAmounts, float multiplier)
	{
		if (upkeep == null)
		{
			return;
		}
		float num = upkeep.upkeepMultiplier * multiplier;
		if (num == 0f)
		{
			return;
		}
		List<ItemAmount> list = BuildCost();
		if (list == null)
		{
			return;
		}
		foreach (ItemAmount item in list)
		{
			if (item.itemDef.category != ItemCategory.Resources)
			{
				continue;
			}
			float num2 = item.amount * num;
			bool flag = false;
			foreach (ItemAmount itemAmount in itemAmounts)
			{
				if ((Object)(object)itemAmount.itemDef == (Object)(object)item.itemDef)
				{
					itemAmount.amount += num2;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				itemAmounts.Add(new ItemAmount(item.itemDef, num2));
			}
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		decayVariance = Random.Range(0.95f, 1f);
		decay = PrefabAttribute.server.Find<Decay>(prefabID);
		decayPoints = PrefabAttribute.server.FindAll<DecayPoint>(prefabID);
		upkeep = PrefabAttribute.server.Find<Upkeep>(prefabID);
		BuildingManager.server.Add(this);
		if (!Application.isLoadingSave)
		{
			BuildingManager.server.CheckMerge(this);
		}
		lastDecayTick = Time.time;
		if (IsDemolishSupported && (HasFlag(Flags.Reserved2) || !Application.isLoadingSave))
		{
			StartBeingDemolishable();
		}
	}

	public override void PostInitShared()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		base.PostInitShared();
		if (base.isServer && !AI.useUnityNavmesh)
		{
			RustNavigation instance = RustNavigation.Instance;
			OBB val = WorldSpaceBounds();
			instance.RebuildTilesInBounds(((OBB)(ref val)).ToBounds());
		}
	}

	public override void DoServerDestroy()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		base.DoServerDestroy();
		BuildingManager.server.Remove(this);
		BuildingManager.server.CheckSplit(this);
		if (!AI.useUnityNavmesh)
		{
			RustNavigation instance = RustNavigation.Instance;
			OBB val = WorldSpaceBounds();
			instance.RebuildTilesInBounds(((OBB)(ref val)).ToBounds());
		}
	}

	public override bool ShouldUseCastNoClipChecks()
	{
		return Time.time - timePlaced <= 5f;
	}

	public virtual void AttachToBuilding(DecayEntity other)
	{
		if ((Object)(object)other != (Object)null)
		{
			AttachToBuilding(other.buildingID);
			BuildingManager.server.CheckMerge(this);
			return;
		}
		BuildingBlock nearbyBuildingBlock = GetNearbyBuildingBlock();
		if (Object.op_Implicit((Object)(object)nearbyBuildingBlock))
		{
			AttachToBuilding(nearbyBuildingBlock.buildingID);
		}
	}

	public BuildingBlock GetNearbyBuildingBlock()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		BuildingBlock result = null;
		Vector3 position = PivotPoint();
		List<BuildingBlock> list = Pool.Get<List<BuildingBlock>>();
		Vis.Entities(position, 1.5f, list, 136314880, (QueryTriggerInteraction)2);
		for (int i = 0; i < list.Count; i++)
		{
			BuildingBlock buildingBlock = list[i];
			if (buildingBlock.isServer == base.isServer)
			{
				float num2 = buildingBlock.SqrDistance(position);
				if (!buildingBlock.grounded)
				{
					num2 += 1f;
				}
				if (num2 < num)
				{
					num = num2;
					result = buildingBlock;
				}
			}
		}
		Pool.FreeUnmanaged<BuildingBlock>(ref list);
		return result;
	}

	public void ResetUpkeepTime()
	{
		upkeepTimer = 0f;
	}

	public void DecayTouch()
	{
		decayTimer = 0f;
	}

	public void AddUpkeepTime(float time)
	{
		upkeepTimer -= time;
	}

	public float GetProtectedSeconds()
	{
		return Mathf.Max(0f, 0f - upkeepTimer);
	}

	public virtual float GetEntityDecayDuration()
	{
		return decay.GetDecayDuration(this);
	}

	public virtual float GetEntityHealScale()
	{
		return decay.GetHealScale(this);
	}

	public virtual float GetEntityDecayDelay()
	{
		return decay.GetDecayDelay(this);
	}

	public virtual void DecayTick()
	{
		if (!(decay == null))
		{
			float num = decay.GetDecayTickOverride();
			if (num == 0f)
			{
				num = ConVar.Decay.tick;
			}
			float num2 = Time.time - lastDecayTick;
			if (!(num2 < num))
			{
				OnDecay(decay, num2);
			}
		}
	}

	public virtual void OnDecay(Decay decay, float decayDeltaTime)
	{
		lastDecayTick = Time.time;
		if (HasParent() || !decay.ShouldDecay(this))
		{
			return;
		}
		float num = decayDeltaTime * ConVar.Decay.scale;
		if (ConVar.Decay.upkeep)
		{
			upkeepTimer += num;
			if (upkeepTimer > 0f)
			{
				BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege();
				if ((Object)(object)buildingPrivilege != (Object)null)
				{
					upkeepTimer -= buildingPrivilege.PurchaseUpkeepTime(this, Mathf.Max(upkeepTimer, 600f));
				}
			}
			if (upkeepTimer < 1f)
			{
				if (base.healthFraction < 1f && GetEntityHealScale() > 0f && base.SecondsSinceAttacked > 600f && Interface.CallHook("OnDecayHeal", this) == null)
				{
					float num2 = decayDeltaTime / GetEntityDecayDuration() * GetEntityHealScale();
					Heal(MaxHealth() * num2);
				}
				return;
			}
			upkeepTimer = 1f;
		}
		decayTimer += num;
		if (decayTimer < GetEntityDecayDelay())
		{
			return;
		}
		using (TimeWarning.New("DecayTick"))
		{
			float num3 = 1f;
			if (ConVar.Decay.upkeep)
			{
				if (!BypassInsideDecayMultiplier && !IsOutside())
				{
					num3 *= ConVar.Decay.upkeep_inside_decay_scale;
				}
			}
			else
			{
				for (int i = 0; i < decayPoints.Length; i++)
				{
					DecayPoint decayPoint = decayPoints[i];
					if (decayPoint.IsOccupied(this))
					{
						num3 -= decayPoint.protection;
					}
				}
			}
			if (Interface.CallHook("OnDecayDamage", this) == null && num3 > 0f)
			{
				float num4 = num / GetEntityDecayDuration() * MaxHealth();
				Hurt(num4 * num3 * decayVariance, DamageType.Decay);
			}
		}
	}

	public override void OnRepairFinished(BasePlayer player)
	{
		base.OnRepairFinished(player);
		DecayTouch();
	}

	public override void OnDied(HitInfo info)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (debrisPrefab.isValid)
		{
			if (DebrisPositions != null && DebrisPositions.Length != 0)
			{
				DebrisPosition[] debrisPositions = DebrisPositions;
				for (int i = 0; i < debrisPositions.Length; i++)
				{
					DebrisPosition debrisPosition = debrisPositions[i];
					SpawnDebris(debrisPosition.Position, Quaternion.Euler(debrisPosition.Rotation), debrisPosition.dropToTerrain);
				}
			}
			else
			{
				SpawnDebris(Vector3.zero, Quaternion.Euler(debrisRotationOffset), dropToTerrain: false);
			}
		}
		base.OnDied(info);
	}

	private void SpawnDebris(Vector3 localPos, Quaternion rot, bool dropToTerrain)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnDebrisSpawn", this, localPos, rot, dropToTerrain) != null)
		{
			return;
		}
		Vector3 val = ((Component)this).transform.TransformPoint(localPos);
		RaycastHit val2 = default(RaycastHit);
		if (dropToTerrain && Physics.Raycast(val, Vector3.down, ref val2, 6f, 8388608))
		{
			float num = val.y - ((RaycastHit)(ref val2)).point.y;
			val.y = ((RaycastHit)(ref val2)).point.y;
			localPos.y -= num;
		}
		List<DebrisEntity> list = Pool.Get<List<DebrisEntity>>();
		Vis.Entities(val, 0.1f, list, 256, (QueryTriggerInteraction)2);
		if (list.Count > 0)
		{
			Pool.FreeUnmanaged<DebrisEntity>(ref list);
			return;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(debrisPrefab.resourcePath, ((Component)this).transform.TransformPoint(localPos), ((Component)this).transform.rotation * rot);
		if (Object.op_Implicit((Object)(object)baseEntity))
		{
			baseEntity.SetParent(parentEntity.Get(serverside: true), worldPositionStays: true);
			baseEntity.Spawn();
		}
		Pool.FreeUnmanaged<DebrisEntity>(ref list);
	}

	public override void Reskin_Preserve(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		base.Reskin_Preserve(ref preserveInfo);
		preserveInfo.decayEntityPreserve.canBeDemolished = HasFlag(Flags.Reserved2);
	}

	public override void Reskin_Restore(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		base.Reskin_Restore(ref preserveInfo);
		ref DecayEntityPreserveInfo decayEntityPreserve = ref preserveInfo.decayEntityPreserve;
		if (decayEntityPreserve.canBeDemolished != HasFlag(Flags.Reserved2))
		{
			SetFlagLocal(Flags.Reserved2, decayEntityPreserve.canBeDemolished);
		}
		AttachToBuilding(null);
	}

	public override bool SupportsChildDeployables()
	{
		BaseEntity baseEntity = GetParentEntity();
		if (!((Object)(object)baseEntity != (Object)null))
		{
			return false;
		}
		return baseEntity.ForceDeployableSetParent();
	}

	public override bool ForceDeployableSetParent()
	{
		BaseEntity baseEntity = GetParentEntity();
		if (!((Object)(object)baseEntity != (Object)null))
		{
			return false;
		}
		return baseEntity.ForceDeployableSetParent();
	}
}
