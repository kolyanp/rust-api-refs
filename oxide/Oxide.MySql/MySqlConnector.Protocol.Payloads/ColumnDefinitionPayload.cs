using System.Runtime.CompilerServices;
using System.Text;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector.Protocol.Payloads;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class ColumnDefinitionPayload
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
	private ResizableArraySegment<byte> m_originalData;

	private bool m_readNames;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private string m_name;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private string m_schemaName;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private string m_catalogName;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private string m_table;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private string m_physicalTable;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private string m_physicalName;

	public string Name
	{
		get
		{
			if (!m_readNames)
			{
				ReadNames();
			}
			return m_name;
		}
	}

	public CharacterSet CharacterSet { get; private set; }

	public uint ColumnLength { get; private set; }

	public ColumnType ColumnType { get; private set; }

	public ColumnFlags ColumnFlags { get; private set; }

	public string SchemaName
	{
		get
		{
			if (!m_readNames)
			{
				ReadNames();
			}
			return m_schemaName;
		}
	}

	public string CatalogName
	{
		get
		{
			if (!m_readNames)
			{
				ReadNames();
			}
			return m_catalogName;
		}
	}

	public string Table
	{
		get
		{
			if (!m_readNames)
			{
				ReadNames();
			}
			return m_table;
		}
	}

	public string PhysicalTable
	{
		get
		{
			if (!m_readNames)
			{
				ReadNames();
			}
			return m_physicalTable;
		}
	}

	public string PhysicalName
	{
		get
		{
			if (!m_readNames)
			{
				ReadNames();
			}
			return m_physicalName;
		}
	}

	public byte Decimals { get; private set; }

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public static void Initialize([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] ref ColumnDefinitionPayload payload, ResizableArraySegment<byte> arraySegment)
	{
		if (payload == null)
		{
			payload = new ColumnDefinitionPayload();
		}
		payload.Initialize(arraySegment);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	private void Initialize(ResizableArraySegment<byte> originalData)
	{
		m_originalData = originalData;
		ByteArrayReader reader = new ByteArrayReader(originalData);
		SkipLengthEncodedByteString(ref reader);
		SkipLengthEncodedByteString(ref reader);
		SkipLengthEncodedByteString(ref reader);
		SkipLengthEncodedByteString(ref reader);
		SkipLengthEncodedByteString(ref reader);
		SkipLengthEncodedByteString(ref reader);
		reader.ReadByte(12);
		CharacterSet = (CharacterSet)reader.ReadUInt16();
		ColumnLength = reader.ReadUInt32();
		ColumnType = (ColumnType)reader.ReadByte();
		ColumnFlags = (ColumnFlags)reader.ReadUInt16();
		Decimals = reader.ReadByte();
		reader.ReadByte(0);
		reader.ReadByte(0);
		if (m_readNames)
		{
			m_catalogName = null;
			m_schemaName = null;
			m_table = null;
			m_physicalTable = null;
			m_name = null;
			m_physicalName = null;
			m_readNames = false;
		}
	}

	private static void SkipLengthEncodedByteString(ref ByteArrayReader reader)
	{
		int num = checked((int)reader.ReadLengthEncodedInteger());
		reader.Offset += num;
	}

	private ColumnDefinitionPayload()
	{
	}

	private void ReadNames()
	{
		ByteArrayReader byteArrayReader = new ByteArrayReader(m_originalData);
		m_catalogName = Utility.GetString(Encoding.UTF8, byteArrayReader.ReadLengthEncodedByteString());
		m_schemaName = Utility.GetString(Encoding.UTF8, byteArrayReader.ReadLengthEncodedByteString());
		m_table = Utility.GetString(Encoding.UTF8, byteArrayReader.ReadLengthEncodedByteString());
		m_physicalTable = Utility.GetString(Encoding.UTF8, byteArrayReader.ReadLengthEncodedByteString());
		m_name = Utility.GetString(Encoding.UTF8, byteArrayReader.ReadLengthEncodedByteString());
		m_physicalName = Utility.GetString(Encoding.UTF8, byteArrayReader.ReadLengthEncodedByteString());
		m_readNames = true;
	}
}
