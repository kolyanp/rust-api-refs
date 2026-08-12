using UnityEngine;

namespace Rust.UI;

public class ReportPlayer : UIDialog
{
	public const string BreakServerRulesKey = "break_server_rules";

	public GameObject FindPlayer;

	public GameObject GetInformation;

	public GameObject Finished;

	public GameObject RecentlyReported;

	public Dropdown ReasonDropdown;

	public RustInput Subject;

	public RustInput Message;

	public RustButton ReportButton;

	public SteamUserButton SteamUserButton;

	public RustIcon ProgressIcon;

	public RustText ProgressText;

	public static Option[] ReportReasons;

	static ReportPlayer()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		ReportReasons = (Option[])(object)new Option[6]
		{
			new Option(new Phrase("report.reason.none", "Select an option"), "none", false, (Icons)61641),
			new Option(new Phrase("report.reason.abuse", "Racism/Sexism/Abusive"), "abusive", false, (Icons)62806),
			new Option(new Phrase("report.reason.cheat", "Cheating"), "cheat", false, (Icons)61531),
			new Option(new Phrase("report.reason.spam", "Spamming"), "spam", false, (Icons)61601),
			new Option(new Phrase("report.reason.name", "Offensive Name"), "name", false, (Icons)63417),
			new Option(new Phrase("report.reason.server_rules", "Breaking Server Rules"), "break_server_rules", false, (Icons)61546)
		};
	}
}
