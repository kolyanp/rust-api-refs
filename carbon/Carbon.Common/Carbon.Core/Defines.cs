using System;
using System.IO;
using Carbon.Extensions;
using ConVar;
using UnityEngine;

namespace Carbon.Core;

[Serializable]
public class Defines
{
	internal static string _customRootFolder;

	internal static string _customCarbonConfigFolder;

	internal static string _customScriptFolder;

	internal static string _customConfigFolder;

	internal static string _customDataFolder;

	internal static string _customModifierFolder;

	internal static string _customLangFolder;

	internal static string _customModuleFolder;

	internal static string _customExtensionsFolder;

	internal static string _customProfilesFolder;

	internal static string _customLogsFolder;

	internal static bool _commandLineInitialized;

	public static void Initialize()
	{
		GetRootFolder();
		GetCarbonConfigFolder();
		GetConfigsFolder();
		GetModulesFolder();
		GetDataFolder();
		GetScriptsFolder();
		GetExtensionsFolder();
		GetLogsFolder();
		GetLangFolder();
		try
		{
			OsEx.Folder.DeleteContents(GetTempFolder());
		}
		catch (Exception ex)
		{
			Logger.Warn("Failed clearing up the temporary folder. (" + ex.Message + ")\n" + ex.StackTrace);
		}
		if (!Community.Runtime.Config.Logging.ReducedLogging)
		{
			Logger.Log("Loaded folders");
		}
	}

	internal static void _initializeCommandLine()
	{
		if (!_commandLineInitialized)
		{
			_commandLineInitialized = true;
			_customRootFolder = Switches.GetRootDir();
			_customCarbonConfigFolder = Switches.GetCarbonConfigDir();
			_customScriptFolder = Switches.GetScriptDir();
			_customConfigFolder = Switches.GetConfigDir();
			_customDataFolder = Switches.GetDataDir();
			_customModifierFolder = Switches.GetModifierDir();
			_customLangFolder = Switches.GetLangDir();
			_customModuleFolder = Switches.GetModuleDir();
			_customExtensionsFolder = Switches.GetExtDir();
			_customLogsFolder = Switches.GetLogDir();
			_customProfilesFolder = Switches.GetProfileDir();
		}
	}

	public static string GetConfigFile()
	{
		_initializeCommandLine();
		return Path.Combine(GetRootFolder(), "config.json");
	}

	public static string GetMonoProfilerConfigFile()
	{
		_initializeCommandLine();
		return Path.Combine(GetRootFolder(), "config.profiler.json");
	}

	public static string GetCarbonAutoFile()
	{
		_initializeCommandLine();
		return Path.Combine(GetRootFolder(), "config.auto.json");
	}

	public static string GetVaultFile()
	{
		return Path.Combine(GetRustIdentityFolder(), "carbon.vault");
	}

	public static string GetWebPanelConfigFile()
	{
		_initializeCommandLine();
		return Path.Combine(GetRootFolder(), "config.webpanel.json");
	}

	public static string GetRootFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(string.IsNullOrEmpty(_customRootFolder) ? Path.Combine(Application.dataPath + "/..", "carbon") : _customRootFolder);
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetCarbonConfigFolder()
	{
		_initializeCommandLine();
		string text = (string.IsNullOrEmpty(_customCarbonConfigFolder) ? GetRootFolder() : _customRootFolder);
		Directory.CreateDirectory(text);
		return text;
	}

	public static string GetCompilerFolder()
	{
		string text = Path.Combine(GetRootFolder() ?? "", "compiler");
		Directory.CreateDirectory(text);
		return text;
	}

	public static string GetLibFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(string.IsNullOrEmpty(_customModuleFolder) ? Path.Combine(GetManagedFolder(), "lib") : _customModuleFolder);
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetConfigsFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(string.IsNullOrEmpty(_customConfigFolder) ? Path.Combine(GetRootFolder(), "configs") : _customConfigFolder);
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetModulesFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(string.IsNullOrEmpty(_customModuleFolder) ? Path.Combine(GetRootFolder(), "modules") : _customModuleFolder);
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetManagedModulesFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(Path.Combine(GetManagedFolder(), "modules"));
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetDataFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(string.IsNullOrEmpty(_customDataFolder) ? Path.Combine(GetRootFolder(), "data") : _customDataFolder);
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetModifierFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(string.IsNullOrEmpty(_customModifierFolder) ? Path.Combine(GetRootFolder(), "modifiers") : _customModifierFolder);
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetScriptsFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(string.IsNullOrEmpty(_customScriptFolder) ? Path.Combine(GetRootFolder(), "plugins") : _customScriptFolder);
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetScriptBackupFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(Path.Combine(GetScriptsFolder(), "backups"));
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetScriptDebugFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(Path.Combine(GetScriptsFolder(), "debug"));
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetZipDevFolder()
	{
		string text = Path.Combine(GetScriptsFolder(), "cszip_dev");
		Directory.CreateDirectory(text);
		return text;
	}

	public static string GetExtensionsFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(string.IsNullOrEmpty(_customExtensionsFolder) ? Path.Combine(GetRootFolder(), "extensions") : _customExtensionsFolder);
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetDeveloperFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(Path.Combine(GetRootFolder(), "developer"));
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetManagedFolder()
	{
		_initializeCommandLine();
		string text = Path.Combine(GetRootFolder(), "managed");
		Directory.CreateDirectory(text);
		return text;
	}

	public static string GetHooksFolder()
	{
		_initializeCommandLine();
		string text = Path.Combine(GetManagedFolder(), "hooks");
		Directory.CreateDirectory(text);
		return text;
	}

	public static string GetLogsFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(string.IsNullOrEmpty(_customLogsFolder) ? Path.Combine(GetRootFolder(), "logs") : _customLogsFolder);
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetProfilesFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(string.IsNullOrEmpty(_customProfilesFolder) ? Path.Combine(GetRootFolder(), "profiles") : _customProfilesFolder);
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetLangFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(string.IsNullOrEmpty(_customLangFolder) ? Path.Combine(GetRootFolder(), "lang") : _customLangFolder);
		Directory.CreateDirectory(fullPath);
		return fullPath;
	}

	public static string GetTempFolder()
	{
		_initializeCommandLine();
		string text = Path.Combine(GetRootFolder() ?? "", "temp");
		Directory.CreateDirectory(text);
		return text;
	}

	public static string GetRustRootFolder()
	{
		_initializeCommandLine();
		return Path.GetFullPath(Path.Combine(new string[1] { Path.Combine(Application.dataPath, "..") }));
	}

	public static string GetRustManagedFolder()
	{
		_initializeCommandLine();
		return Path.GetFullPath(Path.Combine(new string[1] { Path.Combine(Application.dataPath, "Managed") }));
	}

	public static string GetRustIdentityFolder()
	{
		return Path.GetFullPath(Path.Combine(new string[1] { Path.Combine(Application.dataPath, "..", "server", Server.identity) }));
	}
}
