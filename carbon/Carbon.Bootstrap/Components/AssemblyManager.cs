using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using API.Abstracts;
using API.Assembly;
using API.Commands;
using Carbon.Compat;
using Loaders;
using UnityEngine;

namespace Components;

internal sealed class AssemblyManager : CarbonBehaviour, IAssemblyManager
{
	private LibraryLoader _library;

	private static readonly IReadOnlyList<string> _blacklistLibs = new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "^Carbon\\.Bootstrap|Preloader$", "^Carbon\\..+_\\d{4}\\.\\d{2}\\.\\d{2}\\.\\d{4}$" });

	private static readonly IReadOnlyList<string> _whitelistLibs = new global::_003C_003Ez__ReadOnlyArray<string>(new string[88]
	{
		"mscorlib", "netstandard", "System.Core", "System.Data", "System.Drawing", "System.Globalization", "System.Management", "System.Net.Http", "System.Memory", "System.Runtime.CompilerServices.Unsafe",
		"System.Runtime", "System.Threading.Tasks.Extensions", "System.Xml.Linq", "System.Xml.Serialization", "System.Xml", "System", "Carbon", "Carbon.Common", "Carbon.SDK", "Carbon.Test",
		"Carbon.Startup", "Carbon.Profiler", "MySql.Data", "MySqlConnector", "protobuf-net.Core", "protobuf-net", "websocket-sharp", "Assembly-CSharp-firstpass", "Assembly-CSharp", "ZString",
		"Fleck", "Newtonsoft.Json", "Ionic.Zip.Reduced", "Cronos", "BoomlagoonJSON", "Facepunch.BurstCloth", "Facepunch.Console", "Facepunch.Network", "Facepunch.Rcon", "Facepunch.Raknet",
		"Facepunch.Sqlite", "Facepunch.System", "Facepunch.Steamworks.Win64", "Facepunch.SteamNetworking", "Facepunch.Unity", "Facepunch.UnityEngine", "Facepunch.Nexus", "Facepunch.Ping", "UniTask", "UniTask.Addressables",
		"UniTask.DOTween", "UniTask.Linq", "UniTask.TextMeshPro", "Rust.Data", "Rust.FileSystem", "Rust.Global", "Rust.Harmony", "Rust.Localization", "Rust.Platform.Common", "Rust.Platform",
		"Rust.UI", "Rust.Workshop", "Rust.World", "Rust.Clans", "Rust.Clans.Local", "Unity.Mathematics", "Unity.Timeline", "UnityEngine.AIModule", "UnityEngine.AssetBundleModule", "UnityEngine.AnimationModule",
		"UnityEngine.CoreModule", "UnityEngine.ImageConversionModule", "UnityEngine.ParticleSystemModule", "UnityEngine.PhysicsModule", "UnityEngine.SharedInternalsModule", "UnityEngine.TerrainModule", "UnityEngine.TerrainPhysicsModule", "UnityEngine.TextRenderingModule", "UnityEngine.UI", "UnityEngine.UIModule",
		"UnityEngine.UnityWebRequestAssetBundleModule", "UnityEngine.UnityWebRequestAudioModule", "UnityEngine.UnityWebRequestModule", "UnityEngine.UnityWebRequestTextureModule", "UnityEngine.UnityWebRequestWWWModule", "UnityEngine.VehiclesModule", "UnityEngine", "Facepunch.Steamworks.Win64"
	});

	private static readonly IReadOnlyList<string> _proxyLibs = new global::_003C_003Ez__ReadOnlySingleElementList<string>("Carbon.Proxy");

	public IReadOnlyList<string> RefBlacklist => _blacklistLibs;

	public IReadOnlyList<string> RefWhitelist => _whitelistLibs;

	public IReadOnlyList<string> RefProxy => _proxyLibs;

	public IAddonManager Components { get; private set; }

	public IExtensionManager Extensions { get; private set; }

	public IAddonManager Hooks { get; private set; }

	public IAddonManager Modules { get; private set; }

	private void Awake()
	{
		_library = Singleton<LibraryLoader>.GetInstance();
		((Component)this).gameObject.AddComponent<EventManager>();
		Components = ((Component)this).gameObject.AddComponent<ComponentManager>();
		Extensions = ((Component)this).gameObject.AddComponent<ExtensionManager>();
		Hooks = ((Component)this).gameObject.AddComponent<HookManager>();
		Modules = ((Component)this).gameObject.AddComponent<ModuleManager>();
		((Component)this).gameObject.AddComponent<CompatManager>();
	}

	public byte[] Read(string file, string[] directories = null)
	{
		byte[] array = null;
		Assembly assembly = _library.GetDomain().GetAssemblies().FirstOrDefault((Assembly asm) => asm.GetName().Name.Equals(file));
		if (assembly != null && !assembly.IsDynamic && assembly.Location != string.Empty)
		{
			array = File.ReadAllBytes(assembly.Location);
			if (array != null)
			{
				return array;
			}
		}
		foreach (KeyValuePair<string, byte[]> value in Extensions.Loaded.Values)
		{
			if (!(Path.GetFileName(value.Key) != file))
			{
				array = Extensions.Read(file);
				if (array != null)
				{
					return array;
				}
			}
		}
		foreach (string blacklistLib in _blacklistLibs)
		{
			if (Regex.IsMatch(file, blacklistLib))
			{
				break;
			}
			IAssemblyCache assemblyCache = _library.ResolveAssembly(file, $"{this}", directories);
			if (assemblyCache != null && assemblyCache.Raw != null)
			{
				return assemblyCache.Raw;
			}
		}
		return null;
	}

	public bool IsType<T>(Assembly assembly, out IEnumerable<Type> output)
	{
		try
		{
			output = from type in assembly.GetTypes()
				where typeof(T).IsAssignableFrom(type)
				select type;
			return output.Count() > 0;
		}
		catch
		{
			output = null;
			return false;
		}
	}

	private void CMDAssemblyInfo(Command.Args arg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		int num = 0;
		TextTable val = new TextTable();
		val.AddColumns(new string[5] { "#", "Assembly", "Version", "Dynamic", "Location" });
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			val.AddRow(new string[5]
			{
				$"{num++:n0}",
				assembly.GetName().Name,
				$"{assembly.GetName().Version}",
				$"{assembly.IsDynamic}",
				assembly.IsDynamic ? string.Empty : assembly.Location
			});
		}
		arg.ReplyWith(((object)val).ToString());
	}
}
