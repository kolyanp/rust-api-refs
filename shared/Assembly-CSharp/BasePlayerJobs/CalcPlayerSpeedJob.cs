using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BasePlayerJobs;

[BurstCompile]
public struct CalcPlayerSpeedJob : IJobFor
{
	[WriteOnly]
	public NativeArray<float> Speed;

	public ReadOnly<BasePlayer.CachedState> States;

	public ReadOnly<int> Indices;

	public ReadOnly<RDC> RDCs;

	public void Execute(int jobInd)
	{
		int num = Indices[jobInd];
		BasePlayer.CachedState cachedState = States[num];
		RDC rDC = RDCs[num];
		float num2 = 1f;
		num2 -= cachedState.ClothingMoveSpeedReduction;
		if (cachedState.IsSwimming)
		{
			num2 += cachedState.ClothingWaterSpeedBonus;
		}
		float num3;
		if (rDC.Crawling > 0f)
		{
			num3 = Mathf.Lerp(2.8f, 0.72f, rDC.Crawling) * num2 * cachedState.ModifiersMovementMultiplier;
		}
		else
		{
			num3 = Mathf.Lerp(Mathf.Lerp(2.8f, 5.5f, rDC.Running), 1.7f, rDC.Ducking) * num2 * cachedState.WeaponMoveSpeedScale * cachedState.ModifiersMovementMultiplier;
			if (!cachedState.IsSwimming)
			{
				num3 = Mathf.Lerp(num3, 0f, Mathf.Max(cachedState.MovementModify.drag, cachedState.ClothingMoveSpeedReduction));
			}
		}
		Speed[jobInd] = num3;
	}
}
