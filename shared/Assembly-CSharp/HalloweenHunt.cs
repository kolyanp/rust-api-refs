public class HalloweenHunt : EggHuntEvent
{
	public static Phrase topCreepPhrase;

	public static Phrase placeCreepPhrase;

	protected override Phrase GetTopBunnyPhrase()
	{
		return topCreepPhrase;
	}

	protected override Phrase GetPlacePhrase()
	{
		return placeCreepPhrase;
	}

	protected override void ReportPlayerParticipated(int topCount)
	{
	}

	protected override void ReportEggsCollected(int numEggs)
	{
	}

	static HalloweenHunt()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		topCreepPhrase = new Phrase("candyhunt.result.topcreeps", "{0} is the top creep with {1} candies collected.");
		placeCreepPhrase = new Phrase("candyhunt.result.place", "You placed {0} of {1} with {2} candies collected.");
	}
}
