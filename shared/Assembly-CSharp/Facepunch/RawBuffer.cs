using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Facepunch;

public sealed class RawBuffer<T> : IDisposable where T : unmanaged
{
	private unsafe void* _ptr;

	private int _length;

	private int _capacity;

	public int Count => _length;

	public int Capacity => _capacity;

	public unsafe IntPtr Ptr => (IntPtr)_ptr;

	public unsafe T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ((T*)_ptr)[index];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			((T*)_ptr)[index] = value;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear()
	{
		_length = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Add(T item)
	{
		if (_length >= _capacity)
		{
			Grow(_length + 1);
		}
		((T*)_ptr)[_length++] = item;
	}

	public unsafe void AddRange(T* src, int count)
	{
		if (count > 0)
		{
			EnsureCapacity(_length + count);
			UnsafeUtility.MemCpy((void*)((byte*)_ptr + (nint)_length * (nint)sizeof(T)), (void*)src, (long)count * (long)sizeof(T));
			_length += count;
		}
	}

	public unsafe void AddRange(NativeArray<T> src, int count)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (count > 0)
		{
			AddRange((T*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr<T>(src), count);
		}
	}

	public unsafe T* AppendUninitialized(int count)
	{
		EnsureCapacity(_length + count);
		byte* result = (byte*)_ptr + (nint)_length * (nint)sizeof(T);
		_length += count;
		return (T*)result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void EnsureCapacity(int required)
	{
		if (required > _capacity)
		{
			Grow(required);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe void Grow(int required)
	{
		int num = ((_capacity == 0) ? 4096 : (_capacity * 2));
		if (num < required)
		{
			num = required;
		}
		void* ptr = UnsafeUtility.Malloc((long)num * (long)sizeof(T), UnsafeUtility.AlignOf<T>(), (Allocator)4);
		if (_ptr != null)
		{
			if (_length > 0)
			{
				UnsafeUtility.MemCpy(ptr, _ptr, (long)_length * (long)sizeof(T));
			}
			UnsafeUtility.Free(_ptr, (Allocator)4);
		}
		_ptr = ptr;
		_capacity = num;
	}

	public unsafe void Dispose()
	{
		if (_ptr != null)
		{
			UnsafeUtility.Free(_ptr, (Allocator)4);
			_ptr = null;
		}
		_length = 0;
		_capacity = 0;
	}
}
