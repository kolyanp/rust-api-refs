using System.Threading;
using Oxide.Core.Logging;
using UnityEngine;

namespace Oxide.Core.Unity.Logging;

public sealed class UnityLogger : Logger
{
	private readonly Thread mainThread = Thread.CurrentThread;

	public UnityLogger()
		: base(processImmediately: true)
	{
	}

	protected override void ProcessMessage(LogMessage message)
	{
		if (Thread.CurrentThread != mainThread)
		{
			Interface.Oxide.NextTick(delegate
			{
				ProcessMessage(message);
			});
			return;
		}
		switch (message.Type)
		{
		case LogType.Error:
			Debug.LogError((object)message.ConsoleMessage);
			break;
		case LogType.Warning:
			Debug.LogWarning((object)message.ConsoleMessage);
			break;
		default:
			Debug.Log((object)message.ConsoleMessage);
			break;
		}
	}
}
