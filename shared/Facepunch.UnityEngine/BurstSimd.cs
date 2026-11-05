using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

public static class BurstSimd
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4x2 LoadTransposed2(this ref ReadOnly<float2> array, int index)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return math.transpose(array.ReinterpretLoad<float2x4, float2>(index));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4x2 LoadTransposed2(this NativeArray<float2> array, int index)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return math.transpose(array.ReinterpretLoad<float2x4>(index));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void StoreTransposed2(this NativeArray<float2> array, int index, in float4x2 value)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		array.ReinterpretStore<float2x4>(index, math.transpose(value));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4x3 LoadTransposed3(this ref ReadOnly<float3> array, int index)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return math.transpose(array.ReinterpretLoad<float3x4, float3>(index));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4x3 LoadTransposed3(this NativeArray<float3> array, int index)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return math.transpose(array.ReinterpretLoad<float3x4>(index));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void StoreTransposed3(this NativeArray<float3> array, int index, in float4x3 value)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		array.ReinterpretStore<float3x4>(index, math.transpose(value));
	}
}
