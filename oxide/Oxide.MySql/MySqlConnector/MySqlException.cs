using System;
using System.Collections;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace MySqlConnector;

[Serializable]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
public sealed class MySqlException : DbException
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private IDictionary m_data;

	public int Number { get; }

	public new MySqlErrorCode ErrorCode { get; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public string SqlState
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
	}

	public bool IsTransient => IsErrorTransient(ErrorCode);

	public override IDictionary Data
	{
		get
		{
			if (m_data == null)
			{
				m_data = base.Data;
				m_data["Server Error Code"] = Number;
				m_data["SqlState"] = SqlState;
			}
			return m_data;
		}
	}

	private MySqlException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		Number = info.GetInt32("Number");
		ErrorCode = (MySqlErrorCode)Number;
		SqlState = info.GetString("SqlState");
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Number", Number);
		info.AddValue("SqlState", SqlState);
	}

	internal MySqlException(string message)
		: this(message, null)
	{
	}

	internal MySqlException(string message, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] Exception innerException)
		: this(MySqlErrorCode.None, null, message, innerException)
	{
	}

	internal MySqlException(MySqlErrorCode errorCode, string message)
		: this(errorCode, null, message, null)
	{
	}

	internal MySqlException(MySqlErrorCode errorCode, string message, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] Exception innerException)
		: this(errorCode, null, message, innerException)
	{
	}

	internal MySqlException(MySqlErrorCode errorCode, string sqlState, string message)
		: this(errorCode, sqlState, message, null)
	{
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	internal MySqlException(MySqlErrorCode errorCode, string sqlState, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] string message, Exception innerException)
		: base(message, innerException)
	{
		ErrorCode = errorCode;
		Number = (int)errorCode;
		SqlState = sqlState;
	}

	internal static MySqlException CreateForTimeout()
	{
		return CreateForTimeout(null);
	}

	internal static MySqlException CreateForTimeout([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] Exception innerException)
	{
		return new MySqlException(MySqlErrorCode.CommandTimeoutExpired, "The Command Timeout expired before the operation completed.", innerException);
	}

	private static bool IsErrorTransient(MySqlErrorCode errorCode)
	{
		switch (errorCode)
		{
		case MySqlErrorCode.ConnectionCountError:
		case MySqlErrorCode.UnableToConnectToHost:
		case MySqlErrorCode.LockWaitTimeout:
		case MySqlErrorCode.LockDeadlock:
		case MySqlErrorCode.XARBDeadlock:
			return true;
		default:
			return false;
		}
	}
}
