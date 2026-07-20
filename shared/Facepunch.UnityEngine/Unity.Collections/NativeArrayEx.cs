using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Unity.Collections;

public static class NativeArrayEx
{
	public static void Add<T>(this ref NativeArray<T> array, T item, ref int size) where T : unmanaged
	{
		if (size >= array.Length)
		{
			Expand(ref array, array.Length * 2, (NativeArrayOptions)1);
		}
		array[size] = item;
		size++;
	}

	public static void RemoveUnordered<T>(this ref NativeArray<T> array, int index, ref int count) where T : unmanaged
	{
		int num = count - 1;
		if (index != num)
		{
			array[index] = array[num];
		}
		count--;
	}

	public static void Expand<T>(this ref NativeArray<T> array, int newCapacity, NativeArrayOptions options = (NativeArrayOptions)1, bool copyContents = true, bool usePowerOfTwo = false) where T : struct
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (newCapacity <= array.Length)
		{
			return;
		}
		NativeArray<T> val = default(NativeArray<T>);
		val._002Ector(usePowerOfTwo ? Mathf.NextPowerOfTwo(newCapacity) : newCapacity, (Allocator)4, options);
		if (array.IsCreated)
		{
			if (copyContents)
			{
				array.CopyTo(val.GetSubArray(0, array.Length));
			}
			array.Dispose();
		}
		array = val;
	}

	public static void SafeDispose<T>(this ref NativeArray<T> array) where T : unmanaged
	{
		if (array.IsCreated)
		{
			array.Dispose();
		}
	}

	public static void SafeDispose<T>(this ref NativeArray<T> array, JobHandle dependsOn) where T : unmanaged
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (array.IsCreated)
		{
			array.Dispose(dependsOn);
		}
	}

	public unsafe static void MemClear<T>(this in NativeArray<T> array) where T : struct
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		UnsafeUtility.MemClear(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks<T>(array), (long)(UnsafeUtility.SizeOf<T>() * array.Length));
	}

	public unsafe static void MemSet<T>(this in NativeArray<T> array, byte value) where T : struct
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		UnsafeUtility.MemSet(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks<T>(array), value, (long)(UnsafeUtility.SizeOf<T>() * array.Length));
	}

	public unsafe static NativeBitArray AsBitArray<T>(this ref NativeArray<T> array) where T : struct
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return NativeBitArrayUnsafeUtility.ConvertExistingDataToNativeBitArray(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks<T>(array), array.Length, AllocatorHandle.op_Implicit((Allocator)1));
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private static void CheckReinterpretLoadRange<U, T>(this ref ReadOnly<T> array, int sourceIndex) where U : struct where T : struct
	{
	}

	public unsafe static U ReinterpretLoad<U, T>(this ref ReadOnly<T> array, int sourceIndex) where U : struct where T : struct
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return UnsafeUtility.ReadArrayElement<U>((void*)((byte*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr<T>(array) + (long)UnsafeUtility.SizeOf<T>() * (long)sourceIndex), 0);
	}

	public static bool TryFindRegion<T>(this ref NativeArray<T> array, T value, ref int index, ref int length) where T : struct, IEquatable<T>
	{
		index += length;
		while (index < array.Length && !EqualityComparer<T>.Default.Equals(array[index], value))
		{
			index++;
		}
		if (index == array.Length)
		{
			return false;
		}
		length = 1;
		while (index + length < array.Length && EqualityComparer<T>.Default.Equals(array[index + length], value))
		{
			length++;
		}
		return true;
	}

	public static bool GenericEquals<T>(T a, T b) where T : IEquatable<T>
	{
		return a.Equals(b);
	}
}
