using Oxide.Core.Plugins;
using Oxide.Core.Unity.Logging;
using UnityEngine;

namespace Oxide.Core.Unity.Plugins;

public class UnityCore : CSPlugin
{
	private UnityLogger logger;

	public UnityCore()
	{
		base.Title = "Unity";
		base.Author = UnityExtension.AssemblyAuthors;
		base.Version = UnityExtension.AssemblyVersion;
	}

	[HookMethod("InitLogging")]
	private void InitLogging()
	{
		Interface.Oxide.NextTick(delegate
		{
			logger = new UnityLogger();
			Interface.Oxide.RootLogger.AddLogger(logger);
			Interface.Oxide.RootLogger.DisableCache();
		});
	}

	public void Print(string message)
	{
		Debug.Log((object)message);
	}

	public void PrintWarning(string message)
	{
		Debug.LogWarning((object)message);
	}

	public void PrintError(string message)
	{
		Debug.LogError((object)message);
	}
}
