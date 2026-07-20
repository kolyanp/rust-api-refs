using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
public sealed class MySqlBulkLoader
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	internal const string SourcePrefix = ":SOURCE:";

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	private static readonly object s_lock = new object();

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	private static readonly Dictionary<string, object> s_sources = new Dictionary<string, object>();

	public string CharacterSet { get; set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public List<string> Columns
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get;
	}

	public MySqlBulkLoaderConflictOption ConflictOption { get; set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public MySqlConnection Connection
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		set;
	}

	public char EscapeCharacter { get; set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public List<string> Expressions
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get;
	}

	public char FieldQuotationCharacter { get; set; }

	public bool FieldQuotationOptional { get; set; }

	public string FieldTerminator { get; set; }

	public string FileName { get; set; }

	public string LinePrefix { get; set; }

	public string LineTerminator { get; set; }

	public bool Local { get; set; }

	public int NumberOfLinesToSkip { get; set; }

	public MySqlBulkLoaderPriority Priority { get; set; }

	public Stream SourceStream
	{
		get
		{
			return Source as Stream;
		}
		set
		{
			Source = value;
		}
	}

	public string TableName { get; set; }

	public int Timeout { get; set; }

	internal object Source { get; set; }

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public MySqlBulkLoader(MySqlConnection connection)
	{
		Connection = connection;
		Local = true;
		Columns = new List<string>();
		Expressions = new List<string>();
	}

	public int Load()
	{
		return LoadAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public Task<int> LoadAsync()
	{
		return LoadAsync(IOBehavior.Asynchronous, CancellationToken.None).AsTask();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public Task<int> LoadAsync(CancellationToken cancellationToken)
	{
		return LoadAsync(IOBehavior.Asynchronous, cancellationToken).AsTask();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	internal async ValueTask<int> LoadAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		if (Connection == null)
		{
			throw new InvalidOperationException("Connection not set");
		}
		if (string.IsNullOrWhiteSpace(TableName))
		{
			throw new InvalidOperationException("TableName is required.");
		}
		if (!string.IsNullOrWhiteSpace(FileName) && Source != null)
		{
			throw new InvalidOperationException("Exactly one of FileName or SourceStream must be set.");
		}
		if (!string.IsNullOrWhiteSpace(FileName))
		{
			if (Local)
			{
				string text = GenerateSourceFileName();
				lock (s_lock)
				{
					s_sources.Add(text, CreateFileStream(FileName));
				}
				FileName = text;
			}
		}
		else
		{
			if (!Local)
			{
				throw new InvalidOperationException("Local must be true to use SourceStream, SourceDataTable, or SourceDataReader.");
			}
			FileName = GenerateSourceFileName();
			lock (s_lock)
			{
				s_sources.Add(FileName, Source);
			}
		}
		bool closeConnection = false;
		if (Connection.State != ConnectionState.Open)
		{
			closeConnection = true;
			Connection.Open();
		}
		bool closeStream = SourceStream != null;
		try
		{
			if (Local && !Connection.AllowLoadLocalInfile)
			{
				throw new NotSupportedException("To use MySqlBulkLoader.Local=true, set AllowLoadLocalInfile=true in the connection string. See https://fl.vu/mysql-load-data");
			}
			using MySqlCommand cmd = new MySqlCommand(CreateSql(), Connection, Connection.CurrentTransaction)
			{
				AllowUserVariables = true,
				CommandTimeout = Timeout
			};
			int result = await cmd.ExecuteNonQueryAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			closeStream = false;
			return result;
		}
		finally
		{
			if (closeStream && TryGetAndRemoveSource(FileName, out var source))
			{
				((IDisposable)source).Dispose();
			}
			if (closeConnection)
			{
				Connection.Close();
			}
		}
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	private string CreateSql()
	{
		StringBuilder stringBuilder = new StringBuilder("LOAD DATA ");
		StringBuilder stringBuilder2 = stringBuilder;
		stringBuilder2.Append(Priority switch
		{
			MySqlBulkLoaderPriority.Low => "LOW_PRIORITY ", 
			MySqlBulkLoaderPriority.Concurrent => "CONCURRENT ", 
			_ => "", 
		});
		if (Local)
		{
			stringBuilder.Append("LOCAL ");
		}
		stringBuilder.Append("INFILE '" + MySqlHelper.EscapeString(FileName) + "' ");
		stringBuilder2 = stringBuilder;
		stringBuilder2.Append(ConflictOption switch
		{
			MySqlBulkLoaderConflictOption.Replace => "REPLACE ", 
			MySqlBulkLoaderConflictOption.Ignore => "IGNORE ", 
			_ => "", 
		});
		stringBuilder.Append("INTO TABLE " + TableName + " ");
		if (CharacterSet != null)
		{
			stringBuilder.Append("CHARACTER SET " + CharacterSet + " ");
		}
		string text = ((FieldTerminator == null) ? "" : ("TERMINATED BY '" + MySqlHelper.EscapeString(FieldTerminator) + "' "));
		string text2 = ((FieldQuotationCharacter == '\0') ? "" : ((FieldQuotationOptional ? "OPTIONALLY " : "") + "ENCLOSED BY '" + MySqlHelper.EscapeString(FieldQuotationCharacter.ToString()) + "' "));
		string text3 = ((EscapeCharacter == '\0') ? "" : ("ESCAPED BY '" + MySqlHelper.EscapeString(EscapeCharacter.ToString()) + "' "));
		if (text.Length + text2.Length + text3.Length > 0)
		{
			stringBuilder.Append("FIELDS " + text + text2 + text3);
		}
		string text4 = ((LineTerminator == null) ? "" : ("TERMINATED BY '" + MySqlHelper.EscapeString(LineTerminator) + "' "));
		string text5 = ((LinePrefix == null) ? "" : ("STARTING BY '" + MySqlHelper.EscapeString(LinePrefix) + "' "));
		if (text4.Length + text5.Length > 0)
		{
			stringBuilder.Append("LINES " + text4 + text5);
		}
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "IGNORE {0} LINES ", NumberOfLinesToSkip);
		if (Columns.Count > 0)
		{
			stringBuilder.Append("(" + string.Join(",", Columns) + ") ");
		}
		if (Expressions.Count > 0)
		{
			stringBuilder.Append("SET " + string.Join(",", Expressions));
		}
		stringBuilder.Append(';');
		return stringBuilder.ToString();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	private static FileStream CreateFileStream(string fileName)
	{
		try
		{
			return File.OpenRead(fileName);
		}
		catch (Exception innerException)
		{
			throw new MySqlException("Could not access file \"" + fileName + "\"", innerException);
		}
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	internal static object GetAndRemoveSource(string sourceKey)
	{
		lock (s_lock)
		{
			object result = s_sources[sourceKey];
			s_sources.Remove(sourceKey);
			return result;
		}
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	internal static bool TryGetAndRemoveSource(string sourceKey, [_003Ce940fe46_002D60b5_002D4fb7_002D817f_002D6effabbc4d82_003ENotNullWhen(true)][_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] out object source)
	{
		lock (s_lock)
		{
			if (s_sources.TryGetValue(sourceKey, out source))
			{
				return s_sources.Remove(sourceKey);
			}
		}
		return false;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	private static string GenerateSourceFileName()
	{
		return ":SOURCE:" + Guid.NewGuid().ToString("N");
	}
}
