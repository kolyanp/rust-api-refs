using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CompanionServer.Cameras;

public class CameraRenderTask : CustomYieldInstruction, IDisposable
{
	public const int MaxSamplesPerRender = 10000;

	public const int MaxColliders = 512;

	private static readonly Dictionary<(int, int), NativeArray<int2>> _samplePositions = new Dictionary<(int, int), NativeArray<int2>>();

	private NativeArray<RaycastCommand> _raycastCommands;

	private NativeArray<RaycastHit> _raycastHits;

	private NativeArray<int> _colliderIds;

	private NativeArray<byte> _colliderMaterials;

	private NativeArray<int> _colliderHits;

	private NativeArray<int> _raycastOutput;

	private NativeArray<int> _foundCollidersLength;

	private NativeArray<int> _foundColliders;

	private NativeArray<int> _outputDataLength;

	private NativeArray<byte> _outputData;

	private NativeArray<int> _emptyTopology;

	private JobHandle? _pendingJob;

	private int _sampleCount;

	private int _colliderLength;

	public override bool keepWaiting
	{
		get
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (_pendingJob.HasValue)
			{
				JobHandle value = _pendingJob.Value;
				return !((JobHandle)(ref value)).IsCompleted;
			}
			return false;
		}
	}

	public CameraRenderTask()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		_raycastCommands = new NativeArray<RaycastCommand>(10000, (Allocator)4, (NativeArrayOptions)1);
		_raycastHits = new NativeArray<RaycastHit>(10000, (Allocator)4, (NativeArrayOptions)0);
		_colliderIds = new NativeArray<int>(512, (Allocator)4, (NativeArrayOptions)0);
		_colliderMaterials = new NativeArray<byte>(512, (Allocator)4, (NativeArrayOptions)0);
		_colliderHits = new NativeArray<int>(512, (Allocator)4, (NativeArrayOptions)0);
		_raycastOutput = new NativeArray<int>(10000, (Allocator)4, (NativeArrayOptions)0);
		_foundCollidersLength = new NativeArray<int>(1, (Allocator)4, (NativeArrayOptions)0);
		_foundColliders = new NativeArray<int>(10000, (Allocator)4, (NativeArrayOptions)0);
		_outputDataLength = new NativeArray<int>(1, (Allocator)4, (NativeArrayOptions)0);
		_outputData = new NativeArray<byte>(40000, (Allocator)4, (NativeArrayOptions)0);
		_emptyTopology = new NativeArray<int>(1, (Allocator)4, (NativeArrayOptions)1);
	}

	~CameraRenderTask()
	{
		try
		{
			Dispose();
		}
		finally
		{
			((object)this).Finalize();
		}
	}

	public void Dispose()
	{
		_raycastCommands.Dispose();
		_raycastHits.Dispose();
		_colliderIds.Dispose();
		_colliderMaterials.Dispose();
		_colliderHits.Dispose();
		_raycastOutput.Dispose();
		_foundCollidersLength.Dispose();
		_foundColliders.Dispose();
		_outputDataLength.Dispose();
		_outputData.Dispose();
		_emptyTopology.Dispose();
	}

	public void Reset()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (_pendingJob.HasValue)
		{
			JobHandle value = _pendingJob.Value;
			if (!((JobHandle)(ref value)).IsCompleted)
			{
				Debug.LogWarning((object)"CameraRenderTask is resetting before completion! This will cause it to synchronously block for completion.");
			}
			value = _pendingJob.Value;
			((JobHandle)(ref value)).Complete();
		}
		_pendingJob = null;
		_sampleCount = 0;
	}

	public int Start(int width, int height, float verticalFov, float nearPlane, float farPlane, int layerMask, in Matrix4x4 transf, int sampleCount, int sampleOffset, Dictionary<int, (byte MaterialIndex, int Age)> knownColliders)
	{
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		if (sampleCount <= 0 || sampleCount > 10000)
		{
			throw new ArgumentOutOfRangeException("sampleCount");
		}
		if (sampleOffset < 0)
		{
			throw new ArgumentOutOfRangeException("sampleOffset");
		}
		if (knownColliders == null)
		{
			throw new ArgumentNullException("knownColliders");
		}
		if (knownColliders.Count > 512)
		{
			throw new ArgumentException("Too many colliders", "knownColliders");
		}
		if (_pendingJob.HasValue)
		{
			throw new InvalidOperationException("A render job was already started for this instance.");
		}
		_sampleCount = sampleCount;
		_colliderLength = knownColliders.Count;
		int num = 0;
		foreach (KeyValuePair<int, (byte, int)> knownCollider in knownColliders)
		{
			_colliderIds[num] = knownCollider.Key;
			_colliderMaterials[num] = knownCollider.Value.Item1;
			num++;
		}
		NativeArray<int2> samplePositions = GetSamplePositions(width, height);
		_foundCollidersLength[0] = 0;
		RaycastBufferSetupJob raycastBufferSetupJob = new RaycastBufferSetupJob
		{
			colliderIds = _colliderIds.GetSubArray(0, _colliderLength),
			colliderMaterials = _colliderMaterials.GetSubArray(0, _colliderLength),
			colliderHits = _colliderHits.GetSubArray(0, _colliderLength)
		};
		Vector3 position = ((Matrix4x4)(ref transf)).GetPosition();
		Quaternion rotation = ((Matrix4x4)(ref transf)).rotation;
		RaycastRaySetupJob raycastRaySetupJob = new RaycastRaySetupJob
		{
			res = new float2((float)width, (float)height),
			halfRes = new float2((float)width / 2f, (float)height / 2f),
			aspectRatio = (float)width / (float)height,
			worldHeight = 2f * Mathf.Tan(MathF.PI / 360f * verticalFov),
			cameraPos = float3.op_Implicit(position),
			cameraRot = quaternion.op_Implicit(rotation),
			nearPlane = nearPlane,
			farPlane = farPlane,
			layerMask = layerMask,
			samplePositions = samplePositions,
			sampleOffset = sampleOffset % samplePositions.Length,
			raycastCommands = _raycastCommands.GetSubArray(0, sampleCount)
		};
		float oceanLevel = WaterSystem.OceanLevel;
		TerrainTopologyMap topologyMap = TerrainMeta.TopologyMap;
		bool num2 = (Object)(object)topologyMap != (Object)null;
		TerrainTopologyMap.TopologyQueryStructure topologyQueryStructure = (num2 ? topologyMap.GetQueryStructure() : default(TerrainTopologyMap.TopologyQueryStructure));
		bool flag = num2 && topologyQueryStructure.source.IsCreated && position.y > oceanLevel;
		Vector3 val = rotation * Vector3.forward;
		RaycastRayProcessingJob raycastRayProcessingJob = new RaycastRayProcessingJob
		{
			cameraForward = float3.op_Implicit(-val),
			farPlane = farPlane,
			oceanEnabled = flag,
			oceanLevel = oceanLevel,
			oceanTopologyMask = 384,
			topologyRes = ((!flag) ? 1 : topologyQueryStructure.res),
			topologyOrigin = new float2(TerrainMeta.Position.x, TerrainMeta.Position.z),
			topologyOneOverSize = new float2(TerrainMeta.OneOverSize.x, TerrainMeta.OneOverSize.z),
			topology = (flag ? topologyQueryStructure.source : _emptyTopology.AsReadOnly()),
			raycastCommands = _raycastCommands.GetSubArray(0, sampleCount),
			raycastHits = _raycastHits.GetSubArray(0, sampleCount),
			colliderIds = _colliderIds.GetSubArray(0, _colliderLength),
			colliderMaterials = _colliderMaterials.GetSubArray(0, _colliderLength),
			colliderHits = _colliderHits.GetSubArray(0, _colliderLength),
			outputs = _raycastOutput.GetSubArray(0, sampleCount),
			foundCollidersIndex = _foundCollidersLength,
			foundColliders = _foundColliders
		};
		RaycastColliderProcessingJob raycastColliderProcessingJob = new RaycastColliderProcessingJob
		{
			foundCollidersLength = _foundCollidersLength,
			foundColliders = _foundColliders
		};
		RaycastOutputCompressJob obj = new RaycastOutputCompressJob
		{
			rayOutputs = _raycastOutput.GetSubArray(0, sampleCount),
			dataLength = _outputDataLength,
			data = _outputData
		};
		JobHandle val2 = IJobExtensions.Schedule<RaycastBufferSetupJob>(raycastBufferSetupJob, default(JobHandle));
		JobHandle val3 = IJobParallelForExtensions.Schedule<RaycastRaySetupJob>(raycastRaySetupJob, sampleCount, 100, default(JobHandle));
		JobHandle val4 = RaycastCommand.ScheduleBatch(_raycastCommands.GetSubArray(0, sampleCount), _raycastHits.GetSubArray(0, sampleCount), 100, val3);
		JobHandle val5 = IJobParallelForExtensions.Schedule<RaycastRayProcessingJob>(raycastRayProcessingJob, sampleCount, 100, JobHandle.CombineDependencies(val2, val4));
		JobHandle val6 = IJobExtensions.Schedule<RaycastColliderProcessingJob>(raycastColliderProcessingJob, val5);
		JobHandle val7 = IJobExtensions.Schedule<RaycastOutputCompressJob>(obj, val5);
		_pendingJob = JobHandle.CombineDependencies(val7, val6);
		return sampleOffset + sampleCount;
	}

	public int ExtractRayData(byte[] buffer, List<int> hitColliderIds = null, List<int> foundColliderIds = null)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		int num = _sampleCount * 4;
		if (buffer.Length < num)
		{
			throw new ArgumentException("Output buffer is not large enough to hold all the ray data", "buffer");
		}
		if (!_pendingJob.HasValue)
		{
			throw new InvalidOperationException("Job was not started for this CameraRenderTask");
		}
		JobHandle value = _pendingJob.Value;
		if (!((JobHandle)(ref value)).IsCompleted)
		{
			Debug.LogWarning((object)"Trying to extract ray data from CameraRenderTask before completion! This will cause it to synchronously block for completion.");
		}
		value = _pendingJob.Value;
		((JobHandle)(ref value)).Complete();
		int num2 = _outputDataLength[0];
		NativeArray<byte>.Copy(_outputData.GetSubArray(0, num2), buffer, num2);
		if (hitColliderIds != null)
		{
			hitColliderIds.Clear();
			for (int i = 0; i < _colliderLength; i++)
			{
				if (_colliderHits[i] > 0)
				{
					hitColliderIds.Add(_colliderIds[i]);
				}
			}
		}
		if (foundColliderIds != null)
		{
			foundColliderIds.Clear();
			int num3 = _foundCollidersLength[0];
			for (int j = 0; j < num3; j++)
			{
				foundColliderIds.Add(_foundColliders[j]);
			}
		}
		return num2;
	}

	private static NativeArray<int2> GetSamplePositions(int width, int height)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (width <= 0)
		{
			throw new ArgumentOutOfRangeException("width");
		}
		if (height <= 0)
		{
			throw new ArgumentOutOfRangeException("height");
		}
		(int, int) key = (width, height);
		if (_samplePositions.TryGetValue(key, out var value))
		{
			return value;
		}
		value._002Ector(width * height, (Allocator)4, (NativeArrayOptions)0);
		IJobExtensions.Run<RaycastSamplePositionsJob>(new RaycastSamplePositionsJob
		{
			res = new int2(width, height),
			random = new Random(1337u),
			positions = value
		});
		_samplePositions.Add(key, value);
		return value;
	}

	public static void FreeCachedSamplePositions()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<(int, int), NativeArray<int2>> samplePosition in _samplePositions)
		{
			samplePosition.Value.Dispose();
		}
		_samplePositions.Clear();
	}
}
