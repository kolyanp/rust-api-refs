using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class Door : AnimatedBuildingBlock, INotifyTrigger, ISimpleUpgradable
{
	public static readonly Phrase UpgradeBlockedLock;

	public GameObjectRef knockEffect;

	public bool canTakeLock = true;

	public bool hasHatch;

	public bool canTakeCloser;

	public bool canTakeKnocker;

	public bool canNpcOpen = true;

	public bool canHandOpen = true;

	public bool isSecurityDoor;

	public bool canReverseOpen;

	public TriggerNotify[] vehiclePhysBoxes;

	public bool checkPhysBoxesOnOpen;

	public SoundDefinition vehicleCollisionSfx;

	public GameObject[] BusyColliderRoots;

	public GameObject[] ClosedColliderRoots;

	public bool allowOnCargoShip;

	public bool useCastNoClipChecks;

	public List<ItemDefinition> UpgradeItems;

	public Menu.Option UpgradeMenu;

	[SerializeField]
	[ReadOnly]
	private float openAnimLength = 4f;

	[SerializeField]
	[ReadOnly]
	private float closeAnimLength = 4f;

	public const Flags ReverseOpen = Flags.Reserved1;

	public NavMeshModifierVolume NavMeshVolumeAnimals;

	public NavMeshModifierVolume NavMeshVolumeHumanoids;

	public NPCDoorTriggerBox NpcTriggerBox;

	public NavMeshLink NavMeshLink;

	private static int nonWalkableArea;

	private static int animalAgentTypeId;

	private static int humanoidAgentTypeId;

	private float decayResetTimeLast = float.NegativeInfinity;

	private Dictionary<BasePlayer, TimeSince> woundedOpens = new Dictionary<BasePlayer, TimeSince>();

	private Dictionary<BasePlayer, TimeSince> woundedCloses = new Dictionary<BasePlayer, TimeSince>();

	private Action enableVehiclePhysBoxesAction;

	private Action disableVehiclePhysBoxAction;

	private float nextKnockTime = float.NegativeInfinity;

	private static int openHash;

	private static int closeHash;

	private static int reverseOpenHash;

	private static int reverseCloseAnimHash;

	private static int reverseOpenAnimHash;

	public override bool AllowOnCargoShip => allowOnCargoShip;

	private bool HasVehiclePushBoxes
	{
		get
		{
			if (vehiclePhysBoxes != null)
			{
				return vehiclePhysBoxes.Length != 0;
			}
			return false;
		}
	}

	public override bool PreserveChildrenWhenReskinning => true;

	protected virtual bool IgnoreBlockageDotCheck => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Door.OnRpcMessage"))
		{
			if (rpc == 2824056853u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DoSimpleUpgrade"));
				}
				using (TimeWarning.New("DoSimpleUpgrade"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2824056853u, "DoSimpleUpgrade", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2824056853u, "DoSimpleUpgrade", this, player, 3f))
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
							DoSimpleUpgrade(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in DoSimpleUpgrade");
					}
				}
				return true;
			}
			if (rpc == 3999508679u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_CloseDoor"));
				}
				using (TimeWarning.New("RPC_CloseDoor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3999508679u, "RPC_CloseDoor", this, player, 3f))
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
							RPC_CloseDoor(rpc2);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_CloseDoor");
					}
				}
				return true;
			}
			if (rpc == 1487779344 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_KnockDoor"));
				}
				using (TimeWarning.New("RPC_KnockDoor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1487779344u, "RPC_KnockDoor", this, player, 3f))
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
							RPC_KnockDoor(rpc3);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_KnockDoor");
					}
				}
				return true;
			}
			if (rpc == 3314360565u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenDoor"));
				}
				using (TimeWarning.New("RPC_OpenDoor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3314360565u, "RPC_OpenDoor", this, player, 3f))
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
							RPC_OpenDoor(rpc4);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RPC_OpenDoor");
					}
				}
				return true;
			}
			if (rpc == 3000490601u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ToggleHatch"));
				}
				using (TimeWarning.New("RPC_ToggleHatch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3000490601u, "RPC_ToggleHatch", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_ToggleHatch(rpc5);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in RPC_ToggleHatch");
					}
				}
				return true;
			}
			if (rpc == 3672787865u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_NotifyWoundedClose"));
				}
				using (TimeWarning.New("Server_NotifyWoundedClose"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3672787865u, "Server_NotifyWoundedClose", this, player, 3f))
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
							Server_NotifyWoundedClose(msg3);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in Server_NotifyWoundedClose");
					}
				}
				return true;
			}
			if (rpc == 3730851545u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_NotifyWoundedOpen"));
				}
				using (TimeWarning.New("Server_NotifyWoundedOpen"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3730851545u, "Server_NotifyWoundedOpen", this, player, 3f))
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
							Server_NotifyWoundedOpen(msg4);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
						player.Kick("RPC Error in Server_NotifyWoundedOpen");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		base.ResetState();
		if (base.isServer)
		{
			decayResetTimeLast = float.NegativeInfinity;
			if (isSecurityDoor && (Object)(object)NavMeshLink != (Object)null)
			{
				SetNavMeshLinkEnabled(wantsOn: false);
			}
			woundedCloses.Clear();
			woundedOpens.Clear();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.door = Pool.Get<Door>();
		info.msg.door.canNpcOpen = canNpcOpen;
		info.msg.door.canHandOpen = canHandOpen;
		info.msg.door.isSecurityDoor = isSecurityDoor;
	}

	public override void ServerInit()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		if (nonWalkableArea < 0)
		{
			nonWalkableArea = NavMesh.GetAreaFromName("Not Walkable");
		}
		if (animalAgentTypeId < 0)
		{
			NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(1);
			animalAgentTypeId = ((NavMeshBuildSettings)(ref settingsByIndex)).agentTypeID;
		}
		if ((Object)(object)NavMeshVolumeAnimals == (Object)null)
		{
			NavMeshVolumeAnimals = ((Component)this).gameObject.AddComponent<NavMeshModifierVolume>();
			NavMeshVolumeAnimals.area = nonWalkableArea;
			NavMeshVolumeAnimals.AddAgentType(animalAgentTypeId);
			NavMeshVolumeAnimals.center = Vector3.zero;
			NavMeshVolumeAnimals.size = Vector3.one;
		}
		if (!Application.isLoadingSave)
		{
			InitializeNpcInteraction();
		}
		AIInformationZone forPoint = AIInformationZone.GetForPoint(((Component)this).transform.position);
		if ((Object)(object)forPoint != (Object)null && (Object)(object)NavMeshLink == (Object)null)
		{
			NavMeshLink = forPoint.GetClosestNavMeshLink(((Component)this).transform.position);
		}
		DisableVehiclePhysBox();
		UpdateColliderStates();
	}

	private void InitializeNpcInteraction()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (HasSlot(Slot.Lock))
		{
			canNpcOpen = false;
		}
		if (!canNpcOpen)
		{
			if (humanoidAgentTypeId < 0)
			{
				NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(0);
				humanoidAgentTypeId = ((NavMeshBuildSettings)(ref settingsByIndex)).agentTypeID;
			}
			if ((Object)(object)NavMeshVolumeHumanoids == (Object)null)
			{
				NavMeshVolumeHumanoids = ((Component)this).gameObject.AddComponent<NavMeshModifierVolume>();
				NavMeshVolumeHumanoids.area = nonWalkableArea;
				NavMeshVolumeHumanoids.AddAgentType(humanoidAgentTypeId);
				NavMeshVolumeHumanoids.center = Vector3.zero;
				NavMeshVolumeHumanoids.size = Vector3.one + Vector3.up + Vector3.forward;
			}
		}
		else if ((Object)(object)NpcTriggerBox == (Object)null)
		{
			if (isSecurityDoor)
			{
				NavMeshObstacle obj = ((Component)this).gameObject.AddComponent<NavMeshObstacle>();
				obj.carving = true;
				obj.center = Vector3.zero;
				obj.size = Vector3.one;
				obj.shape = (NavMeshObstacleShape)1;
			}
			NpcTriggerBox = new GameObject("NpcTriggerBox").AddComponent<NPCDoorTriggerBox>();
			NpcTriggerBox.Setup(this);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		InitializeNpcInteraction();
	}

	public override void OnEntityMessage(BaseEntity from, string msg)
	{
		base.OnEntityMessage(from, msg);
		if (msg == "ForceOpenNPCDoor")
		{
			ForceOpenNPCDoor();
		}
	}

	public void ForceOpenNPCDoor()
	{
		SetOpen(open: true);
		Invoke(CloseRequest, 5f);
	}

	public override bool HasSlot(Slot slot)
	{
		if (slot == Slot.Lock && canTakeLock)
		{
			return true;
		}
		switch (slot)
		{
		case Slot.UpperModifier:
			return true;
		case Slot.CenterDecoration:
			if (canTakeCloser)
			{
				return true;
			}
			break;
		}
		if (slot == Slot.LowerCenterDecoration && canTakeKnocker)
		{
			return true;
		}
		return base.HasSlot(slot);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.door != null)
		{
			canNpcOpen = info.msg.door.canNpcOpen;
			canHandOpen = info.msg.door.canHandOpen;
			isSecurityDoor = info.msg.door.isSecurityDoor;
		}
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (IsOpen() && !Object.op_Implicit((Object)(object)GetSlot(Slot.Lock)))
		{
			return base.ShouldDisplayPickupOption(player);
		}
		return false;
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		pickupErrorToFormat.arg0 = pickup.itemTarget.displayName;
		if (Object.op_Implicit((Object)(object)GetSlot(Slot.UpperModifier)))
		{
			pickupErrorToFormat.format = PickupErrors.ItemHasCloser;
			return false;
		}
		if (Object.op_Implicit((Object)(object)GetSlot(Slot.CenterDecoration)) || Object.op_Implicit((Object)(object)GetSlot(Slot.LowerCenterDecoration)))
		{
			pickupErrorToFormat.format = PickupErrors.ItemHasDecoration;
			return false;
		}
		return base.CanCompletePickup(player);
	}

	public void CloseRequest()
	{
		SetOpen(open: false);
	}

	[UnityEvent]
	public void SetOpen(bool open, bool suppressBlockageChecks = false)
	{
		SetFlagLocal(Flags.Open, open);
		SendNetworkUpdateImmediate();
		if (isSecurityDoor && (Object)(object)NavMeshLink != (Object)null)
		{
			SetNavMeshLinkEnabled(open);
		}
		if (!suppressBlockageChecks && (!open || checkPhysBoxesOnOpen))
		{
			StartCheckingForBlockages(open);
		}
	}

	[UnityEvent]
	public void SetLocked(bool locked)
	{
		SetFlagLocal(Flags.Locked, b: false);
		SendNetworkUpdateImmediate();
	}

	public bool GetPlayerLockPermission(BasePlayer player)
	{
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if ((Object)(object)baseLock == (Object)null)
		{
			return true;
		}
		return baseLock.GetPlayerLockPermission(player);
	}

	public void SetNavMeshLinkEnabled(bool wantsOn)
	{
		if ((Object)(object)NavMeshLink != (Object)null)
		{
			if (wantsOn)
			{
				((Component)NavMeshLink).gameObject.SetActive(true);
				((Behaviour)NavMeshLink).enabled = true;
			}
			else
			{
				((Behaviour)NavMeshLink).enabled = false;
				((Component)NavMeshLink).gameObject.SetActive(false);
			}
		}
	}

	protected virtual bool CanDoorBeOpened()
	{
		return true;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	protected void RPC_OpenDoor(RPCMessage rpc)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		if (!rpc.player.CanInteract(usableWhileCrawling: true) || !canHandOpen || IsOpen() || IsBusy() || IsLocked() || IsInvoking(DelayedDoorOpening) || !CanDoorBeOpened())
		{
			return;
		}
		if (rpc.player.IsWounded())
		{
			if (!woundedOpens.ContainsKey(rpc.player) || !(TimeSince.op_Implicit(woundedOpens[rpc.player]) > 2.5f))
			{
				return;
			}
			woundedOpens.Remove(rpc.player);
		}
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if ((Object)(object)baseLock != (Object)null)
		{
			if (!baseLock.OnTryToOpen(rpc.player))
			{
				return;
			}
			if (baseLock.IsLocked() && Time.realtimeSinceStartup - decayResetTimeLast > 60f)
			{
				BuildingBlock buildingBlock = FindLinkedEntity<BuildingBlock>();
				if (Object.op_Implicit((Object)(object)buildingBlock))
				{
					Decay.BuildingDecayTouch(buildingBlock);
				}
				else
				{
					Decay.RadialDecayTouch(((Component)this).transform.position, 40f, 2097408);
				}
				decayResetTimeLast = Time.realtimeSinceStartup;
			}
		}
		if (canReverseOpen)
		{
			Vector3 val = ((Component)this).transform.InverseTransformPoint(((Component)rpc.player).transform.position);
			SetFlagLocal(Flags.Reserved1, val.x > 0f);
		}
		if (ShouldDelayOpen(rpc.player, out var delay))
		{
			Invoke(DelayedDoorOpening, delay);
		}
		else
		{
			SetFlagLocal(Flags.Open, b: true);
			SendNetworkUpdateImmediate();
		}
		if (isSecurityDoor && (Object)(object)NavMeshLink != (Object)null)
		{
			SetNavMeshLinkEnabled(wantsOn: true);
		}
		if (checkPhysBoxesOnOpen)
		{
			StartCheckingForBlockages(isOpening: true);
		}
		Facepunch.Rust.Analytics.Azure.OnBaseInteract(rpc.player, this);
		OnPlayerOpenedDoor(rpc.player);
		Interface.CallHook("OnDoorOpened", this, rpc.player);
	}

	private void DelayedDoorOpening()
	{
		SetFlagLocal(Flags.Open, b: true);
		SendNetworkUpdateImmediate();
	}

	protected virtual void OnPlayerOpenedDoor(BasePlayer p)
	{
	}

	protected virtual bool ShouldDelayOpen(BasePlayer forPlayer, out float delay)
	{
		delay = 0f;
		return false;
	}

	private void StartCheckingForBlockages(bool isOpening)
	{
		using (TimeWarning.New("StartCheckingForBlockages"))
		{
			if (HasVehiclePushBoxes)
			{
				if (enableVehiclePhysBoxesAction == null)
				{
					enableVehiclePhysBoxesAction = EnableVehiclePhysBoxes;
				}
				if (disableVehiclePhysBoxAction == null)
				{
					disableVehiclePhysBoxAction = DisableVehiclePhysBox;
				}
				float num = (isOpening ? openAnimLength : closeAnimLength);
				Invoke(enableVehiclePhysBoxesAction, num * 0.1f);
				Invoke(disableVehiclePhysBoxAction, num * 0.8f);
			}
		}
	}

	private void StopCheckingForBlockages()
	{
		using (TimeWarning.New("StopCheckingForBlockages"))
		{
			if (HasVehiclePushBoxes)
			{
				if (disableVehiclePhysBoxAction == null)
				{
					disableVehiclePhysBoxAction = DisableVehiclePhysBox;
				}
				ToggleVehiclePushBoxes(state: false);
				CancelInvoke(disableVehiclePhysBoxAction);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_CloseDoor(RPCMessage rpc)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!rpc.player.CanInteract(usableWhileCrawling: true) || !canHandOpen || !IsOpen() || IsBusy() || IsLocked())
		{
			return;
		}
		if (rpc.player.IsWounded())
		{
			if (!woundedCloses.ContainsKey(rpc.player) || !(TimeSince.op_Implicit(woundedCloses[rpc.player]) > 2.5f))
			{
				return;
			}
			woundedCloses.Remove(rpc.player);
		}
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if (!((Object)(object)baseLock != (Object)null) || baseLock.OnTryToClose(rpc.player))
		{
			SetFlagLocal(Flags.Open, b: false);
			SendNetworkUpdateImmediate();
			if (isSecurityDoor && (Object)(object)NavMeshLink != (Object)null)
			{
				SetNavMeshLinkEnabled(wantsOn: false);
			}
			Facepunch.Rust.Analytics.Azure.OnBaseInteract(rpc.player, this);
			StartCheckingForBlockages(isOpening: false);
			OnPlayerClosedDoor(rpc.player);
			Interface.CallHook("OnDoorClosed", this, rpc.player);
		}
	}

	protected virtual void OnPlayerClosedDoor(BasePlayer p)
	{
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_KnockDoor(RPCMessage rpc)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (!rpc.player.CanInteract(usableWhileCrawling: true) || !knockEffect.isValid || Time.realtimeSinceStartup < nextKnockTime)
		{
			return;
		}
		nextKnockTime = Time.realtimeSinceStartup + 0.5f;
		BaseEntity slot = GetSlot(Slot.LowerCenterDecoration);
		if ((Object)(object)slot != (Object)null)
		{
			DoorKnocker component = ((Component)slot).GetComponent<DoorKnocker>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.Knock(rpc.player);
				return;
			}
		}
		Effect.server.Run(knockEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
		Interface.CallHook("OnDoorKnocked", this, rpc.player);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_ToggleHatch(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract(usableWhileCrawling: true) || !hasHatch)
		{
			return;
		}
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if (Object.op_Implicit((Object)(object)baseLock) && !baseLock.OnTryToOpen(rpc.player))
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, !HasFlag(Flags.Reserved3));
	}

	private void EnableVehiclePhysBoxes()
	{
		ToggleVehiclePushBoxes(state: true);
	}

	private void DisableVehiclePhysBox()
	{
		ToggleVehiclePushBoxes(state: false);
	}

	private void ToggleVehiclePushBoxes(bool state)
	{
		if (vehiclePhysBoxes == null)
		{
			return;
		}
		TriggerNotify[] array = vehiclePhysBoxes;
		foreach (TriggerNotify triggerNotify in array)
		{
			if ((Object)(object)triggerNotify != (Object)null)
			{
				((Behaviour)triggerNotify).enabled = state;
				((Component)triggerNotify).gameObject.SetActive(state);
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void Server_NotifyWoundedOpen(RPCMessage msg)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (player.IsWounded())
		{
			if (!woundedOpens.ContainsKey(player))
			{
				woundedOpens.Add(player, default(TimeSince));
			}
			else
			{
				woundedOpens[player] = TimeSince.op_Implicit(0f);
			}
			Invoke(delegate
			{
				CheckTimedOutPlayers(woundedOpens);
			}, 5f);
		}
	}

	private void CheckTimedOutPlayers(Dictionary<BasePlayer, TimeSince> dictionary)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		foreach (KeyValuePair<BasePlayer, TimeSince> item in dictionary)
		{
			if (TimeSince.op_Implicit(item.Value) > 5f)
			{
				list.Add(item.Key);
			}
		}
		foreach (BasePlayer item2 in list)
		{
			if (dictionary.ContainsKey(item2))
			{
				dictionary.Remove(item2);
			}
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void Server_NotifyWoundedClose(RPCMessage msg)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (player.IsWounded())
		{
			if (!woundedCloses.ContainsKey(player))
			{
				woundedCloses.Add(player, default(TimeSince));
			}
			else
			{
				woundedCloses[player] = TimeSince.op_Implicit(0f);
			}
			Invoke(delegate
			{
				CheckTimedOutPlayers(woundedCloses);
			}, 5f);
		}
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	public override void OnPickedUp(Item createdItem, BasePlayer player)
	{
		base.OnPickedUp(createdItem, player);
		foreach (BaseEntity child in children)
		{
			if (child is CustomDoorManipulator customDoorManipulator)
			{
				player.GiveItem(ItemManager.CreateByItemID(customDoorManipulator.sourceItem.itemid, 1, 0uL, 0uL), GiveItemReason.PickedUp);
			}
		}
	}

	public override bool ShouldUseCastNoClipChecks()
	{
		if (!useCastNoClipChecks)
		{
			return base.ShouldUseCastNoClipChecks();
		}
		return true;
	}

	private void ReparentDisabledExplosives()
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		if (children.Count == 0)
		{
			return;
		}
		PooledList<TimedExplosive> val = Pool.Get<PooledList<TimedExplosive>>();
		try
		{
			foreach (BaseEntity child in children)
			{
				if (!((Component)child).gameObject.activeInHierarchy && child is TimedExplosive item)
				{
					((List<TimedExplosive>)(object)val).Add(item);
				}
			}
			if (((List<TimedExplosive>)(object)val).Count == 0)
			{
				return;
			}
			PooledList<Collider> val2 = Pool.Get<PooledList<Collider>>();
			try
			{
				((Component)this).gameObject.GetComponentsInChildren<Collider>(false, (List<Collider>)(object)val2);
				foreach (TimedExplosive item2 in (List<TimedExplosive>)(object)val)
				{
					Collider val3 = null;
					float num = float.MaxValue;
					foreach (Collider item3 in (List<Collider>)(object)val2)
					{
						if (!item3.isTrigger && !((Object)(object)GameObjectEx.ToBaseEntity(item3) != (Object)(object)this))
						{
							Bounds val4 = item3.bounds;
							float num2 = ((Bounds)(ref val4)).SqrDistance(item2.WorldSpaceBounds().position);
							if (num2 < num)
							{
								num = num2;
								val3 = item3;
							}
						}
					}
					if ((Object)(object)val3 != (Object)null)
					{
						item2.SetParent(this, FindBoneID(((Component)val3).transform), worldPositionStays: true, sendImmediate: true);
					}
					else
					{
						item2.SetParent(this, worldPositionStays: true, sendImmediate: true);
					}
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void ReverseDoorAnimation(bool wasOpening, bool reverse)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)model == (Object)null) && !((Object)(object)model.animator == (Object)null))
		{
			AnimatorStateInfo currentAnimatorStateInfo = model.animator.GetCurrentAnimatorStateInfo(0);
			if (reverse)
			{
				model.animator.Play(wasOpening ? reverseCloseAnimHash : reverseOpenAnimHash, 0, 1f - ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).normalizedTime);
			}
			else
			{
				model.animator.Play(wasOpening ? closeHash : openHash, 0, 1f - ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).normalizedTime);
			}
		}
	}

	public override float AntiHackPadding()
	{
		return 2f;
	}

	protected virtual bool OnlyCheckForVehicles()
	{
		return true;
	}

	protected virtual bool InverseDotCheck()
	{
		return false;
	}

	protected virtual bool CheckOnClose()
	{
		return true;
	}

	public void OnObjects(TriggerNotify trigger)
	{
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isServer || !((Behaviour)trigger).isActiveAndEnabled || (!HasFlag(Flags.Open) && !CheckOnClose()))
		{
			return;
		}
		bool flag = false;
		BaseEntity baseEntity = null;
		if (!flag && (trigger.entityContents != null || trigger.entityContents.Count != 0))
		{
			foreach (BaseEntity entityContent in trigger.entityContents)
			{
				if (OnlyCheckForVehicles())
				{
					if (entityContent is BaseMountable { BlocksDoors: not false } baseMountable)
					{
						flag = true;
						baseEntity = baseMountable;
						break;
					}
					if (entityContent is BaseVehicleModule baseVehicleModule && (Object)(object)baseVehicleModule.Vehicle != (Object)null && baseVehicleModule.Vehicle.BlocksDoors)
					{
						flag = true;
						baseEntity = baseVehicleModule.VehicleParent();
						break;
					}
					if (entityContent is BoatBuildingBlock)
					{
						flag = true;
						baseEntity = entityContent;
						break;
					}
				}
				else if (!((Object)(object)entityContent == (Object)null) && entityContent.IsValid() && !((Object)(object)entityContent == (Object)(object)this) && !((Object)(object)parentEntity.Get(serverside: true) == (Object)(object)entityContent))
				{
					flag = true;
					baseEntity = entityContent;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		bool flag2 = HasFlag(Flags.Open);
		bool flag3 = HasFlag(Flags.Reserved1);
		if (checkPhysBoxesOnOpen && !canReverseOpen && !IgnoreBlockageDotCheck)
		{
			bool flag4 = true;
			TriggerNotify[] array = vehiclePhysBoxes;
			foreach (TriggerNotify triggerNotify in array)
			{
				Vector3 forward = ((Component)triggerNotify).transform.forward;
				Vector3 val = ((Component)baseEntity).transform.position - ((Component)triggerNotify).transform.position;
				float num = Vector3.Dot(forward, ((Vector3)(ref val)).normalized);
				if (InverseDotCheck() ? (num < 0f) : (num > 0f))
				{
					flag4 = false;
					break;
				}
			}
			if (flag4 == flag2 || flag4 == flag3)
			{
				return;
			}
		}
		ReverseDoorAnimation(flag2, flag3);
		SetOpen(!flag2, suppressBlockageChecks: true);
		StopCheckingForBlockages();
		ClientRPC(RpcTarget.NetworkGroup("OnDoorInterrupted"), flag2, flag3);
	}

	public void OnEmpty()
	{
	}

	protected override void ApplySubAnimationParameters(bool init, Animator toAnimator)
	{
		base.ApplySubAnimationParameters(init, toAnimator);
		if (canReverseOpen)
		{
			toAnimator.SetBool(reverseOpenHash, HasFlag(Flags.Reserved1));
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (base.isServer)
		{
			BaseEntity slot = GetSlot(Slot.UpperModifier);
			if (Object.op_Implicit((Object)(object)slot))
			{
				((Component)slot).SendMessage("Think");
			}
		}
		UpdateColliderStates();
		if (base.isServer && ((old ^ next) & (Flags.Open | Flags.Busy)) != 0)
		{
			ReparentDisabledExplosives();
		}
	}

	private void UpdateColliderStates()
	{
		UpdateBusyColliderState();
		UpdateClosedColliderState();
	}

	private void UpdateBusyColliderState()
	{
		if (BusyColliderRoots == null)
		{
			return;
		}
		bool flag = HasFlag(Flags.Busy);
		GameObject[] busyColliderRoots = BusyColliderRoots;
		foreach (GameObject val in busyColliderRoots)
		{
			if ((Object)(object)val != (Object)null && val.gameObject.activeSelf != flag)
			{
				val.gameObject.SetActive(flag);
			}
		}
	}

	private void UpdateClosedColliderState()
	{
		if (ClosedColliderRoots == null)
		{
			return;
		}
		bool flag = !HasFlag(Flags.Open) && !HasFlag(Flags.Busy);
		GameObject[] closedColliderRoots = ClosedColliderRoots;
		foreach (GameObject val in closedColliderRoots)
		{
			if ((Object)(object)val != (Object)null && val.gameObject.activeSelf != flag)
			{
				val.gameObject.SetActive(flag);
			}
		}
	}

	public List<ItemDefinition> GetUpgradeItems()
	{
		return UpgradeItems;
	}

	public bool CanUpgrade(BasePlayer player, ItemDefinition upgradeItem)
	{
		if (IsOpen())
		{
			return false;
		}
		return global::SimpleUpgrade.CanUpgrade(this, upgradeItem, player);
	}

	public bool HasLock()
	{
		return (Object)(object)GetSlot(Slot.Lock) != (Object)null;
	}

	public void DoUpgrade(BasePlayer player, ItemDefinition upgradeItem)
	{
		global::SimpleUpgrade.DoUpgrade(this, player, upgradeItem);
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	public void DoSimpleUpgrade(RPCMessage msg)
	{
		if (base.SecondsSinceAttacked < 30f)
		{
			msg.player.ShowToast(GameTip.Styles.Error, ConstructionErrors.CantUpgradeRecentlyDamaged, false, (30f - base.SecondsSinceAttacked).ToString("N0"));
			return;
		}
		int num = msg.read.Int32();
		if (num >= 0 && num < UpgradeItems.Count && CanUpgrade(msg.player, UpgradeItems[num]))
		{
			DoUpgrade(msg.player, UpgradeItems[num]);
		}
	}

	public bool UpgradingEnabled()
	{
		if (UpgradeItems != null)
		{
			return UpgradeItems.Count > 0;
		}
		return false;
	}

	public bool CostIsItem()
	{
		return true;
	}

	public override bool CanBeReskinned(BasePlayer player)
	{
		if (!GetPlayerLockPermission(player))
		{
			SprayCan.LastReskinError = SprayCan.NeedDoorAccess;
			return false;
		}
		if (IsOpen() || IsBusy())
		{
			SprayCan.LastReskinError = SprayCan.DoorMustBeClosed;
			return false;
		}
		if ((Object)(object)GetParentEntity() != (Object)null && GetParentEntity() is HotAirBalloonArmor)
		{
			SprayCan.LastReskinError = SprayCan.CannotReskinThatDoor;
			return false;
		}
		return base.CanBeReskinned(player);
	}

	static Door()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		UpgradeBlockedLock = new Phrase("simple.upgrade.blocked_lock", "Remove lock to upgrade.");
		nonWalkableArea = -1;
		animalAgentTypeId = -1;
		humanoidAgentTypeId = -1;
		openHash = Animator.StringToHash("open");
		closeHash = Animator.StringToHash("close");
		reverseOpenHash = Animator.StringToHash("reverseOpen");
		reverseCloseAnimHash = Animator.StringToHash("CloseReverse");
		reverseOpenAnimHash = Animator.StringToHash("OpenReverse");
	}
}
