using System;
using System.Collections.Generic;
using System.Globalization;
using Carbon.Core;
using UnityEngine;

namespace Oxide.Core.Plugins;

public class PluginLoader
{
	public virtual Type[] CorePlugins { get; } = Array.Empty<Type>();

	public virtual IEnumerable<string> ScanDirectory(string directory)
	{
		foreach (CorePlugin.ProcessableFile processableFile in CorePlugin.ProcessableFiles)
		{
			if (StringEx.Contains(processableFile.Path, directory, CompareOptions.OrdinalIgnoreCase))
			{
				yield return processableFile.Path;
			}
		}
	}
}
