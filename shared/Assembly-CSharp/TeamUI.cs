using System;
using Rust.UI;
using UnityEngine;

public class TeamUI : MonoBehaviour
{
	public static Phrase invitePhrase;

	public Canvas canvas;

	public RectTransform MemberPanel;

	public GameObject memberEntryPrefab;

	public GameObjectRef InviteFriendDialog;

	public TeamMemberElement[] elements;

	public GameObject NoTeamPanel;

	public GameObject TeamPanel;

	public GameObject LeaveTeamButton;

	public GameObject InviteAcceptPanel;

	public GameObject InviteButton;

	public RustText inviteText;

	public static bool dirty;

	[NonSerialized]
	public static ulong pendingTeamID;

	[NonSerialized]
	public static string pendingTeamLeaderName;

	public GameObject teamMemberDetailsPanel;

	public RustText selectedTeamMemberNameText;

	static TeamUI()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		invitePhrase = new Phrase("team.invited", "{0} has invited you to join a team");
		dirty = true;
	}
}
