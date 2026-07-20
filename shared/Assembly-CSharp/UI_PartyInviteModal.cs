using System.Threading;
using Rust.UI;
using UnityEngine;

public class UI_PartyInviteModal : SingletonComponent<UI_PartyInviteModal>
{
	public RustText InviteLabel;

	public RustButton AcceptButton;

	public static Phrase InvitePhrase = new Phrase("party.invite", "{0} has invited you to a party");

	private ulong pendingLobbyId;

	private TimeSince age;

	public HttpImage ProfilePicture;

	public RectTransform ProgressBar;

	private CancellationTokenSource cancel;

	private ulong lastUserId;

	public bool IsShown => ((Component)this).gameObject.activeInHierarchy;

	public void Show(string username, ulong userId, ulong lobbyId)
	{
	}

	public void OnClientStartup()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (lastUserId != 0L)
		{
			age = TimeSince.op_Implicit(0f);
		}
	}

	public void Hide()
	{
	}

	[UnityEvent]
	public void OnAcceptButtonClicked()
	{
	}
}
