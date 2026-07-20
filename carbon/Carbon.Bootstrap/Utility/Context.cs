using System;
using System.IO;
using Carbon;

namespace Utility;

internal sealed class Context
{
	private static readonly string[] Needles;

	internal static readonly string Game;

	internal static readonly string GameManaged;

	internal static readonly string Carbon;

	internal static readonly string CarbonData;

	internal static readonly string CarbonExtensions;

	internal static readonly string CarbonHooks;

	internal static readonly string CarbonLib;

	internal static readonly string CarbonLogs;

	internal static readonly string CarbonManaged;

	internal static readonly string CarbonModules;

	internal static readonly string CarbonPlugins;

	internal static readonly string CarbonConfig;

	static Context()
	{
		Needles = new string[3] { ".", "..", "../.." };
		Game = null;
		string[] needles = Needles;
		foreach (string path in needles)
		{
			string fullPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
			if (Directory.Exists(Path.Combine(fullPath, "RustDedicated_Data")))
			{
				Game = fullPath;
				break;
			}
		}
		try
		{
			if (Game == null)
			{
				throw new Exception("Unable to find root folder");
			}
			GameManaged = Path.GetFullPath(Path.Combine(Game, "RustDedicated_Data", "Managed"));
			Carbon = Path.GetFullPath(Switches.GetRootDir(Path.Combine(Game, "carbon")));
			if (!Directory.Exists(Carbon))
			{
				throw new Exception("Carbon folder is missing");
			}
			CarbonData = Switches.GetDataDir(Path.Combine(Carbon, "data"));
			if (!Directory.Exists(CarbonData))
			{
				Directory.CreateDirectory(CarbonData);
			}
			CarbonExtensions = Switches.GetExtDir(Path.Combine(Carbon, "extensions"));
			if (!Directory.Exists(CarbonExtensions))
			{
				Directory.CreateDirectory(CarbonExtensions);
			}
			CarbonHooks = Path.Combine(Carbon, "managed", "hooks");
			if (!Directory.Exists(CarbonHooks))
			{
				Directory.CreateDirectory(CarbonHooks);
			}
			CarbonLib = Path.Combine(Carbon, "managed", "lib");
			if (!Directory.Exists(CarbonLib))
			{
				Directory.CreateDirectory(CarbonLib);
			}
			CarbonLogs = Path.Combine(Carbon, "logs");
			if (!Directory.Exists(CarbonLogs))
			{
				Directory.CreateDirectory(CarbonLogs);
			}
			CarbonManaged = Path.Combine(Carbon, "managed");
			if (!Directory.Exists(CarbonManaged))
			{
				Directory.CreateDirectory(CarbonManaged);
			}
			CarbonModules = Path.Combine(Carbon, "managed", "modules");
			if (!Directory.Exists(CarbonModules))
			{
				Directory.CreateDirectory(CarbonModules);
			}
			CarbonPlugins = Switches.GetScriptDir(Path.Combine(Carbon, "plugins"));
			if (!Directory.Exists(CarbonPlugins))
			{
				Directory.CreateDirectory(CarbonPlugins);
			}
			CarbonConfig = Path.Combine(Switches.GetCarbonConfigDir(Carbon), "config.json");
		}
		catch (Exception ex)
		{
			Logger.Error("Critical error while loading Carbon", ex);
			Environment.Exit(1);
			throw;
		}
	}
}
