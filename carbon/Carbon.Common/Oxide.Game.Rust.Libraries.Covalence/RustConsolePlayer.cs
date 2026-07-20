using System;
using System.Globalization;
using Carbon;
using Oxide.Core.Libraries.Covalence;

namespace Oxide.Game.Rust.Libraries.Covalence;

public struct RustConsolePlayer : IPlayer
{
	public object Object { get; set; }

	public CommandType LastCommand
	{
		get
		{
			return CommandType.Console;
		}
		set
		{
		}
	}

	public string Name
	{
		get
		{
			return "Server Console";
		}
		set
		{
		}
	}

	public string Id => "server_console";

	public CultureInfo Language => CultureInfo.InstalledUICulture;

	public string Address => "127.0.0.1";

	public int Ping => 0;

	public bool IsAdmin => true;

	public bool IsBanned => false;

	public bool IsConnected => true;

	public bool IsSleeping => false;

	public bool IsServer => true;

	public TimeSpan BanTimeRemaining => TimeSpan.Zero;

	public float Health { get; set; }

	public float MaxHealth { get; set; }

	public void Ban(string reason, TimeSpan duration)
	{
	}

	public void Heal(float amount)
	{
	}

	public void Hurt(float amount)
	{
	}

	public void Kick(string reason)
	{
	}

	public void Kill()
	{
	}

	public void Rename(string name)
	{
	}

	public void Teleport(float x, float y, float z)
	{
	}

	public void Teleport(GenericPosition pos)
	{
		Teleport(pos.X, pos.Y, pos.Z);
	}

	public void Unban()
	{
	}

	public void Position(out float x, out float y, out float z)
	{
		x = 0f;
		y = 0f;
		z = 0f;
	}

	public GenericPosition Position()
	{
		return new GenericPosition(0f, 0f, 0f);
	}

	public void Message(string message, string prefix, params object[] args)
	{
		message = ((args != null && args.Length != 0) ? string.Format(message, args) : message);
		string message2 = ((prefix != null) ? (prefix + " " + message) : message);
		Logger.Log(message2);
	}

	public void Message(string message)
	{
		Message(message, null, null);
	}

	public void Reply(string message, string prefix, params object[] args)
	{
		Message(message, prefix, args);
	}

	public void Reply(string message)
	{
		Message(message, null, null);
	}

	public void Command(string command, params object[] args)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		ConsoleSystem.Run(Option.Server, command, args);
	}

	public bool HasPermission(string perm)
	{
		return true;
	}

	public void GrantPermission(string perm)
	{
	}

	public void RevokePermission(string perm)
	{
	}

	public bool BelongsToGroup(string group)
	{
		return false;
	}

	public void AddToGroup(string group)
	{
	}

	public void RemoveFromGroup(string group)
	{
	}
}
