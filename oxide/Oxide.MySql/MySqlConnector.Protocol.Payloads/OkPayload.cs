using System;
using System.Runtime.CompilerServices;
using System.Text;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector.Protocol.Payloads;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
internal sealed class OkPayload
{
	public const byte Signature = 0;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	private static readonly OkPayload s_autoCommitOk = new OkPayload(0uL, 0uL, ServerStatus.AutoCommit, 0, null, null);

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	private static readonly OkPayload s_autoCommitSessionStateChangedOk = new OkPayload(0uL, 0uL, ServerStatus.AutoCommit | ServerStatus.SessionStateChanged, 0, null, null);

	public ulong AffectedRowCount { get; }

	public ulong LastInsertId { get; }

	public ServerStatus ServerStatus { get; }

	public int WarningCount { get; }

	public string StatusInfo { get; }

	public string NewSchema { get; }

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public static bool IsOk(ReadOnlySpan<byte> span, bool deprecateEof)
	{
		if (span.Length > 0)
		{
			if (span.Length <= 6 || span[0] != 0)
			{
				if (deprecateEof && span.Length < 16777215)
				{
					return span[0] == 254;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public static OkPayload Create(ReadOnlySpan<byte> span, bool deprecateEof, bool clientSessionTrack)
	{
		return Read(span, deprecateEof, clientSessionTrack, createPayload: true);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public static void Verify(ReadOnlySpan<byte> span, bool deprecateEof, bool clientSessionTrack)
	{
		Read(span, deprecateEof, clientSessionTrack, createPayload: false);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private static OkPayload Read(ReadOnlySpan<byte> span, bool deprecateEof, bool clientSessionTrack, bool createPayload)
	{
		ByteArrayReader byteArrayReader = new ByteArrayReader(span);
		byte b = byteArrayReader.ReadByte();
		if (b != 0 && (!deprecateEof || b != 254))
		{
			throw new FormatException($"Expected to read 0x00 or 0xFE but got 0x{b:X2}");
		}
		ulong num = byteArrayReader.ReadLengthEncodedInteger();
		ulong num2 = byteArrayReader.ReadLengthEncodedInteger();
		ServerStatus serverStatus = (ServerStatus)byteArrayReader.ReadUInt16();
		int num3 = byteArrayReader.ReadUInt16();
		string text = null;
		ReadOnlySpan<byte> span2;
		if (clientSessionTrack)
		{
			if (byteArrayReader.BytesRemaining > 0)
			{
				span2 = byteArrayReader.ReadLengthEncodedByteString();
				if ((serverStatus & ServerStatus.SessionStateChanged) == ServerStatus.SessionStateChanged && byteArrayReader.BytesRemaining > 0)
				{
					int num4 = checked((int)byteArrayReader.ReadLengthEncodedInteger());
					int num5 = byteArrayReader.Offset + num4;
					while (byteArrayReader.Offset < num5)
					{
						byte num6 = byteArrayReader.ReadByte();
						int num7 = (int)byteArrayReader.ReadLengthEncodedInteger();
						if (num6 == 1)
						{
							text = Utility.GetString(Encoding.UTF8, byteArrayReader.ReadLengthEncodedByteString());
						}
						else
						{
							byteArrayReader.Offset += num7;
						}
					}
				}
			}
			else
			{
				span2 = default(ReadOnlySpan<byte>);
			}
		}
		else
		{
			span2 = byteArrayReader.ReadByteString(byteArrayReader.BytesRemaining);
			if (span2.Length != 0 && span2[0] == span2.Length - 1)
			{
				span2 = span2.Slice(1, span2.Length - 1);
			}
		}
		if (createPayload)
		{
			string text2 = ((span2.Length == 0) ? null : Utility.GetString(Encoding.UTF8, span2));
			if (num == 0L && num2 == 0L && num3 == 0 && text2 == null && text == null)
			{
				switch (serverStatus)
				{
				case ServerStatus.AutoCommit:
					return s_autoCommitOk;
				case ServerStatus.AutoCommit | ServerStatus.SessionStateChanged:
					return s_autoCommitSessionStateChangedOk;
				}
			}
			return new OkPayload(num, num2, serverStatus, num3, text2, text);
		}
		return null;
	}

	private OkPayload(ulong affectedRowCount, ulong lastInsertId, ServerStatus serverStatus, int warningCount, string statusInfo, string newSchema)
	{
		AffectedRowCount = affectedRowCount;
		LastInsertId = lastInsertId;
		ServerStatus = serverStatus;
		WarningCount = warningCount;
		StatusInfo = statusInfo;
		NewSchema = newSchema;
	}
}
