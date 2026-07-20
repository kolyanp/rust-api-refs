namespace ConVar;

[Factory("decay")]
public class Decay : ConsoleSystem
{
	[ReplicatedVar(Help = "Can players upgrade building blocks to wood")]
	public static bool upgrade_wood_enabled = true;

	[ReplicatedVar(Help = "Can players upgrade building blocks to stone")]
	public static bool upgrade_stone_enabled = true;

	[ReplicatedVar(Help = "Can players upgrade building blocks to metal")]
	public static bool upgrade_metal_enabled = true;

	[ReplicatedVar(Help = "Can players upgrade building blocks to hqm")]
	public static bool upgrade_hqm_enabled = true;

	[ReplicatedVar(Help = "Can players upgrade building blocks to wood")]
	public static float build_twig_cost_multiplier = 1f;

	[ReplicatedVar(Help = "Can players upgrade building blocks to wood")]
	public static float upgrade_wood_cost_multiplier = 1f;

	[ReplicatedVar(Help = "Can players upgrade building blocks to stone")]
	public static float upgrade_stone_cost_multiplier = 1f;

	[ReplicatedVar(Help = "Can players upgrade building blocks to metal")]
	public static float upgrade_metal_cost_multiplier = 1f;

	[ReplicatedVar(Help = "Can players upgrade building blocks to hqm")]
	public static float upgrade_hqm_cost_multiplier = 1f;

	[ServerVar(Help = "Maximum distance to test to see if a structure is outside, higher values are slower but accurate for huge buildings")]
	public static float outside_test_range = 50f;

	[ServerVar(Help = "(Generated) Interval in seconds between decay processing ticks; default is 10 minutes; lower values cause buildings to lose health more frequently")]
	public static float tick = 600f;

	[ServerVar(Help = "(Generated) Multiplier applied to all decay damage per tick; 1.0 = normal, 2.0 = double decay rate, 0.0 = no decay")]
	public static float scale = 1f;

	[ServerVar(Help = "(Generated) When enabled, logs decay tick details to the console including which entities took damage and how much")]
	public static bool debug = false;

	[ServerVar(Help = "Is upkeep enabled")]
	public static bool upkeep = true;

	[ServerVar(Help = "How many minutes does the upkeep cost last? default : 1440 (24 hours)")]
	public static float upkeep_period_minutes = 1440f;

	[ServerVar(Help = "How many minutes can the upkeep cost last after the cupboard was destroyed? default : 1440 (24 hours)")]
	public static float upkeep_grief_protection = 1440f;

	[ServerVar(Help = "Scale at which objects heal when upkeep conditions are met, default of 1 is same rate at which they decay")]
	public static float upkeep_heal_scale = 1f;

	[ServerVar(Help = "Scale at which objects decay when they are inside, default of 0.1")]
	public static float upkeep_inside_decay_scale = 0.1f;

	[ServerVar(Help = "When set to a value above 0 everything will decay with this delay")]
	public static float delay_override = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_twig = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_wood = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_stone = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_metal = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_toptier = 0f;

	[ServerVar(Help = "When set to a value above 0 everything will decay with this duration")]
	public static float duration_override = 0f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_twig = 1f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_wood = 3f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_stone = 5f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_metal = 8f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_toptier = 12f;

	public const int UpkeepBracketCount = 4;

	[ServerVar(Help = "Number of blocks in the 1st upkeep bracket")]
	public static int bracket_0_blockcount = 15;

	[ServerVar(Help = "Blocks in the 1st upkeep bracket will cost this value per day to maintain")]
	public static float bracket_0_costfraction = 0.1f;

	[ServerVar(Help = "Number of blocks in the 2nd upkeep bracket")]
	public static int bracket_1_blockcount = 50;

	[ServerVar(Help = "Blocks in the 2nd upkeep bracket will cost this value per day to maintain")]
	public static float bracket_1_costfraction = 0.15f;

	[ServerVar(Help = "The number of blocks in the 3rd upkeep bracket")]
	public static int bracket_2_blockcount = 125;

	[ServerVar(Help = "Blocks in the 3rd upkeep bracket will cost this value per day to maintain")]
	public static float bracket_2_costfraction = 0.2f;

	[ServerVar(Help = "Blocks in the 4th upkeep bracket will cost this value per day to maintain")]
	public static float bracket_3_costfraction = 0.333f;

	[ServerVar(Help = "Should doors have their own upkeep brackets separate from building blocks")]
	public static bool use_door_upkeep_brackets = false;

	[ServerVar(Help = "Number of doors in the 1st upkeep bracket")]
	public static int bracket_0_doorcount = 15;

	[ServerVar(Help = "Doors in the 1st upkeep bracket will cost this value per day to maintain")]
	public static float bracket_0_doorfraction = 0.1f;

	[ServerVar(Help = "Number of doors in the 2nd upkeep bracket")]
	public static int bracket_1_doorcount = 50;

	[ServerVar(Help = "Doors in the 2nd upkeep bracket will cost this value per day to maintain")]
	public static float bracket_1_doorfraction = 0.15f;

	[ServerVar(Help = "The number of doors in the 3rd upkeep bracket")]
	public static int bracket_2_doorcount = 125;

	[ServerVar(Help = "Doors in the 3rd upkeep bracket will cost this value per day to maintain")]
	public static float bracket_2_doorfraction = 0.2f;

	[ServerVar(Help = "Doors in the 4th upkeep bracket will cost this value per day to maintain")]
	public static float bracket_3_doorfraction = 0.333f;

	public static float GetCostMultiplier(BuildingGrade.Enum grade)
	{
		return grade switch
		{
			BuildingGrade.Enum.Twigs => build_twig_cost_multiplier, 
			BuildingGrade.Enum.Wood => upgrade_wood_cost_multiplier, 
			BuildingGrade.Enum.Stone => upgrade_stone_cost_multiplier, 
			BuildingGrade.Enum.Metal => upgrade_metal_cost_multiplier, 
			BuildingGrade.Enum.TopTier => upgrade_hqm_cost_multiplier, 
			_ => 1f, 
		};
	}

	public static bool CanUpgradeToGrade(BuildingGrade.Enum grade)
	{
		return grade switch
		{
			BuildingGrade.Enum.Twigs => true, 
			BuildingGrade.Enum.Wood => upgrade_wood_enabled, 
			BuildingGrade.Enum.Stone => upgrade_stone_enabled, 
			BuildingGrade.Enum.Metal => upgrade_metal_enabled, 
			BuildingGrade.Enum.TopTier => upgrade_hqm_enabled, 
			_ => true, 
		};
	}
}
