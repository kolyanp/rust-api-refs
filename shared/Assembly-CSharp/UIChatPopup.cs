using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIChatPopup : MonoBehaviour
{
	public static Phrase MutePhrase;

	public static Phrase UnmutePhrase;

	public static Phrase MutedGlobalChatPhrase;

	public static Phrase UnmutedGlobalChatPhrase;

	public UIChat Chat;

	public RustText TextToggleMute;

	public RustText TextToggleGlobalMute;

	public Button SendMessageButton;

	public Button SteamProfileButton;

	public Button MuteButton;

	public Button ReportButton;

	public GameObject AddFriendRow;

	public Button AddSteamFriendButton;

	public Button AddDiscordFriendButton;

	public GameObject InviteToTeamButton;

	public GameObject ViewInDiscordButton;

	public GameObject AcceptInviteButton;

	static UIChatPopup()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		MutePhrase = new Phrase("chat.mute", "Mute");
		UnmutePhrase = new Phrase("chat.unmute", "Unmute");
		MutedGlobalChatPhrase = new Phrase("chat.mutedglobal", "Muted global chat.");
		UnmutedGlobalChatPhrase = new Phrase("chat.unmutedglobal", "Unmuted global chat.");
	}
}
