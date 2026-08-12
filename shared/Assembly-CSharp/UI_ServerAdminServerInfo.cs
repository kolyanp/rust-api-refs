using Rust.UI;
using UnityEngine;

public class UI_ServerAdminServerInfo : MonoBehaviour
{
	[SerializeField]
	private RustText InfoName;

	[SerializeField]
	private RustText InfoValue;

	private static Phrase HostNamePhrase;

	private static Phrase MaxPlayersPhrase;

	private static Phrase PlayersPhrase;

	private static Phrase QueuedPhrase;

	private static Phrase JoiningPhrase;

	private static Phrase ReservedSlotsPhrase;

	private static Phrase EntityCountPhrase;

	private static Phrase GameTimePhrase;

	private static Phrase UptimePhrase;

	private static Phrase MapPhrase;

	private static Phrase FrameratePhrase;

	private static Phrase MemoryPhrase;

	private static Phrase MemoryUsageSystemPhrase;

	private static Phrase CollectionsPhrase;

	private static Phrase NetworkInPhrase;

	private static Phrase NetworkOutPhrase;

	private static Phrase RestartingPhrase;

	private static Phrase SaveCreatedTimePhrase;

	private static Phrase VersionPhrase;

	private static Phrase ProtocolPhrase;

	static UI_ServerAdminServerInfo()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected O, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected O, but got Unknown
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected O, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Expected O, but got Unknown
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		HostNamePhrase = new Phrase("serverinfo.HostName", "Host Name");
		MaxPlayersPhrase = new Phrase("serverinfo.MaxPlayers", "Max Players");
		PlayersPhrase = new Phrase("serverinfo.Players", "Players");
		QueuedPhrase = new Phrase("serverinfo.Queued", "Queued");
		JoiningPhrase = new Phrase("serverinfo.Joining", "Joining");
		ReservedSlotsPhrase = new Phrase("serverinfo.ReservedSlots", "Reserved Slots");
		EntityCountPhrase = new Phrase("serverinfo.EntityCount", "Entity Count");
		GameTimePhrase = new Phrase("serverinfo.GameTime", "Game Time");
		UptimePhrase = new Phrase("serverinfo.Uptime", "Uptime");
		MapPhrase = new Phrase("serverinfo.Map", "Map");
		FrameratePhrase = new Phrase("serverinfo.Framerate", "Framerate");
		MemoryPhrase = new Phrase("serverinfo.Memory", "Memory");
		MemoryUsageSystemPhrase = new Phrase("serverinfo.MemoryUsageSystem", "System Memory Usage");
		CollectionsPhrase = new Phrase("serverinfo.Collections", "Garbage Collections");
		NetworkInPhrase = new Phrase("serverinfo.NetworkIn", "Network In");
		NetworkOutPhrase = new Phrase("serverinfo.NetworkOut", "Network Out");
		RestartingPhrase = new Phrase("serverinfo.Restarting", "Restarting");
		SaveCreatedTimePhrase = new Phrase("serverinfo.SaveCreatedTime", "Save Created Time");
		VersionPhrase = new Phrase("serverinfo.Version", "Version");
		ProtocolPhrase = new Phrase("serverinfo.Protocol", "Protocol");
	}
}
