using Rust.UI;
using UnityEngine;

public class UI_ServerAdminServerInfo : MonoBehaviour
{
	[SerializeField]
	private RustText InfoName;

	[SerializeField]
	private RustText InfoValue;

	private static Phrase HostNamePhrase = new Phrase("serverinfo.HostName", "Host Name");

	private static Phrase MaxPlayersPhrase = new Phrase("serverinfo.MaxPlayers", "Max Players");

	private static Phrase PlayersPhrase = new Phrase("serverinfo.Players", "Players");

	private static Phrase QueuedPhrase = new Phrase("serverinfo.Queued", "Queued");

	private static Phrase JoiningPhrase = new Phrase("serverinfo.Joining", "Joining");

	private static Phrase ReservedSlotsPhrase = new Phrase("serverinfo.ReservedSlots", "Reserved Slots");

	private static Phrase EntityCountPhrase = new Phrase("serverinfo.EntityCount", "Entity Count");

	private static Phrase GameTimePhrase = new Phrase("serverinfo.GameTime", "Game Time");

	private static Phrase UptimePhrase = new Phrase("serverinfo.Uptime", "Uptime");

	private static Phrase MapPhrase = new Phrase("serverinfo.Map", "Map");

	private static Phrase FrameratePhrase = new Phrase("serverinfo.Framerate", "Framerate");

	private static Phrase MemoryPhrase = new Phrase("serverinfo.Memory", "Memory");

	private static Phrase MemoryUsageSystemPhrase = new Phrase("serverinfo.MemoryUsageSystem", "System Memory Usage");

	private static Phrase CollectionsPhrase = new Phrase("serverinfo.Collections", "Garbage Collections");

	private static Phrase NetworkInPhrase = new Phrase("serverinfo.NetworkIn", "Network In");

	private static Phrase NetworkOutPhrase = new Phrase("serverinfo.NetworkOut", "Network Out");

	private static Phrase RestartingPhrase = new Phrase("serverinfo.Restarting", "Restarting");

	private static Phrase SaveCreatedTimePhrase = new Phrase("serverinfo.SaveCreatedTime", "Save Created Time");

	private static Phrase VersionPhrase = new Phrase("serverinfo.Version", "Version");

	private static Phrase ProtocolPhrase = new Phrase("serverinfo.Protocol", "Protocol");

	static UI_ServerAdminServerInfo()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected Obj, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected Obj, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected Obj, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected Obj, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected Obj, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected Obj, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected Obj, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected Obj, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected Obj, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected Obj, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected Obj, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected Obj, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected Obj, but got Unknown
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected Obj, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Expected Obj, but got Unknown
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected Obj, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected Obj, but got Unknown
	}
}
