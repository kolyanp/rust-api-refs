using System.Text;
using Rust.UI;

public class NucleusInformationPanel : ItemInformationPanel
{
	public InfoBar xpDisplay;

	public RustText gradeLabel;

	public RustText nextLevelLabel;

	public static readonly Phrase GradePhrase;

	public static readonly Phrase XPPhrase;

	public static readonly Phrase XPRequiredPhrase;

	public static readonly Phrase MaxPhrase;

	private static StringBuilder builder;

	static NucleusInformationPanel()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		GradePhrase = new Phrase("nucleus.grade", "GRADE {0}");
		XPPhrase = new Phrase("nucleus.xp", "{0} XP");
		XPRequiredPhrase = new Phrase("nucleus.required", "{0} XP REQUIRED");
		MaxPhrase = new Phrase("nucleus.max", "MAX LEVEL");
	}
}
