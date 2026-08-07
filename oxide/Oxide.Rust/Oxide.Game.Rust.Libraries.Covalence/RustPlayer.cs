using System;
using System.Globalization;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;

namespace Oxide.Game.Rust.Libraries.Covalence;

public class RustPlayer : IPlayer, IEquatable<IPlayer>
{
	private static Player libPlayer;

	private static Permission libPerms;

	private readonly BasePlayer player;

	private readonly ulong steamId;

	public object Object => player;

	public CommandType LastCommand { get; set; }

	public string Name { get; set; }

	public string Id { get; }

	public CultureInfo Language => ((Object)(object)player != (Object)null) ? libPlayer.Language(player) : CultureInfo.GetCultureInfo("en");

	public string Address => ((Object)(object)player != (Object)null) ? libPlayer.Address(player) : "0.0.0.0";

	public int Ping => ((Object)(object)player != (Object)null) ? libPlayer.Ping(player) : 0;

	public bool IsAdmin => libPlayer.IsAdmin(steamId);

	public bool IsBanned => libPlayer.IsBanned(steamId);

	public bool IsConnected => ((Object)(object)player != (Object)null) ? libPlayer.IsConnected(player) : ((Object)(object)BasePlayer.FindByID(steamId) != (Object)null);

	public bool IsSleeping => ((Object)(object)player != (Object)null) ? libPlayer.IsSleeping(player) : ((Object)(object)BasePlayer.FindSleeping(steamId) != (Object)null);

	public bool IsServer => false;

	public TimeSpan BanTimeRemaining => IsBanned ? TimeSpan.MaxValue : TimeSpan.Zero;

	public float Health
	{
		get
		{
			return player.health;
		}
		set
		{
			player.health = value;
		}
	}

	public float MaxHealth
	{
		get
		{
			return player.MaxHealth();
		}
		set
		{
			player._maxHealth = value;
		}
	}

	internal RustPlayer(ulong id, string name)
	{
		if (libPerms == null)
		{
			libPerms = Interface.Oxide.GetLibrary<Permission>();
		}
		if (libPlayer == null)
		{
			libPlayer = Interface.Oxide.GetLibrary<Player>();
		}
		steamId = id;
		Name = name.Sanitize();
		Id = id.ToString();
	}

	internal RustPlayer(BasePlayer player)
		: this(player.userID, player.displayName)
	{
		this.player = player;
	}

	public void Ban(string reason, TimeSpan duration = default(TimeSpan))
	{
		libPlayer.Ban(steamId, reason, -1L);
	}

	public void Heal(float amount)
	{
		libPlayer.Heal(player, amount);
	}

	public void Hurt(float amount)
	{
		libPlayer.Hurt(player, amount);
	}

	public void Kick(string reason)
	{
		libPlayer.Kick(player, reason);
	}

	public void Kill()
	{
		libPlayer.Kill(player);
	}

	public void Rename(string name)
	{
		libPlayer.Rename(player, name);
	}

	public void Teleport(float x, float y, float z)
	{
		libPlayer.Teleport(player, x, y, z);
	}

	public void Teleport(GenericPosition pos)
	{
		Teleport(pos.X, pos.Y, pos.Z);
	}

	public void Unban()
	{
		libPlayer.Unban(steamId);
	}

	public void Position(out float x, out float y, out float z)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = libPlayer.Position(player);
		x = val.x;
		y = val.y;
		z = val.z;
	}

	public GenericPosition Position()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = libPlayer.Position(player);
		return new GenericPosition(val.x, val.y, val.z);
	}

	public void Message(string message, string prefix, params object[] args)
	{
		libPlayer.Message(player, message, prefix, 0uL, args);
	}

	public void Message(string message)
	{
		Message(message, null);
	}

	public void Reply(string message, string prefix, params object[] args)
	{
		switch (LastCommand)
		{
		case CommandType.Chat:
			Message(message, prefix, args);
			break;
		case CommandType.Console:
			player.ConsoleMessage(string.Format(Formatter.ToPlaintext(message), args));
			break;
		}
	}

	public void Reply(string message)
	{
		Reply(message, null);
	}

	public void Command(string command, params object[] args)
	{
		player.SendConsoleCommand(command, args);
	}

	public bool HasPermission(string perm)
	{
		return libPerms.UserHasPermission(Id, perm);
	}

	public void GrantPermission(string perm)
	{
		libPerms.GrantUserPermission(Id, perm, null);
	}

	public void RevokePermission(string perm)
	{
		libPerms.RevokeUserPermission(Id, perm);
	}

	public bool BelongsToGroup(string group)
	{
		return libPerms.UserHasGroup(Id, group);
	}

	public void AddToGroup(string group)
	{
		libPerms.AddUserGroup(Id, group);
	}

	public void RemoveFromGroup(string group)
	{
		libPerms.RemoveUserGroup(Id, group);
	}

	public bool Equals(IPlayer other)
	{
		return Id == other?.Id;
	}

	public override bool Equals(object obj)
	{
		return obj is IPlayer && Id == ((IPlayer)obj).Id;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}

	public override string ToString()
	{
		return "Covalence.RustPlayer[" + Id + ", " + Name + "]";
	}
}
