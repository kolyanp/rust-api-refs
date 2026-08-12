public static class NPCConversationPhrases
{
	public static readonly Phrase MissionObjectiveOptional;

	public static readonly Phrase MissionCompleted;

	public static readonly Phrase SelectReward;

	static NPCConversationPhrases()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		MissionObjectiveOptional = new Phrase("mission_objective_optional_prefix", "Optional:");
		MissionCompleted = new Phrase("mission_completed", "Completed");
		SelectReward = new Phrase("select.missionreward", "Select");
	}
}
