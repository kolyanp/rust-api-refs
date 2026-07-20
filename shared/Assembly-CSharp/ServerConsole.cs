using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Network;
using UnityEngine;
using Windows;

public class ServerConsole : SingletonComponent<ServerConsole>
{
	private struct ConsoleMessage(string message, string stackTrace, LogType type)
	{
		public string Message = message;

		public string StackTrace = stackTrace;

		public LogType Type = type;

		public ConsoleColor? Color = null;

		public List<string> StatusUpdate = null;
	}

	private ConsoleWindow console;

	private ConsoleInput input;

	private CancellationTokenSource logThreadCancellation;

	private static bool ignoreLogs;

	private ConcurrentQueue<ConsoleMessage> queuedMessages = new ConcurrentQueue<ConsoleMessage>();

	private ConcurrentQueue<string> queuedCommands = new ConcurrentQueue<string>();

	private float nextUpdate;

	private static bool consoleEnabled => !CommandLine.HasSwitch("-noconsole");

	private DateTime currentGameTime
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)TOD_Sky.Instance))
			{
				return DateTime.Now;
			}
			return TOD_Sky.Instance.Cycle.DateTime;
		}
	}

	private int currentPlayerCount => BasePlayer.activePlayerList.Count;

	private int maxPlayerCount => ConVar.Server.maxplayers;

	private int currentEntityCount => BaseNetworkable.serverEntities.Count;

	private int currentSleeperCount => BasePlayer.sleepingPlayerList.Count;

	public void OnEnable()
	{
		if (!consoleEnabled)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		console = new ConsoleWindow();
		input = new ConsoleInput();
		console.Initialize();
		input.OnInputText += OnInputText;
		Output.OnMessage += HandleLog;
		input.ClearLine(System.Console.WindowHeight);
		for (int i = 0; i < System.Console.WindowHeight; i++)
		{
			System.Console.WriteLine("");
		}
		if (logThreadCancellation != null)
		{
			logThreadCancellation.Cancel();
			logThreadCancellation.Dispose();
		}
		logThreadCancellation = new CancellationTokenSource();
		Task.Run(async delegate
		{
			await LogThread(logThreadCancellation.Token);
		});
	}

	private void OnDisable()
	{
		if (logThreadCancellation != null)
		{
			logThreadCancellation.Cancel();
			logThreadCancellation.Dispose();
			logThreadCancellation = null;
		}
		Output.OnMessage -= HandleLog;
		if (input != null)
		{
			input.OnInputText -= OnInputText;
		}
		console?.Shutdown();
	}

	private void OnInputText(string obj)
	{
		queuedCommands.Enqueue(obj);
	}

	public static void PrintColoured(string text, ConsoleColor color)
	{
		ignoreLogs = true;
		DebugEx.Log(text, (StackTraceLogType)0);
		ignoreLogs = false;
		if (!((Object)(object)SingletonComponent<ServerConsole>.Instance == (Object)null) && SingletonComponent<ServerConsole>.Instance.input != null)
		{
			SingletonComponent<ServerConsole>.Instance.queuedMessages.Enqueue(new ConsoleMessage(text, null, (LogType)3)
			{
				Color = color
			});
		}
	}

	private void HandleLog(string message, string stackTrace, LogType type)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!ignoreLogs)
		{
			queuedMessages.Enqueue(new ConsoleMessage(message, stackTrace, type));
		}
	}

	private async Task LogThread(CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				if (queuedMessages.Count > 0)
				{
					bool flag = false;
					ConsoleMessage result;
					while (queuedMessages.TryDequeue(out result))
					{
						if (result.StatusUpdate != null)
						{
							for (int i = 0; i < result.StatusUpdate.Count; i++)
							{
								input.statusText[i] = result.StatusUpdate[i];
							}
							Pool.FreeUnmanaged<string>(ref result.StatusUpdate);
							continue;
						}
						if (!flag)
						{
							input.ClearLine(input.statusText.Length + 1);
							flag = true;
						}
						PrintMessage(result.Message, result.Type, result.Color);
					}
					if (System.Console.CursorTop == System.Console.BufferHeight - 1)
					{
						System.Console.WriteLine();
					}
					if (flag)
					{
						input.RedrawInputLine(clear: false);
					}
					System.Console.CursorVisible = false;
					input.RedrawStatusText();
					System.Console.CursorVisible = true;
					input.FixBottomOfBuffer();
				}
				input?.Update();
			}
			catch (Exception arg)
			{
				System.Console.WriteLine($"Console Thread Error: {arg}");
			}
			await Task.Delay(20);
		}
	}

	private void PrintMessage(string message, LogType type, ConsoleColor? colorOverride)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Invalid comparison between Unknown and I4
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Invalid comparison between Unknown and I4
		if (message == null || message.StartsWith("[CHAT]") || message.StartsWith("[TEAM CHAT]") || message.StartsWith("[CARDS CHAT]"))
		{
			return;
		}
		ConsoleColor foregroundColor = System.Console.ForegroundColor;
		if (colorOverride.HasValue)
		{
			try
			{
				System.Console.ForegroundColor = colorOverride.Value;
			}
			catch
			{
				System.Console.ForegroundColor = ConsoleColor.Gray;
			}
		}
		else if ((int)type == 2)
		{
			if (message.StartsWith("HDR RenderTexture format is not") || message.StartsWith("The image effect") || message.StartsWith("Image Effects are not supported on this platform") || message.StartsWith("[AmplifyColor]") || message.StartsWith("Skipping profile frame.") || message.StartsWith("Kinematic body only supports Speculative Continuous collision detection"))
			{
				return;
			}
			System.Console.ForegroundColor = ConsoleColor.Yellow;
		}
		else if ((int)type == 0)
		{
			System.Console.ForegroundColor = ConsoleColor.Red;
		}
		else if ((int)type == 4)
		{
			System.Console.ForegroundColor = ConsoleColor.Red;
		}
		else if ((int)type == 1)
		{
			System.Console.ForegroundColor = ConsoleColor.Red;
		}
		else
		{
			System.Console.ForegroundColor = ConsoleColor.Gray;
		}
		if (input != null)
		{
			System.Console.WriteLine(message);
		}
		System.Console.ForegroundColor = foregroundColor;
	}

	private void Update()
	{
		UpdateStatus();
		string result;
		while (queuedCommands.TryDequeue(out result))
		{
			ConsoleSystem.Run(ConsoleSystem.Option.Server.FromServerConsole(), result);
		}
	}

	private void UpdateStatus()
	{
		if (!(nextUpdate > Time.realtimeSinceStartup) && Net.sv != null && Net.sv.IsConnected())
		{
			nextUpdate = Time.realtimeSinceStartup + 0.33f;
			if (input != null && input.valid)
			{
				string text = NumberExtensions.FormatSeconds((long)Time.realtimeSinceStartup);
				string text2 = currentGameTime.ToString("[H:mm]");
				string text3 = " " + text2 + " [" + currentPlayerCount + "/" + maxPlayerCount + "] " + ConVar.Server.hostname + " [" + ConVar.Server.level + "]";
				string text4 = Performance.current.frameRate + "fps " + Performance.current.memoryCollections + "gc " + text;
				string text5 = NumberExtensions.FormatBytes<ulong>(Net.sv.GetStat(null, BaseNetwork.StatTypeLong.BytesReceived_LastSecond), true) + "/s in, " + NumberExtensions.FormatBytes<ulong>(Net.sv.GetStat(null, BaseNetwork.StatTypeLong.BytesSent_LastSecond), true) + "/s out";
				string text6 = text4.PadLeft(input.lineWidth - 1);
				text6 = text3 + ((text3.Length < text6.Length) ? text6.Substring(text3.Length) : "");
				string text7 = " " + currentEntityCount.ToString("n0") + " ents, " + currentSleeperCount.ToString("n0") + " slprs";
				string text8 = text5.PadLeft(input.lineWidth - 1);
				text8 = text7 + ((text7.Length < text8.Length) ? text8.Substring(text7.Length) : "");
				ConsoleMessage item = new ConsoleMessage
				{
					StatusUpdate = Pool.Get<List<string>>()
				};
				item.StatusUpdate.Add("");
				item.StatusUpdate.Add(text6);
				item.StatusUpdate.Add(text8);
				queuedMessages.Enqueue(item);
			}
		}
	}
}
