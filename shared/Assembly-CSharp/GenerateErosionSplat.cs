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
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		Debug.Assert(GenerateErosion.splatPaintingData.HeightMapDelta.IsCreated);
		Debug.Assert(GenerateErosion.splatPaintingData.AngleMap.IsCreated);
		TerrainSplatMap splatMap = TerrainMeta.SplatMap;
		NativeArray<float> heightMapDelta = GenerateErosion.splatPaintingData.HeightMapDelta;
		NativeArray<float> angleMap = GenerateErosion.splatPaintingData.AngleMap;
		int res = TerrainMeta.HeightMap.res;
		Parallel.For(1, res - 1, delegate(int z)
		{
			for (int i = 1; i < res - 1; i++)
			{
				angleMap[z * res + i] = TerrainMeta.HeightMap.GetSlope(i, z);
			}
		});
		splatMap.Push();
		NativeHashMap<int, int> val = default(NativeHashMap<int, int>);
		val._002Ector(splatMap.num, AllocatorHandle.op_Implicit((Allocator)3));
		foreach (var (num3, num4) in TerrainSplat.GetType2IndexDic())
		{
			val.Add(num3, num4);
		}
		JobHandle val2 = default(JobHandle);
		GenerateErosionJobs.PaintSplatJob paintSplatJob = new GenerateErosionJobs.PaintSplatJob
		{
			HeightMapDelta = heightMapDelta.AsReadOnly(),
			HeightMapRes = TerrainMeta.HeightMap.res,
			AngleMapDeg = angleMap.AsReadOnly(),
			TopologyMap = TerrainMeta.TopologyMap.src.AsReadOnly(),
			TopologyMapRes = TerrainMeta.TopologyMap.res,
			SplatMap = splatMap.dst,
			SplatMapRes = splatMap.res,
			SplatNum = splatMap.num,
			SplatType2Index = val.AsReadOnly(),
			TerrainOneOverSizeX = TerrainMeta.OneOverSize.x
		};
		val2 = IJobForExtensions.ScheduleByRef<GenerateErosionJobs.PaintSplatJob>(ref paintSplatJob, heightMapDelta.Length, val2);
		val.Dispose(val2);
		GenerateErosion.splatPaintingData.Dispose(val2);
		((JobHandle)(ref val2)).Complete();
		splatMap.Pop();
	}
}
