using System.Collections.Generic;
using System.Threading.Tasks;
using GenerateErosionJobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class GenerateErosionSplat : ProceduralComponent
{
	public override void Process(uint seed)
	{
		if (!World.Networked)
		{
			Debug.Assert(GenerateErosion.splatPaintingData.IsValid, "GenerateErosion.splatPaintingData is not populated, GenerateErosion likely missing from WorldSetup");
			if (GenerateErosion.splatPaintingData.IsValid)
			{
				PaintSplat();
			}
		}
	}

	private void PaintSplat()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		Debug.Assert(GenerateErosion.splatPaintingData.HeightMapDelta.IsCreated);
		TerrainSplatMap splatMap = TerrainMeta.SplatMap;
		NativeArray<float> heightMapDelta = GenerateErosion.splatPaintingData.HeightMapDelta;
		int res = TerrainMeta.HeightMap.res;
		NativeArray<float> angleMapDeg = new NativeArray<float>(heightMapDelta.Length, (Allocator)3, (NativeArrayOptions)1);
		Parallel.For(1, res - 1, (int z) =>
		{
			for (int i = 1; i < res - 1; i++)
			{
				angleMapDeg[z * res + i] = TerrainMeta.HeightMap.GetSlope(i, z);
			}
		});
		splatMap.Push();
		NativeHashMap<int, int> val = new NativeHashMap<int, int>(splatMap.num, AllocatorHandle.op_Implicit((Allocator)3));
		foreach (var (num3, num4) in TerrainSplat.GetType2IndexDic())
		{
			val.Add(num3, num4);
		}
		JobHandle val2 = default;
		PaintSplatJob paintSplatJob = new PaintSplatJob
		{
			HeightMapDelta = heightMapDelta.AsReadOnly(),
			HeightMapRes = TerrainMeta.HeightMap.res,
			AngleMapDeg = angleMapDeg.AsReadOnly(),
			TopologyMap = TerrainMeta.TopologyMap.GetHierarchyQuery(),
			SplatMap = splatMap.dst,
			SplatMapRes = splatMap.res,
			SplatNum = splatMap.num,
			SplatType2Index = val.AsReadOnly(),
			TerrainOneOverSizeX = TerrainMeta.OneOverSize.x
		};
		val2 = IJobForExtensions.ScheduleByRef<PaintSplatJob>(ref paintSplatJob, heightMapDelta.Length, val2);
		val.Dispose(val2);
		angleMapDeg.Dispose(val2);
		GenerateErosion.splatPaintingData.Dispose(val2);
		val2.Complete();
		splatMap.Pop();
	}
}
