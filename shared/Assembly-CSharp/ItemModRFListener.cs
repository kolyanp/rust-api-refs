public class ItemModRFListener : ItemModAssociatedEntity<BaseEntity>
{
	public static readonly Phrase SetFreqTitle;

	public static readonly Phrase SetFreqDesc;

	public GameObjectRef frequencyPanelPrefab;

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		base.ServerCommand(item, command, player);
	}

	static ItemModRFListener()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		SetFreqTitle = new Phrase("setfreq", "Set Frequency");
		SetFreqDesc = new Phrase("setfreq_desc", "Configure which frequency to listen to");
	}
}
