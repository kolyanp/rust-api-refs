using System;
using System.Collections.Generic;

namespace Network.Relay;

[Serializable]
public class RustRelayConfig
{
	public bool Enabled { get; set; }

	public string ServerUrl { get; set; } = "";

	public string AuthToken { get; set; }

	public bool EncryptPackets { get; set; }

	public bool FakePlayer { get; set; }

	public RPCFilterMode RPCFilterMode { get; set; }

	public HashSet<string> RPCWhitelist { get; set; } = new HashSet<string>();

	public bool EnableVoiceData { get; set; }

	public bool EnableConsoleData { get; set; } = true;

	public bool AutoReconnect { get; set; } = true;

	public int MaxReconnectsInWindow { get; set; } = 5;

	public int ReconnectWindowMinutes { get; set; } = 30;

	public int ReconnectDelaySeconds { get; set; } = 5;
}
