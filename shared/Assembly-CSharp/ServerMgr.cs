using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CompanionServer;
using ConVar;
using Facepunch;
using Facepunch.Math;
using Facepunch.Models;
using Facepunch.Network;
using Facepunch.Ping;
using Facepunch.Rust;
using Facepunch.Rust.Profiling;
using Ionic.Crc;
using Network;
using Network.Relay;
using Network.Visibility;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Ai.Gen2;
using Rust.Ai.Gen2.Nav;
using SilentOrbit.ProtocolBuffers;
using Steamworks;
using UnityEngine;

public class ServerMgr : SingletonComponent<ServerMgr>, IServerCallback
{
	public ConnectionQueue connectionQueue = new ConnectionQueue();

	public TimeAverageValueLookup<Message.Type> packetHistory = new TimeAverageValueLookup<Message.Type>();

	public TimeAverageValueLookup<uint> rpcHistory = new TimeAverageValueLookup<uint>();

	private Stopwatch timer = new Stopwatch();

	public const string BYPASS_PROCEDURAL_SPAWN_PREF = "bypassProceduralSpawn";

	private ConnectionAuth auth;

	public UserPersistance persistance;

	public PlayerStateManager playerStateManager;

	private AIThinkManager.QueueType aiTick;

	private Stopwatch methodTimer = new Stopwatch();

	private Stopwatch updateTimer = new Stopwatch();

	private RealTimeSinceEx sinceLastPremiumRecheck = 0.0;

	private List<ulong> bannedPlayerNotices = new List<ulong>();

	private string _AssemblyHash;

	private static readonly Memoized<string, (bool Server, bool Player)> _systemConfigTag;

	private GameObject[] proceduralSpawnPoints;

	private GameObject[] fallbackProceduralSpawnPoints;

	public IEnumerator restartCoroutine;

	public static readonly Phrase SERVER_RESTARTING;

	public static readonly Phrase RESTART_INTERRUPTED_PHRASE;

	public bool runFrameUpdate { get; private set; }

	public static int FrameCount { get; private set; }

	public int AvailableSlots => ConVar.Server.maxplayers - BasePlayer.activePlayerList.Count - connectionQueue.ReservedCount;

	private string AssemblyHash
	{
		get
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			if (_AssemblyHash == null)
			{
				string location = typeof(ServerMgr).Assembly.Location;
				if (!string.IsNullOrEmpty(location))
				{
					byte[] array = File.ReadAllBytes(location);
					CRC32 val = new CRC32();
					val.SlurpBlock(array, 0, array.Length);
					_AssemblyHash = val.Crc32Result.ToString("x");
				}
				else
				{
					_AssemblyHash = "il2cpp";
				}
			}
			return _AssemblyHash;
		}
	}

	public bool Restarting => restartCoroutine != null;

	private void Log(Exception e)
	{
		if (Global.developer > 0)
		{
			Debug.LogException(e);
		}
	}

	public void OnNetworkMessage(Message packet)
	{
		if (ConVar.Server.packetlog_enabled)
		{
			packetHistory.Increment(packet.type);
		}
		if (PacketProfiler.enabled)
		{
			PacketProfiler.LogSimpleInbound(packet.type, (int)packet.read.Length);
		}
		switch (packet.type)
		{
		case Message.Type.GiveUserInformation:
			if (packet.connection.GetPacketsPerSecond(packet.type) >= 1)
			{
				Net.sv.Kick(packet.connection, "Packet Flooding: User Information", packet.connection.connected);
				break;
			}
			using (TimeWarning.New("GiveUserInformation", 20))
			{
				try
				{
					OnGiveUserInformation(packet);
				}
				catch (Exception e7)
				{
					Log(e7);
					Net.sv.Kick(packet.connection, "Invalid Packet: User Information");
				}
			}
			packet.connection.AddPacketsPerSecond(packet.type);
			break;
		case Message.Type.Ready:
			if (!packet.connection.connected)
			{
				break;
			}
			if (packet.connection.GetPacketsPerSecond(packet.type) >= 1)
			{
				Net.sv.Kick(packet.connection, "Packet Flooding: Client Ready", packet.connection.connected);
				break;
			}
			using (TimeWarning.New("ClientReady", 20))
			{
				try
				{
					ClientReady(packet);
				}
				catch (Exception e9)
				{
					Log(e9);
					Net.sv.Kick(packet.connection, "Invalid Packet: Client Ready");
				}
			}
			packet.connection.AddPacketsPerSecond(packet.type);
			break;
		case Message.Type.RPCMessage:
			if (!packet.connection.connected)
			{
				break;
			}
			if (packet.connection.GetPacketsPerSecond(packet.type) >= (ulong)ConVar.Server.maxpacketspersecond_rpc)
			{
				Net.sv.Kick(packet.connection, "Packet Flooding: RPC Message");
				break;
			}
			using (TimeWarning.New("OnRPCMessage", 20))
			{
				try
				{
					OnRPCMessage(packet);
				}
				catch (Exception e8)
				{
					Log(e8);
					Net.sv.Kick(packet.connection, "Invalid Packet: RPC Message");
				}
			}
			packet.connection.AddPacketsPerSecond(packet.type);
			break;
		case Message.Type.ConsoleCommand:
			if (!packet.connection.connected)
			{
				break;
			}
			if (packet.connection.GetPacketsPerSecond(packet.type) >= (ulong)ConVar.Server.maxpacketspersecond_command)
			{
				Net.sv.Kick(packet.connection, "Packet Flooding: Client Command", packet.connection.connected);
				break;
			}
			using (TimeWarning.New("OnClientCommand", 20))
			{
				try
				{
					ConsoleNetwork.OnClientCommand(packet);
				}
				catch (Exception e5)
				{
					Log(e5);
					Net.sv.Kick(packet.connection, "Invalid Packet: Client Command");
				}
			}
			packet.connection.AddPacketsPerSecond(packet.type);
			break;
		case Message.Type.DisconnectReason:
			if (!packet.connection.connected)
			{
				break;
			}
			if (packet.connection.GetPacketsPerSecond(packet.type) >= 1)
			{
				Net.sv.Kick(packet.connection, "Packet Flooding: Disconnect Reason", packet.connection.connected);
				break;
			}
			using (TimeWarning.New("ReadDisconnectReason", 20))
			{
				try
				{
					ReadDisconnectReason(packet);
					Net.sv.Disconnect(packet.connection);
				}
				catch (Exception e2)
				{
					Log(e2);
					Net.sv.Kick(packet.connection, "Invalid Packet: Disconnect Reason");
				}
			}
			packet.connection.AddPacketsPerSecond(packet.type);
			break;
		case Message.Type.Tick:
			if (!packet.connection.connected)
			{
				break;
			}
			if (packet.connection.GetPacketsPerSecond(packet.type) >= (ulong)ConVar.Server.maxpacketspersecond_tick)
			{
				Net.sv.Kick(packet.connection, "Packet Flooding: Player Tick", packet.connection.connected);
				break;
			}
			using (TimeWarning.New("OnPlayerTick", 20))
			{
				try
				{
					OnPlayerTick(packet);
				}
				catch (Exception e4)
				{
					Log(e4);
					Net.sv.Kick(packet.connection, "Invalid Packet: Player Tick");
				}
			}
			packet.connection.AddPacketsPerSecond(packet.type);
			break;
		case Message.Type.EAC:
			using (TimeWarning.New("OnEACMessage", 20))
			{
				try
				{
					EACServer.OnMessageReceived(packet);
					break;
				}
				catch (Exception e3)
				{
					Log(e3);
					Net.sv.Kick(packet.connection, "Invalid Packet: EAC");
					break;
				}
			}
		case Message.Type.World:
			if (!World.Transfer || !packet.connection.connected)
			{
				break;
			}
			if (packet.connection.GetPacketsPerSecond(packet.type) >= (ulong)ConVar.Server.maxpacketspersecond_world)
			{
				Net.sv.Kick(packet.connection, "Packet Flooding: World", packet.connection.connected);
				break;
			}
			using (TimeWarning.New("OnWorldMessage", 20))
			{
				try
				{
					WorldNetworking.OnServerMessageReceived(packet);
					break;
				}
				catch (Exception e6)
				{
					Log(e6);
					Net.sv.Kick(packet.connection, "Invalid Packet: World");
					break;
				}
			}
		case Message.Type.VoiceData:
			if (!packet.connection.connected)
			{
				break;
			}
			if (packet.connection.GetPacketsPerSecond(packet.type) >= (ulong)ConVar.Server.maxpacketspersecond_voice)
			{
				Net.sv.Kick(packet.connection, "Packet Flooding: Disconnect Reason", packet.connection.connected);
				break;
			}
			using (TimeWarning.New("OnPlayerVoice", 20))
			{
				try
				{
					OnPlayerVoice(packet);
				}
				catch (Exception e)
				{
					Log(e);
					Net.sv.Kick(packet.connection, "Invalid Packet: Player Voice");
				}
			}
			packet.connection.AddPacketsPerSecond(packet.type);
			break;
		default:
			ProcessUnhandledPacket(packet);
			break;
		}
	}

	public void ProcessUnhandledPacket(Message packet)
	{
		if (Global.developer > 0)
		{
			Debug.LogWarning((object)("[SERVER][UNHANDLED] " + packet.type));
		}
		Net.sv.Kick(packet.connection, "Sent Unhandled Message");
	}

	public void ReadDisconnectReason(Message packet)
	{
		string text = packet.read.String(4096);
		string text2 = packet.connection.ToString();
		if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
		{
			Interface.CallHook("OnClientDisconnect", packet.connection, text);
			DebugEx.Log(text2 + " disconnecting: " + text, (StackTraceLogType)0);
		}
	}

	private BasePlayer SpawnPlayerSleeping(Network.Connection connection)
	{
		BasePlayer basePlayer = BasePlayer.FindSleeping(connection.userid);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return null;
		}
		if (!basePlayer.IsSleeping())
		{
			Debug.LogWarning((object)"Player spawning into sleeper that isn't sleeping!");
			basePlayer.Kill();
			return null;
		}
		basePlayer.PlayerInit(connection);
		basePlayer.inventory.SendSnapshot();
		DebugEx.Log(basePlayer.net.connection.ToString() + " joined [" + basePlayer.net.connection.os + "/" + basePlayer.net.connection.ownerid + "]", (StackTraceLogType)0);
		return basePlayer;
	}

	public BasePlayer SpawnNewPlayer(Network.Connection connection)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(connection.userid);
		BasePlayer.SpawnPoint spawnPoint = FindSpawnPoint(null, playerTeam?.teamID ?? 0);
		BasePlayer basePlayer = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", spawnPoint.pos, spawnPoint.rot).ToPlayer();
		if (Interface.CallHook("OnPlayerSpawn", basePlayer, connection) != null)
		{
			return basePlayer;
		}
		basePlayer.health = 0f;
		basePlayer.lifestate = BaseCombatEntity.LifeState.Dead;
		basePlayer.ResetLifeStateOnSpawn = false;
		basePlayer.limitNetworking = true;
		if (connection == null)
		{
			basePlayer.EnableTransferProtection();
		}
		basePlayer.Spawn();
		basePlayer.limitNetworking = false;
		if (connection != null)
		{
			basePlayer.PlayerInit(connection);
			bool flag = !basePlayer.HasRespawnOptions() && !basePlayer.hasPreviousLife;
			if (Object.op_Implicit((Object)(object)BaseGameMode.GetActiveGameMode(serverside: true)))
			{
				BaseGameMode.GetActiveGameMode(serverside: true).OnNewPlayer(basePlayer, flag);
			}
			else if (flag)
			{
				basePlayer.Respawn();
			}
			DebugEx.Log(string.Format("{0} with steamid {1} joined from ip {2} with platform {3}", new object[4]
			{
				basePlayer.displayName,
				basePlayer.userID.Get(),
				basePlayer.net.connection.ipaddress,
				basePlayer.net.connection.os
			}), (StackTraceLogType)0);
			DebugEx.Log($"\tNetworkId {basePlayer.userID.Get()} is {basePlayer.net.ID} ({basePlayer.displayName})", (StackTraceLogType)0);
			if (basePlayer.net.connection.ownerid != 0L && basePlayer.net.connection.ownerid != basePlayer.net.connection.userid)
			{
				DebugEx.Log($"\t{basePlayer} is sharing the account {basePlayer.net.connection.ownerid}", (StackTraceLogType)0);
			}
		}
		if (playerTeam != null && playerTeam.usePartySpawn && spawnPoint.isProcedualSpawn && playerTeam.firstSpawnLocation == default(Vector3))
		{
			playerTeam.firstSpawnLocation = spawnPoint.pos;
		}
		return basePlayer;
	}

	private void ClientReady(Message packet)
	{
		if (packet.connection.state != Network.Connection.State.Welcoming)
		{
			Net.sv.Kick(packet.connection, "Invalid connection state");
			return;
		}
		ClientReady val = packet.read.Proto<ClientReady>((ClientReady)null);
		try
		{
			foreach (ClientInfo item in val.clientInfo)
			{
				Interface.CallHook("OnPlayerSetInfo", packet.connection, item.name, item.value);
				packet.connection.info.Set(item.name, item.value);
			}
			packet.connection.globalNetworking = val.globalNetworking;
			packet.connection.state = Network.Connection.State.Connected;
			connectionQueue.JoinedGame(packet.connection);
			Facepunch.Rust.Analytics.Azure.OnPlayerConnected(packet.connection);
			AddPartyMembersToTeam(packet.connection, val.party);
			using (TimeWarning.New("ClientReady"))
			{
				BasePlayer basePlayer;
				using (TimeWarning.New("SpawnPlayerSleeping"))
				{
					basePlayer = SpawnPlayerSleeping(packet.connection);
				}
				if ((Object)(object)basePlayer == (Object)null)
				{
					using (TimeWarning.New("SpawnNewPlayer"))
					{
						basePlayer = SpawnNewPlayer(packet.connection);
					}
				}
				basePlayer.SendRespawnOptions();
				basePlayer.LoadClanInfo();
				if ((Object)(object)basePlayer != (Object)null)
				{
					Util.SendSignedInNotification(basePlayer);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		SendReplicatedVars(packet.connection);
	}

	private void AddPartyMembersToTeam(Network.Connection connection, PartyData party)
	{
		if (party != null && party.members != null && party.members.Count != 0 && !string.IsNullOrEmpty(party.joinKey) && !RelationshipManager.ServerInstance.IsPlayerInTeam(connection.userid))
		{
			RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindByJoinKey(party.joinKey);
			if (playerTeam == null)
			{
				playerTeam = RelationshipManager.ServerInstance.CreatePartyTeam(party.joinKey);
				playerTeam.teamLeader = connection.userid;
			}
			playerTeam?.AddPlayer(connection.userid);
		}
	}

	private void OnRPCMessage(Message packet)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		timer.Restart();
		NetworkableId uid = packet.read.EntityID();
		uint num = packet.read.UInt32();
		if (ConVar.Server.rpclog_enabled)
		{
			rpcHistory.Increment(num);
		}
		BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(uid) as BaseEntity;
		if (!((Object)(object)baseEntity == (Object)null))
		{
			if (PacketProfiler.shouldCaptureDetailedProfiling)
			{
				PacketProfiler.LogDetailedInbound(Message.Type.RPCMessage, baseEntity.net.ID, baseEntity.PrefabName, (int)packet.read.Length, null, Epoch.Current, server: true, StringPool.Get(num));
			}
			baseEntity.SV_RPCMessage(num, packet);
			if (timer.Elapsed > RuntimeProfiler.RpcWarningThreshold)
			{
				LagSpikeProfiler.RPC(timer.Elapsed, packet, baseEntity, num);
			}
		}
	}

	private void OnPlayerTick(Message packet)
	{
		BasePlayer basePlayer = NetworkPacketEx.Player(packet);
		if (!((Object)(object)basePlayer == (Object)null))
		{
			basePlayer.OnReceivedTick(packet.read);
		}
	}

	private void OnPlayerVoice(Message packet)
	{
		BasePlayer basePlayer = NetworkPacketEx.Player(packet);
		if (!((Object)(object)basePlayer == (Object)null))
		{
			ArraySegment<byte> arraySegment = packet.read.BytesSegmentWithSize(1048576u);
			if (arraySegment.Count <= ConVar.Server.maxpacketsize_voicedata && Interface.CallHook("OnPlayerVoice", basePlayer, arraySegment) == null)
			{
				basePlayer.OnReceivedVoice(arraySegment);
			}
		}
	}

	private void OnGiveUserInformation(Message packet)
	{
		if (packet.connection.state != Network.Connection.State.Unconnected)
		{
			Net.sv.Kick(packet.connection, "Invalid connection state");
			return;
		}
		packet.connection.state = Network.Connection.State.Connecting;
		if (packet.read.UInt8() != 228)
		{
			Net.sv.Kick(packet.connection, "Invalid Connection Protocol");
			return;
		}
		packet.connection.userid = packet.read.UInt64();
		packet.connection.protocol = packet.read.UInt32();
		packet.connection.os = packet.read.String(128);
		packet.connection.username = packet.read.String();
		if (string.IsNullOrEmpty(packet.connection.os))
		{
			throw new Exception("Invalid OS");
		}
		if (string.IsNullOrEmpty(packet.connection.username))
		{
			Net.sv.Kick(packet.connection, "Invalid Username");
			return;
		}
		packet.connection.username = packet.connection.username.Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ')
			.Trim();
		if (string.IsNullOrEmpty(packet.connection.username))
		{
			Net.sv.Kick(packet.connection, "Invalid Username");
			return;
		}
		string text = string.Empty;
		string branch = ConVar.Server.branch;
		if (packet.read.Unread >= 4)
		{
			text = packet.read.String(128);
		}
		Interface.CallHook("OnClientAuth", packet.connection);
		if (branch != string.Empty && branch != text)
		{
			DebugEx.Log("Kicking " + packet.connection?.ToString() + " - their branch is '" + text + "' not '" + branch + "'", (StackTraceLogType)0);
			Net.sv.Kick(packet.connection, "Wrong Steam Beta: Requires '" + branch + "' branch!");
		}
		else if (packet.connection.protocol > 2632)
		{
			DebugEx.Log("Kicking " + packet.connection?.ToString() + " - their protocol is " + packet.connection.protocol + " not " + 2632, (StackTraceLogType)0);
			Net.sv.Kick(packet.connection, "Wrong Connection Protocol: Server update required!");
		}
		else if (packet.connection.protocol < 2632)
		{
			DebugEx.Log("Kicking " + packet.connection?.ToString() + " - their protocol is " + packet.connection.protocol + " not " + 2632, (StackTraceLogType)0);
			Net.sv.Kick(packet.connection, "Wrong Connection Protocol: Client update required!");
		}
		else
		{
			packet.connection.token = packet.read.BytesWithSize(512u);
			if (packet.connection.token == null || packet.connection.token.Length < 1)
			{
				Net.sv.Kick(packet.connection, "Invalid Token");
				return;
			}
			packet.connection.anticheatId = packet.read.StringRaw(128);
			packet.connection.anticheatToken = packet.read.StringRaw(2048);
			packet.connection.clientChangeset = packet.read.Int32();
			packet.connection.clientBuildTime = packet.read.Int64();
			packet.connection.language = packet.read.String();
			auth.OnNewConnection(packet.connection);
		}
	}

	public bool Initialize(bool loadSave = true, string saveFile = "", bool allowOutOfDateSaves = false, bool skipInitialSpawn = false)
	{
		Interface.CallHook("OnServerInitialize");
		persistance = new UserPersistance(ConVar.Server.rootFolder);
		playerStateManager = new PlayerStateManager(persistance);
		TutorialIsland.GenerateIslandSpawnPoints(loadingSave: true);
		BasePlayer.InitInternalState();
		TriggerParent.InitInternalState();
		if (Object.op_Implicit((Object)(object)SingletonComponent<SpawnHandler>.Instance))
		{
			using (TimeWarning.New("SpawnHandler.UpdateDistributions"))
			{
				SingletonComponent<SpawnHandler>.Instance.UpdateDistributions();
			}
		}
		if (loadSave)
		{
			World.LoadedFromSave = true;
			World.LoadedFromSave = (skipInitialSpawn = SaveRestore.Load(saveFile, allowOutOfDateSaves));
		}
		else
		{
			SaveRestore.SaveCreatedTime = DateTime.UtcNow;
			World.LoadedFromSave = false;
		}
		if (!World.LoadedFromSave)
		{
			SaveRestore.SpawnMapEntities(SaveRestore.FindMapEntities());
		}
		SaveRestore.InitializeWipeId();
		RustRelay.RelayWipeId = SaveRestore.WipeId;
		RustRelay.ServerHostname = ConVar.Server.hostname ?? string.Empty;
		if (Object.op_Implicit((Object)(object)SingletonComponent<SpawnHandler>.Instance))
		{
			if (!skipInitialSpawn)
			{
				using (TimeWarning.New("SpawnHandler.InitialSpawn", 200))
				{
					SingletonComponent<SpawnHandler>.Instance.InitialSpawn();
				}
			}
			using (TimeWarning.New("SpawnHandler.StartSpawnTick", 200))
			{
				SingletonComponent<SpawnHandler>.Instance.StartSpawnTick();
			}
		}
		CreateImportantEntities();
		auth = ((Component)this).GetComponent<ConnectionAuth>();
		Facepunch.Rust.Analytics.StartForServer();
		Dictionary<uint, string> cachedManifest = GameManifest.Current.prefabProperties.Where((GameManifest.PrefabProperties p) => p != null && !string.IsNullOrWhiteSpace(p.name)).ToDictionary((GameManifest.PrefabProperties p) => p.hash, (GameManifest.PrefabProperties p) => p.name);
		Dictionary<uint, string> cachedStringPool = GameManifest.Current.pooledStrings.ToDictionary((GameManifest.PooledString x) => x.hash, (GameManifest.PooledString x) => x.str);
		RustRelay.SetCachedManifest(cachedManifest);
		RustRelay.SetCachedStringPool(cachedStringPool);
		RustRelay.SetCachedMapSnapshot(World.MapFolderName, World.MapFileName);
		RustRelay.SetCachedSnapshot(ConVar.Server.rootFolder, World.SaveFileName);
		RustRelay.ForceSave = () => SaveRestore.Save(AndWait: true);
		RustRelay.EnabledChanged = delegate
		{
			RustRelayFakePlayer.SetDirty();
		};
		RustRelayFakePlayer.SyncWithConfig();
		if (!string.IsNullOrWhiteSpace(RustRelay.Config.ServerUrl) && !string.IsNullOrWhiteSpace(RustRelay.Config.AuthToken))
		{
			RustRelay.AttemptRestart();
		}
		return World.LoadedFromSave;
	}

	public void OpenConnection(bool useSteamServer = true)
	{
		if (ConVar.Server.queryport <= 0 || ConVar.Server.queryport == ConVar.Server.port)
		{
			ConVar.Server.queryport = Math.Max(ConVar.Server.port, RCon.Port) + 1;
		}
		Net.sv.ip = ConVar.Server.ip;
		Net.sv.port = ConVar.Server.port;
		Net.sv.encryption = ConVar.Server.encryption;
		int num = Application.Manifest?.Features?.MinimumSecureEncryption ?? 2;
		if (CommandLine.HasSwitch("-insecure"))
		{
			Net.sv.secure = false;
			Net.sv.encryption = Mathf.Clamp(ConVar.Server.encryption, 0, 1);
		}
		if (Net.sv.secure && Net.sv.encryption < num)
		{
			Debug.LogWarning((object)$"A server requires a minimum 'encryption' value of {num} to be secure and visible in the server browser. To remain secure, increase your 'encryption' convar to {num} and restart your server.");
			Net.sv.secure = false;
		}
		if (useSteamServer)
		{
			StartSteamServer();
		}
		else
		{
			PlatformService.Instance.Initialize((IPlatformHooks)(object)RustPlatformHooks.Instance);
		}
		if (!Net.sv.Start(this))
		{
			Debug.LogWarning((object)"Couldn't Start Server.");
			CloseConnection();
			return;
		}
		RustRelayFakePlayer.SyncWithConfig();
		Net.sv.cryptography = new NetworkCryptographyServer();
		EACServer.DoStartup();
		((MonoBehaviour)this).InvokeRepeating("DoTick", 1f, 1f / (float)ConVar.Server.tickrate);
		((MonoBehaviour)this).InvokeRepeating("DoHeartbeat", 1f, 1f);
		runFrameUpdate = true;
		ConsoleSystem.OnReplicatedVarChanged += OnReplicatedVarChanged;
		Interface.CallHook("IOnServerInitialized");
	}

	public void CloseConnection()
	{
		if (persistance != null)
		{
			persistance.Dispose();
			persistance = null;
		}
		EACServer.DoShutdown();
		Facepunch.Rust.Analytics.ShutdownForServer();
		RustRelayFakePlayer.Shutdown();
		Net.sv.callbackHandler = null;
		using (TimeWarning.New("sv.Stop"))
		{
			Net.sv.Stop("Shutting Down");
		}
		using (TimeWarning.New("RCon.Shutdown"))
		{
			RCon.Shutdown();
		}
		using (TimeWarning.New("PlatformService.Shutdown"))
		{
			try
			{
				IPlatformService instance = PlatformService.Instance;
				if (instance != null)
				{
					instance.Shutdown();
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		using (TimeWarning.New("CompanionServer.Shutdown"))
		{
			CompanionServer.Server.Shutdown();
		}
		using (TimeWarning.New("NexusServer.Shutdown"))
		{
			NexusServer.Shutdown();
		}
		using (TimeWarning.New("ServerOcclusion.Dispose"))
		{
			if (ServerOcclusion.OcclusionEnabled)
			{
				ServerOcclusion.Dispose();
			}
		}
		ConsoleSystem.OnReplicatedVarChanged -= OnReplicatedVarChanged;
		BasePlayer.DisposeInternalState();
		TriggerParent.DisposeInternalState();
	}

	private void OnDisable()
	{
		if (!Application.isQuitting)
		{
			CloseConnection();
		}
	}

	private void OnApplicationQuit()
	{
		Application.isQuitting = true;
		CloseConnection();
	}

	private void CreateImportantEntities()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		CreateImportantEntity<EnvSync>("assets/bundled/prefabs/system/net_env.prefab");
		CreateImportantEntity<CommunityEntity>("assets/bundled/prefabs/system/server/community.prefab");
		CreateImportantEntity<ResourceDepositManager>("assets/bundled/prefabs/system/server/resourcedepositmanager.prefab");
		CreateImportantEntity<RelationshipManager>("assets/bundled/prefabs/system/server/relationship_manager.prefab");
		if (Clan.enabled)
		{
			CreateImportantEntity<ClanManager>("assets/bundled/prefabs/system/server/clan_manager.prefab");
		}
		CreateImportantEntity<TreeManager>("assets/bundled/prefabs/system/tree_manager.prefab");
		CreateImportantEntity<GlobalNetworkHandler>("assets/bundled/prefabs/system/net_global.prefab");
		CreateImportantEntity<CopyPasteEntity>("assets/bundled/prefabs/system/copy_paste.prefab");
		CreateImportantEntity<BuriedItems>("assets/bundled/prefabs/system/server/buried_items.prefab");
		CreateImportantEntity<PowergridManager>("assets/bundled/prefabs/system/powergrid_manager.prefab");
		CreateDeepSea();
	}

	public void CreateDeepSea()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		CreateImportantEntity<DeepSeaManager>("assets/bundled/prefabs/system/deep_sea_manager.prefab", ((Bounds)(ref DeepSeaManager.DeepSeaBounds)).center);
		if (!DeepSea.enabled)
		{
			Physics.SetBounds(Physics.DeepSeaDisabledBounds);
		}
		else
		{
			Physics.SetBounds(Physics.DeepSeaEnabledBounds);
		}
	}

	public void CreateImportantEntity<T>(string prefabName, Vector3 position = default(Vector3)) where T : BaseEntity
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)BaseNetworkable.serverEntities.OfType<T>().FirstOrDefault()))
		{
			Debug.LogWarning((object)("Missing " + typeof(T).Name + " - creating"));
			BaseEntity baseEntity = GameManager.server.CreateEntity(prefabName, position);
			if ((Object)(object)baseEntity == (Object)null)
			{
				Debug.LogWarning((object)"Couldn't create");
			}
			else
			{
				baseEntity.Spawn();
			}
		}
	}

	private void StartSteamServer()
	{
		PlatformService.Instance.Initialize((IPlatformHooks)(object)RustPlatformHooks.Instance);
		((MonoBehaviour)this).InvokeRepeating("UpdateServerInformation", 2f, 30f);
		((MonoBehaviour)this).InvokeRepeating("UpdateItemDefinitions", 10f, 3600f);
		DebugEx.Log("SteamServer Initialized", (StackTraceLogType)0);
	}

	private void UpdateItemDefinitions()
	{
		Debug.Log((object)"Checking for new Steam Item Definitions..");
		PlatformService.Instance.RefreshItemDefinitions();
	}

	internal unsafe void OnValidateAuthTicketResponse(ulong SteamId, ulong OwnerId, AuthResponse Status)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Invalid comparison between Unknown and I4
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Invalid comparison between Unknown and I4
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Invalid comparison between Unknown and I4
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (Auth_Steam.ValidateConnecting(SteamId, OwnerId, Status))
		{
			return;
		}
		Network.Connection connection = Net.sv.connections.FirstOrDefault((Network.Connection x) => x.userid == SteamId);
		if (connection == null)
		{
			Debug.LogWarning((object)$"Steam gave us a {Status} ticket response for unconnected id {SteamId}");
		}
		else if ((int)Status == 2)
		{
			Debug.LogWarning((object)$"Steam gave us a 'ok' ticket response for already connected id {SteamId}");
		}
		else if ((int)Status != 1)
		{
			if (((int)Status == 4 || (int)Status == 3) && !bannedPlayerNotices.Contains(SteamId))
			{
				Interface.CallHook("IOnPlayerBanned", connection, Status);
				ConsoleNetwork.BroadcastToAllClients("chat.add", 2, 0, "<color=#fff>SERVER</color> Kicking " + StringEx.EscapeRichText(connection.username, false) + " (banned by anticheat)");
				bannedPlayerNotices.Add(SteamId);
			}
			Debug.Log((object)string.Format("Kicking {0}/{1}/{2} (Steam Status \"{3}\")", new object[4]
			{
				connection.ipaddress,
				connection.userid,
				connection.username,
				((object)(*(AuthResponse*)(&Status))/*cast due to constrained. prefix*/).ToString()
			}));
			connection.authStatusSteam = ((object)(*(AuthResponse*)(&Status))/*cast due to constrained. prefix*/).ToString();
			Net.sv.Kick(connection, "Steam: " + ((object)(*(AuthResponse*)(&Status))/*cast due to constrained. prefix*/).ToString());
		}
	}

	private void Update()
	{
		if (!runFrameUpdate)
		{
			return;
		}
		updateTimer.Restart();
		FrameCount = Time.frameCount;
		Manifest manifest = Application.Manifest;
		if (manifest != null && manifest.Features.ServerAnalytics)
		{
			try
			{
				PerformanceLogging.server.OnFrame();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		using (TimeWarning.New("ServerMgr.Update", 500))
		{
			try
			{
				using (TimeWarning.New("EACServer.DoUpdate", 100))
				{
					EACServer.DoUpdate();
				}
			}
			catch (Exception ex2)
			{
				Debug.LogWarning((object)"Server Exception: EACServer.DoUpdate");
				Debug.LogException(ex2, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("BuriedItems.DoUpdate", 100))
				{
					if (Object.op_Implicit((Object)(object)BuriedItems.Instance))
					{
						BuriedItems.Instance.DoUpdate();
					}
				}
			}
			catch (Exception ex3)
			{
				Debug.LogWarning((object)"Server Exception: BuriedItems.DoUpdate");
				Debug.LogException(ex3, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("PlatformService.Update", 100))
				{
					PlatformService.Instance.Update();
				}
			}
			catch (Exception ex4)
			{
				Debug.LogWarning((object)"Server Exception: Platform Service Update");
				Debug.LogException(ex4, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("BaseMountable.PlayerSyncCycle"))
				{
					BaseMountable.PlayerSyncCycle();
				}
			}
			catch (Exception ex5)
			{
				Debug.LogWarning((object)"Server Exception: BaseMountable Player Sync Cycle");
				Debug.LogException(ex5, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("Net.sv.Cycle", 100))
				{
					methodTimer.Restart();
					Net.sv.Cycle();
					RuntimeProfiler.Net_Cycle = methodTimer.Elapsed;
				}
			}
			catch (Exception ex6)
			{
				Debug.LogWarning((object)"Server Exception: Network Cycle");
				Debug.LogException(ex6, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("ServerFileRequestQueue.Cycle", 100))
				{
					ServerFileRequestQueue.Cycle();
				}
			}
			catch (Exception ex7)
			{
				Debug.LogWarning((object)"Server Exception: File Request Queue");
				Debug.LogException(ex7, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("ServerBuildingManager.Cycle"))
				{
					BuildingManager.server.Cycle();
				}
			}
			catch (Exception ex8)
			{
				Debug.LogWarning((object)"Server Exception: Building Manager");
				Debug.LogException(ex8, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("BasePlayer.ServerCycle"))
				{
					bool batchsynctransforms = Physics.batchsynctransforms;
					bool autosynctransforms = Physics.autosynctransforms;
					if (batchsynctransforms & autosynctransforms)
					{
						Physics.autoSyncTransforms = false;
					}
					if (!Physics.autoSyncTransforms)
					{
						methodTimer.Restart();
						Physics.SyncTransforms();
						RuntimeProfiler.Physics_SyncTransforms = methodTimer.Elapsed;
					}
					try
					{
						using (TimeWarning.New("CameraRendererManager.Tick", 100))
						{
							CameraRendererManager instance = SingletonComponent<CameraRendererManager>.Instance;
							if ((Object)(object)instance != (Object)null)
							{
								methodTimer.Restart();
								instance.Tick();
								RuntimeProfiler.Companion_Tick = methodTimer.Elapsed;
							}
						}
					}
					catch (Exception ex9)
					{
						Debug.LogWarning((object)"Server Exception: CameraRendererManager.Tick");
						Debug.LogException(ex9, (Object)(object)this);
					}
					methodTimer.Restart();
					BasePlayer.ServerCycle(Time.deltaTime);
					RuntimeProfiler.BasePlayer_ServerCycle = methodTimer.Elapsed;
					try
					{
						using (TimeWarning.New("FlameTurret.BudgetedUpdate"))
						{
							((ObjectWorkQueue<FlameTurret>)FlameTurret.updateFlameTurretQueueServer).RunQueue(0.25);
						}
					}
					catch (Exception ex10)
					{
						Debug.LogWarning((object)"Server Exception: FlameTurret.BudgetedUpdate");
						Debug.LogException(ex10, (Object)(object)this);
					}
					try
					{
						using (TimeWarning.New("AutoTurret.BudgetedUpdate.Scan"))
						{
							((PersistentObjectWorkQueue<AutoTurret>)AutoTurret.updateAutoTurretScanQueue).RunList((double)AutoTurret.scan_budget_ms);
						}
					}
					catch (Exception ex11)
					{
						Debug.LogWarning((object)"Server Exception: AutoTurret.BudgetedUpdate.Scan");
						Debug.LogException(ex11, (Object)(object)this);
					}
					try
					{
						using (TimeWarning.New("AutoTurret.BudgetedUpdate.Ammo"))
						{
							((ObjectWorkQueue<AutoTurret>)AutoTurret.updateAutoTurretAmmoQueue).RunQueue((double)AutoTurret.ammo_update_ms);
						}
					}
					catch (Exception ex12)
					{
						Debug.LogWarning((object)"Server Exception: AutoTurret.BudgetedUpdate.Ammo");
						Debug.LogException(ex12, (Object)(object)this);
					}
					try
					{
						using (TimeWarning.New("AutoTurret.BudgetedUpdate.Tick"))
						{
							((ObjectWorkQueue<AutoTurret>)AutoTurret.updateTurretTick).RunQueue((double)AutoTurret.tick_update_ms);
						}
					}
					catch (Exception ex13)
					{
						Debug.LogWarning((object)"Server Exception: AutoTurret.BudgetedUpdate.Tick");
						Debug.LogException(ex13, (Object)(object)this);
					}
					try
					{
						using (TimeWarning.New("GunTrap.BudgetedUpdate"))
						{
							((PersistentObjectWorkQueue<GunTrap>)GunTrap.updateGunTrapWorkQueue).RunList((double)GunTrap.gun_trap_budget_ms);
						}
					}
					catch (Exception ex14)
					{
						Debug.LogWarning((object)"Server Exception: GunTrap.BudgetedUpdate");
						Debug.LogException(ex14, (Object)(object)this);
					}
					try
					{
						using (TimeWarning.New("BeeSwarmAI.BudgetedUpdate"))
						{
							((ObjectWorkQueue<BeeSwarmAI>)BeeSwarmAI.updateBeeSwarmThink).RunQueue((double)BeeSwarmAI.think_budget_ms);
						}
					}
					catch (Exception ex15)
					{
						Debug.LogWarning((object)"Server Exception: BeeSwarmAI.BudgetedUpdate");
						Debug.LogException(ex15, (Object)(object)this);
					}
					try
					{
						using (TimeWarning.New("BaseFishingRod.BudgetedUpdate"))
						{
							((ObjectWorkQueue<BaseFishingRod>)BaseFishingRod.updateFishingRodQueue).RunQueue(1.0);
						}
					}
					catch (Exception ex16)
					{
						Debug.LogWarning((object)"Server Exception: BaseFishingRod.BudgetedUpdate");
						Debug.LogException(ex16, (Object)(object)this);
					}
					try
					{
						using (TimeWarning.New("DroppedItem.BudgetedUpdate"))
						{
							((PersistentObjectWorkQueue<DroppedItem>)DroppedItem.underwaterStatusQueue).RunList((double)DroppedItem.underwater_drag_budget_ms);
						}
					}
					catch (Exception ex17)
					{
						Debug.LogWarning((object)"Server Exception: DroppedItem.BudgetedUpdate");
						Debug.LogException(ex17, (Object)(object)this);
					}
					try
					{
						using (TimeWarning.New("BaseOven.BudgetedUpdate"))
						{
							((PersistentObjectWorkQueue<BaseOven>)BaseOven.cookQueue).RunList((double)ConVar.Server.ovenCookBudgetMs);
						}
					}
					catch (Exception ex18)
					{
						Debug.LogWarning((object)"Server Exception: BaseOven.BudgetedUpdate");
						Debug.LogException(ex18, (Object)(object)this);
					}
					try
					{
						using (TimeWarning.New("ItemModFoodSpoiling.BudgetedUpdate"))
						{
							if (ConVar.Server.foodSpoiling)
							{
								((PersistentObjectWorkQueue<Item>)ItemModFoodSpoiling.foodSpoilItems).RunList((double)ConVar.Server.foodSpoilingBudgetMs);
							}
						}
					}
					catch (Exception ex19)
					{
						Debug.LogWarning((object)"Server Exception: ItemModFoodSpoiling.BudgetedUpdate");
						Debug.LogException(ex19, (Object)(object)this);
					}
					try
					{
						using (TimeWarning.New("CCTV_RC.BudgetedUpdate"))
						{
							((PersistentObjectWorkQueue<CCTV_RC>)CCTV_RC.WorkQueue).RunList((double)CCTV_RC.inputBudgetMs);
						}
					}
					catch (Exception ex20)
					{
						Debug.LogWarning((object)"Server Exception: CCTV_RC.BudgetedUpdate");
						Debug.LogException(ex20, (Object)(object)this);
					}
					try
					{
						using (TimeWarning.New("ElectricBattery.DischargeQueue"))
						{
							((PersistentObjectWorkQueue<ElectricBattery>)ElectricBattery.DischargeQueue).RunList((double)ElectricBattery.DischargeBudgetMs);
						}
					}
					catch (Exception ex21)
					{
						Debug.LogWarning((object)"Server Exception: ElectricBattery.DischargeQueue");
						Debug.LogException(ex21, (Object)(object)this);
					}
					try
					{
						using (TimeWarning.New("SolarPanel.SunUpdateWorkQueue"))
						{
							((PersistentObjectWorkQueue<SolarPanel>)SolarPanel.WorkQueue).RunList((double)SolarPanel.sunUpdateBudgetMs);
						}
					}
					catch (Exception ex22)
					{
						Debug.LogWarning((object)"Server Exception: SolarPanel.SunUpdateWorkQueue");
						Debug.LogException(ex22, (Object)(object)this);
					}
					try
					{
						using (TimeWarning.New("WaterCatcher.CollectWorkQueue"))
						{
							((PersistentObjectWorkQueue<WaterCatcher>)WaterCatcher.CollectWorkQueue).RunList((double)WaterCatcher.WaterCatcherBudgetMs);
						}
					}
					catch (Exception ex23)
					{
						Debug.LogWarning((object)"Server Exception: WaterCatcher.CollectWorkQueue");
						Debug.LogException(ex23, (Object)(object)this);
					}
					if (batchsynctransforms & autosynctransforms)
					{
						Physics.autoSyncTransforms = true;
					}
				}
			}
			catch (Exception ex24)
			{
				Debug.LogWarning((object)"Server Exception: Player Update");
				Debug.LogException(ex24, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("connectionQueue.Cycle"))
				{
					connectionQueue.Cycle(AvailableSlots);
				}
			}
			catch (Exception ex25)
			{
				Debug.LogWarning((object)"Server Exception: Connection Queue");
				Debug.LogException(ex25, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("IOEntity.ProcessQueue"))
				{
					IOEntity.ProcessQueue();
				}
			}
			catch (Exception ex26)
			{
				Debug.LogWarning((object)"Server Exception: IOEntity.ProcessQueue");
				Debug.LogException(ex26, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("RustNavigation.Tick"))
				{
					RustNavigation.Instance.Tick();
				}
			}
			catch (Exception ex27)
			{
				Debug.LogWarning((object)"Server Exception: RustNavigation.Tick");
				Debug.LogException(ex27, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("NpcManagers.Tick"))
				{
					if ((Object)(object)SingletonComponent<NpcFireManager>.Instance != (Object)null)
					{
						SingletonComponent<NpcFireManager>.Instance.Tick();
					}
					if ((Object)(object)SingletonComponent<NpcNoiseManager>.Instance != (Object)null)
					{
						SingletonComponent<NpcNoiseManager>.Instance.Tick();
					}
					if ((Object)(object)SingletonComponent<NpcCoverManager>.Instance != (Object)null)
					{
						SingletonComponent<NpcCoverManager>.Instance.Tick();
					}
				}
			}
			catch (Exception ex28)
			{
				Debug.LogWarning((object)"Server Exception: NpcManagers.Tick");
				Debug.LogException(ex28, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("FSMComponent.BudgetedUpdate"))
				{
					((PersistentObjectWorkQueue<FSMComponent>)FSMComponent.workQueue).RunList(1.0);
				}
			}
			catch (Exception ex29)
			{
				Debug.LogWarning((object)"Server Exception: FSMComponent.BudgetedUpdate");
				Debug.LogException(ex29, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("RustNavMeshAgent.TickEnabledComponents"))
				{
					RustNavMeshAgent.TickEnabledComponents();
				}
			}
			catch (Exception ex30)
			{
				Debug.LogWarning((object)"Server Exception: RustNavMeshAgent.TickEnabledComponents");
				Debug.LogException(ex30, (Object)(object)this);
			}
			if (!AI.spliceupdates)
			{
				aiTick = AIThinkManager.QueueType.Human;
			}
			else
			{
				aiTick = ((aiTick == AIThinkManager.QueueType.Human) ? AIThinkManager.QueueType.Animal : AIThinkManager.QueueType.Human);
			}
			if (aiTick == AIThinkManager.QueueType.Human)
			{
				try
				{
					using (TimeWarning.New("AIThinkManager.ProcessQueue"))
					{
						AIThinkManager.ProcessQueue(AIThinkManager.QueueType.Human);
					}
				}
				catch (Exception ex31)
				{
					Debug.LogWarning((object)"Server Exception: AIThinkManager.ProcessQueue");
					Debug.LogException(ex31, (Object)(object)this);
				}
				if (!AI.spliceupdates)
				{
					aiTick = AIThinkManager.QueueType.Animal;
				}
			}
			if (aiTick == AIThinkManager.QueueType.Animal)
			{
				try
				{
					using (TimeWarning.New("AIThinkManager.ProcessAnimalQueue"))
					{
						AIThinkManager.ProcessQueue(AIThinkManager.QueueType.Animal);
					}
				}
				catch (Exception ex32)
				{
					Debug.LogWarning((object)"Server Exception: AIThinkManager.ProcessAnimalQueue");
					Debug.LogException(ex32, (Object)(object)this);
				}
			}
			try
			{
				using (TimeWarning.New("AIThinkManager.ProcessPetQueue"))
				{
					AIThinkManager.ProcessQueue(AIThinkManager.QueueType.Pets);
				}
			}
			catch (Exception ex33)
			{
				Debug.LogWarning((object)"Server Exception: AIThinkManager.ProcessPetQueue");
				Debug.LogException(ex33, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("AIThinkManager.ProcessPetMovementQueue"))
				{
					BasePet.ProcessMovementQueue();
				}
			}
			catch (Exception ex34)
			{
				Debug.LogWarning((object)"Server Exception: AIThinkManager.ProcessPetMovementQueue");
				Debug.LogException(ex34, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("BaseSculpture.ProcessSculptureUpdates"))
				{
					BaseSculpture.ProcessSculptureUpdates();
				}
			}
			catch (Exception ex35)
			{
				Debug.LogWarning((object)"Server Exception: BaseSculpture.ProcessGridUpdates");
				Debug.LogException(ex35, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("GrowableEntity.BudgetedUpdate"))
				{
					((ObjectWorkQueue<GrowableEntity>)GrowableEntity.growableEntityUpdateQueue).RunQueue((double)GrowableEntity.framebudgetms);
				}
			}
			catch (Exception ex36)
			{
				Debug.LogWarning((object)"Server Exception: GrowableEntity.BudgetedUpdate");
				Debug.LogException(ex36, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("BasePlayer.BudgetedLifeStoryUpdate"))
				{
					((ObjectWorkQueue<BasePlayer>)BasePlayer.lifeStoryQueue).RunQueue((double)BasePlayer.lifeStoryFramebudgetms);
				}
			}
			catch (Exception ex37)
			{
				Debug.LogWarning((object)"Server Exception: BasePlayer.BudgetedLifeStoryUpdate");
				Debug.LogException(ex37, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("JunkPileWater.UpdateNearbyPlayers"))
				{
					((PersistentObjectWorkQueue<IBudgetedFloatingEntity>)JunkPileWater.junkpileWaterWorkQueue).RunList((double)JunkPileWater.framebudgetms);
				}
			}
			catch (Exception ex38)
			{
				Debug.LogWarning((object)"Server Exception: JunkPileWater.UpdateNearbyPlayers");
				Debug.LogException(ex38, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("IndustrialEntity.RunQueue"))
				{
					if (!SaveRestore.IsSaving || !ConVar.Server.pauseindustrialduringsave)
					{
						((ObjectWorkQueue<IndustrialEntity>)IndustrialEntity.Queue).RunQueue((double)ConVar.Server.industrialFrameBudgetMs);
					}
				}
			}
			catch (Exception ex39)
			{
				Debug.LogWarning((object)"Server Exception: IndustrialEntity.RunQueue");
				Debug.LogException(ex39, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("PowergridManager.StageChangeWorkQueue"))
				{
					PowergridManager.stageChangeWorkQueue.RunList(Powergrid.stageChangeWorkQueueBudget);
				}
			}
			catch (Exception ex40)
			{
				Debug.LogWarning((object)"Server Exception: PowergridManager.StageChangeWorkQueue");
				Debug.LogException(ex40, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("Hopper.WorkQueue"))
				{
					((PersistentObjectWorkQueue<Hopper>)Hopper.WorkQueue).RunList((double)ConVar.Server.hopperAnimationBudgetMs);
				}
			}
			catch (Exception ex41)
			{
				Debug.LogWarning((object)"Server Exception: Hopper.WorkQueue");
				Debug.LogException(ex41, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("AntiHack.Cycle"))
				{
					AntiHack.Cycle();
				}
			}
			catch (Exception ex42)
			{
				Debug.LogWarning((object)"Server Exception: AntiHack.Cycle");
				Debug.LogException(ex42, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("TreeManager.SendPendingTrees"))
				{
					TreeManager.server.SendPendingTrees();
				}
			}
			catch (Exception ex43)
			{
				Debug.LogWarning((object)"Server Exception: TreeManager.SendPendingTrees");
				Debug.LogException(ex43, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("ChickenCoop.CoopWorkQueue"))
				{
					((ObjectWorkQueue<ChickenCoop>)ChickenCoop.CoopWorkQueue).RunQueue(0.10000000149011612);
				}
			}
			catch (Exception ex44)
			{
				Debug.LogWarning((object)"Server Exception: ChickenCoop.CoopWorkQueue");
				Debug.LogException(ex44, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("FarmableAnimal.NeedsWorkQueue"))
				{
					((ObjectWorkQueue<FarmableAnimal>)FarmableAnimal.NeedsWorkQueue).RunQueue(0.10000000149011612);
				}
			}
			catch (Exception ex45)
			{
				Debug.LogWarning((object)"Server Exception: FarmableAnimal.NeedsWorkQueue");
				Debug.LogException(ex45, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("Chicken.EggWorkQueue"))
				{
					((ObjectWorkQueue<Chicken>)Chicken.EggWorkQueue).RunQueue(0.10000000149011612);
				}
			}
			catch (Exception ex46)
			{
				Debug.LogWarning((object)"Server Exception: Chicken.EggWorkQueue");
				Debug.LogException(ex46, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("IndustrialStorageAdaptor.SortQueue"))
				{
					((ObjectWorkQueue<IndustrialStorageAdaptor>)IndustrialStorageAdaptor.SortQueue).RunQueue(0.10000000149011612);
				}
			}
			catch (Exception ex47)
			{
				Debug.LogWarning((object)"Server Exception: IndustrialStorageAdaptor.SortQueue");
				Debug.LogException(ex47, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("BasePlayer.RelationshipUpdateQueue"))
				{
					if (RelationshipManager.contacts)
					{
						((PersistentObjectWorkQueueListBacked<BasePlayer>)BasePlayer.relationshipUpdateQueue).RunList((double)BasePlayer.relationshipUpdateQueueFrameBudgetMs);
					}
				}
			}
			catch (Exception ex48)
			{
				Debug.LogWarning((object)"Server Exception: BasePlayer.RelationshipUpdateQueue");
				Debug.LogException(ex48, (Object)(object)this);
				throw;
			}
			try
			{
				using (TimeWarning.New("TriggerParent.RunOnTick"))
				{
					TriggerParent.RunOnTick();
				}
			}
			catch (Exception ex49)
			{
				Debug.LogWarning((object)"Server Exception: TriggerParent.RunOnTick");
				Debug.LogException(ex49, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("BaseMission.UpdateMissionValidStateWorkQueue"))
				{
					((ObjectWorkQueue<BaseMission.MissionIdentifierData>)BaseMission.updateMissionValidStateWorkQueue).RunQueue((double)BaseMission.missionValidStateWorkQueueBudget);
				}
			}
			catch (Exception ex50)
			{
				Debug.LogWarning((object)"Server Exception: BaseMission.UpdateMissionValidStateWorkQueue");
				Debug.LogException(ex50, (Object)(object)this);
			}
		}
		try
		{
			using (TimeWarning.New("BoatAI.BoatWorkQueue"))
			{
				((ObjectWorkQueue<BoatAI.BoatAIInstruction>)BoatAI.BoatWorkQueue).RunQueue((double)BoatAI.boat_ai_frame_budget_ms);
			}
		}
		catch (Exception ex51)
		{
			Debug.LogWarning((object)"Server Exception: BoatAI.BoatWorkQueue");
			Debug.LogException(ex51, (Object)(object)this);
		}
		try
		{
			using (TimeWarning.New("ElectricWaterWheel.UpdateWorkQueue"))
			{
				((PersistentObjectWorkQueue<ElectricWaterWheel>)ElectricWaterWheel.UpdateWorkQueue).RunList((double)ConVar.Server.waterWheelWorkBudgetMs);
			}
		}
		catch (Exception ex52)
		{
			Debug.LogWarning((object)"Server Exception: DisplayingBoxStorage.UpdateWorkQueue");
			Debug.LogException(ex52, (Object)(object)this);
		}
		RuntimeProfiler.ServerMgr_Update = updateTimer.Elapsed;
	}

	private void LateUpdate()
	{
		if (!runFrameUpdate)
		{
			return;
		}
		using (TimeWarning.New("ServerMgr.LateUpdate", 500))
		{
			if (!SteamNetworking.steamnagleflush)
			{
				return;
			}
			try
			{
				using (TimeWarning.New("Connection.Flush"))
				{
					for (int i = 0; i < Net.sv.connections.Count; i++)
					{
						Net.sv.Flush(Net.sv.connections[i]);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning((object)"Server Exception: Connection.Flush");
				Debug.LogException(ex, (Object)(object)this);
			}
		}
	}

	private void FixedUpdate()
	{
		using (TimeWarning.New("ServerMgr.FixedUpdate"))
		{
			try
			{
				using (TimeWarning.New("BaseMountable.FixedUpdateCycle"))
				{
					BaseMountable.FixedUpdateCycle();
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning((object)"Server Exception: Mountable Cycle");
				Debug.LogException(ex, (Object)(object)this);
			}
			try
			{
				using (TimeWarning.New("Buoyancy.Cycle"))
				{
					Buoyancy.Cycle();
				}
			}
			catch (Exception ex2)
			{
				Debug.LogWarning((object)"Server Exception: Buoyancy Cycle");
				Debug.LogException(ex2, (Object)(object)this);
			}
		}
	}

	private void DoTick()
	{
		Interface.CallHook("OnTick");
		RustRelayFakePlayer.Tick();
		RCon.Update();
		CompanionServer.Server.Update();
		NexusServer.Update();
		for (int i = 0; i < Net.sv.connections.Count; i++)
		{
			Network.Connection connection = Net.sv.connections[i];
			if (!connection.isAuthenticated && !(connection.GetSecondsConnected() < (float)ConVar.Server.authtimeout))
			{
				Net.sv.Kick(connection, "Authentication Timed Out");
			}
		}
		float num = Mathf.Max(ConVar.Server.premiumRecheckInterval, 60f);
		if (ConVar.Server.premium && (double)sinceLastPremiumRecheck > (double)num)
		{
			sinceLastPremiumRecheck = 0.0;
			RecheckPremiumStatus();
		}
	}

	private void DoHeartbeat()
	{
		ItemManager.Heartbeat();
	}

	private void RecheckPremiumStatus()
	{
		float num = Mathf.Clamp(ConVar.Server.premiumRecheckMinSeconds, 60f, 1800f);
		double num2 = Time.realtimeSinceStartupAsDouble - (double)num;
		List<Network.Connection> list = Pool.Get<List<Network.Connection>>();
		foreach (Network.Connection connection in Net.sv.connections)
		{
			if (connection.connected && connection.lastPremiumCheckTime < num2)
			{
				list.Add(connection);
			}
		}
		if (list.Count == 0)
		{
			Pool.FreeUnmanaged<Network.Connection>(ref list);
			return;
		}
		list.Sort((Network.Connection x, Network.Connection y) => x.lastPremiumCheckTime.CompareTo(y.lastPremiumCheckTime));
		int num3 = Mathf.Clamp(ConVar.Server.premiumRecheckMaxBatchSize, 10, 500);
		if (list.Count > num3)
		{
			list.RemoveRange(num3, list.Count - num3);
		}
		RecheckPremiumStatusImpl(list);
		static async void RecheckPremiumStatusImpl(List<Network.Connection> connections)
		{
			try
			{
				List<ulong> steamIds = Pool.Get<List<ulong>>();
				foreach (Network.Connection connection2 in connections)
				{
					steamIds.Add(connection2.userid);
				}
				Dictionary<ulong, bool> dictionary = await PremiumUtil.CheckIfPlayersArePremium(steamIds);
				Pool.FreeUnmanaged<ulong>(ref steamIds);
				double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
				foreach (Network.Connection connection3 in connections)
				{
					if (connection3.connected)
					{
						if (!dictionary.TryGetValue(connection3.userid, out var value))
						{
							Debug.LogWarning((object)$"Missing premium status for {connection3.userid}");
						}
						else
						{
							connection3.lastPremiumCheckTime = realtimeSinceStartupAsDouble;
							if (!value && BasePlayer.TryFindByID(connection3.userid, out var basePlayer))
							{
								basePlayer.Kick("premium_account_required", reserveSlot: false);
							}
						}
					}
				}
				Pool.FreeUnmanaged<Network.Connection>(ref connections);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)"Error rechecking premium status for connected players");
				Debug.LogException(ex);
			}
		}
	}

	private static BaseGameMode Gamemode()
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (!((Object)(object)activeGameMode != (Object)null))
		{
			return null;
		}
		return activeGameMode;
	}

	public static string GamemodeName()
	{
		return Gamemode()?.shortname ?? "rust";
	}

	public static string GamemodeTitle()
	{
		return Gamemode()?.gamemodeTitle ?? "Survival";
	}

	private void UpdateServerInformation()
	{
		if (!SteamServer.IsValid)
		{
			return;
		}
		using (TimeWarning.New("UpdateServerInformation"))
		{
			SteamServer.ServerName = ConVar.Server.hostname;
			SteamServer.MaxPlayers = ConVar.Server.maxplayers;
			SteamServer.Passworded = false;
			SteamServer.MapName = World.GetServerBrowserMapName();
			string text = "stok";
			if (Restarting)
			{
				text = "strst";
			}
			string text2 = $"born{Epoch.FromDateTime(SaveRestore.SaveCreatedTime)}";
			string text3 = $"gm{GamemodeName()}";
			if (text3 != "gmrust" && text3 != "gmvanilla")
			{
				ConVar.Server.tags = ConVar.Server.tags.Replace("vanilla", "");
			}
			string text4 = (ConVar.Server.pve ? ",pve" : string.Empty);
			string text5 = ConVar.Server.tags?.Trim(',') ?? "";
			string text6 = ((!string.IsNullOrWhiteSpace(text5)) ? ("," + text5) : "");
			BuildInfo current = BuildInfo.Current;
			object obj;
			if (current == null)
			{
				obj = null;
			}
			else
			{
				ScmInfo scm = current.Scm;
				obj = ((scm != null) ? scm.ChangeId : null);
			}
			if (obj == null)
			{
				obj = "0";
			}
			string text7 = (string)obj;
			string text8 = (ConVar.Server.premium ? ",premium" : "");
			string text9 = _systemConfigTag.Get((ConVar.Server.useServerWideRequiredSystemConfig, ConVar.Server.usePerPlayerRequiredSystemConfig));
			string text10 = PingEstimater.GetCachedClosestRegion().Code;
			if (!string.IsNullOrEmpty(ConVar.Server.ping_region_code_override))
			{
				text10 = ConVar.Server.ping_region_code_override;
			}
			SteamServer.GameTags = ServerTagCompressor.CompressTags(string.Format("mp{0},cp{1},pt{2},qp{3},$r{4},v{5}{6}{7},{8},{9},cs{10}{11}{12},ts{13}", new object[14]
			{
				ConVar.Server.maxplayers,
				BasePlayer.activePlayerList.Count,
				Net.sv.ProtocolId,
				SingletonComponent<ServerMgr>.Instance.connectionQueue.Queued,
				text10,
				2632,
				text4,
				text6,
				text2,
				text3,
				text7,
				text8,
				text9,
				RelationshipManager.maxTeamSize
			}));
			if (ConVar.Server.description != null && ConVar.Server.description.Length > 100)
			{
				string[] array = StringEx.SplitToChunks(ConVar.Server.description, 100).ToArray();
				for (int i = 0; i < 16; i++)
				{
					if (i < array.Length)
					{
						SteamServer.SetKey($"description_{i:00}", array[i]);
					}
					else
					{
						SteamServer.SetKey($"description_{i:00}", string.Empty);
					}
				}
			}
			else
			{
				SteamServer.SetKey("description_0", ConVar.Server.description);
				for (int j = 1; j < 16; j++)
				{
					SteamServer.SetKey($"description_{j:00}", string.Empty);
				}
			}
			SteamServer.SetKey("hash", AssemblyHash);
			SteamServer.SetKey("status", text);
			string text11 = World.Seed.ToString();
			if (!ConVar.Server.mapenabled || ConVar.Server.fogofwar)
			{
				text11 = "0";
			}
			SteamServer.SetKey("world.seed", text11);
			SteamServer.SetKey("world.size", World.Size.ToString());
			SteamServer.SetKey("pve", ConVar.Server.pve.ToString());
			SteamServer.SetKey("headerimage", ConVar.Server.headerimage);
			SteamServer.SetKey("logoimage", ConVar.Server.logoimage);
			SteamServer.SetKey("url", ConVar.Server.url);
			SteamServer.SetKey("map_image_url", MapUploader.ImageUrl);
			SteamServer.SetKey("level_url", ConVar.Server.levelurl);
			if (!string.IsNullOrWhiteSpace(ConVar.Server.favoritesEndpoint))
			{
				SteamServer.SetKey("favendpoint", ConVar.Server.favoritesEndpoint);
			}
			SteamServer.SetKey("gmn", GamemodeName());
			SteamServer.SetKey("gmt", GamemodeTitle());
			SteamServer.SetKey("uptime", ((int)Time.realtimeSinceStartup).ToString());
			SteamServer.SetKey("gc_mb", Performance.report.memoryAllocations.ToString());
			SteamServer.SetKey("gc_cl", Performance.report.memoryCollections.ToString());
			SteamServer.SetKey("ram_sys", (Performance.report.memoryUsageSystem / 1000000).ToString());
			SteamServer.SetKey("fps", Performance.report.frameRate.ToString());
			SteamServer.SetKey("fps_avg", Performance.report.frameRateAverage.ToString("0.00"));
			SteamServer.SetKey("ent_cnt", BaseNetworkable.serverEntities.Count.ToString());
			SteamServer.SetKey("build", BuildInfo.Current.Scm.ChangeId);
		}
		Interface.CallHook("OnServerInformationUpdated");
	}

	public void OnDisconnected(string strReason, Network.Connection connection)
	{
		Facepunch.Rust.Analytics.Azure.OnPlayerDisconnected(connection, strReason);
		GlobalNetworkHandler.server.OnClientDisconnected(connection);
		ServerFileRequestQueue.OnDisconnected(connection);
		connectionQueue.TryAddReservedSlot(connection);
		connectionQueue.RemoveConnection(connection);
		ConnectionAuth.OnDisconnect(connection);
		if (connection.authStatusSteam == "ok")
		{
			PlatformService.Instance.EndPlayerSession(connection.userid);
		}
		EACServer.OnLeaveGame(connection);
		BasePlayer basePlayer = connection.player as BasePlayer;
		if ((Object)(object)basePlayer != (Object)null)
		{
			Interface.CallHook("OnPlayerDisconnected", basePlayer, strReason);
			basePlayer.OnDisconnected();
		}
		if (connection.authStatusNexus == "ok")
		{
			NexusServer.Logout(connection.userid);
		}
	}

	public static void OnEnterVisibility(Network.Connection connection, Group group)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.IsConnected())
		{
			NetWrite netWrite = Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.GroupEnter);
			netWrite.GroupID(group.ID);
			if (PacketProfiler.shouldCaptureDetailedProfiling)
			{
				PacketProfiler.LogDetailedOutbound(Message.Type.GroupEnter, NetworkableId.EmptyId, null, (int)netWrite.Length, null, Epoch.Current, server: true, "Group: " + group.ID);
			}
			netWrite.Send(new SendInfo(connection));
		}
	}

	public static void OnLeaveVisibility(Network.Connection connection, Group group)
	{
		if (Net.sv.IsConnected())
		{
			NetWrite netWrite = Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.GroupLeave);
			netWrite.GroupID(group.ID);
			netWrite.Send(new SendInfo(connection));
			NetWrite netWrite2 = Net.sv.StartWrite();
			netWrite2.PacketID(Message.Type.GroupDestroy);
			netWrite2.GroupID(group.ID);
			netWrite2.Send(new SendInfo(connection));
		}
	}

	public static BasePlayer.SpawnPoint FindSpawnPoint(BasePlayer forPlayer = null, ulong teamId = 0uL)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		object obj = Interface.CallHook("OnFindSpawnPoint", forPlayer, teamId);
		if (obj is BasePlayer.SpawnPoint)
		{
			return (BasePlayer.SpawnPoint)obj;
		}
		bool flag = false;
		if ((Object)(object)forPlayer != (Object)null && forPlayer.IsInTutorial)
		{
			TutorialIsland currentTutorialIsland = forPlayer.GetCurrentTutorialIsland();
			if ((Object)(object)currentTutorialIsland != (Object)null)
			{
				BasePlayer.SpawnPoint spawnPoint = new BasePlayer.SpawnPoint();
				if (forPlayer.CurrentTutorialAllowance > BasePlayer.TutorialItemAllowance.Level1_HatchetPickaxe)
				{
					spawnPoint.pos = currentTutorialIsland.MidMissionSpawnPoint.position;
					spawnPoint.rot = currentTutorialIsland.MidMissionSpawnPoint.rotation;
				}
				else
				{
					spawnPoint.pos = currentTutorialIsland.InitialSpawnPoint.position;
					spawnPoint.rot = currentTutorialIsland.InitialSpawnPoint.rotation;
				}
				return spawnPoint;
			}
		}
		BaseGameMode baseGameMode = Gamemode();
		if (Object.op_Implicit((Object)(object)baseGameMode) && baseGameMode.useCustomSpawns)
		{
			BasePlayer.SpawnPoint playerSpawn = baseGameMode.GetPlayerSpawn(forPlayer);
			if (playerSpawn != null)
			{
				return playerSpawn;
			}
		}
		if (((Object)(object)SingletonComponent<SpawnHandler>.Instance != (Object)null) & !flag)
		{
			BasePlayer.SpawnPoint spawnPointForTeam = SpawnHandler.GetSpawnPointForTeam(teamId);
			if (spawnPointForTeam != null)
			{
				spawnPointForTeam.isProcedualSpawn = true;
				return spawnPointForTeam;
			}
			BasePlayer.SpawnPoint spawnPoint2 = SpawnHandler.GetSpawnPoint();
			if (spawnPoint2 != null)
			{
				spawnPoint2.isProcedualSpawn = true;
				return spawnPoint2;
			}
		}
		BasePlayer.SpawnPoint spawnPoint3 = new BasePlayer.SpawnPoint();
		if ((Object)(object)forPlayer != (Object)null && forPlayer.IsInTutorial)
		{
			TutorialIsland currentTutorialIsland2 = forPlayer.GetCurrentTutorialIsland();
			if ((Object)(object)currentTutorialIsland2 != (Object)null)
			{
				spawnPoint3.pos = currentTutorialIsland2.InitialSpawnPoint.position;
				spawnPoint3.rot = currentTutorialIsland2.InitialSpawnPoint.rotation;
				return spawnPoint3;
			}
		}
		if (SingletonComponent<ServerMgr>.Instance.proceduralSpawnPoints == null)
		{
			SingletonComponent<ServerMgr>.Instance.proceduralSpawnPoints = GameObject.FindGameObjectsWithTag("spawnpoint");
		}
		if (SingletonComponent<ServerMgr>.Instance.fallbackProceduralSpawnPoints == null)
		{
			SingletonComponent<ServerMgr>.Instance.fallbackProceduralSpawnPoints = GameObject.FindGameObjectsWithTag("SpawnPointFallback");
		}
		if (SingletonComponent<ServerMgr>.Instance.proceduralSpawnPoints != null && SingletonComponent<ServerMgr>.Instance.proceduralSpawnPoints.Length != 0)
		{
			GameObject val = SingletonComponent<ServerMgr>.Instance.proceduralSpawnPoints[Random.Range(0, SingletonComponent<ServerMgr>.Instance.proceduralSpawnPoints.Length)];
			spawnPoint3.pos = val.transform.position;
			spawnPoint3.rot = val.transform.rotation;
		}
		else if (SingletonComponent<ServerMgr>.Instance.fallbackProceduralSpawnPoints != null && SingletonComponent<ServerMgr>.Instance.fallbackProceduralSpawnPoints.Length != 0)
		{
			GameObject val2 = SingletonComponent<ServerMgr>.Instance.fallbackProceduralSpawnPoints[Random.Range(0, SingletonComponent<ServerMgr>.Instance.fallbackProceduralSpawnPoints.Length)];
			spawnPoint3.pos = val2.transform.position;
			spawnPoint3.rot = val2.transform.rotation;
		}
		else
		{
			Debug.Log((object)"Couldn't find an appropriate spawnpoint for the player - so spawning at camera");
			if ((Object)(object)MainCamera.mainCamera != (Object)null)
			{
				spawnPoint3.pos = MainCamera.position;
				spawnPoint3.rot = MainCamera.rotation;
			}
		}
		RaycastHit val3 = default(RaycastHit);
		if (Physics.Raycast(new Ray(spawnPoint3.pos, Vector3.down), ref val3, 32f, 1537286401))
		{
			spawnPoint3.pos = ((RaycastHit)(ref val3)).point;
		}
		return spawnPoint3;
	}

	public void JoinGame(Network.Connection connection)
	{
		Approval val = Pool.Get<Approval>();
		try
		{
			uint num = (uint)Net.sv.encryption;
			if (num > 1 && connection.os == "editor" && DeveloperList.Contains(connection.ownerid))
			{
				num = 1u;
			}
			if (num > 1 && !Net.sv.secure)
			{
				num = 1u;
			}
			val.level = Application.loadedLevelName;
			val.levelConfig = World.Config.JsonString;
			val.levelTransfer = World.Transfer;
			val.levelUrl = World.Url;
			val.levelSeed = World.Seed;
			val.levelSize = World.Size;
			val.checksum = World.Checksum;
			val.hostname = ConVar.Server.hostname;
			val.official = ConVar.Server.official;
			val.encryption = num;
			val.version = BuildInfo.Current.Scm.Branch + "#" + BuildInfo.Current.Scm.ChangeId;
			val.nexus = World.Nexus;
			val.nexusEndpoint = Nexus.endpoint;
			val.nexusId = NexusServer.NexusId.GetValueOrDefault();
			val.dnsEndpoint = ConVar.Server.favoritesEndpoint;
			NetWrite netWrite = Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.Approved);
			ProtoStreamExtensions.WriteToStream((IProto)(object)val, (Stream)netWrite, false, 2097152);
			netWrite.Send(new SendInfo(connection));
			connection.encryptionLevel = num;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		connection.connected = true;
	}

	internal void Shutdown()
	{
		Interface.CallHook("IOnServerShutdown");
		BasePlayer[] array = ((IEnumerable<BasePlayer>)BasePlayer.activePlayerList).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Kick("Server Shutting Down");
		}
		ConsoleSystem.Run(ConsoleSystem.Option.Server, "server.save");
		ConsoleSystem.Run(ConsoleSystem.Option.Server, "server.writecfg");
	}

	private static void ShowToastToAllClients(GameTip.Styles style, Phrase phrase, bool overlay = false, params string[] arguments)
	{
		ConsoleNetwork.BroadcastToAllClients("gametip.showtoast_translated", (int)style, phrase.token, phrase.english, overlay, arguments);
	}

	private IEnumerator ServerRestartWarning(string info, int iSeconds)
	{
		if (iSeconds < 0)
		{
			yield break;
		}
		for (int i = iSeconds; i > 0; i--)
		{
			if (i == iSeconds || i % 60 == 0 || (i < 300 && i % 30 == 0) || (i < 60 && i % 10 == 0) || i < 10)
			{
				ConsoleNetwork.BroadcastToAllClients("baseplayer.showserverrestartwarning", i, info);
				Debug.Log((object)$"Restarting in {i} seconds");
			}
			yield return CoroutineEx.waitForSeconds(1f);
		}
		ShowToastToAllClients(GameTip.Styles.Server_Event, SERVER_RESTARTING, false);
		yield return CoroutineEx.waitForSeconds(2f);
		BasePlayer[] array = ((IEnumerable<BasePlayer>)BasePlayer.activePlayerList).ToArray();
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Kick("Server Restarting");
		}
		yield return CoroutineEx.waitForSeconds(1f);
		ConsoleSystem.Run(ConsoleSystem.Option.Server, "quit");
	}

	public static void RestartServer(string strNotice, int iSeconds)
	{
		if ((Object)(object)SingletonComponent<ServerMgr>.Instance == (Object)null)
		{
			return;
		}
		if (SingletonComponent<ServerMgr>.Instance.restartCoroutine != null)
		{
			if (Interface.CallHook("OnServerRestartInterrupt") != null)
			{
				return;
			}
			ShowToastToAllClients(GameTip.Styles.Server_Event, RESTART_INTERRUPTED_PHRASE, false);
			((MonoBehaviour)SingletonComponent<ServerMgr>.Instance).StopCoroutine(SingletonComponent<ServerMgr>.Instance.restartCoroutine);
			SingletonComponent<ServerMgr>.Instance.restartCoroutine = null;
		}
		if (Interface.CallHook("OnServerRestart", strNotice, iSeconds) == null)
		{
			SingletonComponent<ServerMgr>.Instance.restartCoroutine = SingletonComponent<ServerMgr>.Instance.ServerRestartWarning(strNotice, iSeconds);
			((MonoBehaviour)SingletonComponent<ServerMgr>.Instance).StartCoroutine(SingletonComponent<ServerMgr>.Instance.restartCoroutine);
			SingletonComponent<ServerMgr>.Instance.UpdateServerInformation();
		}
	}

	public static void SendReplicatedVars(string filter)
	{
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		NetWrite netWrite = Net.sv.StartWrite();
		List<Network.Connection> list = Pool.Get<List<Network.Connection>>();
		foreach (Network.Connection connection in Net.sv.connections)
		{
			if (connection.connected)
			{
				list.Add(connection);
			}
		}
		List<ConsoleSystem.Command> list2 = Pool.Get<List<ConsoleSystem.Command>>();
		foreach (ConsoleSystem.Command item in ConsoleSystem.Index.Server.Replicated)
		{
			if (item.FullName.StartsWith(filter))
			{
				list2.Add(item);
			}
		}
		netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
		netWrite.Int32(list2.Count);
		foreach (ConsoleSystem.Command item2 in list2)
		{
			netWrite.String(item2.FullName);
			netWrite.String(item2.String);
		}
		if (PacketProfiler.shouldCaptureDetailedProfiling)
		{
			PacketProfiler.LogDetailedOutbound(Message.Type.ConsoleReplicatedVars, NetworkableId.EmptyId, null, (int)netWrite.Length, null, Epoch.Current, server: true);
		}
		netWrite.Send(new SendInfo(list));
		Pool.FreeUnmanaged<ConsoleSystem.Command>(ref list2);
		Pool.FreeUnmanaged<Network.Connection>(ref list);
	}

	public static void SendReplicatedVars(Network.Connection connection)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		List<ConsoleSystem.Command> replicated = ConsoleSystem.Index.Server.Replicated;
		netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
		netWrite.Int32(replicated.Count);
		foreach (ConsoleSystem.Command item in replicated)
		{
			netWrite.String(item.FullName);
			netWrite.String(item.String);
		}
		netWrite.Send(new SendInfo(connection));
	}

	private static void OnReplicatedVarChanged(string fullName, string value)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		List<Network.Connection> list = Pool.Get<List<Network.Connection>>();
		foreach (Network.Connection connection in Net.sv.connections)
		{
			if (connection.connected)
			{
				list.Add(connection);
			}
		}
		netWrite.PacketID(Message.Type.ConsoleReplicatedVars);
		netWrite.Int32(1);
		netWrite.String(fullName);
		netWrite.String(value);
		netWrite.Send(new SendInfo(list));
		Pool.FreeUnmanaged<Network.Connection>(ref list);
	}

	static ServerMgr()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		_systemConfigTag = new Memoized<string, (bool, bool)>((Func<(bool, bool), string>)(((bool Server, bool Player) t) => (!t.Server && !t.Player) ? null : $",sc{(t.Player ? 2 : 0) | (t.Server ? 1 : 0)}"));
		SERVER_RESTARTING = new Phrase("server.restarting", "Server Restarting!");
		RESTART_INTERRUPTED_PHRASE = new Phrase("server.restart_interrupted", "Server Restart interrupted!");
	}
}
