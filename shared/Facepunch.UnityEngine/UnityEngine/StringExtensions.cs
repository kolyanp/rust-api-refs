using Facepunch;
using Facepunch.Extend;

namespace UnityEngine;

public static class StringExtensions
{
	public static string BBCodeToUnity(this string x)
	{
		x = x.Replace("[", "<");
		x = x.Replace("]", ">");
		return x;
	}

	public static Vector3 ToVector3(this string str)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return StringView.op_Implicit(str).ToVector3();
	}

	public static Vector3 ToVector3(this StringView str)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		str = ((StringView)(ref str)).Trim('(', ')', ' ');
		int num = ((StringView)(ref str)).IndexOfAny(StringView.op_Implicit(" ,"));
		if (num == -1)
		{
			return default(Vector3);
		}
		StringView val = ((StringView)(ref str)).Substring(0, num);
		StringView val2 = ((StringView)(ref val)).Trim(' ', ',');
		val = ((StringView)(ref str)).Substring(num + 1);
		str = ((StringView)(ref val)).Trim(' ', ',');
		num = ((StringView)(ref str)).IndexOfAny(StringView.op_Implicit(" ,"));
		if (num == -1)
		{
			return default(Vector3);
		}
		val = ((StringView)(ref str)).Substring(0, num);
		StringView val3 = ((StringView)(ref val)).Trim(' ', ',');
		val = ((StringView)(ref str)).Substring(num + 1);
		StringView val4 = ((StringView)(ref val)).Trim(' ', ',');
		return new Vector3(StringExtensions.ToFloat(val2, 0f), StringExtensions.ToFloat(val3, 0f), StringExtensions.ToFloat(val4, 0f));
	}

	public static Color ToColor(this string str)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return StringView.op_Implicit(str).ToColor();
	}

	public static Color ToColor(this StringView str)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		int num = ((StringView)(ref str)).IndexOf(StringView.op_Implicit(","));
		if (num == -1)
		{
			return default(Color);
		}
		StringView val = ((StringView)(ref str)).Substring(0, num);
		str = ((StringView)(ref str)).Substring(num + 1);
		num = ((StringView)(ref str)).IndexOf(StringView.op_Implicit(","));
		if (num == -1)
		{
			return default(Color);
		}
		StringView val2 = ((StringView)(ref str)).Substring(0, num);
		str = ((StringView)(ref str)).Substring(num + 1);
		num = ((StringView)(ref str)).IndexOf(StringView.op_Implicit(","));
		if (num == -1)
		{
			StringView val3 = str;
			return new Color(StringExtensions.ToFloat(val, 0f), StringExtensions.ToFloat(val2, 0f), StringExtensions.ToFloat(val3, 0f));
		}
		StringView val4 = ((StringView)(ref str)).Substring(0, num);
		StringView val5 = ((StringView)(ref str)).Substring(num + 1);
		return new Color(StringExtensions.ToFloat(val, 0f), StringExtensions.ToFloat(val2, 0f), StringExtensions.ToFloat(val4, 0f), StringExtensions.ToFloat(val5, 0f));
	}
}
