using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;

namespace Rust.Ai.Gen2;

public class NavStressOcean
{
	private NativeArray<int> originalTopology;

	private bool scoped;

	public void SetFlatOcean()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (!scoped)
		{
			scoped = true;
			originalTopology = new NativeArray<int>(TerrainMeta.TopologyMap.src, (Allocator)4);
			TerrainMeta.TopologyMap.Push();
			FillJob<int> fillJob = new FillJob<int>
			{
				Values = TerrainMeta.TopologyMap.dst,
				Value = 128
			};
			IJobExtensions.RunByRef<FillJob<int>>(ref fillJob);
			TerrainMeta.TopologyMap.Pop();
			TerrainMeta.Texturing.Setup();
			ref TerrainTexturing.ShoreData mapByRef = ref TerrainMeta.Texturing.GetMapByRef(isDeepSea: false);
			mapByRef.DefaultVector = new Vector4(1f, 1f, 1f, 1f);
			mapByRef.FillWithDefault();
		}
	}

	public void Restore()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (scoped)
		{
			scoped = false;
			TerrainMeta.TopologyMap.Push();
			TerrainMeta.TopologyMap.dst.CopyFrom(originalTopology);
			TerrainMeta.TopologyMap.Pop();
			TerrainMeta.Texturing.Setup();
			NativeArrayEx.SafeDispose(ref originalTopology);
		}
	}
}
