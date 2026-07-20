using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Rust.Water5;

internal struct NativeOceanDisplacementShort3 : IDisposable
{
	public readonly struct ReadOnly(ReadOnly<OceanDisplacementShort3> oceanDisplacementShort3s, int spectrumCount, int frameCount)
	{
		private readonly ReadOnly<OceanDisplacementShort3> oceanDisplacementShort3S = oceanDisplacementShort3s;

		private readonly int spectrumCount = spectrumCount;

		private readonly int frameCount = frameCount;

		public OceanDisplacementShort3 this[int x, int y, int z]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return oceanDisplacementShort3S[z * spectrumCount * frameCount + y * spectrumCount + x];
			}
		}
	}

	[NativeDisableParallelForRestriction]
	private NativeArray<OceanDisplacementShort3> _arr;

	private int spectrumCount;

	private int frameCount;

	public OceanDisplacementShort3 this[int x, int y, int z]
	{
		get
		{
			return _arr[z * spectrumCount * frameCount + y * spectrumCount + x];
		}
		set
		{
			_arr[z * spectrumCount * frameCount + y * spectrumCount + x] = value;
		}
	}

	public int Length => _arr.Length;

	public static Rust.Water5.NativeOceanDisplacementShort3 Create(int x, int y, int z)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return new Rust.Water5.NativeOceanDisplacementShort3
		{
			_arr = new NativeArray<OceanDisplacementShort3>(x * y * z, (Allocator)4, (NativeArrayOptions)0),
			spectrumCount = x,
			frameCount = y
		};
	}

	public static Rust.Water5.NativeOceanDisplacementShort3 Create(OceanDisplacementShort3[,,] simData)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Rust.Water5.NativeOceanDisplacementShort3 result = new Rust.Water5.NativeOceanDisplacementShort3
		{
			_arr = new NativeArray<OceanDisplacementShort3>(simData.Length, (Allocator)4, (NativeArrayOptions)1),
			spectrumCount = simData.GetLength(0),
			frameCount = simData.GetLength(1)
		};
		for (int i = 0; i < result.spectrumCount; i++)
		{
			for (int j = 0; j < result.frameCount; j++)
			{
				for (int k = 0; k < simData.GetLength(2); k++)
				{
					result._arr[i * result.spectrumCount + j * result.frameCount + k] = simData[i, j, k];
				}
			}
		}
		return result;
	}

	public unsafe OceanDisplacementShort3* GetUnsafePtr()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!_arr.IsCreated)
		{
			return null;
		}
		return (OceanDisplacementShort3*)NativeArrayUnsafeUtility.GetUnsafePtr<OceanDisplacementShort3>(_arr);
	}

	public ReadOnly<OceanDisplacementShort3> GetNativeRawReadOnly()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _arr.AsReadOnly();
	}

	public NativeArray<OceanDisplacementShort3> GetNativeRaw()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _arr;
	}

	public void Dispose()
	{
		_arr.Dispose();
	}

	public ReadOnly AsReadOnly()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return new ReadOnly(_arr.AsReadOnly(), spectrumCount, frameCount);
	}
}
