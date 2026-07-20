using System;
using System.Reflection;
using UnityEngine;

namespace Oxide.Core.Unity;

public class UnityScript : MonoBehaviour
{
	private OxideMod oxideMod;

	public static GameObject Instance { get; private set; }

	public static float RealtimeSinceStartup { get; private set; }

	public static void Create()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		Instance = new GameObject("Oxide.Core.Unity");
		Object.DontDestroyOnLoad((Object)(object)Instance);
		Instance.AddComponent<UnityScript>();
	}

	private void Awake()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		RealtimeSinceStartup = Time.realtimeSinceStartup;
		oxideMod = Interface.Oxide;
		EventInfo eventInfo = typeof(Application).GetEvent("logMessageReceived");
		if ((object)eventInfo == null)
		{
			object? obj = typeof(Application).GetField("s_LogCallback", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);
			LogCallback logCallback = (LogCallback)((obj is LogCallback) ? obj : null);
			if (logCallback == null)
			{
				Interface.Oxide.LogWarning("No Unity application log callback is registered");
			}
			Application.RegisterLogCallback((LogCallback)delegate(string message, string stack_trace, LogType type)
			{
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				LogCallback obj3 = logCallback;
				if (obj3 != null)
				{
					obj3.Invoke(message, stack_trace, type);
				}
				LogMessageReceived(message, stack_trace, type);
			});
		}
		else
		{
			Delegate obj2 = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, "LogMessageReceived");
			eventInfo.GetAddMethod().Invoke(null, new object[1] { obj2 });
		}
	}

	private void Update()
	{
		RealtimeSinceStartup = Time.realtimeSinceStartup;
		oxideMod.OnFrame(Time.deltaTime);
	}

	private void OnDestroy()
	{
		if (!oxideMod.IsShuttingDown)
		{
			oxideMod.LogWarning("The Oxide Unity Script was destroyed (creating a new instance)");
			oxideMod.NextTick(Create);
		}
	}

	private void OnApplicationQuit()
	{
		if (!oxideMod.IsShuttingDown)
		{
			Interface.Call("OnServerShutdown");
			Interface.Oxide.OnShutdown();
		}
	}

	private void LogMessageReceived(string message, string stackTrace, LogType type)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)type == 4)
		{
			RemoteLogger.Exception(message, stackTrace);
		}
	}
}
