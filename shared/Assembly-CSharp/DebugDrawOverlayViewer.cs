using System;
using System.Collections.Generic;
using ConVar;
using UnityEngine;

public class DebugDrawOverlayViewer : FacepunchBehaviour
{
	private const float RefreshSeconds = 0.5f;

	private readonly List<DebugDrawOverlayFrame> frames = new List<DebugDrawOverlayFrame>();

	private BasePlayer player;

	private Action refreshAction;

	private int firstFrame;

	private static int AllowancePerRefresh => Mathf.Max(1, (int)(Debugging.overlayKBps * 1024f * 0.5f));

	public static bool IsShowing(BasePlayer player, DebugDrawOverlay overlay)
	{
		DebugDrawOverlayViewer component = ((Component)player).GetComponent<DebugDrawOverlayViewer>();
		if ((Object)(object)component != (Object)null)
		{
			return component.IndexOf(overlay) >= 0;
		}
		return false;
	}

	public static void Show(BasePlayer player, DebugDrawOverlay overlay, bool show)
	{
		DebugDrawOverlayViewer debugDrawOverlayViewer = ((Component)player).GetComponent<DebugDrawOverlayViewer>();
		if (show)
		{
			if ((Object)(object)debugDrawOverlayViewer == (Object)null)
			{
				debugDrawOverlayViewer = ((Component)player).gameObject.AddComponent<DebugDrawOverlayViewer>();
				debugDrawOverlayViewer.player = player;
			}
			debugDrawOverlayViewer.Add(overlay);
		}
		else if ((Object)(object)debugDrawOverlayViewer != (Object)null)
		{
			debugDrawOverlayViewer.Remove(overlay);
		}
	}

	public static void Toggle(ConsoleSystem.Arg args, DebugDrawOverlay overlay, string what)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		bool flag = args.GetBool(0, !IsShowing(basePlayer, overlay));
		Show(basePlayer, overlay, flag);
		args.ReplyWith(flag ? ("Drawing " + what + ".") : ("Stopped drawing " + what + "."));
	}

	private void Add(DebugDrawOverlay overlay)
	{
		if (IndexOf(overlay) < 0)
		{
			DebugDrawOverlayFrame debugDrawOverlayFrame = new DebugDrawOverlayFrame(overlay);
			frames.Add(debugDrawOverlayFrame);
			int allowance = AllowancePerRefresh;
			Refresh(debugDrawOverlayFrame, ref allowance);
			if (refreshAction == null)
			{
				refreshAction = RefreshAll;
			}
			if (!IsInvoking(refreshAction))
			{
				InvokeRepeating(refreshAction, 0.5f, 0.5f);
			}
		}
	}

	private void Remove(DebugDrawOverlay overlay)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		int num = IndexOf(overlay);
		if (num >= 0)
		{
			DebugDrawOverlayFrame debugDrawOverlayFrame = frames[num];
			frames.RemoveAt(num);
			debugDrawOverlayFrame.Begin();
			int allowance = 0;
			if (debugDrawOverlayFrame.End(((Component)player).transform.position, ref allowance))
			{
				player.SendDebugDrawOverlay(debugDrawOverlayFrame);
			}
			if (frames.Count == 0)
			{
				Stop();
			}
		}
	}

	private int IndexOf(DebugDrawOverlay overlay)
	{
		for (int i = 0; i < frames.Count; i++)
		{
			if (frames[i].Overlay == overlay)
			{
				return i;
			}
		}
		return -1;
	}

	private void RefreshAll()
	{
		if ((Object)(object)player == (Object)null || player.IsDestroyed || !player.IsConnected || frames.Count == 0)
		{
			Stop();
			return;
		}
		int allowance = AllowancePerRefresh;
		firstFrame = (firstFrame + 1) % frames.Count;
		for (int i = 0; i < frames.Count; i++)
		{
			Refresh(frames[(firstFrame + i) % frames.Count], ref allowance);
		}
	}

	private void Refresh(DebugDrawOverlayFrame frame, ref int allowance)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("DebugDrawOverlayViewer.Refresh"))
		{
			frame.Begin();
			frame.Overlay.Build(player, frame);
			if (frame.End(((Component)player).transform.position, ref allowance))
			{
				player.SendDebugDrawOverlay(frame);
			}
		}
	}

	private void Stop()
	{
		if (refreshAction != null)
		{
			CancelInvoke(refreshAction);
		}
		frames.Clear();
		Object.Destroy((Object)(object)this);
	}
}
