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

	[ServerVar]
	[Help("Pretend there are this many additional heavy fuses currently plugged into the power plant. Can input negative numbers to negate the effect of any currently plugged in fuses.")]
	public static int simulatePowerPlantFuses = 0;

	[ServerVar]
	[Help("How long heavy fuses last for when plugged into the power plant. If <= 0 then fuses will last forever. This is the total time a fuse takes to go from full condition to broken, regardless of fuseFastDeteriorationThreshold / fuseFastDeteriorationScale.")]
	public static float fuseLifespanSeconds = 9600f;

	private const float defaultFuseLifespanSeconds = 9600f;

	[ServerVar(Help = "Normalized condition (0-1) at which heavy fuses plugged into the power plant start deteriorating faster. At or below this threshold the deterioration rate is multiplied by fuseFastDeteriorationScale. Set to 0 to disable accelerated deterioration.", Saved = true)]
	public static float fuseFastDeteriorationThreshold = 0.3f;

	private const float defaultFuseFastDeteriorationThreshold = 0.3f;

	[ServerVar(Help = "How much faster heavy fuses plugged into the power plant deteriorate once their normalized condition is at or below fuseFastDeteriorationThreshold.", Saved = true)]
	public static float fuseFastDeteriorationScale = 3f;

	private const float defaultFuseFastDeteriorationScale = 3f;

	[ServerVar(Help = "If enabled, whenever no heavy fuse in the power plant is below fuseFastDeteriorationThreshold, the single lowest condition fuse deteriorates at the fast rate anyway.", Saved = true)]
	public static bool fuseFastDeteriorateLowest = true;

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
		stringBuilder.AppendLine(string.Format("{0}: {1}", "fuseFastDeteriorationThreshold", fuseFastDeteriorationThreshold));
		stringBuilder.AppendLine(string.Format("{0}: {1}", "fuseFastDeteriorationScale", fuseFastDeteriorationScale));
		stringBuilder.AppendLine(string.Format("{0}: {1}", "fuseFastDeteriorateLowest", fuseFastDeteriorateLowest));
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
}
