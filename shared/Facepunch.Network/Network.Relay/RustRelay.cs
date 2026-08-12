using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Facepunch;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Modes.Gcm;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using UnityEngine;
using UnityEngine.Profiling;
using WebSocketSharp;

namespace Network.Relay;

public static class RustRelay
{
	private readonly struct RelayQueueItem : IEquatable<RelayQueueItem>
	{
		public NetWrite Packet { get; }

		public ArraySegment<byte> Segment { get; }

		private string WipeId { get; }

		private bool ReturnBuffer { get; }

		private long ServerTimeTicks { get; }

		public bool IsMarker { get; }

		public bool ShouldEncrypt { get; }

		private RelayQueueItem(NetWrite packet, ArraySegment<byte> segment, string wipeId, bool returnBuffer, long serverTimeTicks, bool isMarker, bool shouldEncrypt)
		{
			Packet = packet;
			Segment = segment;
			WipeId = wipeId;
			ReturnBuffer = returnBuffer;
			ServerTimeTicks = serverTimeTicks;
			IsMarker = isMarker;
			ShouldEncrypt = shouldEncrypt;
		}

		public static RelayQueueItem FromNetWrite(NetWrite packet, string wipeId, long serverTimeTicks, bool shouldEncrypt)
		{
			return new RelayQueueItem(packet, default(ArraySegment<byte>), wipeId, returnBuffer: false, serverTimeTicks, isMarker: false, shouldEncrypt);
		}

		public static RelayQueueItem FromMarker(byte[] buffer, string wipeId, long serverTimeTicks)
		{
			return new RelayQueueItem(null, new ArraySegment<byte>(buffer, 0, 12), wipeId, returnBuffer: true, serverTimeTicks, isMarker: true, shouldEncrypt: false);
		}

		public bool Equals(RelayQueueItem other)
		{
			if ((Packet?.PeekPacketID() ?? 0) == (other.Packet?.PeekPacketID() ?? 0) && (Packet?.Length ?? 0) == (other.Packet?.Length ?? 0) && Segment.Equals(other.Segment) && WipeId == other.WipeId && ReturnBuffer == other.ReturnBuffer && ServerTimeTicks == other.ServerTimeTicks)
			{
				return IsMarker == other.IsMarker;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is RelayQueueItem other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine((Packet?.PeekPacketID() ?? 0).GetHashCode(), (Packet?.Length ?? 0).GetHashCode(), Segment.GetHashCode(), (WipeId != null) ? WipeId.GetHashCode() : 0, ReturnBuffer.GetHashCode(), ServerTimeTicks.GetHashCode(), IsMarker.GetHashCode());
		}
	}

	private static class Sodium
	{
		private const string LibraryName = "libsodium";

		private static int _initialized;

		private static int _aes256GcmAvailable;

		public static bool IsAes256GcmAvailable
		{
			get
			{
				EnsureInitialized();
				return _aes256GcmAvailable == 1;
			}
		}

		private static void EnsureInitialized()
		{
			if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0)
			{
				return;
			}
			try
			{
				if (sodium_init() >= 0)
				{
					_aes256GcmAvailable = crypto_aead_aes256gcm_is_available();
				}
			}
			catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException || ex is BadImageFormatException)
			{
				_aes256GcmAvailable = 0;
			}
		}

		[DllImport("libsodium", CallingConvention = CallingConvention.Cdecl)]
		private static extern int sodium_init();

		[DllImport("libsodium", CallingConvention = CallingConvention.Cdecl)]
		private static extern int crypto_aead_aes256gcm_is_available();

		[DllImport("libsodium", CallingConvention = CallingConvention.Cdecl)]
		public static extern int crypto_aead_aes256gcm_encrypt(byte[] c, out ulong clen, byte[] m, ulong mlen, IntPtr ad, ulong adlen, IntPtr nsec, byte[] npub, byte[] k);
	}

	private sealed class KeyExchangeRequest
	{
		[JsonProperty("wipeId")]
		public string WipeId { get; set; }

		[JsonProperty("clientPublicKey")]
		public string ClientPublicKey { get; set; }
	}

	private sealed class KeyExchangeResponse
	{
		[JsonProperty("serverPublicKey")]
		public string ServerPublicKey { get; set; }
	}

	private static readonly HttpClient Client;

	private static string StringPoolUrl;

	private static string SnapshotUrl;

	private static string MapSnapshotUrl;

	private static string ManifestUrl;

	private static string KeyExchangeUrl;

	private const int MarkerMagic = 1398035026;

	private const int MarkerLength = 12;

	private static long _lastMarkerTicks;

	private static WebSocket _socket;

	private static string _socketWipeId;

	private static string _socketToken;

	private static Thread _sendThread;

	private static int _sendThreadStarted;

	private static readonly AutoResetEvent _sendThreadReset;

	private static readonly ConcurrentQueue<RelayQueueItem> _sendQueue;

	private const int GcmNonceLength = 12;

	private const int GcmTagLength = 16;

	public static int SendQueueCount;

	private static long _windowMaxTicks;

	private static long _lastSendTicks;

	private const int MaxReconnectSlots = 64;

	private static readonly long[] _reconnectTimestamps;

	private static int _reconnectWriteIndex;

	private static volatile int _reconnectPending;

	private static int _consecutiveReconnects;

	private static volatile int _sendThreadGeneration;

	private static readonly byte[] _aesKey;

	private static KeyParameter _aesKeyParameter;

	private static readonly GcmBlockCipher _gcmCipher;

	private static X25519PrivateKeyParameters _localPrivateKey;

	private static byte[] _localPublicKey;

	private static X25519PublicKeyParameters _remotePublicKey;

	private static string _keyExchangeWipeId;

	private static string _keyExchangeToken;

	private static Dictionary<uint, string> _manifestPayload;

	private static Dictionary<uint, string> _stringPool;

	private static Dictionary<string, uint> _stringPoolByString;

	private static string _mapFolderName;

	private static string _mapFileName;

	private static string _rootFolder;

	private static string _saveFileName;

	private static Connection _fakeConnection;

	private static HashSet<uint> _rpcWhitelist;

	private static byte[] _rpcPacketBuffer;

	private static readonly byte[] _encryptionPacketBuffer;

	private static readonly byte[] _encryptionNonceBuffer;

	private static readonly byte[] _encryptionPlaintextBuffer;

	private static readonly byte[] _encryptionCiphertextBuffer;

	public static readonly ArrayPool<byte> PacketArrayPool;

	private static readonly TimeSpan ApiTimeout;

	private static readonly TimeSpan UploadTimeout;

	private static readonly Stopwatch SendTimer;

	public static string RelayWipeId { get; set; }

	public static string ServerHostname { get; set; }

	public static RustRelayConfig Config { get; set; }

	public static Func<bool> ForceSave { get; set; }

	public static Action<bool> EnabledChanged { get; set; }

	public static bool HasActiveFakeConnection
	{
		get
		{
			if (_fakeConnection != null && Config.Enabled)
			{
				return Config.FakePlayer;
			}
			return false;
		}
	}

	public static Connection ActiveFakeConnection
	{
		get
		{
			if (!HasActiveFakeConnection)
			{
				return null;
			}
			return _fakeConnection;
		}
	}

	static RustRelay()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_0060: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		Client = new HttpClient();
		RelayWipeId = string.Empty;
		ServerHostname = string.Empty;
		_lastMarkerTicks = -1L;
		_socketWipeId = string.Empty;
		_socketToken = string.Empty;
		_reconnectTimestamps = new long[64];
		_aesKey = new byte[32];
		_gcmCipher = new GcmBlockCipher((IBlockCipher)new AesEngine(), (IGcmMultiplier)new BasicGcmMultiplier());
		_keyExchangeWipeId = string.Empty;
		_keyExchangeToken = string.Empty;
		_rpcWhitelist = new HashSet<uint>();
		_rpcPacketBuffer = new byte[16];
		_encryptionPacketBuffer = new byte[4194332];
		_encryptionNonceBuffer = new byte[12];
		_encryptionPlaintextBuffer = new byte[4194304];
		_encryptionCiphertextBuffer = new byte[4194320];
		Config = new RustRelayConfig();
		PacketArrayPool = new ArrayPool<byte>(Math.Max(4194304, 12));
		ApiTimeout = TimeSpan.FromSeconds(10.0);
		UploadTimeout = TimeSpan.FromMinutes(2.0);
		SendTimer = new Stopwatch();
		_localPrivateKey = CreatePrivateKey();
		_localPublicKey = _localPrivateKey.GeneratePublicKey().GetEncoded();
		_sendQueue = new ConcurrentQueue<RelayQueueItem>();
		_sendThreadReset = new AutoResetEvent(initialState: false);
		_sendThread = new Thread(SendThread)
		{
			IsBackground = true,
			Name = "RustRelaySend"
		};
		Client.Timeout = Timeout.InfiniteTimeSpan;
	}

	private static async Task<HttpResponseMessage> SendWithTimeoutAsync(HttpRequestMessage request, TimeSpan timeout)
	{
		using CancellationTokenSource cts = new CancellationTokenSource(timeout);
		try
		{
			return await Client.SendAsync(request, cts.Token).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException) when (cts.IsCancellationRequested)
		{
			throw new TimeoutException($"Relay request to {request.RequestUri} timed out after {timeout.TotalSeconds:0}s");
		}
	}

	public static void InitUrls()
	{
		StringPoolUrl = Config.ServerUrl + "/api/StringPool";
		SnapshotUrl = Config.ServerUrl + "/api/Snapshot";
		MapSnapshotUrl = Config.ServerUrl + "/api/MapSnapshot";
		ManifestUrl = Config.ServerUrl + "/api/Manifest";
		KeyExchangeUrl = Config.ServerUrl + "/api/auth/exchangeKey";
	}

	private static void EnqueueMarker(long serverTimeTicks)
	{
		byte[] array = PacketArrayPool.Rent(12);
		Span<byte> destination = new Span<byte>(array, 0, 12);
		BinaryPrimitives.WriteInt32LittleEndian(destination, 1398035026);
		BinaryPrimitives.WriteInt64LittleEndian(destination.Slice(4), serverTimeTicks);
		_sendQueue.Enqueue(RelayQueueItem.FromMarker(array, RelayWipeId, serverTimeTicks));
		Interlocked.Increment(ref SendQueueCount);
	}

	public static void BuildRPCWhitelist()
	{
		_rpcWhitelist.Clear();
		foreach (string item in Config.RPCWhitelist)
		{
			if (_stringPoolByString.TryGetValue(item, out var value))
			{
				_rpcWhitelist.Add(value);
			}
		}
	}

	public static void EnqueuePacket(NetWrite packet)
	{
		Message.Type type = (Message.Type)(packet.PeekPacketID() - 140);
		bool shouldEncrypt = false;
		bool flag = false;
		switch (type)
		{
		case Message.Type.RPCMessage:
			if (!ShouldAllowRpc(packet))
			{
				return;
			}
			break;
		case Message.Type.VoiceData:
			if (!Config.EnableVoiceData)
			{
				return;
			}
			break;
		case Message.Type.ConsoleMessage:
		case Message.Type.ConsoleCommand:
			if (!Config.EnableConsoleData)
			{
				return;
			}
			shouldEncrypt = true;
			flag = true;
			break;
		default:
			return;
		case Message.Type.Entities:
		case Message.Type.EntityDestroy:
		case Message.Type.EntityPosition:
		case Message.Type.Effect:
		case Message.Type.EntityFlags:
			break;
		}
		if (flag || ShouldRelayPacket(packet))
		{
			if (Interlocked.Exchange(ref _sendThreadStarted, 1) == 0)
			{
				_sendThread.Start();
			}
			if (Config.EncryptPackets)
			{
				shouldEncrypt = true;
			}
			if (packet.serverTicks != _lastMarkerTicks)
			{
				_lastMarkerTicks = packet.serverTicks;
				EnqueueMarker(packet.serverTicks);
			}
			packet.AddReference();
			_sendQueue.Enqueue(RelayQueueItem.FromNetWrite(packet, RelayWipeId, packet.serverTicks, shouldEncrypt));
			Interlocked.Increment(ref SendQueueCount);
			_sendThreadReset.Set();
		}
	}

	public static void RegisterFakeConnection(Connection connection)
	{
		Debug.Assert(connection != null, "RustRelay fake connection cannot be null");
		Debug.Assert(_fakeConnection == null || _fakeConnection == connection, "RustRelay fake connection registered twice");
		_fakeConnection = connection;
	}

	public static void ClearFakeConnection(Connection connection)
	{
		Debug.Assert(connection != null, "RustRelay fake connection cannot be null");
		Debug.Assert(_fakeConnection == connection, "RustRelay fake connection clear mismatch");
		_fakeConnection = null;
	}

	public static bool IsFakeConnection(Connection connection)
	{
		if (connection != null)
		{
			return _fakeConnection == connection;
		}
		return false;
	}

	public static string GetStatusReport()
	{
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder(2048);
		WebSocket socket = _socket;
		Connection fakeConnection = _fakeConnection;
		string text = Config.AuthToken ?? string.Empty;
		Thread sendThread = _sendThread;
		stringBuilder.AppendLine("Rust Relay Status");
		stringBuilder.AppendLine("=================");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Config");
		stringBuilder.AppendLine($"  Enabled: {Config.Enabled}");
		stringBuilder.AppendLine($"  FakePlayer: {Config.FakePlayer}");
		stringBuilder.AppendLine($"  EncryptPackets: {Config.EncryptPackets}");
		stringBuilder.AppendLine($"  RPCFilterMode: {Config.RPCFilterMode}");
		stringBuilder.AppendLine($"  RPCWhitelist configured/resolved: {Config.RPCWhitelist?.Count ?? 0}/{_rpcWhitelist.Count}");
		stringBuilder.AppendLine($"  EnableVoiceData: {Config.EnableVoiceData}");
		stringBuilder.AppendLine($"  EnableConsoleData: {Config.EnableConsoleData}");
		stringBuilder.AppendLine("  ServerUrl: " + FormatStatusValue(Config.ServerUrl));
		stringBuilder.AppendLine($"  AuthToken configured: {!string.IsNullOrWhiteSpace(text)}");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Runtime");
		stringBuilder.AppendLine("  RelayWipeId: " + FormatStatusValue(RelayWipeId));
		stringBuilder.AppendLine($"  HasActiveFakeConnection: {HasActiveFakeConnection}");
		stringBuilder.AppendLine($"  ForceSave callback set: {ForceSave != null}");
		stringBuilder.AppendLine($"  EnabledChanged callback set: {EnabledChanged != null}");
		stringBuilder.AppendLine($"  Send thread started flag: {Volatile.Read(in _sendThreadStarted)}");
		stringBuilder.AppendLine($"  Send thread alive: {sendThread?.IsAlive ?? false}");
		stringBuilder.AppendLine("  Send thread state: " + (sendThread?.ThreadState.ToString() ?? "null"));
		stringBuilder.AppendLine($"  SendQueueCount counter: {Volatile.Read(in SendQueueCount)}");
		stringBuilder.AppendLine($"  Send queue actual count: {_sendQueue.Count}");
		stringBuilder.AppendLine($"  Last marker ticks: {_lastMarkerTicks}");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("WebSocket");
		stringBuilder.AppendLine($"  Socket exists: {socket != null}");
		stringBuilder.AppendLine("  ReadyState: " + (((socket != null) ? ((object)socket.ReadyState/*cast due to constrained. prefix*/).ToString() : null) ?? "null"));
		stringBuilder.AppendLine("  Socket wipe id: " + FormatStatusValue(_socketWipeId));
		stringBuilder.AppendLine($"  Socket wipe id matches current: {_socketWipeId == RelayWipeId}");
		stringBuilder.AppendLine($"  Socket token matches current: {_socketToken == text}");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("URLs");
		stringBuilder.AppendLine("  StringPoolUrl: " + FormatStatusValue(StringPoolUrl));
		stringBuilder.AppendLine("  SnapshotUrl: " + FormatStatusValue(SnapshotUrl));
		stringBuilder.AppendLine("  MapSnapshotUrl: " + FormatStatusValue(MapSnapshotUrl));
		stringBuilder.AppendLine("  ManifestUrl: " + FormatStatusValue(ManifestUrl));
		stringBuilder.AppendLine("  KeyExchangeUrl: " + FormatStatusValue(KeyExchangeUrl));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Key exchange");
		stringBuilder.AppendLine($"  Local private key ready: {_localPrivateKey != null}");
		stringBuilder.AppendLine($"  Local public key ready: {_localPublicKey != null}");
		stringBuilder.AppendLine($"  Remote public key ready: {_remotePublicKey != null}");
		stringBuilder.AppendLine($"  AES key parameter ready: {_aesKeyParameter != null}");
		stringBuilder.AppendLine("  Key exchange wipe id: " + FormatStatusValue(_keyExchangeWipeId));
		stringBuilder.AppendLine($"  Key exchange wipe id matches current: {_keyExchangeWipeId == RelayWipeId}");
		stringBuilder.AppendLine($"  Key exchange token matches current: {_keyExchangeToken == text}");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Auto-reconnect");
		stringBuilder.AppendLine($"  AutoReconnect: {Config.AutoReconnect}");
		stringBuilder.AppendLine($"  MaxReconnectsInWindow: {Config.MaxReconnectsInWindow}");
		stringBuilder.AppendLine($"  ReconnectWindowMinutes: {Config.ReconnectWindowMinutes}");
		stringBuilder.AppendLine($"  ReconnectDelaySeconds: {Config.ReconnectDelaySeconds}");
		stringBuilder.AppendLine($"  Reconnect pending: {_reconnectPending != 0}");
		long ticks = DateTime.UtcNow.Ticks;
		long num = (long)Config.ReconnectWindowMinutes * 600000000L;
		int num2 = 0;
		for (int i = 0; i < 64; i++)
		{
			long num3 = _reconnectTimestamps[i];
			if (num3 != 0L && ticks - num3 <= num)
			{
				num2++;
			}
		}
		stringBuilder.AppendLine($"  Reconnects in window: {num2}/{Config.MaxReconnectsInWindow}");
		stringBuilder.AppendLine($"  Consecutive reconnects: {_consecutiveReconnects}");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Cached uploads");
		stringBuilder.AppendLine($"  Manifest entries: {_manifestPayload?.Count ?? 0}");
		stringBuilder.AppendLine($"  String pool entries: {_stringPool?.Count ?? 0}");
		stringBuilder.AppendLine($"  String pool reverse entries: {_stringPoolByString?.Count ?? 0}");
		stringBuilder.AppendLine("  Map folder/name: " + FormatStatusValue(_mapFolderName) + " / " + FormatStatusValue(_mapFileName));
		stringBuilder.AppendLine("  Save root/name: " + FormatStatusValue(_rootFolder) + " / " + FormatStatusValue(_saveFileName));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Registered fake connection");
		stringBuilder.AppendLine($"  Exists: {fakeConnection != null}");
		if (fakeConnection != null)
		{
			stringBuilder.AppendLine($"  Guid: {fakeConnection.guid}");
			stringBuilder.AppendLine($"  UserId: {fakeConnection.userid}");
			stringBuilder.AppendLine($"  OwnerId: {fakeConnection.ownerid}");
			stringBuilder.AppendLine($"  Active: {fakeConnection.active}");
			stringBuilder.AppendLine($"  Connected: {fakeConnection.connected}");
			stringBuilder.AppendLine($"  State: {fakeConnection.state}");
			stringBuilder.AppendLine($"  GlobalNetworking: {fakeConnection.globalNetworking}");
			stringBuilder.AppendLine($"  Player: {fakeConnection.player}");
		}
		stringBuilder.AppendLine();
		return stringBuilder.ToString();
	}

	private static string FormatStatusValue(string value)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			return value;
		}
		return "<unset>";
	}

	public static bool ShouldRelayPacket(NetWrite packet)
	{
		if (!Config.FakePlayer)
		{
			return true;
		}
		Connection fakeConnection = _fakeConnection;
		if (fakeConnection == null)
		{
			return false;
		}
		List<Connection> connections = packet.connections;
		if (connections == null || connections.Count == 0)
		{
			return false;
		}
		return connections[0] == fakeConnection;
	}

	private static bool ShouldAllowRpc(NetWrite packet)
	{
		switch (Config.RPCFilterMode)
		{
		case RPCFilterMode.Ignore:
			return false;
		case RPCFilterMode.AllowWhitelist:
		{
			_rpcPacketBuffer = packet.GetBuffer().Buffer;
			uint item = BitConverter.ToUInt32(_rpcPacketBuffer, 12);
			return _rpcWhitelist.Contains(item);
		}
		default:
			return true;
		}
	}

	public static void WipeSendThreadQueue()
	{
		RelayQueueItem result;
		while (_sendQueue.TryDequeue(out result))
		{
			result.Packet?.RemoveReference();
		}
	}

	private static void SendThread()
	{
		int sendThreadGeneration = _sendThreadGeneration;
		try
		{
			Debug.Log((object)"[RustRelay] Send Thread Started");
			if (EnsureSocketInternal() == null && Config.Enabled)
			{
				_reconnectPending = 1;
			}
			while (_sendThreadGeneration == sendThreadGeneration && (Config.Enabled || _reconnectPending != 0))
			{
				if (_reconnectPending != 0)
				{
					if (!TryReconnect(sendThreadGeneration))
					{
						break;
					}
					continue;
				}
				try
				{
					RelayQueueItem result;
					while (_sendQueue.TryDequeue(out result))
					{
						if (_sendThreadGeneration != sendThreadGeneration || (!Config.Enabled && _reconnectPending == 0))
						{
							result.Packet?.RemoveReference();
							break;
						}
						Interlocked.Decrement(ref SendQueueCount);
						if (result.IsMarker)
						{
							SendMarker(result.Segment);
						}
						else
						{
							SendPacket(result.Packet, result.ShouldEncrypt);
						}
						_lastSendTicks = DateTime.UtcNow.Ticks;
					}
				}
				catch (Exception arg)
				{
					SignalReconnect($"Send Thread Error: {arg}");
				}
				long ticks = DateTime.UtcNow.Ticks;
				if (ticks - _lastSendTicks > 150000000)
				{
					try
					{
						WebSocket socket = _socket;
						if (socket != null)
						{
							socket.Ping();
						}
						_lastSendTicks = ticks;
					}
					catch
					{
					}
				}
				_sendThreadReset.WaitOne(100);
			}
		}
		finally
		{
			if (_sendThreadGeneration == sendThreadGeneration)
			{
				ResetSocket("Send thread exiting");
				WipeSendThreadQueue();
			}
			Profiler.EndThreadProfiling();
			Debug.Log((object)"[RustRelay] Send Thread Exited");
		}
	}

	public static void Failover(string reason = "No Reason Provided")
	{
		if (Config.Enabled)
		{
			Debug.LogError((object)("[Rust Relay] DISABLED! Reason: " + reason));
			ResetKeyExchangeState();
			_reconnectPending = 0;
			Config.Enabled = false;
			EnabledChanged?.Invoke(obj: false);
		}
	}

	private static void SignalReconnect(string reason)
	{
		if (!Config.AutoReconnect)
		{
			Failover(reason);
		}
		else if (Config.Enabled)
		{
			Debug.LogWarning((object)("[RustRelay] Connection issue: " + reason));
			ResetSocket(reason);
			_reconnectPending = 1;
			_sendThreadReset.Set();
		}
	}

	private static bool TryReconnect(int currentGeneration)
	{
		_reconnectPending = 0;
		long ticks = DateTime.UtcNow.Ticks;
		long num = (long)Config.ReconnectWindowMinutes * 600000000L;
		int num2 = 0;
		for (int i = 0; i < 64; i++)
		{
			long num3 = _reconnectTimestamps[i];
			if (num3 != 0L && ticks - num3 <= num)
			{
				num2++;
			}
		}
		if (num2 >= Config.MaxReconnectsInWindow)
		{
			Failover($"Auto-reconnect budget exhausted ({num2} reconnects in {Config.ReconnectWindowMinutes}m)");
			return false;
		}
		_reconnectTimestamps[_reconnectWriteIndex] = ticks;
		_reconnectWriteIndex = (_reconnectWriteIndex + 1) % 64;
		_consecutiveReconnects++;
		int num4 = Math.Max(Config.ReconnectDelaySeconds, 1) * _consecutiveReconnects * 1000;
		Debug.Log((object)string.Format("[RustRelay] Reconnecting ({0}/{1} in {2}m window, delay {3}s)...", new object[4]
		{
			num2 + 1,
			Config.MaxReconnectsInWindow,
			Config.ReconnectWindowMinutes,
			num4 / 1000
		}));
		_sendThreadReset.WaitOne(num4);
		if (_sendThreadGeneration != currentGeneration)
		{
			return false;
		}
		if (!Config.Enabled && _reconnectPending == 0)
		{
			return false;
		}
		try
		{
			ResetKeyExchangeState();
			if (EnsureSocketInternal() != null)
			{
				_consecutiveReconnects = 0;
				Debug.Log((object)"[RustRelay] Reconnected successfully");
				return true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[RustRelay] Reconnect failed: " + ex.Message));
		}
		_reconnectPending = 1;
		return true;
	}

	public static void ClearReconnectHistory()
	{
		Array.Clear(_reconnectTimestamps, 0, 64);
		_reconnectWriteIndex = 0;
		_consecutiveReconnects = 0;
		_reconnectPending = 0;
	}

	public static void SetCachedStringPool(Dictionary<uint, string> stringPool)
	{
		_stringPool = stringPool;
		_stringPoolByString = stringPool?.ToDictionary((KeyValuePair<uint, string> x) => x.Value, (KeyValuePair<uint, string> x) => x.Key);
	}

	public static void SetCachedManifest(Dictionary<uint, string> manifestPayload)
	{
		_manifestPayload = manifestPayload;
	}

	public static void SetCachedMapSnapshot(string mapFolderName, string mapFileName)
	{
		_mapFolderName = mapFolderName;
		_mapFileName = mapFileName;
	}

	public static void SetCachedSnapshot(string rootFolder, string saveFileName)
	{
		_rootFolder = rootFolder;
		_saveFileName = saveFileName;
	}

	public static async Task UploadStringPoolToRelayAsync(Dictionary<uint, string> stringPool = null)
	{
		if (stringPool != null)
		{
			SetCachedStringPool(stringPool);
		}
		stringPool = _stringPool;
		if (!Config.Enabled)
		{
			return;
		}
		try
		{
			if (stringPool == null || !stringPool.Any())
			{
				return;
			}
			if (string.IsNullOrWhiteSpace(Config.AuthToken))
			{
				Failover("[Rust Relay] Auth Token Not Provided");
				return;
			}
			string content = JsonConvert.SerializeObject((object)stringPool);
			using StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json");
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, StringPoolUrl)
			{
				Content = content2
			};
			AddRelayHeaders(request);
			await SendWithTimeoutAsync(request, ApiTimeout).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex)
		{
			Failover("Failed to upload StringPool to relay: " + ex.Message);
		}
	}

	public static async Task UploadManifestToRelayAsync(Dictionary<uint, string> manifestPayload = null)
	{
		if (manifestPayload != null)
		{
			SetCachedManifest(manifestPayload);
		}
		manifestPayload = _manifestPayload;
		if (!Config.Enabled)
		{
			return;
		}
		try
		{
			if (manifestPayload == null || manifestPayload.Count == 0)
			{
				Debug.LogWarning((object)"[Rust Relay] Manifest upload skipped, missing prefab properties");
				return;
			}
			if (string.IsNullOrWhiteSpace(Config.AuthToken))
			{
				Failover("[RustRelay] Auth Token not provided");
				return;
			}
			string content = JsonConvert.SerializeObject((object)manifestPayload);
			using StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json");
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ManifestUrl)
			{
				Content = content2
			};
			AddRelayHeaders(request);
			await SendWithTimeoutAsync(request, ApiTimeout).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex)
		{
			Failover("Failed to upload manifest to relay: " + ex.Message);
		}
	}

	public static async Task UploadMapSnapshotAsync(string mapFolderName = null, string mapFileName = null)
	{
		if (mapFolderName != null || mapFileName != null)
		{
			SetCachedMapSnapshot(mapFolderName, mapFileName);
		}
		mapFolderName = _mapFolderName;
		mapFileName = _mapFileName;
		if (!Config.Enabled)
		{
			return;
		}
		try
		{
			if (string.IsNullOrWhiteSpace(Config.AuthToken))
			{
				Failover("Map snapshot upload blocked: missing relay bearer token");
				return;
			}
			string path = Path.Combine(mapFolderName, mapFileName);
			path = ResolveServerRelativePath(path);
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				return;
			}
			string path2 = Path.ChangeExtension(path, ".dat");
			bool flag = File.Exists(path2);
			using FileStream mapStream = File.OpenRead(path);
			using StreamContent mapContent = new StreamContent(mapStream);
			using MultipartFormDataContent content = new MultipartFormDataContent();
			content.Add(mapContent, "map", Path.GetFileName(path));
			using FileStream datStream = (flag ? File.OpenRead(path2) : null);
			using StreamContent datContent = ((datStream != null) ? new StreamContent(datStream) : null);
			if (datContent != null)
			{
				content.Add(datContent, "dat", Path.GetFileName(path2));
			}
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, MapSnapshotUrl)
			{
				Content = content
			};
			AddRelayHeaders(request);
			HttpResponseMessage httpResponseMessage = await SendWithTimeoutAsync(request, UploadTimeout).ConfigureAwait(continueOnCapturedContext: false);
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				Failover($"Map snapshot upload failed: {(int)httpResponseMessage.StatusCode} {httpResponseMessage.ReasonPhrase}");
			}
		}
		catch (Exception ex)
		{
			Failover("Failed to upload map snapshot to relay: " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static async Task UploadSnapshotAsync(string rootFolder = null, string saveFileName = null)
	{
		if (rootFolder != null || saveFileName != null)
		{
			SetCachedSnapshot(rootFolder, saveFileName);
		}
		rootFolder = _rootFolder;
		saveFileName = _saveFileName;
		if (!Config.Enabled)
		{
			return;
		}
		try
		{
			if (string.IsNullOrWhiteSpace(Config.AuthToken))
			{
				Failover("Snapshot upload blocked: missing relay bearer token");
				return;
			}
			string path = Path.Combine(rootFolder, saveFileName);
			path = ResolveServerRelativePath(path);
			if (!File.Exists(path))
			{
				try
				{
					ForceSave?.Invoke();
				}
				catch (Exception ex)
				{
					Debug.LogWarning((object)("[RustRelay] ForceSave failed (server may still be starting): " + ex.Message));
				}
				path = ResolveServerRelativePath(Path.Combine(rootFolder, saveFileName));
				if (!File.Exists(path))
				{
					Debug.LogWarning((object)("[RustRelay] Snapshot upload deferred, save file not yet available: " + path));
					return;
				}
			}
			using FileStream stream = File.OpenRead(path);
			using StreamContent content = new StreamContent(stream);
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, SnapshotUrl)
			{
				Content = content
			};
			AddRelayHeaders(request);
			HttpResponseMessage httpResponseMessage = await SendWithTimeoutAsync(request, UploadTimeout).ConfigureAwait(continueOnCapturedContext: false);
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				Failover($"Snapshot upload failed: {(int)httpResponseMessage.StatusCode} {httpResponseMessage.ReasonPhrase}");
			}
		}
		catch (Exception ex2)
		{
			Failover("Failed to upload snapshot to relay: " + ex2.Message);
		}
	}

	private static string ResolveServerRelativePath(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return path;
		}
		if (Path.IsPathRooted(path))
		{
			return path;
		}
		string fullPath = Path.GetFullPath(path);
		if (File.Exists(fullPath))
		{
			return fullPath;
		}
		string dataPath = Application.dataPath;
		if (!string.IsNullOrWhiteSpace(dataPath))
		{
			string fullPath2 = Path.GetFullPath(Path.Combine(Path.GetFullPath(Path.Combine(dataPath, "..")), path));
			if (File.Exists(fullPath2))
			{
				return fullPath2;
			}
		}
		return path;
	}

	private static void AddRelayHeaders(HttpRequestMessage request)
	{
		request.Headers.Remove("X-Server-Time");
		request.Headers.Remove("X-Wipe-Id");
		request.Headers.Remove("Authorization");
		long ticks = DateTime.UtcNow.Ticks;
		DateTime unixEpoch = DateTime.UnixEpoch;
		string value = (ticks - unixEpoch.Ticks).ToString();
		request.Headers.Add("X-Server-Time", value);
		request.Headers.Add("X-Wipe-Id", RelayWipeId);
		if (!string.IsNullOrWhiteSpace(Config.AuthToken))
		{
			request.Headers.Add("Authorization", "Bearer " + Config.AuthToken);
		}
	}

	private static void SendPacket(NetWrite packet, bool shouldEncrypt)
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Expected O, but got Unknown
		try
		{
			var (array, num) = packet.GetBuffer();
			if (!shouldEncrypt)
			{
				SendPacket(new ArraySegment<byte>(array, 0, num));
				return;
			}
			if (_aesKeyParameter == null)
			{
				Failover("Packet encryption requested before key exchange completed.");
				return;
			}
			int num2 = num - 1;
			_encryptionPacketBuffer[0] = array[0];
			RandomNumberGenerator.Fill(_encryptionNonceBuffer);
			Buffer.BlockCopy(_encryptionNonceBuffer, 0, _encryptionPacketBuffer, 1, 12);
			int num3 = 13;
			if (Sodium.IsAes256GcmAvailable)
			{
				Buffer.BlockCopy(array, 1, _encryptionPlaintextBuffer, 0, num2);
				if (Sodium.crypto_aead_aes256gcm_encrypt(_encryptionCiphertextBuffer, out var clen, _encryptionPlaintextBuffer, (ulong)num2, IntPtr.Zero, 0uL, IntPtr.Zero, _encryptionNonceBuffer, _aesKey) != 0)
				{
					Failover("libsodium AES-256-GCM packet encryption failed.");
					return;
				}
				int num4 = checked((int)clen);
				Buffer.BlockCopy(_encryptionCiphertextBuffer, 0, _encryptionPacketBuffer, num3, num4);
				SendPacket(new ArraySegment<byte>(_encryptionPacketBuffer, 0, num3 + num4));
			}
			else
			{
				_gcmCipher.Init(true, (ICipherParameters)new ParametersWithIV((ICipherParameters)(object)_aesKeyParameter, _encryptionNonceBuffer));
				int num5 = _gcmCipher.ProcessBytes(array, 1, num2, _encryptionPacketBuffer, num3);
				num5 += _gcmCipher.DoFinal(_encryptionPacketBuffer, num3 + num5);
				SendPacket(new ArraySegment<byte>(_encryptionPacketBuffer, 0, num3 + num5));
			}
		}
		finally
		{
			packet.RemoveReference();
		}
	}

	private static void SendMarker(ArraySegment<byte> segment)
	{
		try
		{
			WebSocket socket = _socket;
			if (socket != null)
			{
				socket.SendRaw(segment.Array, segment.Offset, segment.Count);
			}
		}
		catch (Exception ex)
		{
			SignalReconnect("Marker send failed: " + ex.Message);
		}
		finally
		{
			PacketArrayPool.Return(segment.Array);
		}
	}

	private static void SendPacket(ArraySegment<byte> segment)
	{
		try
		{
			WebSocket socket = _socket;
			if (socket != null)
			{
				socket.SendRaw(segment.Array, segment.Offset, segment.Count);
			}
		}
		catch (Exception ex)
		{
			SignalReconnect("WebSocket send failed: " + ex.Message);
		}
	}

	private static WebSocket EnsureSocketInternal()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		if (!Config.Enabled)
		{
			return null;
		}
		try
		{
			string text = Config.AuthToken ?? string.Empty;
			if (_socket != null && (int)_socket.ReadyState == 2 && _socketWipeId == RelayWipeId && _socketToken == text)
			{
				return _socket;
			}
			if (Config.EnableConsoleData || Config.EncryptPackets)
			{
				EnsureServerKeyExchange();
			}
			ResetSocket("Replacing stale socket");
			_socket = new WebSocket(BuildPacketWsUri(RelayWipeId, text).ToString(), Array.Empty<string>());
			if (!string.IsNullOrWhiteSpace(text))
			{
				_socket.SetUserHeader("Authorization", "Bearer " + text);
			}
			_socket.OnOpen += delegate
			{
				Debug.Log((object)("[RustRelay] WebSocket opened wipeId=" + RelayWipeId));
			};
			_socket.OnClose += delegate(object _, CloseEventArgs args)
			{
				SignalReconnect($"WebSocket closed code={args.Code} reason={args.Reason} wasClean={args.WasClean}");
			};
			_socket.OnError += delegate(object _, ErrorEventArgs args)
			{
				SignalReconnect("WebSocket error: " + args.Message);
			};
			_socket.Connect();
			_socketWipeId = RelayWipeId;
			_socketToken = text;
			return _socket;
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[RustRelay] WebSocket connection failed: " + ex.Message));
			ResetSocket("Connection failed: " + ex.Message);
			return null;
		}
	}

	private static void ResetSocket(string reason = null)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		WebSocket socket = _socket;
		if (socket == null)
		{
			return;
		}
		_socket = null;
		try
		{
			if ((int)socket.ReadyState != 2 && (int)socket.ReadyState != 1)
			{
				return;
			}
			if (reason != null)
			{
				if (reason.Length > 123)
				{
					reason = reason.Substring(0, 123);
				}
				socket.Close((CloseStatusCode)1000, reason);
			}
			else
			{
				socket.Close();
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[RustRelay] ResetSocket exception during Close: " + ex.GetType().Name + " " + ex.Message));
		}
	}

	private static Uri BuildPacketWsUri(string wipeId, string token)
	{
		UriBuilder obj = new UriBuilder(new Uri(Config.ServerUrl.Replace("https://", "http://")))
		{
			Scheme = "ws",
			Path = "/ws/ingest"
		};
		string text = "wipeId=" + Uri.EscapeDataString(wipeId ?? string.Empty);
		if (!string.IsNullOrWhiteSpace(token))
		{
			text = text + "&access_token=" + Uri.EscapeDataString(token);
		}
		if (!string.IsNullOrEmpty(ServerHostname))
		{
			text = text + "&hostname=" + Uri.EscapeDataString(ServerHostname);
		}
		if (_consecutiveReconnects > 0)
		{
			text += $"&reconnectAttempt={_consecutiveReconnects}";
		}
		obj.Query = text;
		return obj.Uri;
	}

	public static async void AttemptRestart()
	{
		_ = 1;
		try
		{
			Thread sendThread = _sendThread;
			Interlocked.Increment(ref _sendThreadGeneration);
			Config.Enabled = false;
			_reconnectPending = 0;
			_sendThreadReset.Set();
			if (sendThread != null && sendThread.IsAlive)
			{
				sendThread.Join(15000);
			}
			Debug.Log((object)"[Rust Relay] Uploading initial server state");
			InitUrls();
			ResetSocket("Relay restarting");
			ResetKeyExchangeState();
			ClearReconnectHistory();
			_lastMarkerTicks = -1L;
			if (Config.EnableConsoleData || Config.EncryptPackets)
			{
				await ExchangeServerKeyAsync(RelayWipeId, Config.AuthToken ?? string.Empty);
				Debug.Log((object)"[Rust Relay] Server key exchange completed");
			}
			Config.Enabled = true;
			EnabledChanged?.Invoke(obj: true);
			await Task.WhenAll(new Task[4]
			{
				UploadManifestToRelayAsync(_manifestPayload),
				UploadStringPoolToRelayAsync(_stringPool),
				UploadMapSnapshotAsync(_mapFolderName, _mapFileName),
				UploadSnapshotAsync(_rootFolder, _saveFileName)
			});
			Debug.Log((object)"[Rust Relay] Initial server state uploaded");
			BuildRPCWhitelist();
			_sendThread = new Thread(SendThread)
			{
				IsBackground = true,
				Name = "RustRelaySend"
			};
			Interlocked.Exchange(ref _sendThreadStarted, 1);
			_sendThread.Start();
		}
		catch (Exception arg)
		{
			Failover($"Failed to restart relay: {arg}");
		}
	}

	public static void LoadConfig(RustRelayConfig loaded)
	{
		Config = loaded;
		InitUrls();
	}

	public static void EnsureServerKeyExchange()
	{
		if (_localPrivateKey == null || _localPublicKey == null)
		{
			throw new InvalidOperationException("Local X25519 key not initialized.");
		}
		string text = Config.AuthToken ?? string.Empty;
		if (_aesKeyParameter == null || !(_keyExchangeWipeId == RelayWipeId) || !(_keyExchangeToken == text))
		{
			ExchangeServerKeyAsync(RelayWipeId, text).GetAwaiter().GetResult();
		}
	}

	private static void ResetKeyExchangeState()
	{
		_aesKeyParameter = null;
		_remotePublicKey = null;
		_keyExchangeWipeId = string.Empty;
		_keyExchangeToken = string.Empty;
		CryptographicOperations.ZeroMemory((Span<byte>)_aesKey);
	}

	private static async Task ExchangeServerKeyAsync(string wipeId, string token)
	{
		_aesKeyParameter = null;
		string clientPublicKey = Bech32.Encode("rk", _localPublicKey);
		string content = JsonConvert.SerializeObject((object)new KeyExchangeRequest
		{
			WipeId = wipeId,
			ClientPublicKey = clientPublicKey
		});
		using StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json");
		using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, KeyExchangeUrl);
		request.Content = content2;
		AddRelayHeaders(request);
		using HttpResponseMessage response = await SendWithTimeoutAsync(request, ApiTimeout).ConfigureAwait(continueOnCapturedContext: false);
		response.EnsureSuccessStatusCode();
		KeyExchangeResponse keyExchangeResponse = JsonConvert.DeserializeObject<KeyExchangeResponse>(await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false));
		if (keyExchangeResponse == null || string.IsNullOrWhiteSpace(keyExchangeResponse.ServerPublicKey))
		{
			throw new InvalidOperationException("Relay key exchange response did not include a server public key.");
		}
		byte[] array = Bech32.Decode(keyExchangeResponse.ServerPublicKey, out var hrp);
		if (hrp != "rk")
		{
			throw new Exception("invalid public key");
		}
		_remotePublicKey = new X25519PublicKeyParameters(array, 0);
		byte[] array2 = new byte[32];
		_localPrivateKey.GenerateSecret(_remotePublicKey, array2, 0);
		try
		{
			using SHA256 sHA = SHA256.Create();
			if (!sHA.TryComputeHash(array2, _aesKey, out var bytesWritten) || bytesWritten != 32)
			{
				throw new CryptographicException("Failed to derive AES key.");
			}
		}
		finally
		{
			CryptographicOperations.ZeroMemory((Span<byte>)array2);
		}
		_aesKeyParameter = new KeyParameter((byte[])_aesKey.Clone());
		_keyExchangeWipeId = wipeId;
		_keyExchangeToken = token;
	}

	private static X25519PrivateKeyParameters CreatePrivateKey()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		return new X25519PrivateKeyParameters(new SecureRandom());
	}

	public static string DownloadStringSync(string url)
	{
		HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(url);
		obj.Method = "GET";
		using HttpWebResponse httpWebResponse = (HttpWebResponse)obj.GetResponse();
		using Stream stream = httpWebResponse.GetResponseStream();
		using StreamReader streamReader = new StreamReader(stream);
		return streamReader.ReadToEnd();
	}
}
