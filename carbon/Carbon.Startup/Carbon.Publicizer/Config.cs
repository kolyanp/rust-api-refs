using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Carbon.Publicizer;

[Serializable]
public class Config
{
	public class PublicizerConfig
	{
		public List<string> PublicizedAssemblies { get; set; } = new List<string>();

		public List<string> PublicizerMemberIgnores { get; set; } = new List<string>(3) { "^HiddenValueBase$", "^HiddenValue`1$", "^Pool$" };

		public bool IsMemberIgnored(string name)
		{
			foreach (string publicizerMemberIgnore in PublicizerMemberIgnores)
			{
				if (Regex.IsMatch(name, publicizerMemberIgnore))
				{
					return true;
				}
			}
			return false;
		}
	}

	public static Config Singleton;

	public bool DeveloperMode { get; set; }

	public PublicizerConfig Publicizer { get; set; } = new PublicizerConfig();

	public void ForceEnsurePublicizedAssembly(string value)
	{
		if (!Publicizer.PublicizedAssemblies.Contains(value))
		{
			Publicizer.PublicizedAssemblies.Add(value);
		}
	}

	public static void Init(string configFile)
	{
		if (Singleton == null)
		{
			if (!File.Exists(configFile))
			{
				Singleton = new Config();
			}
			else
			{
				Singleton = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFile));
			}
			Singleton.ForceEnsurePublicizedAssembly("Assembly-CSharp.dll");
			Singleton.ForceEnsurePublicizedAssembly("Facepunch.Console.dll");
			Singleton.ForceEnsurePublicizedAssembly("Facepunch.Network.dll");
			Singleton.ForceEnsurePublicizedAssembly("Facepunch.Nexus.dll");
			Singleton.ForceEnsurePublicizedAssembly("Facepunch.Ping.dll");
			Singleton.ForceEnsurePublicizedAssembly("Facepunch.Unity.dll");
			Singleton.ForceEnsurePublicizedAssembly("Facepunch.Rcon.dll");
			Singleton.ForceEnsurePublicizedAssembly("Rust.Localization.dll");
			Singleton.ForceEnsurePublicizedAssembly("Rust.Clans.Local.dll");
			Singleton.ForceEnsurePublicizedAssembly("Rust.FileSystem.dll");
			Singleton.ForceEnsurePublicizedAssembly("Rust.Harmony.dll");
			Singleton.ForceEnsurePublicizedAssembly("Rust.Global.dll");
			Singleton.ForceEnsurePublicizedAssembly("Rust.Data.dll");
			Singleton.ForceEnsurePublicizedAssembly("Fleck.dll");
		}
	}
}
