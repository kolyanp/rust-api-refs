using System;
using System.Collections.Generic;
using Facepunch;
using Newtonsoft.Json;

namespace API.Commands;

public class Command : IDisposable
{
	public enum Types
	{
		Generic,
		Chat,
		Console,
		Rcon
	}

	public class Prefix
	{
		public string Value;

		public bool PrintToChat;

		public bool PrintToConsole;

		public int SuggestionAuthLevel = 2;
	}

	public class Args : IPooled
	{
		public Types Type { get; set; }

		public object[] Arguments { get; set; }

		public string Reply { get; set; }

		public object Token { get; set; }

		public bool IsRCon { get; set; }

		public bool IsServer { get; set; }

		public bool PrintOutput { get; set; }

		public bool Tokenize<T>(out T value)
		{
			return (value = (T)Token) != null;
		}

		public void ReplyWith(string message)
		{
			Reply = message;
		}

		public void ReplyWith<T>(T message)
		{
			Reply = JsonConvert.SerializeObject((object)message, (Formatting)1);
		}

		public virtual void EnterPool()
		{
			Type = Types.Generic;
			Reply = null;
			Token = null;
			Arguments = null;
		}

		public virtual void LeavePool()
		{
		}
	}

	public class RCon : Command
	{
	}

	public class Chat : AuthenticatedCommand
	{
	}

	public class ClientConsole : AuthenticatedCommand
	{
	}

	public class Authentication
	{
		public int AuthLevel { get; set; }

		public string[] Permissions { get; set; }

		public string[] Groups { get; set; }

		public int Cooldown { get; set; }

		public bool DoCooldownPenalty { get; set; }
	}

	public static List<Prefix> Prefixes;

	internal const char _splitDelimiter = '.';

	public static bool FromRcon { get; set; }

	public string Name { get; set; }

	public string Help { get; set; }

	public object Token { get; set; }

	public object Reference { get; set; }

	public Types Type { get; set; }

	public CommandFlags Flags { get; set; }

	public Action<Args> Callback { get; set; }

	public Func<Command, Args, bool> CanExecute { get; set; }

	public Command RustCommand { get; set; }

	public static Prefix FindPrefix(string command)
	{
		foreach (Prefix prefix in Prefixes)
		{
			if (command.StartsWith(prefix.Value, StringComparison.OrdinalIgnoreCase))
			{
				return prefix;
			}
		}
		return null;
	}

	public static bool HasPrefix(string command, out Prefix prefix)
	{
		prefix = FindPrefix(command);
		return prefix != null;
	}

	public void Fetch()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		Name = Name?.ToLower().Trim();
		Help = Help?.Trim();
		if (!(this is RCon))
		{
			if (!(this is ClientConsole))
			{
				if (this is Chat)
				{
					Type = Types.Chat;
				}
			}
			else
			{
				Type = Types.Console;
			}
		}
		else
		{
			Type = Types.Rcon;
		}
		if (RustCommand == null)
		{
			RustCommand = new Command();
		}
		string[] array = Name.Split('.');
		string parent = ((array.Length > 1) ? array[0] : "global");
		string name = ((array.Length > 1) ? array[1] : Name);
		Array.Clear(array, 0, array.Length);
		array = null;
		RustCommand.Name = name;
		RustCommand.Parent = parent;
		RustCommand.FullName = Name;
		RustCommand.ServerUser = true;
		RustCommand.ServerAdmin = true;
		RustCommand.Client = true;
		RustCommand.ClientInfo = true;
		RustCommand.Variable = false;
	}

	public void Dispose()
	{
		Callback = null;
		CanExecute = null;
		RustCommand = null;
	}

	public bool HasFlag(CommandFlags flag)
	{
		return (Flags & flag) != 0;
	}

	public void SetFlag(CommandFlags flag, bool wants)
	{
		if (wants)
		{
			Flags |= flag;
		}
		else
		{
			Flags &= ~flag;
		}
	}

	public void ClearFlags()
	{
		Flags = CommandFlags.None;
	}
}
