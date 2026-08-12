public class UnderwaterDweller : HumanNPC
{
	public static readonly Phrase UnderwaterDwellerName;

	public override string displayName => UnderwaterDwellerName.translated;

	static UnderwaterDweller()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		UnderwaterDwellerName = new Phrase("npc_underwaterdweller", "Underwater Dweller");
	}
}
