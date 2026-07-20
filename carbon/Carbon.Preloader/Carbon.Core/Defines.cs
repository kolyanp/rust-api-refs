using System;
using System.IO;
using Carbon.Extensions;
using Doorstop;

namespace Carbon.Core;

[Serializable]
public class Defines
{
	internal static string root;

	internal static string _customRustRootFolder;

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
	}

	internal static void _initializeCommandLine()
	{
		if (!_commandLineInitialized)
		{
			_commandLineInitialized = true;
			try
			{
				root = Path.GetFullPath(Path.Combine(typeof(Entrypoint).Assembly.Location, "..", "..", ".."));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			_customRustRootFolder = "-carbon.rustrootdir".GetArgumentResult();
			_customRootFolder = "-carbon.rootdir".GetArgumentResult();
			_customCarbonConfigFolder = "-carbon.carbonconfigdir".GetArgumentResult();
			_customScriptFolder = "-carbon.scriptdir".GetArgumentResult();
			_customConfigFolder = "-carbon.configdir".GetArgumentResult();
			_customDataFolder = "-carbon.datadir".GetArgumentResult();
			_customModifierFolder = "-carbon.modifierdir".GetArgumentResult();
			_customLangFolder = "-carbon.langdir".GetArgumentResult();
			_customModuleFolder = "-carbon.moduledir".GetArgumentResult();
			_customExtensionsFolder = "-carbon.extdir".GetArgumentResult();
			_customLogsFolder = "-carbon.logdir".GetArgumentResult();
			_customProfilesFolder = "-carbon.profiledir".GetArgumentResult();
		}
	}

	public static string GetConfigFile()
	{
		_initializeCommandLine();
		return Path.Combine(GetCarbonConfigFolder(), "config.json");
	}

	public static string GetMonoProfilerConfigFile()
	{
		_initializeCommandLine();
		return Path.Combine(GetCarbonConfigFolder(), "config.profiler.json");
	}

	public static string GetCarbonAutoFile()
	{
		_initializeCommandLine();
		return Path.Combine(GetCarbonConfigFolder(), "config.auto.json");
	}

	public static string GetRootFolder()
	{
		_initializeCommandLine();
		string text = (string.IsNullOrEmpty(_customRootFolder) ? Path.Combine(root, "carbon") : _customRootFolder);
		Directory.CreateDirectory(text);
		return text;
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

	public static string GetDeveloperPatchedAssembliesFolder()
	{
		_initializeCommandLine();
		string fullPath = Path.GetFullPath(Path.Combine(GetDeveloperFolder(), "patched_assemblies"));
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
		return Path.GetFullPath(string.IsNullOrEmpty(_customRustRootFolder) ? root : _customRustRootFolder);
	}

	public static string GetRustManagedFolder()
	{
		_initializeCommandLine();
		return Path.Combine(new string[1] { Path.Combine(GetRustRootFolder(), "RustDedicated_Data", "Managed") });
	}
}
