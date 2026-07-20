using System;
using System.Text;
using Facepunch;
using Facepunch.Extend;
using UnityEngine;

namespace Carbon.Extensions;

public static class ConsoleArgEx
{
	public static char[] CommandSpacing = new char[1] { ' ' };

	public static bool TryParseCommand(string input, out string command, out object[] args)
	{
		if (input == null)
		{
			command = string.Empty;
			args = Array.Empty<object>();
			return false;
		}
		return TryParseCommand(input.AsSpan(), out command, out args);
	}

	public static bool TryParseCommand(string input, object[] extraArgs, out string command, out object[] args)
	{
		if (!TryParseCommand(input, out command, out var args2))
		{
			args = Array.Empty<object>();
			return false;
		}
		int num = ((extraArgs != null) ? extraArgs.Length : 0);
		if (num == 0)
		{
			args = args2;
			return true;
		}
		args = new object[args2.Length + num];
		Array.Copy(args2, args, args2.Length);
		for (int i = 0; i < num; i++)
		{
			args[args2.Length + i] = extraArgs[i]?.ToString();
		}
		return true;
	}

	public static bool TryParseCommand(ReadOnlySpan<char> input, out string command, out object[] args)
	{
		command = string.Empty;
		args = Array.Empty<object>();
		if (input.IsEmpty)
		{
			return false;
		}
		bool flag = false;
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < input.Length; i++)
		{
			char ch = input[i];
			if (IsCommandSpacing(ch))
			{
				if (flag)
				{
					num2 = i;
					break;
				}
				num = i;
			}
			else if (!flag)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return false;
		}
		int num3 = num + 1;
		int num4 = ((num2 == -1) ? input.Length : num2);
		int num5 = num3;
		command = input.Slice(num5, num4 - num5).ToString();
		if (num2 == -1)
		{
			return true;
		}
		int num6 = -1;
		for (int j = num4; j < input.Length; j++)
		{
			if (!IsCommandSpacing(input[j]))
			{
				num6 = j;
				break;
			}
		}
		if (num6 == -1)
		{
			return true;
		}
		object[] array = StringExtensions.SplitQuotesStrings(input.Slice(num6).ToString(), int.MaxValue);
		args = array;
		return true;
	}

	public static bool IsPlayerCalledOrAdmin(this Arg arg)
	{
		if (!((Object)(object)ArgEx.Player(arg) == (Object)null))
		{
			return arg.IsAdmin;
		}
		return true;
	}

	public static string GetFullString(this Arg arg, int startIndex = 0)
	{
		if (arg.Args == null || arg.Args.Length <= startIndex)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		for (int i = startIndex; i < arg.Args.Length; i++)
		{
			if (i > startIndex)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(arg.GetString(i, ""));
		}
		string result = stringBuilder.ToString();
		Pool.FreeUnmanaged(ref stringBuilder);
		return result;
	}

	private static bool IsCommandSpacing(char ch)
	{
		for (int i = 0; i < CommandSpacing.Length; i++)
		{
			if (ch == CommandSpacing[i])
			{
				return true;
			}
		}
		return false;
	}
}
