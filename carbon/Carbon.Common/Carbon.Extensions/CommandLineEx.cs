using System;

namespace Carbon.Extensions;

public static class CommandLineEx
{
	public static string GetArgumentResult(this string[] args, string argument, string Default = null)
	{
		string text = string.Empty;
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == argument)
			{
				if (!string.IsNullOrEmpty(args[i + 1]))
				{
					return args[i + 1];
				}
				return Default;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = Default;
		}
		return text;
	}

	public static string GetArgumentResult(this string argument, string Default = null)
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == argument)
			{
				if (!string.IsNullOrEmpty(commandLineArgs[i + 1]))
				{
					return commandLineArgs[i + 1];
				}
				return Default;
			}
		}
		return Default;
	}

	public static bool GetArgumentExists(this string[] args, string argument)
	{
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == argument)
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetArgumentExists(this string argument)
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == argument)
			{
				return true;
			}
		}
		return false;
	}
}
