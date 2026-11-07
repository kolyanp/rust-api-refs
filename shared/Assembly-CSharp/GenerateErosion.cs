using System;
using System.Threading.Tasks;
using GenerateErosionJobs;
using Ionic.Crc;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public class GenerateErosion : ProceduralComponent
{
	public struct SplatPaintingData(NativeArray<float> heightMapDelta) : IDisposable
	{
		public bool IsValid = true;

		public readonly NativeArray<float> HeightMapDelta = heightMapDelta;

		public void Dispose()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			IsValid = false;
			if (HeightMapDelta.IsCreated)
			{
				HeightMapDelta.Dispose();
			}
		}

		public void Dispose(JobHandle inputDeps)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			IsValid = false;
			if (HeightMapDelta.IsCreated)
			{
				HeightMapDelta.Dispose(inputDeps);
			}
		}
	}

	[ClientVar(Saved = true, Help = "Log a CRC32 fingerprint of the eroded heightmap after generation, for determinism validation (e.g. across platforms or client vs server)")]
	[ServerVar(Saved = true, Help = "Log a CRC32 fingerprint of the eroded heightmap after generation, for determinism validation (e.g. across platforms or client vs server)")]
	public static bool determinism_log;

	public static SplatPaintingData splatPaintingData;

	public override void Process(uint seed)
	{
		if (!World.Networked)
		{
			GridErosion(seed);
		}
	}

	private static int GetBatchSize(int length)
	{
		int num = length / JobsUtility.JobWorkerCount;
		if (num < 64)
		{
			return 64;
		}
		return num;
	}

	private void GridErosion(uint seed)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0525: Unknown result type (might be due to invalid IL or missing references)
		//IL_0527: Unknown result type (might be due to invalid IL or missing references)
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0538: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0543: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		//IL_0556: Unknown result type (might be due to invalid IL or missing references)
		//IL_0582: Unknown result type (might be due to invalid IL or missing references)
		//IL_0584: Unknown result type (might be due to invalid IL or missing references)
		//IL_0589: Unknown result type (might be due to invalid IL or missing references)
		//IL_058b: Unknown result type (might be due to invalid IL or missing references)
		//IL_058d: Unknown result type (might be due to invalid IL or missing references)
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0591: Unknown result type (might be due to invalid IL or missing references)
		//IL_0593: Unknown result type (might be due to invalid IL or missing references)
		//IL_0595: Unknown result type (might be due to invalid IL or missing references)
		//IL_0597: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		//IL_059b: Unknown result type (might be due to invalid IL or missing references)
		//IL_059d: Unknown result type (might be due to invalid IL or missing references)
		//IL_059f: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_0615: Unknown result type (might be due to invalid IL or missing references)
		//IL_0620: Unknown result type (might be due to invalid IL or missing references)
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		//IL_0633: Unknown result type (might be due to invalid IL or missing references)
		//IL_0638: Unknown result type (might be due to invalid IL or missing references)
		//IL_0641: Unknown result type (might be due to invalid IL or missing references)
		//IL_0646: Unknown result type (might be due to invalid IL or missing references)
		//IL_064d: Unknown result type (might be due to invalid IL or missing references)
		//IL_064f: Unknown result type (might be due to invalid IL or missing references)
		//IL_066d: Unknown result type (might be due to invalid IL or missing references)
		//IL_066f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0674: Unknown result type (might be due to invalid IL or missing references)
		//IL_0678: Unknown result type (might be due to invalid IL or missing references)
		//IL_067a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0682: Unknown result type (might be due to invalid IL or missing references)
		//IL_0684: Unknown result type (might be due to invalid IL or missing references)
		//IL_068c: Unknown result type (might be due to invalid IL or missing references)
		//IL_068e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0696: Unknown result type (might be due to invalid IL or missing references)
		//IL_0698: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06be: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06de: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0705: Unknown result type (might be due to invalid IL or missing references)
		//IL_0718: Unknown result type (might be due to invalid IL or missing references)
		//IL_071f: Expected Obj, but got Unknown
		//IL_0726: Unknown result type (might be due to invalid IL or missing references)
		//IL_072b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GridErosion"))
		{
			TerrainHeightMap heightMap = TerrainMeta.HeightMap;
			heightMap.Push();
			NativeArray<short> src = heightMap.src;
			NativeArray<short> dst = heightMap.dst;
			NativeArray<float> minTerrainHeightMap = new NativeArray<float>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)0);
			NativeArray<float> waterMap = new NativeArray<float>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)1);
			NativeList<int> val = new NativeList<int>(heightMap.src.Length, AllocatorHandle.op_Implicit((Allocator)4));
			NativeArray<float> fluxLeft = new NativeArray<float>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)1);
			NativeArray<float> fluxRight = new NativeArray<float>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)1);
			NativeArray<float> fluxTop = new NativeArray<float>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)1);
			NativeArray<float> fluxBottom = new NativeArray<float>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)1);
			NativeArray<float2> velocityMap = new NativeArray<float2>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)1);
			NativeArray<float> val2 = new NativeArray<float>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)1);
			NativeArray<float> val3 = new NativeArray<float>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)1);
			Debug.Assert(waterMap.Length == heightMap.src.Length);
			Debug.Assert(fluxLeft.Length == heightMap.src.Length);
			Debug.Assert(velocityMap.Length == heightMap.src.Length);
			Debug.Assert(val2.Length == heightMap.src.Length);
			float num = TerrainMeta.Size.x / (float)heightMap.res * TerrainMeta.Size.z / (float)heightMap.res;
			float invGridCellSquareSize = 1f / num;
			float pipeLength = 1f;
			float pipeArea = 1f;
			JobHandle val4 = default;
			NativeArray<float> val5 = new NativeArray<float>(src.Length, (Allocator)4, (NativeArrayOptions)1);
			NativeArray<float> val6 = new NativeArray<float>(src.Length, (Allocator)4, (NativeArrayOptions)1);
			val4 = IJobParallelForBatchExtensions.Schedule<PrepareMapJob>(new PrepareMapJob
			{
				HeightMapAsShort = src.AsReadOnly(),
				HeightMapAsFloat = val5,
				OceanIndicesWriter = val.AsParallelWriter(),
				OceanLevel = WaterSystem.OceanLevel,
				TerrainPositionY = TerrainMeta.Position.y,
				TerrainSizeY = TerrainMeta.Size.y
			}, src.Length, GetBatchSize(src.Length), val4);
			int num2 = Mathf.Max(1, heightMap.res / JobsUtility.JobWorkerCount);
			val4 = IJobParallelForExtensions.Schedule<CalcMinHeightMapJob>(new CalcMinHeightMapJob
			{
				TerrainHeightMap = val5.AsReadOnly(),
				MinTerrainHeightMap = minTerrainHeightMap,
				HeightMapRes = TerrainMeta.HeightMap.res,
				TopologyMap = TerrainMeta.TopologyMap.GetHierarchyQuery(),
				OceanHeight = WaterSystem.OceanLevel,
				TerrainOneOverSizeX = TerrainMeta.OneOverSize.x
			}, heightMap.res, num2, val4);
			val4.Complete();
			val4 = default;
			NativeArray<float> copyTarget = new NativeArray<float>(src.Length, (Allocator)4, (NativeArrayOptions)1);
			val4 = IJobExtensions.Schedule<CopyArrayJob<float>>(new CopyArrayJob<float>
			{
				CopyTarget = copyTarget,
				CopySource = val5
			}, val4);
			for (int i = 0; i < 512; i++)
			{
				RefillOceanJob refillOceanJob = new RefillOceanJob
				{
					OceanIndices = val.AsReadOnly(),
					HeightMap = val5.AsReadOnly(),
					OceanLevel = WaterSystem.OceanLevel,
					WaterMap = waterMap
				};
				val4 = IJobParallelForExtensions.ScheduleByRef<RefillOceanJob>(ref refillOceanJob, val.Length, GetBatchSize(val.Length), val4);
				CalculateOutputFluxJob calculateOutputFluxJob = new CalculateOutputFluxJob
				{
					TerrainHeightMapFloatVal = val5.AsReadOnly(),
					WaterMap = waterMap.AsReadOnly(),
					FluxLeft = fluxLeft,
					FluxRight = fluxRight,
					FluxTop = fluxTop,
					FluxBottom = fluxBottom,
					Res = heightMap.res,
					DT = 0.06f,
					GridCellSquareSize = num,
					PipeLength = pipeLength,
					PipeArea = pipeArea,
					WaterFill = 0.0023999999f
				};
				val4 = IJobParallelForExtensions.ScheduleByRef<CalculateOutputFluxJob>(ref calculateOutputFluxJob, heightMap.res, num2, val4);
				AdjustWaterHeightByFluxJob adjustWaterHeightByFluxJob = new AdjustWaterHeightByFluxJob
				{
					WaterMap = waterMap,
					VelocityMap = velocityMap,
					FluxLeft = fluxLeft.AsReadOnly(),
					FluxRight = fluxRight.AsReadOnly(),
					FluxTop = fluxTop.AsReadOnly(),
					FluxBottom = fluxBottom.AsReadOnly(),
					Res = heightMap.res,
					DT = 0.06f,
					InvGridCellSquareSize = invGridCellSquareSize,
					WaterFill = 0.0023999999f
				};
				val4 = IJobParallelForExtensions.ScheduleByRef<AdjustWaterHeightByFluxJob>(ref adjustWaterHeightByFluxJob, heightMap.res, num2, val4);
				ErosionAndDepositionJob erosionAndDepositionJob = new ErosionAndDepositionJob
				{
					SedimentMap = val2,
					MinTerrainHeightMap = minTerrainHeightMap.AsReadOnly(),
					TerrainHeightMapSrcFloat = val5.AsReadOnly(),
					TerrainHeightMapDstFloat = val6,
					WaterMap = waterMap,
					VelocityMap = velocityMap.AsReadOnly(),
					DT = 0.06f,
					NormY = heightMap.normY,
					Res = heightMap.res,
					EvapFactor = 0.9991f
				};
				val4 = IJobParallelForExtensions.ScheduleByRef<ErosionAndDepositionJob>(ref erosionAndDepositionJob, heightMap.res, num2, val4);
				TransportSedimentJob transportSedimentJob = new TransportSedimentJob
				{
					SedimentMap = val3,
					SedimentReadOnlyMap = val2.AsReadOnly(),
					VelocityMap = velocityMap.AsReadOnly(),
					Res = heightMap.res,
					DT = 0.06f
				};
				val4 = IJobParallelForExtensions.ScheduleByRef<TransportSedimentJob>(ref transportSedimentJob, heightMap.res, num2, val4);
				NativeArray<float> val7 = val6;
				NativeArray<float> val8 = val5;
				val5 = val7;
				val6 = val8;
				NativeArray<float> val9 = val3;
				val8 = val2;
				val2 = val9;
				val3 = val8;
			}
			CopyBackFloatHeightToShortHeightJob copyBackFloatHeightToShortHeightJob = new CopyBackFloatHeightToShortHeightJob
			{
				HeightMapAsFloat = val5.AsReadOnly(),
				HeightMapAsShort = dst,
				TerrainOneOverSizeY = TerrainMeta.OneOverSize.y,
				TerrainPositionY = TerrainMeta.Position.y
			};
			val4 = IJobParallelForExtensions.ScheduleByRef<CopyBackFloatHeightToShortHeightJob>(ref copyBackFloatHeightToShortHeightJob, val5.Length, GetBatchSize(val5.Length), val4);
			NativeArray<float> val10 = new NativeArray<float>(val5.Length, (Allocator)4, (NativeArrayOptions)1);
			PopulateDeltaHeightJob populateDeltaHeightJob = new PopulateDeltaHeightJob
			{
				HeightMapOriginal = copyTarget.AsReadOnly(),
				HeightMap = val5.AsReadOnly(),
				DeltaHeightMap = val10
			};
			val4 = IJobParallelForExtensions.ScheduleByRef<PopulateDeltaHeightJob>(ref populateDeltaHeightJob, val10.Length, GetBatchSize(val10.Length), val4);
			minTerrainHeightMap.Dispose(val4);
			waterMap.Dispose(val4);
			fluxLeft.Dispose(val4);
			fluxRight.Dispose(val4);
			fluxTop.Dispose(val4);
			fluxBottom.Dispose(val4);
			velocityMap.Dispose(val4);
			val2.Dispose(val4);
			val3.Dispose(val4);
			val5.Dispose(val4);
			val6.Dispose(val4);
			val.Dispose(val4);
			copyTarget.Dispose(val4);
			val4.Complete();
			heightMap.Pop();
			splatPaintingData = new SplatPaintingData(val10);
			if (determinism_log)
			{
				CRC32 val11 = new CRC32();
				byte[] array = heightMap.src.Reinterpret<byte>(2).ToArray();
				val11.SlurpBlock(array, 0, array.Length);
				Debug.Log((object)$"[GridErosion] fingerprint=0x{(uint)val11.Crc32Result:X8} seed={seed} res={heightMap.res}");
			}
		}
	}

	private void OnDestroy()
	{
		if (splatPaintingData.IsValid)
		{
			splatPaintingData.Dispose();
		}
	}

	private void OldErosion(uint seed)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		TerrainTopologyMap topologyMap = TerrainMeta.TopologyMap;
		TerrainHeightMap heightmap = TerrainMeta.HeightMap;
		TerrainSplatMap splatmap = TerrainMeta.SplatMap;
		int erosion_res = heightmap.res;
		float[] erosion = new float[erosion_res * erosion_res];
		int deposit_res = splatmap.res;
		float[] deposit = new float[deposit_res * deposit_res];
		for (float num = TerrainMeta.Position.z; num < TerrainMeta.Position.z + TerrainMeta.Size.z; num += 10f)
		{
			for (float num2 = TerrainMeta.Position.x; num2 < TerrainMeta.Position.x + TerrainMeta.Size.x; num2 += 10f)
			{
				Vector3 val = new Vector3(num2, 0f, num);
				float num3 = (val.y = heightmap.GetHeight(val));
				if (val.y <= 15f)
				{
					continue;
				}
				Vector3 normal = heightmap.GetNormal(val);
				if (normal.y <= 0.01f || normal.y >= 0.99f)
				{
					continue;
				}
				Vector2 val2 = Vector3Ex.XZ2D(normal);
				Vector2 normalized = val2.normalized;
				Vector2 val3 = normalized;
				float num4 = 0f;
				float num5 = 0f;
				for (int i = 0; i < 300; i++)
				{
					val.x += normalized.x;
					val.z += normalized.y;
					if (Vector3.Angle(Vector2.op_Implicit(normalized), Vector2.op_Implicit(val3)) > 90f)
					{
						break;
					}
					float num6 = TerrainMeta.NormalizeX(val.x);
					float num7 = TerrainMeta.NormalizeZ(val.z);
					int topology = topologyMap.GetTopology(num6, num7);
					if ((topology & 0xB4990) != 0)
					{
						break;
					}
					float height = heightmap.GetHeight(num6, num7);
					if (height > num3 + 8f)
					{
						break;
					}
					float num8 = Mathf.Min(height, num3);
					val.y = Mathf.Lerp(val.y, num8, 0.5f);
					normal = heightmap.GetNormal(val);
					Vector2 val4 = normalized;
					val2 = Vector3Ex.XZ2D(normal);
					val2 = Vector2.Lerp(val4, val2.normalized, 0.5f);
					normalized = val2.normalized;
					num3 = num8;
					float num9 = 0f;
					float num10 = 0f;
					if ((topology & 0x800400) == 0)
					{
						float num11 = Vector3.Angle(Vector3.up, normal);
						num9 = Mathf.InverseLerp(5f, 15f, num11);
						num10 = 1f;
						if ((topology & 0x8000) == 0)
						{
							num10 = num9;
						}
					}
					num4 = Mathf.MoveTowards(num4, num9, 0.05f);
					num5 = Mathf.MoveTowards(num5, num10, 0.05f);
					if ((topologyMap.GetTopology(num6, num7, 10f) & 2) == 0)
					{
						int num12 = Mathf.Clamp((int)(num6 * (float)erosion_res), 0, erosion_res - 1);
						int num13 = Mathf.Clamp((int)(num7 * (float)erosion_res), 0, erosion_res - 1);
						int num14 = Mathf.Clamp((int)(num6 * (float)deposit_res), 0, deposit_res - 1);
						int num15 = Mathf.Clamp((int)(num7 * (float)deposit_res), 0, deposit_res - 1);
						erosion[num13 * erosion_res + num12] += num4;
						deposit[num15 * deposit_res + num14] += num5;
					}
				}
			}
		}
		Parallel.For(1, erosion_res - 1, (int z) =>
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			for (int j = 1; j < erosion_res - 1; j++)
			{
				float num16 = CalculateDelta(erosion, erosion_res, j, z, 1f, 0.8f, 0.6f);
				float delta = (0f - Mathf.Lerp(0f, 0.25f, num16)) * TerrainMeta.OneOverSize.y;
				heightmap.AddHeight(j, z, delta);
			}
		});
		Parallel.For(1, deposit_res - 1, (int z) =>
		{
			for (int j = 1; j < deposit_res - 1; j++)
			{
				float splat = splatmap.GetSplat(j, z, 2);
				float splat2 = splatmap.GetSplat(j, z, 4);
				if (splat > 0.1f || splat2 > 0.1f)
				{
					float num16 = CalculateDelta(deposit, deposit_res, j, z, 1f, 0.4f, 0.2f);
					num16 = Mathf.InverseLerp(1f, 3f, num16);
					num16 = Mathf.Lerp(0f, 0.5f, num16);
					splatmap.AddSplat(j, z, 128, num16);
				}
				else
				{
					float num17 = CalculateDelta(deposit, deposit_res, j, z, 1f, 0.2f, 0.1f);
					float num18 = CalculateDelta(deposit, deposit_res, j, z, 1f, 0.8f, 0.4f);
					num17 = Mathf.InverseLerp(1f, 3f, num17);
					num18 = Mathf.InverseLerp(1f, 3f, num18);
					num17 = Mathf.Lerp(0f, 1f, num17);
					num18 = Mathf.Lerp(0f, 1f, num18);
					splatmap.AddSplat(j, z, 1, num18 * 0.5f);
					splatmap.AddSplat(j, z, 64, num17 * 0.7f);
					splatmap.AddSplat(j, z, 128, num17 * 0.5f);
				}
			}
		});
		static float CalculateDelta(float[] data, int res, int x, int z, float cntr, float side, float diag)
		{
			int num16 = x - 1;
			int num17 = x + 1;
			int num18 = z - 1;
			int num19 = z + 1;
			side /= 4f;
			diag /= 4f;
			float num20 = data[z * res + x];
			float num21 = data[z * res + num16] + data[z * res + num17] + data[num19 * res + x] + data[num19 * res + x];
			float num22 = data[num18 * res + num16] + data[num18 * res + num17] + data[num19 * res + num16] + data[num19 * res + num17];
			return cntr * num20 + side * num21 + diag * num22;
		}
	}
}
