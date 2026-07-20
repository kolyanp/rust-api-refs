using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector.Core;
using MySqlConnector.Logging;
using MySqlConnector.Protocol;
using MySqlConnector.Protocol.Payloads;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
public sealed class MySqlDataReader : DbDataReader, IDbColumnSchemaGenerator
{
	private readonly ResultSet m_resultSet;

	private CommandBehavior m_behavior;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private ICommandPayloadCreator m_payloadCreator;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 2 })]
	private IDictionary<string, CachedProcedure> m_cachedProcedures;

	private CommandListPosition m_commandListPosition;

	private bool m_closed;

	private bool m_hasWarnings;

	private bool m_hasMoreResults;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private DataTable m_schemaTable;

	public override int FieldCount
	{
		get
		{
			VerifyNotDisposed();
			if (m_resultSet != null)
			{
				if (!m_resultSet.ContainsCommandParameters)
				{
					return m_resultSet.FieldCount;
				}
				return 0;
			}
			throw new InvalidOperationException("There is no current result set.");
		}
	}

	public override object this[int ordinal] => GetResultSet().GetCurrentRow()[ordinal];

	public override object this[string name] => GetResultSet().GetCurrentRow()[name];

	public override bool HasRows
	{
		get
		{
			VerifyNotDisposed();
			if (m_resultSet != null)
			{
				if (!m_resultSet.ContainsCommandParameters)
				{
					return m_resultSet.HasRows;
				}
				return false;
			}
			throw new InvalidOperationException("There is no current result set.");
		}
	}

	public override bool IsClosed => Command == null;

	public override int RecordsAffected
	{
		get
		{
			ulong? realRecordsAffected = RealRecordsAffected;
			if (realRecordsAffected.HasValue)
			{
				ulong valueOrDefault = realRecordsAffected.GetValueOrDefault();
				return checked((int)valueOrDefault);
			}
			return -1;
		}
	}

	public override int Depth => GetResultSet().Depth;

	public override int VisibleFieldCount => FieldCount;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	internal Activity Activity
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		private set;
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	internal IMySqlCommand Command
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		private set;
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	internal MySqlConnection Connection
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get
		{
			return Command?.Connection;
		}
	}

	internal ulong? RealRecordsAffected { get; set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	internal ServerSession Session
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get
		{
			return Command?.Connection.Session;
		}
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	internal PreparedStatement LastUsedPreparedStatement
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get
		{
			return m_commandListPosition.LastUsedPreparedStatement;
		}
	}

	public override bool NextResult()
	{
		Command?.CancellableCommand.ResetCommandTimeout();
		return NextResultAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
	}

	public override bool Read()
	{
		VerifyNotDisposed();
		Command.CancellableCommand.ResetCommandTimeout();
		return m_resultSet.Read();
	}

	public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
	{
		VerifyNotDisposed();
		Command.CancellableCommand.ResetCommandTimeout();
		using (Command.CancellableCommand.RegisterCancel(cancellationToken))
		{
			return await m_resultSet.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	internal Task<bool> ReadAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		return m_resultSet.ReadAsync(ioBehavior, cancellationToken);
	}

	public override async Task<bool> NextResultAsync(CancellationToken cancellationToken)
	{
		VerifyNotDisposed();
		Command.CancellableCommand.ResetCommandTimeout();
		using (Command.CancellableCommand.RegisterCancel(cancellationToken))
		{
			return await NextResultAsync(Command?.Connection?.AsyncIOBehavior ?? IOBehavior.Asynchronous, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	internal async Task<bool> NextResultAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		VerifyNotDisposed();
		try
		{
			while (true)
			{
				await m_resultSet.ReadEntireAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				await ScanResultSetAsync(ioBehavior, m_resultSet, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (m_hasMoreResults && m_resultSet.ContainsCommandParameters)
				{
					await ReadOutParametersAsync(Command, m_resultSet, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					continue;
				}
				if (!m_hasMoreResults)
				{
					if (m_commandListPosition.CommandIndex < m_commandListPosition.CommandCount)
					{
						Command = m_commandListPosition.CommandAt(m_commandListPosition.CommandIndex);
						using (Command.CancellableCommand.RegisterCancel(cancellationToken))
						{
							ByteBufferWriter byteBufferWriter = new ByteBufferWriter();
							if (!Command.Connection.Session.IsCancelingQuery && m_payloadCreator.WriteQueryCommand(ref m_commandListPosition, m_cachedProcedures, byteBufferWriter, appendSemicolon: false))
							{
								using PayloadData payload = byteBufferWriter.ToPayloadData();
								await Command.Connection.Session.SendAsync(payload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
								await m_resultSet.ReadResultSetHeaderAsync(ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
								ActivateResultSet(cancellationToken);
								m_hasMoreResults = true;
							}
						}
					}
				}
				else
				{
					ActivateResultSet(cancellationToken);
				}
				if (!m_hasMoreResults || (Command.CommandBehavior & (CommandBehavior.SingleResult | CommandBehavior.SingleRow)) == 0)
				{
					break;
				}
			}
			if (!m_hasMoreResults)
			{
				m_resultSet.Reset();
			}
			m_schemaTable = null;
			return m_hasMoreResults;
		}
		catch (MySqlException)
		{
			m_resultSet.Reset();
			m_hasMoreResults = false;
			m_schemaTable = null;
			throw;
		}
	}

	private void ActivateResultSet(CancellationToken cancellationToken)
	{
		if (m_resultSet.ReadResultSetHeaderException != null)
		{
			MySqlException ex = m_resultSet.ReadResultSetHeaderException.SourceException as MySqlException;
			if (ex?.SqlState == null)
			{
				Command.Connection.SetSessionFailed(m_resultSet.ReadResultSetHeaderException.SourceException);
			}
			if (ex != null && ex.ErrorCode == MySqlErrorCode.QueryInterrupted && cancellationToken.IsCancellationRequested)
			{
				throw new OperationCanceledException(ex.Message, ex, cancellationToken);
			}
			if (ex != null && ex.ErrorCode == MySqlErrorCode.QueryInterrupted && Command.CancellableCommand.IsTimedOut)
			{
				throw MySqlException.CreateForTimeout(ex);
			}
			if (ex != null)
			{
				ServerSession.ThrowIfStatementContainsDelimiter(ex, Command);
				m_resultSet.ReadResultSetHeaderException.Throw();
			}
			throw new MySqlException("Failed to read the result set.", m_resultSet.ReadResultSetHeaderException.SourceException);
		}
		m_hasWarnings = m_resultSet.WarningCount != 0;
	}

	private async ValueTask ScanResultSetAsync(IOBehavior ioBehavior, ResultSet resultSet, CancellationToken cancellationToken)
	{
		if (!m_hasMoreResults)
		{
			return;
		}
		ResultSetState bufferState = resultSet.BufferState;
		if ((bufferState == ResultSetState.None || bufferState == ResultSetState.NoMoreData) ? true : false)
		{
			m_hasMoreResults = false;
			return;
		}
		if (resultSet.BufferState != ResultSetState.HasMoreData)
		{
			throw new InvalidOperationException($"Invalid state: {resultSet.BufferState}");
		}
		using (Command.CancellableCommand.RegisterCancel(cancellationToken))
		{
			try
			{
				await resultSet.ReadResultSetHeaderAsync(ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
				m_hasMoreResults = resultSet.BufferState != ResultSetState.NoMoreData;
			}
			catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.QueryInterrupted)
			{
				m_hasMoreResults = false;
				cancellationToken.ThrowIfCancellationRequested();
				throw;
			}
		}
	}

	public override string GetName(int ordinal)
	{
		return GetResultSet().GetName(ordinal);
	}

	public override int GetValues(object[] values)
	{
		return GetResultSet().GetCurrentRow().GetValues(values);
	}

	public override bool IsDBNull(int ordinal)
	{
		return GetResultSet().GetCurrentRow().IsDBNull(ordinal);
	}

	public override int GetOrdinal(string name)
	{
		return GetResultSet().GetOrdinal(name);
	}

	public override bool GetBoolean(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetBoolean(ordinal);
	}

	public bool GetBoolean(string name)
	{
		return GetBoolean(GetOrdinal(name));
	}

	public override byte GetByte(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetByte(ordinal);
	}

	public byte GetByte(string name)
	{
		return GetByte(GetOrdinal(name));
	}

	public sbyte GetSByte(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetSByte(ordinal);
	}

	public sbyte GetSByte(string name)
	{
		return GetSByte(GetOrdinal(name));
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
	{
		return GetResultSet().GetCurrentRow().GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
	}

	public long GetBytes(string name, long dataOffset, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] byte[] buffer, int bufferOffset, int length)
	{
		return GetResultSet().GetCurrentRow().GetBytes(GetOrdinal(name), dataOffset, buffer, bufferOffset, length);
	}

	public override char GetChar(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetChar(ordinal);
	}

	public char GetChar(string name)
	{
		return GetChar(GetOrdinal(name));
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
	{
		return GetResultSet().GetCurrentRow().GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
	}

	public override Guid GetGuid(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetGuid(ordinal);
	}

	public Guid GetGuid(string name)
	{
		return GetGuid(GetOrdinal(name));
	}

	public override short GetInt16(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetInt16(ordinal);
	}

	public short GetInt16(string name)
	{
		return GetInt16(GetOrdinal(name));
	}

	public override int GetInt32(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetInt32(ordinal);
	}

	public int GetInt32(string name)
	{
		return GetInt32(GetOrdinal(name));
	}

	public override long GetInt64(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetInt64(ordinal);
	}

	public long GetInt64(string name)
	{
		return GetInt64(GetOrdinal(name));
	}

	public override string GetDataTypeName(int ordinal)
	{
		return GetResultSet().GetDataTypeName(ordinal);
	}

	public Type GetFieldType(string name)
	{
		return GetFieldType(GetOrdinal(name));
	}

	public override Type GetFieldType(int ordinal)
	{
		return GetResultSet().GetFieldType(ordinal);
	}

	public override object GetValue(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetValue(ordinal);
	}

	public override IEnumerator GetEnumerator()
	{
		return new DbEnumerator(this, closeReader: false);
	}

	protected override DbDataReader GetDbDataReader(int ordinal)
	{
		throw new NotSupportedException();
	}

	public override DateTime GetDateTime(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetDateTime(ordinal);
	}

	public DateTime GetDateTime(string name)
	{
		return GetDateTime(GetOrdinal(name));
	}

	public DateTimeOffset GetDateTimeOffset(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetDateTimeOffset(ordinal);
	}

	public DateTimeOffset GetDateTimeOffset(string name)
	{
		return GetDateTimeOffset(GetOrdinal(name));
	}

	public MySqlDateTime GetMySqlDateTime(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetMySqlDateTime(ordinal);
	}

	public MySqlDateTime GetMySqlDateTime(string name)
	{
		return GetMySqlDateTime(GetOrdinal(name));
	}

	public MySqlGeometry GetMySqlGeometry(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetMySqlGeometry(ordinal);
	}

	public MySqlGeometry GetMySqlGeometry(string name)
	{
		return GetMySqlGeometry(GetOrdinal(name));
	}

	public MySqlDecimal GetMySqlDecimal(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetMySqlDecimal(ordinal);
	}

	public MySqlDecimal GetMySqlDecimal(string name)
	{
		return GetMySqlDecimal(GetOrdinal(name));
	}

	public TimeSpan GetTimeSpan(int ordinal)
	{
		return (TimeSpan)GetValue(ordinal);
	}

	public TimeSpan GetTimeSpan(string name)
	{
		return GetTimeSpan(GetOrdinal(name));
	}

	public override Stream GetStream(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetStream(ordinal);
	}

	public Stream GetStream(string name)
	{
		return GetStream(GetOrdinal(name));
	}

	public override TextReader GetTextReader(int ordinal)
	{
		return new StringReader(GetString(ordinal));
	}

	public TextReader GetTextReader(string name)
	{
		return new StringReader(GetString(name));
	}

	public override string GetString(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetString(ordinal);
	}

	public string GetString(string name)
	{
		return GetString(GetOrdinal(name));
	}

	public override decimal GetDecimal(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetDecimal(ordinal);
	}

	public decimal GetDecimal(string name)
	{
		return GetDecimal(GetOrdinal(name));
	}

	public override double GetDouble(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetDouble(ordinal);
	}

	public double GetDouble(string name)
	{
		return GetDouble(GetOrdinal(name));
	}

	public override float GetFloat(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetFloat(ordinal);
	}

	public float GetFloat(string name)
	{
		return GetFloat(GetOrdinal(name));
	}

	public ushort GetUInt16(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetUInt16(ordinal);
	}

	public ushort GetUInt16(string name)
	{
		return GetUInt16(GetOrdinal(name));
	}

	public uint GetUInt32(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetUInt32(ordinal);
	}

	public uint GetUInt32(string name)
	{
		return GetUInt32(GetOrdinal(name));
	}

	public ulong GetUInt64(int ordinal)
	{
		return GetResultSet().GetCurrentRow().GetUInt64(ordinal);
	}

	public ulong GetUInt64(string name)
	{
		return GetUInt64(GetOrdinal(name));
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	public override DataTable GetSchemaTable()
	{
		return m_schemaTable ?? (m_schemaTable = BuildSchemaTable());
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })]
	public Task<DataTable> GetSchemaTableAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Task.FromResult(GetSchemaTable());
	}

	public override void Close()
	{
		DisposeAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
	}

	public ReadOnlyCollection<DbColumn> GetColumnSchema()
	{
		if (!m_resultSet.HasResultSet || m_resultSet.ContainsCommandParameters)
		{
			return new ReadOnlyCollection<DbColumn>(new List<DbColumn>());
		}
		ReadOnlySpan<ColumnDefinitionPayload> columnDefinitions = m_resultSet.ColumnDefinitions;
		ResultSet resultSet = GetResultSet();
		List<DbColumn> list = new List<DbColumn>(columnDefinitions.Length);
		for (int i = 0; i < columnDefinitions.Length; i++)
		{
			list.Add(new MySqlDbColumn(i, columnDefinitions[i], Connection.AllowZeroDateTime, resultSet.GetColumnType(i)));
		}
		return list.AsReadOnly();
	}

	public Task<ReadOnlyCollection<DbColumn>> GetColumnSchemaAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Task.FromResult(GetColumnSchema());
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public override T GetFieldValue<T>(int ordinal)
	{
		if (typeof(T) == typeof(bool))
		{
			return (T)(object)GetBoolean(ordinal);
		}
		if (typeof(T) == typeof(byte))
		{
			return (T)(object)GetByte(ordinal);
		}
		if (typeof(T) == typeof(sbyte))
		{
			return (T)(object)GetSByte(ordinal);
		}
		if (typeof(T) == typeof(short))
		{
			return (T)(object)GetInt16(ordinal);
		}
		if (typeof(T) == typeof(ushort))
		{
			return (T)(object)GetUInt16(ordinal);
		}
		if (typeof(T) == typeof(int))
		{
			return (T)(object)GetInt32(ordinal);
		}
		if (typeof(T) == typeof(uint))
		{
			return (T)(object)GetUInt32(ordinal);
		}
		if (typeof(T) == typeof(long))
		{
			return (T)(object)GetInt64(ordinal);
		}
		if (typeof(T) == typeof(ulong))
		{
			return (T)(object)GetUInt64(ordinal);
		}
		if (typeof(T) == typeof(char))
		{
			return (T)(object)GetChar(ordinal);
		}
		if (typeof(T) == typeof(decimal))
		{
			return (T)(object)GetDecimal(ordinal);
		}
		if (typeof(T) == typeof(double))
		{
			return (T)(object)GetDouble(ordinal);
		}
		if (typeof(T) == typeof(float))
		{
			return (T)(object)GetFloat(ordinal);
		}
		if (typeof(T) == typeof(string))
		{
			return (T)(object)GetString(ordinal);
		}
		if (typeof(T) == typeof(DateTime))
		{
			return (T)(object)GetDateTime(ordinal);
		}
		if (typeof(T) == typeof(DateTimeOffset))
		{
			return (T)(object)GetDateTimeOffset(ordinal);
		}
		if (typeof(T) == typeof(Guid))
		{
			return (T)(object)GetGuid(ordinal);
		}
		if (typeof(T) == typeof(MySqlGeometry))
		{
			return (T)(object)GetMySqlGeometry(ordinal);
		}
		if (typeof(T) == typeof(Stream))
		{
			return (T)(object)GetStream(ordinal);
		}
		if (typeof(T) == typeof(TextReader) || typeof(T) == typeof(StringReader))
		{
			return (T)(object)GetTextReader(ordinal);
		}
		if (typeof(T) == typeof(TimeSpan))
		{
			return (T)(object)GetTimeSpan(ordinal);
		}
		if (typeof(T) == typeof(MySqlDecimal))
		{
			return (T)(object)GetMySqlDecimal(ordinal);
		}
		return base.GetFieldValue<T>(ordinal);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				DisposeAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	public Task DisposeAsync()
	{
		return DisposeAsync(Connection?.AsyncIOBehavior ?? IOBehavior.Asynchronous, CancellationToken.None);
	}

	internal async Task InitAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 2 })] IDictionary<string, CachedProcedure> cachedProcedures, IMySqlCommand command, CommandBehavior behavior, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		if (m_hasMoreResults)
		{
			throw new InvalidOperationException("Expected m_hasMoreResults to be false");
		}
		if (m_resultSet.BufferState != ResultSetState.None || m_resultSet.State != ResultSetState.None)
		{
			throw new InvalidOperationException("Expected BufferState and State to be ResultSetState.None.");
		}
		m_closed = false;
		m_hasWarnings = false;
		RealRecordsAffected = null;
		m_commandListPosition = commandListPosition;
		m_payloadCreator = payloadCreator;
		m_cachedProcedures = cachedProcedures;
		Command = command;
		m_behavior = behavior;
		Activity = activity;
		command.Connection.SetActiveReader(this);
		try
		{
			await m_resultSet.ReadResultSetHeaderAsync(ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
			ActivateResultSet(cancellationToken);
			m_hasMoreResults = true;
			if (m_resultSet.ContainsCommandParameters)
			{
				await ReadOutParametersAsync(command, m_resultSet, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			while (m_resultSet.State == ResultSetState.NoMoreData && commandListPosition.CommandIndex < commandListPosition.CommandCount)
			{
				await NextResultAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		catch (Exception exception)
		{
			if (activity != null && activity.IsAllDataRequested)
			{
				activity.SetException(exception);
				activity.Stop();
			}
			Dispose();
			throw;
		}
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	internal DataTable BuildSchemaTable()
	{
		if (!m_resultSet.HasResultSet || m_resultSet.ContainsCommandParameters)
		{
			return null;
		}
		DataTable dataTable = new DataTable("SchemaTable")
		{
			Locale = CultureInfo.InvariantCulture,
			MinimumCapacity = m_resultSet.ColumnDefinitions.Length
		};
		DataColumn column = new DataColumn(SchemaTableColumn.ColumnName, typeof(string));
		DataColumn dataColumn = new DataColumn(SchemaTableColumn.ColumnOrdinal, typeof(int));
		DataColumn column2 = new DataColumn(SchemaTableColumn.ColumnSize, typeof(int));
		DataColumn dataColumn2 = new DataColumn(SchemaTableColumn.NumericPrecision, typeof(int));
		DataColumn dataColumn3 = new DataColumn(SchemaTableColumn.NumericScale, typeof(int));
		DataColumn column3 = new DataColumn(SchemaTableColumn.DataType, typeof(Type));
		DataColumn column4 = new DataColumn(SchemaTableColumn.ProviderType, typeof(int));
		DataColumn dataColumn4 = new DataColumn(SchemaTableColumn.IsLong, typeof(bool));
		DataColumn column5 = new DataColumn(SchemaTableColumn.AllowDBNull, typeof(bool));
		DataColumn column6 = new DataColumn(SchemaTableOptionalColumn.IsReadOnly, typeof(bool));
		DataColumn column7 = new DataColumn(SchemaTableOptionalColumn.IsRowVersion, typeof(bool));
		DataColumn column8 = new DataColumn(SchemaTableColumn.IsUnique, typeof(bool));
		DataColumn column9 = new DataColumn(SchemaTableColumn.IsKey, typeof(bool));
		DataColumn column10 = new DataColumn(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool));
		DataColumn column11 = new DataColumn(SchemaTableOptionalColumn.IsHidden, typeof(bool));
		DataColumn column12 = new DataColumn(SchemaTableOptionalColumn.BaseCatalogName, typeof(string));
		DataColumn column13 = new DataColumn(SchemaTableColumn.BaseSchemaName, typeof(string));
		DataColumn column14 = new DataColumn(SchemaTableColumn.BaseTableName, typeof(string));
		DataColumn column15 = new DataColumn(SchemaTableColumn.BaseColumnName, typeof(string));
		DataColumn column16 = new DataColumn(SchemaTableColumn.IsAliased, typeof(bool));
		DataColumn column17 = new DataColumn(SchemaTableColumn.IsExpression, typeof(bool));
		DataColumn column18 = new DataColumn("IsIdentity", typeof(bool));
		dataColumn.DefaultValue = 0;
		dataColumn2.DefaultValue = 0;
		dataColumn3.DefaultValue = 0;
		dataColumn4.DefaultValue = false;
		DataColumnCollection columns = dataTable.Columns;
		columns.Add(column);
		columns.Add(dataColumn);
		columns.Add(column2);
		columns.Add(dataColumn2);
		columns.Add(dataColumn3);
		columns.Add(column8);
		columns.Add(column9);
		columns.Add(column12);
		columns.Add(column15);
		columns.Add(column13);
		columns.Add(column14);
		columns.Add(column3);
		columns.Add(column5);
		columns.Add(column4);
		columns.Add(column16);
		columns.Add(column17);
		columns.Add(column18);
		columns.Add(column10);
		columns.Add(column7);
		columns.Add(column11);
		columns.Add(dataColumn4);
		columns.Add(column6);
		foreach (MySqlDbColumn item in GetColumnSchema())
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow[column] = item.ColumnName;
			dataRow[dataColumn] = item.ColumnOrdinal;
			dataRow[column3] = item.DataType;
			dataRow[column2] = item.ColumnSize;
			dataRow[column4] = item.ProviderType;
			dataRow[dataColumn4] = item.IsLong;
			dataRow[column8] = false;
			dataRow[column9] = item.IsKey;
			dataRow[column5] = item.AllowDBNull;
			dataRow[dataColumn3] = item.NumericScale;
			dataRow[dataColumn2] = item.NumericPrecision.GetValueOrDefault();
			dataRow[column12] = item.BaseCatalogName;
			dataRow[column15] = item.BaseColumnName;
			dataRow[column13] = item.BaseSchemaName;
			dataRow[column14] = item.BaseTableName;
			dataRow[column10] = item.IsAutoIncrement;
			dataRow[column7] = false;
			dataRow[column6] = item.IsReadOnly;
			dataTable.Rows.Add(dataRow);
			dataRow.AcceptChanges();
		}
		return dataTable;
	}

	internal MySqlDataReader()
	{
		m_resultSet = new ResultSet(this);
	}

	internal async Task DisposeAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		if (m_closed)
		{
			return;
		}
		m_closed = true;
		if (m_resultSet != null && Command.Connection.State == ConnectionState.Open)
		{
			Command.Connection.Session.SetTimeout(int.MaxValue);
			try
			{
				while (await NextResultAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
				}
			}
			catch (MySqlException ex)
			{
				if (ex.ErrorCode != MySqlErrorCode.QueryInterrupted)
				{
					Log.IgnoringExceptionInDisposeAsync(Command.Logger, ex, Command.Connection.Session.Id, ex.Message, Command.CommandText);
				}
			}
		}
		m_hasMoreResults = false;
		MySqlConnection connection = Command.Connection;
		Command.CancellableCommand.SetTimeout(int.MaxValue);
		connection.FinishQuerying(m_hasWarnings);
		Activity?.Stop();
		Activity = null;
		if ((m_behavior & CommandBehavior.CloseConnection) != CommandBehavior.Default)
		{
			await connection.CloseAsync(ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
		}
		Command = null;
		m_commandListPosition = default(CommandListPosition);
		m_payloadCreator = null;
		m_cachedProcedures = null;
	}

	private static async Task ReadOutParametersAsync(IMySqlCommand command, ResultSet resultSet, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		await resultSet.ReadAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		Row currentRow = resultSet.GetCurrentRow();
		if (currentRow.GetString(0) != SingleCommandPayloadCreator.OutParameterSentinelColumnName)
		{
			throw new InvalidOperationException("Expected out parameter values.");
		}
		for (int i = 0; i < command.OutParameters.Count; i++)
		{
			MySqlParameter mySqlParameter = command.OutParameters[i];
			int ordinal = i + 1;
			if (mySqlParameter.HasSetDbType && !currentRow.IsDBNull(ordinal))
			{
				DbTypeMapping dbTypeMapping = TypeMapper.Instance.GetDbTypeMapping(mySqlParameter.DbType);
				if (dbTypeMapping != null)
				{
					mySqlParameter.Value = dbTypeMapping.DoConversion(currentRow.GetValue(ordinal));
					continue;
				}
			}
			mySqlParameter.Value = currentRow.GetValue(ordinal);
		}
		if (await resultSet.ReadAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			throw new InvalidOperationException("Expected only one row.");
		}
	}

	private void VerifyNotDisposed()
	{
		if (Command == null)
		{
			throw new InvalidOperationException("Can't call this method when MySqlDataReader is closed.");
		}
	}

	private ResultSet GetResultSet()
	{
		VerifyNotDisposed();
		if (m_resultSet != null && !m_resultSet.ContainsCommandParameters)
		{
			return m_resultSet;
		}
		throw new InvalidOperationException("There is no current result set.");
	}
}
