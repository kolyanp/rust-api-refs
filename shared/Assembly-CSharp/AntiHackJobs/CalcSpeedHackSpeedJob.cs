using BasePlayerJobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct CalcSpeedHackSpeedJob : IJobFor
{
	[WriteOnly]
	public NativeArray<float> Speed;

	public ReadOnly<BasePlayer.CachedState> States;

	public ReadOnly<int> Indices;

	public ReadOnly<RDC> RDCs;

	[ReadOnly]
	public float WaterThreshold;

	public void Execute(int jobInd)
	{
		int num = Indices[jobInd];
		BasePlayer.CachedState cachedState = States[num];
		RDC rDC = RDCs[num];
		float num2 = 1f - cachedState.ClothingMoveSpeedReduction;
		float num3 = num2 + cachedState.ClothingWaterSpeedBonus;
		if (cachedState.IsSwimming)
		{
			float num4 = ((!(rDC.Crawling > 0f)) ? (Mathf.Lerp(Mathf.Lerp(2.8f, 5.5f, rDC.Running), 1.7f, rDC.Ducking) * num3 * cachedState.WeaponMoveSpeedScale * cachedState.ModifiersMovementMultiplier) : (Mathf.Lerp(2.8f, 0.72f, rDC.Crawling) * cachedState.ModifiersMovementMultiplier * num3));
			Speed[jobInd] = num4;
			return;
		}
		if (cachedState.WaterFactor < WaterThreshold)
		{
			float num5 = num2;
			float num6;
			if (rDC.Crawling > 0f)
			{
				num6 = Mathf.Lerp(2.8f, 0.72f, rDC.Crawling) * num5 * cachedState.ModifiersMovementMultiplier;
			}
			else
			{
				num6 = Mathf.Lerp(Mathf.Lerp(2.8f, 5.5f, rDC.Running), 1.7f, rDC.Ducking) * num5 * cachedState.WeaponMoveSpeedScale * cachedState.ModifiersMovementMultiplier;
				num6 = Mathf.Lerp(num6, 0f, Mathf.Max(cachedState.MovementModify.drag, cachedState.ClothingMoveSpeedReduction));
			}
			Speed[jobInd] = num6;
			return;
		}
		float num7 = num2;
		float num9;
		float num10;
		if (rDC.Crawling > 0f)
		{
			float num8 = Mathf.Lerp(2.8f, 0.72f, rDC.Crawling) * cachedState.ModifiersMovementMultiplier;
			num9 = num8 * num3;
			num10 = num8 * num7;
		}
		else
		{
			float num11 = Mathf.Lerp(2.8f, 5.5f, rDC.Running);
			float num12 = Mathf.Lerp(num11, 1.7f, rDC.Ducking) * cachedState.WeaponMoveSpeedScale * cachedState.ModifiersMovementMultiplier;
			num9 = Mathf.Lerp(num11, 1.7f, 1f) * num3 * cachedState.WeaponMoveSpeedScale * cachedState.ModifiersMovementMultiplier;
			num10 = Mathf.Lerp(num12 * num7, 0f, Mathf.Max(cachedState.MovementModify.drag, cachedState.ClothingMoveSpeedReduction));
		}
		Speed[jobInd] = Mathf.Max(num9, num10);
	}
}
