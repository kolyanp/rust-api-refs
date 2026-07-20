using System;
using System.Linq;
using API.Commands;
using Carbon.Extensions;
using Facepunch;

namespace Carbon.Components;

public static class CommandLine
{
	internal static readonly string[] _emptyArgs = new string[0];

	internal static readonly char[] _delimiter = new char[1] { '|' };

	public static void ExecuteCommands(string @switch, string context, string[] lines = null)
	{
		string argumentResult = (lines ?? Environment.GetCommandLineArgs()).GetArgumentResult(@switch, string.Empty);
		string[] array = argumentResult.Split(_delimiter, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 0)
		{
			Logger.Log(string.Format(" Executing {0:n0} {1} for the '{2}' switch ({3}):", new object[4]
			{
				array.Length,
				array.Length.Plural("command", "commands"),
				@switch,
				context
			}));
		}
		ExecuteCommands(array);
	}

	public static void ExecuteCommands(string[] commands)
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		foreach (string text in commands)
		{
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			string[] array = text.Split(' ');
			string name = ((array.Length != 0) ? array[0] : string.Empty);
			if (Community.Runtime.CommandManager.Contains(Community.Runtime.CommandManager.RCon, name, out var outCommand))
			{
				try
				{
					PlayerArgs playerArgs = Pool.Get<PlayerArgs>();
					playerArgs.Type = outCommand.Type;
					PlayerArgs playerArgs2 = playerArgs;
					object[] arguments = array.Skip(1).ToArray();
					playerArgs2.Arguments = arguments;
					playerArgs.PrintOutput = true;
					playerArgs.IsServer = true;
					playerArgs.IsRCon = true;
					Community.Runtime.CommandManager.Execute(outCommand, playerArgs);
					Pool.Free<PlayerArgs>(ref playerArgs);
				}
				catch (Exception ex)
				{
					Logger.Error("Failed executing Carbon command '" + text + "'", ex);
				}
				continue;
			}
			try
			{
				if (Index.All.Any((Command x) => x.FullName == name.Replace("+", string.Empty).Replace("-", string.Empty)))
				{
					ConsoleSystem.Run(Option.Server, text, Array.Empty<object>());
				}
			}
			catch (Exception ex2)
			{
				Logger.Error("Failed executing native command '" + text + "'", ex2);
			}
		}
	}
}
