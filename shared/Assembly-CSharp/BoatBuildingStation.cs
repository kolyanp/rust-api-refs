using System;
using System.Collections.Generic;
using System.Text;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Spatial;
using UnityEngine;
using UnityEngine.Assertions;

public class BoatBuildingStation : DecayEntity
{
	private enum BoatValidationStatus
	{
		Valid,
		Invalid_Multiple_Buildings,
		Invalid_No_Blocks,
		Invalid_No_Propulsion,
		Invalid_Missing_Item,
		Invalid_Too_Many_Blocks,
		Invalid_Too_Many_Deployables
	}

	public static readonly Phrase invalidTooManyDeployablesPhrase = new Phrase("boatbuilding.invalid.tooManyDeployables", "Deployable limit reached");

	public static readonly Phrase invalidIllegalPlacement = new Phrase("boatbuilding.invalid.illegalPlacement", "Illegal deployable placement.");

	[ReplicatedVar]
	public static int max_bbs = 1;

	private static Dictionary<ulong, List<BoatBuildingStation>> bbsPerPlayer = new Dictionary<ulong, List<BoatBuildingStation>>();

	public static Phrase bbsLimitPhrase = new Phrase("bbs_limit_update", "You are now at {0}/{1} Boat Building Stations");

	public static Phrase bbsLimitReachedPhrase = new Phrase("bbs_limit_reached", "You have reached your Boat Building Station limit!");

	private float lastInteractionTime;

	public const string ACHIEVEMENT_FINISH_BOAT_NAME = "BBS_FINISH_BOAT";

	[Help("When disabled, any spawned static BBS will destroy themselves on spawn")]
	[ServerVar]
	public static bool StaticStationsEnabled = true;

	[Help("When set above zero, enables a global shared cooldown for boat edit/finishing.")]
	[ServerVar]
	public static float GlobalEditFinishUseInterval = 0f;

	public static float NextGlobalEditFinishUseTime = 0f;

	[ServerVar]
	public static bool LogBoatBuildingEvents = false;

	[ServerVar]
	public static float AutoClosePlayerCheckInterval = 150f;

	[ServerVar]
	public static int AutoClosePlayerCheckTriggerCount = 2;

	private static Grid<BoatBuildingStation> serverStations = new Grid<BoatBuildingStation>(32, 8096f);

	private int autoClosePassCount;

	private ulong bbsOwnerID;

	private static StringBuilder logStringBuilder = new StringBuilder();

	public bool IsStatic;

	[ReplicatedVar]
	public static float EditFinishUseInterval = 5f;

	public GameObjectRef BoatPrefab;

	public GameObject BuildArea;

	public GameObject Netting;

	public List<ItemDefinition> RequiredItems;

	public List<ItemDefinition> PropulsionItems;

	public Animator Animator;

	public HashSet<ulong> authorizedPlayers = new HashSet<ulong>();

	public string boatLockCode;

	public TriggerPlayer AutoClosePlayerTrigger;

	public List<TriggerPlayer> StationNettingPlayerTriggers;

	public TriggerBoatBuildingArea BoatBuildingAreaTrigger;

	private const float GridQueryRadius = 20f;

	private SteeringWheel cachedSteeringWheel;

	public static Dictionary<ulong, List<BoatBuildingStation>> BBSPerPlayer => bbsPerPlayer;

	public bool KilledDuringWheelFinish { get; set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BoatBuildingStation.OnRpcMessage"))
		{
			if (rpc == 252213800 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ClearArea"));
				}
				using (TimeWarning.New("ClearArea"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(252213800u, "ClearArea", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(252213800u, "ClearArea", this, player, 3f))
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
							ClearArea(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ClearArea");
					}
				}
				return true;
			}
			if (rpc == 2844717662u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - EditBoat"));
				}
				using (TimeWarning.New("EditBoat"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2844717662u, "EditBoat", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2844717662u, "EditBoat", this, player, 3f))
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
							EditBoat(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in EditBoat");
					}
				}
				return true;
			}
			if (rpc == 3242354064u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - FinishBuilding"));
				}
				using (TimeWarning.New("FinishBuilding"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3242354064u, "FinishBuilding", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3242354064u, "FinishBuilding", this, player, 3f))
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
							FinishBuilding(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in FinishBuilding");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static Planner.CanBuildResult? CanBuildBBS(BasePlayer player, Construction construction)
	{
		GameObject obj = GameManager.server.FindPrefab(construction.prefabID);
		if (((obj != null) ? obj.GetComponent<BaseEntity>() : null) is BoatBuildingStation)
		{
			int num = 1;
			if (bbsPerPlayer.TryGetValue(player.userID, out var value))
			{
				num = value.Count + 1;
				if (value.Count >= max_bbs)
				{
					return new Planner.CanBuildResult
					{
						Result = false,
						Phrase = bbsLimitReachedPhrase
					};
				}
			}
			return new Planner.CanBuildResult
			{
				Result = true,
				Phrase = bbsLimitPhrase,
				Arguments = new string[2]
				{
					num.ToString(),
					max_bbs.ToString()
				}
			};
		}
		return null;
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (bbsPerPlayer.TryGetValue(bbsOwnerID, out var _))
		{
			bbsPerPlayer[bbsOwnerID].Remove(this);
		}
		serverStations.Remove(this);
	}

	public static int GetBBSCount(ulong userId)
	{
		if (userId == 0L)
		{
			return 0;
		}
		if (!bbsPerPlayer.TryGetValue(userId, out var value))
		{
			return 0;
		}
		return value.Count;
	}

	private void AddToBBSList(ulong id)
	{
		if (!bbsPerPlayer.ContainsKey(id))
		{
			bbsPerPlayer.Add(id, new List<BoatBuildingStation>());
		}
		if (!IsBBSInList(bbsPerPlayer[id], out var _))
		{
			bbsPerPlayer[id].Add(this);
		}
	}

	private bool IsBBSInList(List<BoatBuildingStation> bbss, out BoatBuildingStation thisBBS)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		thisBBS = null;
		if (bbss.Count == 0)
		{
			return false;
		}
		if ((Object)(object)thisBBS == (Object)null)
		{
			return false;
		}
		foreach (BoatBuildingStation item in bbss)
		{
			if (item.net.ID == net.ID)
			{
				result = true;
				thisBBS = item;
				break;
			}
		}
		return result;
	}

	public override void ServerInit()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isServer)
		{
			return;
		}
		if (IsStatic && !StaticStationsEnabled)
		{
			Kill();
			return;
		}
		base.ServerInit();
		if (IsStatic && !Application.isLoadingSave)
		{
			((Component)this).transform.position = Vector3Ex.WithY(((Component)this).transform.position, WaterLevel.GetWaterSurface(((Component)this).transform.position, waves: false, volumes: false) + 0.57f);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: true);
			}
			FinishBuilding();
		}
		Netting.gameObject.SetActive(false);
		if (!Application.isLoadingSave && !IsStatic)
		{
			LogBuildingEvent(((Component)this).transform.position, null, null, "Boat Building station deployed.");
			EnterEditMode();
		}
		else if (IsOn())
		{
			using (FlagsUpdateScope flagsUpdateScope2 = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope2.Set(Flags.On, b: false);
			}
			EnterEditMode();
		}
		SetLastInteractionTime();
		serverStations.Add(this, ((Component)this).transform.position.x, ((Component)this).transform.position.z);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (base.isServer)
		{
			if (IsInvoking(ClearCooldown))
			{
				CancelInvoke(ClearCooldown);
			}
			ClearCooldown();
			if (IsOn())
			{
				RefreshSteeringWheelCache();
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (base.isServer)
		{
			info.msg.boatBuildingStation = Pool.Get<BoatBuildingStation>();
			info.msg.boatBuildingStation.ownerId = bbsOwnerID;
		}
	}

	public override void OnPlaced(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			base.OwnerID = player.userID;
		}
		if (bbsPerPlayer.TryGetValue(player.userID, out var value) && value.Count >= max_bbs)
		{
			value[0].Kill(DestroyMode.Gib);
		}
		bbsOwnerID = player.userID;
		AddToBBSList(bbsOwnerID);
	}

	public static void StartGlobalEditFinishCoolDown()
	{
		NextGlobalEditFinishUseTime = Time.time + GlobalEditFinishUseInterval;
	}

	private void SetLastInteractionTime()
	{
		lastInteractionTime = Time.time;
	}

	public static void LogBuildingEvent(Vector3 pos, BasePlayer player, PlayerBoat boat, string message)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (LogBoatBuildingEvents)
		{
			logStringBuilder.Clear();
			logStringBuilder.Append(message);
			logStringBuilder.Append(" ");
			logStringBuilder.Append(pos);
			if ((Object)(object)player != (Object)null)
			{
				logStringBuilder.Append(". ");
				logStringBuilder.Append(player.displayName);
				logStringBuilder.Append(" - ");
				logStringBuilder.Append(player.userID.Get());
				logStringBuilder.Append(".");
			}
			if ((Object)(object)boat != (Object)null)
			{
				logStringBuilder.Append(". Boat alive time: ");
				logStringBuilder.Append(Time.time - boat.boatSpawnTime);
				logStringBuilder.Append("s");
			}
			Debug.Log((object)logStringBuilder.ToString());
		}
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void EditBoat(RPCMessage msg)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)msg.player == (Object)null))
		{
			SetLastInteractionTime();
			LogBuildingEvent(((Component)this).transform.position, msg.player, null, "Edit boat requested.");
			if (!CanEnterEditMode(msg.player, sendErrorToasts: true))
			{
				StartCooldown();
			}
			else
			{
				EnterEditMode();
			}
		}
	}

	public void EnterEditMode()
	{
		if (!IsOn())
		{
			SetLastInteractionTime();
			StartCooldown();
			ConvertPlayerBoatToConstruction();
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: true);
			}
			StartAutoCloseInvoke();
			UnStickExplosives();
		}
	}

	private void StartCooldown()
	{
		StartGlobalEditFinishCoolDown();
		if (EditFinishUseInterval <= 0f)
		{
			ClearCooldown();
			return;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: true);
		}
		if (IsInvoking(ClearCooldown))
		{
			CancelInvoke(ClearCooldown);
		}
		Invoke(ClearCooldown, EditFinishUseInterval);
	}

	private void ClearCooldown()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: false);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	public void FinishBuilding(RPCMessage msg)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)msg.player == (Object)null))
		{
			SetLastInteractionTime();
			LogBuildingEvent(((Component)this).transform.position, msg.player, null, "Finish building requested.");
			if (CanPlayerBuild(msg.player))
			{
				FinishBuilding(msg.player);
			}
		}
	}

	public bool FinishBuilding(BasePlayer player = null)
	{
		if (!PlayerBoat.FinishEditingEnabled)
		{
			return false;
		}
		if (IsOnEditFinishCooldown())
		{
			return false;
		}
		cachedSteeringWheel = null;
		SetLastInteractionTime();
		StartCooldown();
		if (!IsOn())
		{
			return true;
		}
		List<BoatBuildingBlock> entitiesInBuildArea = GetEntitiesInBuildArea<BoatBuildingBlock>(BuildArea, 134217728, server: true);
		List<BaseEntity> deployedEntities = GetDeployedEntities();
		bool flag = ValidBoat(entitiesInBuildArea, deployedEntities) == BoatValidationStatus.Valid;
		bool flag2 = flag || (entitiesInBuildArea.Count == 0 && deployedEntities.Count == 0);
		if (flag2)
		{
			Netting.gameObject.SetActive(false);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: false);
			}
			WakeUpDroppedItemsInBuildArea();
			if (flag)
			{
				BaseEntity baseEntity = CreateBoat(entitiesInBuildArea, deployedEntities, ((Component)this).gameObject);
				if ((Object)(object)player != (Object)null && (Object)(object)baseEntity != (Object)null)
				{
					if (Rust.GameInfo.HasAchievements)
					{
						player.GiveAchievement("BBS_FINISH_BOAT");
					}
					baseEntity.OwnerID = player.userID;
					Facepunch.Rust.Analytics.Azure.OnPlayerBoatFinish(player, entitiesInBuildArea.Count, deployedEntities.Count);
				}
			}
		}
		Pool.FreeUnmanaged<BoatBuildingBlock>(ref entitiesInBuildArea);
		Pool.FreeUnmanaged<BaseEntity>(ref deployedEntities);
		if (flag2)
		{
			StopAutoCloseInvoke();
		}
		UnStickExplosives();
		return flag2;
	}

	private void UnStickExplosives()
	{
		for (int num = children.Count - 1; num >= 0; num--)
		{
			BaseEntity baseEntity = children[num];
			if (!((Object)(object)baseEntity == (Object)null) && baseEntity is TimedExplosive timedExplosive)
			{
				timedExplosive.UnStick();
			}
		}
	}

	private void WakeUpDroppedItemsInBuildArea()
	{
		List<DroppedItem> entitiesInBuildArea = GetEntitiesInBuildArea<DroppedItem>(BuildArea, -2146959360, server: true);
		foreach (DroppedItem item in entitiesInBuildArea)
		{
			item.OnPhysicsNeighbourChanged();
		}
		Pool.FreeUnmanaged<DroppedItem>(ref entitiesInBuildArea);
	}

	private void ConvertPlayerBoatToConstruction()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		List<PlayerBoat> playerBoats = GetPlayerBoats();
		if (playerBoats.Count != 1)
		{
			Pool.FreeUnmanaged<PlayerBoat>(ref playerBoats);
			return;
		}
		PlayerBoat playerBoat = playerBoats[0];
		cachedSteeringWheel = playerBoat.GetSteeringWheel();
		playerBoat.PowerDown(force: true);
		playerBoat.rigidBody.isKinematic = true;
		Object.Destroy((Object)(object)playerBoat.rigidBody);
		bool autoSyncTransforms = Physics.autoSyncTransforms;
		try
		{
			Physics.autoSyncTransforms = false;
			((Component)playerBoat).transform.position = Vector3Ex.WithY(((Component)playerBoat).transform.position, 0f);
			((Component)playerBoat).transform.localEulerAngles = new Vector3(0f, ((Component)playerBoat).transform.localEulerAngles.y, 0f);
			playerBoat.SendNetworkUpdate();
			playerBoat.DistributeHealthAcrossBlocks();
			playerBoat.SwitchToConstruction();
			playerBoat.KilledForEditMode = true;
			playerBoat.OrphanChildEntities();
		}
		finally
		{
			if (autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}
			Physics.autoSyncTransforms = autoSyncTransforms;
		}
		Interface.CallHook("OnPlayerBoatEditStarted", playerBoat, this);
		playerBoat.Kill();
		Pool.FreeUnmanaged<PlayerBoat>(ref playerBoats);
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.IsVisible(3f)]
	public void ClearArea(RPCMessage msg)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)msg.player == (Object)null))
		{
			LogBuildingEvent(((Component)this).transform.position, msg.player, null, "Clear Area requested at BoatBuildingStation.");
			SetLastInteractionTime();
			if (!CanClearArea(msg.player))
			{
				StartCooldown();
			}
			else
			{
				ClearArea();
			}
		}
	}

	private void ClearArea()
	{
		List<BoatBuildingBlock> entitiesInBuildArea = GetEntitiesInBuildArea<BoatBuildingBlock>(BuildArea, 134217728, server: true);
		List<BaseEntity> deployedEntities = GetDeployedEntities();
		for (int num = deployedEntities.Count - 1; num >= 0; num--)
		{
			BaseEntity baseEntity = deployedEntities[num];
			if (!((Object)(object)baseEntity == (Object)null) && !PlayerBoat.IsChildOfFinishedPlayerBoat(baseEntity))
			{
				baseEntity.Kill();
			}
		}
		for (int num2 = entitiesInBuildArea.Count - 1; num2 >= 0; num2--)
		{
			BoatBuildingBlock boatBuildingBlock = entitiesInBuildArea[num2];
			if (!((Object)(object)boatBuildingBlock == (Object)null) && !PlayerBoat.IsChildOfFinishedPlayerBoat(boatBuildingBlock))
			{
				boatBuildingBlock.Kill();
			}
		}
		Pool.FreeUnmanaged<BoatBuildingBlock>(ref entitiesInBuildArea);
		Pool.FreeUnmanaged<BaseEntity>(ref deployedEntities);
		StartCooldown();
	}

	private BaseEntity CreateBoat(List<BoatBuildingBlock> blocks, List<BaseEntity> ents, GameObject stationGameObject)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (!BoatPrefab.isValid)
		{
			return null;
		}
		Quaternion val = CalculateBoatForward(blocks, ents, stationGameObject);
		GetBoatBlocksOBBExtents(blocks, val * Vector3.forward, out var center, out var halfExtents, out var _);
		PlayerBoat obj = GameManager.server.CreateEntity(BoatPrefab.resourcePath, Vector3Ex.WithY(center, 0f), val) as PlayerBoat;
		obj.Spawn();
		obj.OnCreatedAtBBS(this);
		obj.Init(blocks, ents, halfExtents, loading: false);
		return obj;
	}

	private Quaternion CalculateBoatForward(List<BoatBuildingBlock> blocks, List<BaseEntity> ents, GameObject station)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		foreach (BaseEntity ent in ents)
		{
			if (ent is SteeringWheel)
			{
				return ((Component)ent).gameObject.transform.rotation;
			}
		}
		return station.transform.rotation * Quaternion.AngleAxis(180f, Vector3.up);
	}

	private void StartAutoCloseInvoke()
	{
		if (IsInvoking(CheckAutoClose))
		{
			CancelInvoke(CheckAutoClose);
		}
		autoClosePassCount = 0;
		InvokeRandomized(CheckAutoClose, AutoClosePlayerCheckInterval, AutoClosePlayerCheckInterval, AutoClosePlayerCheckInterval * 0.1f);
	}

	private void StopAutoCloseInvoke()
	{
		if (IsInvoking(CheckAutoClose))
		{
			CancelInvoke(CheckAutoClose);
		}
	}

	private void CheckAutoClose()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (AutoClosePlayerTrigger.contents == null || AutoClosePlayerTrigger.contents.Count == 0)
		{
			if (GetEntitiesInBuildArea<BaseEntity>(BuildArea, -1, base.isServer).Count > 1)
			{
				autoClosePassCount = 0;
				return;
			}
			autoClosePassCount++;
			if (autoClosePassCount >= AutoClosePlayerCheckTriggerCount)
			{
				LogBuildingEvent(((Component)this).transform.position, null, null, "Finish building requested by CheckAutoClose.");
				if (FinishBuilding())
				{
					StopAutoCloseInvoke();
				}
			}
		}
		else
		{
			autoClosePassCount = 0;
		}
	}

	public override void OnDied(HitInfo info)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		LogBuildingEvent(((Component)this).transform.position, null, null, "BoatBuildingStation.OnDied rquesting FinishBuilding");
		FinishBuilding();
		KillAllBoatBuildingEntities();
		base.OnDied(info);
	}

	public override void OnKilled()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!KilledDuringWheelFinish)
		{
			LogBuildingEvent(((Component)this).transform.position, null, null, "BoatBuildingStation.OnKilled rquesting FinishBuilding");
			FinishBuilding();
			KillAllBoatBuildingEntities();
		}
		base.OnKilled();
	}

	private void KillAllBoatBuildingEntities()
	{
		List<BoatBuildingBlock> entitiesInBuildArea = GetEntitiesInBuildArea<BoatBuildingBlock>(BuildArea, 134217728, base.isServer);
		List<BaseEntity> deployedEntities = GetDeployedEntities();
		for (int num = entitiesInBuildArea.Count - 1; num >= 0; num--)
		{
			BoatBuildingBlock boatBuildingBlock = entitiesInBuildArea[num];
			if (!((Object)(object)boatBuildingBlock == (Object)null) && !boatBuildingBlock.HasParent())
			{
				boatBuildingBlock.DieInstantly();
			}
		}
		for (int num2 = deployedEntities.Count - 1; num2 >= 0; num2--)
		{
			BaseEntity baseEntity = deployedEntities[num2];
			if (!((Object)(object)baseEntity == (Object)null) && !baseEntity.HasParent())
			{
				baseEntity.Kill();
			}
		}
		Pool.FreeUnmanaged<BoatBuildingBlock>(ref entitiesInBuildArea);
		Pool.FreeUnmanaged<BaseEntity>(ref deployedEntities);
	}

	[ServerVar]
	public static void print_stats(ConsoleSystem.Arg arg)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("BOAT BUILDING STATIONS:");
		BoatBuildingStation[] array = Util.FindAll<BoatBuildingStation>();
		foreach (BoatBuildingStation boatBuildingStation in array)
		{
			if (!((Object)(object)boatBuildingStation == (Object)null))
			{
				stringBuilder.AppendLine("Last Interaction: " + $"{Time.time - boatBuildingStation.lastInteractionTime}s. Pos: " + $"{((Component)boatBuildingStation).transform.position}");
			}
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	public static BoatBuildingStation GetStationOverlappingPosition(Vector3 position, bool isServer)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		if (isServer)
		{
			return GetFromGrid(serverStations);
		}
		return null;
		BoatBuildingStation GetFromGrid(Grid<BoatBuildingStation> grid)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			PooledList<BoatBuildingStation> val = Pool.Get<PooledList<BoatBuildingStation>>();
			try
			{
				grid.Query(position.x, position.z, 20f, (List<BoatBuildingStation>)(object)val);
				foreach (BoatBuildingStation item in (List<BoatBuildingStation>)(object)val)
				{
					if (item.IsInsideBuildArea(position))
					{
						return item;
					}
				}
				return null;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public static BoatBuildingStation GetStationIntersectingOBB(OBB obb, bool isServer)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		if (isServer)
		{
			return GetFromGrid(serverStations);
		}
		return null;
		BoatBuildingStation GetFromGrid(Grid<BoatBuildingStation> grid)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position = obb.position;
			float num = Mathf.Max(new float[3]
			{
				obb.extents.x,
				obb.extents.y,
				obb.extents.z
			});
			PooledList<BoatBuildingStation> val = Pool.Get<PooledList<BoatBuildingStation>>();
			try
			{
				grid.Query(position.x, position.z, num, (List<BoatBuildingStation>)(object)val);
				foreach (BoatBuildingStation item in (List<BoatBuildingStation>)(object)val)
				{
					if (item.IntersectsBuildArea(obb))
					{
						return item;
					}
				}
				return null;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public static BoatBuildingStation GetForPosition(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<TriggerBoatBuildingArea> list = Pool.Get<List<TriggerBoatBuildingArea>>();
		Vis.Components<TriggerBoatBuildingArea>(position, 4f, list, 262144, (QueryTriggerInteraction)2);
		foreach (TriggerBoatBuildingArea item in list)
		{
			BoatBuildingStation boatBuildingStation = GameObjectEx.ToBaseEntity(((Component)item).gameObject) as BoatBuildingStation;
			if (!((Object)(object)boatBuildingStation == (Object)null))
			{
				Pool.FreeUnmanaged<TriggerBoatBuildingArea>(ref list);
				return boatBuildingStation;
			}
		}
		Pool.FreeUnmanaged<TriggerBoatBuildingArea>(ref list);
		return null;
	}

	public static BoatBuildingStation GetForPlayer(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return GetForPosition(((Component)player).transform.position);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && info.msg.boatBuildingStation != null)
		{
			bbsOwnerID = info.msg.boatBuildingStation.ownerId;
			AddToBBSList(bbsOwnerID);
		}
	}

	public bool HasPlayerInsideNettingBlockerTrigger()
	{
		foreach (TriggerPlayer stationNettingPlayerTrigger in StationNettingPlayerTriggers)
		{
			if (!((Object)(object)stationNettingPlayerTrigger == (Object)null) && stationNettingPlayerTrigger.contents != null && stationNettingPlayerTrigger.contents.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanPlayerBuild(BasePlayer player)
	{
		SteeringWheel steeringWheel = GetSteeringWheel();
		if ((Object)(object)steeringWheel == (Object)null)
		{
			return true;
		}
		return steeringWheel.IsAuthed(player);
	}

	public bool CanPlayerDemolish(BasePlayer player)
	{
		SteeringWheel steeringWheel = GetSteeringWheel();
		if ((Object)(object)steeringWheel == (Object)null)
		{
			return false;
		}
		return steeringWheel.IsAuthed(player);
	}

	public SteeringWheel GetSteeringWheel(bool cached = true)
	{
		if (!cached)
		{
			RefreshSteeringWheelCache();
		}
		return cachedSteeringWheel;
	}

	public void RefreshSteeringWheelCache()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		cachedSteeringWheel = null;
		List<SteeringWheel> list = Pool.Get<List<SteeringWheel>>();
		Vis.Entities(GetBuildAreaOBB(BuildArea), list, 256, (QueryTriggerInteraction)2);
		foreach (SteeringWheel item in list)
		{
			if (!((Object)(object)item == (Object)null))
			{
				cachedSteeringWheel = item;
				break;
			}
		}
		Pool.FreeUnmanaged<SteeringWheel>(ref list);
	}

	public void OnSteeringWheelPlaced(SteeringWheel wheel)
	{
		cachedSteeringWheel = wheel;
	}

	public void OnSteeringWheelRemoved(SteeringWheel wheel)
	{
		if ((Object)(object)cachedSteeringWheel == (Object)(object)wheel)
		{
			cachedSteeringWheel = null;
		}
	}

	public bool IsInsideBuildArea(Vector3 pos)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)BuildArea == (Object)null || (Object)(object)BuildArea.transform == (Object)null)
		{
			return false;
		}
		OBB buildAreaOBB = GetBuildAreaOBB(BuildArea);
		return ((OBB)(ref buildAreaOBB)).Contains(pos);
	}

	public bool IntersectsBuildArea(OBB obb)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		OBB buildAreaOBB = GetBuildAreaOBB(BuildArea);
		return ((OBB)(ref buildAreaOBB)).Intersects(obb);
	}

	public bool IsOnEditFinishCooldown()
	{
		if (!IsBusy())
		{
			return IsOnGlobalEditFinishCoolDown();
		}
		return true;
	}

	public static bool IsOnGlobalEditFinishCoolDown()
	{
		if (GlobalEditFinishUseInterval <= 0f)
		{
			return false;
		}
		return Time.time < NextGlobalEditFinishUseTime;
	}

	public bool CanEnterEditMode(BasePlayer player, bool sendErrorToasts)
	{
		if (!PlayerBoat.EditEnabled)
		{
			return false;
		}
		if (IsOnEditFinishCooldown())
		{
			return false;
		}
		if (HasPlayerInsideNettingBlockerTrigger())
		{
			return false;
		}
		PlayerBoat playerBoat = null;
		List<BaseVehicle> allVehicles = GetAllVehicles();
		int num = 0;
		int count = allVehicles.Count;
		foreach (BaseVehicle item in allVehicles)
		{
			if (item is PlayerBoat playerBoat2)
			{
				playerBoat = playerBoat2;
				num++;
			}
		}
		Pool.FreeUnmanaged<BaseVehicle>(ref allVehicles);
		if (num > 1 || num != count)
		{
			return false;
		}
		if ((Object)(object)playerBoat != (Object)null && (!playerBoat.CanStartEditing(player, sendErrorToasts) || !IsBoatFullyContained(playerBoat)))
		{
			return false;
		}
		return true;
	}

	private bool IsBoatFullyContained(PlayerBoat boat)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)boat == (Object)null)
		{
			return false;
		}
		if (boat.children == null || boat.children.Count == 0)
		{
			return false;
		}
		OBB buildAreaOBB = GetBuildAreaOBB(BuildArea);
		foreach (BoatBuildingBlock item in boat.BoatBuildingBlocks.Cached)
		{
			if (!((Object)(object)item == (Object)null) && item.Floor && !item.IsFullyInsideOBB(buildAreaOBB))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsSingularBuilding(List<BoatBuildingBlock> blocks)
	{
		BuildingManager.Building building = null;
		foreach (BoatBuildingBlock block in blocks)
		{
			BuildingManager.Building building2 = block.GetBuilding();
			if (building == null)
			{
				building = building2;
			}
			if (building2 == null || building2 != building)
			{
				return false;
			}
			building = building2;
		}
		return true;
	}

	private bool CanClearArea(BasePlayer player = null)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (IsOnEditFinishCooldown())
		{
			return false;
		}
		if (!CanPlayerBuild(player))
		{
			return false;
		}
		if (HasPlayerInBuildArea())
		{
			return false;
		}
		return true;
	}

	private BoatValidationStatus ValidBoat(List<BoatBuildingBlock> blocks, List<BaseEntity> deployables)
	{
		BoatValidationStatus boatValidationStatus = ValidateBoatBlocks(blocks);
		if (boatValidationStatus != BoatValidationStatus.Valid)
		{
			return boatValidationStatus;
		}
		BoatValidationStatus boatValidationStatus2 = ValidateBoatDeployables(deployables);
		if (boatValidationStatus2 != BoatValidationStatus.Valid)
		{
			return boatValidationStatus2;
		}
		BoatValidationStatus boatValidationStatus3 = HasRequiredItems(blocks, deployables);
		if (boatValidationStatus3 != BoatValidationStatus.Valid)
		{
			return boatValidationStatus3;
		}
		BoatValidationStatus boatValidationStatus4 = HasPropulsion(deployables);
		if (boatValidationStatus4 != BoatValidationStatus.Valid)
		{
			return boatValidationStatus4;
		}
		return BoatValidationStatus.Valid;
	}

	private BoatValidationStatus ValidateBoatBlocks(List<BoatBuildingBlock> blocks)
	{
		if (blocks.Count <= 0)
		{
			return BoatValidationStatus.Invalid_No_Blocks;
		}
		if (PlayerBoat.MaxBlockCount > 0 && blocks.Count > PlayerBoat.MaxBlockCount)
		{
			return BoatValidationStatus.Invalid_Too_Many_Blocks;
		}
		if (!IsSingularBuilding(blocks))
		{
			return BoatValidationStatus.Invalid_Multiple_Buildings;
		}
		return BoatValidationStatus.Valid;
	}

	private BoatValidationStatus ValidateBoatDeployables(List<BaseEntity> deployables)
	{
		if (PlayerBoat.MaxDeployableCount > 0 && deployables.Count > PlayerBoat.MaxDeployableCount)
		{
			return BoatValidationStatus.Invalid_Too_Many_Deployables;
		}
		return BoatValidationStatus.Valid;
	}

	private BoatValidationStatus HasRequiredItems(List<BoatBuildingBlock> blocks, List<BaseEntity> ents)
	{
		foreach (ItemDefinition requiredItem in RequiredItems)
		{
			if (!HasRequiredItem(requiredItem, ents))
			{
				return BoatValidationStatus.Invalid_Missing_Item;
			}
		}
		return BoatValidationStatus.Valid;
	}

	private BoatValidationStatus HasPropulsion(List<BaseEntity> ents)
	{
		foreach (ItemDefinition propulsionItem in PropulsionItems)
		{
			if (HasRequiredItem(propulsionItem, ents))
			{
				return BoatValidationStatus.Valid;
			}
		}
		return BoatValidationStatus.Invalid_No_Propulsion;
	}

	public bool HasRequiredItem(ItemDefinition item, List<BaseEntity> ents)
	{
		GameObjectRef gameObjectRef = ((Component)item).GetComponent<ItemModDeployable>()?.entityPrefab;
		if (gameObjectRef == null)
		{
			return false;
		}
		uint num = gameObjectRef.GetEntity().prefabID;
		foreach (BaseEntity ent in ents)
		{
			if (ent.prefabID == num)
			{
				return true;
			}
		}
		return false;
	}

	public List<BaseEntity> GetDeployedEntities()
	{
		List<BaseEntity> entitiesInBuildArea = GetEntitiesInBuildArea<BaseEntity>(BuildArea, 2097408, base.isServer);
		if (entitiesInBuildArea.Count > 0)
		{
			for (int num = entitiesInBuildArea.Count - 1; num >= 0; num--)
			{
				BaseEntity baseEntity = entitiesInBuildArea[num];
				if ((Object)(object)baseEntity == (Object)null || baseEntity is BoatBuildingStation || baseEntity is BuildingBlock || (Object)(object)baseEntity.GetParentEntity() != (Object)null)
				{
					entitiesInBuildArea.RemoveAt(num);
				}
			}
		}
		return entitiesInBuildArea;
	}

	public bool HasPlayerInBuildArea()
	{
		if (BoatBuildingAreaTrigger.contents == null)
		{
			return false;
		}
		if (BoatBuildingAreaTrigger.contents.Count == 0)
		{
			return false;
		}
		foreach (GameObject content in BoatBuildingAreaTrigger.contents)
		{
			if (GameObjectEx.ToBaseEntity(content) is BasePlayer { IsNpc: false })
			{
				return true;
			}
		}
		return false;
	}

	public int GetPlayerBoatCount()
	{
		List<PlayerBoat> playerBoats = GetPlayerBoats();
		int count = playerBoats.Count;
		Pool.FreeUnmanaged<PlayerBoat>(ref playerBoats);
		return count;
	}

	private List<PlayerBoat> GetPlayerBoats()
	{
		return GetEntitiesInBuildArea<PlayerBoat>(BuildArea, 134217728, base.isServer);
	}

	private List<BaseVehicle> GetAllVehicles()
	{
		return GetEntitiesInBuildArea<BaseVehicle>(BuildArea, -1, base.isServer);
	}

	private List<BaseVehicle> GetNonPlayerBoatVehicles()
	{
		List<BaseVehicle> entitiesInBuildArea = GetEntitiesInBuildArea<BaseVehicle>(BuildArea, 134225920, base.isServer);
		for (int num = entitiesInBuildArea.Count - 1; num >= 0; num--)
		{
			BaseVehicle baseVehicle = entitiesInBuildArea[num];
			if ((Object)(object)baseVehicle == (Object)null || baseVehicle is PlayerBoat)
			{
				entitiesInBuildArea.RemoveAt(num);
			}
		}
		return entitiesInBuildArea;
	}

	public static List<T> GetEntitiesInBuildArea<T>(GameObject buildArea, int layerMask, bool server) where T : BaseEntity
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		List<T> list = Pool.Get<List<T>>();
		Vis.Entities(GetBuildAreaOBB(buildArea), list, layerMask, (QueryTriggerInteraction)2);
		if (list.Count > 0)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				T val = list[num];
				if ((Object)(object)val == (Object)null || val.isServer != server)
				{
					list.RemoveAt(num);
				}
			}
		}
		return list;
	}

	public static OBB GetBuildAreaOBB(GameObject buildArea)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = buildArea.transform.position;
		Vector3 lossyScale = buildArea.transform.lossyScale;
		Quaternion rotation = buildArea.transform.rotation;
		return new OBB(position, lossyScale, rotation);
	}

	public static void GetBoatBlocksOBBExtents(List<BoatBuildingBlock> blocks, Vector3 forward, out Vector3 center, out Vector3 halfExtents, out Quaternion rot)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3> list = Pool.Get<List<Vector3>>();
		foreach (BoatBuildingBlock block in blocks)
		{
			if (!((Object)(object)block == (Object)null) && !block.isClient)
			{
				OBB val = block.WorldSpaceBounds();
				list.Add(((OBB)(ref val)).GetPoint(-1f, -1f, -1f));
				list.Add(((OBB)(ref val)).GetPoint(-1f, -1f, 1f));
				list.Add(((OBB)(ref val)).GetPoint(1f, 1f, -1f));
				list.Add(((OBB)(ref val)).GetPoint(1f, 1f, 1f));
			}
		}
		GetOBBExtents(list, forward, out center, out halfExtents, out rot);
		Pool.FreeUnmanaged<Vector3>(ref list);
	}

	private static void GetOBBExtents(List<Vector3> points, Vector3 forward, out Vector3 center, out Vector3 halfExtents, out Quaternion rotation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		forward = ((Vector3)(ref forward)).normalized;
		Vector3 val = Vector3.Cross(Vector3.up, forward);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 val2 = Vector3.Cross(forward, normalized);
		rotation = Quaternion.LookRotation(forward, val2);
		Matrix4x4 val3 = Matrix4x4.Rotate(rotation);
		Matrix4x4 inverse = ((Matrix4x4)(ref val3)).inverse;
		Vector3 val4 = default(Vector3);
		((Vector3)(ref val4))._002Ector(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 val5 = default(Vector3);
		((Vector3)(ref val5))._002Ector(float.MinValue, float.MinValue, float.MinValue);
		foreach (Vector3 point in points)
		{
			Vector3 val6 = ((Matrix4x4)(ref inverse)).MultiplyPoint3x4(point);
			val4 = Vector3.Min(val4, val6);
			val5 = Vector3.Max(val5, val6);
		}
		Vector3 val7 = (val4 + val5) * 0.5f;
		halfExtents = (val5 - val4) * 0.5f;
		center = rotation * val7;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	public void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.magenta;
		Matrix4x4 matrix = Matrix4x4.TRS(BuildArea.transform.position, BuildArea.transform.rotation, BuildArea.transform.lossyScale);
		Matrix4x4 matrix2 = Gizmos.matrix;
		Gizmos.matrix = matrix;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		Gizmos.matrix = matrix2;
	}
}
