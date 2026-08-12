using System;
using System.Runtime.CompilerServices;
using Facepunch.NativeMeshSimplification;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Facepunch.MarchingCubes;

public class MarchingCubesGenerator : IDisposable
{
	[CompilerGenerated]
	private float3 _003COffset_003Ek__BackingField;

	[CompilerGenerated]
	private TimeSince _003CSinceLastUse_003Ek__BackingField;

	[CompilerGenerated]
	private Bounds _003CMeshSpaceBounds_003Ek__BackingField;

	public const int MaxMipLevel = 2;

	private readonly NativeMeshSimplifier _simplifier;

	private readonly QuantizedFloatData3DArray[] mips;

	private int3 mipSourceBounds;

	public Mesh Mesh { get; set; }

	public Mesh MeshForCollision { get; set; }

	public MeshCollider MeshCollider { get; set; }

	public float3 Offset
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003COffset_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003COffset_003Ek__BackingField = value;
		}
	}

	public float Scale { get; set; }

	public TimeSince SinceLastUse
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CSinceLastUse_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CSinceLastUse_003Ek__BackingField = value;
		}
	}

	public bool UsedSinceLastFree { get; set; }

	public int MeshInstanceId => ((Object)Mesh).GetInstanceID();

	public int CollisionMeshInstanceId => ((Object)MeshForCollision).GetInstanceID();

	public Bounds MeshSpaceBounds
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CMeshSpaceBounds_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CMeshSpaceBounds_003Ek__BackingField = value;
		}
	}

	public static int ClampRenderMeshCount(int count)
	{
		return math.clamp(count, 0, 3);
	}

	public MarchingCubesGenerator(Mesh meshToUpdate, Mesh meshForCollision, MeshCollider meshCollider, float3 vertexOffset, float vertexScale)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		mips = new QuantizedFloatData3DArray[2];
		base._002Ector();
		Mesh = meshToUpdate;
		MeshForCollision = meshForCollision;
		MeshCollider = meshCollider;
		_simplifier = new NativeMeshSimplifier();
		Offset = vertexOffset;
		Scale = vertexScale;
	}

	public int TotalNativeMemoryUsage()
	{
		int num = 0;
		for (int i = 0; i < mips.Length; i++)
		{
			if (mips[i].IsCreated)
			{
				num += mips[i].NumCells;
			}
		}
		return num;
	}

	public void ZeroOutAllocations()
	{
		UsedSinceLastFree = false;
		DisposeMips();
	}

	private void DisposeMips()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < mips.Length; i++)
		{
			mips[i].Dispose();
			mips[i] = default(QuantizedFloatData3DArray);
		}
		mipSourceBounds = default(int3);
	}

	private void MarkUsed()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		SinceLastUse = TimeSince.op_Implicit(0f);
		UsedSinceLastFree = true;
	}

	public JobHandle ScheduleMarchChain(SDFSet set, int renderMeshCount, int colliderMipLevel, bool censored, NativeList<MeshDataArray> results, JobHandle inputDeps)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		Debug.Assert(set.Chunks.Count == 1);
		colliderMipLevel = math.clamp(colliderMipLevel, 0, 2);
		renderMeshCount = ClampRenderMeshCount(renderMeshCount);
		censored &= set.CensorChunks != null && set.CensorChunks.Count > 0;
		QuantizedFloatData3DArray dataArray = set.Chunks[0].DataArray;
		QuantizedFloatData3DArray source = (censored ? set.CensorChunks[0].DataArray : dataArray);
		JobHandle val = inputDeps;
		int length = results.Length;
		MeshDataArray val2 = default(MeshDataArray);
		results.Add(ref val2);
		bool flag = !censored && colliderMipLevel < renderMeshCount;
		if (!flag)
		{
			val = ScheduleMipPyramid(dataArray, colliderMipLevel, val);
			val = ScheduleLevelMarch(dataArray, set.iso, colliderMipLevel, out var vertices, out var indices, val);
			val = ScheduleMeshWrite(vertices, indices, out var meshData, withNormals: false, val);
			results[length] = meshData;
			vertices.Dispose(val);
			indices.Dispose(val);
		}
		if (renderMeshCount == 0)
		{
			set.AddDataDependency(val);
			return val;
		}
		val = ScheduleMipPyramid(source, renderMeshCount - 1, val);
		JobHandle val3 = default(JobHandle);
		for (int i = 0; i < renderMeshCount; i++)
		{
			JobHandle inputDeps2 = ScheduleLevelMarch(source, set.iso, i, out var vertices2, out var indices2, val);
			JobHandle val4 = ScheduleMeshWrite(vertices2, indices2, out var meshData2, withNormals: true, inputDeps2);
			results.Add(ref meshData2);
			if (flag && i == colliderMipLevel)
			{
				JobHandle val5 = ScheduleMeshWrite(vertices2, indices2, out var meshData3, withNormals: false, inputDeps2);
				results[length] = meshData3;
				val4 = JobHandle.CombineDependencies(val4, val5);
			}
			vertices2.Dispose(val4);
			indices2.Dispose(val4);
			val3 = JobHandle.CombineDependencies(val3, val4);
		}
		set.AddDataDependency(val3);
		return val3;
	}

	private JobHandle ScheduleLevelMarch(QuantizedFloatData3DArray source, float iso, int level, out NativeList<float3> vertices, out NativeList<int> indices, JobHandle inputDeps)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		QuantizedFloatData3DArray data3DArray = ((level == 0) ? source : mips[level - 1]);
		return ScheduleSDFMarch(data3DArray, iso, level, source.Bounds, out vertices, out indices, inputDeps);
	}

	public JobHandle ScheduleSDFMarch(SDFSet set, bool isCensored, int mipLevel, out NativeList<float3> vertices, out NativeList<int> indices, JobHandle inputDeps)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Debug.Assert(set.Chunks.Count == 1);
		QuantizedFloatData3DArray source = (isCensored ? set.CensorChunks[0].DataArray : set.Chunks[0].DataArray);
		mipLevel = math.clamp(mipLevel, 0, 2);
		if (mipLevel > 0)
		{
			inputDeps = ScheduleMipPyramid(source, mipLevel, inputDeps);
		}
		return ScheduleLevelMarch(source, set.iso, mipLevel, out vertices, out indices, inputDeps);
	}

	private JobHandle ScheduleMipPyramid(QuantizedFloatData3DArray source, int mipLevel, JobHandle inputDeps)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (!math.all(mipSourceBounds == source.Bounds))
		{
			DisposeMips();
			mipSourceBounds = source.Bounds;
		}
		JobHandle val = inputDeps;
		for (int i = 1; i <= mipLevel; i++)
		{
			QuantizedFloatData3DArray quantizedFloatData3DArray = mips[i - 1];
			if (!quantizedFloatData3DArray.IsCreated)
			{
				quantizedFloatData3DArray.Init(source.Origin, QuantizedFloatData3DArray.MipBounds(source.Bounds, i), (Allocator)4);
				mips[i - 1] = quantizedFloatData3DArray;
			}
			QuantizedFloatData3DArray src = ((i == 1) ? source : mips[i - 2]);
			int numCells = quantizedFloatData3DArray.NumCells;
			int num = math.max(1, numCells / JobsUtility.JobWorkerCount);
			val = IJobParallelForBatchExtensions.Schedule<Facepunch.MarchingCubes.DownsampleJob>(new Facepunch.MarchingCubes.DownsampleJob
			{
				src = src,
				dst = quantizedFloatData3DArray
			}, numCells, num, val);
			val = SDFChunk.ScheduleClearBoundaries(quantizedFloatData3DArray, val);
		}
		return val;
	}

	public JobHandle ScheduleSDFMarch(QuantizedFloatData3DArray data3DArray, float iso, out NativeList<float3> vertices, out NativeList<int> indices, JobHandle inputDeps)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		return ScheduleSDFMarch(data3DArray, iso, 0, data3DArray.Bounds, out vertices, out indices, inputDeps);
	}

	public JobHandle ScheduleSDFMarch(QuantizedFloatData3DArray data3DArray, float iso, int mipLevel, int3 baseBounds, out NativeList<float3> vertices, out NativeList<int> indices, JobHandle inputDeps)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		MarkUsed();
		float3 val = new float3(float3.op_Implicit(baseBounds) * 0.5f) + Offset;
		float3 val2 = -val * Scale;
		float3 val3 = (float3.op_Implicit(baseBounds - 1) - val) * Scale;
		MeshSpaceBounds = new Bounds(float3.op_Implicit((val2 + val3) * 0.5f), float3.op_Implicit(val3 - val2));
		int num = 1 << mipLevel;
		float scale = Scale * (float)num;
		float3 vertexOffset = (val - (float)(num - 1) * 0.5f) / (float)num;
		JobHandle val4 = inputDeps;
		int num2 = data3DArray.WidthHeight * data3DArray.Depth;
		int num3 = math.max(1, num2 / JobsUtility.JobWorkerCount);
		int num4 = (num2 + num3 - 1) / num3;
		NativeStream val5 = default(NativeStream);
		((NativeStream)(ref val5))._002Ector(num4, AllocatorHandle.op_Implicit((Allocator)3));
		val4 = IJobParallelForBatchExtensions.Schedule<Facepunch.MarchingCubes.MarchFloatGenerateTrianglesJob>(new Facepunch.MarchingCubes.MarchFloatGenerateTrianglesJob
		{
			sampler = data3DArray,
			edgeStream = ((NativeStream)(ref val5)).AsWriter(),
			iso = iso,
			vertexOffset = vertexOffset,
			scale = scale,
			batchSize = num3
		}, num2, num3, val4);
		vertices = new NativeList<float3>(AllocatorHandle.op_Implicit((Allocator)3));
		indices = new NativeList<int>(AllocatorHandle.op_Implicit((Allocator)3));
		val4 = IJobExtensions.Schedule<Facepunch.MarchingCubes.ProcessTrianglesJob>(new Facepunch.MarchingCubes.ProcessTrianglesJob
		{
			edgeStream = ((NativeStream)(ref val5)).AsReader(),
			vertices = vertices,
			indices = indices,
			edgeArraySize = data3DArray.NumCells * 3
		}, val4);
		((NativeStream)(ref val5)).Dispose(val4);
		return val4;
	}

	public JobHandle ScheduleSimplification(NativeList<float3> verticesIn, NativeList<int> indicesIn, out NativeList<float3> verticesOut, out NativeList<int> indicesOut, JobHandle inputDeps)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		verticesOut = new NativeList<float3>(AllocatorHandle.op_Implicit((Allocator)3));
		indicesOut = new NativeList<int>(AllocatorHandle.op_Implicit((Allocator)3));
		return _simplifier.ScheduleMeshSimplify(0.4f, verticesIn, indicesIn, verticesOut, indicesOut, inputDeps);
	}

	public JobHandle ScheduleMeshWrite(NativeList<float3> vertices, NativeList<int> indices, out MeshDataArray meshData, bool withNormals, JobHandle inputDeps)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		meshData = Mesh.AllocateWritableMeshData(1);
		return IJobExtensions.Schedule<Facepunch.MarchingCubes.WriteMeshDataJob>(new Facepunch.MarchingCubes.WriteMeshDataJob
		{
			vertices = vertices.AsDeferredJobArray(),
			indices = indices.AsDeferredJobArray(),
			meshData = ((MeshDataArray)(ref meshData))[0],
			withNormals = withNormals
		}, inputDeps);
	}

	public void ApplyMeshData(MeshDataArray meshData, Mesh toMesh)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("MarchingCubes.ApplyMeshData"))
		{
			if ((Object)(object)toMesh == (Object)null)
			{
				((MeshDataArray)(ref meshData)).Dispose();
				return;
			}
			Mesh.ApplyAndDisposeWritableMeshData(meshData, toMesh, (MeshUpdateFlags)9);
			toMesh.bounds = MeshSpaceBounds;
			if (BaseSculpture.LogMeshStats)
			{
				Debug.Log((object)$"{((Object)toMesh).name} : tris({toMesh.GetIndexCount(0) / 3}) verts({toMesh.vertexCount})");
			}
		}
	}

	public void Dispose()
	{
		DisposeMips();
		_simplifier.Dispose();
	}
}
