using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace MySqlConnector;

public sealed class MySqlHelper
{
	[Obsolete("Use MySqlConnection.ClearAllPools or MySqlConnection.ClearAllPoolsAsync")]
	public static void ClearConnectionPools()
	{
		MySqlConnection.ClearAllPools();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static string EscapeString(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		StringBuilder stringBuilder = null;
		int num = -1;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if ((c == '"' || c == '\'' || c == '\\') ? true : false)
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder();
				}
				stringBuilder.Append(value, num + 1, i - (num + 1));
				stringBuilder.Append('\\');
				stringBuilder.Append(value[i]);
				num = i;
			}
		}
		stringBuilder?.Append(value, num + 1, value.Length - (num + 1));
		return stringBuilder?.ToString() ?? value;
	}
}
