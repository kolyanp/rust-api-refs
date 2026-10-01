using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIChatPopup : MonoBehaviour
{
	public static Phrase MutePhrase = new Phrase("chat.mute", "Mute");

	public static Phrase UnmutePhrase = new Phrase("chat.unmute", "Unmute");

	public static Phrase MutedGlobalChatPhrase = new Phrase("chat.mutedglobal", "Muted global chat.");

	public static Phrase UnmutedGlobalChatPhrase = new Phrase("chat.unmutedglobal", "Unmuted global chat.");

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
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected Obj, but got Unknown
	}
}
