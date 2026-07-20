using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace MySqlConnector;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal abstract class MySqlConnectionStringOption
{
	public static readonly MySqlConnectionStringReferenceOption<string> Server;

	public static readonly MySqlConnectionStringValueOption<uint> Port;

	public static readonly MySqlConnectionStringReferenceOption<string> UserID;

	public static readonly MySqlConnectionStringReferenceOption<string> Password;

	public static readonly MySqlConnectionStringReferenceOption<string> Database;

	public static readonly MySqlConnectionStringValueOption<MySqlLoadBalance> LoadBalance;

	public static readonly MySqlConnectionStringValueOption<MySqlConnectionProtocol> ConnectionProtocol;

	public static readonly MySqlConnectionStringReferenceOption<string> PipeName;

	public static readonly MySqlConnectionStringValueOption<MySqlSslMode> SslMode;

	public static readonly MySqlConnectionStringReferenceOption<string> CertificateFile;

	public static readonly MySqlConnectionStringReferenceOption<string> CertificatePassword;

	public static readonly MySqlConnectionStringValueOption<MySqlCertificateStoreLocation> CertificateStoreLocation;

	public static readonly MySqlConnectionStringReferenceOption<string> CertificateThumbprint;

	public static readonly MySqlConnectionStringReferenceOption<string> SslCert;

	public static readonly MySqlConnectionStringReferenceOption<string> SslKey;

	public static readonly MySqlConnectionStringReferenceOption<string> SslCa;

	public static readonly MySqlConnectionStringReferenceOption<string> TlsVersion;

	public static readonly MySqlConnectionStringReferenceOption<string> TlsCipherSuites;

	public static readonly MySqlConnectionStringValueOption<bool> Pooling;

	public static readonly MySqlConnectionStringValueOption<uint> ConnectionLifeTime;

	public static readonly MySqlConnectionStringValueOption<bool> ConnectionReset;

	public static readonly MySqlConnectionStringValueOption<bool> DeferConnectionReset;

	public static readonly MySqlConnectionStringValueOption<uint> ConnectionIdlePingTime;

	public static readonly MySqlConnectionStringValueOption<uint> ConnectionIdleTimeout;

	public static readonly MySqlConnectionStringValueOption<uint> MinimumPoolSize;

	public static readonly MySqlConnectionStringValueOption<uint> MaximumPoolSize;

	public static readonly MySqlConnectionStringValueOption<uint> DnsCheckInterval;

	public static readonly MySqlConnectionStringValueOption<bool> AllowLoadLocalInfile;

	public static readonly MySqlConnectionStringValueOption<bool> AllowPublicKeyRetrieval;

	public static readonly MySqlConnectionStringValueOption<bool> AllowUserVariables;

	public static readonly MySqlConnectionStringValueOption<bool> AllowZeroDateTime;

	public static readonly MySqlConnectionStringReferenceOption<string> ApplicationName;

	public static readonly MySqlConnectionStringValueOption<bool> AutoEnlist;

	public static readonly MySqlConnectionStringValueOption<int> CancellationTimeout;

	public static readonly MySqlConnectionStringReferenceOption<string> CharacterSet;

	public static readonly MySqlConnectionStringValueOption<uint> ConnectionTimeout;

	public static readonly MySqlConnectionStringValueOption<bool> ConvertZeroDateTime;

	public static readonly MySqlConnectionStringValueOption<MySqlDateTimeKind> DateTimeKind;

	public static readonly MySqlConnectionStringValueOption<uint> DefaultCommandTimeout;

	public static readonly MySqlConnectionStringValueOption<bool> ForceSynchronous;

	public static readonly MySqlConnectionStringValueOption<MySqlGuidFormat> GuidFormat;

	public static readonly MySqlConnectionStringValueOption<bool> IgnoreCommandTransaction;

	public static readonly MySqlConnectionStringValueOption<bool> IgnorePrepare;

	public static readonly MySqlConnectionStringValueOption<bool> InteractiveSession;

	public static readonly MySqlConnectionStringValueOption<uint> Keepalive;

	public static readonly MySqlConnectionStringValueOption<bool> NoBackslashEscapes;

	public static readonly MySqlConnectionStringValueOption<bool> OldGuids;

	public static readonly MySqlConnectionStringValueOption<bool> PersistSecurityInfo;

	public static readonly MySqlConnectionStringValueOption<bool> Pipelining;

	public static readonly MySqlConnectionStringValueOption<MySqlServerRedirectionMode> ServerRedirectionMode;

	public static readonly MySqlConnectionStringReferenceOption<string> ServerRsaPublicKeyFile;

	public static readonly MySqlConnectionStringReferenceOption<string> ServerSPN;

	public static readonly MySqlConnectionStringValueOption<bool> TreatTinyAsBoolean;

	public static readonly MySqlConnectionStringValueOption<bool> UseAffectedRows;

	public static readonly MySqlConnectionStringValueOption<bool> UseCompression;

	public static readonly MySqlConnectionStringValueOption<bool> UseXaTransactions;

	private const string c_tlsVersionsRegexPattern = "\\s*TLS( ?v?(1|1\\.?0|1\\.?1|1\\.?2|1\\.?3))?$";

	private static readonly Regex s_tlsVersionsRegex;

	private static readonly Dictionary<string, MySqlConnectionStringOption> s_options;

	private readonly IReadOnlyList<string> m_keys;

	public static List<string> OptionNames { get; }

	public string Key => m_keys[0];

	public IReadOnlyList<string> Keys => m_keys;

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public static MySqlConnectionStringOption TryGetOptionForKey(string key)
	{
		if (!s_options.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public static MySqlConnectionStringOption GetOptionForKey(string key)
	{
		return TryGetOptionForKey(key) ?? throw new ArgumentException("Option '" + key + "' not supported.");
	}

	public abstract object GetObject(MySqlConnectionStringBuilder builder);

	public abstract void SetObject(MySqlConnectionStringBuilder builder, object value);

	protected MySqlConnectionStringOption(IReadOnlyList<string> keys)
	{
		m_keys = keys;
	}

	private static void AddOption(Dictionary<string, MySqlConnectionStringOption> options, MySqlConnectionStringOption option)
	{
		foreach (string key in option.m_keys)
		{
			options.Add(key, option);
		}
		OptionNames.Add(option.m_keys[0]);
	}

	static MySqlConnectionStringOption()
	{
		OptionNames = new List<string>();
		s_tlsVersionsRegex = new Regex("\\s*TLS( ?v?(1|1\\.?0|1\\.?1|1\\.?2|1\\.?3))?$", RegexOptions.IgnoreCase);
		Dictionary<string, MySqlConnectionStringOption> options = new Dictionary<string, MySqlConnectionStringOption>(StringComparer.OrdinalIgnoreCase);
		AddOption(options, Server = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[7] { "Server", "Host", "Data Source", "DataSource", "Address", "Addr", "Network Address" }), ""));
		AddOption(options, Port = new MySqlConnectionStringValueOption<uint>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[1] { "Port" }), 3306u));
		AddOption(options, UserID = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[6] { "User ID", "UserID", "Username", "Uid", "User name", "User" }), ""));
		AddOption(options, Password = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Password", "pwd" }), ""));
		AddOption(options, Database = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Database", "Initial Catalog" }), ""));
		AddOption(options, LoadBalance = new MySqlConnectionStringValueOption<MySqlLoadBalance>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Load Balance", "LoadBalance" }), MySqlLoadBalance.RoundRobin));
		AddOption(options, ConnectionProtocol = new MySqlConnectionStringValueOption<MySqlConnectionProtocol>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "Connection Protocol", "ConnectionProtocol", "Protocol" }), MySqlConnectionProtocol.Sockets));
		AddOption(options, PipeName = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "Pipe Name", "PipeName", "Pipe" }), "MYSQL"));
		AddOption(options, SslMode = new MySqlConnectionStringValueOption<MySqlSslMode>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "SSL Mode", "SslMode" }), MySqlSslMode.Preferred));
		AddOption(options, CertificateFile = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Certificate File", "CertificateFile" }), ""));
		AddOption(options, CertificatePassword = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Certificate Password", "CertificatePassword" }), ""));
		AddOption(options, CertificateStoreLocation = new MySqlConnectionStringValueOption<MySqlCertificateStoreLocation>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Certificate Store Location", "CertificateStoreLocation" }), MySqlCertificateStoreLocation.None));
		AddOption(options, CertificateThumbprint = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "Certificate Thumbprint", "CertificateThumbprint", "Certificate Thumb Print" }), ""));
		AddOption(options, SslCert = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "SSL Cert", "SslCert", "Ssl-Cert" }), ""));
		AddOption(options, SslKey = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "SSL Key", "SslKey", "Ssl-Key" }), ""));
		AddOption(options, SslCa = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[5] { "SSL CA", "CACertificateFile", "CA Certificate File", "SslCa", "Ssl-Ca" }), ""));
		AddOption(options, TlsVersion = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "TLS Version", "TlsVersion", "Tls-Version" }), "", ([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] string value) =>
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return "";
			}
			Span<bool> span = stackalloc bool[4];
			string[] array = value.TrimStart(new char[2] { '[', '(' }).TrimEnd(new char[2] { ')', ']' }).Split(new char[1] { ',' });
			foreach (string text in array)
			{
				Match match = TlsVersionsRegex().Match(text);
				if (!match.Success)
				{
					throw new ArgumentException("Unrecognized TlsVersion protocol version '" + text + "'; permitted versions are: TLS 1.0, TLS 1.1, TLS 1.2, TLS 1.3.");
				}
				string value2 = match.Groups[2].Value;
				if (value2 == null || value2.Length != 0)
				{
					switch (value2)
					{
					case "1":
					case "10":
					case "1.0":
						break;
					default:
						goto IL_00da;
					}
				}
				bool flag = true;
				goto IL_00dd;
				IL_00da:
				flag = false;
				goto IL_00dd;
				IL_00dd:
				if (flag)
				{
					span[0] = true;
				}
				else if ((value2 == "11" || value2 == "1.1") ? true : false)
				{
					span[1] = true;
				}
				else if ((value2 == "12" || value2 == "1.2") ? true : false)
				{
					span[2] = true;
				}
				else if ((value2 == "13" || value2 == "1.3") ? true : false)
				{
					span[3] = true;
				}
			}
			string text2 = "";
			_ = stackalloc char[7];
			for (int i = 0; i < span.Length; i++)
			{
				if (span[i])
				{
					if (text2.Length != 0)
					{
						text2 += ", ";
					}
					text2 += FormattableString.Invariant($"TLS 1.{i}");
				}
			}
			return text2;
		}));
		AddOption(options, TlsCipherSuites = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "TLS Cipher Suites", "TlsCipherSuites" }), ""));
		AddOption(options, Pooling = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[1] { "Pooling" }), defaultValue: true));
		AddOption(options, ConnectionLifeTime = new MySqlConnectionStringValueOption<uint>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Connection Lifetime", "ConnectionLifeTime" }), 0u));
		AddOption(options, ConnectionReset = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Connection Reset", "ConnectionReset" }), defaultValue: true));
		AddOption(options, DeferConnectionReset = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Defer Connection Reset", "DeferConnectionReset" }), defaultValue: true));
		AddOption(options, ConnectionIdlePingTime = new MySqlConnectionStringValueOption<uint>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Connection Idle Ping Time", "ConnectionIdlePingTime" }), 0u));
		AddOption(options, ConnectionIdleTimeout = new MySqlConnectionStringValueOption<uint>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Connection Idle Timeout", "ConnectionIdleTimeout" }), 180u));
		AddOption(options, MinimumPoolSize = new MySqlConnectionStringValueOption<uint>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[4] { "Minimum Pool Size", "Min Pool Size", "MinimumPoolSize", "minpoolsize" }), 0u));
		AddOption(options, MaximumPoolSize = new MySqlConnectionStringValueOption<uint>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[4] { "Maximum Pool Size", "Max Pool Size", "MaximumPoolSize", "maxpoolsize" }), 100u));
		AddOption(options, DnsCheckInterval = new MySqlConnectionStringValueOption<uint>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "DNS Check Interval", "DnsCheckInterval" }), 0u));
		AddOption(options, AllowLoadLocalInfile = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Allow Load Local Infile", "AllowLoadLocalInfile" }), defaultValue: false));
		AddOption(options, AllowPublicKeyRetrieval = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Allow Public Key Retrieval", "AllowPublicKeyRetrieval" }), defaultValue: false));
		AddOption(options, AllowUserVariables = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Allow User Variables", "AllowUserVariables" }), defaultValue: false));
		AddOption(options, AllowZeroDateTime = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Allow Zero DateTime", "AllowZeroDateTime" }), defaultValue: false));
		AddOption(options, ApplicationName = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Application Name", "ApplicationName" }), ""));
		AddOption(options, AutoEnlist = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Auto Enlist", "AutoEnlist" }), defaultValue: true));
		AddOption(options, CancellationTimeout = new MySqlConnectionStringValueOption<int>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Cancellation Timeout", "CancellationTimeout" }), 2, delegate(int x)
		{
			if (x < -1)
			{
				throw new ArgumentOutOfRangeException("CancellationTimeout", "CancellationTimeout must be greater than or equal to -1");
			}
			return x;
		}));
		AddOption(options, CharacterSet = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "Character Set", "CharSet", "CharacterSet" }), ""));
		AddOption(options, ConnectionTimeout = new MySqlConnectionStringValueOption<uint>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "Connection Timeout", "ConnectionTimeout", "Connect Timeout" }), 15u));
		AddOption(options, ConvertZeroDateTime = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Convert Zero DateTime", "ConvertZeroDateTime" }), defaultValue: false));
		AddOption(options, DateTimeKind = new MySqlConnectionStringValueOption<MySqlDateTimeKind>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "DateTime Kind", "DateTimeKind" }), MySqlDateTimeKind.Unspecified));
		AddOption(options, DefaultCommandTimeout = new MySqlConnectionStringValueOption<uint>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "Default Command Timeout", "DefaultCommandTimeout", "Command Timeout" }), 30u));
		AddOption(options, ForceSynchronous = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Force Synchronous", "ForceSynchronous" }), defaultValue: false));
		AddOption(options, GuidFormat = new MySqlConnectionStringValueOption<MySqlGuidFormat>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "GUID Format", "GuidFormat" }), MySqlGuidFormat.Default));
		AddOption(options, IgnoreCommandTransaction = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Ignore Command Transaction", "IgnoreCommandTransaction" }), defaultValue: false));
		AddOption(options, IgnorePrepare = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Ignore Prepare", "IgnorePrepare" }), defaultValue: false));
		AddOption(options, InteractiveSession = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "Interactive Session", "InteractiveSession", "Interactive" }), defaultValue: false));
		AddOption(options, Keepalive = new MySqlConnectionStringValueOption<uint>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Keep Alive", "Keepalive" }), 0u));
		AddOption(options, NoBackslashEscapes = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "No Backslash Escapes", "NoBackslashEscapes" }), defaultValue: false));
		AddOption(options, OldGuids = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Old Guids", "OldGuids" }), defaultValue: false));
		AddOption(options, PersistSecurityInfo = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Persist Security Info", "PersistSecurityInfo" }), defaultValue: false));
		AddOption(options, Pipelining = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[1] { "Pipelining" }), defaultValue: true));
		AddOption(options, ServerRedirectionMode = new MySqlConnectionStringValueOption<MySqlServerRedirectionMode>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Server Redirection Mode", "ServerRedirectionMode" }), MySqlServerRedirectionMode.Disabled));
		AddOption(options, ServerRsaPublicKeyFile = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Server RSA Public Key File", "ServerRsaPublicKeyFile" }), ""));
		AddOption(options, ServerSPN = new MySqlConnectionStringReferenceOption<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Server SPN", "ServerSPN" }), ""));
		AddOption(options, TreatTinyAsBoolean = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Treat Tiny As Boolean", "TreatTinyAsBoolean" }), defaultValue: true));
		AddOption(options, UseAffectedRows = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Use Affected Rows", "UseAffectedRows" }), defaultValue: false));
		AddOption(options, UseCompression = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "Use Compression", "Compress", "UseCompression" }), defaultValue: false));
		AddOption(options, UseXaTransactions = new MySqlConnectionStringValueOption<bool>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "Use XA Transactions", "UseXaTransactions" }), defaultValue: true));
		s_options = options;
	}

	private static Regex TlsVersionsRegex()
	{
		return s_tlsVersionsRegex;
	}
}
