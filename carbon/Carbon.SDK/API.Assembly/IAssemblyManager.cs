using System;
using System.Collections.Generic;
using System.Reflection;

namespace API.Assembly;

public interface IAssemblyManager
{
	IAddonManager Components { get; }

	IExtensionManager Extensions { get; }

	IAddonManager Hooks { get; }

	IAddonManager Modules { get; }

	IReadOnlyList<string> RefBlacklist { get; }

	IReadOnlyList<string> RefWhitelist { get; }

	byte[] Read(string file, string[] directories = null);

	bool IsType<T>(System.Reflection.Assembly assembly, out IEnumerable<Type> output);
}
