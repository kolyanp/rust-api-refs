using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class SteeringWheel : BaseMountable, global::IBoatBuildingPiece
{
	public static Phrase TipPhrase = new Phrase("boat_steeringwheel_tip", "Interact with the steering wheel to code lock your boat.");

	public static Phrase MountedTipPhrase = new Phrase("boat_steeringwheel_tip_mounted", "Look at the center of the wheel for options when mounted.");

	[Header("Steering Wheel")]
	public GameObjectRef PrivPrefab;

	public GameObjectRef KeyEnterDialog;

	public float TurnLerpSpeed = 1f;

	public Transform Wheel;

	public PlayerBoatPrivilege Privilege;

	public SoundDefinition wheelTurnLoopDef;

	public SoundDefinition wheelTurnStartDef;

	public SoundDefinition wheelTurnStopDef;

	public float wheelTurnLoopFadeTime = 0.1f;

	public float stopDelay;

	public GameObjectRef finishBuildingEffect;

	public SoundDefinition wheelCenterDef;

	[Header("Effects")]
	public Transform EffectLocation;

	public GameObjectRef effectUnlocked;

	public GameObjectRef effectLocked;

	public GameObjectRef effectDenied;

	public GameObjectRef effectCodeChanged;

	public GameObjectRef effectShock;

	[NonSerialized]
	public PlayerBoat ParentBoat;

	private Action _mountedPlayerClipCheck;

	private float __sync_ServerSteeringRotation;

	public PlayerBoatLock BoatLock { get; private set; }

	[Sync(Pack = false, Autosave = true)]
	public float ServerSteeringRotation
	{
		[CompilerGenerated]
		get
		{
			return __sync_ServerSteeringRotation;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_ServerSteeringRotation, value))
			{
				__sync_ServerSteeringRotation = value;
				byte nameID = __GetWeaverID("ServerSteeringRotation");
				SV_SyncVarSend(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SteeringWheel.OnRpcMessage"))
		{
			if (rpc == 3277541392u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ReceiveClientRotation"));
				}
				using (TimeWarning.New("ReceiveClientRotation"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3277541392u, "ReceiveClientRotation", this, player, 15uL))
						{
							return true;
						}
						long position = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<float>()))
						{
							return true;
						}
						msg.read.Position = position;
						if (!RPC_Server.IsVisible.Test(3277541392u, "ReceiveClientRotation", this, player, 3f))
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
							ReceiveClientRotation(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ReceiveClientRotation");
					}
				}
				return true;
			}
			if (rpc == 1618039250 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RequestDeployAndEditBoat"));
				}
				using (TimeWarning.New("RequestDeployAndEditBoat"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1618039250u, "RequestDeployAndEditBoat", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1618039250u, "RequestDeployAndEditBoat", this, player, 3f))
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
							RequestDeployAndEditBoat(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RequestDeployAndEditBoat");
					}
				}
				return true;
			}
			if (rpc == 3915953376u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RequestEditBoat"));
				}
				using (TimeWarning.New("RequestEditBoat"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3915953376u, "RequestEditBoat", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3915953376u, "RequestEditBoat", this, player, 3f))
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
							RequestEditBoat(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RequestEditBoat");
					}
				}
				return true;
			}
			if (rpc == 3194773350u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RequestFinishBuilding"));
				}
				using (TimeWarning.New("RequestFinishBuilding"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3194773350u, "RequestFinishBuilding", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3194773350u, "RequestFinishBuilding", this, player, 3f))
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
							RequestFinishBuilding(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RequestFinishBuilding");
					}
				}
				return true;
			}
			if (rpc == 3963850389u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RequestFinishBuildingFromWheel"));
				}
				using (TimeWarning.New("RequestFinishBuildingFromWheel"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3963850389u, "RequestFinishBuildingFromWheel", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3963850389u, "RequestFinishBuildingFromWheel", this, player, 3f))
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
							RequestFinishBuildingFromWheel(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in RequestFinishBuildingFromWheel");
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
							RPCMessage msg7 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_RequestAddLock(msg7);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in RPC_RequestAddLock");
					}
				}
				return true;
			}
			if (rpc == 2818660542u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_TryMountWithKeycode"));
				}
				using (TimeWarning.New("RPC_TryMountWithKeycode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2818660542u, "RPC_TryMountWithKeycode", this, player, 3f))
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
							RPC_TryMountWithKeycode(msg8);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
						player.Kick("RPC Error in RPC_TryMountWithKeycode");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public PlayerBoat GetParentBoat()
	{
		return PlayerBoat.GetParentPlayerBoat(this);
	}

	public override bool DirectlyMountable()
	{
		return true;
	}

	private BoatBuildingStation GetCurrentBoatBuildingStation(BasePlayer player)
	{
		TriggerBoatBuildingArea triggerBoatBuildingArea = player.FindTrigger<TriggerBoatBuildingArea>();
		if ((Object)(object)triggerBoatBuildingArea == (Object)null)
		{
			return null;
		}
		return ((Component)triggerBoatBuildingArea).GetComponentInParent<BoatBuildingStation>();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		BoatLock.Load(info);
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (child is PlayerBoatPrivilege privilege)
		{
			Privilege = privilege;
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		if (BoatLock == null)
		{
			BoatLock = new PlayerBoatLock(this, base.isServer);
		}
	}

	public bool IsFlipped()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Dot(Vector3.up, ((Component)this).transform.up) <= 0.175f;
	}

	public bool IsAuthed(BasePlayer player)
	{
		if ((Object)(object)Privilege == (Object)null)
		{
			return false;
		}
		return Privilege.IsAuthed(player);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Locked, BoatLock != null && BoatLock.HasALock);
		}
		ServerSteeringRotation = 0f;
		CreatePrivilege();
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		base.OnDeployed(parent, deployedBy, fromItem);
		AuthPlayer(deployedBy);
		BoatBuildingStation stationOverlappingPosition = BoatBuildingStation.GetStationOverlappingPosition(((Component)this).transform.position, isServer: true);
		if ((Object)(object)stationOverlappingPosition != (Object)null)
		{
			stationOverlappingPosition.OnSteeringWheelPlaced(this);
		}
		ClientRPC(RpcTarget.Player("CLIENT_OnDeployed", deployedBy));
	}

	internal override void DoServerDestroy()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ParentBoat == (Object)null)
		{
			BoatBuildingStation stationOverlappingPosition = BoatBuildingStation.GetStationOverlappingPosition(((Component)this).transform.position, isServer: true);
			if ((Object)(object)stationOverlappingPosition != (Object)null)
			{
				stationOverlappingPosition.OnSteeringWheelRemoved(this);
			}
		}
		base.DoServerDestroy();
	}

	private void CreatePrivilege()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!Application.isLoadingSave)
		{
			Privilege = null;
			BaseEntity baseEntity = GameManager.server.CreateEntity(PrivPrefab.resourcePath, ((Component)this).transform.position, ((Component)this).transform.rotation);
			if ((Object)(object)baseEntity != (Object)null)
			{
				baseEntity.SetParent(this, worldPositionStays: true);
				baseEntity.Spawn();
			}
			Privilege = baseEntity as PlayerBoatPrivilege;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.steeringWheel = Pool.Get<SteeringWheel>();
		BoatLock.Save(info);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (!base.isServer)
		{
			return;
		}
		BoatLock.PostServerLoad();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Locked, BoatLock.HasALock);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_RequestAddLock(RPCMessage msg)
	{
		if (BoatLock.HasALock)
		{
			return;
		}
		BasePlayer player = msg.player;
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		string code = msg.read.String();
		BoatLock.TryAddALock(code, player);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Locked, BoatLock.HasALock);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_TryMountWithKeycode(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null))
		{
			string codeEntered = msg.read.String();
			if (BoatLock.TryOpenWithCode(player, codeEntered))
			{
				AuthPlayer(player);
				WantsMount(player);
			}
		}
	}

	private void AuthPlayer(BasePlayer player)
	{
		if (!((Object)(object)Privilege == (Object)null) && !Privilege.IsAuthed(player))
		{
			Privilege.AddPlayer(player);
			Privilege.SendNetworkUpdate();
		}
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if ((!BoatLock.HasALock || BoatLock.HasLockPermission(player)) && !IsFlipped() && (!((Object)(object)ParentBoat != (Object)null) || !ParentBoat.IsDying))
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		if (Object.op_Implicit((Object)(object)ParentBoat))
		{
			using FlagsUpdateScope flagsUpdateScope = ParentBoat.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved17, b: true);
		}
		if (_mountedPlayerClipCheck == null)
		{
			_mountedPlayerClipCheck = CheckSeatClipping;
		}
		InvokeRepeatingFixedTime(_mountedPlayerClipCheck);
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		if (Object.op_Implicit((Object)(object)ParentBoat))
		{
			using FlagsUpdateScope flagsUpdateScope = ParentBoat.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved17, b: false);
		}
		if (_mountedPlayerClipCheck == null)
		{
			_mountedPlayerClipCheck = CheckSeatClipping;
		}
		CancelInvokeFixedTime(_mountedPlayerClipCheck);
	}

	private void CheckSeatClipping()
	{
		if (_mountedPlayerClipCheck == null)
		{
			_mountedPlayerClipCheck = CheckSeatClipping;
		}
		if (!AnyMounted())
		{
			CancelInvokeFixedTime(_mountedPlayerClipCheck);
		}
		else if (IsSeatClipping(this))
		{
			DismountAllPlayers();
			CancelInvokeFixedTime(_mountedPlayerClipCheck);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	public void RequestFinishBuilding(RPCMessage msg)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null))
		{
			BoatBuildingStation currentBoatBuildingStation = GetCurrentBoatBuildingStation(player);
			if (!((Object)(object)currentBoatBuildingStation == (Object)null))
			{
				BoatBuildingStation.LogBuildingEvent(((Component)this).transform.position, player, null, "SteeringWheel rquesting FinishBuilding");
				currentBoatBuildingStation.FinishBuilding(msg.player);
			}
		}
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RequestFinishBuildingFromWheel(RPCMessage msg)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		BoatBuildingStation.LogBuildingEvent(((Component)this).transform.position, msg.player, null, "Finish boat requested via steering wheel.");
		BoatBuildingStation currentBoatBuildingStation = GetCurrentBoatBuildingStation(player);
		if (!((Object)(object)currentBoatBuildingStation != (Object)null) || !currentBoatBuildingStation.FinishBuilding(msg.player) || currentBoatBuildingStation.IsStatic)
		{
			return;
		}
		currentBoatBuildingStation.KilledDuringWheelFinish = true;
		currentBoatBuildingStation.Kill();
		if (!((Object)(object)ParentBoat == (Object)null))
		{
			Item item = ItemManager.Create(ParentBoat.BoatBuildingStationItem, 1, 0uL, isServerSide: true, 0uL);
			item.SetItemOwnership(player, ItemOwnershipPhrases.PickedUp);
			player.GiveItem(item, GiveItemReason.PickedUp);
			if (finishBuildingEffect.isValid)
			{
				Effect.server.Run(finishBuildingEffect.resourcePath, this, 0u, default(Vector3), default(Vector3), null, false, null, 0, Effect.Type.Generic);
			}
		}
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RequestEditBoat(RPCMessage msg)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		BoatBuildingStation currentBoatBuildingStation = GetCurrentBoatBuildingStation(player);
		if (!((Object)(object)currentBoatBuildingStation == (Object)null))
		{
			BoatBuildingStation.LogBuildingEvent(((Component)this).transform.position, player, null, "Edit boat requested from steering wheel.");
			if (currentBoatBuildingStation.CanEnterEditMode(player, sendErrorToasts: true))
			{
				currentBoatBuildingStation.EnterEditMode();
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	public void RequestDeployAndEditBoat(RPCMessage msg)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null))
		{
			PlayerBoat parentBoat = GetParentBoat();
			if (!((Object)(object)parentBoat == (Object)null))
			{
				BoatBuildingStation.LogBuildingEvent(((Component)this).transform.position, player, parentBoat, "Deploy and Edit requested via steering wheel.");
				parentBoat.DeployAndEdit(msg.player);
			}
		}
	}

	public override void Hurt(HitInfo info)
	{
		PlayerBoat parentPlayerBoat = PlayerBoat.GetParentPlayerBoat(this);
		if ((Object)(object)parentPlayerBoat != (Object)null && !parentPlayerBoat.IsDestructibleWreck)
		{
			parentPlayerBoat.OnBoatDeployableHurt(this, info);
		}
		else
		{
			base.Hurt(info);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(15uL)]
	[RPC_Server.InputValidation(new Type[] { typeof(float) })]
	public void ReceiveClientRotation(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && !((Object)(object)GetMounted() != (Object)(object)msg.player) && !((Object)(object)ParentBoat == (Object)null))
		{
			float num = (ServerSteeringRotation = msg.read.Float());
			ParentBoat.steering = Mathx.RemapValClamped(num, -170f, 170f, 1f, -1f);
		}
	}

	public void ResetSteering()
	{
		ServerSteeringRotation = 0f;
		ParentBoat.steering = 0f;
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		if ((Object)(object)ParentBoat != (Object)null)
		{
			ParentBoat.ResetTimeSinceUsed();
		}
	}

	void global::IBoatBuildingPiece.OnAddedToBoat(PlayerBoat boat)
	{
		ParentBoat = boat;
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (base.ShouldDisplayPickupOption(player))
		{
			return !PlayerBoat.IsChildOfInteractablePlayerBoat(this);
		}
		return false;
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: ServerSteeringRotation for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_ServerSteeringRotation);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 0)
		{
			try
			{
				_ = __sync_ServerSteeringRotation;
				float _sync_ServerSteeringRotation = reader.Float();
				__sync_ServerSteeringRotation = _sync_ServerSteeringRotation;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "ServerSteeringRotation")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		var (src, num) = netWrite.GetBuffer();
		if (_autosaveBuffer == null)
		{
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		if (_autosaveBuffer.Length < num)
		{
			BaseEntity._autosaveBufferPool.Return(_autosaveBuffer);
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		Buffer.BlockCopy(src, 0, _autosaveBuffer, 0, num);
		save.msg.baseEntity.syncVars = _autosaveBuffer;
		Pool.Free<NetWrite>(ref netWrite);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(netRead);
			Pool.Free<NetRead>(ref netRead);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_ServerSteeringRotation = 0f;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}
}
