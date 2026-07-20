using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using API.Abstracts;
using API.Assembly;
using Utility;

namespace Loaders;

internal sealed class LibraryLoader : Singleton<LibraryLoader>, IDisposable
{
	private class Item : IAssemblyCache
	{
		public string Name { get; internal set; }

		public byte[] Raw { get; internal set; }

		public Assembly Assembly { get; internal set; }
	}

	private static readonly string[] _blacklist = new string[5] { "^.+\\.XmlSerializers$", "^Oxide\\..+$", "^System.Globalization$", "^System.Management$", "^System.Xml.Serialization$" };

	private AppDomain _domain;

	private readonly Dictionary<string, Item> _cache = new Dictionary<string, Item>();

	private readonly string[] _directoryList = new string[4]
	{
		Context.CarbonLib,
		Context.GameManaged,
		Context.CarbonModules,
		Context.CarbonExtensions
	};

	private bool _disposing;

	private LibraryLoader()
	{
		RegisterDomain(AppDomain.CurrentDomain);
	}

	internal AppDomain GetDomain()
	{
		return _domain;
	}

	internal void RegisterDomain(AppDomain domain)
	{
		_domain = domain;
		_domain.AssemblyResolve += ResolveAssembly;
		Logger.Log("Library resolver attached to '" + _domain.FriendlyName + "'");
	}

	internal void UnregisterDomain()
	{
		_domain = null;
		_domain.AssemblyResolve -= ResolveAssembly;
		Logger.Log("Library resolver detached from '" + _domain.FriendlyName + "'");
	}

	internal Assembly ResolveAssembly(object sender, ResolveEventArgs args)
	{
		AssemblyName assemblyName = new AssemblyName(args.Name);
		string requester = args.RequestingAssembly?.GetName().Name ?? "unknown";
		return ResolveAssembly(assemblyName.Name, requester)?.Assembly;
	}

	internal IAssemblyCache ResolveAssembly(string name, string requester, string[] customDirectories = null)
	{
		try
		{
			if (IsBlacklisted(name))
			{
				return null;
			}
			string text = null;
			string[] array = customDirectories ?? _directoryList;
			foreach (string path in array)
			{
				string text2 = Path.Combine(path, name.EndsWith(".dll") ? name : (name + ".dll"));
				if (File.Exists(text2))
				{
					text = text2;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			byte[] array2 = File.ReadAllBytes(text);
			string key = Util.sha1(array2);
			if (_cache.TryGetValue(key, out var value))
			{
				return value;
			}
			Assembly assembly = Assembly.Load(array2);
			value = new Item
			{
				Name = name,
				Raw = array2,
				Assembly = assembly
			};
			_cache.Add(key, value);
			return value;
		}
		catch (Exception ex)
		{
			Logger.Error("Unresolved library: '" + name + "'", ex);
			return null;
		}
	}

	internal IAssemblyCache ReadFromCache(string name)
	{
		Item item = _cache.Select((KeyValuePair<string, Item> x) => x.Value).Last((Item x) => x.Name == name);
		return item ?? null;
	}

	internal static bool IsBlacklisted(string Name)
	{
		if (Name.Contains(">"))
		{
			return true;
		}
		string[] blacklist = _blacklist;
		foreach (string pattern in blacklist)
		{
			if (Regex.IsMatch(Name, pattern))
			{
				return true;
			}
		}
		return false;
	}

	private void Dispose(bool disposing)
	{
		if (!_disposing)
		{
			if (disposing)
			{
				_cache.Clear();
			}
			_disposing = true;
			_domain.AssemblyResolve -= ResolveAssembly;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
