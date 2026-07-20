using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;

public class ScopedOcean
{
	private NativeArray<int> originalTopologyCopy;

	public bool IsScoped { get; set; }

	public void SetFlatOcean()
	{
		IsScoped = true;
		SetupFlatOcean();
	}

	public void Restore()
	{
		if (IsScoped)
		{
			RevertOcean();
		}
	}

	private void SetupFlatOcean()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		originalTopologyCopy = new NativeArray<int>(TerrainMeta.TopologyMap.src, (Allocator)4);
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

	private void RevertOcean()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		TerrainMeta.TopologyMap.Push();
		TerrainMeta.TopologyMap.dst.CopyFrom(originalTopologyCopy);
		TerrainMeta.TopologyMap.Pop();
		TerrainMeta.Texturing.Setup();
		NativeArrayEx.SafeDispose(ref originalTopologyCopy);
	}
}
