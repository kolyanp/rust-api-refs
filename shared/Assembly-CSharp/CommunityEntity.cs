using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class CommunityEntity : PointEntity
{
	private class Countdown : MonoBehaviour
	{
		public enum TimerFormat
		{
			None,
			SecondsHundreth,
			MinutesSeconds,
			MinutesSecondsHundreth,
			HoursMinutes,
			HoursMinutesSeconds,
			HoursMinutesSecondsMilliseconds,
			HoursMinutesSecondsTenths,
			DaysHoursMinutes,
			DaysHoursMinutesSeconds,
			Custom
		}

		public string command = "";

		public float endTime;

		public float startTime;

		public float step = 1f;

		public float interval = 1f;

		public TimerFormat timerFormat;

		public string numberFormat = "0.####";

		public bool destroyIfDone = true;
	}

	public enum DraggablePositionSendType
	{
		NormalizedScreen,
		NormalizedParent,
		Relative,
		RelativeAnchor
	}

	private class FadeOut : MonoBehaviour
	{
		public float duration;
	}

	public enum TooltipType
	{
		Default,
		AlwaysOnTop,
		AlwaysOnTopEmoji
	}

	public GameObject TooltipRef;

	public GameObject TooltipAlwaysOnTopRef;

	public GameObject TooltipAlwaysOnTopEmojiRef;

	public static CommunityEntity ServerInstance;

	public static CommunityEntity ClientInstance;

	public GameObject[] OverallPanels;

	public Canvas[] AllCanvases;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CommunityEntity.OnRpcMessage"))
		{
			if (rpc == 2271099967u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DragRPC"));
				}
				using (TimeWarning.New("DragRPC"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							DragRPC(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in DragRPC");
					}
				}
				return true;
			}
			if (rpc == 3687934507u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DropRPC"));
				}
				using (TimeWarning.New("DropRPC"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							DropRPC(rpc3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in DropRPC");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[ServerVar]
	public static void pietest(ConsoleSystem.Arg arg)
	{
		CustomPie val = Pool.Get<CustomPie>();
		try
		{
			val.menus = Pool.Get<List<CustomPieMenu>>();
			AddPieMenu(val, "Switch Night", "Switch to night mode", "env.time 0", "assets/icons/device_add.png", disabled: false, selected: false, "", "");
			AddPieMenu(val, "Switch Day", "Switch to day mode", "env.time 12", "assets/icons/device_add.png", disabled: false, selected: false, "", "");
			AddPieMenu(val, "Rain", "Make it rain on the server", "sv weather.load storm", "assets/icons/embrella.png", disabled: false, selected: false, "", "");
			AddPieMenu(val, "Be Malicious", "This attempts to open your player inventory.", "inventory.toggle", "assets/icons/explosion_sprite.png", disabled: false, selected: false, "", "");
			AddPieMenu(val, "Left/Right Test", "Pick one or the other", "pietest_prev", "assets/icons/facepunch.png", disabled: false, selected: false, "pietest_prev", "pietest_next");
			AddPieMenu(val, "Exit", "Close this context menu", "", "assets/icons/close.png", disabled: false, selected: false, "", "");
			ServerInstance.SendPie(ArgEx.Player(arg), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar]
	public static void pietest_prev(ConsoleSystem.Arg arg)
	{
		CustomPie val = Pool.Get<CustomPie>();
		try
		{
			val.menus = Pool.Get<List<CustomPieMenu>>();
			AddPieMenu(val, "Go Back", null, "pietest", "assets/icons/fun.png", disabled: false, selected: false, "", "");
			AddPieMenu(val, "Or Don't", "You went previous", "pietest", "assets/icons/fun.png", disabled: true, selected: false, "", "");
			ServerInstance.SendPie(ArgEx.Player(arg), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar]
	public static void pietest_next(ConsoleSystem.Arg arg)
	{
		CustomPie val = Pool.Get<CustomPie>();
		try
		{
			val.menus = Pool.Get<List<CustomPieMenu>>();
			AddPieMenu(val, "Go Back", null, "pietest", "assets/icons/fun.png", disabled: false, selected: false, "", "");
			AddPieMenu(val, "Or Don't", "You went next", "pietest", "assets/icons/fun.png", disabled: true, selected: false, "", "");
			ServerInstance.SendPie(ArgEx.Player(arg), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void AddPieMenu(CustomPie pie, string name, string description, string command, string sprite, bool disabled, bool selected, string next, string prev)
	{
		CustomPieMenu val = Pool.Get<CustomPieMenu>();
		val.name = name;
		val.description = description;
		val.command = command;
		val.sprite = sprite;
		val.disabled = disabled;
		val.selected = selected;
		val.nextCommand = next;
		val.prevCommand = prev;
		pie.menus.Add(val);
	}

	public void SendPie(BasePlayer player, CustomPie pie)
	{
		ClientRPC(RpcTarget.Player("OpenPie", player), pie);
	}

	[RPC_Server]
	public void DragRPC(RPCMessage rpc)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		string name = rpc.read.String();
		Vector3 position = rpc.read.Vector3();
		DraggablePositionSendType type = (DraggablePositionSendType)rpc.read.Int32();
		Hook_DragRPC(rpc.player, name, position, type);
	}

	private void Hook_DragRPC(BasePlayer player, string name, Vector3 position, DraggablePositionSendType type)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Interface.CallHook("OnCuiDraggableDrag", player, name, position, type);
	}

	[RPC_Server]
	public void DropRPC(RPCMessage rpc)
	{
		string draggedName = rpc.read.String();
		string draggedSlot = rpc.read.String();
		string swappedName = rpc.read.String();
		string swappedSlot = rpc.read.String();
		Hook_DropRPC(rpc.player, draggedName, draggedSlot, swappedName, swappedSlot);
	}

	private void Hook_DropRPC(BasePlayer player, string draggedName, string draggedSlot, string swappedName, string swappedSlot)
	{
		Interface.CallHook("OnCuiDraggableDrop", player, draggedName, draggedSlot, swappedName, swappedSlot);
	}

	public void SendCustomVitals(BasePlayer player, CustomVitals vitals)
	{
		ClientRPC(RpcTarget.Player("RPC_UpdateVitals", player), vitals);
	}

	public override void InitShared()
	{
		if (base.isServer)
		{
			ServerInstance = this;
		}
		else
		{
			ClientInstance = this;
		}
		base.InitShared();
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (base.isServer)
		{
			ServerInstance = null;
		}
		else
		{
			ClientInstance = null;
		}
	}

	public void SendDestroyUIs(BasePlayer player, List<string> uiPanels)
	{
		CommunityEntity_DestroyUIs val = Pool.Get<CommunityEntity_DestroyUIs>();
		try
		{
			val.list = Pool.Get<List<string>>();
			for (int i = 0; i < uiPanels.Count; i++)
			{
				val.list.Add(uiPanels[i]);
			}
			ClientRPC(RpcTarget.Player("DestroyUIs", player), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void SendDestroyUIs(BasePlayer player, string[] uiPanels)
	{
		CommunityEntity_DestroyUIs val = Pool.Get<CommunityEntity_DestroyUIs>();
		try
		{
			val.list = Pool.Get<List<string>>();
			for (int i = 0; i < uiPanels.Length; i++)
			{
				val.list.Add(uiPanels[i]);
			}
			ClientRPC(RpcTarget.Player("DestroyUIs", player), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
