using System;
using System.Collections.Generic;
using System.Text;
using Facepunch;
using UnityEngine;

public class Powergrid : ConsoleSystem
{
	[Help("If disabled power grid functionality will be disabled.")]
	[ReplicatedVar]
	public static bool enabled = true;

	[ReplicatedVar]
	[Help("Required powergrid stage for green recyclers to return to baseline efficiency. If < 0 then will use the default values.")]
	public static int greenRecyclerFullEfficiencyStage = -1;

	[Help("Pretend there are this many additional heavy fuses currently plugged into the power plant. Can input negative numbers to negate the effect of any currently plugged in fuses.")]
	[ServerVar]
	public static int simulatePowerPlantFuses = 0;

	[ServerVar(Help = "How long a heavy fuse plugged into the power plant lasts while it is decaying at the full rate (how long the worst fuses in the power plant survive for). If <= 0 then fuses last forever.", Saved = true)]
	public static float fuseLifespanSeconds = 9600f;

	private const float defaultFuseLifespanSeconds = 9600f;

	[ServerVar(Help = "How many of the worst condition heavy fuses in the power plant decay at the full rate (burning out after fuseLifespanSeconds). Every other inserted fuse decays slowly instead. If 0 no fuse ever decays at the full rate.", Saved = true)]
	public static int fuseFullDecayCount = 3;

	private const int defaultFuseFullDecayCount = 3;

	[ServerVar(Help = "Minimum fraction (0-1) of the full decay rate applied to heavy fuses that aren't one of the worst fuseFullDecayCount. Each fuse rolls its own fraction between fuseSlowDecayFractionMin and fuseSlowDecayFractionMax and keeps it for its lifetime.", Saved = true)]
	public static float fuseSlowDecayFractionMin = 0.08f;

	private const float defaultFuseSlowDecayFractionMin = 0.08f;

	[ServerVar(Help = "Maximum fraction (0-1) of the full decay rate applied to heavy fuses that aren't one of the worst fuseFullDecayCount. See fuseSlowDecayFractionMin.", Saved = true)]
	public static float fuseSlowDecayFractionMax = 0.12f;

	private const float defaultFuseSlowDecayFractionMax = 0.12f;

	[ServerVar(Help = "Max time per frame (ms) to spend notifying powergrid entities of a stage change.", Saved = true)]
	public static float stageChangeWorkQueueBudget = 0.1f;

	[ServerVar(Help = "Time to wait (s) between each individual entity getting notified of a powergrid stage change. Higher values will delay the time it takes for all entities to receive notification of a stage change. Entities can skip this wait with stageChangeWorkQueueGroupJobsDistance", Saved = true)]
	public static float stageChangeWorkQueueDelayBetweenJobs = 0f;

	[ServerVar(Help = "If a powergrid entity is within this range of the first powergrid entity to receive a stage change update this frame, then that entity will also receive an update (skipping stageChangeWorkQueueTimeBetweenJobs)", Saved = true)]
	public static float stageChangeWorkQueueGroupJobsDistance = 20f;

	public static float stageChangeWorkQueueGroupJobsSqrDistance => stageChangeWorkQueueGroupJobsDistance * stageChangeWorkQueueGroupJobsDistance;

	[ServerVar]
	public static void status(Arg arg)
	{
		PowergridManager serverInstance = PointEntity<PowergridManager>.ServerInstance;
		if ((Object)(object)serverInstance == (Object)null)
		{
			arg.ReplyWith("Failed to retrieve server instance for PowergridManager");
			return;
		}
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		stringBuilder.AppendLine(string.Format("{0}: {1}", "enabled", enabled));
		stringBuilder.AppendLine(string.Format("{0}: {1}", "simulatePowerPlantFuses", simulatePowerPlantFuses));
		stringBuilder.AppendLine(string.Format("{0}: {1}", "fuseLifespanSeconds", fuseLifespanSeconds));
		stringBuilder.AppendLine(string.Format("{0}: {1}", "fuseFullDecayCount", fuseFullDecayCount));
		stringBuilder.AppendLine(string.Format("{0}: {1}", "fuseSlowDecayFractionMin", fuseSlowDecayFractionMin));
		stringBuilder.AppendLine(string.Format("{0}: {1}", "fuseSlowDecayFractionMax", fuseSlowDecayFractionMax));
		stringBuilder.AppendLine(string.Format("{0}: {1}", "greenRecyclerFullEfficiencyStage", greenRecyclerFullEfficiencyStage));
		stringBuilder.AppendLine(string.Format("{0}: {1}", "stageChangeWorkQueueBudget", stageChangeWorkQueueBudget));
		stringBuilder.AppendLine(string.Format("{0}: {1}", "stageChangeWorkQueueDelayBetweenJobs", stageChangeWorkQueueDelayBetweenJobs));
		stringBuilder.AppendLine(string.Format("{0}: {1}", "stageChangeWorkQueueGroupJobsDistance", stageChangeWorkQueueGroupJobsDistance));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine($"Current stage: {serverInstance.CurrentStage}");
		int numberOfStages = PowergridStageConfig.instance.GetNumberOfStages();
		for (int i = 1; i <= numberOfStages; i++)
		{
			if (PowergridStageConfig.instance.TryGetStageDataForStage(i, out var stageData))
			{
				stringBuilder.AppendLine($"Stage {i} required fuses: {stageData.requiredFuses}");
			}
		}
		stringBuilder.AppendLine($"Number of inserted PowerPlant fuses: {serverInstance.Server_GetPowerPlantInsertedFuses()}");
		stringBuilder.AppendLine($"Number of PowerPlant fuse sockets: {serverInstance.Server_GetFuseSocketsCount()}");
		stringBuilder.AppendLine($"Number of Powergrid access points: {PowergridManager.GetNoOfPowergridAccessPoints()}");
		stringBuilder.AppendLine($"Number of Powergrid connected entities: {PowergridManager.GetNoOfPowergridEntities()}");
		arg.ReplyWith(stringBuilder.ToString());
		Pool.FreeUnmanaged(ref stringBuilder);
	}

	[ServerVar]
	public static void fuseStatus(Arg arg)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PointEntity<PowergridManager>.ServerInstance == (Object)null)
		{
			arg.ReplyWith("Failed to retrieve server instance for PowergridManager");
			return;
		}
		PooledList<Item> val = Pool.Get<PooledList<Item>>();
		try
		{
			PowergridManager.Server_GatherInsertedFuses((List<Item>)(object)val);
			if (((List<Item>)(object)val).Count > 0)
			{
				StringBuilder stringBuilder = Pool.Get<StringBuilder>();
				PooledList<Item> val2 = Pool.Get<PooledList<Item>>();
				try
				{
					PowergridManager.Server_GatherFullDecayFuses((List<Item>)(object)val2);
					int i = 0;
					for (int count = ((List<Item>)(object)val).Count; i < count; i++)
					{
						Item item = ((List<Item>)(object)val)[i];
						float num = (((List<Item>)(object)val2).Contains(item) ? 1f : PowergridManager.Server_GetSlowDecayRateScale(item));
						stringBuilder.Append($"  Fuse {item.uid}: condition {item.conditionNormalized:P0}");
						stringBuilder.Append($", decay rate {num:P0}");
						if (fuseLifespanSeconds > 0f && num > 0f)
						{
							float num2 = item.conditionNormalized * fuseLifespanSeconds / num;
							stringBuilder.Append($", ~{TimeSpan.FromSeconds(num2):d\\.hh\\:mm\\:ss} left at this rate");
						}
						else
						{
							stringBuilder.Append(", never burns out at this rate");
						}
						stringBuilder.AppendLine();
					}
					arg.ReplyWith(stringBuilder.ToString());
					Pool.FreeUnmanaged(ref stringBuilder);
					return;
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			arg.ReplyWith("No fuses inserted");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
