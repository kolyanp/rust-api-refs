using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Facepunch.MarchingCubes;

public class MarchingCubeManager : FacepunchBehaviour
{
	private static MarchingCubeManager _instance;

	public List<MarchingCubesGenerator> AllCubesList;

	private ListHashSet<MarchingCubesGenerator> _cubesWaitingForGeneration;

	private BufferList<MarchingCubesGenerator> _toAssignPhysics;

	private JobHandle _physicsBakeHandle;

	public static MarchingCubeManager Instance
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			if (_instance != null)
			{
				return _instance;
			}
			GameObject val = new GameObject("MarchingCubeManager");
			Object.DontDestroyOnLoad((Object)val);
			_instance = val.AddComponent<MarchingCubeManager>();
			return _instance;
		}
	}

	public void Awake()
	{
		AllCubesList = new List<MarchingCubesGenerator>();
		_cubesWaitingForGeneration = new ListHashSet<MarchingCubesGenerator>();
		_toAssignPhysics = new BufferList<MarchingCubesGenerator>();
	}

	public void FixedUpdate()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (_toAssignPhysics.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("PhysicsBakeComplete"))
		{
			((JobHandle)(ref _physicsBakeHandle)).Complete();
			_physicsBakeHandle = default(JobHandle);
		}
		using (TimeWarning.New("PhysicsMeshAssign"))
		{
			Enumerator<MarchingCubesGenerator> enumerator = _toAssignPhysics.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MarchingCubesGenerator current = enumerator.Current;
					current.MeshCollider.sharedMesh = current.Mesh;
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
		_toAssignPhysics.Clear();
	}

	public void LateUpdate()
	{
		ProcessQueue();
	}

	public void Add(MarchingCubesGenerator cubes)
	{
		AllCubesList.Add(cubes);
	}

	public void Remove(MarchingCubesGenerator cubes)
	{
		AllCubesList.Remove(cubes);
	}

	public void EnqueueUpdate(MarchingCubesGenerator cubes)
	{
		_cubesWaitingForGeneration.TryAdd(cubes);
	}

	private void ProcessQueue()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		if (_cubesWaitingForGeneration.Count != 0)
		{
			int count = _cubesWaitingForGeneration.Count;
			NativeArray<JobHandle> val = default(NativeArray<JobHandle>);
			val._002Ector(count, (Allocator)2, (NativeArrayOptions)1);
			for (int i = 0; i < count; i++)
			{
				val[i] = _cubesWaitingForGeneration[i].ScheduleMarch();
			}
			JobHandle.CompleteAll(val);
			NativeArray<int> meshIds = default(NativeArray<int>);
			meshIds._002Ector(_cubesWaitingForGeneration.Count, (Allocator)3, (NativeArrayOptions)1);
			for (int j = 0; j < count; j++)
			{
				_cubesWaitingForGeneration[j].ApplyUpdate();
				meshIds[j] = _cubesWaitingForGeneration[j].MeshInstanceId;
				_toAssignPhysics.Add(_cubesWaitingForGeneration[j]);
			}
			_physicsBakeHandle = IJobParallelForExtensions.Schedule<Facepunch.MarchingCubes.BakePhysicsMeshesJob>(new Facepunch.MarchingCubes.BakePhysicsMeshesJob
			{
				MeshIds = meshIds
			}, count, 1, _physicsBakeHandle);
			meshIds.Dispose(_physicsBakeHandle);
			_cubesWaitingForGeneration.Clear();
		}
	}
}
