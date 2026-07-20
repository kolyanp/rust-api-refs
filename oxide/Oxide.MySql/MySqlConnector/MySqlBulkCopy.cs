using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySqlConnector.Core;
using MySqlConnector.Logging;
using MySqlConnector.Protocol;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
public sealed class MySqlBulkCopy
{
	private static readonly char[] s_specialCharacters = new char[3] { '\t', '\\', '\n' };

	private static readonly UTF8Encoding s_utf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	private readonly MySqlConnection m_connection;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private readonly MySqlTransaction m_transaction;

	private readonly ILogger m_logger;

	private int m_rowsCopied;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private IValuesEnumerator m_valuesEnumerator;

	private bool m_wasAborted;

	public MySqlBulkLoaderConflictOption ConflictOption { get; set; }

	public int BulkCopyTimeout { get; set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public string DestinationTableName
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		set;
	}

	public int NotifyAfter { get; set; }

	public List<MySqlBulkCopyColumnMapping> ColumnMappings { get; }

	[Obsolete("Use the MySqlBulkCopyResult.RowsInserted property returned by WriteToServer.")]
	public int RowsCopied => m_rowsCopied;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[method: _003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public event MySqlRowsCopiedEventHandler MySqlRowsCopied;

	public MySqlBulkCopy(MySqlConnection connection, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] MySqlTransaction transaction = null)
	{
		if (connection == null)
		{
			throw new ArgumentNullException("connection");
		}
		m_connection = connection;
		m_transaction = transaction;
		m_logger = m_connection.LoggingConfiguration.BulkCopyLogger;
		ColumnMappings = new List<MySqlBulkCopyColumnMapping>();
	}

	public MySqlBulkCopyResult WriteToServer(DataTable dataTable)
	{
		if (dataTable == null)
		{
			throw new ArgumentNullException("dataTable");
		}
		m_valuesEnumerator = DataRowsValuesEnumerator.Create(dataTable);
		return WriteToServerAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public async ValueTask<MySqlBulkCopyResult> WriteToServerAsync(DataTable dataTable, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (dataTable == null)
		{
			throw new ArgumentNullException("dataTable");
		}
		m_valuesEnumerator = DataRowsValuesEnumerator.Create(dataTable);
		return await WriteToServerAsync(IOBehavior.Asynchronous, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public MySqlBulkCopyResult WriteToServer(IEnumerable<DataRow> dataRows, int columnCount)
	{
		if (dataRows == null)
		{
			throw new ArgumentNullException("dataRows");
		}
		m_valuesEnumerator = new DataRowsValuesEnumerator(dataRows, columnCount);
		return WriteToServerAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public async ValueTask<MySqlBulkCopyResult> WriteToServerAsync(IEnumerable<DataRow> dataRows, int columnCount, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (dataRows == null)
		{
			throw new ArgumentNullException("dataRows");
		}
		m_valuesEnumerator = new DataRowsValuesEnumerator(dataRows, columnCount);
		return await WriteToServerAsync(IOBehavior.Asynchronous, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public MySqlBulkCopyResult WriteToServer(IDataReader dataReader)
	{
		if (dataReader == null)
		{
			throw new ArgumentNullException("dataReader");
		}
		m_valuesEnumerator = DataReaderValuesEnumerator.Create(dataReader);
		return WriteToServerAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public async ValueTask<MySqlBulkCopyResult> WriteToServerAsync(IDataReader dataReader, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (dataReader == null)
		{
			throw new ArgumentNullException("dataReader");
		}
		m_valuesEnumerator = DataReaderValuesEnumerator.Create(dataReader);
		return await WriteToServerAsync(IOBehavior.Asynchronous, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	private async ValueTask<MySqlBulkCopyResult> WriteToServerAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		string tableName = DestinationTableName ?? throw new InvalidOperationException("DestinationTableName must be set before calling WriteToServer");
		m_wasAborted = false;
		Log.StartingBulkCopy(m_logger, tableName);
		MySqlBulkLoader bulkLoader = new MySqlBulkLoader(m_connection)
		{
			CharacterSet = "utf8mb4",
			EscapeCharacter = '\\',
			FieldQuotationCharacter = '\0',
			FieldTerminator = "\t",
			LinePrefix = null,
			LineTerminator = "\n",
			Local = true,
			NumberOfLinesToSkip = 0,
			Source = this,
			TableName = tableName,
			Timeout = BulkCopyTimeout,
			ConflictOption = ConflictOption
		};
		bool closeConnection = false;
		if (m_connection.State != ConnectionState.Open)
		{
			m_connection.Open();
			closeConnection = true;
		}
		List<MySqlBulkCopyColumnMapping> columnMappings = new List<MySqlBulkCopyColumnMapping>(ColumnMappings);
		bool addDefaultMappings = columnMappings.Count == 0;
		using (MySqlCommand cmd = new MySqlCommand("select * from " + tableName + ";", m_connection, m_transaction))
		{
			using MySqlDataReader mySqlDataReader = await cmd.ExecuteReaderAsync(CommandBehavior.SchemaOnly, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			ReadOnlyCollection<DbColumn> columnSchema = mySqlDataReader.GetColumnSchema();
			for (int i = 0; i < Math.Min(m_valuesEnumerator.FieldCount, columnSchema.Count); i++)
			{
				string name = mySqlDataReader.GetName(i);
				if (columnSchema[i].DataTypeName == "BIT")
				{
					AddColumnMapping(m_logger, columnMappings, addDefaultMappings, i, name, $"@`\ue002\bcol{i}`", "%COL% = CAST(%VAR% AS UNSIGNED)");
					continue;
				}
				if (columnSchema[i].DataTypeName == "YEAR")
				{
					throw new NotSupportedException("'YEAR' columns are not supported by MySqlBulkLoader.");
				}
				Type dataType = columnSchema[i].DataType;
				bool flag = dataType == typeof(byte[]);
				if (!flag)
				{
					bool flag2 = dataType == typeof(Guid);
					if (flag2)
					{
						MySqlGuidFormat guidFormat = m_connection.GuidFormat;
						bool flag3 = (uint)(guidFormat - 4) <= 2u;
						flag2 = flag3;
					}
					flag = flag2;
				}
				if (flag)
				{
					AddColumnMapping(m_logger, columnMappings, addDefaultMappings, i, name, $"@`\ue002\bcol{i}`", "%COL% = UNHEX(%VAR%)");
				}
				else if (addDefaultMappings)
				{
					Log.AddingDefaultColumnMapping(m_logger, i, name);
					columnMappings.Add(new MySqlBulkCopyColumnMapping(i, name));
				}
			}
		}
		int j;
		for (j = 0; j < m_valuesEnumerator.FieldCount; j++)
		{
			MySqlBulkCopyColumnMapping mySqlBulkCopyColumnMapping = columnMappings.FirstOrDefault([_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)] (MySqlBulkCopyColumnMapping x) => x.SourceOrdinal == j);
			if (mySqlBulkCopyColumnMapping == null)
			{
				Log.IgnoringColumn(m_logger, j);
				bulkLoader.Columns.Add("@`\ue002\bignore`");
				continue;
			}
			if (mySqlBulkCopyColumnMapping.DestinationColumn.Length == 0)
			{
				throw new InvalidOperationException($"MySqlBulkCopyColumnMapping.DestinationName is not set for SourceOrdinal {mySqlBulkCopyColumnMapping.SourceOrdinal}");
			}
			if (mySqlBulkCopyColumnMapping.DestinationColumn[0] == '@' && mySqlBulkCopyColumnMapping.Expression != null)
			{
				bulkLoader.Columns.Add(mySqlBulkCopyColumnMapping.DestinationColumn);
			}
			else
			{
				bulkLoader.Columns.Add(QuoteIdentifier(mySqlBulkCopyColumnMapping.DestinationColumn));
			}
			if (mySqlBulkCopyColumnMapping.Expression != null)
			{
				bulkLoader.Expressions.Add(mySqlBulkCopyColumnMapping.Expression);
			}
		}
		foreach (MySqlBulkCopyColumnMapping item in columnMappings)
		{
			if (item.SourceOrdinal < 0 || item.SourceOrdinal >= m_valuesEnumerator.FieldCount)
			{
				throw new InvalidOperationException($"SourceOrdinal {item.SourceOrdinal} is an invalid value");
			}
		}
		List<MySqlError> errors = new List<MySqlError>();
		MySqlInfoMessageEventHandler infoMessageHandler = delegate(object s, MySqlInfoMessageEventArgs e)
		{
			errors.AddRange(e.Errors);
		};
		m_connection.InfoMessage += infoMessageHandler;
		int num;
		try
		{
			num = await bulkLoader.LoadAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			m_connection.InfoMessage -= infoMessageHandler;
		}
		if (closeConnection)
		{
			m_connection.Close();
		}
		Log.FinishedBulkCopy(m_logger, tableName);
		if (!m_wasAborted && num != m_rowsCopied && ConflictOption == MySqlBulkLoaderConflictOption.None)
		{
			Log.BulkCopyFailed(m_logger, tableName, m_rowsCopied, num);
			throw new MySqlException(MySqlErrorCode.BulkCopyFailed, string.Format("{0} row{1} copied to {2} but only {3} {4} inserted.", new object[5]
			{
				m_rowsCopied,
				(m_rowsCopied == 1) ? " was" : "s were",
				tableName,
				num,
				(num == 1) ? "was" : "were"
			}));
		}
		return new MySqlBulkCopyResult(errors, num);
		static void AddColumnMapping(ILogger logger, List<MySqlBulkCopyColumnMapping> list, bool flag4, int destinationOrdinal, string destinationColumn, string variableName, string expression)
		{
			expression = expression.Replace("%COL%", "`" + destinationColumn + "`").Replace("%VAR%", variableName);
			MySqlBulkCopyColumnMapping mySqlBulkCopyColumnMapping2 = list.FirstOrDefault([_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)] (MySqlBulkCopyColumnMapping x) => destinationColumn.Equals(x.DestinationColumn, StringComparison.OrdinalIgnoreCase));
			if (mySqlBulkCopyColumnMapping2 != null)
			{
				if (mySqlBulkCopyColumnMapping2.Expression != null)
				{
					Log.ColumnMappingAlreadyHasExpression(logger, mySqlBulkCopyColumnMapping2.SourceOrdinal, destinationColumn, mySqlBulkCopyColumnMapping2.Expression);
				}
				else
				{
					Log.SettingExpressionToMapColumn(logger, mySqlBulkCopyColumnMapping2.SourceOrdinal, destinationColumn, expression);
					list.Remove(mySqlBulkCopyColumnMapping2);
					list.Add(new MySqlBulkCopyColumnMapping(mySqlBulkCopyColumnMapping2.SourceOrdinal, variableName, expression));
				}
			}
			else if (flag4)
			{
				Log.AddingDefaultColumnMapping(logger, destinationOrdinal, destinationColumn);
				list.Add(new MySqlBulkCopyColumnMapping(destinationOrdinal, variableName, expression));
			}
		}
		static string QuoteIdentifier(string identifier)
		{
			return "`" + identifier.Replace("`", "``") + "`";
		}
	}

	internal async Task SendDataReaderAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		byte[] buffer = ArrayPool<byte>.Shared.Rent(1048576);
		int outputIndex = 0;
		m_rowsCopied = 0;
		MySqlRowsCopiedEventArgs eventArgs = null;
		if (NotifyAfter > 0 && this.MySqlRowsCopied != null)
		{
			eventArgs = new MySqlRowsCopiedEventArgs();
		}
		try
		{
			object[] values = new object[m_valuesEnumerator.FieldCount];
			Encoder utf8Encoder = null;
			while ((ioBehavior != IOBehavior.Asynchronous) ? m_valuesEnumerator.MoveNext() : (await m_valuesEnumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false)))
			{
				m_valuesEnumerator.GetValues(values);
				for (int valueIndex = 0; valueIndex < values.Length; valueIndex++)
				{
					if (valueIndex > 0)
					{
						buffer[outputIndex++] = 9;
					}
					int inputIndex = 0;
					int bytesWritten = 0;
					while (true)
					{
						if (outputIndex < 1048575)
						{
							MySqlConnection connection = m_connection;
							object value = values[valueIndex];
							Span<byte> span = MemoryExtensions.AsSpan(buffer, 0, 1048575);
							int num = outputIndex;
							if (WriteValue(connection, value, ref inputIndex, ref utf8Encoder, span.Slice(num, span.Length - num), out bytesWritten))
							{
								break;
							}
						}
						PayloadData payload = new PayloadData(new ArraySegment<byte>(buffer, 0, outputIndex + bytesWritten));
						await m_connection.Session.SendReplyAsync(payload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						outputIndex = 0;
						bytesWritten = 0;
					}
					outputIndex += bytesWritten;
				}
				buffer[outputIndex++] = 10;
				m_rowsCopied++;
				if (eventArgs != null && m_rowsCopied % NotifyAfter == 0)
				{
					eventArgs.RowsCopied = m_rowsCopied;
					this.MySqlRowsCopied(this, eventArgs);
					if (eventArgs.Abort)
					{
						break;
					}
				}
			}
			if (outputIndex != 0 && ((!(eventArgs?.Abort)) ?? true))
			{
				PayloadData payload2 = new PayloadData(new ArraySegment<byte>(buffer, 0, outputIndex));
				await m_connection.Session.SendReplyAsync(payload2, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
			m_wasAborted = eventArgs?.Abort ?? false;
		}
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
		static bool WriteBytes(ReadOnlySpan<byte> readOnlySpan2, ref int reference2, Span<byte> output, out int reference)
		{
			ReadOnlySpan<byte> readOnlySpan = "0123456789ABCDEF"u8;
			reference = 0;
			while (reference2 < readOnlySpan2.Length && output.Length > 2)
			{
				byte b = readOnlySpan2[reference2];
				output[0] = readOnlySpan[(b >> 4) & 0xF];
				output[1] = readOnlySpan[b & 0xF];
				output = output.Slice(2, output.Length - 2);
				reference += 2;
				reference2++;
			}
			return reference2 == readOnlySpan2.Length;
		}
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
		static bool WriteString([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] string value2, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] ref Encoder utf8Encoder2, Span<byte> output, out int reference)
		{
			int inputIndex2 = 0;
			if (WriteSubstring(value2, ref inputIndex2, ref utf8Encoder2, output, out reference))
			{
				return true;
			}
			reference = 0;
			return false;
		}
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
		static bool WriteSubstring([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] string text, ref int reference2, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] ref Encoder reference4, Span<byte> output, out int reference)
		{
			reference = 0;
			while (reference2 < text.Length)
			{
				if (Array.IndexOf(s_specialCharacters, text[reference2]) != -1)
				{
					if (output.Length <= 2)
					{
						return false;
					}
					output[0] = 92;
					output[1] = (byte)text[reference2];
					ref Span<byte> reference3 = ref output;
					output = reference3.Slice(2, reference3.Length - 2);
					reference += 2;
					reference2++;
				}
				else
				{
					int num2 = text.IndexOfAny(s_specialCharacters, reference2);
					if (num2 == -1)
					{
						num2 = text.Length;
					}
					if (reference4 == null)
					{
						reference4 = s_utf8Encoding.GetEncoder();
					}
					if (output.Length < 4 && Utility.GetByteCount(reference4, MemoryExtensions.AsSpan(text, reference2, Math.Min(2, num2 - reference2)), flush: false) > output.Length)
					{
						return false;
					}
					reference4.Convert(MemoryExtensions.AsSpan(text, reference2, num2 - reference2), output, num2 == text.Length, out var charsUsed, out var bytesUsed, out var completed);
					reference += bytesUsed;
					ref Span<byte> reference3 = ref output;
					int num3 = bytesUsed;
					output = reference3.Slice(num3, reference3.Length - num3);
					reference2 += charsUsed;
					if (!completed)
					{
						return false;
					}
				}
			}
			return true;
		}
		static bool WriteValue(MySqlConnection mySqlConnection, object obj, ref int inputIndex2, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] ref Encoder utf8Encoder2, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] Span<byte> output, out int reference)
		{
			if (output.Length == 0)
			{
				reference = 0;
				return false;
			}
			if (obj == null || obj == DBNull.Value)
			{
				ReadOnlySpan<byte> readOnlySpan = "\\N"u8;
				if (output.Length < readOnlySpan.Length)
				{
					reference = 0;
					return false;
				}
				readOnlySpan.CopyTo(output);
				reference = readOnlySpan.Length;
				return true;
			}
			if (obj is string value2)
			{
				return WriteSubstring(value2, ref inputIndex2, ref utf8Encoder2, output, out reference);
			}
			if (obj is char c)
			{
				return WriteString(c.ToString(), ref utf8Encoder2, output, out reference);
			}
			if (obj is byte value3)
			{
				return Utf8Formatter.TryFormat(value3, output, out reference);
			}
			if (obj is sbyte value4)
			{
				return Utf8Formatter.TryFormat(value4, output, out reference);
			}
			if (obj is short value5)
			{
				return Utf8Formatter.TryFormat(value5, output, out reference);
			}
			if (obj is ushort value6)
			{
				return Utf8Formatter.TryFormat(value6, output, out reference);
			}
			if (obj is int value7)
			{
				return Utf8Formatter.TryFormat(value7, output, out reference);
			}
			if (obj is uint value8)
			{
				return Utf8Formatter.TryFormat(value8, output, out reference);
			}
			if (obj is long value9)
			{
				return Utf8Formatter.TryFormat(value9, output, out reference);
			}
			if (obj is ulong value10)
			{
				return Utf8Formatter.TryFormat(value10, output, out reference);
			}
			if (obj is decimal value11)
			{
				return Utf8Formatter.TryFormat(value11, output, out reference);
			}
			if ((obj is byte[] || obj is ReadOnlyMemory<byte> || obj is Memory<byte> || obj is ArraySegment<byte> || obj is MySqlGeometry) ? true : false)
			{
				ReadOnlySpan<byte> value12 = ((obj is byte[] array) ? ((ReadOnlySpan<byte>)MemoryExtensions.AsSpan(array)) : ((obj is ArraySegment<byte> segment) ? ((ReadOnlySpan<byte>)MemoryExtensions.AsSpan(segment)) : ((obj is Memory<byte> memory) ? ((ReadOnlySpan<byte>)memory.Span) : ((!(obj is MySqlGeometry mySqlGeometry)) ? ((ReadOnlyMemory<byte>)obj).Span : mySqlGeometry.ValueSpan))));
				return WriteBytes(value12, ref inputIndex2, output, out reference);
			}
			if (obj is bool flag)
			{
				if (output.Length < 1)
				{
					reference = 0;
					return false;
				}
				output[0] = (byte)(flag ? 49 : 48);
				reference = 1;
				return true;
			}
			if (obj is float num2)
			{
				return WriteString(num2.ToString("R", CultureInfo.InvariantCulture), ref utf8Encoder2, output, out reference);
			}
			if (obj is double num3)
			{
				return WriteString(num3.ToString("R", CultureInfo.InvariantCulture), ref utf8Encoder2, output, out reference);
			}
			if (obj is MySqlDateTime mySqlDateTime)
			{
				if (mySqlDateTime.IsValidDateTime)
				{
					return WriteString(mySqlDateTime.GetDateTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'ffffff", CultureInfo.InvariantCulture), ref utf8Encoder2, output, out reference);
				}
				return WriteString("0000-00-00", ref utf8Encoder2, output, out reference);
			}
			if (obj is DateTime dateTime)
			{
				if (mySqlConnection.DateTimeKind == DateTimeKind.Utc && dateTime.Kind == DateTimeKind.Local)
				{
					throw new MySqlException("DateTime.Kind must not be Local when DateTimeKind setting is Utc");
				}
				if (mySqlConnection.DateTimeKind == DateTimeKind.Local && dateTime.Kind == DateTimeKind.Utc)
				{
					throw new MySqlException("DateTime.Kind must not be Utc when DateTimeKind setting is Local");
				}
				return WriteString(dateTime.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'ffffff", CultureInfo.InvariantCulture), ref utf8Encoder2, output, out reference);
			}
			if (obj is DateTimeOffset { UtcDateTime: var utcDateTime })
			{
				return WriteString(utcDateTime.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'ffffff", CultureInfo.InvariantCulture), ref utf8Encoder2, output, out reference);
			}
			if (obj is TimeSpan timeSpan)
			{
				bool flag2 = false;
				if (timeSpan.Ticks < 0)
				{
					flag2 = true;
					timeSpan = TimeSpan.FromTicks(-timeSpan.Ticks);
				}
				return WriteString(FormattableString.Invariant(FormattableStringFactory.Create("{0}{1}:{2:mm':'ss'.'ffffff}", flag2 ? "-" : "", timeSpan.Days * 24 + timeSpan.Hours, timeSpan)), ref utf8Encoder2, output, out reference);
			}
			if (obj is Guid value13)
			{
				MySqlGuidFormat guidFormat = mySqlConnection.GuidFormat;
				if ((uint)(guidFormat - 4) <= 2u)
				{
					byte[] array2 = value13.ToByteArray();
					if (mySqlConnection.GuidFormat != MySqlGuidFormat.LittleEndianBinary16)
					{
						Utility.SwapBytes(array2, 0, 3);
						Utility.SwapBytes(array2, 1, 2);
						Utility.SwapBytes(array2, 4, 5);
						Utility.SwapBytes(array2, 6, 7);
						if (mySqlConnection.GuidFormat == MySqlGuidFormat.TimeSwapBinary16)
						{
							Utility.SwapBytes(array2, 0, 4);
							Utility.SwapBytes(array2, 1, 5);
							Utility.SwapBytes(array2, 2, 6);
							Utility.SwapBytes(array2, 3, 7);
							Utility.SwapBytes(array2, 0, 2);
							Utility.SwapBytes(array2, 1, 3);
						}
					}
					return WriteBytes(array2, ref inputIndex2, output, out reference);
				}
				bool flag3 = mySqlConnection.GuidFormat == MySqlGuidFormat.Char32;
				return Utf8Formatter.TryFormat(value13, output, out reference, flag3 ? 'N' : 'D');
			}
			if (obj is Enum obj2)
			{
				return WriteString(obj2.ToString("d"), ref utf8Encoder2, output, out reference);
			}
			if (obj is BigInteger bigInteger)
			{
				return WriteString(bigInteger.ToString(CultureInfo.InvariantCulture), ref utf8Encoder2, output, out reference);
			}
			if (obj is MySqlDecimal mySqlDecimal)
			{
				return WriteString(mySqlDecimal.ToString(), ref utf8Encoder2, output, out reference);
			}
			throw new NotSupportedException($"Type {obj.GetType().Name} not currently supported. Value: {obj}");
		}
	}
}
