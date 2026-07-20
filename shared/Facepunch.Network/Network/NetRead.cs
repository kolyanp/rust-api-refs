using System;
using System.IO;
using System.Text;
using System.Threading;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;
using UnityEngine.Assertions;

namespace Network;

public class NetRead : Stream, IPooled
{
	private BufferStream stream;

	public int refCount;

	public ulong guid;

	public string ipaddress;

	public Connection connection;

	private const int bufferSize = 4195328;

	private static byte[] byteBuffer = new byte[4195328];

	private static char[] charBuffer = new char[4195328];

	public int Unread => (int)(Length - Position);

	public override bool CanRead => true;

	public override bool CanWrite => false;

	public override bool CanSeek => false;

	public override long Length => stream.Length;

	public override long Position
	{
		get
		{
			return stream.Position;
		}
		set
		{
			stream.Position = (int)value;
		}
	}

	public void AddReference()
	{
		Interlocked.Increment(ref refCount);
	}

	public void RemoveReference()
	{
		if (Interlocked.Decrement(ref refCount) == 0)
		{
			NetRead netRead = this;
			Pool.Free<NetRead>(ref netRead);
		}
	}

	public bool Init(Span<byte> buffer)
	{
		if (buffer.Length > 4194304)
		{
			throw new Exception($"Packet was too large (max is {4194304})");
		}
		stream = Pool.Get<BufferStream>().Initialize(buffer);
		return true;
	}

	public void EnterPool()
	{
		connection = null;
		BufferStream obj = stream;
		if (obj != null)
		{
			obj.Dispose();
		}
		stream = null;
	}

	public void LeavePool()
	{
		refCount = 1;
	}

	public (byte[] Buffer, int Length) GetBuffer()
	{
		ArraySegment<byte> buffer = stream.GetBuffer();
		Assert.IsNotNull<byte[]>(buffer.Array, "buffer.Array != null");
		Assert.IsTrue(buffer.Offset == 0, "buffer.Offset == 0");
		return (Buffer: buffer.Array, Length: buffer.Count);
	}

	public bool Start(ulong guid, string ipaddress, Span<byte> buffer)
	{
		connection = null;
		this.guid = guid;
		this.ipaddress = ipaddress;
		return Init(buffer);
	}

	public bool Start(Connection connection, Span<byte> buffer)
	{
		this.connection = connection;
		guid = connection.guid;
		ipaddress = connection.ipaddress;
		return Init(buffer);
	}

	public bool Start(Connection connection, ulong guid, Span<byte> buffer)
	{
		this.connection = connection;
		this.guid = guid;
		ipaddress = connection.ipaddress;
		return Init(buffer);
	}

	public bool Start(Connection connection, ulong guid, string ipaddress, Span<byte> buffer)
	{
		this.connection = connection;
		this.guid = guid;
		this.ipaddress = ipaddress;
		return Init(buffer);
	}

	public string String(int maxLength = 256, bool variableLength = false)
	{
		return StringInternal(maxLength, allowNewLine: false, variableLength);
	}

	public string StringMultiLine(int maxLength = 2048, bool variableLength = false)
	{
		return StringInternal(maxLength, allowNewLine: true, variableLength);
	}

	private string StringInternal(int maxLength, bool allowNewLine, bool variableLength = false)
	{
		int num = BytesWithSize(byteBuffer, 4195328u, variableLength);
		if (num <= 0)
		{
			return string.Empty;
		}
		int num2 = Encoding.UTF8.GetChars(byteBuffer, 0, num, charBuffer, 0);
		if (num2 > maxLength)
		{
			num2 = maxLength;
		}
		for (int i = 0; i < num2; i++)
		{
			char c = charBuffer[i];
			if (char.IsControl(c) && (!allowNewLine || c != '\n'))
			{
				charBuffer[i] = ' ';
			}
		}
		return new string(charBuffer, 0, num2);
	}

	public string StringRaw(int maxLength = 4195328, bool variableLength = false)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		int num = Math.Min((int)(variableLength ? VarUInt32() : UInt32()), stream.Length);
		if (num <= 0 || num > maxLength)
		{
			return string.Empty;
		}
		RangeHandle range = stream.GetRange(num);
		ArraySegment<byte> segment = ((RangeHandle)(ref range)).GetSegment();
		return Encoding.UTF8.GetString(segment.Array, segment.Offset, segment.Count);
	}

	public bool TemporaryBytesWithSize(out byte[] buffer, out int size)
	{
		buffer = byteBuffer;
		size = 0;
		uint num = UInt32();
		if (num == 0)
		{
			return false;
		}
		if (num > byteBuffer.Length)
		{
			return false;
		}
		size = Read(byteBuffer, 0, (int)num);
		if (size != num)
		{
			return false;
		}
		return true;
	}

	public NetworkableId EntityID()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return new NetworkableId(UInt64());
	}

	public ItemContainerId ItemContainerID()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return new ItemContainerId(UInt64());
	}

	public ItemId ItemID()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return new ItemId(UInt64());
	}

	public uint GroupID()
	{
		return UInt32();
	}

	public int BytesWithSize(byte[] buffer, uint maxLength = uint.MaxValue, bool variableLength = false)
	{
		uint num = (variableLength ? VarUInt32() : UInt32());
		if (num == 0)
		{
			return 0;
		}
		if (num > buffer.Length || num > maxLength)
		{
			return -1;
		}
		if (Read(buffer, 0, (int)num) != num)
		{
			return -1;
		}
		return (int)num;
	}

	public byte[] BytesWithSize(uint maxSize = 10485760u, bool variableLength = false)
	{
		uint num = (variableLength ? VarUInt32() : UInt32());
		if (num == 0)
		{
			return null;
		}
		if (num > maxSize)
		{
			return null;
		}
		byte[] array = new byte[num];
		if (Read(array, 0, (int)num) != num)
		{
			return null;
		}
		return array;
	}

	public ArraySegment<byte> BytesSegmentWithSize(uint maxSize = 4194304u, bool variableLength = false)
	{
		uint num = (variableLength ? VarUInt32() : UInt32());
		if (num == 0)
		{
			return default(ArraySegment<byte>);
		}
		int num2 = (int)Position;
		if (num > maxSize || num2 + num > stream.Length)
		{
			return default(ArraySegment<byte>);
		}
		ArraySegment<byte> result = stream.GetBuffer().Slice(num2, (int)num);
		stream.Skip((int)num);
		return result;
	}

	public override int ReadByte()
	{
		return stream.ReadByte();
	}

	public override void SetLength(long value)
	{
		stream.Length = (int)value;
	}

	public byte PacketID()
	{
		return Read<byte>();
	}

	public byte PeekPacketID()
	{
		return Peek<byte>();
	}

	public bool Bool()
	{
		return UInt8() != 0;
	}

	public bool Bit()
	{
		return UInt8() != 0;
	}

	public byte UInt8()
	{
		return Read<byte>();
	}

	public ushort UInt16()
	{
		return Read<ushort>();
	}

	public uint UInt32()
	{
		return Read<uint>();
	}

	public ulong UInt64()
	{
		return Read<ulong>();
	}

	public sbyte Int8()
	{
		return Read<sbyte>();
	}

	public short Int16()
	{
		return Read<short>();
	}

	public int Int32()
	{
		return Read<int>();
	}

	public long Int64()
	{
		return Read<long>();
	}

	public float Float()
	{
		return Read<float>();
	}

	public double Double()
	{
		return Read<double>();
	}

	public uint VarUInt32()
	{
		return ProtocolParser.ReadUInt32(stream);
	}

	public Vector3 Vector3()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return this.Read<Vector3>();
	}

	public Quaternion Quaternion()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return this.Read<Quaternion>();
	}

	public Ray Ray()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return this.Read<Ray>();
	}

	public Color Color()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return this.Read<Color>();
	}

	public Color32 Color32()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return this.Read<Color32>();
	}

	public T Proto<T>(T proto = null) where T : class, IProto<T>, new()
	{
		if (proto == null)
		{
			proto = Pool.Get<T>();
		}
		((IProto)proto).ReadFromStream(stream, false);
		return proto;
	}

	public T ProtoDelta<T>(T proto) where T : class, IProto<T>, new()
	{
		if (proto == null)
		{
			throw new ArgumentNullException("proto");
		}
		T val = Pool.Get<T>();
		((IProto<T>)proto).CopyTo(val);
		((IProto)val).ReadFromStream(stream, true);
		return val;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (stream.Position + count > stream.Length)
		{
			count = stream.Length - stream.Position;
		}
		RangeHandle range = stream.GetRange(count);
		((RangeHandle)(ref range)).GetSpan().CopyTo(new Span<byte>(buffer, offset, count));
		return count;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public T Read<T>() where T : unmanaged
	{
		return stream.Read<T>();
	}

	public T Peek<T>() where T : unmanaged
	{
		return stream.Peek<T>();
	}

	public override void Flush()
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}

	public override void WriteByte(byte value)
	{
		throw new NotImplementedException();
	}
}
