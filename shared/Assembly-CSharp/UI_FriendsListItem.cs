using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_FriendsListItem : MonoBehaviour
{
	public HttpImage Avatar;

	public RawImage AvatarImage;

	public Image StatusIndicator;

	public RustText Name;

	public GameObject NicknameSection;

	public RustText Nickname;

	public RustText Subtitle;

	public Image PlatformIcon;

	public GameObject SteamAccountTag;

	public RustText SteamAccountName;

	public GameObject PendingFriendControls;

	public GameObject PartyOwnerIcon;

	public static Phrase InvitedToPartyPhrase = new Phrase("party_invite.invited_to_party", "has invited you to a party");

	static UI_FriendsListItem()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
	}
}
