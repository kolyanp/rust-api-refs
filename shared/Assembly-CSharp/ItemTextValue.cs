using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemTextValue : MonoBehaviour
{
	public Text text;

	public Color bad;

	public Color good;

	public bool negativestat;

	public bool asPercentage;

	public bool useColors = true;

	public bool signed = true;

	public string affix;

	public string suffix;

	public float multiplier = 1f;

	public float addition;

	public void SetValue(float val, int numDecimals = 0, string overrideText = "")
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		val = val * multiplier + addition;
		text.text = ((overrideText == "") ? string.Format("{0}{1:n" + numDecimals + "}", (val > 0f && signed) ? "+" : "", val) : overrideText);
		if (!string.IsNullOrEmpty(affix))
		{
			text.text = affix + text.text;
		}
		if (asPercentage)
		{
			Text obj = text;
			obj.text += " %";
		}
		if (suffix != "" && !float.IsPositiveInfinity(val))
		{
			Text obj2 = text;
			obj2.text += suffix;
		}
		bool flag = val > 0f;
		if (negativestat)
		{
			flag = !flag;
		}
		if (useColors)
		{
			((Graphic)text).color = (flag ? good : bad);
		}
	}

	public void SetValue(TimeSpan time)
	{
		text.text = TimeSpanEx.ToShortString(time);
	}

	public void SetValue(string display)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		text.text = display;
		if (!string.IsNullOrEmpty(affix))
		{
			text.text = affix + text.text;
		}
		bool flag = true;
		if (negativestat)
		{
			flag = !flag;
		}
		if (useColors)
		{
			((Graphic)text).color = (flag ? good : bad);
		}
	}
}
