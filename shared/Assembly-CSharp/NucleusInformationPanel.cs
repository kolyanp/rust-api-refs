using System.Text;
using Rust.UI;

public class NucleusInformationPanel : ItemInformationPanel
{
	public InfoBar xpDisplay;

	public RustText gradeLabel;

	public RustText nextLevelLabel;

	public static readonly Phrase GradePhrase = new Phrase("nucleus.grade", "GRADE {0}");

	public static readonly Phrase XPPhrase = new Phrase("nucleus.xp", "{0} XP");

	public static readonly Phrase XPRequiredPhrase = new Phrase("nucleus.required", "{0} XP REQUIRED");

	public static readonly Phrase MaxPhrase = new Phrase("nucleus.max", "MAX LEVEL");

	private static StringBuilder builder;
}
