using System.Runtime.CompilerServices;

namespace System.Buffers;

internal sealed class ArrayMemoryPool<T> : MemoryPool<T>
{
	private sealed class ArrayMemoryPoolBuffer : IMemoryOwner<T>, IDisposable
	{
		private T[] _array;

		public Memory<T> Memory
		{
			get
			{
				T[] array = _array;
				if (array == null)
				{
					_003Cefefee0c_002Dbc34_002D4852_002Da4f2_002Df3a29a97a3fc_003EThrowHelper.ThrowObjectDisposedException_ArrayMemoryPoolBuffer();
				}
				return new Memory<T>(array);
			}
		}

		public ArrayMemoryPoolBuffer(int size)
		{
			_array = ArrayPool<T>.Shared.Rent(size);
		}

		public void Dispose()
		{
			T[] array = _array;
			if (array != null)
			{
				_array = null;
				ArrayPool<T>.Shared.Return(array);
			}
		}
	}

	private const int s_maxBufferSize = int.MaxValue;

	public sealed override int MaxBufferSize => int.MaxValue;

	public sealed override IMemoryOwner<T> Rent(int minimumBufferSize = -1)
	{
		if (minimumBufferSize == -1)
		{
			minimumBufferSize = 1 + 4095 / Unsafe.SizeOf<T>();
		}
		else if ((uint)minimumBufferSize > 2147483647u)
		{
			_003Cefefee0c_002Dbc34_002D4852_002Da4f2_002Df3a29a97a3fc_003EThrowHelper.ThrowArgumentOutOfRangeException(_003Ccc7a1cbb_002D4170_002D432f_002Db89a_002De8b4f50166fc_003EExceptionArgument.minimumBufferSize);
		}
		return new ArrayMemoryPoolBuffer(minimumBufferSize);
	}

	protected sealed override void Dispose(bool disposing)
	{
	}
}
