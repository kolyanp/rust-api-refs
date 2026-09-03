using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;
using UnityEngine.Assertions;

namespace Network;

public class NetWrite : Stream, IPooled
{
	private static MemoryStream stringBuffer = new MemoryStream();

	private BaseNetwork peer;

	private BufferStream stream;

	public int refCount = 1;

	public SendMethod method;

	public sbyte channel;

	public long serverTicks;

	public ulong profilerEntityId;

	public uint profilerPrefabId;

	public uint profilerAux;

	public ushort profilerInfoId;

	public bool profilerAuxIsStringId;

	public Priority priority;

	public List<Connection> connections = new List<Connection>();

	public override bool CanSeek => false;

	public override bool CanRead => false;

	public override bool CanWrite => true;

	public override long Length => stream.Position;

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
			NetWrite netWrite = this;
			Pool.Free<NetWrite>(ref netWrite);
		}
	}

	public void EnterPool()
	{
		peer = null;
		connections.Clear();
		BufferStream obj = stream;
		if (obj != null)
		{
			obj.Dispose();
		}
		stream = null;
		serverTicks = 0L;
		profilerEntityId = 0uL;
		profilerPrefabId = 0u;
		profilerAux = 0u;
		profilerInfoId = 0;
		profilerAuxIsStringId = false;
	}

	public void LeavePool()
	{
		refCount = 1;
	}

	public bool Start(BaseNetwork peer)
	{
		this.peer = peer;
		connections.Clear();
		stream = Pool.Get<BufferStream>().Initialize();
		return true;
	}

	public void Send(SendInfo info)
	{
		method = info.method;
		channel = info.channel;
		priority = info.priority;
		serverTicks = peer.GetServerTimestampTicks();
		if (info.connections != null)
		{
			connections.AddRange(info.connections);
		}
		if (info.connection != null)
		{
			connections.Add(info.connection);
		}
		NetProfileCapture.OnSend(peer, this);
		if (BaseNetwork.Multithreading)
		{
			peer.EnqueueWrite(this);
		}
		else
		{
			peer.ProcessWrite(this);
		}
	}

	public void SendImmediate(SendInfo info)
	{
		method = info.method;
		channel = info.channel;
		priority = info.priority;
		BaseNetwork baseNetwork = peer;
		long num;
		if (baseNetwork == null)
		{
			long ticks = DateTime.UtcNow.Ticks;
			DateTime unixEpoch = DateTime.UnixEpoch;
			num = ticks - unixEpoch.Ticks;
		}
		else
		{
			num = baseNetwork.GetServerTimestampTicks();
		}
		serverTicks = num;
		if (info.connections != null)
		{
			connections.AddRange(info.connections);
		}
		if (info.connection != null)
		{
			connections.Add(info.connection);
		}
		NetProfileCapture.OnSend(peer, this, immediate: true);
		peer.ProcessWrite(this);
	}

	public (byte[] Buffer, int Length) GetBuffer()
	{
		ArraySegment<byte> buffer = stream.GetBuffer();
		Assert.IsNotNull<byte[]>(buffer.Array, "buffer.Array != null");
		Assert.IsTrue(buffer.Offset == 0, "buffer.Offset == 0");
		return (Buffer: buffer.Array, Length: buffer.Count);
	}

	public Span<byte> GetBufferSpan()
	{
		var (array, length) = GetBuffer();
		return new Span<byte>(array, 0, length);
	}

	public byte PeekPacketID()
	{
		ArraySegment<byte> buffer = stream.GetBuffer();
		if (buffer.Array == null || buffer.Count <= 0)
		{
			return 0;
		}
		return buffer.Array[buffer.Offset];
	}

	public void PacketID(Message.Type val)
	{
		byte b = (byte)val;
		b += 140;
		UInt8(b);
	}

	public void UInt8(byte val)
	{
		WriteUnmanaged(in val);
	}

	public void UInt16(ushort val)
	{
		WriteUnmanaged(in val);
	}

	public void UInt32(uint val)
	{
		WriteUnmanaged(in val);
	}

	public void UInt64(ulong val)
	{
		WriteUnmanaged(in val);
	}

	public void Int8(sbyte val)
	{
		WriteUnmanaged(in val);
	}

	public void Int16(short val)
	{
		WriteUnmanaged(in val);
	}

	public void Int32(int val)
	{
		WriteUnmanaged(in val);
	}

	public void Int64(long val)
	{
		WriteUnmanaged(in val);
	}

	public void Bool(bool val)
	{
		WriteUnmanaged<byte>((byte)(val ? 1 : 0));
	}

	public void Float(float val)
	{
		WriteUnmanaged(in val);
	}

	public void Double(double val)
	{
		WriteUnmanaged(in val);
	}

	public void Bytes(ReadOnlySpan<byte> span)
	{
		Write(span);
	}

	public void VarUInt32(uint val)
	{
		ProtocolParser.WriteUInt32(stream, val);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteUInt32(uint value, bool variableLength)
	{
		if (variableLength)
		{
			VarUInt32(value);
		}
		else
		{
			UInt32(value);
		}
	}

	public void String(string val, bool variableLength = false)
	{
		if (string.IsNullOrEmpty(val))
		{
			BytesWithSize((MemoryStream)null, variableLength);
			return;
		}
		if (stringBuffer.Capacity < val.Length * 8)
		{
			stringBuffer.Capacity = val.Length * 8;
		}
		stringBuffer.Position = 0L;
		stringBuffer.SetLength(stringBuffer.Capacity);
		int bytes = Encoding.UTF8.GetBytes(val, 0, val.Length, stringBuffer.GetBuffer(), 0);
		stringBuffer.SetLength(bytes);
		BytesWithSize(stringBuffer, variableLength);
	}

	public void Vector3(in Vector3 obj)
	{
		Float(obj.x);
		Float(obj.y);
		Float(obj.z);
	}

	public void Vector4(in Vector4 obj)
	{
		Float(obj.x);
		Float(obj.y);
		Float(obj.z);
		Float(obj.w);
	}

	public void Quaternion(in Quaternion obj)
	{
		Float(obj.x);
		Float(obj.y);
		Float(obj.z);
		Float(obj.w);
	}

	public void Ray(in Ray obj)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector3(((Ray)(ref obj)).origin);
		Vector3(((Ray)(ref obj)).direction);
	}

	public void Color(in Color obj)
	{
		Float(obj.r);
		Float(obj.g);
		Float(obj.b);
		Float(obj.a);
	}

	public void Color32(in Color32 obj)
	{
		UInt8(obj.r);
		UInt8(obj.g);
		UInt8(obj.b);
		UInt8(obj.a);
	}

	public void EntityID(NetworkableId id)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		UInt64(id.Value);
	}

	public void ItemContainerID(ItemContainerId id)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		UInt64(id.Value);
	}

	public void ItemID(ItemId id)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		UInt64(id.Value);
	}

	public void GroupID(uint id)
	{
		UInt32(id);
	}

	public void Proto<T>(T proto) where T : IProto
	{
		if (proto == null)
		{
			throw new ArgumentNullException("proto");
		}
		((IProto)proto/*cast due to constrained. prefix*/).WriteToStream(stream);
	}

	public void ProtoDelta<T>(T proto, T previousProto) where T : IProto<T>
	{
		if (proto == null)
		{
			throw new ArgumentNullException("proto");
		}
		((IProto<T>)proto/*cast due to constrained. prefix*/).WriteToStreamDelta(stream, previousProto);
	}

	public void BytesWithSize(MemoryStream val, bool variableLength = false)
	{
		if (val == null || val.Length == 0L)
		{
			WriteUInt32(0u, variableLength);
		}
		else
		{
			BytesWithSize(new ReadOnlySpan<byte>(val.GetBuffer(), 0, (int)val.Length), variableLength);
		}
	}

	public void BytesWithSize(ReadOnlySpan<byte> span, bool variableLength = false)
	{
		if (span.Length > 10485760)
		{
			WriteUInt32(0u, variableLength);
			Debug.LogError((object)("BytesWithSize: Too big " + span.Length));
		}
		else
		{
			WriteUInt32((uint)span.Length, variableLength);
			Write(span);
		}
	}

	private void WriteUnmanaged<T>(in T val) where T : unmanaged
	{
		stream.Write<T>(val);
	}

	public override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	public override int ReadByte()
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlySpan<byte> readOnlySpan = new ReadOnlySpan<byte>(buffer, offset, count);
		RangeHandle range = stream.GetRange(count);
		readOnlySpan.CopyTo(((RangeHandle)(ref range)).GetSpan());
	}

	public override void Write(ReadOnlySpan<byte> span)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		RangeHandle range = stream.GetRange(span.Length);
		span.CopyTo(((RangeHandle)(ref range)).GetSpan());
	}

	public override void WriteByte(byte value)
	{
		UInt8(value);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}
}
