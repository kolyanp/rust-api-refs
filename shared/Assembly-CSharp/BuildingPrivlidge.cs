using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Spatial;
using UnityEngine;
using UnityEngine.Assertions;

public class BuildingPrivlidge : StorageContainer, IPrivilege
{
	public class UpkeepBracket
	{
		public int objectsUpTo;

		public float fraction;

		public float blocksTaxPaid;

		public UpkeepBracket(int numObjs, float frac)
		{
			objectsUpTo = numObjs;
			fraction = frac;
			blocksTaxPaid = 0f;
		}
	}

	public struct BuildingPrivilegePreserveInfo
	{
		public HashSet<ulong> authedPlayers;
	}

	public const Flags Flag_Raidable = Flags.Reserved7;

	private static HashSet<BuildingPrivlidge> raidStatusSet;

	private const int RefreshBatchSize = 20;

	private const float RefreshBatchInterval = 0.2f;

	public float cachedProtectedMinutes;

	public float nextProtectedCalcTime;

	public static UpkeepBracket[] upkeepBrackets = new UpkeepBracket[4]
	{
		new UpkeepBracket(ConVar.Decay.bracket_0_blockcount, ConVar.Decay.bracket_0_costfraction),
		new UpkeepBracket(ConVar.Decay.bracket_1_blockcount, ConVar.Decay.bracket_1_costfraction),
		new UpkeepBracket(ConVar.Decay.bracket_2_blockcount, ConVar.Decay.bracket_2_costfraction),
		new UpkeepBracket(0, ConVar.Decay.bracket_3_costfraction)
	};

	private static UpkeepBracket[] doorUpkeepBrackets = new UpkeepBracket[4]
	{
		new UpkeepBracket(ConVar.Decay.bracket_0_doorcount, ConVar.Decay.bracket_0_doorfraction),
		new UpkeepBracket(ConVar.Decay.bracket_1_doorcount, ConVar.Decay.bracket_1_doorfraction),
		new UpkeepBracket(ConVar.Decay.bracket_2_doorcount, ConVar.Decay.bracket_2_doorfraction),
		new UpkeepBracket(0, ConVar.Decay.bracket_3_doorfraction)
	};

	public List<ItemAmount> upkeepBuffer = new List<ItemAmount>();

	public GameObject assignDialog;

	[NonSerialized]
	public HashSet<ulong> authorizedPlayers = new HashSet<ulong>();

	public const Flags Flag_MaxAuths = Flags.Reserved5;

	public const Flags Flag_BlockAllFromBuilding = Flags.Reserved6;

	public List<ItemDefinition> allowedConstructionItems = new List<ItemDefinition>();

	public bool IsInvisibleAuth;

	public static Grid<BuildingPrivlidge> InvisibleAuthGrid = new Grid<BuildingPrivlidge>(32, 8096f);

	private static ListHashSet<ItemDefinition> allowedConstructionFast = null;

	private Action delayedUpdateCB;

	public override bool PreserveChildrenWhenReskinning => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BuildingPrivlidge.OnRpcMessage"))
		{
			if (rpc == 82205621 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - AddAuthorize"));
				}
				using (TimeWarning.New("AddAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(82205621u, "AddAuthorize", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							AddAuthorize(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in AddAuthorize");
					}
				}
				return true;
			}
			if (rpc == 253307592 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ClearList"));
				}
				using (TimeWarning.New("ClearList"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(253307592u, "ClearList", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ClearList(rpc3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in ClearList");
					}
				}
				return true;
			}
			if (rpc == 3617985969u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RemoveSelfAuthorize"));
				}
				using (TimeWarning.New("RemoveSelfAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3617985969u, "RemoveSelfAuthorize", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RemoveSelfAuthorize(rpc4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RemoveSelfAuthorize");
					}
				}
				return true;
			}
			if (rpc == 2051750736 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Rotate"));
				}
				using (TimeWarning.New("RPC_Rotate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2051750736u, "RPC_Rotate", this, player, 3f))
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
							RPC_Rotate(msg2);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RPC_Rotate");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnDeployableCorpseSpawned(BaseEntity corpse)
	{
		base.OnDeployableCorpseSpawned(corpse);
		BuildingPrivlidge componentInChildren = ((Component)corpse).GetComponentInChildren<BuildingPrivlidge>();
		if ((Object)(object)componentInChildren == (Object)null)
		{
			Debug.LogError((object)"Not able to transfer auth of TC to corpse: BuildingPrivlidge component not found in child");
			return;
		}
		componentInChildren.SetAuthListFrom(this);
		componentInChildren.AttachToBuilding(buildingID);
	}

	public override bool ShouldDropDeployableCorpse(HitInfo info)
	{
		if (!StorageContainer.dropCorpseOnDeath || !GameModeSoftcore.allow_tc_corpse_no_building)
		{
			BuildingManager.Building building = GetBuilding();
			if (building == null)
			{
				return false;
			}
			if (building.buildingBlocks == null || building.buildingBlocks.Count == 0)
			{
				return false;
			}
		}
		return base.ShouldDropDeployableCorpse(info);
	}

	private void OnDestroyShared_Softcore()
	{
		if (base.isServer)
		{
			raidStatusSet?.Remove(this);
		}
	}

	private void SetDeployedBy(BasePlayer player)
	{
		if (!base.isClient && BaseGameMode.GetActiveGameMode(serverside: true) is GameModeSoftcore)
		{
			timePlaced = GetNetworkTime();
		}
	}

	private void OnServerInit_Softcore()
	{
		if (raidStatusSet == null)
		{
			raidStatusSet = new HashSet<BuildingPrivlidge>();
		}
		raidStatusSet.Add(this);
		if (BaseGameMode.GetActiveGameMode(serverside: true) is GameModeSoftcore)
		{
			Invoke(UpdateRaidableFlag, 0.25f);
		}
	}

	public static void RefreshAllRaidStatus()
	{
		if (raidStatusSet == null)
		{
			return;
		}
		int num = 0;
		foreach (BuildingPrivlidge item in raidStatusSet)
		{
			item.CancelInvoke(item.UpdateRaidableFlag);
			item.Invoke(item.UpdateRaidableFlag, (float)(num / 20) * 0.2f);
			num++;
		}
	}

	private void UpdateRaidableFlag()
	{
		CancelInvoke(UpdateRaidableFlag);
		bool flag = Softcore.raidwindow_fresh_tc_seconds > 0f && timePlaced > 0f && Time.time - timePlaced <= Softcore.raidwindow_fresh_tc_seconds;
		bool flag2 = Softcore.raidwindow_enabled && (RaidWindow.IsWindowOpenNow() || flag);
		if (HasFlag(Flags.Reserved7) != flag2)
		{
			SetFlagLocal(Flags.Reserved7, flag2);
			SendNetworkUpdate_Flags();
		}
		if (flag)
		{
			Invoke(UpdateRaidableFlag, Softcore.raidwindow_fresh_tc_seconds - (Time.time - timePlaced) + 0.1f);
		}
	}

	public float CalculateUpkeepPeriodMinutes()
	{
		if (base.isServer)
		{
			return ConVar.Decay.upkeep_period_minutes;
		}
		return 0f;
	}

	public float CalculateUpkeepCostFraction(bool doors)
	{
		if (base.isServer)
		{
			if (!doors)
			{
				return CalculateBuildingTaxRate();
			}
			return CalculateDoorTaxRate();
		}
		return 0f;
	}

	public void CalculateUpkeepCostAmounts(List<ItemAmount> itemAmounts)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		BuildingManager.Building building = GetBuilding();
		if (building == null || !building.HasDecayEntities())
		{
			return;
		}
		float num = CalculateUpkeepCostFraction(doors: false);
		float num2 = CalculateUpkeepCostFraction(doors: true);
		Enumerator<DecayEntity> enumerator = building.decayEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				DecayEntity current = enumerator.Current;
				float multiplier = ((current is Door) ? num2 : num);
				current.CalculateUpkeepCostAmounts(itemAmounts, multiplier);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public float GetProtectedMinutes(bool force = false)
	{
		if (base.isServer)
		{
			if (!force && Time.realtimeSinceStartup < nextProtectedCalcTime)
			{
				return cachedProtectedMinutes;
			}
			nextProtectedCalcTime = Time.realtimeSinceStartup + 60f;
			List<ItemAmount> list = Pool.Get<List<ItemAmount>>();
			CalculateUpkeepCostAmounts(list);
			float num = CalculateUpkeepPeriodMinutes();
			float num2 = -1f;
			if (base.inventory != null)
			{
				PooledList<Item> val = Pool.Get<PooledList<Item>>();
				try
				{
					foreach (ItemAmount item in list)
					{
						((List<Item>)(object)val).Clear();
						base.inventory.FindItemsByItemID((List<Item>)(object)val, item.itemid);
						int num3 = ((IEnumerable<Item>)val).Sum((Item x) => x.amount);
						if (num3 > 0 && item.amount > 0f)
						{
							float num4 = (float)num3 / item.amount * num;
							if (num2 == -1f || num4 < num2)
							{
								num2 = num4;
							}
						}
						else
						{
							num2 = 0f;
						}
					}
					if (num2 == -1f)
					{
						num2 = 0f;
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			Pool.FreeUnmanaged<ItemAmount>(ref list);
			cachedProtectedMinutes = num2;
			Interface.CallHook("OnCupboardProtectionCalculated", this, cachedProtectedMinutes);
			return cachedProtectedMinutes;
		}
		return 0f;
	}

	public override void OnDied(HitInfo info)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (info != null && (Object)(object)info.InitiatorPlayer != (Object)null && !info.InitiatorPlayer.IsNpc && info.InitiatorPlayer.serverClan != null)
		{
			IReadOnlyList<ClanMember> members = info.InitiatorPlayer.serverClan.Members;
			bool flag = false;
			foreach (ClanMember item in members)
			{
				if (item.SteamId == base.OwnerID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				HandleKilledByClanMember(info.InitiatorPlayer);
			}
		}
		UnlinkDoorControllers();
		base.OnDied(info);
	}

	public override void Die(HitInfo info = null)
	{
		if (!IsDead())
		{
			if (ConVar.Decay.upkeep_grief_protection > 0f)
			{
				PurchaseAntiGriefTime(ConVar.Decay.upkeep_grief_protection * 60f);
			}
			base.Die(info);
		}
	}

	private async void HandleKilledByClanMember(BasePlayer player)
	{
		try
		{
			ClanValueResult<IClan> val = await ClanManager.ServerInstance.Backend.GetByMember(base.OwnerID);
			IClan val2 = (val.IsSuccess ? val.Value : null);
			if (val2 != null)
			{
				player.AddClanScore((ClanScoreEventType)4, 1, null, val2);
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}

	public override void DecayTick()
	{
		BuildingBlock nearbyBuildingBlock = GetNearbyBuildingBlock();
		if ((Object)(object)nearbyBuildingBlock != (Object)null)
		{
			BuildingManager.Building building = nearbyBuildingBlock.GetBuilding();
			if (building != null && building.ID != buildingID)
			{
				AttachToBuilding(building.ID);
			}
		}
		else
		{
			bool flag = true;
			BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(base.isServer);
			if ((Object)(object)activeGameMode != (Object)null && activeGameMode is GameModeSoftcore && GetParentEntity() is ContainerCorpse)
			{
				flag = false;
			}
			if (flag)
			{
				Kill(DestroyMode.Gib);
			}
		}
		if (EnsurePrimary())
		{
			base.DecayTick();
		}
	}

	public bool EnsurePrimary()
	{
		BuildingManager.Building building = GetBuilding();
		if (building != null)
		{
			BuildingPrivlidge dominatingBuildingPrivilege = building.GetDominatingBuildingPrivilege();
			if ((Object)(object)dominatingBuildingPrivilege != (Object)null && (Object)(object)dominatingBuildingPrivilege != (Object)(object)this)
			{
				Kill(DestroyMode.Gib);
				return false;
			}
		}
		return true;
	}

	public void MarkProtectedMinutesDirty(float delay = 0f)
	{
		nextProtectedCalcTime = Time.realtimeSinceStartup + delay;
	}

	private static float CalculateTaxRate(int entityCount, bool blocks)
	{
		if (entityCount == 0)
		{
			if (!blocks)
			{
				return ConVar.Decay.bracket_0_doorfraction;
			}
			return ConVar.Decay.bracket_0_costfraction;
		}
		int num = entityCount;
		float num2 = 0f;
		for (int i = 0; i < 4; i++)
		{
			float num3 = (blocks ? ConVar.Decay.bracket_0_costfraction : ConVar.Decay.bracket_0_doorfraction);
			int num4 = (blocks ? ConVar.Decay.bracket_0_blockcount : ConVar.Decay.bracket_0_doorcount);
			switch (i)
			{
			case 1:
				num3 = (blocks ? ConVar.Decay.bracket_1_costfraction : ConVar.Decay.bracket_1_doorfraction);
				num4 = (blocks ? ConVar.Decay.bracket_1_blockcount : ConVar.Decay.bracket_1_doorcount);
				break;
			case 2:
				num3 = (blocks ? ConVar.Decay.bracket_2_costfraction : ConVar.Decay.bracket_1_doorfraction);
				num4 = (blocks ? ConVar.Decay.bracket_2_blockcount : ConVar.Decay.bracket_1_doorcount);
				break;
			case 3:
				num3 = (blocks ? ConVar.Decay.bracket_3_costfraction : ConVar.Decay.bracket_1_doorfraction);
				num4 = int.MaxValue;
				break;
			}
			if (num > 0)
			{
				int num5 = Mathf.Min(num, num4);
				num -= num5;
				num2 += (float)num5 * num3;
			}
		}
		return num2 /= (float)entityCount;
	}

	private float CalculateDoorTaxRate()
	{
		if (!ConVar.Decay.use_door_upkeep_brackets)
		{
			return CalculateBuildingTaxRate();
		}
		BuildingManager.Building building = GetBuilding();
		if (building == null)
		{
			return ConVar.Decay.bracket_0_doorfraction;
		}
		if (!building.HasDecayEntities())
		{
			return ConVar.Decay.bracket_0_doorfraction;
		}
		return CalculateTaxRate(building.doors.Count, blocks: false);
	}

	public float CalculateBuildingTaxRate()
	{
		BuildingManager.Building building = GetBuilding();
		if (building == null)
		{
			return ConVar.Decay.bracket_0_costfraction;
		}
		if (!building.HasBuildingBlocks())
		{
			return ConVar.Decay.bracket_0_costfraction;
		}
		return CalculateTaxRate(building.buildingBlocks.Count, blocks: true);
	}

	public void ApplyUpkeepPayment()
	{
		List<Item> list = Pool.Get<List<Item>>();
		for (int i = 0; i < upkeepBuffer.Count; i++)
		{
			ItemAmount itemAmount = upkeepBuffer[i];
			int num = (int)itemAmount.amount;
			if (num < 1)
			{
				continue;
			}
			base.inventory.Take(list, itemAmount.itemid, num);
			Facepunch.Rust.Analytics.Azure.AddPendingItems(this, itemAmount.itemDef.shortname, num, "upkeep", consumed: true, perEntity: true);
			foreach (Item item in list)
			{
				if (IsDebugging())
				{
					Debug.Log((object)(((object)this).ToString() + ": Using " + item.amount + " of " + item.info.shortname));
				}
				item.UseItem(item.amount);
			}
			list.Clear();
			itemAmount.amount -= num;
			upkeepBuffer[i] = itemAmount;
		}
		Pool.Free<Item>(ref list, false);
	}

	public void QueueUpkeepPayment(List<ItemAmount> itemAmounts)
	{
		for (int i = 0; i < itemAmounts.Count; i++)
		{
			ItemAmount itemAmount = itemAmounts[i];
			bool flag = false;
			foreach (ItemAmount item in upkeepBuffer)
			{
				if ((Object)(object)item.itemDef == (Object)(object)itemAmount.itemDef)
				{
					item.amount += itemAmount.amount;
					if (IsDebugging())
					{
						Debug.Log((object)(((object)this).ToString() + ": Adding " + itemAmount.amount + " of " + itemAmount.itemDef.shortname + " to " + item.amount));
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (IsDebugging())
				{
					Debug.Log((object)(((object)this).ToString() + ": Adding " + itemAmount.amount + " of " + itemAmount.itemDef.shortname));
				}
				upkeepBuffer.Add(new ItemAmount(itemAmount.itemDef, itemAmount.amount));
			}
		}
	}

	public bool CanAffordUpkeepPayment(List<ItemAmount> itemAmounts)
	{
		for (int i = 0; i < itemAmounts.Count; i++)
		{
			ItemAmount itemAmount = itemAmounts[i];
			if ((float)base.inventory.GetAmount(itemAmount.itemid, onlyUsableAmounts: true) < itemAmount.amount)
			{
				if (IsDebugging())
				{
					Debug.Log((object)(((object)this).ToString() + ": Can't afford " + itemAmount.amount + " of " + itemAmount.itemDef.shortname));
				}
				return false;
			}
		}
		return true;
	}

	public float PurchaseUpkeepTime(DecayEntity entity, float deltaTime)
	{
		float num = CalculateUpkeepCostFraction(doors: false);
		float num2 = CalculateUpkeepCostFraction(doors: true);
		float num3 = CalculateUpkeepPeriodMinutes() * 60f;
		float multiplier = ((entity is Door) ? num2 : num) * deltaTime / num3;
		List<ItemAmount> itemAmounts = Pool.Get<List<ItemAmount>>();
		entity.CalculateUpkeepCostAmounts(itemAmounts, multiplier);
		bool num4 = CanAffordUpkeepPayment(itemAmounts);
		QueueUpkeepPayment(itemAmounts);
		Pool.FreeUnmanaged<ItemAmount>(ref itemAmounts);
		ApplyUpkeepPayment();
		if (!num4)
		{
			return 0f;
		}
		return deltaTime;
	}

	public void PurchaseUpkeepTime(float deltaTime)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		BuildingManager.Building building = GetBuilding();
		if (building == null || !building.HasDecayEntities())
		{
			return;
		}
		float num = Mathf.Min(GetProtectedMinutes(force: true) * 60f, deltaTime);
		if (!(num > 0f))
		{
			return;
		}
		Enumerator<DecayEntity> enumerator = building.decayEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				DecayEntity current = enumerator.Current;
				float protectedSeconds = current.GetProtectedSeconds();
				if (num > protectedSeconds)
				{
					float time = PurchaseUpkeepTime(current, num - protectedSeconds);
					current.AddUpkeepTime(time);
					if (IsDebugging())
					{
						Debug.Log((object)(((object)this).ToString() + " purchased upkeep time for " + ((object)current).ToString() + ": " + protectedSeconds + " + " + time + " = " + current.GetProtectedSeconds()));
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public void PurchaseAntiGriefTime(float deltaTime)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		BuildingManager.Building building = GetBuilding();
		if (building == null || !building.HasDecayEntities())
		{
			return;
		}
		Enumerator<DecayEntity> enumerator = building.decayEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				DecayEntity current = enumerator.Current;
				float protectedSeconds = current.GetProtectedSeconds();
				float num = Mathf.Max(0f, deltaTime - protectedSeconds);
				if (num > 0f)
				{
					float time = PurchaseUpkeepTime(current, num);
					current.AddUpkeepTime(time);
					if (IsDebugging())
					{
						Debug.Log((object)(((object)this).ToString() + " purchased upkeep time for " + ((object)current).ToString() + ": " + protectedSeconds + " + " + time + " = " + current.GetProtectedSeconds()));
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static string FormatUpkeepMinutes(float minutes)
	{
		int num = Mathf.FloorToInt(minutes / 60f);
		int num2 = Mathf.FloorToInt(minutes - (float)num * 60f);
		int num3 = Mathf.FloorToInt(minutes * 60f % 60f);
		if (num >= 72)
		{
			string text = Translate.Get("days", "days", false);
			int num4 = num / 24;
			if (num4 >= 30)
			{
				return "> 30 " + text;
			}
			return $"{num4:N0} {text}";
		}
		if (num >= 12)
		{
			return $"{num:N0} hrs";
		}
		if (num >= 1)
		{
			return $"{num:N0}h{num2:N0}m";
		}
		if (minutes >= 1f)
		{
			return $"{num2:N0}m{num3:N0}s";
		}
		return $"{minutes * 60f:N0}s";
	}

	public override void ResetState()
	{
		base.ResetState();
		authorizedPlayers.Clear();
	}

	public bool CanBuild(BasePlayer player)
	{
		if (HasFlag(Flags.Reserved6))
		{
			return false;
		}
		return IsAuthed(player);
	}

	public bool IsAuthed(BasePlayer player)
	{
		return IsAuthed(player.userID);
	}

	public bool IsAuthed(ulong userId)
	{
		return authorizedPlayers.Contains(userId);
	}

	public bool AnyAuthed()
	{
		return authorizedPlayers.Count > 0;
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (IsInvisibleAuth && InvisibleAuthGrid.Contains(this))
		{
			InvisibleAuthGrid.Remove(this);
		}
		OnDestroyShared_Softcore();
	}

	public override void ServerInit()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		if (IsInvisibleAuth && !InvisibleAuthGrid.Contains(this))
		{
			Vector3 position = ((Component)this).transform.position;
			InvisibleAuthGrid.Add(this, position.x, position.z);
		}
		OnServerInit_Softcore();
	}

	public void SetAuthListFrom(BuildingPrivlidge source)
	{
		authorizedPlayers = new HashSet<ulong>();
		foreach (ulong authorizedPlayer in source.authorizedPlayers)
		{
			authorizedPlayers.Add(authorizedPlayer);
		}
		UpdatePrivilegeReceivers();
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		using (TimeWarning.New("BuildPrivItemFilter"))
		{
			if (allowedConstructionFast == null)
			{
				allowedConstructionFast = new ListHashSet<ItemDefinition>();
				allowedConstructionFast.AddRange(allowedConstructionItems);
			}
			bool flag = allowedConstructionFast.Contains(item.info);
			if (!flag && targetSlot == -1)
			{
				int num = 0;
				foreach (Item item2 in base.inventory.itemList)
				{
					if (!allowedConstructionItems.Contains(item2.info) && ((Object)(object)item2.info != (Object)(object)item.info || item2.amount == item2.MaxStackable()))
					{
						num++;
					}
				}
				if (num >= 24)
				{
					return false;
				}
			}
			if (targetSlot >= 24 && targetSlot <= 28)
			{
				return flag;
			}
			return base.ItemFilter(item, targetSlot);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.buildingPrivilege = Pool.Get<BuildingPrivilege>();
		if (!info.forDisk)
		{
			float num = CalculateUpkeepPeriodMinutes();
			float protectedMinutes = GetProtectedMinutes();
			if ((double)ConVar.Decay.scale > 0.01)
			{
				info.msg.buildingPrivilege.upkeepPeriodMinutes = num / ConVar.Decay.scale;
				info.msg.buildingPrivilege.protectedMinutes = protectedMinutes / ConVar.Decay.scale;
			}
			else
			{
				info.msg.buildingPrivilege.upkeepPeriodMinutes = num;
				info.msg.buildingPrivilege.protectedMinutes = protectedMinutes;
			}
			info.msg.buildingPrivilege.costFraction = CalculateUpkeepCostFraction(doors: false);
			info.msg.buildingPrivilege.doorCostFraction = CalculateUpkeepCostFraction(doors: true);
			info.msg.buildingPrivilege.clientAuthed = IsAuthed(info.forConnection.userid);
			info.msg.buildingPrivilege.clientAnyAuthed = AnyAuthed();
		}
		if (!info.forDisk && !info.msg.buildingPrivilege.clientAuthed)
		{
			return;
		}
		info.msg.buildingPrivilege.users = Pool.Get<List<PlayerNameID>>();
		foreach (ulong authorizedPlayer in authorizedPlayers)
		{
			PlayerNameID val = Pool.Get<PlayerNameID>();
			val.userid = authorizedPlayer;
			info.msg.buildingPrivilege.users.Add(val);
		}
	}

	public override bool CanUseNetworkCache(Connection connection)
	{
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		authorizedPlayers.Clear();
		if (info.msg.buildingPrivilege == null)
		{
			return;
		}
		if (info.msg.buildingPrivilege.users != null)
		{
			foreach (PlayerNameID user in info.msg.buildingPrivilege.users)
			{
				authorizedPlayers.Add(user.userid);
			}
		}
		if (!info.fromDisk)
		{
			cachedProtectedMinutes = info.msg.buildingPrivilege.protectedMinutes;
		}
	}

	public void BuildingDirty()
	{
		if (base.isServer)
		{
			AddDelayedUpdate();
		}
	}

	public bool AtMaxAuthCapacity()
	{
		return HasFlag(Flags.Reserved5);
	}

	public void UpdateMaxAuthCapacity()
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (Object.op_Implicit((Object)(object)activeGameMode) && activeGameMode.limitTeamAuths)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved5, authorizedPlayers.Count >= activeGameMode.GetMaxRelationshipTeamSize());
			}
		}
	}

	protected override void OnInventoryDirty()
	{
		base.OnInventoryDirty();
		AddDelayedUpdate();
	}

	public override void OnItemAddedOrRemoved(Item item, bool bAdded)
	{
		base.OnItemAddedOrRemoved(item, bAdded);
		AddDelayedUpdate();
	}

	public void AddDelayedUpdate()
	{
		if (delayedUpdateCB == null)
		{
			delayedUpdateCB = DelayedUpdate;
		}
		if (IsInvoking(delayedUpdateCB))
		{
			CancelInvoke(delayedUpdateCB);
		}
		Invoke(delayedUpdateCB, 1f);
	}

	private void DelayedUpdate()
	{
		MarkProtectedMinutesDirty();
		SendNetworkUpdate();
	}

	public bool CanAdministrate(BasePlayer player)
	{
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if ((Object)(object)baseLock == (Object)null)
		{
			return true;
		}
		return baseLock.OnTryToOpen(player);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void AddAuthorize(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && CanAdministrate(rpc.player))
		{
			ulong num = rpc.read.UInt64();
			if (Interface.CallHook("IOnCupboardAuthorize", num, rpc.player, this) == null)
			{
				AddPlayer(rpc.player, num);
				SendNetworkUpdate();
			}
		}
	}

	public void AddPlayer(BasePlayer granter, ulong targetPlayerId)
	{
		if (!AtMaxAuthCapacity())
		{
			authorizedPlayers.Add(targetPlayerId);
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, granter, authorizedPlayers, "added", targetPlayerId);
			UpdateMaxAuthCapacity();
			UpdatePrivilegeReceivers();
		}
	}

	public void RemovePlayer(BasePlayer fromPly, ulong targetPlayerId)
	{
		if (authorizedPlayers.Remove(targetPlayerId))
		{
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, fromPly, authorizedPlayers, "removed", targetPlayerId);
			UpdateMaxAuthCapacity();
			UpdatePrivilegeReceivers();
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RemoveSelfAuthorize(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && CanAdministrate(rpc.player) && Interface.CallHook("OnCupboardDeauthorize", this, rpc.player) == null)
		{
			RemovePlayer(rpc.player, rpc.player.userID);
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void ClearList(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && CanAdministrate(rpc.player) && Interface.CallHook("OnCupboardClearList", this, rpc.player) == null)
		{
			authorizedPlayers.Clear();
			UpdateMaxAuthCapacity();
			UpdatePrivilegeReceivers();
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Rotate(RPCMessage msg)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (player.CanBuild() && Object.op_Implicit((Object)(object)player.GetHeldEntity()) && (Object)(object)((Component)player.GetHeldEntity()).GetComponent<Hammer>() != (Object)null && ((Object)(object)GetSlot(Slot.Lock) == (Object)null || !GetSlot(Slot.Lock).IsLocked()) && !HasAttachedStorageAdaptor() && !HasAttachedStorageMonitor())
		{
			((Component)this).transform.rotation = Quaternion.LookRotation(-((Component)this).transform.forward, ((Component)this).transform.up);
			SendNetworkUpdate();
			Deployable component = ((Component)this).GetComponent<Deployable>();
			if (component != null && component.placeEffect.isValid)
			{
				Effect.server.Run(component.placeEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
			}
		}
		BaseEntity slot = GetSlot(Slot.Lock);
		if ((Object)(object)slot != (Object)null)
		{
			slot.SendNetworkUpdate();
		}
	}

	public override int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		if (item != null && (Object)(object)item.info != (Object)null && allowedConstructionItems.Contains(item.info))
		{
			if ((Object)(object)player != (Object)null && player.IsInTutorial)
			{
				return 0;
			}
			for (int i = 24; i <= 27; i++)
			{
				if (base.inventory.GetSlot(i) == null)
				{
					return i;
				}
			}
		}
		return base.GetIdealSlot(player, container, item);
	}

	private void UnlinkDoorControllers()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		BuildingManager.Building building = GetBuilding();
		if (building == null)
		{
			return;
		}
		Enumerator<DecayEntity> enumerator = building.decayEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (!(enumerator.Current is Door door))
				{
					continue;
				}
				foreach (BaseEntity child in door.children)
				{
					if (child is CustomDoorManipulator customDoorManipulator)
					{
						customDoorManipulator.SetTargetDoor(null);
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private void UpdatePrivilegeReceivers()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		BuildingManager.Building building = GetBuilding();
		if (building == null)
		{
			return;
		}
		Enumerator<DecayEntity> enumerator = building.decayEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is IPrivilegeUpdateReceiver privilegeUpdateReceiver)
				{
					privilegeUpdateReceiver.OnPrivilegeUpdated(this, authorizedPlayers);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public override void Reskin_Preserve(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		base.Reskin_Preserve(ref preserveInfo);
		ref BuildingPrivilegePreserveInfo buildingPrivilegePreserve = ref preserveInfo.buildingPrivilegePreserve;
		buildingPrivilegePreserve.authedPlayers = Pool.Get<HashSet<ulong>>();
		foreach (ulong authorizedPlayer in authorizedPlayers)
		{
			buildingPrivilegePreserve.authedPlayers.Add(authorizedPlayer);
		}
	}

	public override void Reskin_Restore(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		base.Reskin_Restore(ref preserveInfo);
		ref BuildingPrivilegePreserveInfo buildingPrivilegePreserve = ref preserveInfo.buildingPrivilegePreserve;
		foreach (ulong authedPlayer in buildingPrivilegePreserve.authedPlayers)
		{
			authorizedPlayers.Add(authedPlayer);
		}
		Pool.FreeUnmanaged<ulong>(ref buildingPrivilegePreserve.authedPlayers);
	}

	public override bool HasSlot(Slot slot)
	{
		if (slot == Slot.Lock)
		{
			return true;
		}
		return base.HasSlot(slot);
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}
}
