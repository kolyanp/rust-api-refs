using System;
using System.Collections.Generic;
using System.Reflection;

namespace API.Assembly;

public interface IAddonManager
{
	WatchFolder Watcher { get; }

	IReadOnlyDictionary<Type, KeyValuePair<string, byte[]>> Loaded { get; }

	IReadOnlyDictionary<Type, string> Shared { get; }

	byte[] Read(string file);

	System.Reflection.Assembly Load(string file, string requester);

	void Unload(string file, string requester);
}
