using System;
using System.Threading.Tasks;
using GenerateErosionJobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public class GenerateErosion : ProceduralComponent
{
	public struct SplatPaintingData : IDisposable
	{
		public bool IsValid;

		public readonly NativeArray<float> HeightMapDelta;

		public readonly NativeArray<float> AngleMap;

		public SplatPaintingData(NativeArray<float> heightMapDelta, NativeArray<float> angleMap)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			HeightMapDelta = heightMapDelta;
			AngleMap = angleMap;
			IsValid = true;
		}

		public void Dispose()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			IsValid = false;
			if (HeightMapDelta.IsCreated)
			{
				HeightMapDelta.Dispose();
			}
			if (AngleMap.IsCreated)
			{
				AngleMap.Dispose();
			}
		}

		public void Dispose(JobHandle inputDeps)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			IsValid = false;
			if (HeightMapDelta.IsCreated)
			{
				HeightMapDelta.Dispose(inputDeps);
			}
			if (AngleMap.IsCreated)
			{
				AngleMap.Dispose(inputDeps);
			}
		}
	}

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
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0461: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0541: Unknown result type (might be due to invalid IL or missing references)
		//IL_054d: Unknown result type (might be due to invalid IL or missing references)
		//IL_054f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0558: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0566: Unknown result type (might be due to invalid IL or missing references)
		//IL_056b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0572: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_057b: Unknown result type (might be due to invalid IL or missing references)
		//IL_057d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0586: Unknown result type (might be due to invalid IL or missing references)
		//IL_058b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0594: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0601: Unknown result type (might be due to invalid IL or missing references)
		//IL_0608: Unknown result type (might be due to invalid IL or missing references)
		//IL_060a: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Unknown result type (might be due to invalid IL or missing references)
		//IL_061f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0628: Unknown result type (might be due to invalid IL or missing references)
		//IL_062d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0636: Unknown result type (might be due to invalid IL or missing references)
		//IL_063b: Unknown result type (might be due to invalid IL or missing references)
		//IL_065f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0661: Unknown result type (might be due to invalid IL or missing references)
		//IL_067b: Unknown result type (might be due to invalid IL or missing references)
		//IL_067d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0682: Unknown result type (might be due to invalid IL or missing references)
		//IL_0687: Unknown result type (might be due to invalid IL or missing references)
		//IL_0693: Unknown result type (might be due to invalid IL or missing references)
		//IL_0695: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0706: Unknown result type (might be due to invalid IL or missing references)
		//IL_0717: Unknown result type (might be due to invalid IL or missing references)
		//IL_073f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0741: Unknown result type (might be due to invalid IL or missing references)
		//IL_0746: Unknown result type (might be due to invalid IL or missing references)
		//IL_0751: Unknown result type (might be due to invalid IL or missing references)
		//IL_0756: Unknown result type (might be due to invalid IL or missing references)
		//IL_0764: Unknown result type (might be due to invalid IL or missing references)
		//IL_0769: Unknown result type (might be due to invalid IL or missing references)
		//IL_0772: Unknown result type (might be due to invalid IL or missing references)
		//IL_0777: Unknown result type (might be due to invalid IL or missing references)
		//IL_077e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0780: Unknown result type (might be due to invalid IL or missing references)
		//IL_079e: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07db: Unknown result type (might be due to invalid IL or missing references)
		//IL_07dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0803: Unknown result type (might be due to invalid IL or missing references)
		//IL_0805: Unknown result type (might be due to invalid IL or missing references)
		//IL_0818: Unknown result type (might be due to invalid IL or missing references)
		//IL_081a: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GridErosion"))
		{
			TerrainHeightMap heightMap = TerrainMeta.HeightMap;
			heightMap.Push();
			NativeArray<short> src = heightMap.src;
			NativeArray<short> dst = heightMap.dst;
			NativeArray<float> minTerrainHeightMap = new NativeArray<float>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)0);
			NativeArray<float> waterMap = new NativeArray<float>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)1);
			NativeList<int> val = new NativeList<int>(heightMap.src.Length, AllocatorHandle.op_Implicit((Allocator)4));
			NativeArray<float4> fluxMap = new NativeArray<float4>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)1);
			NativeArray<float2> velocityMap = new NativeArray<float2>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)1);
			NativeArray<float> val2 = new NativeArray<float>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)1);
			NativeArray<float> copyTarget = new NativeArray<float>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)1);
			NativeArray<float> angleMap = new NativeArray<float>(heightMap.src.Length, (Allocator)4, (NativeArrayOptions)0);
			Debug.Assert(waterMap.Length == heightMap.src.Length);
			Debug.Assert(fluxMap.Length == heightMap.src.Length);
			Debug.Assert(velocityMap.Length == heightMap.src.Length);
			Debug.Assert(val2.Length == heightMap.src.Length);
			float num = TerrainMeta.Size.x / (float)heightMap.res * TerrainMeta.Size.z / (float)heightMap.res;
			float invGridCellSquareSize = 1f / num;
			float pipeLength = 1f;
			float pipeArea = 1f;
			JobHandle val3 = default(JobHandle);
			NativeArray<float> val4 = new NativeArray<float>(src.Length, (Allocator)4, (NativeArrayOptions)1);
			NativeArray<float> val5 = new NativeArray<float>(val4, (Allocator)4);
			val3 = IJobParallelForBatchExtensions.Schedule<GenerateErosionJobs.PrepareMapJob>(new GenerateErosionJobs.PrepareMapJob
			{
				HeightMapAsShort = src.AsReadOnly(),
				HeightMapAsFloat = val4,
				OceanIndicesWriter = val.AsParallelWriter(),
				OceanLevel = WaterSystem.OceanLevel,
				TerrainPositionY = TerrainMeta.Position.y,
				TerrainSizeY = TerrainMeta.Size.y
			}, src.Length, GetBatchSize(src.Length), val3);
			val3 = IJobParallelForExtensions.Schedule<GenerateErosionJobs.CalcMinHeightMapJob>(new GenerateErosionJobs.CalcMinHeightMapJob
			{
				TerrainHeightMap = val4.AsReadOnly(),
				MinTerrainHeightMap = minTerrainHeightMap,
				HeightMapRes = TerrainMeta.HeightMap.res,
				TopologyMap = TerrainMeta.TopologyMap.src.AsReadOnly(),
				TopologyMapRes = TerrainMeta.TopologyMap.res,
				OceanHeight = WaterSystem.OceanLevel,
				TerrainOneOverSizeX = TerrainMeta.OneOverSize.x
			}, val4.Length, GetBatchSize(val4.Length), val3);
			((JobHandle)(ref val3)).Complete();
			val3 = default(JobHandle);
			NativeArray<float> copyTarget2 = new NativeArray<float>(src.Length, (Allocator)4, (NativeArrayOptions)1);
			GenerateErosionJobs.CopyArrayJob<float> obj = new GenerateErosionJobs.CopyArrayJob<float>
			{
				CopyTarget = copyTarget2,
				CopySource = val4
			};
			GenerateErosionJobs.CopyArrayJob<float> copyArrayJob = new GenerateErosionJobs.CopyArrayJob<float>
			{
				CopyTarget = val5,
				CopySource = val4
			};
			val3 = JobHandle.CombineDependencies(IJobExtensions.Schedule<GenerateErosionJobs.CopyArrayJob<float>>(obj, val3), IJobExtensions.Schedule<GenerateErosionJobs.CopyArrayJob<float>>(copyArrayJob, val3));
			int num2 = 32;
			int num3 = 32;
			int num4 = (heightMap.res + num2 - 1) / num2;
			int num5 = (heightMap.res + num3 - 1) / num3;
			int num6 = num4 * num5;
			for (int i = 0; i < 512; i++)
			{
				GenerateErosionJobs.RefillOceanJob refillOceanJob = new GenerateErosionJobs.RefillOceanJob
				{
					OceanIndices = val.AsReadOnly(),
					HeightMap = val4.AsReadOnly(),
					OceanLevel = WaterSystem.OceanLevel,
					WaterMap = waterMap
				};
				val3 = IJobParallelForExtensions.ScheduleByRef<GenerateErosionJobs.RefillOceanJob>(ref refillOceanJob, val.Length, GetBatchSize(val.Length), val3);
				GenerateErosionJobs.WaterIncrementationJob waterIncrementationJob = new GenerateErosionJobs.WaterIncrementationJob
				{
					WaterMap = waterMap,
					WaterFillRate = 0.04f,
					DT = 0.06f
				};
				val3 = IJobParallelForExtensions.ScheduleByRef<GenerateErosionJobs.WaterIncrementationJob>(ref waterIncrementationJob, waterMap.Length, GetBatchSize(waterMap.Length), val3);
				GenerateErosionJobs.CalculateOutputFluxJob calculateOutputFluxJob = new GenerateErosionJobs.CalculateOutputFluxJob
				{
					TerrainHeightMapFloatVal = val4.AsReadOnly(),
					WaterMap = waterMap.AsReadOnly(),
					FluxMap = fluxMap,
					Res = heightMap.res,
					DT = 0.06f,
					GridCellSquareSize = num,
					PipeLength = pipeLength,
					PipeArea = pipeArea
				};
				val3 = IJobParallelForExtensions.ScheduleByRef<GenerateErosionJobs.CalculateOutputFluxJob>(ref calculateOutputFluxJob, fluxMap.Length, GetBatchSize(fluxMap.Length), val3);
				GenerateErosionJobs.AdjustWaterHeightByFluxJob adjustWaterHeightByFluxJob = new GenerateErosionJobs.AdjustWaterHeightByFluxJob
				{
					WaterMap = waterMap,
					VelocityMap = velocityMap,
					FluxMap = fluxMap.AsReadOnly(),
					Res = heightMap.res,
					DT = 0.06f,
					InvGridCellSquareSize = invGridCellSquareSize
				};
				val3 = IJobParallelForExtensions.ScheduleByRef<GenerateErosionJobs.AdjustWaterHeightByFluxJob>(ref adjustWaterHeightByFluxJob, waterMap.Length, GetBatchSize(waterMap.Length), val3);
				GenerateErosionJobs.TileCalculateAngleMap tileCalculateAngleMap = new GenerateErosionJobs.TileCalculateAngleMap
				{
					AngleMap = angleMap,
					TerrainHeightMapSrcFloat = val4.AsReadOnly(),
					NormY = heightMap.normY,
					Res = heightMap.res,
					TileSizeX = num2,
					TileSizeZ = num3,
					NumXTiles = num4
				};
				val3 = IJobParallelForExtensions.ScheduleByRef<GenerateErosionJobs.TileCalculateAngleMap>(ref tileCalculateAngleMap, num6, num6 / JobsUtility.JobWorkerCount, val3);
				GenerateErosionJobs.ErosionAndDepositionJob erosionAndDepositionJob = new GenerateErosionJobs.ErosionAndDepositionJob
				{
					SedimentMap = val2,
					MinTerrainHeightMap = minTerrainHeightMap.AsReadOnly(),
					TerrainHeightMapSrcFloat = val4.AsReadOnly(),
					TerrainHeightMapDstFloat = val5,
					WaterMap = waterMap,
					VelocityMap = velocityMap.AsReadOnly(),
					AngleMap = angleMap.AsReadOnly(),
					DT = 0.06f
				};
				val3 = IJobParallelForExtensions.ScheduleByRef<GenerateErosionJobs.ErosionAndDepositionJob>(ref erosionAndDepositionJob, val2.Length, GetBatchSize(val2.Length), val3);
				GenerateErosionJobs.CopyArrayJob<float> copyArrayJob2 = new GenerateErosionJobs.CopyArrayJob<float>
				{
					CopyTarget = copyTarget,
					CopySource = val2
				};
				val3 = IJobExtensions.ScheduleByRef<GenerateErosionJobs.CopyArrayJob<float>>(ref copyArrayJob2, val3);
				GenerateErosionJobs.CopyArrayJob<float> copyArrayJob3 = new GenerateErosionJobs.CopyArrayJob<float>
				{
					CopyTarget = val4,
					CopySource = val5
				};
				GenerateErosionJobs.TransportSedimentJob transportSedimentJob = new GenerateErosionJobs.TransportSedimentJob
				{
					SedimentMap = val2,
					SedimentReadOnlyMap = copyTarget.AsReadOnly(),
					VelocityMap = velocityMap.AsReadOnly(),
					Res = heightMap.res,
					DT = 0.06f
				};
				val3 = JobHandle.CombineDependencies(IJobExtensions.ScheduleByRef<GenerateErosionJobs.CopyArrayJob<float>>(ref copyArrayJob3, val3), IJobParallelForExtensions.ScheduleByRef<GenerateErosionJobs.TransportSedimentJob>(ref transportSedimentJob, val2.Length, GetBatchSize(val2.Length), val3));
				GenerateErosionJobs.EvaporationJob evaporationJob = new GenerateErosionJobs.EvaporationJob
				{
					WaterMap = waterMap,
					DT = 0.06f,
					EvaporationRate = 0.015f
				};
				val3 = IJobParallelForExtensions.ScheduleByRef<GenerateErosionJobs.EvaporationJob>(ref evaporationJob, waterMap.Length, GetBatchSize(waterMap.Length), val3);
			}
			GenerateErosionJobs.CopyBackFloatHeightToShortHeightJob copyBackFloatHeightToShortHeightJob = new GenerateErosionJobs.CopyBackFloatHeightToShortHeightJob
			{
				HeightMapAsFloat = val4.AsReadOnly(),
				HeightMapAsShort = dst,
				TerrainOneOverSizeY = TerrainMeta.OneOverSize.y,
				TerrainPositionY = TerrainMeta.Position.y
			};
			val3 = IJobParallelForExtensions.ScheduleByRef<GenerateErosionJobs.CopyBackFloatHeightToShortHeightJob>(ref copyBackFloatHeightToShortHeightJob, val4.Length, GetBatchSize(val4.Length), val3);
			NativeArray<float> val6 = new NativeArray<float>(val4.Length, (Allocator)4, (NativeArrayOptions)1);
			GenerateErosionJobs.PopulateDeltaHeightJob populateDeltaHeightJob = new GenerateErosionJobs.PopulateDeltaHeightJob
			{
				HeightMapOriginal = copyTarget2.AsReadOnly(),
				HeightMap = val4.AsReadOnly(),
				DeltaHeightMap = val6
			};
			val3 = IJobParallelForExtensions.ScheduleByRef<GenerateErosionJobs.PopulateDeltaHeightJob>(ref populateDeltaHeightJob, val6.Length, GetBatchSize(val6.Length), val3);
			minTerrainHeightMap.Dispose(val3);
			waterMap.Dispose(val3);
			fluxMap.Dispose(val3);
			velocityMap.Dispose(val3);
			val2.Dispose(val3);
			copyTarget.Dispose(val3);
			val4.Dispose(val3);
			val5.Dispose(val3);
			val.Dispose(val3);
			copyTarget2.Dispose(val3);
			((JobHandle)(ref val3)).Complete();
			heightMap.Pop();
			splatPaintingData = new SplatPaintingData(val6, angleMap);
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
		Vector3 val = default(Vector3);
		for (float num = TerrainMeta.Position.z; num < TerrainMeta.Position.z + TerrainMeta.Size.z; num += 10f)
		{
			for (float num2 = TerrainMeta.Position.x; num2 < TerrainMeta.Position.x + TerrainMeta.Size.x; num2 += 10f)
			{
				((Vector3)(ref val))._002Ector(num2, 0f, num);
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
				Vector2 normalized = ((Vector2)(ref val2)).normalized;
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
					val2 = Vector2.Lerp(val4, ((Vector2)(ref val2)).normalized, 0.5f);
					normalized = ((Vector2)(ref val2)).normalized;
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
		Parallel.For(1, erosion_res - 1, delegate(int z)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			for (int j = 1; j < erosion_res - 1; j++)
			{
				float num16 = CalculateDelta(erosion, erosion_res, j, z, 1f, 0.8f, 0.6f);
				float delta = (0f - Mathf.Lerp(0f, 0.25f, num16)) * TerrainMeta.OneOverSize.y;
				heightmap.AddHeight(j, z, delta);
			}
		});
		Parallel.For(1, deposit_res - 1, delegate(int z)
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
