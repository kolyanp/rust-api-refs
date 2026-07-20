using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using Carbon.Core;
using Carbon.Extensions;
using Mono.Cecil;

namespace Doorstop.Utility;

public static class SelfUpdater
{
	private enum OsType
	{
		Windows,
		Linux
	}

	private enum ReleaseType
	{
		Edge,
		Experimental,
		RustRelease,
		RustStaging,
		RustAux01,
		RustAux02,
		RustAux03,
		RustAux04,
		Production
	}

	private const string Repository = "CarbonCommunity/Carbon";

	private const string CarbonVersionsEndpoint = "https://api.carbonmod.gg/releases";

	private static OsType Platform;

	private static ReleaseType Release;

	private static string Target;

	private static bool IsMinimal;

	private static Version LocalCarbonProtocol;

	private static Version LocalRustProtocol;

	private static readonly string[] Files = new string[2] { "carbon/managed", "carbon/native" };

	private static string Tag => Release switch
	{
		ReleaseType.Edge => "edge_build", 
		ReleaseType.Experimental => "experimental_build", 
		ReleaseType.RustRelease => "rustbeta_release_build", 
		ReleaseType.RustStaging => "rustbeta_staging_build", 
		ReleaseType.RustAux01 => "rustbeta_aux01_build", 
		ReleaseType.RustAux02 => "rustbeta_aux02_build", 
		ReleaseType.RustAux03 => "rustbeta_aux03_build", 
		ReleaseType.RustAux04 => "rustbeta_aux04_build", 
		ReleaseType.Production => "production_build", 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	private static string File => Platform switch
	{
		OsType.Windows => "Carbon.Windows." + Target + ".zip", 
		OsType.Linux => "Carbon.Linux." + Target + ".tar.gz", 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	private static string LocalProtocolFile => Path.Combine(Defines.GetRootFolder(), ".protocol");

	internal static void Init()
	{
		OsType platform = ((!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? OsType.Linux : OsType.Windows);
		Platform = platform;
		Release = ReleaseType.Production;
		IsMinimal = false;
		Target = (IsMinimal ? "Minimal" : "Release");
		if (System.IO.File.Exists(LocalProtocolFile))
		{
			string[] array = System.IO.File.ReadAllLines(LocalProtocolFile);
			if (array.Length >= 1 && Version.TryParse(array[0], out Version result))
			{
				LocalRustProtocol = result;
			}
			if (array.Length >= 2 && Version.TryParse(array[1], out Version result2))
			{
				LocalCarbonProtocol = result2;
			}
		}
	}

	internal static void Execute()
	{
		string versionOverride = GetVersionOverride();
		bool flag = !string.IsNullOrEmpty(versionOverride);
		Versions.VersionValue version = Versions.GetVersion(Tag);
		if (version == null || string.IsNullOrEmpty(version.Version))
		{
			return;
		}
		if (!flag && version.Version.Equals(Versions.CurrentVersion))
		{
			Logger.Log(string.Format(" Carbon {0} is up to date, no self-updating necessary. Running {1} build [{2}] on tag '{3}'.", new object[4]
			{
				Target,
				Release,
				Versions.CurrentVersion,
				Tag
			}));
			return;
		}
		string currentRustProtocol = string.Empty;
		if (!flag && !HasValidLocalProtocol(version, out currentRustProtocol))
		{
			Logger.Log(" Skipped self-updating since the pending Carbon update has changed its protocol. Update the Rust server to self-update!");
			return;
		}
		string text = versionOverride ?? GithubReleaseUrl();
		if (flag)
		{
			Logger.Log(string.Format(" Carbon version override detected and now self-updating - {0} [{1}] on {2} [{3}]", new object[4] { Release, Tag, Platform, text }));
		}
		else
		{
			Logger.Log(string.Format(" Carbon {0} is out of date and now self-updating - {1} [{2}] on {3} [{4} -> {5}]", new object[6]
			{
				Target,
				Release,
				Tag,
				Platform,
				Versions.CurrentVersion,
				version.Version
			}));
		}
		if (!flag)
		{
			WriteLocalProtocol(currentRustProtocol, version.Protocol);
		}
		OsEx.ExecuteProcess("curl", "-s -H \"Cache-Control: no-store, no-cache, must-revalidate, max-age=0\" -H \"Pragma: no-cache\" -fSL -o \"" + Path.Combine(Defines.GetTempFolder(), "patch.zip") + "\" \"" + text + "\"");
		int num = 0;
		try
		{
			string archiveFileName = Path.Combine(Defines.GetTempFolder(), "patch.zip");
			string rootFolder = Defines.GetRootFolder();
			Console.Write(" Updating Carbon... ");
			using ZipArchive zipArchive = ZipFile.OpenRead(archiveFileName);
			foreach (ZipArchiveEntry entry in zipArchive.Entries)
			{
				if (string.IsNullOrEmpty(entry.Name) || !Files.Any((string x) => entry.FullName.Contains(x)))
				{
					continue;
				}
				string text2 = entry.FullName.Replace("carbon/", string.Empty).Replace("carbon\\", string.Empty);
				string path = Path.Combine(rootFolder, text2);
				string directoryName = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				try
				{
					using (FileStream destination = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
					{
						using Stream stream = entry.Open();
						stream.CopyTo(destination);
					}
					Console.Write(Environment.NewLine + " - " + text2 + " (" + entry.Length.Format().ToUpper() + ")");
				}
				catch
				{
					Console.Write(Environment.NewLine + " File used by another process, skipping '" + text2 + "' (" + entry.Length.Format().ToUpper() + ")");
				}
				num++;
			}
			Console.WriteLine(string.Empty);
		}
		catch (Exception ex)
		{
			Logger.Error($"Error while updating 'Carbon [{Platform}]'", ex);
		}
		if (flag)
		{
			Logger.Log($" Carbon finished self-updating the custom version override with {num:n0} files. You're now running the latest build.");
		}
		else
		{
			Logger.Log($" Carbon {Target} finished self-updating {num:n0} files. You're now running the latest {Release} build.");
		}
	}

	internal static bool HasValidLocalProtocol(Versions.VersionValue version, out string currentRustProtocol)
	{
		using MemoryStream memoryStream = new MemoryStream(System.IO.File.ReadAllBytes(Path.Combine(Defines.GetRustManagedFolder(), "Rust.Global.dll")));
		AssemblyDefinition val = AssemblyDefinition.ReadAssembly((Stream)memoryStream);
		TypeDefinition type = val.MainModule.GetType("Rust.Protocol");
		object constant = ((IEnumerable<FieldDefinition>)type.Fields).FirstOrDefault((FieldDefinition x) => ((MemberReference)x).Name == "network").Constant;
		object constant2 = ((IEnumerable<FieldDefinition>)type.Fields).FirstOrDefault((FieldDefinition x) => ((MemberReference)x).Name == "save").Constant;
		object constant3 = ((IEnumerable<FieldDefinition>)type.Fields).FirstOrDefault((FieldDefinition x) => ((MemberReference)x).Name == "report").Constant;
		currentRustProtocol = $"{constant}.{constant2}.{constant3}";
		Version version2 = Version.Parse(currentRustProtocol);
		val.Dispose();
		val = null;
		if (LocalCarbonProtocol == null || LocalRustProtocol == null)
		{
			return true;
		}
		if (!Version.TryParse(version.Protocol, out Version result))
		{
			return true;
		}
		if (LocalCarbonProtocol.Revision < result.Revision)
		{
			return true;
		}
		if (LocalRustProtocol != version2)
		{
			return true;
		}
		if (LocalCarbonProtocol == result)
		{
			return true;
		}
		return false;
	}

	internal static void WriteLocalProtocol(string rustProtocol, string carbonProtocol)
	{
		System.IO.File.WriteAllText(LocalProtocolFile, rustProtocol + "\n" + carbonProtocol);
	}

	internal static bool GetCarbonVersions()
	{
		string text = Path.Combine(Defines.GetTempFolder(), "versions.json");
		if (OsEx.ExecuteProcess("curl", "-s -H \"Cache-Control: no-store, no-cache, must-revalidate, max-age=0\" -H \"Pragma: no-cache\" -fSL -o \"" + text + "\" \"https://api.carbonmod.gg/releases\""))
		{
			return Versions.Init(System.IO.File.ReadAllText(text));
		}
		return false;
	}

	internal static string GithubReleaseUrl()
	{
		return "http://github.com/CarbonCommunity/Carbon/releases/download/" + Tag + "/" + File;
	}

	internal static string GetVersionOverride()
	{
		string path = Path.Combine(Defines.GetTempFolder(), "versionoverride.txt");
		if (System.IO.File.Exists(path))
		{
			string result = System.IO.File.ReadAllText(path);
			System.IO.File.Delete(path);
			return result;
		}
		if (!string.IsNullOrEmpty(Config.Singleton.SelfUpdating.RedirectUri))
		{
			return Config.Singleton.SelfUpdating.RedirectUri;
		}
		return null;
	}
}
