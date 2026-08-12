using Rust.UI;
using UnityEngine;

public class UIClanMemberDetails : UIClanMember
{
	public static readonly Phrase KickConfirmation;

	public static readonly Phrase SaveNotesInvalidLength;

	public static readonly Phrase SaveNotesFailure;

	public static readonly Phrase ChangeRankCannotDemoteLeader;

	public static readonly Phrase ChangeRankFailure;

	public static readonly Phrase KickFailure;

	public UIClans UiClans;

	public RustInput NoteEditor;

	public RustText NoteCharacterLimit;

	public RustButton SaveNoteButton;

	public GameObject ChangeRankSection;

	public Dropdown ChangeRankDropdown;

	public GameObject KickSection;

	public RustButton KickButton;

	static UIClanMemberDetails()
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
		KickConfirmation = new Phrase("clan.confirmation.kick", "Are you sure you want to kick this player out of your clan?");
		SaveNotesInvalidLength = new Phrase("clan.set_member_notes.invalid_length", "Clan member notes cannot be more than {0} characters long.");
		SaveNotesFailure = new Phrase("clan.set_member_notes.fail", "Failed to save your updated player notes.");
		ChangeRankCannotDemoteLeader = new Phrase("clan.change_member_rank.cannot_demote_leader", "As a clan leader, you cannot demote yourself unless you promote another clan member to the leader role.");
		ChangeRankFailure = new Phrase("clan.change_member_rank.fail", "Failed to change the rank of the player.");
		KickFailure = new Phrase("clan.kick_member.fail", "Failed to kick the player out of the clan.");
	}
}
