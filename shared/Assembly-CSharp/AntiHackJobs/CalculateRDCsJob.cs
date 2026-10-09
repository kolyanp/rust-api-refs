using BasePlayerJobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct CalculateRDCsJob : IJobFor
{
	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<RDC> RDCs;

	public ReadOnly<BasePlayer.CachedState> States;

	public ReadOnly<Flag> MsFlags;

	public ReadOnly<float> Ducking;

	public ReadOnly<int> Indices;

	public void Execute(int jobInd)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		int num = Indices[jobInd];
		BasePlayer.CachedState cachedState = States[num];
		float running = (((MsFlags[num] & 0x10) != 0) ? 1 : 0);
		float ducking = ((BasePlayer.IsDucked(Ducking[num]) || cachedState.IsSwimming) ? 1 : 0);
		float crawling = (BasePlayer.IsCrawling(cachedState.PlayerFlags) ? 1 : 0);
		RDCs[num] = new RDC
		{
			Running = running,
			Ducking = ducking,
			Crawling = crawling
		};
	}
}
