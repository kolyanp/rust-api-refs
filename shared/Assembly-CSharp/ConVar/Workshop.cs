using System;
using Facepunch;

namespace ConVar;

[Factory("workshop")]
public class Workshop : ConsoleSystem
{
	[ServerVar(Help = "(Generated) Prints a list of all workshop-approved skins on the server with their item short names and approved skin IDs")]
	public static void print_approved_skins(Arg arg)
	{
		if (!PlatformService.Instance.IsValid || PlatformService.Instance.ItemDefinitions == null)
		{
			return;
		}
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.AddColumn("name");
			val.AddColumn("itemshortname");
			val.AddColumn("workshopid");
			val.AddColumn("workshopdownload");
			foreach (IPlayerItemDefinition itemDefinition in PlatformService.Instance.ItemDefinitions)
			{
				string name = itemDefinition.Name;
				string itemShortName = itemDefinition.ItemShortName;
				string text = itemDefinition.WorkshopId.ToString();
				string text2 = itemDefinition.WorkshopDownload.ToString();
				val.AddRow(new string[4] { name, itemShortName, text, text2 });
			}
			arg.ReplyWith(flag ? val.ToJson(true) : ((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
