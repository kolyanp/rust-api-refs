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

	static NucleusInformationPanel()
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
