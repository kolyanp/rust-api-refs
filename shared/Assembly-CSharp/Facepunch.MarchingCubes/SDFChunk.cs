using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Facepunch.MarchingCubes;

public class SDFChunk : FacepunchBehaviour, IDisposable
{
	[NonSerialized]
	public SDFSet Owner;

	[NonSerialized]
	public int ChunkId;

	[NonSerialized]
	public QuantizedFloatData3DArray DataArray;

	[NonSerialized]
	public int3 Origin;

	[NonSerialized]
	public Bounds ChunkBoundsSetSpace;

	private float _iso;

	public void Init(SDFSet owner, int chunkId, int3 origin, int3 bounds, float scale, float iso)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Owner = owner;
		ChunkId = chunkId;
		DataArray.Init(origin, bounds, (Allocator)4);
		Origin = origin;
		((Component)this).transform.localPosition = float3.op_Implicit(float3.op_Implicit(Origin) * scale);
		ChunkBoundsSetSpace = new Bounds(float3.op_Implicit(float3.op_Implicit(origin) + float3.op_Implicit(bounds) / 2f), float3.op_Implicit(float3.op_Implicit(bounds)));
		_iso = iso;
	}

	public unsafe void FillEmpty()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		UnsafeUtility.MemSet(NativeArrayUnsafeUtility.GetUnsafePtr<byte>(DataArray.FlatArray), byte.MaxValue, (long)DataArray.FlatArray.Length);
	}

	public QuantizedFloatData3DArray AcquireDataArray()
	{
		Owner.CompleteDataJobs();
		return DataArray;
	}

	public void CopyFrom(SDFChunk source)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Owner.CompleteDataJobs();
		source.Owner.CompleteDataJobs();
		DataArray.FlatArray.CopyFrom(source.DataArray.FlatArray);
	}

	public void CopyToByteArray(ref byte[] arr)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Owner.CompleteDataJobs();
		NativeArray<byte> flatArray = DataArray.FlatArray;
		if (arr.Length < flatArray.Length)
		{
			arr = new byte[flatArray.Length];
		}
		flatArray.CopyTo(arr);
	}

	public unsafe void CopyFromByteArray(ArraySegment<byte> arr)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		Owner.CompleteDataJobs();
		NativeArray<byte> flatArray = DataArray.FlatArray;
		if (arr.Count != flatArray.Length)
		{
			Debug.LogError((object)"Trying to load non-matching sized grid");
			return;
		}
		fixed (byte* array = arr.Array)
		{
			void* unsafePtr = NativeArrayUnsafeUtility.GetUnsafePtr<byte>(flatArray);
			int num = UnsafeUtility.SizeOf<byte>();
			UnsafeUtility.MemCpy(unsafePtr, (void*)(array + arr.Offset * num), (long)(arr.Count * num));
		}
	}

	public JobHandle GenerateCensoredChunk(QuantizedFloatData3DArray srcData, int3 segments, JobHandle inputDeps)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		Debug.Assert(math.all(segments > 0));
		int num = segments.x * segments.y * segments.z;
		int num2 = math.max(1, num / JobsUtility.JobWorkerCount);
		int num3 = (num + num2 - 1) / num2;
		NativeStream val = default(NativeStream);
		((NativeStream)(ref val))._002Ector(num3, AllocatorHandle.op_Implicit((Allocator)3));
		JobHandle val2 = IJobParallelForBatchExtensions.Schedule<Facepunch.MarchingCubes.SDFChunkJobs.AccumulateCensorBoundsJob>(new Facepunch.MarchingCubes.SDFChunkJobs.AccumulateCensorBoundsJob
		{
			SrcData = srcData,
			ShapeStream = ((NativeStream)(ref val)).AsWriter(),
			SegmentsX = segments.x,
			SegmentsY = segments.y,
			SegmentsZ = segments.z,
			iso = _iso,
			batchSize = num2
		}, num, num2, inputDeps);
		Facepunch.MarchingCubes.SDFChunkJobs.ApplyCensorBoundsJob obj = new Facepunch.MarchingCubes.SDFChunkJobs.ApplyCensorBoundsJob
		{
			OutputArray = DataArray,
			ShapeStream = ((NativeStream)(ref val)).AsReader()
		};
		int num4 = math.max(1, DataArray.Depth / JobsUtility.JobWorkerCount);
		val2 = IJobParallelForExtensions.Schedule<Facepunch.MarchingCubes.SDFChunkJobs.ApplyCensorBoundsJob>(obj, DataArray.Depth, num4, val2);
		((NativeStream)(ref val)).Dispose(val2);
		Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob clearBoundariesJob = new Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob
		{
			DataArray = DataArray
		};
		return IJobExtensions.ScheduleByRef<Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob>(ref clearBoundariesJob, val2);
	}

	public static JobHandle ScheduleClearBoundaries(QuantizedFloatData3DArray dataArray, JobHandle inputDeps)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob clearBoundariesJob = new Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob
		{
			DataArray = dataArray
		};
		return IJobExtensions.ScheduleByRef<Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob>(ref clearBoundariesJob, inputDeps);
	}

	public JobHandle GenerateChunkData(ReadOnly<Shape> mods, JobHandle inputDeps)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		Facepunch.MarchingCubes.SDFChunkJobs.CalculateDistanceFieldJob calculateDistanceFieldJob = new Facepunch.MarchingCubes.SDFChunkJobs.CalculateDistanceFieldJob
		{
			Origin = float3.op_Implicit(Origin),
			ChunkBounds = ChunkBoundsSetSpace,
			Mods = mods,
			DataArray = DataArray
		};
		inputDeps = IJobExtensions.ScheduleByRef<Facepunch.MarchingCubes.SDFChunkJobs.CalculateDistanceFieldJob>(ref calculateDistanceFieldJob, inputDeps);
		Facepunch.MarchingCubes.SDFChunkJobs.CleanupIslandsJob cleanupIslandsJob = new Facepunch.MarchingCubes.SDFChunkJobs.CleanupIslandsJob
		{
			DataArray = DataArray,
			Iso = _iso
		};
		inputDeps = IJobExtensions.ScheduleByRef<Facepunch.MarchingCubes.SDFChunkJobs.CleanupIslandsJob>(ref cleanupIslandsJob, inputDeps);
		Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob clearBoundariesJob = new Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob
		{
			DataArray = DataArray
		};
		inputDeps = IJobExtensions.ScheduleByRef<Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob>(ref clearBoundariesJob, inputDeps);
		return inputDeps;
	}

	public void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		DataArray.Dispose();
	}
}
