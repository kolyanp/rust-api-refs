using System.Collections.Generic;
using System.Diagnostics;
using ConVar;
using Development.Attributes;
using Facepunch;
using Facepunch.Rust;
using Facepunch.Rust.Profiling;
using Network;
using Oxide.Core;
using UnityEngine;

public static class ConsoleNetwork
{
	private static Stopwatch timer = new Stopwatch();

	internal static void Init()
	{
	}

	internal static void OnClientCommand(Message packet)
	{
		if (packet.read.Unread > ConVar.Server.maxpacketsize_command)
		{
			Debug.LogWarning((object)"Dropping client command due to size");
			return;
		}
		timer.Restart();
		string text = packet.read.StringRaw(ConVar.Server.maxpacketsize_command);
		if (packet.connection == null || !packet.connection.connected)
		{
			Debug.LogWarning((object)("Client without connection tried to run command: " + text));
		}
		else if (Interface.CallHook("OnClientCommand", packet.connection, text) == null)
		{
			ConsoleSystem.CommandResult commandResult = ConsoleSystem.RunWithResult(ConsoleSystem.Option.Server.FromConnection(packet.connection).Quiet(), text);
			if (commandResult.Result == ConsoleSystem.CommandResultType.Success && (packet.connection.authLevel != 0 || (commandResult.Command.ServerAdmin && !commandResult.Command.ServerUser)))
			{
				Facepunch.Rust.Analytics.Azure.OnClientRanCommand(packet.connection, text);
			}
			if (!string.IsNullOrEmpty(commandResult.Output))
			{
				SendClientReply(packet.connection, commandResult.Output);
			}
			if (timer.Elapsed > RuntimeProfiler.ConsoleCommandWarningThreshold)
			{
				LagSpikeProfiler.ConsoleCommand(timer.Elapsed, packet, text);
			}
		}
	}

	internal static void SendClientReply(Connection cn, string strCommand)
	{
		if (Net.sv.IsConnected())
		{
			NetWrite netWrite = Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.ConsoleMessage);
			netWrite.String(strCommand);
			netWrite.Send(new SendInfo(cn));
		}
	}

	public static void SendClientCommand(Connection cn, string strCommand, params object[] args)
	{
		if (Net.sv.IsConnected() && Interface.CallHook("OnSendCommand", cn, strCommand, args) == null)
		{
			NetWrite netWrite = Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.ConsoleCommand);
			string val = ConsoleSystem.BuildCommand(strCommand, args);
			netWrite.String(val);
			NetProfileCapture.Annotate(netWrite, strCommand);
			netWrite.Send(new SendInfo(cn));
		}
	}

	public static void SendClientCommandImmediate(Connection cn, string strCommand, params object[] args)
	{
		if (Net.sv.IsConnected())
		{
			NetWrite netWrite = Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.ConsoleCommand);
			string val = ConsoleSystem.BuildCommand(strCommand, args);
			netWrite.String(val);
			NetProfileCapture.Annotate(netWrite, strCommand);
			netWrite.SendImmediate(new SendInfo(cn)
			{
				priority = Priority.Immediate
			});
		}
	}

	[PoolAnalyzerNonCaching]
	public static void SendClientCommand(List<Connection> cn, string strCommand, params object[] args)
	{
		if (Net.sv.IsConnected() && Interface.CallHook("OnSendCommand", cn, strCommand, args) == null)
		{
			NetWrite netWrite = Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.ConsoleCommand);
			netWrite.String(ConsoleSystem.BuildCommand(strCommand, args));
			NetProfileCapture.Annotate(netWrite, strCommand);
			netWrite.Send(new SendInfo(cn));
		}
	}

	public static void BroadcastToAllClients(string strCommand, params object[] args)
	{
		if (Net.sv.IsConnected() && Interface.CallHook("OnBroadcastCommand", strCommand, args) == null)
		{
			NetWrite netWrite = Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.ConsoleCommand);
			netWrite.String(ConsoleSystem.BuildCommand(strCommand, args));
			NetProfileCapture.Annotate(netWrite, strCommand);
			netWrite.Send(new SendInfo(Net.sv.connections));
		}
	}

	public static void BroadcastToAdmins(string strCommand, params object[] args)
	{
		if (!Net.sv.IsConnected())
		{
			return;
		}
		List<Connection> list = Pool.Get<List<Connection>>();
		foreach (Connection connection in Net.sv.connections)
		{
			if (connection.authLevel != 0)
			{
				list.Add(connection);
			}
		}
		NetWrite netWrite = Net.sv.StartWrite();
		netWrite.PacketID(Message.Type.ConsoleCommand);
		netWrite.String(ConsoleSystem.BuildCommand(strCommand, args));
		NetProfileCapture.Annotate(netWrite, strCommand);
		netWrite.Send(new SendInfo(list));
		Pool.FreeUnmanaged<Connection>(ref list);
	}
}
