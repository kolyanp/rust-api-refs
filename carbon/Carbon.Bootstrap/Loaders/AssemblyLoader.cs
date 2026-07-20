using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using API.Assembly;
using Carbon;
using Carbon.Components;
using Carbon.Profiler;
using Utility;

namespace Loaders;

internal sealed class AssemblyLoader : IDisposable
{
	private class Item : IAssemblyCache
	{
		public string Name { get; internal set; }

		public byte[] Raw { get; internal set; }

		public Assembly Assembly { get; internal set; }
	}

	private readonly Dictionary<string, Item> _cache = new Dictionary<string, Item>();

	private readonly string[] _directoryList = new string[3]
	{
		Context.CarbonManaged,
		Context.CarbonHooks,
		Context.CarbonExtensions
	};

	internal byte[] _checksumBuffer = new byte[20];

	internal IReadOnlyList<byte> _needleBuffer = new global::_003C_003Ez__ReadOnlyArray<byte>(new byte[4] { 1, 220, 127, 1 });

	private bool _disposing;

	internal void ResetChechsum()
	{
		for (int i = 0; i < _checksumBuffer.Length; i++)
		{
			_checksumBuffer[i] = 0;
		}
	}

	internal IAssemblyCache Load(string file, string requester, string[] directories, IExtensionManager.ExtensionTypes extensionType = IExtensionManager.ExtensionTypes.Default)
	{
		file = Path.GetFileName(file);
		string text = null;
		string[] array = ((directories == null) ? _directoryList : directories);
		foreach (string path in array)
		{
			if (File.Exists(Path.Combine(path, file)))
			{
				text = Path.Combine(path, file);
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		byte[] asm = File.ReadAllBytes(text);
		switch (extensionType)
		{
		case IExtensionManager.ExtensionTypes.Extension:
		{
			ConversionResult conversionResult = Community.Runtime.Compat.AttemptOxideConvert(ref asm);
			if (conversionResult == ConversionResult.Fail)
			{
				Utility.Logger.Warn(" >> Failed Oxide extension conversion for '" + file + "'");
				return null;
			}
			break;
		}
		case IExtensionManager.ExtensionTypes.HarmonyMod:
			Community.Runtime.Compat.ConvertHarmonyMod(ref asm);
			if (asm == null)
			{
				Utility.Logger.Warn(" >> Failed HarmonyMod conversion for '" + file + "'");
				return null;
			}
			break;
		}
		string key = Util.sha1(asm);
		if (_cache.TryGetValue(key, out var value))
		{
			return value;
		}
		Assembly assembly;
		if (IndexOf(asm, _needleBuffer) == 0)
		{
			ResetChechsum();
			Buffer.BlockCopy(asm, 4, _checksumBuffer, 0, 20);
			assembly = Assembly.Load(Package(_checksumBuffer, asm, 24));
		}
		else
		{
			assembly = Assembly.Load(asm);
		}
		if (extensionType == IExtensionManager.ExtensionTypes.Extension)
		{
			MonoProfiler.TryStartProfileFor(MonoProfilerConfig.ProfileTypes.Extension, assembly, Path.GetFileNameWithoutExtension(file));
			Assemblies.Extensions.Update(Path.GetFileNameWithoutExtension(file), assembly, file);
		}
		value = new Item
		{
			Name = file,
			Raw = asm,
			Assembly = assembly
		};
		_cache.Add(key, value);
		return value;
	}

	internal IAssemblyCache ReadFromCache(string name)
	{
		return _cache.Select((KeyValuePair<string, Item> x) => x.Value).LastOrDefault((Item x) => x.Name == name);
	}

	internal static byte[] Package(IReadOnlyList<byte> a, IReadOnlyList<byte> b, int c = 0)
	{
		byte[] array = new byte[b.Count - c];
		for (int i = c; i < b.Count; i++)
		{
			array[i - c] = (byte)(b[i] ^ a[(i - c) % a.Count]);
		}
		return array;
	}

	internal static int IndexOf(IReadOnlyList<byte> haystack, IReadOnlyList<byte> needle)
	{
		int count = needle.Count;
		int num = haystack.Count - count;
		for (int i = 0; i <= num; i++)
		{
			int j;
			for (j = 0; j < count && needle[j] == haystack[i + j]; j++)
			{
			}
			if (j == count)
			{
				return i;
			}
		}
		return -1;
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
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
