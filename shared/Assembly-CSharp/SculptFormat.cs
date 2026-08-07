using System;
using Facepunch;
using LZ4;
using ProtoBuf;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public static class SculptFormat
{
	private static readonly byte[] _decompressArr = new byte[81920];

	public unsafe static void SerializeToSculpt(BaseSculpture sculpture, BufferStream bufferStream)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SculptFormat.SerializeToSculpt"))
		{
			Sculpt val = Pool.Get<Sculpt>();
			try
			{
				val.bounds = Vector3Int.op_Implicit(sculpture.GridResolution);
				NativeArray<byte> flatArray = sculpture.SDFSet.Chunks[0].AcquireDataArray().FlatArray;
				byte[] array = Shared.ArrayPool.Rent(flatArray.Length);
				Debug.Assert(array.Length >= flatArray.Length);
				fixed (byte* ptr = array)
				{
					void* ptr2 = ptr;
					UnsafeUtility.MemCpy(ptr2, NativeArrayUnsafeUtility.GetUnsafePtr<byte>(flatArray), (long)(flatArray.Length * UnsafeUtility.SizeOf<byte>()));
				}
				val.data = new ArraySegment<byte>(array, 0, flatArray.Length);
				val.ownerId = sculpture.net.ID;
				val.ToProto(bufferStream);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public static byte[] GetStorageReadyBuffer(BufferStream bs)
	{
		using (TimeWarning.New("SculptFormat.GetStorageReadyBuffer"))
		{
			ArraySegment<byte> buffer = bs.GetBuffer();
			return LZ4Codec.Encode(buffer.Array, buffer.Offset, buffer.Count);
		}
	}

	public static Sculpt LoadDisposableSculptFromStorage(ArraySegment<byte> encoded)
	{
		using (TimeWarning.New("SculptFormat.LoadFromStorage"))
		{
			int num = 0;
			using (TimeWarning.New("LZ4.Decode"))
			{
				num = LZ4Codec.Decode(encoded.Array, encoded.Offset, encoded.Count, _decompressArr, 0, _decompressArr.Length, false);
			}
			BufferStream val = Pool.Get<BufferStream>().Initialize();
			try
			{
				val.Clear();
				Sculpt val2 = Pool.Get<Sculpt>();
				val.Initialize(_decompressArr, num);
				Sculpt.Deserialize(val, val2, false);
				return val2;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public static Sculpt LoadDisposableSculptFromStorage(byte[] encoded)
	{
		return LoadDisposableSculptFromStorage(new ArraySegment<byte>(encoded));
	}
}
