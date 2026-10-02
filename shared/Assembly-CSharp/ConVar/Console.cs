using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Facepunch;
using UnityEngine;

namespace ConVar;

[Factory("console")]
public class Console : ConsoleSystem
{
	private static int consolehistorysizeValue = 50;

	[ServerVar(Help = "Maximum number of console history entries")]
	public static int consolehistorysize
	{
		get
		{
			return consolehistorysizeValue;
		}
		set
		{
			consolehistorysizeValue = Mathf.Max(value, 0);
			if ((Object)(object)SingletonComponent<ServerConsole>.Instance != (Object)null && SingletonComponent<ServerConsole>.Instance.input != null)
			{
				SingletonComponent<ServerConsole>.Instance.input.TrimHistory(consolehistorysizeValue);
			}
		}
	}

	[Help("Return the last x lines of the console. Default is 200")]
	[ServerVar]
	public static IEnumerable<Output.Entry> tail(Arg arg)
	{
		int num = arg.GetInt(0, 200);
		int num2 = Output.HistoryOutput.Count - num;
		if (num2 < 0)
		{
			num2 = 0;
		}
		return Output.HistoryOutput.Skip(num2);
	}

	[Help("Search the console for a particular string")]
	[ServerVar]
	public static IEnumerable<Output.Entry> search(Arg arg)
	{
		string search = arg.GetString(0, null);
		if (search == null)
		{
			return Enumerable.Empty<Output.Entry>();
		}
		return Output.HistoryOutput.Where((Output.Entry x) => x.Message.Length < 4096 && StringEx.Contains(x.Message, search, CompareOptions.IgnoreCase));
	}
}
