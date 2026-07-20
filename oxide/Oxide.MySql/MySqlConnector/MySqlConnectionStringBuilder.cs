using System;
using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MySqlConnector;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
public sealed class MySqlConnectionStringBuilder : DbConnectionStringBuilder
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private string m_cachedConnectionString;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private string m_cachedConnectionStringWithoutPassword;

	[DefaultValue("")]
	[Description("The host name or network address of the MySQL Server to which to connect.")]
	[DisplayName("Server")]
	[Category("Connection")]
	public string Server
	{
		get
		{
			return MySqlConnectionStringOption.Server.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.Server.SetValue(this, value);
		}
	}

	[DefaultValue(3306L)]
	[DisplayName("Port")]
	[Description("The TCP port on which MySQL Server is listening for connections.")]
	[Category("Connection")]
	public uint Port
	{
		get
		{
			return MySqlConnectionStringOption.Port.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.Port.SetValue(this, value);
		}
	}

	[Category("Connection")]
	[DefaultValue("")]
	[DisplayName("User ID")]
	[Description("The MySQL user ID.")]
	public string UserID
	{
		get
		{
			return MySqlConnectionStringOption.UserID.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.UserID.SetValue(this, value);
		}
	}

	[DisplayName("Password")]
	[DefaultValue("")]
	[Description("The password for the MySQL user.")]
	[Category("Connection")]
	public string Password
	{
		get
		{
			return MySqlConnectionStringOption.Password.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.Password.SetValue(this, value);
		}
	}

	[Description("The case-sensitive name of the initial database to use")]
	[Category("Connection")]
	[DisplayName("Database")]
	[DefaultValue("The case-sensitive name of the initial database to use.")]
	public string Database
	{
		get
		{
			return MySqlConnectionStringOption.Database.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.Database.SetValue(this, value);
		}
	}

	[Description("Specifies how load is distributed across backend servers.")]
	[DefaultValue(MySqlLoadBalance.RoundRobin)]
	[Category("Connection")]
	[DisplayName("Load Balance")]
	public MySqlLoadBalance LoadBalance
	{
		get
		{
			return MySqlConnectionStringOption.LoadBalance.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.LoadBalance.SetValue(this, value);
		}
	}

	[Description("The protocol to use to connect to the MySQL Server.")]
	[DisplayName("Connection Protocol")]
	[Category("Connection")]
	[DefaultValue(MySqlConnectionProtocol.Sockets)]
	public MySqlConnectionProtocol ConnectionProtocol
	{
		get
		{
			return MySqlConnectionStringOption.ConnectionProtocol.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.ConnectionProtocol.SetValue(this, value);
		}
	}

	[DefaultValue("MYSQL")]
	[Category("Connection")]
	[DisplayName("Pipe Name")]
	[Description("The name of the Windows named pipe to use to connect to the server.")]
	public string PipeName
	{
		get
		{
			return MySqlConnectionStringOption.PipeName.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.PipeName.SetValue(this, value);
		}
	}

	[Description("Whether to use SSL/TLS when connecting to the MySQL server.")]
	[DefaultValue(MySqlSslMode.Preferred)]
	[Category("TLS")]
	[DisplayName("SSL Mode")]
	public MySqlSslMode SslMode
	{
		get
		{
			return MySqlConnectionStringOption.SslMode.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.SslMode.SetValue(this, value);
		}
	}

	[DefaultValue("")]
	[Category("TLS")]
	[DisplayName("Certificate File")]
	[Description("The path to a certificate file in PKCS #12 (.pfx) format containing a bundled Certificate and Private Key used for mutual authentication.")]
	public string CertificateFile
	{
		get
		{
			return MySqlConnectionStringOption.CertificateFile.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.CertificateFile.SetValue(this, value);
		}
	}

	[Category("TLS")]
	[DefaultValue("")]
	[Description("The password for the certificate specified using the Certificate File option.")]
	[DisplayName("Certificate Password")]
	public string CertificatePassword
	{
		get
		{
			return MySqlConnectionStringOption.CertificatePassword.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.CertificatePassword.SetValue(this, value);
		}
	}

	[Description("Uses a certificate from the specified Certificate Store on the machine.")]
	[Category("TLS")]
	[DefaultValue(MySqlCertificateStoreLocation.None)]
	[DisplayName("Certificate Store Location")]
	public MySqlCertificateStoreLocation CertificateStoreLocation
	{
		get
		{
			return MySqlConnectionStringOption.CertificateStoreLocation.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.CertificateStoreLocation.SetValue(this, value);
		}
	}

	[DefaultValue("")]
	[DisplayName("Certificate Thumbprint")]
	[Description("Specifies which certificate should be used from the certificate store specified in Certificate Store Location")]
	[Category("TLS")]
	public string CertificateThumbprint
	{
		get
		{
			return MySqlConnectionStringOption.CertificateThumbprint.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.CertificateThumbprint.SetValue(this, value);
		}
	}

	[Description("The path to the client’s SSL certificate file in PEM format.")]
	[DisplayName("SSL Cert")]
	[DefaultValue("")]
	[Category("TLS")]
	public string SslCert
	{
		get
		{
			return MySqlConnectionStringOption.SslCert.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.SslCert.SetValue(this, value);
		}
	}

	[Description("The path to the client’s SSL private key in PEM format.")]
	[DisplayName("SSL Key")]
	[Category("TLS")]
	[DefaultValue("")]
	public string SslKey
	{
		get
		{
			return MySqlConnectionStringOption.SslKey.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.SslKey.SetValue(this, value);
		}
	}

	[DisplayName("CA Certificate File")]
	[Category("Obsolete")]
	[Obsolete("Use SslCa instead.")]
	[Browsable(false)]
	public string CACertificateFile
	{
		get
		{
			return MySqlConnectionStringOption.SslCa.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.SslCa.SetValue(this, value);
		}
	}

	[DefaultValue("")]
	[DisplayName("SSL CA")]
	[Description("The path to a CA certificate file in a PEM Encoded (.pem) format.")]
	[Category("TLS")]
	public string SslCa
	{
		get
		{
			return MySqlConnectionStringOption.SslCa.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.SslCa.SetValue(this, value);
		}
	}

	[DefaultValue("")]
	[DisplayName("TLS Version")]
	[Description("The TLS versions which may be used during TLS negotiation.")]
	[Category("TLS")]
	public string TlsVersion
	{
		get
		{
			return MySqlConnectionStringOption.TlsVersion.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.TlsVersion.SetValue(this, value);
		}
	}

	[DisplayName("TLS Cipher Suites")]
	[DefaultValue("")]
	[Category("TLS")]
	[Description("The TLS cipher suites which may be used during TLS negotiation.")]
	public string TlsCipherSuites
	{
		get
		{
			return MySqlConnectionStringOption.TlsCipherSuites.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.TlsCipherSuites.SetValue(this, value);
		}
	}

	[DisplayName("Pooling")]
	[Description("Enables connection pooling.")]
	[Category("Pooling")]
	[DefaultValue(true)]
	public bool Pooling
	{
		get
		{
			return MySqlConnectionStringOption.Pooling.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.Pooling.SetValue(this, value);
		}
	}

	[DisplayName("Connection Lifetime")]
	[DefaultValue(0L)]
	[Category("Pooling")]
	[Description("The maximum lifetime (in seconds) for any connection, or 0 for no lifetime limit.")]
	public uint ConnectionLifeTime
	{
		get
		{
			return MySqlConnectionStringOption.ConnectionLifeTime.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.ConnectionLifeTime.SetValue(this, value);
		}
	}

	[DefaultValue(true)]
	[Category("Pooling")]
	[DisplayName("Connection Reset")]
	[Description("Whether connections are reset when being retrieved from the pool.")]
	public bool ConnectionReset
	{
		get
		{
			return MySqlConnectionStringOption.ConnectionReset.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.ConnectionReset.SetValue(this, value);
		}
	}

	[DefaultValue(true)]
	[Category("Obsolete")]
	[DisplayName("Defer Connection Reset")]
	[Obsolete("This option is no longer supported in MySqlConnector >= 1.4.0.")]
	public bool DeferConnectionReset
	{
		get
		{
			return MySqlConnectionStringOption.DeferConnectionReset.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.DeferConnectionReset.SetValue(this, value);
		}
	}

	[DefaultValue(0L)]
	[Category("Obsolete")]
	[DisplayName("Connection Idle Ping Time")]
	[Obsolete("This option is no longer supported in MySqlConnector >= 1.4.0.")]
	public uint ConnectionIdlePingTime
	{
		get
		{
			return MySqlConnectionStringOption.ConnectionIdlePingTime.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.ConnectionIdlePingTime.SetValue(this, value);
		}
	}

	[DefaultValue(180L)]
	[Category("Pooling")]
	[Description("The amount of time (in seconds) that a connection can remain idle in the pool.")]
	[DisplayName("Connection Idle Timeout")]
	public uint ConnectionIdleTimeout
	{
		get
		{
			return MySqlConnectionStringOption.ConnectionIdleTimeout.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.ConnectionIdleTimeout.SetValue(this, value);
		}
	}

	[Category("Pooling")]
	[DisplayName("Minimum Pool Size")]
	[Description("The minimum number of connections to leave in the pool if Connection Idle Timeout is reached.")]
	[DefaultValue(0L)]
	public uint MinimumPoolSize
	{
		get
		{
			return MySqlConnectionStringOption.MinimumPoolSize.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.MinimumPoolSize.SetValue(this, value);
		}
	}

	[DefaultValue(100L)]
	[DisplayName("Maximum Pool Size")]
	[Category("Pooling")]
	[Description("The maximum number of connections allowed in the pool.")]
	public uint MaximumPoolSize
	{
		get
		{
			return MySqlConnectionStringOption.MaximumPoolSize.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.MaximumPoolSize.SetValue(this, value);
		}
	}

	[Description("The number of seconds between checks for DNS changes.")]
	[DefaultValue(0L)]
	[Category("Pooling")]
	[DisplayName("DNS Check Interval")]
	public uint DnsCheckInterval
	{
		get
		{
			return MySqlConnectionStringOption.DnsCheckInterval.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.DnsCheckInterval.SetValue(this, value);
		}
	}

	[DefaultValue(false)]
	[Category("Other")]
	[Description("Allows the LOAD DATA LOCAL command to request files from the client.")]
	[DisplayName("Allow Load Local Infile")]
	public bool AllowLoadLocalInfile
	{
		get
		{
			return MySqlConnectionStringOption.AllowLoadLocalInfile.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.AllowLoadLocalInfile.SetValue(this, value);
		}
	}

	[Description("Allows the client to automatically request the RSA public key from the server.")]
	[DisplayName("Allow Public Key Retrieval")]
	[Category("Other")]
	[DefaultValue(false)]
	public bool AllowPublicKeyRetrieval
	{
		get
		{
			return MySqlConnectionStringOption.AllowPublicKeyRetrieval.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.AllowPublicKeyRetrieval.SetValue(this, value);
		}
	}

	[DisplayName("Allow User Variables")]
	[Description("Allows user-defined variables (prefixed with @) to be used in SQL statements.")]
	[DefaultValue(false)]
	[Category("Other")]
	public bool AllowUserVariables
	{
		get
		{
			return MySqlConnectionStringOption.AllowUserVariables.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.AllowUserVariables.SetValue(this, value);
		}
	}

	[DisplayName("Allow Zero DateTime")]
	[Description("Returns DATETIME fields as MySqlDateTime objects instead of DateTime objects.")]
	[Category("Other")]
	[DefaultValue(false)]
	public bool AllowZeroDateTime
	{
		get
		{
			return MySqlConnectionStringOption.AllowZeroDateTime.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.AllowZeroDateTime.SetValue(this, value);
		}
	}

	[DefaultValue("")]
	[Category("Other")]
	[Description("Sets the program_name connection attribute passed to MySQL Server.")]
	[DisplayName("Application Name")]
	public string ApplicationName
	{
		get
		{
			return MySqlConnectionStringOption.ApplicationName.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.ApplicationName.SetValue(this, value);
		}
	}

	[DisplayName("Auto Enlist")]
	[Category("Other")]
	[DefaultValue(true)]
	[Description("Automatically enlists this connection in any active TransactionScope.")]
	public bool AutoEnlist
	{
		get
		{
			return MySqlConnectionStringOption.AutoEnlist.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.AutoEnlist.SetValue(this, value);
		}
	}

	[DefaultValue(2)]
	[Category("Other")]
	[Description("The length of time (in seconds) to wait for a query to be canceled when MySqlCommand.CommandTimeout expires, or zero for no timeout.")]
	[DisplayName("Cancellation Timeout")]
	public int CancellationTimeout
	{
		get
		{
			return MySqlConnectionStringOption.CancellationTimeout.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.CancellationTimeout.SetValue(this, value);
		}
	}

	[DisplayName("Character Set")]
	[DefaultValue("")]
	[Category("Obsolete")]
	public string CharacterSet
	{
		get
		{
			return MySqlConnectionStringOption.CharacterSet.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.CharacterSet.SetValue(this, value);
		}
	}

	[Description("The length of time (in seconds) to wait for a connection to the server before terminating the attempt and generating an error.")]
	[Category("Connection")]
	[DisplayName("Connection Timeout")]
	[DefaultValue(15L)]
	public uint ConnectionTimeout
	{
		get
		{
			return MySqlConnectionStringOption.ConnectionTimeout.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.ConnectionTimeout.SetValue(this, value);
		}
	}

	[Description("Whether invalid DATETIME fields should be converted to DateTime.MinValue.")]
	[DisplayName("Convert Zero DateTime")]
	[Category("Other")]
	[DefaultValue(false)]
	public bool ConvertZeroDateTime
	{
		get
		{
			return MySqlConnectionStringOption.ConvertZeroDateTime.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.ConvertZeroDateTime.SetValue(this, value);
		}
	}

	[DefaultValue(MySqlDateTimeKind.Unspecified)]
	[Category("Other")]
	[Description("The DateTimeKind to use when deserializing DATETIME values.")]
	[DisplayName("DateTime Kind")]
	public MySqlDateTimeKind DateTimeKind
	{
		get
		{
			return MySqlConnectionStringOption.DateTimeKind.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.DateTimeKind.SetValue(this, value);
		}
	}

	[Category("Other")]
	[DisplayName("Default Command Timeout")]
	[Description("The length of time (in seconds) each command can execute before the query is cancelled on the server, or zero to disable timeouts.")]
	[DefaultValue(30L)]
	public uint DefaultCommandTimeout
	{
		get
		{
			return MySqlConnectionStringOption.DefaultCommandTimeout.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.DefaultCommandTimeout.SetValue(this, value);
		}
	}

	[DefaultValue(false)]
	[Description("Forces all async methods to execute synchronously.")]
	[DisplayName("Force Synchronous")]
	[Category("Other")]
	public bool ForceSynchronous
	{
		get
		{
			return MySqlConnectionStringOption.ForceSynchronous.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.ForceSynchronous.SetValue(this, value);
		}
	}

	[Description("Determines which column type (if any) should be read as a Guid.")]
	[DisplayName("GUID Format")]
	[Category("Other")]
	[DefaultValue(MySqlGuidFormat.Default)]
	public MySqlGuidFormat GuidFormat
	{
		get
		{
			return MySqlConnectionStringOption.GuidFormat.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.GuidFormat.SetValue(this, value);
		}
	}

	[Category("Other")]
	[DefaultValue(false)]
	[DisplayName("Ignore Command Transaction")]
	[Description("Does not check the MySqlCommand.Transaction property for validity when executing a command.")]
	public bool IgnoreCommandTransaction
	{
		get
		{
			return MySqlConnectionStringOption.IgnoreCommandTransaction.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.IgnoreCommandTransaction.SetValue(this, value);
		}
	}

	[Category("Other")]
	[DisplayName("Ignore Prepare")]
	[Description("Ignores calls to MySqlCommand.Prepare and PrepareAsync.")]
	[DefaultValue(false)]
	public bool IgnorePrepare
	{
		get
		{
			return MySqlConnectionStringOption.IgnorePrepare.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.IgnorePrepare.SetValue(this, value);
		}
	}

	[Description("Instructs the MySQL server that this is an interactive session.")]
	[DisplayName("Interactive Session")]
	[Category("Connection")]
	[DefaultValue(false)]
	public bool InteractiveSession
	{
		get
		{
			return MySqlConnectionStringOption.InteractiveSession.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.InteractiveSession.SetValue(this, value);
		}
	}

	[Category("Connection")]
	[DefaultValue(0L)]
	[DisplayName("Keep Alive")]
	[Description("TCP Keepalive idle time (in seconds), or 0 to use OS defaults.")]
	public uint Keepalive
	{
		get
		{
			return MySqlConnectionStringOption.Keepalive.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.Keepalive.SetValue(this, value);
		}
	}

	[Description("Doesn't escape backslashes in string parameters. For use with the NO_BACKSLASH_ESCAPES MySQL server mode.")]
	[Category("Other")]
	[DefaultValue(false)]
	[DisplayName("No Backslash Escapes")]
	public bool NoBackslashEscapes
	{
		get
		{
			return MySqlConnectionStringOption.NoBackslashEscapes.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.NoBackslashEscapes.SetValue(this, value);
		}
	}

	[DefaultValue(false)]
	[DisplayName("Old Guids")]
	[Category("Obsolete")]
	public bool OldGuids
	{
		get
		{
			return MySqlConnectionStringOption.OldGuids.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.OldGuids.SetValue(this, value);
		}
	}

	[DefaultValue(false)]
	[DisplayName("Persist Security Info")]
	[Category("Other")]
	[Description("Preserves security-sensitive information in the connection string retrieved from any open MySqlConnection.")]
	public bool PersistSecurityInfo
	{
		get
		{
			return MySqlConnectionStringOption.PersistSecurityInfo.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.PersistSecurityInfo.SetValue(this, value);
		}
	}

	[Category("Other")]
	[DefaultValue(true)]
	[Description("Enables query pipelining.")]
	[DisplayName("Pipelining")]
	public bool Pipelining
	{
		get
		{
			return MySqlConnectionStringOption.Pipelining.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.Pipelining.SetValue(this, value);
		}
	}

	[DefaultValue(MySqlServerRedirectionMode.Disabled)]
	[Description("Whether to use server redirection.")]
	[DisplayName("Server Redirection Mode")]
	[Category("Connection")]
	public MySqlServerRedirectionMode ServerRedirectionMode
	{
		get
		{
			return MySqlConnectionStringOption.ServerRedirectionMode.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.ServerRedirectionMode.SetValue(this, value);
		}
	}

	[Description("The path to a file containing the server's RSA public key.")]
	[Category("Connection")]
	[DefaultValue("")]
	[DisplayName("Server RSA Public Key File")]
	public string ServerRsaPublicKeyFile
	{
		get
		{
			return MySqlConnectionStringOption.ServerRsaPublicKeyFile.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.ServerRsaPublicKeyFile.SetValue(this, value);
		}
	}

	[Category("Connection")]
	[DisplayName("Server SPN")]
	[DefaultValue("")]
	[Description("The server’s Service Principal Name (for auth_gssapi_client authentication).")]
	public string ServerSPN
	{
		get
		{
			return MySqlConnectionStringOption.ServerSPN.GetValue(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption.ServerSPN.SetValue(this, value);
		}
	}

	[DefaultValue(true)]
	[Category("Other")]
	[Description("Returns TINYINT(1) fields as Boolean values.")]
	[DisplayName("Treat Tiny As Boolean")]
	public bool TreatTinyAsBoolean
	{
		get
		{
			return MySqlConnectionStringOption.TreatTinyAsBoolean.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.TreatTinyAsBoolean.SetValue(this, value);
		}
	}

	[DisplayName("Use Affected Rows")]
	[DefaultValue(false)]
	[Category("Other")]
	[Description("Report changed rows instead of found rows.")]
	public bool UseAffectedRows
	{
		get
		{
			return MySqlConnectionStringOption.UseAffectedRows.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.UseAffectedRows.SetValue(this, value);
		}
	}

	[DefaultValue(false)]
	[Category("Other")]
	[DisplayName("Use Compression")]
	[Description("Compress packets sent to and from the server.")]
	public bool UseCompression
	{
		get
		{
			return MySqlConnectionStringOption.UseCompression.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.UseCompression.SetValue(this, value);
		}
	}

	[DefaultValue(true)]
	[Description("Use XA transactions to implement System.Transactions distributed transactions.")]
	[DisplayName("Use XA Transactions")]
	[Category("Other")]
	public bool UseXaTransactions
	{
		get
		{
			return MySqlConnectionStringOption.UseXaTransactions.GetValue(this);
		}
		set
		{
			MySqlConnectionStringOption.UseXaTransactions.SetValue(this, value);
		}
	}

	public override ICollection Keys => (from string x in base.Keys
		orderby MySqlConnectionStringOption.OptionNames.IndexOf(x)
		select x).ToList();

	public override object this[string key]
	{
		get
		{
			return MySqlConnectionStringOption.GetOptionForKey(key).GetObject(this);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			MySqlConnectionStringOption optionForKey = MySqlConnectionStringOption.GetOptionForKey(key);
			if (value == null)
			{
				base[optionForKey.Key] = null;
			}
			else
			{
				optionForKey.SetObject(this, value);
			}
		}
	}

	public MySqlConnectionStringBuilder()
	{
	}

	public MySqlConnectionStringBuilder(string connectionString)
	{
		base.ConnectionString = connectionString;
	}

	public override bool ContainsKey(string keyword)
	{
		MySqlConnectionStringOption mySqlConnectionStringOption = MySqlConnectionStringOption.TryGetOptionForKey(keyword);
		if (mySqlConnectionStringOption != null)
		{
			return base.ContainsKey(mySqlConnectionStringOption.Key);
		}
		return false;
	}

	public override bool Remove(string keyword)
	{
		MySqlConnectionStringOption mySqlConnectionStringOption = MySqlConnectionStringOption.TryGetOptionForKey(keyword);
		if (mySqlConnectionStringOption != null)
		{
			return base.Remove(mySqlConnectionStringOption.Key);
		}
		return false;
	}

	internal void DoSetValue(string key, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] object value)
	{
		base[key] = value;
	}

	internal string GetConnectionString(bool includePassword)
	{
		string connectionString = base.ConnectionString;
		if (includePassword)
		{
			return connectionString;
		}
		if (m_cachedConnectionString != connectionString)
		{
			MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
			foreach (string key in Keys)
			{
				foreach (string key2 in MySqlConnectionStringOption.Password.Keys)
				{
					if (string.Equals(key, key2, StringComparison.OrdinalIgnoreCase))
					{
						mySqlConnectionStringBuilder.Remove(key);
					}
				}
			}
			m_cachedConnectionStringWithoutPassword = mySqlConnectionStringBuilder.ConnectionString;
			m_cachedConnectionString = connectionString;
		}
		return m_cachedConnectionStringWithoutPassword;
	}

	protected override void GetProperties(Hashtable propertyDescriptors)
	{
		base.GetProperties(propertyDescriptors);
		foreach (PropertyDescriptor item in (from PropertyDescriptor x in propertyDescriptors.Values
			where !x.Attributes.OfType<CategoryAttribute>().Any() || x.Attributes.OfType<ObsoleteAttribute>().Any()
			select x).ToList())
		{
			propertyDescriptors.Remove(item.DisplayName);
		}
	}
}
