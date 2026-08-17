using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust.Modular;
using UnityEngine;
using UnityEngine.Assertions;

public class ModularCarGarage : ContainerIOEntity
{
	[Serializable]
	public class ChassisBuildOption
	{
		public GameObjectRef prefab;

		public ItemDefinition itemDef;
	}

	public enum OccupantLock
	{
		CannotHaveLock,
		NoLock,
		HasLock
	}

	protected enum VehicleLiftState
	{
		Down,
		Up
	}

	public ModularCar lockedOccupant;

	public readonly HashSet<BasePlayer> lootingPlayers = new HashSet<BasePlayer>();

	public MagnetSnap magnetSnap;

	[Header("ModularCarGarage")]
	[SerializeField]
	public Transform vehicleLift;

	[SerializeField]
	public Animation vehicleLiftAnim;

	[SerializeField]
	private string animName = "LiftUp";

	[SerializeField]
	public VehicleLiftOccupantTrigger occupantTrigger;

	[SerializeField]
	public float liftMoveTime = 1f;

	[SerializeField]
	public EmissionToggle poweredLight;

	[SerializeField]
	public EmissionToggle inUseLight;

	[SerializeField]
	public Transform vehicleLiftPos;

	[SerializeField]
	[Range(0f, 1f)]
	public float recycleEfficiency = 0.5f;

	[SerializeField]
	public Transform recycleDropPos;

	[SerializeField]
	public bool needsElectricity;

	[SerializeField]
	private SoundDefinition liftStartSoundDef;

	[SerializeField]
	private SoundDefinition liftStopSoundDef;

	[SerializeField]
	private SoundDefinition liftStopDownSoundDef;

	[SerializeField]
	private SoundDefinition liftLoopSoundDef;

	[SerializeField]
	private GameObjectRef addRemoveLockEffect;

	[SerializeField]
	private GameObjectRef changeLockCodeEffect;

	[SerializeField]
	private GameObjectRef repairEffect;

	[SerializeField]
	private TriggerBase playerTrigger;

	public ChassisBuildOption[] chassisBuildOptions;

	public ItemAmount lockResourceCost;

	protected VehicleLiftState vehicleLiftState;

	private Sound liftLoopSound;

	public Vector3 downPos;

	public const Flags Flag_DestroyingChassis = Flags.Reserved6;

	public const float TimeToDestroyChassis = 10f;

	public const Flags Flag_EnteringKeycode = Flags.Reserved7;

	public const Flags Flag_PlayerObstructing = Flags.Reserved8;

	public ModularCar carOccupant
	{
		get
		{
			if (!((Object)(object)lockedOccupant != (Object)null))
			{
				return occupantTrigger.carOccupant;
			}
			return lockedOccupant;
		}
	}

	public bool HasOccupant
	{
		get
		{
			if ((Object)(object)carOccupant != (Object)null)
			{
				return carOccupant.IsFullySpawned();
			}
			return false;
		}
	}

	protected Transform GetVehicleLiftPos => vehicleLiftPos;

	public bool PlatformIsOccupied { get; set; }

	public bool HasEditableOccupant { get; set; }

	public bool HasDriveableOccupant { get; set; }

	public OccupantLock OccupantLockState { get; set; }

	protected virtual bool LiftIsUp => vehicleLiftState == VehicleLiftState.Up;

	protected virtual bool LiftIsMoving => vehicleLiftAnim.isPlaying;

	protected virtual bool LiftIsDown => vehicleLiftState == VehicleLiftState.Down;

	public bool IsDestroyingChassis => HasFlag(Flags.Reserved6);

	private bool IsEnteringKeycode => HasFlag(Flags.Reserved7);

	public bool PlayerObstructingLift => HasFlag(Flags.Reserved8);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ModularCarGarage.OnRpcMessage"))
		{
			if (rpc == 554177909 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_DeselectedLootItem"));
				}
				using (TimeWarning.New("RPC_DeselectedLootItem"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(554177909u, "RPC_DeselectedLootItem", this, player, 3f))
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
							RPC_DeselectedLootItem(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_DeselectedLootItem");
					}
				}
				return true;
			}
			if (rpc == 3683966290u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_DiedWithKeypadOpen"));
				}
				using (TimeWarning.New("RPC_DiedWithKeypadOpen"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3683966290u, "RPC_DiedWithKeypadOpen", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3683966290u, "RPC_DiedWithKeypadOpen", this, player, 3f))
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
							RPC_DiedWithKeypadOpen(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_DiedWithKeypadOpen");
					}
				}
				return true;
			}
			if (rpc == 3659332720u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenEditing"));
				}
				using (TimeWarning.New("RPC_OpenEditing"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3659332720u, "RPC_OpenEditing", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3659332720u, "RPC_OpenEditing", this, player, 3f))
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
							RPC_OpenEditing(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_OpenEditing");
					}
				}
				return true;
			}
			if (rpc == 1582295101 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RepairItem"));
				}
				using (TimeWarning.New("RPC_RepairItem"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1582295101u, "RPC_RepairItem", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1582295101u, "RPC_RepairItem", this, player, 3f))
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
							RPC_RepairItem(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RPC_RepairItem");
					}
				}
				return true;
			}
			if (rpc == 3710764312u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RequestAddLock"));
				}
				using (TimeWarning.New("RPC_RequestAddLock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3710764312u, "RPC_RequestAddLock", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3710764312u, "RPC_RequestAddLock", this, player, 3f))
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
							RPC_RequestAddLock(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in RPC_RequestAddLock");
					}
				}
				return true;
			}
			if (rpc == 3305106830u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RequestNewCode"));
				}
				using (TimeWarning.New("RPC_RequestNewCode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3305106830u, "RPC_RequestNewCode", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3305106830u, "RPC_RequestNewCode", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg7 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_RequestNewCode(msg7);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in RPC_RequestNewCode");
					}
				}
				return true;
			}
			if (rpc == 1046853419 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RequestRemoveLock"));
				}
				using (TimeWarning.New("RPC_RequestRemoveLock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1046853419u, "RPC_RequestRemoveLock", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1046853419u, "RPC_RequestRemoveLock", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg8 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_RequestRemoveLock(msg8);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
						player.Kick("RPC Error in RPC_RequestRemoveLock");
					}
				}
				return true;
			}
			if (rpc == 4033916654u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_SelectedLootItem"));
				}
				using (TimeWarning.New("RPC_SelectedLootItem"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4033916654u, "RPC_SelectedLootItem", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg9 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_SelectedLootItem(msg9);
						}
					}
					catch (Exception ex8)
					{
						Debug.LogException(ex8);
						player.Kick("RPC Error in RPC_SelectedLootItem");
					}
				}
				return true;
			}
			if (rpc == 2974124904u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_StartDestroyingChassis"));
				}
				using (TimeWarning.New("RPC_StartDestroyingChassis"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2974124904u, "RPC_StartDestroyingChassis", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2974124904u, "RPC_StartDestroyingChassis", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2974124904u, "RPC_StartDestroyingChassis", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg10 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_StartDestroyingChassis(msg10);
						}
					}
					catch (Exception ex9)
					{
						Debug.LogException(ex9);
						player.Kick("RPC Error in RPC_StartDestroyingChassis");
					}
				}
				return true;
			}
			if (rpc == 3872977075u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_StartKeycodeEntry"));
				}
				using (TimeWarning.New("RPC_StartKeycodeEntry"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3872977075u, "RPC_StartKeycodeEntry", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg11 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_StartKeycodeEntry(msg11);
						}
					}
					catch (Exception ex10)
					{
						Debug.LogException(ex10);
						player.Kick("RPC Error in RPC_StartKeycodeEntry");
					}
				}
				return true;
			}
			if (rpc == 3830531963u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_StopDestroyingChassis"));
				}
				using (TimeWarning.New("RPC_StopDestroyingChassis"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3830531963u, "RPC_StopDestroyingChassis", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3830531963u, "RPC_StopDestroyingChassis", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3830531963u, "RPC_StopDestroyingChassis", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg12 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_StopDestroyingChassis(msg12);
						}
					}
					catch (Exception ex11)
					{
						Debug.LogException(ex11);
						player.Kick("RPC Error in RPC_StopDestroyingChassis");
					}
				}
				return true;
			}
			if (rpc == 613695921 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_RequestFuelExtract"));
				}
				using (TimeWarning.New("Server_RequestFuelExtract"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(613695921u, "Server_RequestFuelExtract", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg13 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_RequestFuelExtract(msg13);
						}
					}
					catch (Exception ex12)
					{
						Debug.LogException(ex12);
						player.Kick("RPC Error in Server_RequestFuelExtract");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void FixedUpdate()
	{
		if (!base.isServer || magnetSnap == null)
		{
			return;
		}
		if ((Object)(object)playerTrigger != (Object)null)
		{
			bool hasAnyContents = playerTrigger.HasAnyContents;
			if (PlayerObstructingLift != hasAnyContents)
			{
				using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
				flagsUpdateScope.Set(Flags.Reserved8, hasAnyContents);
			}
		}
		UpdateCarOccupant();
		if (HasOccupant && carOccupant.CouldBeEdited() && carOccupant.GetSpeed() <= 1f)
		{
			if (IsOn() || !carOccupant.IsComplete())
			{
				if ((Object)(object)lockedOccupant == (Object)null && !carOccupant.rigidBody.isKinematic)
				{
					GrabOccupant(occupantTrigger.carOccupant);
				}
				magnetSnap.FixedUpdate(((Component)carOccupant).transform);
			}
			if (carOccupant.CarLock.HasALock && !carOccupant.CarLock.CanHaveALock())
			{
				carOccupant.CarLock.RemoveLock();
			}
		}
		else if (HasOccupant && carOccupant.rigidBody.isKinematic)
		{
			ReleaseOccupant();
		}
		if (HasOccupant && IsDestroyingChassis && carOccupant.HasAnyModules)
		{
			StopChassisDestroy();
		}
	}

	internal override void DoServerDestroy()
	{
		if (HasOccupant)
		{
			ReleaseOccupant();
			if (!HasDriveableOccupant)
			{
				carOccupant.Kill(DestroyMode.Gib);
			}
		}
		base.DoServerDestroy();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		magnetSnap = new MagnetSnap(vehicleLiftPos);
		RefreshOnOffState();
		SetOccupantState(hasOccupant: false, editableOccupant: false, driveableOccupant: false, OccupantLock.CannotHaveLock, forced: true);
		RefreshLiftState(forced: true);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.vehicleLift = Pool.Get<VehicleLift>();
		info.msg.vehicleLift.platformIsOccupied = PlatformIsOccupied;
		info.msg.vehicleLift.editableOccupant = HasEditableOccupant;
		info.msg.vehicleLift.driveableOccupant = HasDriveableOccupant;
		info.msg.vehicleLift.occupantLockState = (int)OccupantLockState;
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (PlatformIsOccupied)
		{
			pickupErrorToFormat = (format: PickupErrors.ItemIsOccupied, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	public override ItemContainerId GetIdealContainer(BasePlayer player, Item item, ItemMoveModifier modifier)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return default(ItemContainerId);
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved7, b: false);
		}
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		bool flag = base.PlayerOpenLoot(player, panelToOpen);
		if (!flag)
		{
			return false;
		}
		if (HasEditableOccupant)
		{
			player.inventory.loot.AddContainer(carOccupant.Inventory.ModuleContainer);
			player.inventory.loot.AddContainer(carOccupant.Inventory.ChassisContainer);
			player.inventory.loot.SendImmediate();
		}
		lootingPlayers.Add(player);
		RefreshLiftState();
		return flag;
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		base.PlayerStoppedLooting(player);
		if (!IsEnteringKeycode)
		{
			lootingPlayers.Remove(player);
			RefreshLiftState();
		}
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		RefreshOnOffState();
	}

	public bool TryGetModuleForItem(Item item, out BaseVehicleModule result)
	{
		if (!HasOccupant)
		{
			result = null;
			return false;
		}
		result = carOccupant.GetModuleForItem(item);
		return (Object)(object)result != (Object)null;
	}

	public void RefreshOnOffState()
	{
		bool flag = !needsElectricity || currentEnergy >= ConsumptionAmount();
		if (flag != IsOn())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, flag);
			}
		}
	}

	public void UpdateCarOccupant()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			if (HasOccupant)
			{
				bool editableOccupant = Vector3.SqrMagnitude(((Component)carOccupant).transform.position - vehicleLiftPos.position) < 1f && carOccupant.CouldBeEdited() && !PlayerObstructingLift;
				bool driveableOccupant = carOccupant.IsComplete();
				SetOccupantState(occupantLockState: carOccupant.CarLock.CanHaveALock() ? ((!carOccupant.CarLock.HasALock) ? OccupantLock.NoLock : OccupantLock.HasLock) : OccupantLock.CannotHaveLock, hasOccupant: HasOccupant, editableOccupant: editableOccupant, driveableOccupant: driveableOccupant);
			}
			else
			{
				SetOccupantState(hasOccupant: false, editableOccupant: false, driveableOccupant: false, OccupantLock.CannotHaveLock);
			}
		}
	}

	public void UpdateOccupantMode()
	{
		if (HasOccupant)
		{
			carOccupant.inEditableLocation = HasEditableOccupant && LiftIsUp;
			carOccupant.immuneToDecay = IsOn();
		}
	}

	public void WakeNearbyRigidbodies()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		Vis.Colliders<Collider>(((Component)this).transform.position, 7f, list, 34816, (QueryTriggerInteraction)2);
		foreach (Collider item in list)
		{
			Rigidbody attachedRigidbody = item.attachedRigidbody;
			if ((Object)(object)attachedRigidbody != (Object)null && attachedRigidbody.IsSleeping())
			{
				attachedRigidbody.WakeUp();
			}
		}
		Pool.FreeUnmanaged<Collider>(ref list);
	}

	public void EditableOccupantEntered()
	{
		RefreshLoot();
	}

	public void EditableOccupantLeft()
	{
		RefreshLoot();
	}

	public void RefreshLoot()
	{
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		list.AddRange(lootingPlayers);
		foreach (BasePlayer item in list)
		{
			item.inventory.loot.Clear();
			PlayerOpenLoot(item);
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
	}

	public void GrabOccupant(ModularCar occupant)
	{
		if (!((Object)(object)occupant == (Object)null))
		{
			lockedOccupant = occupant;
			lockedOccupant.DisablePhysics();
		}
	}

	public void ReleaseOccupant()
	{
		if (HasOccupant)
		{
			carOccupant.inEditableLocation = false;
			carOccupant.immuneToDecay = false;
			if ((Object)(object)lockedOccupant != (Object)null)
			{
				lockedOccupant.EnablePhysics();
				lockedOccupant = null;
			}
		}
	}

	public void StopChassisDestroy()
	{
		if (IsInvoking(FinishDestroyingChassis))
		{
			CancelInvoke(FinishDestroyingChassis);
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved6, b: false);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public unsafe void RPC_RepairItem(RPCMessage msg)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		ItemId val = msg.read.ItemID();
		if ((Object)(object)player == (Object)null || !HasOccupant)
		{
			return;
		}
		Item vehicleItem = carOccupant.GetVehicleItem(val);
		if (vehicleItem != null)
		{
			if (!ComponentEx.HasComponent<ItemModVehicleChassis>((Component)(object)vehicleItem.info) && !ComponentEx.HasComponent<ItemModVehicleModule>((Component)(object)vehicleItem.info))
			{
				Debug.Log((object)$"Can't repair {vehicleItem} as it's not a car part");
				return;
			}
			RepairBench.RepairAnItem(vehicleItem, player, this, 0f, mustKnowBlueprint: false);
			Effect.server.Run(repairEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
		}
		else
		{
			string name = ((object)this).GetType().Name;
			ItemId val2 = val;
			Debug.LogError((object)(name + ": Couldn't get item to repair, with ID: " + ((object)(*(ItemId*)(&val2))/*cast due to constrained. prefix*/).ToString()));
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_OpenEditing(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && !LiftIsMoving)
		{
			PlayerOpenLoot(player);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_DiedWithKeypadOpen(RPCMessage msg)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved7, b: false);
		}
		lootingPlayers.Clear();
		RefreshLiftState();
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_SelectedLootItem(RPCMessage msg)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		ItemId itemUID = msg.read.ItemID();
		if ((Object)(object)player == (Object)null || !player.inventory.loot.IsLooting() || (Object)(object)player.inventory.loot.entitySource != (Object)(object)this || !HasOccupant)
		{
			return;
		}
		Item vehicleItem = carOccupant.GetVehicleItem(itemUID);
		if (vehicleItem == null || Interface.CallHook("OnVehicleModuleSelect", vehicleItem, this, player) != null)
		{
			return;
		}
		bool flag = player.inventory.loot.RemoveContainerAt(3);
		if (TryGetModuleForItem(vehicleItem, out var result))
		{
			if (result is VehicleModuleStorage vehicleModuleStorage)
			{
				IItemContainerEntity container = vehicleModuleStorage.GetContainer();
				if (!ObjectEx.IsUnityNull(container))
				{
					player.inventory.loot.AddContainer(container.inventory);
					flag = true;
				}
			}
			else if (result is VehicleModuleCamper vehicleModuleCamper)
			{
				IItemContainerEntity container2 = vehicleModuleCamper.GetContainer();
				if (!ObjectEx.IsUnityNull(container2))
				{
					player.inventory.loot.AddContainer(container2.inventory);
					flag = true;
				}
			}
		}
		if (flag)
		{
			player.inventory.loot.SendImmediate();
		}
		Interface.CallHook("OnVehicleModuleSelected", vehicleItem, this, player);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_DeselectedLootItem(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player.inventory.loot.IsLooting() && !((Object)(object)player.inventory.loot.entitySource != (Object)(object)this))
		{
			if (player.inventory.loot.RemoveContainerAt(3))
			{
				player.inventory.loot.SendImmediate();
			}
			Interface.CallHook("OnVehicleModuleDeselected", this, player);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_StartKeycodeEntry(RPCMessage msg)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved7, b: true);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_RequestAddLock(RPCMessage msg)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (!HasOccupant || carOccupant.CarLock.HasALock)
		{
			return;
		}
		BasePlayer player = msg.player;
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		string text = msg.read.String();
		if (Interface.CallHook("OnVehicleLockRequest", this, player, text) == null)
		{
			ItemAmount itemAmount = lockResourceCost;
			if ((float)player.inventory.GetAmount(itemAmount.itemDef.itemid) >= itemAmount.amount && carOccupant.CarLock.TryAddALock(text, player.userID))
			{
				player.inventory.Take(null, itemAmount.itemDef.itemid, Mathf.CeilToInt(itemAmount.amount));
				Effect.server.Run(addRemoveLockEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_RequestRemoveLock(RPCMessage msg)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (HasOccupant && carOccupant.CarLock.HasALock && Interface.CallHook("OnLockRemove", carOccupant, msg.player) == null)
		{
			carOccupant.CarLock.RemoveLock();
			Effect.server.Run(addRemoveLockEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_RequestNewCode(RPCMessage msg)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (!HasOccupant || !carOccupant.CarLock.HasALock)
		{
			return;
		}
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null))
		{
			string text = msg.read.String();
			if (Interface.CallHook("OnCodeChange", carOccupant, player, text) == null && carOccupant.CarLock.TrySetNewCode(text, player.userID))
			{
				Effect.server.Run(changeLockCodeEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	public void RPC_StartDestroyingChassis(RPCMessage msg)
	{
		if (carOccupant.HasAnyModules)
		{
			return;
		}
		Invoke(FinishDestroyingChassis, 10f);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved6, b: true);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	public void RPC_StopDestroyingChassis(RPCMessage msg)
	{
		StopChassisDestroy();
	}

	public void FinishDestroyingChassis()
	{
		if (!HasOccupant || carOccupant.HasAnyModules)
		{
			return;
		}
		carOccupant.Kill(DestroyMode.Gib);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved6, b: false);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void Server_RequestFuelExtract(RPCMessage msg)
	{
		if (!lootingPlayers.Contains(msg.player))
		{
			return;
		}
		foreach (BaseVehicleModule attachedModuleEntity in carOccupant.AttachedModuleEntities)
		{
			if (!(attachedModuleEntity is VehicleModuleStorage vehicleModuleStorage) || !(vehicleModuleStorage.GetStorageUnitInstance() is LiquidContainerCrude liquidContainerCrude))
			{
				continue;
			}
			Item slot = liquidContainerCrude.inventory.GetSlot(0);
			if (slot != null)
			{
				if (slot.amount > slot.info.stackable)
				{
					Item item = slot.SplitItem(slot.info.stackable);
					msg.player.GiveItem(item);
				}
				else
				{
					slot.LockUnlock(bNewState: false);
					slot.RemoveFromContainer();
					msg.player.GiveItem(slot);
				}
			}
		}
	}

	public override void PreProcess(IPrefabProcessor process, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		downPos = ((Component)vehicleLift).transform.position;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (base.isServer)
		{
			UpdateOccupantMode();
		}
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (IsOn())
		{
			return base.CanBeLooted(player);
		}
		return false;
	}

	public override int ConsumptionAmount()
	{
		return 5;
	}

	public override bool ShouldUseCastNoClipChecks()
	{
		return true;
	}

	public void SetOccupantState(bool hasOccupant, bool editableOccupant, bool driveableOccupant, OccupantLock occupantLockState, bool forced = false)
	{
		if (PlatformIsOccupied == hasOccupant && HasEditableOccupant == editableOccupant && HasDriveableOccupant == driveableOccupant && OccupantLockState == occupantLockState && !forced)
		{
			return;
		}
		bool hasEditableOccupant = HasEditableOccupant;
		PlatformIsOccupied = hasOccupant;
		HasEditableOccupant = editableOccupant;
		HasDriveableOccupant = driveableOccupant;
		OccupantLockState = occupantLockState;
		if (base.isServer)
		{
			UpdateOccupantMode();
			SendNetworkUpdate();
			if (hasEditableOccupant && !editableOccupant)
			{
				EditableOccupantLeft();
			}
			else if (editableOccupant && !hasEditableOccupant)
			{
				EditableOccupantEntered();
			}
		}
		RefreshLiftState();
	}

	public void RefreshLiftState(bool forced = false)
	{
		VehicleLiftState desiredLiftState = ((IsOpen() || IsEnteringKeycode || (HasEditableOccupant && !HasDriveableOccupant)) ? VehicleLiftState.Up : VehicleLiftState.Down);
		MoveLift(desiredLiftState, 0f, forced);
	}

	public void MoveLift(VehicleLiftState desiredLiftState, float startDelay = 0f, bool forced = false)
	{
		if (vehicleLiftState == desiredLiftState && !forced)
		{
			return;
		}
		_ = vehicleLiftState;
		vehicleLiftState = desiredLiftState;
		if (base.isServer)
		{
			UpdateOccupantMode();
			WakeNearbyRigidbodies();
		}
		if (!((Component)this).gameObject.activeSelf)
		{
			if ((Object)(object)vehicleLiftAnim != (Object)null)
			{
				vehicleLiftAnim[animName].time = ((desiredLiftState == VehicleLiftState.Up) ? 1f : 0f);
				vehicleLiftAnim.Play();
			}
		}
		else if (desiredLiftState == VehicleLiftState.Up)
		{
			Invoke(MoveLiftUp, startDelay);
		}
		else
		{
			Invoke(MoveLiftDown, startDelay);
		}
	}

	public void MoveLiftUp()
	{
		if (!((Object)(object)vehicleLiftAnim == (Object)null))
		{
			AnimationState obj = vehicleLiftAnim[animName];
			obj.speed = obj.length / liftMoveTime;
			vehicleLiftAnim.Play();
		}
	}

	public void MoveLiftDown()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)vehicleLiftAnim == (Object)null))
		{
			AnimationState val = vehicleLiftAnim[animName];
			val.speed = val.length / liftMoveTime;
			if (!vehicleLiftAnim.isPlaying && Vector3.Distance(((Component)vehicleLift).transform.position, downPos) > 0.01f)
			{
				val.time = 1f;
			}
			val.speed *= -1f;
			vehicleLiftAnim.Play();
		}
	}
}
