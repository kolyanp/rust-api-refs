using UnityEngine.Rendering;

public static class Keyword
{
	public static readonly GlobalKeyword LIGHTPROBE_SH;

	public static readonly GlobalKeyword SHADOWS_SHADOWMASK;

	public static readonly GlobalKeyword INDIRECT_INSTANCING_ON;

	static Keyword()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		LIGHTPROBE_SH = GlobalKeyword.Create("LIGHTPROBE_SH");
		SHADOWS_SHADOWMASK = GlobalKeyword.Create("SHADOWS_SHADOWMASK");
		INDIRECT_INSTANCING_ON = GlobalKeyword.Create("INDIRECT_INSTANCING_ON");
	}
}
