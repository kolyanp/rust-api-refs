using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Carbon.Profiler;

[Serializable]
public class MonoProfilerConfig
{
	public enum ProfileTypes
	{
		Assembly,
		Plugin,
		Module,
		Extension,
		Harmony
	}

	public bool Enabled = false;

	public bool TrackCalls = false;

	public bool SourceViewer = false;

	public List<string> Assemblies = new List<string>();

	public List<string> Plugins = new List<string>();

	public List<string> Modules = new List<string>();

	public List<string> Extensions = new List<string>();

	public List<string> Harmony = new List<string>();

	public const string Star = "*";

	public static MonoProfilerConfig Instance { get; set; }

	public bool AppendProfile(ProfileTypes profile, string value)
	{
		if (1 == 0)
		{
		}
		bool result = profile switch
		{
			ProfileTypes.Assembly => Do(Assemblies, value), 
			ProfileTypes.Plugin => Do(Plugins, value), 
			ProfileTypes.Module => Do(Modules, value), 
			ProfileTypes.Extension => Do(Extensions, value), 
			ProfileTypes.Harmony => Do(Harmony, value), 
			_ => throw new ArgumentOutOfRangeException("profile", profile, null), 
		};
		if (1 == 0)
		{
		}
		return result;
		static bool Do(List<string> list, string item)
		{
			if (list.Contains(item))
			{
				return false;
			}
			list.Add(item);
			return true;
		}
	}

	public bool RemoveProfile(ProfileTypes profile, string value)
	{
		if (1 == 0)
		{
		}
		bool result = profile switch
		{
			ProfileTypes.Assembly => Do(Assemblies, value), 
			ProfileTypes.Plugin => Do(Plugins, value), 
			ProfileTypes.Module => Do(Modules, value), 
			ProfileTypes.Extension => Do(Extensions, value), 
			ProfileTypes.Harmony => Do(Harmony, value), 
			_ => throw new ArgumentOutOfRangeException("profile", profile, null), 
		};
		if (1 == 0)
		{
		}
		return result;
		static bool Do(List<string> list, string item)
		{
			if (!list.Contains(item))
			{
				return false;
			}
			list.Remove(item);
			return true;
		}
	}

	public bool IsWhitelisted(ProfileTypes profile, string value)
	{
		if (1 == 0)
		{
		}
		bool result = profile switch
		{
			ProfileTypes.Assembly => Assemblies.Contains("*") || Assemblies.Contains(value), 
			ProfileTypes.Plugin => Plugins.Contains("*") || Plugins.Contains(value), 
			ProfileTypes.Module => Modules.Contains("*") || Modules.Contains(value), 
			ProfileTypes.Extension => Extensions.Contains("*") || Extensions.Contains(value), 
			ProfileTypes.Harmony => Harmony.Contains("*") || Harmony.Contains(value), 
			_ => throw new ArgumentOutOfRangeException("profile", profile, null), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public static void Load(string filePath)
	{
		if (File.Exists(filePath))
		{
			Instance = JsonConvert.DeserializeObject<MonoProfilerConfig>(File.ReadAllText(filePath));
			return;
		}
		Instance = new MonoProfilerConfig();
		File.WriteAllText(filePath, JsonConvert.SerializeObject((object)Instance, (Formatting)1));
	}

	public static void Save(string filePath)
	{
		if (Instance == null)
		{
			Instance = new MonoProfilerConfig();
		}
		File.WriteAllText(filePath, JsonConvert.SerializeObject((object)Instance, (Formatting)1));
	}
}
