using System;
using System.Collections.Generic;
using System.Linq;
using API.Abstracts;
using API.Commands;
using Carbon;
using Facepunch;
using UnityEngine;

namespace Components;

public sealed class CommandManager : CarbonBehaviour, ICommandManager
{
	public List<Command> Chat { get; set; } = new List<Command>();

	public List<Command> ClientConsole { get; set; } = new List<Command>();

	public List<Command> RCon { get; set; } = new List<Command>();

	public bool Contains(IList<Command> factory, string command, out Command outCommand)
	{
		List<Command> list = Pool.Get<List<Command>>();
		list.AddRange(factory);
		command = command?.Trim().ToLower();
		foreach (Command item in list)
		{
			if (item.Name == command)
			{
				Pool.FreeUnmanaged<Command>(ref list);
				outCommand = item;
				return true;
			}
		}
		Pool.FreeUnmanaged<Command>(ref list);
		outCommand = null;
		return false;
	}

	public List<T> GetFactory<T>() where T : Command
	{
		if (typeof(T) == typeof(Command.RCon))
		{
			return RCon as List<T>;
		}
		if (typeof(T) == typeof(Command.ClientConsole))
		{
			return ClientConsole as List<T>;
		}
		if (typeof(T) == typeof(Command.Chat))
		{
			return Chat as List<T>;
		}
		return null;
	}

	public List<Command> GetFactory(Command command)
	{
		if (!(command is Command.RCon))
		{
			if (!(command is Command.ClientConsole))
			{
				if (command is Command.Chat)
				{
					return Chat;
				}
				return null;
			}
			return ClientConsole;
		}
		return RCon;
	}

	public IEnumerable<T> GetCommands<T>() where T : Command
	{
		if (typeof(T) == typeof(Command.RCon))
		{
			return RCon.Cast<T>();
		}
		if (typeof(T) == typeof(Command.ClientConsole))
		{
			return ClientConsole.Cast<T>();
		}
		if (typeof(T) == typeof(Command.Chat))
		{
			return Chat.Cast<T>();
		}
		return null;
	}

	public Command Find(string command)
	{
		if (Contains(Chat, command, out var outCommand))
		{
			return outCommand;
		}
		if (Contains(ClientConsole, command, out outCommand))
		{
			return outCommand;
		}
		if (Contains(RCon, command, out outCommand))
		{
			return outCommand;
		}
		return null;
	}

	public void ClearCommands(Func<Command, bool> condition)
	{
		if (condition == null)
		{
			RCon.Clear();
			ClientConsole.Clear();
			Chat.Clear();
			return;
		}
		List<Command> list = Pool.Get<List<Command>>();
		list.AddRange(RCon);
		list.AddRange(ClientConsole);
		list.AddRange(Chat);
		foreach (Command item in list)
		{
			if (condition(item))
			{
				if (RCon.Contains(item))
				{
					RCon.Remove(item);
				}
				else if (ClientConsole.Contains(item))
				{
					ClientConsole.Remove(item);
				}
				else if (Chat.Contains(item))
				{
					Chat.Remove(item);
				}
			}
		}
		Pool.FreeUnmanaged<Command>(ref list);
	}

	public bool Execute(Command command, Command.Args args)
	{
		if (command == null)
		{
			return false;
		}
		try
		{
			if (command.CanExecute != null && !command.CanExecute(command, args))
			{
				return false;
			}
		}
		catch (Exception arg)
		{
			Logger.Error($"Failed command execution authentication for command '{command}': {arg}");
			return false;
		}
		try
		{
			command.Callback?.Invoke(args);
			if (!args.PrintOutput && args.IsRCon && !string.IsNullOrEmpty(args.Reply) && args.Tokenize<Arg>(out var value) && ((Option)(ref value.Option)).RconConnectionId != 0)
			{
				RCon.OnMessage(args.Reply, string.Empty, (LogType)3);
			}
			if (args.PrintOutput && !string.IsNullOrEmpty(args.Reply))
			{
				if (args is PlayerArgs playerArgs)
				{
					if (playerArgs.GetPlayer<BasePlayer>(out var value2))
					{
						value2.ConsoleMessage(args.Reply);
					}
					else
					{
						Logger.Log(args.Reply);
					}
				}
				else
				{
					Logger.Log(args.Reply);
				}
			}
			Arg arg2 = null;
			if (args.PrintOutput)
			{
				if (args.Tokenize<Arg>(out arg2))
				{
					Print(arg2.Reply, ArgEx.Player(arg2));
				}
				else
				{
					BasePlayer value3 = null;
					if (args is PlayerArgs playerArgs2)
					{
						playerArgs2.GetPlayer<BasePlayer>(out value3);
					}
					Print(args.Reply, value3);
				}
			}
			return true;
			void Print(string reply, BasePlayer player)
			{
				if (!string.IsNullOrEmpty(reply))
				{
					if ((Object)(object)player != (Object)null)
					{
						player.ConsoleMessage(reply);
					}
					else if (arg2 != null && arg2.IsRcon)
					{
						RCon.OnMessage(reply, string.Empty, (LogType)3);
					}
					else
					{
						Logger.Log(reply);
					}
				}
			}
		}
		catch (Exception arg3)
		{
			bool flag = false;
			if (!args.PrintOutput && args.IsRCon && args.Tokenize<Arg>(out var value4) && ((Option)(ref value4.Option)).RconConnectionId != 0)
			{
				RCon.OnMessage($"Failed executing command '{command}': {arg3}", string.Empty, (LogType)3);
				flag = true;
			}
			if (!flag)
			{
				Logger.Error($"Failed executing command '{command}': {arg3}");
			}
			return false;
		}
	}

	public bool RegisterCommand(Command command, out string reason)
	{
		if (command == null || string.IsNullOrEmpty(command.Name))
		{
			reason = "Command is null.";
			return false;
		}
		command.Fetch();
		List<Command> factory = GetFactory(command);
		if (Contains(factory, command.Name, out var _))
		{
			reason = "Command '" + command.Name + "' already exists.";
			return false;
		}
		factory.Add(command);
		reason = "Successfully added command.";
		return true;
	}

	public bool UnregisterCommand(Command command, out string reason)
	{
		List<Command> factory = GetFactory(command);
		if (factory == null)
		{
			reason = "Couldn't find factory.";
			return false;
		}
		if (!factory.Contains(command))
		{
			reason = "Couldn't find the command.";
			return false;
		}
		command.Dispose();
		factory.Remove(command);
		reason = "Successfully removed command.";
		return true;
	}
}
