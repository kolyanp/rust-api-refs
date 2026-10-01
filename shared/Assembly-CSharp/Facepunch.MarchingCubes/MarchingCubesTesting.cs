using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Facepunch.MarchingCubes;

public class MarchingCubesTesting : FacepunchBehaviour, IDisposable
{
	public MeshFilter TargetFilter;

	public MeshCollider TargetCollider;

	public Vector3Int GridResolution = new Vector3Int(32, 40, 32);

	public Vector3 GridOffset = new Vector3(0f, -20f, 0f);

	public float GridScale = 1f / 32f;

	private Mesh _mesh;

	private MarchingCubesGenerator _generator;

	private bool _hasInit;

	public void Init()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected Obj, but got Unknown
		if ((Object)(object)_mesh == (Object)null)
		{
			_mesh = new Mesh
			{
				name = "MarchingCubesTestingMesh"
			};
		}
		TargetFilter.sharedMesh = _mesh;
		_generator = new MarchingCubesGenerator(_mesh, _mesh, null, float3.op_Implicit(GridOffset), GridScale);
	}

	public void RegenerateWithFloats(SDFSet set, float iso, Stopwatch sw = null)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		Init();
		set.CompleteDataJobs();
		NativeList<float3> vertices = default;
		NativeList<int> indices = default;
		sw?.Restart();
		JobHandle inputDeps;
		foreach (SDFChunk chunk in set.Chunks)
		{
			if (vertices.IsCreated)
			{
				vertices.Dispose();
			}
			if (indices.IsCreated)
			{
				indices.Dispose();
			}
			MarchingCubesGenerator generator = _generator;
			QuantizedFloatData3DArray dataArray = chunk.DataArray;
			inputDeps = default;
			inputDeps = generator.ScheduleSDFMarch(dataArray, iso, out vertices, out indices, inputDeps);
			inputDeps.Complete();
		}
		sw?.Stop();
		MarchingCubesGenerator generator2 = _generator;
		NativeList<float3> vertices2 = vertices;
		NativeList<int> indices2 = indices;
		inputDeps = default;
		inputDeps = generator2.ScheduleMeshWrite(vertices2, indices2, out var meshData, withNormals: true, inputDeps);
		inputDeps.Complete();
		_generator.ApplyMeshData(meshData, _generator.Mesh);
		TargetCollider.sharedMesh = _mesh;
		vertices.Dispose();
		indices.Dispose();
		Debug.Log((object)$"mesh v: {_mesh.vertexCount} t: {_mesh.triangles.Length / 3}");
		Dispose();
	}

	public void Dispose()
	{
		_generator.Dispose();
	}

	public MarchingCubesTesting()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
	}
}
