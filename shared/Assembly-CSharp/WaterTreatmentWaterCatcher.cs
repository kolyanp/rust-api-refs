using UnityEngine;

public class WaterTreatmentWaterCatcher : WaterCatcher
{
	[ServerVar(Saved = true)]
	public static float evaporationPerMinute = 200f;

	public override void ServerInit()
	{
		base.ServerInit();
		WaterTreatmentFlowRateBroadcast.receivers++;
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		WaterTreatmentFlowRateBroadcast.receivers--;
	}

	protected override void CollectWater()
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (doDrippingFlags)
		{
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
			flagsUpdateScope.Set(Flags.Reserved9, HasFlag(Flags.Reserved3) && hasResource() && (pushTargets == null || pushTargets.Count == 0) && WaterTreatmentFlowRateBroadcast.WaterTreatmentBroadcastFlowRate > 0f);
		}
		ToggleProducing(true);
		float num = ((overrideCollectInterval > 0f) ? overrideCollectInterval : 60f);
		nextCollect = TimeUntil.op_Implicit(num + Random.Range(0f, num * 0.1f));
		if (base.inventory != null && !IsFull())
		{
			if (WaterTreatmentFlowRateBroadcast.WaterTreatmentBroadcastFlowRate > 0f)
			{
				float num2 = WaterTreatmentFlowRateBroadcast.WaterTreatmentBroadcastFlowRate * (TimeUntil.op_Implicit(nextCollect) / 60f);
				AddResource(Mathf.CeilToInt(num2));
			}
			else
			{
				float amount = Mathf.CeilToInt(evaporationPerMinute * (TimeUntil.op_Implicit(nextCollect) / 60f));
				RemoveResource(amount);
			}
		}
	}

	private void RemoveResource(float amount)
	{
		if (hasResource())
		{
			Item liquidItem = GetLiquidItem();
			liquidItem.amount -= Mathf.RoundToInt(amount);
			liquidItem.MarkDirty();
			if (liquidItem.amount <= 0)
			{
				liquidItem.Remove();
			}
		}
	}

	public override void ToggleProducing(bool _)
	{
		bool b = WaterTreatmentFlowRateBroadcast.WaterTreatmentBroadcastFlowRate > 0f;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved3, b);
	}
}
