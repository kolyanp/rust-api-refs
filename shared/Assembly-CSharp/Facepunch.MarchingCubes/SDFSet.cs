using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UtilityJobs;

namespace Facepunch.MarchingCubes;

public class SDFSet : FacepunchBehaviour, IDisposable
{
	public GameObject TargetRoot;

	public GameObjectRef ChunkPrefab;

	public float ChunkScale;

	public float iso;

	[NonSerialized]
	private NativeList<Shape> Modifications;

	[NonSerialized]
	public List<SDFChunk> Chunks;

	[NonSerialized]
	public List<SDFChunk> CensorChunks;

	[CompilerGenerated]
	private JobHandle _003CDataDependency_003Ek__BackingField;

	public const float SoftnessFraction = 0.15f;

	public JobHandle DataDependency
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CDataDependency_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CDataDependency_003Ek__BackingField = value;
		}
	}

	public bool IsCreated { get; private set; }

	public JobHandle ConsumeDataDependency()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		JobHandle dataDependency = DataDependency;
		DataDependency = default(JobHandle);
		return dataDependency;
	}

	public void AddDataDependency(JobHandle handle)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		DataDependency = JobHandle.CombineDependencies(DataDependency, handle);
	}

	public void CompleteDataJobs()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		JobHandle val = DataDependency;
		((JobHandle)(ref val)).Complete();
		val = (DataDependency = default(JobHandle));
	}

	public void Init()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Modifications = new NativeList<Shape>(AllocatorHandle.op_Implicit((Allocator)4));
		Chunks = new List<SDFChunk>();
		iso = 128f;
		IsCreated = true;
	}

	public SDFChunk AddChunk(int3 origin, int3 bounds)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		SDFChunk component = ChunkPrefab.Instantiate(TargetRoot.transform).GetComponent<SDFChunk>();
		component.Init(this, Chunks.Count, origin, bounds, ChunkScale, iso);
		Chunks.Add(component);
		return component;
	}

	public SDFChunk AddCensorChunk(SDFChunk copyOf)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		SDFChunk component = ChunkPrefab.Instantiate(TargetRoot.transform).GetComponent<SDFChunk>();
		component.Init(this, copyOf.ChunkId, copyOf.Origin, copyOf.DataArray.Bounds, ChunkScale, iso);
		CensorChunks.Add(component);
		return component;
	}

	public void ClearAllMods()
	{
		CompleteDataJobs();
		Modifications.Clear();
	}

	public void ScheduleClearAllMods()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		ClearListJob<Shape> clearListJob = new ClearListJob<Shape>
		{
			List = Modifications
		};
		DataDependency = IJobExtensions.Schedule<ClearListJob<Shape>>(clearListJob, DataDependency);
	}

	public void ClearChunks()
	{
		CompleteDataJobs();
		foreach (SDFChunk chunk in Chunks)
		{
			chunk.FillEmpty();
		}
	}

	public static float SmoothingForRadius(float softness, float radius)
	{
		return math.min(softness * 0.15f * radius, 0.625f);
	}

	public void AddMod(in Shape shape)
	{
		CompleteDataJobs();
		Modifications.Add(ref shape);
	}

	public void AddSphereMod(float3 blockSpacePos, float radius, bool isAdditive, float smoothing = 0f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		AddMod(new Shape(ShapeType.Sphere, blockSpacePos, new float3(radius), quaternion.identity, isAdditive, smoothing));
	}

	public void AddBulgeMod(float3 blockSpacePos, float radius, float strength, bool isPull)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		AddMod(new Shape(ShapeType.Bulge, blockSpacePos, new float3(radius, strength, 0f), quaternion.identity, isPull, 0f));
	}

	public void AddSmoothMod(float3 blockSpacePos, float radius, float strength)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		AddMod(new Shape(ShapeType.Smooth, blockSpacePos, new float3(radius, strength, 0f), quaternion.identity, isAdditive: false, 0f));
	}

	public void AddAABBMod(float3 blockSpacePos, float3 extents, bool isAdditive, float smoothing = 0f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		AddMod(new Shape(ShapeType.AABB, blockSpacePos, extents, quaternion.identity, isAdditive, smoothing));
	}

	public void AddOBBMod(float3 blockSpacePos, float3 extents, quaternion rotation, bool isAdditive, float smoothing = 0f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		AddMod(new Shape(ShapeType.OBB, blockSpacePos, extents, rotation, isAdditive, smoothing));
	}

	public void AddSharpOBBMod(float3 blockSpacePos, float3 extents, quaternion rotation, bool isAdditive, float smoothing = 0f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		AddMod(new Shape(ShapeType.SharpOBB, blockSpacePos, extents, rotation, isAdditive, smoothing));
	}

	public void AddCylinderMod(float3 blockSpacePos, float3 extents, quaternion rotation, bool isAdditive, float smoothing = 0f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		AddMod(new Shape(ShapeType.Cylinder, blockSpacePos, extents, rotation, isAdditive, smoothing));
	}

	public void AddCapsuleMod(float3 blockSpacePos, float3 extents, quaternion rotation, bool isAdditive, float smoothing = 0f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		AddMod(new Shape(ShapeType.Capsule, blockSpacePos, extents, rotation, isAdditive, smoothing));
	}

	public void AddConeMod(float3 blockSpacePos, float3 extents, quaternion rotation, bool isAdditive, float smoothing = 0f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		AddMod(new Shape(ShapeType.Cone, blockSpacePos, extents, rotation, isAdditive, smoothing));
	}

	public void AddHexPrismMod(float3 blockSpacePos, float3 extents, quaternion rotation, bool isAdditive, float smoothing = 0f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		AddMod(new Shape(ShapeType.HexPrism, blockSpacePos, extents, rotation, isAdditive, smoothing));
	}

	public void RegenerateAllChunks()
	{
		ScheduleRegenerateAllChunks();
		CompleteDataJobs();
	}

	public void ScheduleRegenerateAllChunks()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (Chunks.Count != 0)
		{
			JobHandle val = DataDependency;
			ReadOnly<Shape> mods = Modifications.AsReadOnly();
			for (int i = 0; i < Chunks.Count; i++)
			{
				JobHandle val2 = Chunks[i].GenerateChunkData(mods, DataDependency);
				val = JobHandle.CombineDependencies(val, val2);
			}
			DataDependency = val;
		}
	}

	public int GetMaxYLayer()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		CompleteDataJobs();
		NativeReference<int> maxYLayer = default(NativeReference<int>);
		maxYLayer._002Ector(AllocatorHandle.op_Implicit((Allocator)3), (NativeArrayOptions)1);
		FindMaxYLayerJob findMaxYLayerJob = new FindMaxYLayerJob
		{
			data = Chunks[0].DataArray,
			iso = iso,
			maxYLayer = maxYLayer
		};
		IJobExtensions.RunByRef<FindMaxYLayerJob>(ref findMaxYLayerJob);
		int value = maxYLayer.Value;
		maxYLayer.Dispose(default(JobHandle));
		return value;
	}

	public void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		IsCreated = false;
		CompleteDataJobs();
		NativeListEx.SafeDispose(ref Modifications);
		if (Chunks != null)
		{
			foreach (SDFChunk chunk in Chunks)
			{
				if (Object.op_Implicit((Object)(object)chunk))
				{
					chunk.Dispose();
				}
			}
		}
		if (CensorChunks == null)
		{
			return;
		}
		foreach (SDFChunk censorChunk in CensorChunks)
		{
			if (Object.op_Implicit((Object)(object)censorChunk))
			{
				censorChunk.Dispose();
			}
		}
	}
}
