using System.Collections.Generic;
using System.Linq;
using Facepunch;
using UnityEngine;

public class Commands : ConsoleSystem
{
	[ClientVar]
	[ServerVar]
	public static void Find(Arg arg)
	{
		if (!arg.HasArgs())
		{
			return;
		}
		string str = arg.GetString(0);
		IEnumerable<Command> enumerable = Index.All.Where((Command x) => x.Description.Contains(str) || x.FullName.Contains(str) || x.Arguments.Contains(str));
		string text = "";
		string text2 = "";
		foreach (Command item in enumerable)
		{
			if (arg.CanSeeInFind(item))
			{
				if (!item.Variable || item.GetOveride == null)
				{
					string arg2 = $"{item.FullName}( {item.Arguments} )";
					text2 += $" {arg2} {item.Description}\n";
				}
				else
				{
					text += $" {item.FullName.PadRight(24)} {item.Description} ({item.String})\n";
				}
			}
		}
		arg.ReplyWith("Variables:\n" + text + "\nCommands:\n" + text2);
	}

	[ServerVar]
	[ClientVar(AllowRunFromServer = true)]
	public static void Echo(StringView fullString)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)fullString);
	}
}
