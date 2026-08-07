using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Facepunch.MarchingCubes;

[GenerateTestsForBurstCompatibility]
public struct QuantizedFloatData3DArray : IDisposable
{
	public const float MaxValue = 2.5f;

	public const float InvMaxValue = 0.4f;

	public const float MaxSmoothing = 0.625f;

	public NativeArray<byte> FlatArray;

	public int3 Origin;

	public int3 Bounds;

	private int _widthHeight;

	public int Width => Bounds.x;

	public int Height => Bounds.y;

	public int Depth => Bounds.z;

	public int WidthHeight => _widthHeight;

	public int NumCells => FlatArray.Length;

	public bool IsCreated => FlatArray.IsCreated;

	public float this[int x, int y, int z]
	{
		get
		{
			return Sample(ToIndex(x, y, z));
		}
		set
		{
			FlatArray[ToIndex(x, y, z)] = Compress(value);
		}
	}

	public void Init(int3 origin, int3 bounds, Allocator allocator = (Allocator)4)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Origin = origin;
		Bounds = bounds;
		_widthHeight = Width * Height;
		FlatArray = new NativeArray<byte>(Bounds.x * Bounds.y * Bounds.z, allocator, (NativeArrayOptions)1);
	}

	public static int3 MipBounds(int3 bounds, int level)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		int num = 1 << level;
		return math.max(int3.op_Implicit(2), (bounds + (num - 1)) / num);
	}

	public void Clear()
	{
		if (FlatArray.IsCreated)
		{
			NativeArrayEx.MemClear(in FlatArray);
		}
	}

	public void ToLocalIntBounds(in Bounds worldFloatBounds, out int3 min, out int3 max)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		min = math.max(int3.op_Implicit(0), (int3)math.floor(float3.op_Implicit(((Bounds)(ref worldFloatBounds)).min) - float3.op_Implicit(Origin)));
		max = math.min(Bounds - 1, (int3)math.ceil(float3.op_Implicit(((Bounds)(ref worldFloatBounds)).max) - float3.op_Implicit(Origin)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int ToIndex(int x, int y, int z)
	{
		return x + y * Width + z * WidthHeight;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int ToIndex(int3 c)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return ToIndex(c.x, c.y, c.z);
	}

	public byte GetByte(int x, int y, int z)
	{
		return FlatArray[ToIndex(x, y, z)];
	}

	public void SetByte(int x, int y, int z, byte b)
	{
		FlatArray[ToIndex(x, y, z)] = b;
	}

	public float Sample(int3 p)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return (int)FlatArray[ToIndex(p.x, p.y, p.z)];
	}

	public float Sample(int flatIndex)
	{
		return (int)FlatArray[flatIndex];
	}

	public byte Compress(float f)
	{
		return (byte)Mathf.Clamp((int)((f * 0.4f * 0.5f + 0.5f) * 255f), 0, 255);
	}

	public void Dispose()
	{
		NativeArrayEx.SafeDispose(ref FlatArray);
	}
}
