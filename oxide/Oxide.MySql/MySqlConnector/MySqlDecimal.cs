using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace MySqlConnector;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
public readonly struct MySqlDecimal
{
	private static readonly Regex s_pattern = new Regex("^-?([0-9]+)(\\.([0-9]+))?$");

	private readonly string m_value;

	public decimal Value => decimal.Parse(m_value, CultureInfo.InvariantCulture);

	public double ToDouble()
	{
		return double.Parse(m_value, CultureInfo.InvariantCulture);
	}

	public override string ToString()
	{
		return m_value;
	}

	internal MySqlDecimal(string value)
	{
		Match match = s_pattern.Match(value);
		if (match != null && match.Success)
		{
			int length = match.Groups[1].Length;
			int length2 = match.Groups[3].Value.TrimEnd(new char[1] { '0' }).Length;
			bool num = length + length2 <= 65 && length2 <= 30;
			bool flag = value[0] == '-' && match.Groups[1].Value == "0" && length2 == 0;
			if (num && !flag)
			{
				m_value = value;
				return;
			}
		}
		throw new FormatException("Could not parse the value as a MySqlDecimal: " + value);
	}
}
