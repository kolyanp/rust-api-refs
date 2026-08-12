using RTLTMPro;
using TMPro;

public class RTLTMP_InputField : TMP_InputField
{
	private static readonly FastStringBuilder inputBuilder;

	public bool changeAlignment = true;

	static RTLTMP_InputField()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		inputBuilder = new FastStringBuilder(2048);
	}
}
