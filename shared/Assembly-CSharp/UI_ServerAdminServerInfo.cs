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
}
