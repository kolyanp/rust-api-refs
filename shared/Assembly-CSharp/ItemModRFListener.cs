public class ItemModRFListener : ItemModAssociatedEntity<BaseEntity>
{
	public static readonly Phrase SetFreqTitle = new Phrase("setfreq", "Set Frequency");

	public static readonly Phrase SetFreqDesc = new Phrase("setfreq_desc", "Configure which frequency to listen to");

	public GameObjectRef frequencyPanelPrefab;

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		base.ServerCommand(item, command, player);
	}
}
