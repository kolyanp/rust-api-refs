using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using MySqlConnector.Utilities;

namespace MySqlConnector.Core;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class ConnectionSettings
{
	private int? m_connectionTimeoutMilliseconds;

	private static readonly string[] s_localhostPipeServer = new string[1] { "." };

	public MySqlConnectionStringBuilder ConnectionStringBuilder { get; }

	public string ConnectionString { get; }

	public MySqlConnectionProtocol ConnectionProtocol { get; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
	public IReadOnlyList<string> HostNames
	{
		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
		get;
	}

	public MySqlLoadBalance LoadBalance { get; }

	public int Port { get; }

	public string PipeName { get; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public string UnixSocket
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
	}

	public string UserID { get; }

	public string Password { get; }

	public string Database { get; }

	public MySqlSslMode SslMode { get; }

	public string CertificateFile { get; }

	public string CertificatePassword { get; }

	public string CACertificateFile { get; }

	public string SslCertificateFile { get; }

	public string SslKeyFile { get; }

	public MySqlCertificateStoreLocation CertificateStoreLocation { get; }

	public string CertificateThumbprint { get; }

	public SslProtocols TlsVersions { get; }

	public bool Pooling { get; }

	public uint ConnectionLifeTime { get; }

	public bool ConnectionReset { get; }

	public int ConnectionIdleTimeout { get; }

	public int MinimumPoolSize { get; }

	public int MaximumPoolSize { get; }

	public int DnsCheckInterval { get; }

	public bool AllowLoadLocalInfile { get; }

	public bool AllowPublicKeyRetrieval { get; }

	public bool AllowUserVariables { get; }

	public bool AllowZeroDateTime { get; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public string ApplicationName
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
	}

	public bool AutoEnlist { get; }

	public int CancellationTimeout { get; }

	public int ConnectionTimeout { get; }

	public bool ConvertZeroDateTime { get; }

	public DateTimeKind DateTimeKind { get; }

	public int DefaultCommandTimeout { get; }

	public bool ForceSynchronous { get; }

	public MySqlGuidFormat GuidFormat { get; }

	public bool IgnoreCommandTransaction { get; }

	public bool IgnorePrepare { get; }

	public bool InteractiveSession { get; }

	public uint Keepalive { get; }

	public bool NoBackslashEscapes { get; }

	public bool PersistSecurityInfo { get; }

	public bool? Pipelining { get; }

	public MySqlServerRedirectionMode ServerRedirectionMode { get; }

	public string ServerRsaPublicKeyFile { get; }

	public string ServerSPN { get; }

	public bool TreatTinyAsBoolean { get; }

	public bool UseAffectedRows { get; }

	public bool UseCompression { get; }

	public bool UseXaTransactions { get; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public byte[] ConnectionAttributes
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		set;
	}

	public int ConnectionTimeoutMilliseconds
	{
		get
		{
			if (!m_connectionTimeoutMilliseconds.HasValue)
			{
				try
				{
					m_connectionTimeoutMilliseconds = checked(ConnectionTimeout * 1000);
				}
				catch (OverflowException)
				{
					m_connectionTimeoutMilliseconds = int.MaxValue;
				}
			}
			return m_connectionTimeoutMilliseconds.Value;
		}
	}

	public ConnectionSettings(MySqlConnectionStringBuilder csb)
	{
		ConnectionStringBuilder = csb;
		ConnectionString = csb.ConnectionString;
		if (csb.ConnectionProtocol == MySqlConnectionProtocol.UnixSocket || (!Utility.IsWindows() && (Utility.StartsWith(csb.Server, '/') || csb.Server.StartsWith("./", StringComparison.Ordinal))))
		{
			if (csb.LoadBalance != MySqlLoadBalance.RoundRobin)
			{
				throw new NotSupportedException("LoadBalance not supported when ConnectionProtocol=UnixSocket");
			}
			if (!File.Exists(csb.Server))
			{
				throw new MySqlException("Cannot find Unix Socket at " + csb.Server);
			}
			ConnectionProtocol = MySqlConnectionProtocol.UnixSocket;
			UnixSocket = Path.GetFullPath(csb.Server);
			PipeName = "";
		}
		else if (csb.ConnectionProtocol == MySqlConnectionProtocol.Pipe)
		{
			if (csb.LoadBalance != MySqlLoadBalance.RoundRobin)
			{
				throw new NotSupportedException("LoadBalance not supported when ConnectionProtocol=NamedPipe");
			}
			ConnectionProtocol = MySqlConnectionProtocol.Pipe;
			HostNames = ((csb.Server == "." || string.Equals(csb.Server, "localhost", StringComparison.OrdinalIgnoreCase)) ? s_localhostPipeServer : new string[1] { csb.Server });
			PipeName = csb.PipeName;
		}
		else
		{
			if (csb.ConnectionProtocol == MySqlConnectionProtocol.SharedMemory)
			{
				throw new NotSupportedException("Shared Memory connections are not supported");
			}
			ConnectionProtocol = MySqlConnectionProtocol.Sockets;
			HostNames = csb.Server.Split(new char[1] { ',' });
			LoadBalance = csb.LoadBalance;
			Port = (int)csb.Port;
			PipeName = "";
		}
		UserID = csb.UserID;
		Password = csb.Password;
		Database = csb.Database;
		SslMode = csb.SslMode;
		CertificateFile = csb.CertificateFile;
		CertificatePassword = csb.CertificatePassword;
		SslCertificateFile = csb.SslCert;
		SslKeyFile = csb.SslKey;
		CACertificateFile = csb.SslCa;
		CertificateStoreLocation = csb.CertificateStoreLocation;
		CertificateThumbprint = csb.CertificateThumbprint;
		if (csb.TlsVersion.Length == 0)
		{
			TlsVersions = Utility.GetDefaultSslProtocols();
		}
		else
		{
			TlsVersions = SslProtocols.None;
			for (int i = 6; i < csb.TlsVersion.Length; i += 9)
			{
				char c = csb.TlsVersion[i];
				switch (c)
				{
				case '0':
					TlsVersions |= SslProtocols.Tls;
					break;
				case '1':
					TlsVersions |= SslProtocols.Tls11;
					break;
				case '2':
					TlsVersions |= SslProtocols.Tls12;
					break;
				case '3':
					TlsVersions |= SslProtocols.Tls13;
					break;
				default:
					throw new InvalidOperationException($"Unexpected character '{c}' for TLS minor version.");
				}
			}
			if (TlsVersions == SslProtocols.None)
			{
				throw new NotSupportedException("All specified TLS versions are incompatible with this platform.");
			}
		}
		if (csb.TlsCipherSuites.Length != 0)
		{
			throw new PlatformNotSupportedException("The TlsCipherSuites connection string option is only supported on .NET Core 3.1 (or later) on Linux.");
		}
		Pooling = csb.Pooling;
		ConnectionLifeTime = Math.Min(csb.ConnectionLifeTime, 4294967u) * 1000;
		ConnectionReset = csb.ConnectionReset;
		ConnectionIdleTimeout = (int)csb.ConnectionIdleTimeout;
		if (csb.MinimumPoolSize > csb.MaximumPoolSize)
		{
			throw new MySqlException("MaximumPoolSize must be greater than or equal to MinimumPoolSize");
		}
		MinimumPoolSize = ToSigned(csb.MinimumPoolSize);
		MaximumPoolSize = ToSigned(csb.MaximumPoolSize);
		DnsCheckInterval = ToSigned(csb.DnsCheckInterval);
		AllowLoadLocalInfile = csb.AllowLoadLocalInfile;
		AllowPublicKeyRetrieval = csb.AllowPublicKeyRetrieval;
		AllowUserVariables = csb.AllowUserVariables;
		AllowZeroDateTime = csb.AllowZeroDateTime;
		string applicationName = csb.ApplicationName;
		ApplicationName = ((applicationName != null && applicationName.Length == 0) ? null : csb.ApplicationName);
		AutoEnlist = csb.AutoEnlist;
		CancellationTimeout = csb.CancellationTimeout;
		ConnectionTimeout = ToSigned(csb.ConnectionTimeout);
		ConvertZeroDateTime = csb.ConvertZeroDateTime;
		DateTimeKind = (DateTimeKind)csb.DateTimeKind;
		DefaultCommandTimeout = ToSigned(csb.DefaultCommandTimeout);
		ForceSynchronous = csb.ForceSynchronous;
		IgnoreCommandTransaction = csb.IgnoreCommandTransaction;
		IgnorePrepare = csb.IgnorePrepare;
		InteractiveSession = csb.InteractiveSession;
		GuidFormat = GetEffectiveGuidFormat(csb.GuidFormat, csb.OldGuids);
		Keepalive = csb.Keepalive;
		NoBackslashEscapes = csb.NoBackslashEscapes;
		PersistSecurityInfo = csb.PersistSecurityInfo;
		Pipelining = (csb.ContainsKey("Pipelining") ? new bool?(csb.Pipelining) : ((bool?)null));
		ServerRedirectionMode = csb.ServerRedirectionMode;
		ServerRsaPublicKeyFile = csb.ServerRsaPublicKeyFile;
		ServerSPN = csb.ServerSPN;
		TreatTinyAsBoolean = csb.TreatTinyAsBoolean;
		UseAffectedRows = csb.UseAffectedRows;
		UseCompression = csb.UseCompression;
		UseXaTransactions = csb.UseXaTransactions;
		static int ToSigned(uint value)
		{
			if (value < int.MaxValue)
			{
				return (int)value;
			}
			return int.MaxValue;
		}
	}

	public ConnectionSettings CloneWith(string host, int port, string userId)
	{
		return new ConnectionSettings(this, host, port, userId);
	}

	private static MySqlGuidFormat GetEffectiveGuidFormat(MySqlGuidFormat guidFormat, bool oldGuids)
	{
		switch (guidFormat)
		{
		case MySqlGuidFormat.Default:
			if (!oldGuids)
			{
				return MySqlGuidFormat.Char36;
			}
			return MySqlGuidFormat.LittleEndianBinary16;
		case MySqlGuidFormat.None:
		case MySqlGuidFormat.Char36:
		case MySqlGuidFormat.Char32:
		case MySqlGuidFormat.Binary16:
		case MySqlGuidFormat.TimeSwapBinary16:
		case MySqlGuidFormat.LittleEndianBinary16:
			if (oldGuids)
			{
				throw new MySqlException("OldGuids cannot be used with GuidFormat");
			}
			return guidFormat;
		default:
			throw new MySqlException("Unknown GuidFormat");
		}
	}

	private ConnectionSettings(ConnectionSettings other, string host, int port, string userId)
	{
		ConnectionStringBuilder = other.ConnectionStringBuilder;
		ConnectionString = other.ConnectionString;
		ConnectionProtocol = MySqlConnectionProtocol.Sockets;
		HostNames = new global::_003C_003Ez__ReadOnlyArray<string>(new string[1] { host });
		LoadBalance = other.LoadBalance;
		Port = port;
		PipeName = other.PipeName;
		UserID = userId;
		Password = other.Password;
		Database = other.Database;
		SslMode = other.SslMode;
		CertificateFile = other.CertificateFile;
		CertificatePassword = other.CertificatePassword;
		SslCertificateFile = other.SslCertificateFile;
		SslKeyFile = other.SslKeyFile;
		CACertificateFile = other.CACertificateFile;
		CertificateStoreLocation = other.CertificateStoreLocation;
		CertificateThumbprint = other.CertificateThumbprint;
		Pooling = other.Pooling;
		ConnectionLifeTime = other.ConnectionLifeTime;
		ConnectionReset = other.ConnectionReset;
		ConnectionIdleTimeout = other.ConnectionIdleTimeout;
		MinimumPoolSize = other.MinimumPoolSize;
		MaximumPoolSize = other.MaximumPoolSize;
		DnsCheckInterval = other.DnsCheckInterval;
		AllowLoadLocalInfile = other.AllowLoadLocalInfile;
		AllowPublicKeyRetrieval = other.AllowPublicKeyRetrieval;
		AllowUserVariables = other.AllowUserVariables;
		AllowZeroDateTime = other.AllowZeroDateTime;
		ApplicationName = other.ApplicationName;
		AutoEnlist = other.AutoEnlist;
		ConnectionTimeout = other.ConnectionTimeout;
		ConvertZeroDateTime = other.ConvertZeroDateTime;
		DateTimeKind = other.DateTimeKind;
		DefaultCommandTimeout = other.DefaultCommandTimeout;
		ForceSynchronous = other.ForceSynchronous;
		IgnoreCommandTransaction = other.IgnoreCommandTransaction;
		IgnorePrepare = other.IgnorePrepare;
		InteractiveSession = other.InteractiveSession;
		GuidFormat = other.GuidFormat;
		Keepalive = other.Keepalive;
		NoBackslashEscapes = other.NoBackslashEscapes;
		PersistSecurityInfo = other.PersistSecurityInfo;
		Pipelining = other.Pipelining;
		ServerRedirectionMode = other.ServerRedirectionMode;
		ServerRsaPublicKeyFile = other.ServerRsaPublicKeyFile;
		ServerSPN = other.ServerSPN;
		TreatTinyAsBoolean = other.TreatTinyAsBoolean;
		UseAffectedRows = other.UseAffectedRows;
		UseCompression = other.UseCompression;
		UseXaTransactions = other.UseXaTransactions;
	}
}
