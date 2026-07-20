using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class ComputerStation : BaseMountable
{
	public struct ComputerStationPreserveInfo
	{
		public List<string> bookmarks;

		public IOEntity.SlotPreserveInfo inputSlotInfo;

		public IOEntity.SlotPreserveInfo connectedToSlotInfo;
	}

	public const Flags Flag_HasFullControl = Flags.Reserved2;

	[Header("Computer")]
	public GameObjectRef menuPrefab;

	public GameObjectRef ioSubEntityPrefab;

	public ComputerMenu computerMenu;

	public EntityRef currentlyControllingEnt;

	private EntityRef<IOEntity> spawnedIo;

	public List<string> controlBookmarks = new List<string>();

	public Transform leftHandIKPosition;

	public Transform rightHandIKPosition;

	public SoundDefinition turnOnSoundDef;

	public SoundDefinition turnOffSoundDef;

	public SoundDefinition onLoopSoundDef;

	public bool isStatic;

	public float autoGatherRadius;

	public ulong currentPlayerID;

	private static readonly List<ComputerStation> controllingStations = new List<ComputerStation>();

	public float nextAddTime;

	public static readonly char[] BookmarkSplit = new char[1] { ';' };

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ComputerStation.OnRpcMessage"))
		{
			if (rpc == 481778085 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - AddBookmark"));
				}
				using (TimeWarning.New("AddBookmark"))
				{
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
							AddBookmark(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in AddBookmark");
					}
				}
				return true;
			}
			if (rpc == 3509814913u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - AdminForceAddBookmark"));
				}
				using (TimeWarning.New("AdminForceAddBookmark"))
				{
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
							AdminForceAddBookmark(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in AdminForceAddBookmark");
					}
				}
				return true;
			}
			if (rpc == 3237405565u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - AdminForceClearBookmarks"));
				}
				using (TimeWarning.New("AdminForceClearBookmarks"))
				{
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
							AdminForceClearBookmarks(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in AdminForceClearBookmarks");
					}
				}
				return true;
			}
			if (rpc == 4037724546u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - AdminForceDeleteBookmark"));
				}
				using (TimeWarning.New("AdminForceDeleteBookmark"))
				{
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
							AdminForceDeleteBookmark(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in AdminForceDeleteBookmark");
					}
				}
				return true;
			}
			if (rpc == 552248427 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - BeginControllingBookmark"));
				}
				using (TimeWarning.New("BeginControllingBookmark"))
				{
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
							BeginControllingBookmark(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in BeginControllingBookmark");
					}
				}
				return true;
			}
			if (rpc == 2498687923u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DeleteBookmark"));
				}
				using (TimeWarning.New("DeleteBookmark"))
				{
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
							DeleteBookmark(msg7);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in DeleteBookmark");
					}
				}
				return true;
			}
			if (rpc == 2139261430 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_DisconnectControl"));
				}
				using (TimeWarning.New("Server_DisconnectControl"))
				{
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
							Server_DisconnectControl(msg8);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
						player.Kick("RPC Error in Server_DisconnectControl");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool AllowPings()
	{
		BaseEntity baseEntity = currentlyControllingEnt.Get(base.isServer);
		if ((Object)(object)baseEntity != (Object)null && baseEntity is IRemoteControllable { CanPing: not false })
		{
			return true;
		}
		return false;
	}

	public static bool IsValidIdentifier(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return false;
		}
		if (str.Length > 32)
		{
			return false;
		}
		return StringEx.IsAlphaNumeric(str);
	}

	public override void DestroyShared()
	{
		if (base.isServer && Object.op_Implicit((Object)(object)GetMounted()))
		{
			StopControl(GetMounted());
		}
		base.DestroyShared();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Invoke(GatherStaticCameras, 5f);
		if (!isStatic && !Application.isLoadingSave)
		{
			SpawnIOEnt();
		}
	}

	private void SpawnIOEnt()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!isStatic && ioSubEntityPrefab.isValid)
		{
			IOEntity iOEntity = GameManager.server.CreateEntity(ioSubEntityPrefab.resourcePath, ((Component)this).transform.position, ((Component)this).transform.rotation) as IOEntity;
			iOEntity.SetParent(this, worldPositionStays: true);
			spawnedIo.Set(iOEntity);
			iOEntity.Spawn();
		}
	}

	public void GatherStaticCameras()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isLoadingSave)
		{
			Invoke(GatherStaticCameras, 1f);
		}
		else
		{
			if (!isStatic || !(autoGatherRadius > 0f))
			{
				return;
			}
			List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
			Vis.Entities(((Component)this).transform.position, autoGatherRadius, list, 256, (QueryTriggerInteraction)1);
			foreach (BaseEntity item in list)
			{
				IRemoteControllable component = ((Component)item).GetComponent<IRemoteControllable>();
				if (component != null)
				{
					CCTV_RC component2 = ((Component)item).GetComponent<CCTV_RC>();
					if (!((Object)(object)component2 == (Object)null) && component2.IsStatic() && !controlBookmarks.Contains(component.GetIdentifier()))
					{
						ForceAddBookmark(component.GetIdentifier());
					}
				}
			}
			Pool.FreeUnmanaged<BaseEntity>(ref list);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		GatherStaticCameras();
	}

	public static void AddRemoteVoiceListeners(List<Connection> connections, Vector3 position, float range)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (controllingStations.Count == 0)
		{
			return;
		}
		float num = range * range;
		foreach (ComputerStation controllingStation in controllingStations)
		{
			if ((Object)(object)controllingStation == (Object)null)
			{
				continue;
			}
			BasePlayer mounted = controllingStation.GetMounted();
			if ((Object)(object)mounted == (Object)null || mounted.Connection == null)
			{
				continue;
			}
			BaseEntity baseEntity = controllingStation.currentlyControllingEnt.Get(serverside: true);
			if (!((Object)(object)baseEntity == (Object)null))
			{
				Vector3 val = ((Component)baseEntity).transform.position - position;
				if (!(((Vector3)(ref val)).sqrMagnitude > num) && !connections.Contains(mounted.Connection))
				{
					connections.Add(mounted.Connection);
				}
			}
		}
	}

	public void StopControl(BasePlayer ply)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = currentlyControllingEnt.Get(serverside: true);
		if (Object.op_Implicit((Object)(object)baseEntity))
		{
			if (Interface.CallHook("OnBookmarkControlEnd", this, ply, baseEntity) != null)
			{
				return;
			}
			((Component)baseEntity).GetComponent<IRemoteControllable>().StopControl(new CameraViewerId(currentPlayerID, 0L));
		}
		if (Object.op_Implicit((Object)(object)ply))
		{
			ply.net.SwitchSecondaryGroup(null);
		}
		currentlyControllingEnt.uid = default(NetworkableId);
		currentPlayerID = 0uL;
		controllingStations.Remove(this);
		SetFlagLocal(Flags.Reserved2, b: false);
		SendNetworkUpdate();
		SendControlBookmarks(ply);
		CancelInvoke(ControlCheck);
		Interface.CallHook("OnBookmarkControlEnded", this, ply, baseEntity);
		CancelInvoke(CheckCCTVAchievement);
	}

	public bool IsPlayerAtStation(BasePlayer player)
	{
		return (Object)(object)player == (Object)(object)GetMounted();
	}

	[RPC_Server]
	public void AdminForceDeleteBookmark(RPCMessage msg)
	{
		if (msg.player.IsAdmin)
		{
			string identifier = msg.read.String();
			RemoveBookmark(identifier, GetMounted());
		}
	}

	[RPC_Server]
	public void AdminForceClearBookmarks(RPCMessage msg)
	{
		if (msg.player.IsAdmin)
		{
			controlBookmarks.Clear();
			BasePlayer mounted = GetMounted();
			if ((Object)(object)mounted != (Object)null)
			{
				SendControlBookmarks(mounted);
				StopControl(mounted);
			}
		}
	}

	[RPC_Server]
	public void DeleteBookmark(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (IsPlayerAtStation(player) && !isStatic)
		{
			string identifier = msg.read.String();
			RemoveBookmark(identifier, player);
		}
	}

	private void RemoveBookmark(string identifier, BasePlayer mountedPlayer = null)
	{
		if (!IsValidIdentifier(identifier) || !controlBookmarks.Contains(identifier) || Interface.CallHook("OnBookmarkDelete", this, mountedPlayer, identifier) != null)
		{
			return;
		}
		controlBookmarks.Remove(identifier);
		if ((Object)(object)mountedPlayer != (Object)null)
		{
			SendControlBookmarks(mountedPlayer);
			BaseEntity baseEntity = currentlyControllingEnt.Get(serverside: true);
			IRemoteControllable remoteControllable = default(IRemoteControllable);
			if ((Object)(object)baseEntity != (Object)null && ((Component)baseEntity).TryGetComponent<IRemoteControllable>(ref remoteControllable) && remoteControllable.GetIdentifier() == identifier)
			{
				StopControl(mountedPlayer);
			}
		}
	}

	[RPC_Server]
	public void Server_DisconnectControl(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (IsPlayerAtStation(player))
		{
			StopControl(player);
		}
	}

	[RPC_Server]
	public void BeginControllingBookmark(RPCMessage msg)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (!IsPlayerAtStation(player))
		{
			return;
		}
		string text = msg.read.String();
		if (!IsValidIdentifier(text) || !controlBookmarks.Contains(text))
		{
			return;
		}
		IRemoteControllable remoteControllable = RemoteControlEntity.FindByID(text);
		if (remoteControllable == null)
		{
			return;
		}
		BaseEntity ent = remoteControllable.GetEnt();
		if ((Object)(object)ent == (Object)null)
		{
			Debug.LogWarning((object)("RC identifier " + text + " was found but has a null or destroyed entity, this should never happen"));
		}
		else if (remoteControllable.CanControl(player.userID) && !(Vector3.Distance(((Component)this).transform.position, ((Component)ent).transform.position) >= remoteControllable.MaxRange) && Interface.CallHook("OnBookmarkControl", this, player, text, remoteControllable) == null)
		{
			BaseEntity baseEntity = currentlyControllingEnt.Get(serverside: true);
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				IRemoteControllable component = ((Component)baseEntity).GetComponent<IRemoteControllable>();
				component?.StopControl(new CameraViewerId(currentPlayerID, 0L));
				Interface.CallHook("OnBookmarkControlEnded", this, player, component);
			}
			player.net.SwitchSecondaryGroup(ent.net.group);
			currentlyControllingEnt.uid = ent.net.ID;
			currentPlayerID = player.userID;
			if (!controllingStations.Contains(this))
			{
				controllingStations.Add(this);
			}
			bool b = remoteControllable.InitializeControl(new CameraViewerId(currentPlayerID, 0L));
			SetFlagLocal(Flags.Reserved2, b);
			SendNetworkUpdateImmediate();
			SendControlBookmarks(player);
			if (Rust.GameInfo.HasAchievements && remoteControllable.GetEnt() is CCTV_RC)
			{
				InvokeRepeating(CheckCCTVAchievement, 1f, 3f);
			}
			Interface.CallHook("OnBookmarkControlStarted", this, player, text, remoteControllable);
			InvokeRepeating(ControlCheck, 0f, 0f);
		}
	}

	public void CheckCCTVAchievement()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer mounted = GetMounted();
		if (!((Object)(object)mounted != (Object)null))
		{
			return;
		}
		BaseEntity baseEntity = currentlyControllingEnt.Get(serverside: true);
		if (!((Object)(object)baseEntity != (Object)null) || !(baseEntity is CCTV_RC cCTV_RC))
		{
			return;
		}
		foreach (Connection subscriber in mounted.net.secondaryGroup.subscribers)
		{
			if (!subscriber.active)
			{
				continue;
			}
			BasePlayer basePlayer = subscriber.player as BasePlayer;
			if (!((Object)(object)basePlayer == (Object)null))
			{
				Vector3 val = basePlayer.CenterPoint();
				Vector3 val2 = val - cCTV_RC.pitch.position;
				float num = Vector3.Dot(((Vector3)(ref val2)).normalized, cCTV_RC.pitch.forward);
				Vector3 val3 = cCTV_RC.pitch.InverseTransformPoint(val);
				if (num > 0.6f && ((Vector3)(ref val3)).magnitude < 10f)
				{
					mounted.GiveAchievement("BIG_BROTHER");
					CancelInvoke(CheckCCTVAchievement);
					break;
				}
			}
		}
	}

	public bool CanAddBookmark(BasePlayer player)
	{
		if (!IsPlayerAtStation(player))
		{
			return false;
		}
		if (isStatic)
		{
			return false;
		}
		if (Time.realtimeSinceStartup < nextAddTime)
		{
			return false;
		}
		if (controlBookmarks.Count > 3)
		{
			player.ChatMessage("Too many bookmarks, delete some");
			return false;
		}
		return true;
	}

	[RPC_Server]
	public void AdminForceAddBookmark(RPCMessage msg)
	{
		if (msg.player.IsAdmin)
		{
			string identifier = msg.read.String();
			ForceAddBookmark(identifier);
			BasePlayer mounted = GetMounted();
			if ((Object)(object)mounted != (Object)null)
			{
				SendControlBookmarks(mounted);
			}
		}
	}

	public void ForceAddBookmark(string identifier)
	{
		if (controlBookmarks.Count >= 128 || !IsValidIdentifier(identifier) || controlBookmarks.Contains(identifier))
		{
			return;
		}
		IRemoteControllable remoteControllable = RemoteControlEntity.FindByID(identifier);
		if (remoteControllable != null)
		{
			if ((Object)(object)remoteControllable.GetEnt() == (Object)null)
			{
				Debug.LogWarning((object)("RC identifier " + identifier + " was found but has a null or destroyed entity, this should never happen"));
			}
			else
			{
				controlBookmarks.Add(identifier);
			}
		}
	}

	[RPC_Server]
	public void AddBookmark(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!IsPlayerAtStation(player) || isStatic)
		{
			return;
		}
		if (Time.realtimeSinceStartup < nextAddTime)
		{
			player.ChatMessage("Slow down...");
			return;
		}
		if (controlBookmarks.Count >= 128)
		{
			player.ChatMessage("Too many bookmarks, delete some");
			return;
		}
		nextAddTime = Time.realtimeSinceStartup + 1f;
		string text = msg.read.String();
		if (Interface.CallHook("OnBookmarkAdd", this, player, text) == null)
		{
			ForceAddBookmark(text);
			SendControlBookmarks(player);
		}
	}

	public void ControlCheck()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		BaseEntity baseEntity = currentlyControllingEnt.Get(base.isServer);
		BasePlayer mounted = GetMounted();
		if (Object.op_Implicit((Object)(object)baseEntity) && Object.op_Implicit((Object)(object)mounted))
		{
			IRemoteControllable component = ((Component)baseEntity).GetComponent<IRemoteControllable>();
			if (component != null && component.CanControl(mounted.userID) && Vector3.SqrMagnitude(((Component)this).transform.position - ((Component)baseEntity).transform.position) < component.MaxRange * component.MaxRange)
			{
				flag = true;
				mounted.net.SwitchSecondaryGroup(baseEntity.net.group);
			}
		}
		if (!flag)
		{
			StopControl(mounted);
		}
	}

	public string GenerateControlBookmarkString()
	{
		return string.Join(";", controlBookmarks);
	}

	public void SendControlBookmarks(BasePlayer player)
	{
		if (!((Object)(object)player == (Object)null))
		{
			string text = GenerateControlBookmarkString();
			if (Interface.CallHook("OnBookmarksSendControl", this, player, text) == null)
			{
				ClientRPC(RpcTarget.Player("ReceiveBookmarks", player), text);
			}
		}
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		BasePlayer mounted = GetMounted();
		if (Object.op_Implicit((Object)(object)mounted))
		{
			SendControlBookmarks(mounted);
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: true);
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		StopControl(player);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: false);
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		if (spawnedIo.IsValid(base.isServer))
		{
			IOEntity iOEntity = spawnedIo.Get(base.isServer);
			if (((Object)(object)iOEntity != (Object)null) & !iOEntity.IsPowered())
			{
				DismountAllPlayers();
			}
		}
		if (HasFlag(Flags.Reserved2) && currentlyControllingEnt.IsValid(serverside: true) && Interface.CallHook("OnBookmarkInput", this, player, inputState) == null)
		{
			((Component)currentlyControllingEnt.Get(serverside: true)).GetComponent<IRemoteControllable>().UserInput(inputState, new CameraViewerId(player.userID, 0L));
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (!isStatic)
		{
			if (info.msg.simpleUID == null)
			{
				info.msg.simpleUID = Pool.Get<SimpleUID>();
			}
			info.msg.simpleUID.uid = spawnedIo.uid;
		}
		if (!info.forDisk)
		{
			info.msg.ioEntity = Pool.Get<IOEntity>();
			info.msg.ioEntity.genericEntRef1 = currentlyControllingEnt.uid;
		}
		else
		{
			info.msg.computerStation = Pool.Get<ComputerStation>();
			info.msg.computerStation.bookmarks = GenerateControlBookmarkString();
		}
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (CanPlayerSeeMountAnchor(player))
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	public override void Reskin_Preserve(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		base.Reskin_Preserve(ref preserveInfo);
		ref ComputerStationPreserveInfo computerStationPreserve = ref preserveInfo.computerStationPreserve;
		computerStationPreserve.bookmarks = Pool.Get<List<string>>();
		computerStationPreserve.bookmarks.AddRange(controlBookmarks);
		if (spawnedIo.IsValid(serverside: true) && spawnedIo.Get(serverside: true).inputs[0].IsConnected())
		{
			IOEntity.IOSlot iOSlot = spawnedIo.Get(serverside: true).inputs[0];
			iOSlot.Preserve(ref computerStationPreserve.inputSlotInfo);
			computerStationPreserve.inputSlotInfo.connectedTo.outputs[iOSlot.connectedToSlot].Preserve(ref computerStationPreserve.connectedToSlotInfo);
		}
	}

	public override void Reskin_Restore(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		base.Reskin_Restore(ref preserveInfo);
		ref ComputerStationPreserveInfo computerStationPreserve = ref preserveInfo.computerStationPreserve;
		if ((Object)(object)computerStationPreserve.inputSlotInfo.connectedTo != (Object)null && spawnedIo.IsValid(serverside: true))
		{
			IOEntity iOEntity = spawnedIo.Get(serverside: true);
			computerStationPreserve.connectedToSlotInfo.connectedTo = iOEntity;
			iOEntity.inputs[0].Restore(ref computerStationPreserve.inputSlotInfo);
			computerStationPreserve.inputSlotInfo.connectedTo.outputs[computerStationPreserve.inputSlotInfo.connectedToSlot].Restore(ref computerStationPreserve.connectedToSlotInfo);
			HashSet<IOEntity> hashSet = Pool.Get<HashSet<IOEntity>>();
			iOEntity.SnapLinesToHandlePositions(hashSet);
			foreach (IOEntity item in hashSet)
			{
				item.MarkDirtyForceUpdateOutputs();
				item.SendNetworkUpdateImmediate();
				item.NotifyClientsLineChanged();
			}
			Pool.FreeUnmanaged<IOEntity>(ref hashSet);
		}
		controlBookmarks.AddRange(computerStationPreserve.bookmarks);
		Pool.FreeUnmanaged<string>(ref computerStationPreserve.bookmarks);
	}

	private bool CanPlayerSeeMountAnchor(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		return !GamePhysics.CheckCapsule(player.eyes.position, mountAnchor.position + Vector3.up * 0.3f, 0.05f, 2097152, (QueryTriggerInteraction)0);
	}

	public override void Load(LoadInfo info)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (!isStatic && info.msg.simpleUID != null)
		{
			spawnedIo.uid = info.msg.simpleUID.uid;
		}
		if (!info.fromDisk)
		{
			if (info.msg.ioEntity != null)
			{
				currentlyControllingEnt.uid = info.msg.ioEntity.genericEntRef1;
			}
		}
		else
		{
			if (info.msg.computerStation == null)
			{
				return;
			}
			string[] array = info.msg.computerStation.bookmarks.Split(BookmarkSplit, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text in array)
			{
				if (IsValidIdentifier(text))
				{
					controlBookmarks.Add(text);
				}
			}
		}
	}
}
