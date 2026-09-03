using System;
using System.Collections.Concurrent;
using ConVar;
using Epic.OnlineServices;
using Epic.OnlineServices.AntiCheatCommon;
using Epic.OnlineServices.AntiCheatServer;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Reports;
using Network;
using Oxide.Core;
using Unity.Collections;
using UnityEngine;

public static class EACServer
{
	private static AntiCheatServerInterface Interface = null;

	private static ReportsInterface Reports = null;

	private static ConcurrentDictionary<uint, Connection> client2connection = new ConcurrentDictionary<uint, Connection>();

	private static ConcurrentDictionary<Connection, uint> connection2client = new ConcurrentDictionary<Connection, uint>();

	private static ConcurrentDictionary<Connection, AntiCheatCommonClientAuthStatus> connection2status = new ConcurrentDictionary<Connection, AntiCheatCommonClientAuthStatus>();

	private static uint clientHandleCounter = 0u;

	private static bool CanEnableGameplayData => ConVar.Server.eac_gameplay_data;

	public static bool CanSendAnalytics
	{
		get
		{
			if (CanEnableGameplayData)
			{
				return (Handle)(object)Interface != (Handle)null;
			}
			return false;
		}
	}

	private static bool CanSendReports => (Handle)(object)Reports != (Handle)null;

	public static bool ValidInterface => (Handle)(object)Interface != (Handle)null;

	private static IntPtr GenerateCompatibilityClient()
	{
		return (IntPtr)(++clientHandleCounter);
	}

	public unsafe static void Encrypt(Connection connection, ArraySegment<byte> src, ref ArraySegment<byte> dst)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		uint count = (uint)dst.Count;
		dst = new ArraySegment<byte>(dst.Array, dst.Offset, 0);
		if (!((Handle)(object)Interface != (Handle)null))
		{
			return;
		}
		IntPtr client = GetClient(connection);
		if (client != IntPtr.Zero)
		{
			ProtectMessageOptions val = default(ProtectMessageOptions);
			((ProtectMessageOptions)(ref val)).ClientHandle = client;
			((ProtectMessageOptions)(ref val)).Data = src;
			((ProtectMessageOptions)(ref val)).OutBufferSizeBytes = count;
			ProtectMessageOptions val2 = val;
			uint count2 = default(uint);
			Result val3 = Interface.ProtectMessage(ref val2, dst, ref count2);
			if ((int)val3 == 0)
			{
				dst = new ArraySegment<byte>(dst.Array, dst.Offset, (int)count2);
			}
			else
			{
				Debug.LogWarning((object)("[EAC] ProtectMessage failed: " + ((object)(*(Result*)(&val3))/*cast due to constrained. prefix*/).ToString()));
			}
		}
	}

	public unsafe static void Decrypt(Connection connection, ArraySegment<byte> src, ref ArraySegment<byte> dst)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		uint count = (uint)dst.Count;
		dst = new ArraySegment<byte>(dst.Array, dst.Offset, 0);
		if (!((Handle)(object)Interface != (Handle)null))
		{
			return;
		}
		IntPtr client = GetClient(connection);
		if (client != IntPtr.Zero)
		{
			UnprotectMessageOptions val = default(UnprotectMessageOptions);
			((UnprotectMessageOptions)(ref val)).ClientHandle = client;
			((UnprotectMessageOptions)(ref val)).Data = src;
			((UnprotectMessageOptions)(ref val)).OutBufferSizeBytes = count;
			UnprotectMessageOptions val2 = val;
			uint count2 = default(uint);
			Result val3 = Interface.UnprotectMessage(ref val2, dst, ref count2);
			if ((int)val3 == 0)
			{
				dst = new ArraySegment<byte>(dst.Array, dst.Offset, (int)count2);
			}
			else
			{
				Debug.LogWarning((object)("[EAC] UnprotectMessage failed: " + ((object)(*(Result*)(&val3))/*cast due to constrained. prefix*/).ToString()));
			}
		}
	}

	public static IntPtr GetClient(Connection connection)
	{
		connection2client.TryGetValue(connection, out var value);
		return (IntPtr)value;
	}

	private static Connection GetConnection(IntPtr client)
	{
		client2connection.TryGetValue((uint)(int)client, out var value);
		return value;
	}

	public static bool IsAuthenticated(Connection connection)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		connection2status.TryGetValue(connection, out var value);
		return (int)value == 2;
	}

	private static void OnAuthenticatedLocal(Connection connection)
	{
		if (!ConVar.Server.strictauth_eac && connection.authStatusEAC == string.Empty)
		{
			connection.authStatusEAC = "ok";
		}
		connection2status[connection] = (AntiCheatCommonClientAuthStatus)1;
	}

	private static void OnAuthenticatedRemote(Connection connection)
	{
		if (ConVar.Server.strictauth_eac && connection.authStatusEAC == string.Empty)
		{
			connection.authStatusEAC = "ok";
		}
		connection2status[connection] = (AntiCheatCommonClientAuthStatus)2;
	}

	private static void OnVerifyIdToken(ref VerifyIdTokenCallbackInfo data)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (!ConVar.Server.anticheattoken)
		{
			Debug.LogWarning((object)"[EAC] Verify ID token skipped: server.anticheattoken == false");
			return;
		}
		IntPtr client = (IntPtr)((VerifyIdTokenCallbackInfo)(ref data)).ClientData;
		Connection connection = GetConnection(client);
		if (connection == null)
		{
			Debug.LogError((object)("[EAC] Verify ID token for invalid client: " + client));
			return;
		}
		if (connection.IsDevelopmentBuild())
		{
			Debug.LogWarning((object)("[EAC] Verify ID token skipped for unprotected client: " + connection.ToString()));
			return;
		}
		if ((int)((VerifyIdTokenCallbackInfo)(ref data)).ResultCode != 0)
		{
			string text = "Verify ID token " + ((object)((VerifyIdTokenCallbackInfo)(ref data)).ResultCode/*cast due to constrained. prefix*/).ToString();
			Debug.Log((object)$"[EAC] Kicking {connection.userid} / {connection.username} ({text})");
			connection.authStatusEAC = "eactoken";
			Net.sv.Kick(connection, "EAC: " + text);
			return;
		}
		string text2 = ((object)((VerifyIdTokenCallbackInfo)(ref data)).AccountId).ToString();
		string text3 = connection.userid.ToString();
		if (text2 != text3)
		{
			string text4 = "Verify ID token account mismatch with " + text2 + " != " + text3;
			Debug.Log((object)$"[EAC] Kicking {connection.userid} / {connection.username} ({text4})");
			connection.authStatusEAC = "eactoken";
			Net.sv.Kick(connection, "EAC: " + text4);
		}
	}

	private static void OnClientAuthStatusChanged(ref OnClientAuthStatusChangedCallbackInfo data)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		using (TimeWarning.New("AntiCheatKickPlayer", 10))
		{
			IntPtr clientHandle = ((OnClientAuthStatusChangedCallbackInfo)(ref data)).ClientHandle;
			Connection connection = GetConnection(clientHandle);
			if (connection == null)
			{
				Debug.LogError((object)("[EAC] Status update for invalid client: " + clientHandle));
			}
			else if ((int)((OnClientAuthStatusChangedCallbackInfo)(ref data)).ClientAuthStatus == 1)
			{
				OnAuthenticatedLocal(connection);
				SetClientNetworkStateOptions val = default(SetClientNetworkStateOptions);
				((SetClientNetworkStateOptions)(ref val)).ClientHandle = clientHandle;
				((SetClientNetworkStateOptions)(ref val)).IsNetworkActive = false;
				SetClientNetworkStateOptions val2 = val;
				Interface.SetClientNetworkState(ref val2);
			}
			else if ((int)((OnClientAuthStatusChangedCallbackInfo)(ref data)).ClientAuthStatus == 2)
			{
				OnAuthenticatedRemote(connection);
				IdToken val3 = default(IdToken);
				((IdToken)(ref val3)).ProductUserId = ProductUserId.FromString(Utf8String.op_Implicit(connection.anticheatId));
				((IdToken)(ref val3)).JsonWebToken = Utf8String.op_Implicit(connection.anticheatToken);
				IdToken val4 = val3;
				EOS.VerifyIdToken(clientHandle, val4, new OnVerifyIdTokenCallback(OnVerifyIdToken));
			}
		}
	}

	private static void OnClientActionRequired(ref OnClientActionRequiredCallbackInfo data)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Invalid comparison between Unknown and I4
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Invalid comparison between Unknown and I4
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Invalid comparison between Unknown and I4
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("OnClientActionRequired", 10))
		{
			IntPtr clientHandle = ((OnClientActionRequiredCallbackInfo)(ref data)).ClientHandle;
			Connection connection = GetConnection(clientHandle);
			if (connection == null)
			{
				Debug.LogError((object)("[EAC] Status update for invalid client: " + clientHandle));
			}
			else
			{
				if ((int)((OnClientActionRequiredCallbackInfo)(ref data)).ClientAction != 1)
				{
					return;
				}
				Utf8String actionReasonDetailsString = ((OnClientActionRequiredCallbackInfo)(ref data)).ActionReasonDetailsString;
				if (connection.IsDevelopmentBuild())
				{
					Debug.LogWarning((object)("[EAC] Remove player action skipped for unprotected client: " + connection.ToString()));
					return;
				}
				Debug.Log((object)$"[EAC] Kicking {connection.userid} / {connection.username} ({actionReasonDetailsString})");
				connection.authStatusEAC = "eac";
				Net.sv.Kick(connection, Utf8String.op_Implicit(Utf8String.op_Implicit("EAC: ") + actionReasonDetailsString));
				Oxide.Core.Interface.CallHook("OnPlayerKicked", connection, actionReasonDetailsString.ToString());
				if ((int)((OnClientActionRequiredCallbackInfo)(ref data)).ActionReasonCode == 10 || (int)((OnClientActionRequiredCallbackInfo)(ref data)).ActionReasonCode == 9)
				{
					connection.authStatusEAC = "eacbanned";
					ConsoleNetwork.BroadcastToAllClients("chat.add", 2, 0, "<color=#fff>SERVER</color> Kicking " + connection.username + " (banned by anticheat)");
					Oxide.Core.Interface.CallHook("OnPlayerBanned", connection, actionReasonDetailsString.ToString());
					if ((int)((OnClientActionRequiredCallbackInfo)(ref data)).ActionReasonCode == 10)
					{
						Entity.DeleteBy(connection.userid);
					}
				}
				UnregisterClientOptions val = default(UnregisterClientOptions);
				((UnregisterClientOptions)(ref val)).ClientHandle = clientHandle;
				UnregisterClientOptions val2 = val;
				Interface.UnregisterClient(ref val2);
				client2connection.TryRemove((uint)(int)clientHandle, out var _);
				connection2client.TryRemove(connection, out var _);
				connection2status.TryRemove(connection, out var _);
			}
		}
	}

	private static void SendToClient(ref OnMessageToClientCallbackInfo data)
	{
		IntPtr clientHandle = ((OnMessageToClientCallbackInfo)(ref data)).ClientHandle;
		Connection connection = GetConnection(clientHandle);
		if (connection == null)
		{
			Debug.LogError((object)("[EAC] Network packet for invalid client: " + clientHandle));
			return;
		}
		NetWrite netWrite = Net.sv.StartWrite();
		netWrite.PacketID(Message.Type.EAC);
		netWrite.UInt32((uint)((OnMessageToClientCallbackInfo)(ref data)).MessageData.Count);
		netWrite.Write(((OnMessageToClientCallbackInfo)(ref data)).MessageData.Array, ((OnMessageToClientCallbackInfo)(ref data)).MessageData.Offset, ((OnMessageToClientCallbackInfo)(ref data)).MessageData.Count);
		netWrite.Send(new SendInfo(connection));
	}

	public static void DoStartup()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected O, but got Unknown
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.secure && !Application.isEditor)
		{
			client2connection.Clear();
			connection2client.Clear();
			connection2status.Clear();
			AddNotifyClientActionRequiredOptions val = default(AddNotifyClientActionRequiredOptions);
			AddNotifyClientAuthStatusChangedOptions val2 = default(AddNotifyClientAuthStatusChangedOptions);
			AddNotifyMessageToClientOptions val3 = default(AddNotifyMessageToClientOptions);
			BeginSessionOptions val4 = default(BeginSessionOptions);
			((BeginSessionOptions)(ref val4)).LocalUserId = null;
			((BeginSessionOptions)(ref val4)).EnableGameplayData = CanEnableGameplayData;
			((BeginSessionOptions)(ref val4)).RegisterTimeoutSeconds = 20u;
			((BeginSessionOptions)(ref val4)).ServerName = Utf8String.op_Implicit(ConVar.Server.hostname);
			BeginSessionOptions val5 = val4;
			LogGameRoundStartOptions val6 = default(LogGameRoundStartOptions);
			((LogGameRoundStartOptions)(ref val6)).LevelName = Utf8String.op_Implicit(World.Name);
			LogGameRoundStartOptions val7 = val6;
			EOS.Initialize(true, ConVar.Server.anticheatid, ConVar.Server.anticheatkey, ConVar.Server.rootFolder + "/Log.EAC.txt");
			Interface = EOS.Interface.GetAntiCheatServerInterface();
			Interface.AddNotifyClientActionRequired(ref val, (object)null, new OnClientActionRequiredCallback(OnClientActionRequired));
			Interface.AddNotifyClientAuthStatusChanged(ref val2, (object)null, new OnClientAuthStatusChangedCallback(OnClientAuthStatusChanged));
			Interface.AddNotifyMessageToClient(ref val3, (object)null, new OnMessageToClientCallback(SendToClient));
			Interface.BeginSession(ref val5);
			Interface.LogGameRoundStart(ref val7);
			if (CanSendAnalytics)
			{
				BasePlayer.EACTickStates = new NativeArray<BasePlayer.EACTickState>(32, (Allocator)4, (NativeArrayOptions)1);
			}
			if (ValidInterface)
			{
				BasePlayer.ClientHandles = new NativeArray<IntPtr>(32, (Allocator)4, (NativeArrayOptions)0);
			}
		}
		else
		{
			client2connection.Clear();
			connection2client.Clear();
			connection2status.Clear();
		}
	}

	public static void DoUpdate()
	{
		if (Net.sv.secure && !Application.isEditor)
		{
			EOS.Tick();
		}
	}

	public static void DoShutdown()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.secure && !Application.isEditor)
		{
			client2connection.Clear();
			connection2client.Clear();
			connection2status.Clear();
			if ((Handle)(object)Interface != (Handle)null)
			{
				Debug.Log((object)"EasyAntiCheat Server Shutting Down");
				EndSessionOptions val = default(EndSessionOptions);
				Interface.EndSession(ref val);
				Interface = null;
				EOS.Shutdown();
			}
		}
		else
		{
			client2connection.Clear();
			connection2client.Clear();
			connection2status.Clear();
		}
	}

	public static void OnLeaveGame(Connection connection)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		AntiCheatCommonClientAuthStatus value3;
		if (Net.sv.secure && !Application.isEditor)
		{
			if ((Handle)(object)Interface != (Handle)null)
			{
				IntPtr client = GetClient(connection);
				if (client != IntPtr.Zero)
				{
					UnregisterClientOptions val = default(UnregisterClientOptions);
					((UnregisterClientOptions)(ref val)).ClientHandle = client;
					UnregisterClientOptions val2 = val;
					Interface.UnregisterClient(ref val2);
					client2connection.TryRemove((uint)(int)client, out var _);
				}
				connection2client.TryRemove(connection, out var _);
				connection2status.TryRemove(connection, out value3);
			}
		}
		else
		{
			connection2status.TryRemove(connection, out value3);
		}
	}

	public static void OnJoinGame(Connection connection, EAC.SystemConfig requiredSystemConfig)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Invalid comparison between Unknown and I4
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.secure && !Application.isEditor)
		{
			if (!((Handle)(object)Interface != (Handle)null))
			{
				return;
			}
			IntPtr intPtr = GenerateCompatibilityClient();
			if (intPtr == IntPtr.Zero)
			{
				Debug.LogError((object)("[EAC] GenerateCompatibilityClient returned invalid client: " + intPtr));
				return;
			}
			RegisterClientOptions val = default(RegisterClientOptions);
			((RegisterClientOptions)(ref val)).ClientHandle = intPtr;
			((RegisterClientOptions)(ref val)).UserId = ProductUserId.FromString(Utf8String.op_Implicit(connection.anticheatId));
			((RegisterClientOptions)(ref val)).IpAddress = Utf8String.op_Implicit(connection.IPAddressWithoutPort());
			((RegisterClientOptions)(ref val)).ClientType = (AntiCheatCommonClientType)(connection.IsDevelopmentBuild() ? 1 : 0);
			((RegisterClientOptions)(ref val)).ClientPlatform = (AntiCheatCommonClientPlatform)((connection.os == "windows") ? 1 : ((connection.os == "linux") ? 3 : ((connection.os == "mac") ? 2 : 0)));
			((RegisterClientOptions)(ref val)).Reserved01 = (int)((!connection.IsDevelopmentBuild()) ? requiredSystemConfig : EAC.SystemConfig.None);
			RegisterClientOptions val2 = val;
			if ((int)((RegisterClientOptions)(ref val2)).ClientType == 1)
			{
				Debug.LogWarning((object)("[EAC] Joining game as unprotected client: " + connection.ToString()));
			}
			SetClientDetailsOptions val3 = default(SetClientDetailsOptions);
			((SetClientDetailsOptions)(ref val3)).ClientHandle = intPtr;
			((SetClientDetailsOptions)(ref val3)).ClientFlags = (AntiCheatCommonClientFlags)((connection.authLevel != 0) ? 1 : 0);
			SetClientDetailsOptions val4 = val3;
			Interface.RegisterClient(ref val2);
			Interface.SetClientDetails(ref val4);
			client2connection.TryAdd((uint)(int)intPtr, connection);
			connection2client.TryAdd(connection, (uint)(int)intPtr);
			connection2status.TryAdd(connection, (AntiCheatCommonClientAuthStatus)0);
		}
		else
		{
			connection2status.TryAdd(connection, (AntiCheatCommonClientAuthStatus)0);
			OnAuthenticatedLocal(connection);
			OnAuthenticatedRemote(connection);
		}
	}

	public static void OnStartLoading(Connection connection)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if ((Handle)(object)Interface != (Handle)null)
		{
			IntPtr client = GetClient(connection);
			if (client != IntPtr.Zero)
			{
				SetClientNetworkStateOptions val = default(SetClientNetworkStateOptions);
				((SetClientNetworkStateOptions)(ref val)).ClientHandle = client;
				((SetClientNetworkStateOptions)(ref val)).IsNetworkActive = false;
				SetClientNetworkStateOptions val2 = val;
				Interface.SetClientNetworkState(ref val2);
			}
		}
	}

	public static void OnFinishLoading(Connection connection)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if ((Handle)(object)Interface != (Handle)null)
		{
			IntPtr client = GetClient(connection);
			if (client != IntPtr.Zero)
			{
				SetClientNetworkStateOptions val = default(SetClientNetworkStateOptions);
				((SetClientNetworkStateOptions)(ref val)).ClientHandle = client;
				((SetClientNetworkStateOptions)(ref val)).IsNetworkActive = true;
				SetClientNetworkStateOptions val2 = val;
				Interface.SetClientNetworkState(ref val2);
			}
		}
	}

	public static void OnMessageReceived(Message message)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		IntPtr client = GetClient(message.connection);
		byte[] buffer;
		int size;
		if (client == IntPtr.Zero)
		{
			Debug.LogError((object)("EAC network packet from invalid connection: " + message.connection.userid));
		}
		else if (message.read.TemporaryBytesWithSize(out buffer, out size))
		{
			ReceiveMessageFromClientOptions val = default(ReceiveMessageFromClientOptions);
			((ReceiveMessageFromClientOptions)(ref val)).ClientHandle = client;
			((ReceiveMessageFromClientOptions)(ref val)).Data = new ArraySegment<byte>(buffer, 0, size);
			ReceiveMessageFromClientOptions val2 = val;
			Interface.ReceiveMessageFromClient(ref val2);
		}
	}

	public static void LogPlayerUseWeapon(BasePlayer player, BaseProjectile weapon)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		if (CanSendAnalytics && player.net.connection != null)
		{
			using (TimeWarning.New("EAC.LogPlayerShooting"))
			{
				Vector3 networkPosition = player.GetNetworkPosition();
				Quaternion networkRotation = player.GetNetworkRotation();
				Item item = weapon.GetItem();
				string text = ((item != null) ? item.info.shortname : "unknown");
				LogPlayerUseWeaponOptions val = default(LogPlayerUseWeaponOptions);
				LogPlayerUseWeaponData value = default(LogPlayerUseWeaponData);
				((LogPlayerUseWeaponData)(ref value)).PlayerHandle = GetClient(player.net.connection);
				Vec3f value2 = default(Vec3f);
				((Vec3f)(ref value2)).x = networkPosition.x;
				((Vec3f)(ref value2)).y = networkPosition.y;
				((Vec3f)(ref value2)).z = networkPosition.z;
				((LogPlayerUseWeaponData)(ref value)).PlayerPosition = value2;
				Quat value3 = default(Quat);
				((Quat)(ref value3)).w = networkRotation.w;
				((Quat)(ref value3)).x = networkRotation.x;
				((Quat)(ref value3)).y = networkRotation.y;
				((Quat)(ref value3)).z = networkRotation.z;
				((LogPlayerUseWeaponData)(ref value)).PlayerViewRotation = value3;
				((LogPlayerUseWeaponData)(ref value)).WeaponName = Utf8String.op_Implicit(text);
				((LogPlayerUseWeaponOptions)(ref val)).UseWeaponData = value;
				Interface.LogPlayerUseWeapon(ref val);
			}
		}
	}

	public static void LogPlayerSpawn(BasePlayer player)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (CanSendAnalytics && player.net.connection != null)
		{
			using (TimeWarning.New("EAC.LogPlayerSpawn"))
			{
				LogPlayerSpawnOptions val = default(LogPlayerSpawnOptions);
				((LogPlayerSpawnOptions)(ref val)).SpawnedPlayerHandle = GetClient(player.net.connection);
				Interface.LogPlayerSpawn(ref val);
			}
		}
	}

	public static void LogPlayerDespawn(BasePlayer player)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (CanSendAnalytics && player.net.connection != null)
		{
			using (TimeWarning.New("EAC.LogPlayerDespawn"))
			{
				LogPlayerDespawnOptions val = default(LogPlayerDespawnOptions);
				((LogPlayerDespawnOptions)(ref val)).DespawnedPlayerHandle = GetClient(player.net.connection);
				Interface.LogPlayerDespawn(ref val);
			}
		}
	}

	public static void LogPlayerTakeDamage(BasePlayer player, HitInfo info, bool wasWounded)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		if (!CanSendAnalytics || !((Object)(object)info.Initiator != (Object)null) || !(info.Initiator is BasePlayer))
		{
			return;
		}
		BasePlayer basePlayer = info.Initiator.ToPlayer();
		if (player.net.connection == null || basePlayer.net.connection == null)
		{
			return;
		}
		using (TimeWarning.New("EAC.LogPlayerTakeDamage"))
		{
			LogPlayerTakeDamageOptions val = default(LogPlayerTakeDamageOptions);
			LogPlayerUseWeaponData value = default(LogPlayerUseWeaponData);
			((LogPlayerTakeDamageOptions)(ref val)).AttackerPlayerHandle = GetClient(basePlayer.net.connection);
			((LogPlayerTakeDamageOptions)(ref val)).VictimPlayerHandle = GetClient(player.net.connection);
			((LogPlayerTakeDamageOptions)(ref val)).DamageTaken = info.damageTypes.Total();
			Vec3f value2 = default(Vec3f);
			((Vec3f)(ref value2)).x = info.HitPositionWorld.x;
			((Vec3f)(ref value2)).y = info.HitPositionWorld.y;
			((Vec3f)(ref value2)).z = info.HitPositionWorld.z;
			((LogPlayerTakeDamageOptions)(ref val)).DamagePosition = value2;
			((LogPlayerTakeDamageOptions)(ref val)).IsCriticalHit = info.isHeadshot;
			if (player.IsDead())
			{
				((LogPlayerTakeDamageOptions)(ref val)).DamageResult = (AntiCheatCommonPlayerTakeDamageResult)(wasWounded ? 5 : 4);
			}
			else if (player.IsWounded())
			{
				((LogPlayerTakeDamageOptions)(ref val)).DamageResult = (AntiCheatCommonPlayerTakeDamageResult)3;
			}
			if ((Object)(object)info.Weapon != (Object)null)
			{
				Item item = info.Weapon.GetItem();
				if (item != null)
				{
					((LogPlayerUseWeaponData)(ref value)).WeaponName = Utf8String.op_Implicit(item.info.shortname);
				}
				else
				{
					((LogPlayerUseWeaponData)(ref value)).WeaponName = Utf8String.op_Implicit("unknown");
				}
			}
			else
			{
				((LogPlayerUseWeaponData)(ref value)).WeaponName = Utf8String.op_Implicit("unknown");
			}
			Vector3 position = basePlayer.eyes.position;
			Quaternion rotation = basePlayer.eyes.rotation;
			Vector3 position2 = player.eyes.position;
			Quaternion rotation2 = player.eyes.rotation;
			value2 = default(Vec3f);
			((Vec3f)(ref value2)).x = position.x;
			((Vec3f)(ref value2)).y = position.y;
			((Vec3f)(ref value2)).z = position.z;
			((LogPlayerTakeDamageOptions)(ref val)).AttackerPlayerPosition = value2;
			Quat value3 = default(Quat);
			((Quat)(ref value3)).w = rotation.w;
			((Quat)(ref value3)).x = rotation.x;
			((Quat)(ref value3)).y = rotation.y;
			((Quat)(ref value3)).z = rotation.z;
			((LogPlayerTakeDamageOptions)(ref val)).AttackerPlayerViewRotation = value3;
			value2 = default(Vec3f);
			((Vec3f)(ref value2)).x = position2.x;
			((Vec3f)(ref value2)).y = position2.y;
			((Vec3f)(ref value2)).z = position2.z;
			((LogPlayerTakeDamageOptions)(ref val)).VictimPlayerPosition = value2;
			value3 = default(Quat);
			((Quat)(ref value3)).w = rotation2.w;
			((Quat)(ref value3)).x = rotation2.x;
			((Quat)(ref value3)).y = rotation2.y;
			((Quat)(ref value3)).z = rotation2.z;
			((LogPlayerTakeDamageOptions)(ref val)).VictimPlayerViewRotation = value3;
			((LogPlayerTakeDamageOptions)(ref val)).PlayerUseWeaponData = value;
			Interface.LogPlayerTakeDamage(ref val);
		}
	}

	internal static void LogPlayerTick(Networkable playerNet, BasePlayer.EACTickState tickState)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (playerNet != null && playerNet.connection != null)
		{
			using (TimeWarning.New("EAC.LogPlayerTick"))
			{
				Interface.LogPlayerTick(ref tickState.TickOptions);
			}
		}
	}

	public static void LogPlayerRevive(BasePlayer source, BasePlayer target)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (CanSendAnalytics && target.net.connection != null && (Object)(object)source != (Object)null && source.net.connection != null)
		{
			using (TimeWarning.New("EAC.LogPlayerRevive"))
			{
				LogPlayerReviveOptions val = default(LogPlayerReviveOptions);
				((LogPlayerReviveOptions)(ref val)).RevivedPlayerHandle = GetClient(target.net.connection);
				((LogPlayerReviveOptions)(ref val)).ReviverPlayerHandle = GetClient(source.net.connection);
				Interface.LogPlayerRevive(ref val);
			}
		}
	}

	public static void SendPlayerBehaviorReport(BasePlayer reporter, PlayerReportsCategory reportCategory, string reportedID, string reportText)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (CanSendReports)
		{
			SendPlayerBehaviorReportOptions val = default(SendPlayerBehaviorReportOptions);
			((SendPlayerBehaviorReportOptions)(ref val)).ReportedUserId = ProductUserId.FromString(Utf8String.op_Implicit(reportedID));
			((SendPlayerBehaviorReportOptions)(ref val)).ReporterUserId = ProductUserId.FromString(Utf8String.op_Implicit(reporter.UserIDString));
			((SendPlayerBehaviorReportOptions)(ref val)).Category = reportCategory;
			((SendPlayerBehaviorReportOptions)(ref val)).Message = Utf8String.op_Implicit(reportText);
			SendPlayerBehaviorReportOptions val2 = val;
			Reports.SendPlayerBehaviorReport(ref val2, (object)null, (OnSendPlayerBehaviorReportCompleteCallback)null);
		}
	}

	public static void SendPlayerBehaviorReport(PlayerReportsCategory reportCategory, string reportedID, string reportText)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (CanSendReports)
		{
			SendPlayerBehaviorReportOptions val = default(SendPlayerBehaviorReportOptions);
			((SendPlayerBehaviorReportOptions)(ref val)).ReportedUserId = ProductUserId.FromString(Utf8String.op_Implicit(reportedID));
			((SendPlayerBehaviorReportOptions)(ref val)).Category = reportCategory;
			((SendPlayerBehaviorReportOptions)(ref val)).Message = Utf8String.op_Implicit(reportText);
			SendPlayerBehaviorReportOptions val2 = val;
			Reports.SendPlayerBehaviorReport(ref val2, (object)null, (OnSendPlayerBehaviorReportCompleteCallback)null);
		}
	}
}
