using System;
using System.Buffers.Text;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySqlConnector.Authentication;
using MySqlConnector.Logging;
using MySqlConnector.Protocol;
using MySqlConnector.Protocol.Payloads;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector.Core;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class ServerSession
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	internal sealed class SslClientAuthenticationOptions
	{
		public X509RevocationMode CertificateRevocationCheckMode { get; set; }

		public X509CertificateCollection ClientCertificates { get; set; }

		public SslProtocols EnabledSslProtocols { get; set; }

		public string TargetHost { get; set; }
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	private enum State
	{
		Created,
		Connecting,
		Connected,
		Querying,
		CancelingQuery,
		ClearingPendingCancellation,
		Closing,
		Closed,
		Failed
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
	private sealed class DelimiterSqlParser(IMySqlCommand command) : SqlParser(new StatementPreparer(command.CommandText, null, command.CreateStatementPreparerOptions()))
	{
		public bool HasDelimiter { get; private set; }

		private string Sql { get; } = command.CommandText;

		protected override void OnStatementBegin(int index)
		{
			if (index + 10 < Sql.Length && MemoryExtensions.Equals(MemoryExtensions.AsSpan(Sql, index, 10), MemoryExtensions.AsSpan("delimiter "), StringComparison.OrdinalIgnoreCase))
			{
				HasDelimiter = true;
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[GeneratedCode("Microsoft.Extensions.Logging.Generators", "7.0.8.27404")]
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
	private readonly struct __ExpectedSessionState6Struct(string sessionId, State expectedState1, State expectedState2, State expectedState3, State expectedState4, State expectedState5, State expectedState6, State sessionState) : IReadOnlyList<KeyValuePair<string, object>>, IReadOnlyCollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
	{
		private readonly string _sessionId = sessionId;

		private readonly State _expectedState1 = expectedState1;

		private readonly State _expectedState2 = expectedState2;

		private readonly State _expectedState3 = expectedState3;

		private readonly State _expectedState4 = expectedState4;

		private readonly State _expectedState5 = expectedState5;

		private readonly State _expectedState6 = expectedState6;

		private readonly State _sessionState = sessionState;

		[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2, 1 })]
		public static readonly Func<__ExpectedSessionState6Struct, Exception, string> Format = (__ExpectedSessionState6Struct state, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] Exception ex) => state.ToString();

		public int Count => 9;

		[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1, 2 })]
		public KeyValuePair<string, object> this[int index]
		{
			[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1, 2 })]
			get
			{
				return index switch
				{
					0 => new KeyValuePair<string, object>("SessionId", _sessionId), 
					1 => new KeyValuePair<string, object>("ExpectedState1", _expectedState1), 
					2 => new KeyValuePair<string, object>("ExpectedState2", _expectedState2), 
					3 => new KeyValuePair<string, object>("ExpectedState3", _expectedState3), 
					4 => new KeyValuePair<string, object>("ExpectedState4", _expectedState4), 
					5 => new KeyValuePair<string, object>("ExpectedState5", _expectedState5), 
					6 => new KeyValuePair<string, object>("ExpectedState6", _expectedState6), 
					7 => new KeyValuePair<string, object>("SessionState", _sessionState), 
					8 => new KeyValuePair<string, object>("{OriginalFormat}", "Session {SessionId} should have state {ExpectedState1} or {ExpectedState2} or {ExpectedState3} or {ExpectedState4} or {ExpectedState5} or {ExpectedState6} but was {SessionState}"), 
					_ => throw new IndexOutOfRangeException("index"), 
				};
			}
		}

		public override string ToString()
		{
			string sessionId = _sessionId;
			State expectedState = _expectedState1;
			State expectedState2 = _expectedState2;
			State expectedState3 = _expectedState3;
			State expectedState4 = _expectedState4;
			State expectedState5 = _expectedState5;
			State expectedState6 = _expectedState6;
			State sessionState = _sessionState;
			return string.Format("Session {0} should have state {1} or {2} or {3} or {4} or {5} or {6} but was {7}", new object[8] { sessionId, expectedState, expectedState2, expectedState3, expectedState4, expectedState5, expectedState6, sessionState });
		}

		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 0, 1, 2 })]
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			for (int i = 0; i < 9; i++)
			{
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private static readonly PayloadData s_setNamesUtf8NoAttributesPayload = QueryPayload.Create(supportsQueryAttributes: false, "SET NAMES utf8;"u8);

	private static readonly PayloadData s_setNamesUtf8mb4NoAttributesPayload = QueryPayload.Create(supportsQueryAttributes: false, "SET NAMES utf8mb4;"u8);

	private static readonly PayloadData s_setNamesUtf8WithAttributesPayload = QueryPayload.Create(supportsQueryAttributes: true, "SET NAMES utf8;"u8);

	private static readonly PayloadData s_setNamesUtf8mb4WithAttributesPayload = QueryPayload.Create(supportsQueryAttributes: true, "SET NAMES utf8mb4;"u8);

	private static readonly PayloadData s_sleepNoAttributesPayload = QueryPayload.Create(supportsQueryAttributes: false, "SELECT SLEEP(0) INTO @\ue001MySqlConnector\ue001Sleep;"u8);

	private static readonly PayloadData s_sleepWithAttributesPayload = QueryPayload.Create(supportsQueryAttributes: true, "SELECT SLEEP(0) INTO @\ue001MySqlConnector\ue001Sleep;"u8);

	private static readonly PayloadData s_selectConnectionIdVersionNoAttributesPayload = QueryPayload.Create(supportsQueryAttributes: false, "SELECT CONNECTION_ID(), VERSION();"u8);

	private static readonly PayloadData s_selectConnectionIdVersionWithAttributesPayload = QueryPayload.Create(supportsQueryAttributes: true, "SELECT CONNECTION_ID(), VERSION();"u8);

	private static int s_lastId;

	private readonly ILogger m_logger;

	private readonly object m_lock;

	private readonly ArraySegmentHolder<byte> m_payloadCache;

	private readonly ActivityTagsCollection m_activityTags;

	private State m_state;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private TcpClient m_tcpClient;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private Socket m_socket;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private Stream m_stream;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private SslStream m_sslStream;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private X509Certificate2 m_clientCertificate;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private IPayloadHandler m_payloadHandler;

	private bool m_useCompression;

	private bool m_isSecureConnection;

	private bool m_supportsConnectionAttributes;

	private bool m_supportsPipelining;

	private CharacterSet m_characterSet;

	private PayloadData m_setNamesPayload;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private byte[] m_pipelinedResetConnectionBytes;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 1 })]
	private Dictionary<string, PreparedStatements> m_preparedStatements;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 1, 1, 2 })]
	[GeneratedCode("Microsoft.Extensions.Logging.Generators", "7.0.8.27404")]
	private static readonly Action<ILogger, string, State, Exception> __CannotExecuteNewCommandInStateCallback = LoggerMessage.Define<string, State>(LogLevel.Error, new EventId(2200, "CannotExecuteNewCommandInState"), "Session {SessionId} can't execute new command when in state {SessionState}", new LogDefineOptions
	{
		SkipEnabledCheck = true
	});

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 1, 1, 2 })]
	[GeneratedCode("Microsoft.Extensions.Logging.Generators", "7.0.8.27404")]
	private static readonly Action<ILogger, string, State, Exception> __EnteringFinishQueryingCallback = LoggerMessage.Define<string, State>(LogLevel.Trace, new EventId(2201, "EnteringFinishQuerying"), "Session {SessionId} entering FinishQuerying; state is {SessionState}", new LogDefineOptions
	{
		SkipEnabledCheck = true
	});

	[GeneratedCode("Microsoft.Extensions.Logging.Generators", "7.0.8.27404")]
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 1, 1, 2 })]
	private static readonly Action<ILogger, string, State, State, Exception> __ExpectedSessionState1Callback = LoggerMessage.Define<string, State, State>(LogLevel.Error, new EventId(2011, "ExpectedSessionState1"), "Session {SessionId} should have state {ExpectedState1} but was {SessionState}", new LogDefineOptions
	{
		SkipEnabledCheck = true
	});

	public string Id { get; }

	public ServerVersion ServerVersion { get; set; }

	public bool SupportsPerQueryVariables
	{
		get
		{
			if (ServerVersion.IsMariaDb)
			{
				return ServerVersion.Version >= ServerVersions.MariaDbSupportsPerQueryVariables;
			}
			return false;
		}
	}

	public int ActiveCommandId { get; private set; }

	public int CancellationTimeout { get; private set; }

	public int ConnectionId { get; set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public byte[] AuthPluginData
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		set;
	}

	public long CreatedTimestamp { get; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public ConnectionPool Pool
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
	}

	public int PoolGeneration { get; }

	public long LastLeasedTimestamp { get; set; }

	public long LastReturnedTimestamp { get; private set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public string DatabaseOverride
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		set;
	}

	public string HostName { get; private set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public IPEndPoint IPEndPoint
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get
		{
			return m_tcpClient?.Client.RemoteEndPoint as IPEndPoint;
		}
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public string UserID
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		private set;
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
	public WeakReference<MySqlConnection> OwningConnection
	{
		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
		get;
		[param: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
		set;
	}

	public bool SupportsDeprecateEof { get; private set; }

	public bool SupportsCachedPreparedMetadata { get; private set; }

	public bool SupportsQueryAttributes { get; private set; }

	public bool SupportsSessionTrack { get; private set; }

	public bool ProcAccessDenied { get; set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 0, 1, 2 })]
	public ICollection<KeyValuePair<string, object>> ActivityTags
	{
		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 0, 1, 2 })]
		get
		{
			return m_activityTags;
		}
	}

	public MySqlDataReader DataReader { get; set; }

	public bool IsConnected
	{
		get
		{
			lock (m_lock)
			{
				return m_state == State.Connected;
			}
		}
	}

	public bool IsCancelingQuery => m_state == State.CancelingQuery;

	internal bool SslIsEncrypted => m_sslStream?.IsEncrypted ?? false;

	internal bool SslIsSigned => m_sslStream?.IsSigned ?? false;

	internal bool SslIsAuthenticated => m_sslStream?.IsAuthenticated ?? false;

	internal bool SslIsMutuallyAuthenticated => m_sslStream?.IsMutuallyAuthenticated ?? false;

	internal SslProtocols SslProtocol => m_sslStream?.SslProtocol ?? SslProtocols.None;

	public ServerSession(ILogger logger)
		: this(logger, null, 0, Interlocked.Increment(ref s_lastId))
	{
	}

	public ServerSession(ILogger logger, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] ConnectionPool pool, int poolGeneration, int id)
	{
		m_logger = logger;
		m_lock = new object();
		m_payloadCache = new ArraySegmentHolder<byte>();
		Id = (pool?.Id ?? 0) + "." + id;
		ServerVersion = ServerVersion.Empty;
		CreatedTimestamp = Stopwatch.GetTimestamp();
		Pool = pool;
		PoolGeneration = poolGeneration;
		HostName = "";
		m_activityTags = new ActivityTagsCollection();
		DataReader = new MySqlDataReader();
		Log.CreatedNewSession(m_logger, Id);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	public ValueTask ReturnToPoolAsync(IOBehavior ioBehavior, MySqlConnection owningConnection)
	{
		Log.ReturningToPool(m_logger, Id, Pool?.Id ?? 0);
		LastReturnedTimestamp = Stopwatch.GetTimestamp();
		if (Pool == null)
		{
			return default(ValueTask);
		}
		MetricsReporter.RecordUseTime(Pool, Utility.GetElapsedSeconds(LastLeasedTimestamp, LastReturnedTimestamp));
		LastLeasedTimestamp = 0L;
		return Pool.ReturnAsync(ioBehavior, this);
	}

	public bool TryStartCancel(ICancellableCommand command)
	{
		lock (m_lock)
		{
			if (ActiveCommandId != command.CommandId)
			{
				return false;
			}
			VerifyState(State.Querying, State.CancelingQuery, State.ClearingPendingCancellation, State.Closing, State.Closed, State.Failed);
			if (m_state != State.Querying)
			{
				return false;
			}
			if (command.CancelAttemptCount++ >= 10)
			{
				return false;
			}
			m_state = State.CancelingQuery;
		}
		Log.WillCancelCommand(m_logger, Id, command.CommandId, command.CancelAttemptCount, (command as MySqlCommand)?.CommandText);
		return true;
	}

	public void DoCancel(ICancellableCommand commandToCancel, MySqlCommand killCommand)
	{
		Log.CancelingCommandFromSession(m_logger, Id, commandToCancel.CommandId, killCommand.Connection.Session.Id, (commandToCancel as MySqlCommand)?.CommandText);
		lock (m_lock)
		{
			if (ActiveCommandId != commandToCancel.CommandId)
			{
				Log.IgnoringCancellationForInactiveCommand(m_logger, Id, ActiveCommandId, commandToCancel.CommandId);
				return;
			}
			Log.CancelingCommand(m_logger, killCommand.Connection.Session.Id, commandToCancel.CommandId, killCommand.CommandText);
			killCommand.ExecuteNonQuery();
		}
	}

	public void AbortCancel(ICancellableCommand command)
	{
		lock (m_lock)
		{
			if (ActiveCommandId == command.CommandId && m_state == State.CancelingQuery)
			{
				m_state = State.Querying;
			}
		}
	}

	public async Task PrepareAsync(IMySqlCommand command, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		string commandText = command.CommandText;
		string commandText2;
		if (command.CommandType == CommandType.StoredProcedure)
		{
			CachedProcedure cachedProcedure = await command.Connection.GetCachedProcedure(commandText, revalidateMissing: false, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (cachedProcedure == null)
			{
				NormalizedSchema normalizedSchema = NormalizedSchema.MustNormalize(command.CommandText, command.Connection.Database);
				throw new MySqlException("Procedure or function '" + normalizedSchema.Component + "' cannot be found in database '" + normalizedSchema.Schema + "'.");
			}
			int count = cachedProcedure.Parameters.Count;
			StringBuilder stringBuilder = new StringBuilder("CALL ", commandText.Length + 8 + count * 2);
			stringBuilder.Append(commandText);
			stringBuilder.Append('(');
			for (int i = 0; i < count; i++)
			{
				stringBuilder.Append("?,");
			}
			if (count == 0)
			{
				stringBuilder.Append(')');
			}
			else
			{
				stringBuilder[stringBuilder.Length - 1] = ')';
			}
			stringBuilder.Append(';');
			commandText2 = stringBuilder.ToString();
		}
		else
		{
			commandText2 = commandText;
		}
		StatementPreparer statementPreparer = new StatementPreparer(commandText2, command.RawParameters, command.CreateStatementPreparerOptions());
		ParsedStatements parsedStatements = statementPreparer.SplitStatements();
		ResizableArray<byte> columnsAndParameters = new ResizableArray<byte>();
		int columnsAndParametersSize = 0;
		List<PreparedStatement> preparedStatements = new List<PreparedStatement>(parsedStatements.Statements.Count);
		foreach (ParsedStatement statement in parsedStatements.Statements)
		{
			await SendAsync(new PayloadData(statement.StatementBytes), ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			PayloadData payloadData;
			try
			{
				payloadData = await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (MySqlException exception)
			{
				ThrowIfStatementContainsDelimiter(exception, command);
				throw;
			}
			StatementPrepareResponsePayload response = StatementPrepareResponsePayload.Create(payloadData.Span);
			ColumnDefinitionPayload[] parameters = null;
			if (response.ParameterCount > 0)
			{
				parameters = new ColumnDefinitionPayload[response.ParameterCount];
				for (int j = 0; j < response.ParameterCount; j++)
				{
					payloadData = await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					int length = payloadData.Span.Length;
					Utility.Resize(ref columnsAndParameters, columnsAndParametersSize + length);
					payloadData.Span.CopyTo(columnsAndParameters.AsSpan(columnsAndParametersSize));
					ColumnDefinitionPayload.Initialize(ref parameters[j], new ResizableArraySegment<byte>(columnsAndParameters, columnsAndParametersSize, length));
					columnsAndParametersSize += length;
				}
				if (!SupportsDeprecateEof)
				{
					EofPayload.Create((await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span);
				}
			}
			ColumnDefinitionPayload[] columns = null;
			if (response.ColumnCount > 0)
			{
				columns = new ColumnDefinitionPayload[response.ColumnCount];
				for (int j = 0; j < response.ColumnCount; j++)
				{
					payloadData = await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					int length2 = payloadData.Span.Length;
					Utility.Resize(ref columnsAndParameters, columnsAndParametersSize + length2);
					payloadData.Span.CopyTo(columnsAndParameters.AsSpan(columnsAndParametersSize));
					ColumnDefinitionPayload.Initialize(ref columns[j], new ResizableArraySegment<byte>(columnsAndParameters, columnsAndParametersSize, length2));
					columnsAndParametersSize += length2;
				}
				if (!SupportsDeprecateEof)
				{
					EofPayload.Create((await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span);
				}
			}
			preparedStatements.Add(new PreparedStatement(response.StatementId, statement, columns, parameters));
		}
		if (m_preparedStatements == null)
		{
			m_preparedStatements = new Dictionary<string, PreparedStatements>();
		}
		m_preparedStatements.Add(commandText, new PreparedStatements(preparedStatements, parsedStatements));
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public PreparedStatements TryGetPreparedStatement(string commandText)
	{
		if (m_preparedStatements == null || !m_preparedStatements.TryGetValue(commandText, out var value))
		{
			return null;
		}
		return value;
	}

	public void StartQuerying(ICancellableCommand command)
	{
		lock (m_lock)
		{
			State state = m_state;
			if ((uint)(state - 3) <= 1u)
			{
				CannotExecuteNewCommandInState(m_logger, Id, m_state);
				throw new InvalidOperationException("This MySqlConnection is already in use. See https://fl.vu/mysql-conn-reuse");
			}
			VerifyState(State.Connected);
			m_state = State.Querying;
			command.CancelAttemptCount = 0;
			ActiveCommandId = command.CommandId;
		}
	}

	public void FinishQuerying()
	{
		EnteringFinishQuerying(m_logger, Id, m_state);
		bool flag = false;
		lock (m_lock)
		{
			if (m_state == State.CancelingQuery)
			{
				m_state = State.ClearingPendingCancellation;
				flag = true;
			}
		}
		if (flag)
		{
			Log.SendingSleepToClearPendingCancellation(m_logger, Id);
			PayloadData payload = (SupportsQueryAttributes ? s_sleepWithAttributesPayload : s_sleepNoAttributesPayload);
			SendAsync(payload, IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
			OkPayload.Verify(ReceiveReplyAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult().Span, SupportsDeprecateEof, SupportsSessionTrack);
		}
		lock (m_lock)
		{
			State state = m_state;
			if ((state == State.Querying || state == State.ClearingPendingCancellation) ? true : false)
			{
				m_state = State.Connected;
			}
			else
			{
				VerifyState(State.Failed);
			}
			ActiveCommandId = 0;
		}
	}

	public void SetTimeout(int timeoutMilliseconds)
	{
		m_payloadHandler.ByteHandler.RemainingTimeout = timeoutMilliseconds;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	public Activity StartActivity([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] string name, string tagName1 = null, object tagValue1 = null)
	{
		Activity activity = ActivitySourceHelper.StartActivity(name, m_activityTags);
		if (activity != null && activity.IsAllDataRequested)
		{
			if (DatabaseOverride != null)
			{
				activity.SetTag("db.name", DatabaseOverride);
			}
			if (tagName1 != null)
			{
				activity.SetTag(tagName1, tagValue1);
			}
		}
		return activity;
	}

	public async Task DisposeAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		if (m_payloadHandler != null)
		{
			State state2;
			lock (m_lock)
			{
				State state = m_state;
				if ((state == State.Connected || state == State.Failed) ? true : false)
				{
					m_state = State.Closing;
				}
				state2 = m_state;
			}
			if (state2 == State.Closing)
			{
				try
				{
					Log.SendingQuitCommand(m_logger, Id);
					m_payloadHandler.StartNewConversation();
					await m_payloadHandler.WritePayloadAsync(QuitPayload.Instance.Memory, ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (IOException)
				{
				}
				catch (NotSupportedException)
				{
				}
				catch (ObjectDisposedException)
				{
				}
				catch (SocketException)
				{
				}
			}
		}
		ClearPreparedStatements();
		ShutdownSocket();
		lock (m_lock)
		{
			m_state = State.Closed;
		}
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })]
	public async Task<string> ConnectAsync(ConnectionSettings cs, MySqlConnection connection, long startingTimestamp, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] ILoadBalancer loadBalancer, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		_ = 10;
		try
		{
			lock (m_lock)
			{
				VerifyState(State.Created);
				m_state = State.Connecting;
			}
			string connectionString = cs.ConnectionStringBuilder.GetConnectionString(cs.ConnectionStringBuilder.PersistSecurityInfo);
			m_activityTags.Add("db.system", "mysql");
			m_activityTags.Add("db.connection_string", connectionString);
			m_activityTags.Add("db.user", cs.UserID);
			if (cs.Database.Length != 0)
			{
				m_activityTags.Add("db.name", cs.Database);
			}
			if (activity != null && activity.IsAllDataRequested)
			{
				activity.SetTag("db.system", "mysql").SetTag("db.connection_string", connectionString).SetTag("db.user", cs.UserID);
				if (cs.Database.Length != 0)
				{
					activity.SetTag("db.name", cs.Database);
				}
			}
			if (cs.ConnectionProtocol switch
			{
				MySqlConnectionProtocol.Sockets => (await OpenTcpSocketAsync(cs, loadBalancer ?? throw new ArgumentNullException("loadBalancer"), activity, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) ? 1 : 0, 
				MySqlConnectionProtocol.UnixSocket => (await OpenUnixSocketAsync(cs, activity, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) ? 1 : 0, 
				MySqlConnectionProtocol.Pipe => (await OpenNamedPipeAsync(cs, startingTimestamp, activity, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) ? 1 : 0, 
				_ => 0, 
			} == 0)
			{
				lock (m_lock)
				{
					m_state = State.Failed;
				}
				Log.ConnectingFailed(m_logger, Id);
				throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Unable to connect to any of the specified MySQL hosts.");
			}
			IByteHandler byteHandler2;
			if (m_socket != null)
			{
				IByteHandler byteHandler = new SocketByteHandler(m_socket);
				byteHandler2 = byteHandler;
			}
			else
			{
				IByteHandler byteHandler = new StreamByteHandler(m_stream);
				byteHandler2 = byteHandler;
			}
			IByteHandler byteHandler3 = byteHandler2;
			if (cs.ConnectionTimeout != 0)
			{
				byteHandler3.RemainingTimeout = Math.Max(1, cs.ConnectionTimeoutMilliseconds - Utility.GetElapsedMilliseconds(startingTimestamp));
			}
			m_payloadHandler = new StandardPayloadHandler(byteHandler3);
			InitialHandshakePayload initialHandshake = InitialHandshakePayload.Create((await ReceiveAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span);
			string text = (((initialHandshake.ProtocolCapabilities & ProtocolCapabilities.PluginAuth) != ProtocolCapabilities.None) ? initialHandshake.AuthPluginName : (((initialHandshake.ProtocolCapabilities & ProtocolCapabilities.SecureConnection) == ProtocolCapabilities.None) ? "mysql_old_password" : "mysql_native_password"));
			Log.ServerSentAuthPluginName(m_logger, Id, text);
			switch (text)
			{
			default:
				Log.UnsupportedAuthenticationMethod(m_logger, Id, text);
				throw new NotSupportedException("Authentication method '" + initialHandshake.AuthPluginName + "' is not supported.");
			case "mysql_native_password":
			case "sha256_password":
			case "caching_sha2_password":
			{
				ServerVersion = new ServerVersion(initialHandshake.ServerVersion);
				ConnectionId = initialHandshake.ConnectionId;
				AuthPluginData = initialHandshake.AuthPluginData;
				m_useCompression = cs.UseCompression && (initialHandshake.ProtocolCapabilities & ProtocolCapabilities.Compress) != 0;
				CancellationTimeout = cs.CancellationTimeout;
				UserID = cs.UserID;
				string value = ConnectionId.ToString(CultureInfo.InvariantCulture);
				m_activityTags["db.connection_id"] = value;
				if (activity != null && activity.IsAllDataRequested)
				{
					activity.SetTag("db.connection_id", value);
				}
				m_supportsConnectionAttributes = (initialHandshake.ProtocolCapabilities & ProtocolCapabilities.ConnectionAttributes) != 0;
				SupportsDeprecateEof = (initialHandshake.ProtocolCapabilities & ProtocolCapabilities.DeprecateEof) != 0;
				SupportsCachedPreparedMetadata = (initialHandshake.ProtocolCapabilities & ProtocolCapabilities.MariaDbCacheMetadata) != 0;
				SupportsQueryAttributes = (initialHandshake.ProtocolCapabilities & ProtocolCapabilities.QueryAttributes) != 0;
				SupportsSessionTrack = (initialHandshake.ProtocolCapabilities & ProtocolCapabilities.SessionTrack) != 0;
				bool flag = (initialHandshake.ProtocolCapabilities & ProtocolCapabilities.Ssl) != 0;
				m_characterSet = ((ServerVersion.Version >= ServerVersions.SupportsUtf8Mb4) ? CharacterSet.Utf8Mb4GeneralCaseInsensitive : CharacterSet.Utf8Mb3GeneralCaseInsensitive);
				m_setNamesPayload = ((!(ServerVersion.Version >= ServerVersions.SupportsUtf8Mb4)) ? (SupportsQueryAttributes ? s_setNamesUtf8WithAttributesPayload : s_setNamesUtf8NoAttributesPayload) : (SupportsQueryAttributes ? s_setNamesUtf8mb4WithAttributesPayload : s_setNamesUtf8mb4NoAttributesPayload));
				if (!cs.Pipelining.HasValue && ServerVersion.Version.Major == 5 && ServerVersion.Version.Minor == 7 && HostName.EndsWith(".rds.amazonaws.com", StringComparison.OrdinalIgnoreCase))
				{
					Log.AutoDetectedAurora57(m_logger, Id, HostName);
					m_supportsPipelining = false;
				}
				else
				{
					m_supportsPipelining = !cs.UseCompression && !((!cs.Pipelining) ?? false);
					if (m_supportsPipelining)
					{
						m_pipelinedResetConnectionBytes = new byte[m_setNamesPayload.Span.Length + 9];
						m_pipelinedResetConnectionBytes[0] = 1;
						m_pipelinedResetConnectionBytes[4] = 31;
						m_pipelinedResetConnectionBytes[5] = (byte)m_setNamesPayload.Span.Length;
						ReadOnlySpan<byte> span = m_setNamesPayload.Span;
						Span<byte> span2 = MemoryExtensions.AsSpan(m_pipelinedResetConnectionBytes);
						span.CopyTo(span2.Slice(9, span2.Length - 9));
					}
				}
				Log.SessionMadeConnection(m_logger, Id, ServerVersion.OriginalString, ConnectionId, m_useCompression, m_supportsConnectionAttributes, SupportsDeprecateEof, SupportsCachedPreparedMetadata, flag, SupportsSessionTrack, m_supportsPipelining, SupportsQueryAttributes);
				if (cs.SslMode != MySqlSslMode.None && (cs.SslMode != MySqlSslMode.Preferred || flag))
				{
					if (!flag)
					{
						Log.ServerDoesNotSupportSsl(m_logger, Id);
						throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Server does not support SSL");
					}
					await InitSslAsync(initialHandshake.ProtocolCapabilities, cs, connection, cs.TlsVersions, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				if (m_supportsConnectionAttributes && cs.ConnectionAttributes == null)
				{
					cs.ConnectionAttributes = CreateConnectionAttributes(cs.ApplicationName);
				}
				string password = GetPassword(cs, connection);
				using (PayloadData handshakeResponsePayload = HandshakeResponse41Payload.Create(initialHandshake, cs, password, m_useCompression, m_characterSet, m_supportsConnectionAttributes ? cs.ConnectionAttributes : null))
				{
					await SendReplyAsync(handshakeResponsePayload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				PayloadData payload = await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				while (payload.HeaderByte == 254)
				{
					payload = await SwitchAuthenticationAsync(cs, password, payload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				OkPayload okPayload = OkPayload.Create(payload.Span, SupportsDeprecateEof, SupportsSessionTrack);
				string statusInfo = okPayload.StatusInfo;
				if (m_useCompression)
				{
					m_payloadHandler = new CompressedPayloadHandler(m_payloadHandler.ByteHandler);
				}
				await SendAsync(m_setNamesPayload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				OkPayload.Verify((await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span, SupportsDeprecateEof, SupportsSessionTrack);
				if (ShouldGetRealServerDetails(cs))
				{
					await GetRealServerDetailsAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
				}
				m_payloadHandler.ByteHandler.RemainingTimeout = int.MaxValue;
				return statusInfo;
			}
			}
		}
		catch (ArgumentException ex)
		{
			Log.CouldNotConnectToServer(m_logger, ex, Id);
			throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Couldn't connect to server", ex);
		}
		catch (IOException ex2)
		{
			Log.CouldNotConnectToServer(m_logger, ex2, Id);
			throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Couldn't connect to server", ex2);
		}
	}

	public async Task<bool> TryResetConnectionAsync(ConnectionSettings cs, MySqlConnection connection, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		VerifyState(State.Connected);
		try
		{
			ClearPreparedStatements();
			if (DatabaseOverride == null && ((!ServerVersion.IsMariaDb && ServerVersion.Version.CompareTo(ServerVersions.SupportsResetConnection) >= 0) || (ServerVersion.IsMariaDb && ServerVersion.Version.CompareTo(ServerVersions.MariaDbSupportsResetConnection) >= 0)))
			{
				if (m_supportsPipelining)
				{
					Log.SendingPipelinedResetConnectionRequest(m_logger, Id, ServerVersion.OriginalString);
					await SendRawAsync(m_pipelinedResetConnectionBytes, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					OkPayload.Verify((await ReceiveReplyAsync(1, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span, SupportsDeprecateEof, SupportsSessionTrack);
					OkPayload.Verify((await ReceiveReplyAsync(1, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span, SupportsDeprecateEof, SupportsSessionTrack);
					return true;
				}
				Log.SendingResetConnectionRequest(m_logger, Id, ServerVersion.OriginalString);
				await SendAsync(ResetConnectionPayload.Instance, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				OkPayload.Verify((await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span, SupportsDeprecateEof, SupportsSessionTrack);
			}
			else
			{
				if (DatabaseOverride == null)
				{
					Log.SendingChangeUserRequest(m_logger, Id, ServerVersion.OriginalString);
				}
				else
				{
					Log.SendingChangeUserRequestDueToChangedDatabase(m_logger, Id, DatabaseOverride);
					DatabaseOverride = null;
				}
				string password = GetPassword(cs, connection);
				byte[] array = AuthenticationUtility.CreateAuthenticationResponse(AuthPluginData, password);
				using (PayloadData changeUserPayload = ChangeUserPayload.Create(cs.UserID, array, cs.Database, m_characterSet, m_supportsConnectionAttributes ? cs.ConnectionAttributes : null))
				{
					await SendAsync(changeUserPayload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				PayloadData payload = await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (payload.HeaderByte == 254)
				{
					Log.OptimisticReauthenticationFailed(m_logger, Id);
					payload = await SwitchAuthenticationAsync(cs, password, payload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				OkPayload.Verify(payload.Span, SupportsDeprecateEof, SupportsSessionTrack);
			}
			await SendAsync(m_setNamesPayload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			OkPayload.Verify((await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span, SupportsDeprecateEof, SupportsSessionTrack);
			return true;
		}
		catch (IOException exception)
		{
			Log.IgnoringFailureInTryResetConnectionAsync(m_logger, exception, Id, "IOException");
		}
		catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.ClientInteractionTimeout)
		{
			Log.IgnoringFailureInTryResetConnectionAsync(m_logger, ex, Id, "ClientInteractionTimeout MySqlException");
		}
		catch (ObjectDisposedException exception2)
		{
			Log.IgnoringFailureInTryResetConnectionAsync(m_logger, exception2, Id, "ObjectDisposedException");
		}
		catch (SocketException exception3)
		{
			Log.IgnoringFailureInTryResetConnectionAsync(m_logger, exception3, Id, "SocketException");
		}
		return false;
	}

	private async Task<PayloadData> SwitchAuthenticationAsync(ConnectionSettings cs, string password, PayloadData payload, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		AuthenticationMethodSwitchRequestPayload switchRequest = AuthenticationMethodSwitchRequestPayload.Create(payload.Span);
		Log.SwitchingToAuthenticationMethod(m_logger, Id, switchRequest.Name);
		switch (switchRequest.Name)
		{
		case "mysql_native_password":
		{
			AuthPluginData = switchRequest.Data;
			byte[] data = AuthenticationUtility.CreateAuthenticationResponse(AuthPluginData, password);
			payload = new PayloadData(data);
			await SendReplyAsync(payload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		case "mysql_clear_password":
		{
			if (!m_isSecureConnection)
			{
				Log.NeedsSecureConnection(m_logger, Id, switchRequest.Name);
				throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Authentication method '" + switchRequest.Name + "' requires a secure connection.");
			}
			byte[] nullTerminatedPasswordBytes = AuthenticationUtility.GetNullTerminatedPasswordBytes(password);
			payload = new PayloadData(nullTerminatedPasswordBytes);
			await SendReplyAsync(payload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		case "caching_sha2_password":
		{
			byte[] data2 = AuthenticationUtility.CreateScrambleResponse(Utility.TrimZeroByte(MemoryExtensions.AsSpan(switchRequest.Data)), password);
			payload = new PayloadData(data2);
			await SendReplyAsync(payload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			payload = await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (OkPayload.IsOk(payload.Span, SupportsDeprecateEof))
			{
				return payload;
			}
			if (CachingSha2ServerResponsePayload.Create(payload.Span).Succeeded)
			{
				return await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			goto case "sha256_password";
		}
		case "sha256_password":
			if (!m_isSecureConnection && password.Length != 0)
			{
				string rsaPublicKey = await GetRsaPublicKeyAsync(switchRequest.Name, cs, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return await SendEncryptedPasswordAsync(switchRequest.Data, rsaPublicKey, password, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			return await SendClearPasswordAsync(password, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		case "auth_gssapi_client":
			return await AuthGSSAPI.AuthenticateAsync(cs, switchRequest.Data, this, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		case "mysql_old_password":
			Log.AuthenticationMethodNotSupported(m_logger, Id, switchRequest.Name);
			throw new NotSupportedException("'MySQL Server is requesting the insecure pre-4.1 auth mechanism (mysql_old_password). The user password must be upgraded; see https://dev.mysql.com/doc/refman/5.7/en/account-upgrades.html.");
		case "client_ed25519":
		{
			if (!AuthenticationPlugins.TryGetPlugin(switchRequest.Name, out var plugin))
			{
				throw new NotSupportedException("You must install the MySqlConnector.Authentication.Ed25519 package and call Ed25519AuthenticationPlugin.Install to use client_ed25519 authentication.");
			}
			payload = new PayloadData(plugin.CreateResponse(password, switchRequest.Data));
			await SendReplyAsync(payload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		default:
			Log.AuthenticationMethodNotSupported(m_logger, Id, switchRequest.Name);
			throw new NotSupportedException("Authentication method '" + switchRequest.Name + "' is not supported.");
		}
	}

	private async Task<PayloadData> SendClearPasswordAsync(string password, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		byte[] nullTerminatedPasswordBytes = AuthenticationUtility.GetNullTerminatedPasswordBytes(password);
		PayloadData payload = new PayloadData(nullTerminatedPasswordBytes);
		await SendReplyAsync(payload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<PayloadData> SendEncryptedPasswordAsync(byte[] switchRequestData, string rsaPublicKey, string password, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		using RSA rsa = RSA.Create();
		RSAParameters rsaParameters;
		try
		{
			rsaParameters = Utility.GetRsaParameters(rsaPublicKey);
		}
		catch (Exception ex)
		{
			Log.CouldNotLoadServerRsaPublicKey(m_logger, ex, Id);
			throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Couldn't load server's RSA public key; try using a secure connection instead.", ex);
		}
		rsa.ImportParameters(rsaParameters);
		byte[] nullTerminatedPasswordBytes = AuthenticationUtility.GetNullTerminatedPasswordBytes(password);
		AuthPluginData = Utility.TrimZeroByte(switchRequestData);
		for (int i = 0; i < nullTerminatedPasswordBytes.Length; i++)
		{
			nullTerminatedPasswordBytes[i] ^= AuthPluginData[i % AuthPluginData.Length];
		}
		RSAEncryptionPadding oaepSHA = RSAEncryptionPadding.OaepSHA1;
		byte[] data = rsa.Encrypt(nullTerminatedPasswordBytes, oaepSHA);
		PayloadData payload = new PayloadData(data);
		await SendReplyAsync(payload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<string> GetRsaPublicKeyAsync(string switchRequestName, ConnectionSettings cs, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		if (cs.ServerRsaPublicKeyFile.Length != 0)
		{
			try
			{
				return File.ReadAllText(cs.ServerRsaPublicKeyFile);
			}
			catch (IOException ex)
			{
				Log.CouldNotLoadServerRsaPublicKeyFromFile(m_logger, ex, Id, cs.ServerRsaPublicKeyFile);
				throw new MySqlException("Couldn't load server's RSA public key from '" + cs.ServerRsaPublicKeyFile + "'", ex);
			}
		}
		if (cs.AllowPublicKeyRetrieval)
		{
			byte b = (byte)((!(switchRequestName == "caching_sha2_password")) ? 1 : 2);
			await SendReplyAsync(new PayloadData(new byte[1] { b }), ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			AuthenticationMoreDataPayload authenticationMoreDataPayload = AuthenticationMoreDataPayload.Create((await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span);
			return Encoding.ASCII.GetString(authenticationMoreDataPayload.Data);
		}
		Log.CouldNotUseAuthenticationMethodForRsa(m_logger, Id, switchRequestName);
		throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Authentication method '" + switchRequestName + "' failed. Either use a secure connection, specify the server's RSA public key with ServerRSAPublicKeyFile, or set AllowPublicKeyRetrieval=True.");
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public async ValueTask<bool> TryPingAsync(bool logInfo, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		VerifyState(State.Connected);
		try
		{
			Log.PingingServer(m_logger, Id);
			await SendAsync(PingPayload.Instance, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			OkPayload.Verify((await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span, SupportsDeprecateEof, SupportsSessionTrack);
			Log.SuccessfullyPingedServer(m_logger, logInfo ? LogLevel.Information : LogLevel.Trace, Id);
			return true;
		}
		catch (IOException exception)
		{
			Log.PingFailed(m_logger, exception, Id, "IOException");
		}
		catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.ClientInteractionTimeout)
		{
			Log.PingFailed(m_logger, ex, Id, "ClientInteractionTimeout MySqlException");
		}
		catch (SocketException exception2)
		{
			Log.PingFailed(m_logger, exception2, Id, "SocketException");
		}
		VerifyState(State.Failed);
		return false;
	}

	public ValueTask SendAsync(PayloadData payload, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		m_payloadHandler.StartNewConversation();
		return SendReplyAsync(payload, ioBehavior, cancellationToken);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public ValueTask<PayloadData> ReceiveAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		m_payloadHandler.StartNewConversation();
		return ReceiveReplyAsync(ioBehavior, cancellationToken);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public async ValueTask<PayloadData> ReceiveReplyAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		InvalidOperationException ex = CreateExceptionForInvalidState();
		if (ex != null)
		{
			Log.FailedInReceiveReplyAsync(m_logger, ex, Id);
			throw ex;
		}
		ArraySegment<byte> arraySegment;
		try
		{
			arraySegment = await m_payloadHandler.ReadPayloadAsync(m_payloadCache, ProtocolErrorBehavior.Throw, ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex2)
		{
			if (ex2 is MySqlEndOfStreamException ex3)
			{
				Log.ExpectedToReadMoreBytes(m_logger, Id, ex3.ExpectedByteCount, ex3.ReadByteCount);
			}
			SetFailed(ex2);
			throw;
		}
		PayloadData result = new PayloadData(arraySegment);
		if (result.HeaderByte != byte.MaxValue)
		{
			return result;
		}
		throw CreateExceptionForErrorPayload(result.Span);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public ValueTask<PayloadData> ReceiveReplyAsync(int expectedSequenceNumber, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		m_payloadHandler.SetNextSequenceNumber(expectedSequenceNumber);
		return ReceiveReplyAsync(ioBehavior, cancellationToken);
	}

	public async ValueTask SendReplyAsync(PayloadData payload, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		InvalidOperationException ex = CreateExceptionForInvalidState();
		if (ex != null)
		{
			Log.FailedInSendReplyAsync(m_logger, ex, Id);
			throw ex;
		}
		try
		{
			await m_payloadHandler.WritePayloadAsync(payload.Memory, ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception failed)
		{
			SetFailed(failed);
			throw;
		}
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public async ValueTask SendRawAsync(ReadOnlyMemory<byte> data, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		InvalidOperationException ex = CreateExceptionForInvalidState();
		if (ex != null)
		{
			Log.FailedInSendReplyAsync(m_logger, ex, Id);
			throw ex;
		}
		try
		{
			await m_payloadHandler.ByteHandler.WriteBytesAsync(data, ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception failed)
		{
			SetFailed(failed);
			throw;
		}
	}

	public static void ThrowIfStatementContainsDelimiter(MySqlException exception, IMySqlCommand command)
	{
		if (exception.ErrorCode != MySqlErrorCode.ParseError)
		{
			return;
		}
		string commandText = command.CommandText;
		if (commandText != null && commandText.IndexOf("delimiter", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			DelimiterSqlParser delimiterSqlParser = new DelimiterSqlParser(command);
			delimiterSqlParser.Parse(command.CommandText);
			if (delimiterSqlParser.HasDelimiter)
			{
				throw new MySqlException(MySqlErrorCode.DelimiterNotSupported, "'DELIMITER' should not be used with MySqlConnector. See https://fl.vu/mysql-delimiter", exception);
			}
		}
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	private InvalidOperationException CreateExceptionForInvalidState()
	{
		lock (m_lock)
		{
			switch (m_state)
			{
			case State.Closed:
				return new ObjectDisposedException("ServerSession");
			case State.Connected:
			case State.Querying:
			case State.CancelingQuery:
			case State.ClearingPendingCancellation:
			case State.Closing:
				return null;
			default:
				return new InvalidOperationException("ServerSession is not connected.");
			}
		}
	}

	private async Task<bool> OpenTcpSocketAsync(ConnectionSettings cs, ILoadBalancer loadBalancer, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		m_activityTags.Add("net.transport", "ip_tcp");
		string text = ((cs.Port == 3306) ? null : cs.Port.ToString(CultureInfo.InvariantCulture));
		if (text != null)
		{
			m_activityTags.Add("net.peer.port", text);
		}
		if (activity != null && activity.IsAllDataRequested)
		{
			activity.SetTag("net.transport", "ip_tcp");
			if (text != null)
			{
				activity.SetTag("net.peer.port", text);
			}
		}
		IReadOnlyList<string> hostNames = loadBalancer.LoadBalance(cs.HostNames);
		bool flag = default(bool);
		for (int hostNameIndex = 0; hostNameIndex < hostNames.Count; hostNameIndex++)
		{
			string hostName = hostNames[hostNameIndex];
			IPAddress[] ipAddresses;
			try
			{
				IPAddress[] array = ((ioBehavior != IOBehavior.Asynchronous) ? Dns.GetHostAddresses(hostName) : (await Dns.GetHostAddressesAsync(hostName).ConfigureAwait(continueOnCapturedContext: false)));
				ipAddresses = array;
			}
			catch (SocketException ex)
			{
				Log.FailedToResolveHostName(m_logger, ex, Id, hostName, hostNameIndex + 1, hostNames.Count, ex.Message);
				continue;
			}
			for (int ipAddressIndex = 0; ipAddressIndex < ipAddresses.Length; ipAddressIndex++)
			{
				IPAddress iPAddress = ipAddresses[ipAddressIndex];
				string ipAddressString = iPAddress.ToString();
				Log.ConnectingToIpAddress(m_logger, Id, ipAddressString, ipAddressIndex + 1, ipAddresses.Length, hostName, hostNameIndex + 1, hostNames.Count);
				m_activityTags["net.peer.ip"] = ipAddressString;
				if (ipAddressString != hostName)
				{
					m_activityTags["net.peer.name"] = hostName;
				}
				else
				{
					m_activityTags.Remove("net.peer.name");
				}
				if (activity != null && activity.IsAllDataRequested)
				{
					activity.SetTag("net.peer.ip", ipAddressString);
					if (ipAddressString != hostName)
					{
						activity.SetTag("net.peer.name", hostName);
					}
					else
					{
						activity.SetTag("net.peer.name", null);
					}
				}
				TcpClient tcpClient = null;
				try
				{
					tcpClient = new TcpClient(iPAddress.AddressFamily);
					using (cancellationToken.Register(delegate
					{
						tcpClient?.Client?.Dispose();
					}))
					{
						_ = 1;
						try
						{
							if (ioBehavior == IOBehavior.Asynchronous)
							{
								await tcpClient.ConnectAsync(iPAddress, cs.Port).ConfigureAwait(continueOnCapturedContext: false);
							}
							else if (Utility.IsWindows())
							{
								tcpClient.Connect(iPAddress, cs.Port);
							}
							else
							{
								int sendTimeout = tcpClient.Client.SendTimeout;
								int receiveTimeout = tcpClient.Client.ReceiveTimeout;
								tcpClient.Client.SendTimeout = cs.ConnectionTimeoutMilliseconds;
								tcpClient.Client.ReceiveTimeout = cs.ConnectionTimeoutMilliseconds;
								tcpClient.Connect(iPAddress, cs.Port);
								tcpClient.Client.SendTimeout = sendTimeout;
								tcpClient.Client.ReceiveTimeout = receiveTimeout;
							}
						}
						catch (Exception ex2) when (((Func<bool>)delegate
						{
							// Could not convert BlockContainer to single expression
							flag = cancellationToken.IsCancellationRequested;
							if (flag)
							{
								flag = ((ex2 is ObjectDisposedException || ex2 is SocketException) ? true : false);
							}
							return flag;
						}).Invoke())
						{
							SafeDispose(ref tcpClient);
							Log.ConnectTimeoutExpired(m_logger, ex2, Id, ipAddressString, hostName);
							throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Connect Timeout expired.");
						}
					}
				}
				catch (SocketException ex3)
				{
					SafeDispose(ref tcpClient);
					if (hostNameIndex == hostNames.Count - 1 && ipAddressIndex == ipAddresses.Length - 1)
					{
						lock (m_lock)
						{
							m_state = State.Failed;
						}
						if (hostNames.Count == 1 && ipAddresses.Length == 1)
						{
							Log.FailedToConnectToSingleIpAddress(m_logger, ex3, Id, ipAddressString, hostName, ex3.Message);
						}
						else
						{
							Log.FailedToConnectToIpAddress(m_logger, ex3, LogLevel.Information, Id, ipAddressString, ipAddressIndex + 1, ipAddresses.Length, hostName, hostNameIndex + 1, hostNames.Count, ex3.Message);
						}
						throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Unable to connect to any of the specified MySQL hosts.");
					}
					Log.FailedToConnectToIpAddress(m_logger, ex3, LogLevel.Trace, Id, ipAddressString, ipAddressIndex + 1, ipAddresses.Length, hostName, hostNameIndex + 1, hostNames.Count, ex3.Message);
					continue;
				}
				if (!tcpClient.Connected && cancellationToken.IsCancellationRequested)
				{
					SafeDispose(ref tcpClient);
					Log.ConnectTimeoutExpired(m_logger, null, Id, ipAddressString, hostName);
					throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Connect Timeout expired.");
				}
				try
				{
					HostName = hostName;
					m_tcpClient = tcpClient;
					m_socket = m_tcpClient.Client;
					m_socket.NoDelay = true;
					m_stream = m_tcpClient.GetStream();
					m_socket.SetKeepAlive(cs.Keepalive);
				}
				catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
				{
					Utility.Dispose(ref m_stream);
					SafeDispose(ref m_tcpClient);
					SafeDispose(ref m_socket);
					Log.ConnectTimeoutExpired(m_logger, null, Id, ipAddressString, hostName);
					throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Connect Timeout expired.");
				}
				lock (m_lock)
				{
					m_state = State.Connected;
				}
				Log.ConnectedToIpAddress(m_logger, Id, ipAddressString, hostName, (m_socket.LocalEndPoint as IPEndPoint)?.Port);
				return true;
			}
		}
		return false;
	}

	private async Task<bool> OpenUnixSocketAsync(ConnectionSettings cs, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		Log.ConnectingToUnixSocket(m_logger, Id, cs.UnixSocket);
		m_activityTags.Add("net.transport", "unix");
		m_activityTags.Add("net.peer.name", cs.UnixSocket);
		if (activity != null && activity.IsAllDataRequested)
		{
			activity.SetTag("net.transport", "unix").SetTag("net.peer.name", cs.UnixSocket);
		}
		Socket socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
		UnixDomainSocketEndPoint unixDomainSocketEndPoint = new UnixDomainSocketEndPoint(cs.UnixSocket);
		try
		{
			using (cancellationToken.Register(socket.Dispose))
			{
				try
				{
					if (ioBehavior == IOBehavior.Asynchronous)
					{
						await Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, unixDomainSocketEndPoint, null).ConfigureAwait(continueOnCapturedContext: false);
					}
					else
					{
						socket.Connect(unixDomainSocketEndPoint);
					}
				}
				catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
				{
					Log.ConnectTimeoutExpiredForUnixSocket(m_logger, Id, cs.UnixSocket);
					throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Connect Timeout expired.");
				}
			}
		}
		catch (SocketException)
		{
			socket.Dispose();
		}
		if (socket.Connected)
		{
			m_socket = socket;
			m_stream = new NetworkStream(socket);
			lock (m_lock)
			{
				m_state = State.Connected;
			}
			return true;
		}
		return false;
	}

	private async Task<bool> OpenNamedPipeAsync(ConnectionSettings cs, long startingTimestamp, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		Log.ConnectingToNamedPipe(m_logger, Id, cs.PipeName, cs.HostNames[0]);
		string value = "\\\\" + cs.HostNames[0] + "\\pipe\\" + cs.PipeName;
		m_activityTags.Add("net.transport", "pipe");
		m_activityTags.Add("net.peer.name", value);
		if (activity != null && activity.IsAllDataRequested)
		{
			activity.SetTag("net.transport", "pipe");
			activity.SetTag("net.peer.name", value);
		}
		NamedPipeClientStream namedPipeStream = new NamedPipeClientStream(cs.HostNames[0], cs.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
		int timeout = Math.Max(1, cs.ConnectionTimeoutMilliseconds - Utility.GetElapsedMilliseconds(startingTimestamp));
		try
		{
			using (cancellationToken.Register(namedPipeStream.Dispose))
			{
				try
				{
					if (ioBehavior == IOBehavior.Asynchronous)
					{
						await namedPipeStream.ConnectAsync(timeout, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
					else
					{
						namedPipeStream.Connect(timeout);
					}
				}
				catch (Exception ex) when ((ex is ObjectDisposedException && cancellationToken.IsCancellationRequested) || ex is TimeoutException)
				{
					Log.ConnectTimeoutExpiredForNamedPipe(m_logger, ex, Id, cs.PipeName, cs.HostNames[0]);
					throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Connect Timeout expired.");
				}
			}
		}
		catch (IOException)
		{
			namedPipeStream.Dispose();
		}
		if (namedPipeStream.IsConnected)
		{
			m_stream = namedPipeStream;
			lock (m_lock)
			{
				m_state = State.Connected;
			}
			return true;
		}
		return false;
	}

	private async Task InitSslAsync(ProtocolCapabilities serverCapabilities, ConnectionSettings cs, MySqlConnection connection, SslProtocols sslProtocols, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		Log.InitializingTlsConnection(m_logger, Id);
		X509CertificateCollection clientCertificates = null;
		if (cs.CertificateStoreLocation != MySqlCertificateStoreLocation.None)
		{
			try
			{
				StoreLocation storeLocation = ((cs.CertificateStoreLocation == MySqlCertificateStoreLocation.CurrentUser) ? StoreLocation.CurrentUser : StoreLocation.LocalMachine);
				using X509Store x509Store = new X509Store(StoreName.My, storeLocation);
				x509Store.Open(OpenFlags.OpenExistingOnly);
				if (cs.CertificateThumbprint.Length == 0)
				{
					if (x509Store.Certificates.Count == 0)
					{
						Log.NoCertificatesFound(m_logger, Id);
						throw new MySqlException("No certificates were found in the certificate store");
					}
					clientCertificates = new X509CertificateCollection(x509Store.Certificates);
				}
				else
				{
					MySqlSslMode sslMode = cs.SslMode;
					bool flag = (uint)(sslMode - 3) <= 1u;
					bool validOnly = flag;
					X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindByThumbprint, cs.CertificateThumbprint, validOnly);
					if (x509Certificate2Collection.Count == 0)
					{
						Log.CertificateNotFoundInStore(m_logger, Id, cs.CertificateThumbprint);
						throw new MySqlException("Certificate with Thumbprint " + cs.CertificateThumbprint + " not found");
					}
					clientCertificates = new X509CertificateCollection(x509Certificate2Collection);
				}
			}
			catch (CryptographicException ex)
			{
				Log.CouldNotLoadCertificate(m_logger, ex, Id, cs.CertificateStoreLocation);
				throw new MySqlException("Certificate couldn't be loaded from the CertificateStoreLocation", ex);
			}
		}
		if (cs.SslKeyFile.Length != 0 && cs.SslCertificateFile.Length != 0)
		{
			clientCertificates = LoadCertificate(cs.SslKeyFile, cs.SslCertificateFile);
		}
		else if (cs.CertificateFile.Length != 0)
		{
			try
			{
				X509Certificate2 x509Certificate = new X509Certificate2(cs.CertificateFile, cs.CertificatePassword, X509KeyStorageFlags.MachineKeySet);
				if (!x509Certificate.HasPrivateKey)
				{
					x509Certificate.Dispose();
					Log.NoPrivateKeyIncludedWithCertificateFile(m_logger, Id, cs.CertificateFile);
					throw new MySqlException("CertificateFile does not contain a private key. CertificateFile should be in PKCS #12 (.pfx) format and contain both a Certificate and Private Key");
				}
				m_clientCertificate = x509Certificate;
				clientCertificates = new X509CertificateCollection { x509Certificate };
			}
			catch (CryptographicException ex2)
			{
				Log.CouldNotLoadCertificateFromFile(m_logger, ex2, Id, cs.CertificateFile);
				if (!File.Exists(cs.CertificateFile))
				{
					throw new MySqlException("Cannot find Certificate File", ex2);
				}
				throw new MySqlException("Either the Certificate Password is incorrect or the Certificate File is invalid", ex2);
			}
		}
		if (clientCertificates == null)
		{
			Func<X509CertificateCollection, ValueTask> provideClientCertificatesCallback = connection.ProvideClientCertificatesCallback;
			if (provideClientCertificatesCallback != null)
			{
				clientCertificates = new X509CertificateCollection();
				try
				{
					await provideClientCertificatesCallback(clientCertificates).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (Exception ex3)
				{
					Log.FailedToObtainClientCertificates(m_logger, ex3, Id, ex3.Message);
					throw new MySqlException("Failed to obtain client certificates via ProvideClientCertificatesCallback", ex3);
				}
			}
		}
		X509Chain caCertificateChain = null;
		if (cs.CACertificateFile.Length != 0)
		{
			X509Chain x509Chain = new X509Chain
			{
				ChainPolicy = 
				{
					RevocationMode = X509RevocationMode.NoCheck,
					VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority
				}
			};
			try
			{
				Log.LoadingCaCertificatesFromFile(m_logger, Id, cs.CACertificateFile);
				byte[] array;
				try
				{
					array = File.ReadAllBytes(cs.CACertificateFile);
				}
				catch (Exception ex4)
				{
					Log.CouldNotLoadCaCertificateFromFile(m_logger, ex4, LogLevel.Error, Id, cs.CACertificateFile);
					if (!File.Exists(cs.CACertificateFile))
					{
						throw new MySqlException("Cannot find CA Certificate File: " + cs.CACertificateFile, ex4);
					}
					throw new MySqlException("Could not load CA Certificate File: " + cs.CACertificateFile, ex4);
				}
				int num = 0;
				while (num != -1)
				{
					int num2 = Utility.FindNextIndex(array, num + 1, "-----BEGIN CERTIFICATE-----"u8);
					try
					{
						Log.LoadingCaCertificate(m_logger, Id, num);
						X509Certificate2 certificate = new X509Certificate2(Utility.ArraySlice(array, num, ((num2 == -1) ? array.Length : num2) - num), (string?)null, X509KeyStorageFlags.MachineKeySet);
						x509Chain.ChainPolicy.ExtraStore.Add(certificate);
					}
					catch (CryptographicException exception)
					{
						Log.CouldNotLoadCaCertificateFromFile(m_logger, exception, LogLevel.Warning, Id, cs.CACertificateFile);
					}
					num = num2;
				}
				Log.LoadedCaCertificatesFromFile(m_logger, Id, x509Chain.ChainPolicy.ExtraStore.Count, cs.CACertificateFile);
				caCertificateChain = x509Chain;
				x509Chain = null;
			}
			finally
			{
				x509Chain?.Dispose();
			}
		}
		RemoteCertificateValidationCallback userCertificateValidationCallback = ValidateRemoteCertificate;
		if (connection.RemoteCertificateValidationCallback != null)
		{
			if (caCertificateChain != null)
			{
				Log.NotUsingRemoteCertificateValidationCallbackDueToSslCa(m_logger, Id);
			}
			else
			{
				MySqlSslMode sslMode = cs.SslMode;
				if (sslMode != MySqlSslMode.Preferred && sslMode != MySqlSslMode.Required)
				{
					Log.NotUsingRemoteCertificateValidationCallbackDueToSslMode(m_logger, Id, cs.SslMode);
				}
				else
				{
					Log.UsingRemoteCertificateValidationCallback(m_logger, Id);
					userCertificateValidationCallback = connection.RemoteCertificateValidationCallback;
				}
			}
		}
		SslStream sslStream = ((clientCertificates == null) ? new SslStream(m_stream, leaveInnerStreamOpen: false, userCertificateValidationCallback) : new SslStream(m_stream, leaveInnerStreamOpen: false, userCertificateValidationCallback, ValidateLocalCertificate));
		bool checkCertificateRevocation = cs.SslMode == MySqlSslMode.VerifyFull;
		using (PayloadData initSsl = HandshakeResponse41Payload.CreateWithSsl(serverCapabilities, cs, m_useCompression, m_characterSet))
		{
			await SendReplyAsync(initSsl, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SslClientAuthenticationOptions sslClientAuthenticationOptions = new SslClientAuthenticationOptions
		{
			EnabledSslProtocols = sslProtocols,
			ClientCertificates = clientCertificates,
			TargetHost = HostName,
			CertificateRevocationCheckMode = (checkCertificateRevocation ? X509RevocationMode.Online : X509RevocationMode.NoCheck)
		};
		try
		{
			if (ioBehavior == IOBehavior.Asynchronous)
			{
				await sslStream.AuthenticateAsClientAsync(sslClientAuthenticationOptions.TargetHost, sslClientAuthenticationOptions.ClientCertificates, sslClientAuthenticationOptions.EnabledSslProtocols, checkCertificateRevocation).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				sslStream.AuthenticateAsClient(sslClientAuthenticationOptions.TargetHost, sslClientAuthenticationOptions.ClientCertificates, sslClientAuthenticationOptions.EnabledSslProtocols, checkCertificateRevocation);
			}
			StreamByteHandler byteHandler = new StreamByteHandler(sslStream);
			m_payloadHandler.ByteHandler = byteHandler;
			m_isSecureConnection = true;
			m_sslStream = sslStream;
			Log.ConnectedTlsDetailed(m_logger, Id, sslStream.SslProtocol, sslStream.CipherAlgorithm, sslStream.HashAlgorithm, sslStream.KeyExchangeAlgorithm, sslStream.KeyExchangeStrength);
		}
		catch (Exception ex5)
		{
			Log.CouldNotInitializeTlsConnection(m_logger, ex5, Id);
			sslStream.Dispose();
			ShutdownSocket();
			HostName = "";
			lock (m_lock)
			{
				m_state = State.Failed;
			}
			if (ex5 is AuthenticationException)
			{
				throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "SSL Authentication Error", ex5);
			}
			if (ex5 is IOException && clientCertificates != null)
			{
				throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "MySQL Server rejected client certificate", ex5);
			}
			if (ex5 is Win32Exception { NativeErrorCode: -2146893007 })
			{
				throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "The server doesn't support the client's specified TLS versions.", ex5);
			}
			throw;
		}
		finally
		{
			caCertificateChain?.Dispose();
		}
		X509CertificateCollection LoadCertificate(string sslKeyFile, string sslCertificateFile)
		{
			Log.LoadingClientKeyFromKeyFile(m_logger, Id, sslKeyFile);
			string key;
			try
			{
				key = File.ReadAllText(sslKeyFile);
			}
			catch (Exception ex7)
			{
				Log.CouldNotLoadClientKeyFromKeyFile(m_logger, ex7, Id, sslKeyFile);
				throw new MySqlException("Could not load the client key from '" + sslKeyFile + "'", ex7);
			}
			RSAParameters rsaParameters;
			try
			{
				rsaParameters = Utility.GetRsaParameters(key);
			}
			catch (FormatException ex8)
			{
				Log.CouldNotLoadClientKeyFromKeyFile(m_logger, ex8, Id, sslKeyFile);
				throw new MySqlException("Could not load the client key from '" + sslKeyFile + "'", ex8);
			}
			try
			{
				RSA rSA;
				try
				{
					rSA = new RSACryptoServiceProvider(new CspParameters
					{
						KeyContainerName = Guid.NewGuid().ToString()
					})
					{
						PersistKeyInCsp = true
					};
				}
				catch (PlatformNotSupportedException)
				{
					rSA = RSA.Create();
				}
				rSA.ImportParameters(rsaParameters);
				X509Certificate2 x509Certificate2;
				using (X509Certificate2 certificate2 = new X509Certificate2(sslCertificateFile))
				{
					x509Certificate2 = certificate2.CopyWithPrivateKey(rSA);
				}
				m_clientCertificate = x509Certificate2;
				return new X509CertificateCollection { x509Certificate2 };
			}
			catch (CryptographicException ex10)
			{
				Log.CouldNotLoadClientKeyFromKeyFile(m_logger, ex10, Id, sslCertificateFile);
				if (!File.Exists(sslCertificateFile))
				{
					throw new MySqlException("Cannot find client certificate file: " + sslCertificateFile, ex10);
				}
				throw new MySqlException("Could not load the client key from " + sslCertificateFile, ex10);
			}
		}
		static X509Certificate ValidateLocalCertificate(object lcbSender, string lcbTargetHost, X509CertificateCollection lcbLocalCertificates, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] X509Certificate lcbRemoteCertificate, string[] lcbAcceptableIssuers)
		{
			return lcbLocalCertificates[0];
		}
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		bool ValidateRemoteCertificate([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] object rcbSender, X509Certificate rcbCertificate, X509Chain rcbChain, SslPolicyErrors rcbPolicyErrors)
		{
			MySqlSslMode sslMode2 = cs.SslMode;
			if ((uint)(sslMode2 - 1) <= 1u)
			{
				return true;
			}
			if ((rcbPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != SslPolicyErrors.None && rcbChain != null && caCertificateChain != null)
			{
				X509Chain x509Chain2 = caCertificateChain;
				X509ChainElementCollection chainElements = rcbChain.ChainElements;
				if (x509Chain2.Build(chainElements[chainElements.Count - 1].Certificate) && caCertificateChain.ChainStatus.Length != 0 && caCertificateChain.ChainStatus[0].Status == X509ChainStatusFlags.UntrustedRoot)
				{
					X509ChainElementCollection chainElements2 = caCertificateChain.ChainElements;
					X509Certificate2 certificate2 = chainElements2[chainElements2.Count - 1].Certificate;
					X509Certificate2Enumerator enumerator = caCertificateChain.ChainPolicy.ExtraStore.GetEnumerator();
					while (enumerator.MoveNext())
					{
						X509Certificate2 current = enumerator.Current;
						if (MemoryExtensions.AsSpan(certificate2.RawData).SequenceEqual(current.RawData))
						{
							rcbPolicyErrors &= ~SslPolicyErrors.RemoteCertificateChainErrors;
							break;
						}
					}
				}
			}
			if (cs.SslMode == MySqlSslMode.VerifyCA)
			{
				rcbPolicyErrors &= ~SslPolicyErrors.RemoteCertificateNameMismatch;
			}
			return rcbPolicyErrors == SslPolicyErrors.None;
		}
	}

	private bool ShouldGetRealServerDetails(ConnectionSettings cs)
	{
		bool flag;
		switch (ServerVersion.OriginalString)
		{
		case "5.6.47.0":
		case "5.6.42.0":
		case "5.6.39.0":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			return true;
		}
		if (cs.ConnectionProtocol == MySqlConnectionProtocol.Sockets && Enumerable.Contains(cs.UserID, '@'))
		{
			if (!HostName.EndsWith(".mysql.database.azure.com", StringComparison.OrdinalIgnoreCase) && !HostName.EndsWith(".database.windows.net", StringComparison.OrdinalIgnoreCase))
			{
				return HostName.EndsWith(".mysql.database.chinacloudapi.cn", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		return false;
	}

	private async Task GetRealServerDetailsAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		Log.DetectedProxy(m_logger, Id);
		try
		{
			PayloadData payload = (SupportsQueryAttributes ? s_selectConnectionIdVersionWithAttributesPayload : s_selectConnectionIdVersionNoAttributesPayload);
			await SendAsync(payload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			await ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			await ReceiveReplyAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
			await ReceiveReplyAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
			if (!SupportsDeprecateEof)
			{
				EofPayload.Create((await ReceiveReplyAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false)).Span);
			}
			ReadRow((await ReceiveReplyAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false)).Span, out var connectionId, out var serverVersion);
			payload = await ReceiveReplyAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
			if (OkPayload.IsOk(payload.Span, SupportsDeprecateEof))
			{
				OkPayload.Verify(payload.Span, SupportsDeprecateEof, SupportsSessionTrack);
			}
			else
			{
				EofPayload.Create(payload.Span);
			}
			if (connectionId.HasValue)
			{
				int valueOrDefault = connectionId.GetValueOrDefault();
				if (serverVersion != null)
				{
					Log.ChangingConnectionId(m_logger, Id, ConnectionId, valueOrDefault, ServerVersion.OriginalString, serverVersion.OriginalString);
					ConnectionId = valueOrDefault;
					ServerVersion = serverVersion;
				}
			}
		}
		catch (MySqlException exception)
		{
			Log.FailedToGetConnectionId(m_logger, exception, Id);
		}
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
		static void ReadRow(ReadOnlySpan<byte> span, out int? reference, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] out ServerVersion reference2)
		{
			ByteArrayReader byteArrayReader = new ByteArrayReader(span);
			int num = byteArrayReader.ReadLengthEncodedIntegerOrNull();
			reference = ((num != -1 && Utf8Parser.TryParse(byteArrayReader.ReadByteString(num), out int value, out int _, '\0')) ? new int?(value) : ((int?)null));
			num = byteArrayReader.ReadLengthEncodedIntegerOrNull();
			reference2 = ((num != -1) ? new ServerVersion(byteArrayReader.ReadByteString(num)) : null);
		}
	}

	private void ShutdownSocket()
	{
		Log.ClosingStreamSocket(m_logger, Id);
		Utility.Dispose(ref m_payloadHandler);
		Utility.Dispose(ref m_stream);
		SafeDispose(ref m_tcpClient);
		SafeDispose(ref m_socket);
		Utility.Dispose(ref m_clientCertificate);
		m_activityTags.Clear();
	}

	private static void SafeDispose<T>([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] ref T disposable) where T : class, IDisposable
	{
		if (disposable != null)
		{
			try
			{
				disposable.Dispose();
			}
			catch (IOException)
			{
			}
			catch (SocketException)
			{
			}
			disposable = null;
		}
	}

	internal void SetFailed(Exception exception)
	{
		Log.SettingStateToFailed(m_logger, exception, Id);
		lock (m_lock)
		{
			m_state = State.Failed;
		}
		if (OwningConnection != null && OwningConnection.TryGetTarget(out var target))
		{
			target.SetState(ConnectionState.Closed);
		}
	}

	private void VerifyState(State state)
	{
		if (m_state != state)
		{
			ExpectedSessionState1(m_logger, Id, state, m_state);
			throw new InvalidOperationException($"Expected state to be {state} but was {m_state}.");
		}
	}

	private void VerifyState(State state1, State state2, State state3, State state4, State state5, State state6)
	{
		if (m_state != state1 && m_state != state2 && m_state != state3 && m_state != state4 && m_state != state5 && m_state != state6)
		{
			ExpectedSessionState6(m_logger, Id, state1, state2, state3, state4, state5, state6, m_state);
			throw new InvalidOperationException(string.Format("Expected state to be ({0}|{1}|{2}|{3}|{4}|{5}) but was {6}.", new object[7] { state1, state2, state3, state4, state5, state6, m_state }));
		}
	}

	private byte[] CreateConnectionAttributes([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] string programName)
	{
		Log.CreatingConnectionAttributes(m_logger, Id);
		ByteBufferWriter byteBufferWriter = new ByteBufferWriter();
		byteBufferWriter.WriteLengthEncodedString("_client_name");
		byteBufferWriter.WriteLengthEncodedString("MySqlConnector");
		byteBufferWriter.WriteLengthEncodedString("_client_version");
		string text = typeof(ServerSession).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
		int num = text.IndexOf('+');
		if (num != -1)
		{
			text = text.Substring(0, num);
		}
		byteBufferWriter.WriteLengthEncodedString(text);
		try
		{
			Utility.GetOSDetails(out var os, out var osDescription, out var architecture);
			if (os != null)
			{
				byteBufferWriter.WriteLengthEncodedString("_os");
				byteBufferWriter.WriteLengthEncodedString(os);
			}
			byteBufferWriter.WriteLengthEncodedString("_os_details");
			byteBufferWriter.WriteLengthEncodedString(osDescription);
			byteBufferWriter.WriteLengthEncodedString("_platform");
			byteBufferWriter.WriteLengthEncodedString(architecture);
		}
		catch (PlatformNotSupportedException)
		{
		}
		using Process process = Process.GetCurrentProcess();
		int id = process.Id;
		byteBufferWriter.WriteLengthEncodedString("_pid");
		byteBufferWriter.WriteLengthEncodedString(id.ToString(CultureInfo.InvariantCulture));
		if (!string.IsNullOrEmpty(programName))
		{
			byteBufferWriter.WriteLengthEncodedString("program_name");
			byteBufferWriter.WriteLengthEncodedString(programName);
		}
		using PayloadData payloadData = byteBufferWriter.ToPayloadData();
		ReadOnlySpan<byte> span = payloadData.Span;
		ByteBufferWriter byteBufferWriter2 = new ByteBufferWriter(span.Length + 9);
		byteBufferWriter2.WriteLengthEncodedInteger((ulong)span.Length);
		byteBufferWriter2.Write(span);
		using PayloadData payloadData2 = byteBufferWriter2.ToPayloadData();
		return payloadData2.Memory.ToArray();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	private MySqlException CreateExceptionForErrorPayload(ReadOnlySpan<byte> span)
	{
		ErrorPayload errorPayload = ErrorPayload.Create(span);
		Log.ErrorPayload(m_logger, Id, errorPayload.ErrorCode, errorPayload.State, errorPayload.Message);
		MySqlException ex = errorPayload.ToException();
		if (ex.ErrorCode == MySqlErrorCode.ClientInteractionTimeout)
		{
			SetFailed(ex);
		}
		return ex;
	}

	private void ClearPreparedStatements()
	{
		if (m_preparedStatements == null)
		{
			return;
		}
		foreach (KeyValuePair<string, PreparedStatements> preparedStatement in m_preparedStatements)
		{
			preparedStatement.Value.Dispose();
		}
		m_preparedStatements.Clear();
	}

	private string GetPassword(ConnectionSettings cs, MySqlConnection connection)
	{
		if (cs.Password.Length != 0)
		{
			return cs.Password;
		}
		Func<MySqlProvidePasswordContext, string> providePasswordCallback = connection.ProvidePasswordCallback;
		if (providePasswordCallback != null)
		{
			try
			{
				Log.ObtainingPasswordViaProvidePasswordCallback(m_logger, Id);
				return providePasswordCallback(new MySqlProvidePasswordContext(HostName, cs.Port, cs.UserID, cs.Database));
			}
			catch (Exception ex)
			{
				Log.FailedToObtainPassword(m_logger, ex, Id, ex.Message);
				throw new MySqlException(MySqlErrorCode.ProvidePasswordCallbackFailed, "Failed to obtain password via ProvidePasswordCallback", ex);
			}
		}
		return "";
	}

	[GeneratedCode("Microsoft.Extensions.Logging.Generators", "7.0.8.27404")]
	[LoggerMessage(2200, LogLevel.Error, "Session {SessionId} can't execute new command when in state {SessionState}")]
	private static void CannotExecuteNewCommandInState(ILogger logger, string sessionId, State sessionState)
	{
		if (logger.IsEnabled(LogLevel.Error))
		{
			__CannotExecuteNewCommandInStateCallback(logger, sessionId, sessionState, null);
		}
	}

	[GeneratedCode("Microsoft.Extensions.Logging.Generators", "7.0.8.27404")]
	[LoggerMessage(2201, LogLevel.Trace, "Session {SessionId} entering FinishQuerying; state is {SessionState}")]
	private static void EnteringFinishQuerying(ILogger logger, string sessionId, State sessionState)
	{
		if (logger.IsEnabled(LogLevel.Trace))
		{
			__EnteringFinishQueryingCallback(logger, sessionId, sessionState, null);
		}
	}

	[LoggerMessage(2011, LogLevel.Error, "Session {SessionId} should have state {ExpectedState1} but was {SessionState}")]
	[GeneratedCode("Microsoft.Extensions.Logging.Generators", "7.0.8.27404")]
	private static void ExpectedSessionState1(ILogger logger, string sessionId, State expectedState1, State sessionState)
	{
		if (logger.IsEnabled(LogLevel.Error))
		{
			__ExpectedSessionState1Callback(logger, sessionId, expectedState1, sessionState, null);
		}
	}

	[GeneratedCode("Microsoft.Extensions.Logging.Generators", "7.0.8.27404")]
	[LoggerMessage(2016, LogLevel.Error, "Session {SessionId} should have state {ExpectedState1} or {ExpectedState2} or {ExpectedState3} or {ExpectedState4} or {ExpectedState5} or {ExpectedState6} but was {SessionState}")]
	private static void ExpectedSessionState6(ILogger logger, string sessionId, State expectedState1, State expectedState2, State expectedState3, State expectedState4, State expectedState5, State expectedState6, State sessionState)
	{
		if (logger.IsEnabled(LogLevel.Error))
		{
			logger.Log(LogLevel.Error, new EventId(2016, "ExpectedSessionState6"), new __ExpectedSessionState6Struct(sessionId, expectedState1, expectedState2, expectedState3, expectedState4, expectedState5, expectedState6, sessionState), null, __ExpectedSessionState6Struct.Format);
		}
	}
}
