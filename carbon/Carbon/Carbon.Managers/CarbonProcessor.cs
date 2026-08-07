using System;
using System.Collections.Generic;
using API.Commands;
using Carbon.Base;
using Carbon.Contracts;
using Oxide.Core.Libraries;

namespace Carbon.Managers;

public class CarbonProcessor : BaseProcessor, ICarbonProcessor, IDisposable
{
	public override string Name => "Carbon Processor";

	public List<Action> CurrentFrameQueue { get; set; } = new List<Action>();

	public List<Action> PreviousFrameQueue { get; set; } = new List<Action>();

	public object CurrentFrameLock { get; set; } = new object();

	public override void OnDestroy()
	{
	}

	public override void Dispose()
	{
	}

	public override void Start()
	{
		Community.Runtime.CommandManager.RegisterCommand(new Command.RCon
		{
			Name = "avgfps",
			Help = "Displays the server's average FPS.",
			Callback = delegate(Command.Args arg)
			{
				arg.ReplyWith($"{Performance.report.frameRateAverage:0}");
			}
		}, out var reason);
		Community.Runtime.CommandManager.RegisterCommand(new Command.ClientConsole
		{
			Name = "avgfps",
			Help = "Displays the server's average FPS.",
			Callback = delegate(Command.Args arg)
			{
				arg.ReplyWith($"{Performance.report.frameRateAverage:0}");
			},
			Auth = new Command.Authentication
			{
				AuthLevel = 2
			}
		}, out reason);
	}

	public void Update()
	{
		Timer.UpdateStartupTimers();
		if (CurrentFrameQueue.Count <= 0)
		{
			return;
		}
		object currentFrameLock = CurrentFrameLock;
		List<Action> list = null;
		lock (currentFrameLock)
		{
			list = CurrentFrameQueue;
			CurrentFrameQueue = PreviousFrameQueue;
			PreviousFrameQueue = list;
		}
		for (int i = 0; i < list.Count; i++)
		{
			try
			{
				list[i]();
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to execute OnFrame callback", ex.InnerException ?? ex);
			}
		}
		list.Clear();
	}
}
