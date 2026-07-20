using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct BuildLayerMasksJob : IJob
{
	[WriteOnly]
	public NativeList<int> LayerMasks;

	public ReadOnly<AntiHack.Batch> Batches;

	[ReadOnly]
	public int DefaultMask;

	[ReadOnly]
	public int NoVehicleMask;

	public void Execute()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<AntiHack.Batch> enumerator = Batches.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				AntiHack.Batch current = enumerator.Current;
				int num = (current.SkipVehicleLayer ? NoVehicleMask : DefaultMask);
				for (int i = 0; i < current.Count; i++)
				{
					LayerMasks.AddNoResize(num);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
