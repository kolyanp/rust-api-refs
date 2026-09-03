using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

public class UI_ServerAdminPlayerInfo : UI_Window
{
	private static readonly Phrase MutePhrase;

	private static readonly Phrase PermanentPhrase;

	[SerializeField]
	[Space]
	private RawImage avatarImage;

	[SerializeField]
	private RustText headerNameText;

	[SerializeField]
	private RustText playerNameText;

	[SerializeField]
	private RustText steamIDText;

	[SerializeField]
	private RustText pingText;

	[SerializeField]
	private GameObject addressGroup;

	[SerializeField]
	private RustText addressText;

	[SerializeField]
	private RustText connectedTimeText;

	[SerializeField]
	private RustText violationLevelText;

	[SerializeField]
	private RustText healthText;

	[SerializeField]
	private RustText positionText;

	[SerializeField]
	private RustText teamIDText;

	[SerializeField]
	[Space]
	private RustInput muteReasonInput;

	[SerializeField]
	private RustInput kickReasonInput;

	[SerializeField]
	private RustInput banReasonInput;

	[SerializeField]
	private RustText muteButtonText;

	[SerializeField]
	private GameObject muteButton;

	[SerializeField]
	private GameObject unmuteButton;

	static UI_ServerAdminPlayerInfo()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		MutePhrase = new Phrase("playerinfo.mute", "Mute ({0})");
		PermanentPhrase = new Phrase("playerinfo.mutepermanent", "Permanent");
	}
}
