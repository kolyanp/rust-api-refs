using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UtilityJobs;

namespace Facepunch.MarchingCubes;

public class MarchingCubesManager : FacepunchBehaviour
{
	private static MarchingCubesManager instance;

	private ListHashSet<IMarchingCubesTarget> allCubesList;

	private ListHashSet<IMarchingCubesTarget> generationQueue;

	private BufferList<IMarchingCubesTarget> awaitingPhysicsAssignment;

	private JobHandle physicsBakeHandle;

	private MarchingCubesGenerator[] generators;

	[ClientVar]
	[ServerVar]
	public static bool DebugLog = false;

	private static int _generatorPoolCount;

	private static int _colliderMipLevel = 1;

	public static MarchingCubesManager Instance
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			if (instance != null)
			{
				return instance;
			}
			GameObject val = new GameObject("MarchingCubeManager");
			Object.DontDestroyOnLoad((Object)val);
			instance = val.AddComponent<MarchingCubesManager>();
			return instance;
		}
	}

	[ServerVar(Default = "4", Help = "[1-16] - each generator has constant memory overhead, but will allow more to process at once")]
	[ClientVar(Default = "4", Help = "[1-16] - each generator has constant memory overhead, but will allow more to process at once")]
	public static int GeneratorPoolCount
	{
		get
		{
			return Mathf.Max(1, _generatorPoolCount);
		}
		set
		{
			_generatorPoolCount = Mathf.Clamp(value, 1, 16);
			Instance.InitGeneratorPool();
		}
	}

	[ServerVar(Default = "1", Help = "[0-2] - mip level the sculpture collision mesh is marched at. Each level is ~4x fewer collision triangles and a correspondingly cheaper physics bake, at the cost of the collider drifting slightly from the visual surface")]
	[ClientVar(Default = "1", Help = "[0-2] - mip level the sculpture collision mesh is marched at. Each level is ~4x fewer collision triangles and a correspondingly cheaper physics bake, at the cost of the collider drifting slightly from the visual surface")]
	public static int ColliderMipLevel
	{
		get
		{
			return Mathf.Clamp(_colliderMipLevel, 0, 2);
		}
		set
		{
			_colliderMipLevel = Mathf.Clamp(value, 0, 2);
		}
	}

	private void Awake()
	{
		allCubesList = new ListHashSet<IMarchingCubesTarget>();
		generationQueue = new ListHashSet<IMarchingCubesTarget>();
		awaitingPhysicsAssignment = new BufferList<IMarchingCubesTarget>();
		InitGeneratorPool();
	}

	private void InitGeneratorPool()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		DisposeGenerators();
		generators = new MarchingCubesGenerator[GeneratorPoolCount];
		for (int i = 0; i < generators.Length; i++)
		{
			generators[i] = new MarchingCubesGenerator(null, null, null, float3.zero, 0f);
		}
	}

	private void DisposeGenerators()
	{
		if (generators == null)
		{
			return;
		}
		for (int i = 0; i < generators.Length; i++)
		{
			if (generators[i] != null)
			{
				generators[i].Dispose();
			}
		}
	}

	private void OnDestroy()
	{
		DisposeGenerators();
	}

	public void Add(IMarchingCubesTarget target)
	{
		allCubesList.Add(target);
	}

	public void Remove(IMarchingCubesTarget target)
	{
		allCubesList.Remove(target);
	}

	public void FixedUpdate()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (awaitingPhysicsAssignment.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("PhysicsBakeComplete"))
		{
			((JobHandle)(ref physicsBakeHandle)).Complete();
			physicsBakeHandle = default(JobHandle);
		}
		using (TimeWarning.New("PhysicsMeshAssign"))
		{
			Enumerator<IMarchingCubesTarget> enumerator = awaitingPhysicsAssignment.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					IMarchingCubesTarget current = enumerator.Current;
					if (!((Object)(object)current.TargetMeshCollider == (Object)null))
					{
						current.TargetMeshCollider.sharedMesh = current.TargetMeshForCollision;
						if (current.TargetMeshCollider.convex != current.WantsConvexCollider)
						{
							current.TargetMeshCollider.convex = current.WantsConvexCollider;
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
		awaitingPhysicsAssignment.Clear();
	}

	public void LateUpdate()
	{
		ProcessQueue();
	}

	public void Enqueue(IMarchingCubesTarget target)
	{
		generationQueue.TryAdd(target);
	}

	private void ProcessQueue()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("FreeMemoryCheck"))
		{
			MarchingCubesGenerator[] array = generators;
			foreach (MarchingCubesGenerator marchingCubesGenerator in array)
			{
				if (marchingCubesGenerator.UsedSinceLastFree && TimeSince.op_Implicit(marchingCubesGenerator.SinceLastUse) > 10f)
				{
					int num = marchingCubesGenerator.TotalNativeMemoryUsage();
					marchingCubesGenerator.ZeroOutAllocations();
					if (DebugLog)
					{
						Debug.Log((object)$"[MARCHING CUBES] Resized generator from {(float)num / 1024f / 1024f}MB to {(float)marchingCubesGenerator.TotalNativeMemoryUsage() / 1024f / 1024f}MB");
					}
				}
			}
		}
		using (TimeWarning.New("PruneStaleTargets"))
		{
			for (int num2 = generationQueue.Count - 1; num2 >= 0; num2--)
			{
				IMarchingCubesTarget marchingCubesTarget = generationQueue[num2];
				if ((Object)(object)marchingCubesTarget.SDFSet == (Object)null || !marchingCubesTarget.SDFSet.IsCreated)
				{
					generationQueue.RemoveAt(num2);
				}
			}
		}
		if (generationQueue.Count == 0)
		{
			return;
		}
		int num3 = Mathf.Min(GeneratorPoolCount, generationQueue.Count);
		PooledList<IMarchingCubesTarget> val = Pool.Get<PooledList<IMarchingCubesTarget>>();
		try
		{
			NativeArray<JobHandle> val2 = new NativeArray<JobHandle>(num3, (Allocator)2, (NativeArrayOptions)1);
			NativeList<MeshDataArray> results = new NativeList<MeshDataArray>(num3 * 4, AllocatorHandle.op_Implicit((Allocator)2));
			NativeArray<int> val3 = new NativeArray<int>(num3, (Allocator)2, (NativeArrayOptions)1);
			NativeArray<int> val4 = new NativeArray<int>(num3, (Allocator)2, (NativeArrayOptions)1);
			bool censored = false;
			using (TimeWarning.New("SchedulingMarches"))
			{
				for (int j = 0; j < num3; j++)
				{
					IMarchingCubesTarget marchingCubesTarget2 = generationQueue.Values[j];
					MarchingCubesGenerator marchingCubesGenerator2 = generators[j];
					((List<IMarchingCubesTarget>)(object)val).Add(marchingCubesTarget2);
					marchingCubesGenerator2.Mesh = marchingCubesTarget2.TargetMesh;
					marchingCubesGenerator2.MeshForCollision = marchingCubesTarget2.TargetMeshForCollision;
					marchingCubesGenerator2.MeshCollider = marchingCubesTarget2.TargetMeshCollider;
					marchingCubesGenerator2.Offset = float3.op_Implicit(marchingCubesTarget2.VertexOffset);
					marchingCubesGenerator2.Scale = marchingCubesTarget2.VertexScale;
					val3[j] = results.Length;
					val4[j] = RenderMeshCount(marchingCubesTarget2);
					val2[j] = marchingCubesGenerator2.ScheduleMarchChain(marchingCubesTarget2.SDFSet, val4[j], ColliderMipLevel, censored, results, marchingCubesTarget2.SDFSet.ConsumeDataDependency());
				}
			}
			JobHandle.CompleteAll(val2);
			NativeArray<int> val5 = new NativeArray<int>(num3, (Allocator)3, (NativeArrayOptions)1);
			NativeArray<bool> val6 = new NativeArray<bool>(num3, (Allocator)3, (NativeArrayOptions)1);
			using (TimeWarning.New("Apply Updates"))
			{
				for (int k = 0; k < num3; k++)
				{
					IMarchingCubesTarget marchingCubesTarget3 = ((List<IMarchingCubesTarget>)(object)val)[k];
					MarchingCubesGenerator marchingCubesGenerator3 = generators[k];
					int num4 = val3[k];
					marchingCubesGenerator3.ApplyMeshData(results[num4], marchingCubesTarget3.TargetMeshForCollision);
					val5[k] = marchingCubesGenerator3.CollisionMeshInstanceId;
					val6[k] = marchingCubesTarget3.WantsConvexCollider;
					awaitingPhysicsAssignment.Add(marchingCubesTarget3);
					if (val4[k] > 0)
					{
						marchingCubesGenerator3.ApplyMeshData(results[num4 + 1], marchingCubesTarget3.TargetMesh);
						for (int l = 1; l < val4[k]; l++)
						{
							marchingCubesGenerator3.ApplyMeshData(results[num4 + 1 + l], marchingCubesTarget3.GetLodMesh(l));
						}
						marchingCubesTarget3.OnRenderMeshesUpdated();
					}
				}
			}
			using (TimeWarning.New("Schedule Physics Bake"))
			{
				physicsBakeHandle = IJobParallelForExtensions.Schedule<UtilityJobs.BakePhysicsMeshesJob>(new UtilityJobs.BakePhysicsMeshesJob
				{
					MeshIds = val5.AsReadOnly(),
					Convex = val6.AsReadOnly()
				}, num3, 1, physicsBakeHandle);
				val5.Dispose(physicsBakeHandle);
				val6.Dispose(physicsBakeHandle);
				JobHandle.ScheduleBatchedJobs();
			}
			using (TimeWarning.New("Dequeue"))
			{
				for (int m = 0; m < ((List<IMarchingCubesTarget>)(object)val).Count; m++)
				{
					generationQueue.Remove(((List<IMarchingCubesTarget>)(object)val)[m]);
				}
			}
			results.Dispose();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static int RenderMeshCount(IMarchingCubesTarget target)
	{
		if (!target.isClient)
		{
			return 0;
		}
		return MarchingCubesGenerator.ClampRenderMeshCount(1 + target.LodMeshCount);
	}
}
