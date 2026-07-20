using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector.Protocol;
using MySqlConnector.Protocol.Payloads;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector.Core;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class ResultSet(MySqlDataReader dataReader)
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private ResizableArray<byte> m_columnDefinitionPayloadBytes;

	private int m_columnDefinitionPayloadUsedBytes;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
	private Queue<Row> m_readBuffer;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private Row m_row;

	private bool m_hasRows;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	private ReadOnlyMemory<ColumnDefinitionPayload> m_columnDefinitions;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
	private ColumnDefinitionPayload[] m_columnDefinitionPayloadCache;

	public int Depth => 0;

	public int FieldCount => ColumnDefinitions.Length;

	public bool HasRows
	{
		get
		{
			if (BufferState == ResultSetState.ReadResultSetHeader)
			{
				return BufferReadAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult() != null;
			}
			return m_hasRows;
		}
	}

	public MySqlDataReader DataReader { get; } = dataReader;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public ExceptionDispatchInfo ReadResultSetHeaderException
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		private set;
	}

	public IMySqlCommand Command => DataReader.Command;

	public MySqlConnection Connection => DataReader.Connection;

	public ServerSession Session => DataReader.Session;

	public ResultSetState BufferState { get; private set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public ReadOnlySpan<ColumnDefinitionPayload> ColumnDefinitions
	{
		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
		get
		{
			return m_columnDefinitions.Span;
		}
	}

	public int WarningCount { get; private set; }

	public ResultSetState State { get; private set; }

	public bool HasResultSet
	{
		get
		{
			if (State != ResultSetState.None)
			{
				return ColumnDefinitions.Length != 0;
			}
			return false;
		}
	}

	public bool ContainsCommandParameters { get; private set; }

	public void Reset()
	{
		BufferState = ResultSetState.None;
		m_columnDefinitions = default(ReadOnlyMemory<ColumnDefinitionPayload>);
		WarningCount = 0;
		State = ResultSetState.None;
		ContainsCommandParameters = false;
		m_columnDefinitionPayloadUsedBytes = 0;
		m_readBuffer?.Clear();
		m_row = null;
		m_hasRows = false;
		ReadResultSetHeaderException = null;
	}

	public async Task ReadResultSetHeaderAsync(IOBehavior ioBehavior)
	{
		Reset();
		try
		{
			while (true)
			{
				PayloadData payloadData = await Session.ReceiveReplyAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
				switch (payloadData.HeaderByte)
				{
				case 0:
				{
					OkPayload okPayload = OkPayload.Create(payloadData.Span, Session.SupportsDeprecateEof, Session.SupportsSessionTrack);
					if (State != ResultSetState.ReadResultSetHeader || okPayload.AffectedRowCount != 0L)
					{
						DataReader.RealRecordsAffected = DataReader.RealRecordsAffected.GetValueOrDefault() + okPayload.AffectedRowCount;
					}
					if (okPayload.LastInsertId != 0L)
					{
						Command?.SetLastInsertedId((long)okPayload.LastInsertId);
					}
					WarningCount = okPayload.WarningCount;
					if (okPayload.NewSchema != null)
					{
						Connection.Session.DatabaseOverride = okPayload.NewSchema;
					}
					m_columnDefinitions = default(ReadOnlyMemory<ColumnDefinitionPayload>);
					State = (((okPayload.ServerStatus & ServerStatus.MoreResultsExist) == 0) ? ResultSetState.NoMoreData : ResultSetState.HasMoreData);
					if (State == ResultSetState.NoMoreData)
					{
						return;
					}
					continue;
				}
				case 251:
					try
					{
						if (!Connection.AllowLoadLocalInfile)
						{
							throw new NotSupportedException("To use LOAD DATA LOCAL INFILE, set AllowLoadLocalInfile=true in the connection string. See https://fl.vu/mysql-load-data");
						}
						LocalInfilePayload localInfilePayload = LocalInfilePayload.Create(payloadData.Span);
						bool flag = localInfilePayload.FileName.StartsWith(":SOURCE:", StringComparison.Ordinal);
						if (!IsHostVerified(Connection) && !flag)
						{
							throw new NotSupportedException("Use SourceStream or SslMode >= VerifyCA for LOAD DATA LOCAL INFILE. See https://fl.vu/mysql-load-data");
						}
						object obj = (flag ? MySqlBulkLoader.GetAndRemoveSource(localInfilePayload.FileName) : File.OpenRead(localInfilePayload.FileName));
						if (!(obj is Stream stream))
						{
							if (!(obj is MySqlBulkCopy mySqlBulkCopy))
							{
								throw new InvalidOperationException("Unsupported Source type: " + obj.GetType().Name);
							}
							await mySqlBulkCopy.SendDataReaderAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
						}
						else
						{
							byte[] buffer = ArrayPool<byte>.Shared.Rent(1048576);
							try
							{
								int count;
								while ((count = await stream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false)) > 0)
								{
									payloadData = new PayloadData(new ArraySegment<byte>(buffer, 0, count));
									await Session.SendReplyAsync(payloadData, ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
								}
							}
							finally
							{
								ArrayPool<byte>.Shared.Return(buffer);
								stream.Dispose();
							}
						}
					}
					catch (Exception innerException)
					{
						ReadResultSetHeaderException = ExceptionDispatchInfo.Capture(new MySqlException("Error during LOAD DATA LOCAL INFILE", innerException));
					}
					await Session.SendReplyAsync(EmptyPayload.Instance, ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
					continue;
				}
				ColumnCountPayload columnCountPayload = ColumnCountPayload.Create(payloadData.Span, Session.SupportsCachedPreparedMetadata);
				int columnCount = columnCountPayload.ColumnCount;
				if (!columnCountPayload.MetadataFollows)
				{
					m_columnDefinitions = DataReader.LastUsedPreparedStatement.Columns;
					if (m_columnDefinitions.Length != columnCount)
					{
						throw new InvalidOperationException($"Expected result set to have {m_columnDefinitions.Length} columns, but it contains {columnCount} columns");
					}
				}
				else
				{
					Utility.Resize(ref m_columnDefinitionPayloadBytes, columnCount * 96);
					if (m_columnDefinitionPayloadCache == null)
					{
						m_columnDefinitionPayloadCache = new ColumnDefinitionPayload[columnCount];
					}
					else if (m_columnDefinitionPayloadCache.Length < columnCount)
					{
						Array.Resize(ref m_columnDefinitionPayloadCache, Math.Max(columnCount, m_columnDefinitionPayloadCache.Length * 2));
					}
					m_columnDefinitions = MemoryExtensions.AsMemory(m_columnDefinitionPayloadCache, 0, columnCount);
					ColumnDefinitionPayload[] preparedColumns = ((!Session.SupportsCachedPreparedMetadata) ? null : DataReader.LastUsedPreparedStatement?.Columns);
					for (int column = 0; column < columnCount; column++)
					{
						payloadData = await Session.ReceiveReplyAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
						int length = payloadData.Span.Length;
						if (m_columnDefinitionPayloadUsedBytes + length > m_columnDefinitionPayloadBytes.Count)
						{
							Utility.Resize(ref m_columnDefinitionPayloadBytes, m_columnDefinitionPayloadUsedBytes + length);
						}
						payloadData.Span.CopyTo(m_columnDefinitionPayloadBytes.AsSpan(m_columnDefinitionPayloadUsedBytes));
						ResizableArraySegment<byte> arraySegment = new ResizableArraySegment<byte>(m_columnDefinitionPayloadBytes, m_columnDefinitionPayloadUsedBytes, length);
						ColumnDefinitionPayload.Initialize(ref m_columnDefinitionPayloadCache[column], arraySegment);
						if (preparedColumns != null)
						{
							ColumnDefinitionPayload.Initialize(ref preparedColumns[column], arraySegment);
						}
						m_columnDefinitionPayloadUsedBytes += length;
					}
				}
				if (!Session.SupportsDeprecateEof)
				{
					EofPayload.Create((await Session.ReceiveReplyAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false)).Span);
				}
				int length2 = ColumnDefinitions.Length;
				IMySqlCommand command = Command;
				if (length2 == ((command == null) ? ((int?)null) : (command.OutParameters?.Count + 1)) && ColumnDefinitions[0].Name == SingleCommandPayloadCreator.OutParameterSentinelColumnName)
				{
					ContainsCommandParameters = true;
				}
				WarningCount = 0;
				State = ResultSetState.ReadResultSetHeader;
				Activity activity = DataReader.Activity;
				if (activity != null && activity.IsAllDataRequested)
				{
					DataReader.Activity.AddEvent(new ActivityEvent("read-result-set-header"));
				}
				return;
			}
		}
		catch (Exception source)
		{
			ReadResultSetHeaderException = ExceptionDispatchInfo.Capture(source);
		}
		finally
		{
			BufferState = State;
		}
	}

	private static bool IsHostVerified(MySqlConnection connection)
	{
		MySqlSslMode sslMode = connection.SslMode;
		if ((uint)(sslMode - 3) <= 1u)
		{
			return true;
		}
		return false;
	}

	public async Task ReadEntireAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		while (true)
		{
			ResultSetState state = State;
			if ((uint)(state - 1) > 1u)
			{
				break;
			}
			await ReadAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public bool Read()
	{
		return ReadAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
	}

	public Task<bool> ReadAsync(CancellationToken cancellationToken)
	{
		return ReadAsync(Connection.AsyncIOBehavior, cancellationToken);
	}

	public async Task<bool> ReadAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		Queue<Row> readBuffer = m_readBuffer;
		Row row = ((readBuffer == null || readBuffer.Count <= 0) ? (await ScanRowAsync(ioBehavior, m_row, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) : m_readBuffer.Dequeue());
		m_row = row;
		if (Command.ReturnParameter != null && m_row != null)
		{
			Command.ReturnParameter.Value = m_row.GetValue(0);
			Command.ReturnParameter = null;
		}
		if (m_row == null)
		{
			State = BufferState;
			return false;
		}
		State = ResultSetState.ReadingRows;
		return true;
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })]
	public async Task<Row> BufferReadAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		Row row = await ScanRowAsync(ioBehavior, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (row == null)
		{
			return null;
		}
		if (m_readBuffer == null)
		{
			m_readBuffer = new Queue<Row>();
		}
		m_readBuffer.Enqueue(row);
		return row;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 2 })]
	private async ValueTask<Row> ScanRowAsync(IOBehavior ioBehavior, Row row, CancellationToken cancellationToken)
	{
		ResultSetState bufferState = BufferState;
		if ((bufferState == ResultSetState.None || (uint)(bufferState - 3) <= 1u) ? true : false)
		{
			return null;
		}
		PayloadData payload;
		try
		{
			payload = await Session.ReceiveReplyAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (MySqlException ex)
		{
			ResultSet resultSet = this;
			bufferState = (State = ResultSetState.NoMoreData);
			resultSet.BufferState = bufferState;
			if (ex.ErrorCode == MySqlErrorCode.QueryInterrupted && cancellationToken.IsCancellationRequested)
			{
				throw new OperationCanceledException(ex.Message, ex, cancellationToken);
			}
			if (ex.ErrorCode == MySqlErrorCode.QueryInterrupted && Command.CancellableCommand.IsTimedOut)
			{
				throw MySqlException.CreateForTimeout(ex);
			}
			throw;
		}
		if (payload.HeaderByte == 254)
		{
			if (Session.SupportsDeprecateEof && OkPayload.IsOk(payload.Span, Session.SupportsDeprecateEof))
			{
				OkPayload okPayload = OkPayload.Create(payload.Span, Session.SupportsDeprecateEof, Session.SupportsSessionTrack);
				BufferState = (((okPayload.ServerStatus & ServerStatus.MoreResultsExist) == 0) ? ResultSetState.NoMoreData : ResultSetState.HasMoreData);
				return null;
			}
			if (!Session.SupportsDeprecateEof && EofPayload.IsEof(payload))
			{
				BufferState = (((EofPayload.Create(payload.Span).ServerStatus & ServerStatus.MoreResultsExist) == 0) ? ResultSetState.NoMoreData : ResultSetState.HasMoreData);
				return null;
			}
		}
		if (row == null)
		{
			row = new Row(Command.TryGetPreparedStatements() != null, this);
		}
		row.SetData(payload.Memory);
		m_hasRows = true;
		BufferState = ResultSetState.ReadingRows;
		return row;
	}

	public string GetName(int ordinal)
	{
		if (!HasResultSet)
		{
			throw new InvalidOperationException("There is no current result set.");
		}
		if (ordinal < 0 || ordinal >= ColumnDefinitions.Length)
		{
			throw new IndexOutOfRangeException($"value must be between 0 and {ColumnDefinitions.Length - 1}");
		}
		return ColumnDefinitions[ordinal].Name;
	}

	public string GetDataTypeName(int ordinal)
	{
		if (!HasResultSet)
		{
			throw new InvalidOperationException("There is no current result set.");
		}
		if (ordinal < 0 || ordinal >= ColumnDefinitions.Length)
		{
			throw new IndexOutOfRangeException($"value must be between 0 and {ColumnDefinitions.Length - 1}");
		}
		MySqlDbType columnType = GetColumnType(ordinal);
		if (columnType == MySqlDbType.String)
		{
			return string.Format(CultureInfo.InvariantCulture, "CHAR({0})", ColumnDefinitions[ordinal].ColumnLength / ProtocolUtility.GetBytesPerCharacter(ColumnDefinitions[ordinal].CharacterSet));
		}
		return TypeMapper.Instance.GetColumnTypeMetadata(columnType).SimpleDataTypeName;
	}

	public Type GetFieldType(int ordinal)
	{
		if (!HasResultSet)
		{
			throw new InvalidOperationException("There is no current result set.");
		}
		if (ordinal < 0 || ordinal >= ColumnDefinitions.Length)
		{
			throw new IndexOutOfRangeException($"value must be between 0 and {ColumnDefinitions.Length - 1}");
		}
		Type type = TypeMapper.Instance.GetColumnTypeMetadata(GetColumnType(ordinal)).DbTypeMapping.ClrType;
		if (Connection.AllowZeroDateTime && type == typeof(DateTime))
		{
			type = typeof(MySqlDateTime);
		}
		return type;
	}

	public MySqlDbType GetColumnType(int ordinal)
	{
		return TypeMapper.ConvertToMySqlDbType(ColumnDefinitions[ordinal], Connection.TreatTinyAsBoolean, Connection.GuidFormat);
	}

	public int GetOrdinal(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (!HasResultSet)
		{
			throw new InvalidOperationException("There is no current result set.");
		}
		for (int i = 0; i < ColumnDefinitions.Length; i++)
		{
			if (name.Equals(ColumnDefinitions[i].Name, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		throw new IndexOutOfRangeException("The column name '" + name + "' does not exist in the result set.");
	}

	public Row GetCurrentRow()
	{
		if (State != ResultSetState.ReadingRows)
		{
			throw new InvalidOperationException("Read must be called first.");
		}
		return m_row ?? throw new InvalidOperationException("There is no current row.");
	}
}
