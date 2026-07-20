using System.Text;
using Unity.Collections;
using UnityEngine;

namespace Instancing;

public class CullingManager
{
	public NativeArray<RenderSlice> RenderSlicesArray;

	public GPUBuffer<RenderSlice> RenderSlicesBuffer;

	public void Initialize()
	{
		AllocateNativeMemory();
	}

	public void OnDestroy()
	{
		FreeNativeMemory();
	}

	private void AllocateNativeMemory()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		int num = 512;
		RenderSlicesArray = new NativeArray<RenderSlice>(num, (Allocator)4, (NativeArrayOptions)1);
		RenderSlicesBuffer = new GPUBuffer<RenderSlice>(num, GPUBuffer.Target.Structured);
	}

	private void FreeNativeMemory()
	{
		NativeArrayEx.SafeDispose(ref RenderSlicesArray);
		RenderSlicesBuffer?.Dispose();
		RenderSlicesBuffer = null;
	}

	public void EnsureCapacity(int rendererCount)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (RenderSlicesArray.Length < rendererCount + 1)
		{
			int newCapacity = Mathf.ClosestPowerOfTwo(rendererCount) * 2;
			NativeArrayEx.Expand(ref RenderSlicesArray, newCapacity, (NativeArrayOptions)1);
			RenderSlicesBuffer.Expand(newCapacity);
			RenderSlicesBuffer.SetData(RenderSlicesArray);
		}
	}

	public void PrintMemoryUsage(StringBuilder builder)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		builder.AppendLine("### CullingManager ###");
		builder.MemoryUsage<RenderSlice>("PostCullInstanceCounts", RenderSlicesArray);
	}

	public void UpdateComputeBuffers()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		RenderSlicesBuffer.SetData(RenderSlicesArray);
	}
}
