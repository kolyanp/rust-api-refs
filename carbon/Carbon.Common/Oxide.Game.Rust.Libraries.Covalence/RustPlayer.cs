using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Carbon;
using Network;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Plugins;
using UnityEngine;

namespace Oxide.Game.Rust.Libraries.Covalence;

public class RustPlayer : IPlayer
{
	internal const string _ipPattern = ":{1}[0-9]{1}\\d*";

	private static Permission perms;

	public object Object { get; set; }

	public BasePlayer BasePlayer
	{
		get
		{
			object obj = Object;
			return (BasePlayer)((obj is BasePlayer) ? obj : null);
		}
	}

	public CommandType LastCommand { get; set; }

	public string Name { get; set; } = string.Empty;

	public string Id { get; set; } = "server_console";

	public string Address
	{
		get
		{
			BasePlayer basePlayer = BasePlayer;
			if (((basePlayer != null) ? basePlayer.Connection : null) != null)
			{
				return Regex.Replace(BasePlayer.Connection.ipaddress, ":{1}[0-9]{1}\\d*", string.Empty);
			}
			return null;
		}
	}

	public int Ping
	{
		get
		{
			if (!((Object)(object)BasePlayer == (Object)null))
			{
				return Net.sv.GetAveragePing(BasePlayer.Connection);
			}
			return 0;
		}
	}

	public CultureInfo Language => CultureInfo.GetCultureInfo(IsConnected ? ((BaseNetworkable)BasePlayer).net.connection.info.GetString("global.language", "en") : perms.GetUserData(Id).Language);

	public bool IsConnected
	{
		get
		{
			if (!IsServer)
			{
				if ((Object)(object)BasePlayer != (Object)null)
				{
					return BasePlayer.IsConnected;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsSleeping
	{
		get
		{
			if ((Object)(object)BasePlayer != (Object)null)
			{
				return BasePlayer.IsSleeping();
			}
			return false;
		}
	}

	public bool IsServer { get; set; } = true;

	public bool IsAdmin
	{
		get
		{
			if (!IsServer)
			{
				if (ulong.TryParse(Id, out var result))
				{
					return ServerUsers.Is(result, (UserGroup)1);
				}
				return false;
			}
			return true;
		}
	}

	public bool IsBanned
	{
		get
		{
			if (ulong.TryParse(Id, out var result))
			{
				return ServerUsers.Is(result, (UserGroup)3);
			}
			return false;
		}
	}

	public TimeSpan BanTimeRemaining
	{
		get
		{
			if (!IsBanned)
			{
				return TimeSpan.Zero;
			}
			return TimeSpan.MaxValue;
		}
	}

	public float Health
	{
		get
		{
			return ((BaseCombatEntity)BasePlayer).health;
		}
		set
		{
			((BaseCombatEntity)BasePlayer).health = value;
		}
	}

	public float MaxHealth
	{
		get
		{
			return ((BaseEntity)BasePlayer).MaxHealth();
		}
		set
		{
			((BaseCombatEntity)BasePlayer).SetMaxHealth(value);
		}
	}

	public RustPlayer()
	{
	}

	public RustPlayer(BasePlayer player)
	{
		Object = player;
		Id = player.UserIDString;
		Name = player.displayName.Sanitize();
		LastCommand = CommandType.Chat;
		IsServer = false;
		perms = Community.Runtime.Core.permission;
	}

	public RustPlayer(string id, UserData data)
	{
		Id = id;
		Name = data.LastSeenNickname.Sanitize();
		LastCommand = CommandType.Chat;
		IsServer = false;
		perms = Community.Runtime.Core.permission;
	}

	public void AddToGroup(string group)
	{
		if (perms.GroupExists(group) && perms.GetUserData(Id).Groups.Add(group))
		{
			HookCaller.CallStaticHook(3116013984u, Id, group);
		}
	}

	public void Ban(string reason, TimeSpan duration = default(TimeSpan))
	{
		if (!IsBanned && ulong.TryParse(Id, out var result))
		{
			long num = -1L;
			if (duration != TimeSpan.Zero)
			{
				DateTime dateTime = DateTime.UtcNow.Add(duration);
				num = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
			}
			ServerUsers.Set(result, (UserGroup)3, (((Object)(object)BasePlayer != (Object)null) ? BasePlayer.displayName : null) ?? "Unknown", reason, num);
			ServerUsers.Save();
			if (IsConnected)
			{
				Kick(reason);
			}
		}
	}

	public bool BelongsToGroup(string group)
	{
		return perms.UserHasGroup(Id, group);
	}

	public void Command(string command, params object[] args)
	{
		BasePlayer.SendConsoleCommand(command, args);
	}

	public void GrantPermission(string perm)
	{
		perms.GrantUserPermission(Id, perm, null);
	}

	public bool HasPermission(string perm)
	{
		if (IsServer)
		{
			return true;
		}
		return perms.UserHasPermission(Id, perm);
	}

	public void Heal(float amount)
	{
		if (!((Object)(object)BasePlayer == (Object)null))
		{
			((BaseCombatEntity)BasePlayer).Heal(amount);
		}
	}

	public void Hurt(float amount)
	{
		if (!((Object)(object)BasePlayer == (Object)null))
		{
			((BaseCombatEntity)BasePlayer).Hurt(amount);
		}
	}

	public void Kick(string reason)
	{
		if (!((Object)(object)BasePlayer == (Object)null))
		{
			BasePlayer.Kick(reason, true);
		}
	}

	public void Kill()
	{
		if (!((Object)(object)BasePlayer == (Object)null))
		{
			((BaseCombatEntity)BasePlayer).Die((HitInfo)null);
		}
	}

	public void Message(string message, string prefix, params object[] args)
	{
		if (!string.IsNullOrEmpty(message))
		{
			message = ((args.Length != 0) ? string.Format(Formatter.ToUnity(message), args) : Formatter.ToUnity(message));
			string text = ((prefix != null) ? (prefix + " " + message) : message);
			if (IsServer)
			{
				Logger.Log(text);
				return;
			}
			BasePlayer.SendConsoleCommand("chat.add", new object[3] { 2, Id, text });
		}
	}

	public void Message(string message)
	{
		object[] args = Array.Empty<string>();
		Message(message, null, args);
	}

	public void Position(out float x, out float y, out float z)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)BasePlayer == (Object)null)
		{
			x = (y = (z = 0f));
			return;
		}
		Vector3 position = ((Component)BasePlayer).transform.position;
		x = position.x;
		y = position.y;
		z = position.z;
	}

	public GenericPosition Position()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)BasePlayer == (Object)null)
		{
			return GenericPosition.Blank;
		}
		Vector3 position = ((Component)BasePlayer).transform.position;
		return new GenericPosition(position.x, position.y, position.z);
	}

	public void RemoveFromGroup(string name)
	{
		if (!perms.GroupExists(name))
		{
			return;
		}
		UserData userData = perms.GetUserData(Id);
		if (name.Equals("*"))
		{
			if (userData.Groups.Count > 0)
			{
				userData.Groups.Clear();
			}
		}
		else if (userData.Groups.Remove(name))
		{
			HookCaller.CallStaticHook(1018697706u, Id, name);
		}
	}

	public void Rename(string name)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)BasePlayer == (Object)null)
		{
			perms.UpdateNickname(Id, name);
			return;
		}
		name = (string.IsNullOrEmpty(name.Trim()) ? BasePlayer.displayName : name);
		SingletonComponent<ServerMgr>.Instance.persistance.SetPlayerName(EncryptedValue<ulong>.op_Implicit(BasePlayer.userID), name);
		((BaseNetworkable)BasePlayer).net.connection.username = name;
		BasePlayer.displayName = name;
		((BaseEntity)BasePlayer)._name = name;
		((BaseNetworkable)BasePlayer).SendNetworkUpdateImmediate();
		RustPlayer rustPlayer = BasePlayer.AsIPlayer();
		rustPlayer.Name = name;
		perms.UpdateNickname(BasePlayer.UserIDString, name);
		Vector3 position = ((Component)BasePlayer).transform.position;
		Teleport(position.x, position.y, position.z);
	}

	public void Reply(string message, string prefix, params object[] args)
	{
		Message(message, prefix, args);
	}

	public void Reply(string message)
	{
		Message(message);
	}

	public void RevokePermission(string permission)
	{
		if (string.IsNullOrEmpty(permission))
		{
			return;
		}
		UserData userData = perms.GetUserData(Id);
		if (permission.EndsWith("*"))
		{
			if (!permission.Equals("*"))
			{
				userData.Perms.RemoveWhere((string p) => p.StartsWith(permission.TrimEnd('*'), StringComparison.OrdinalIgnoreCase));
			}
			else if (userData.Perms.Count > 0)
			{
				userData.Perms.Clear();
			}
		}
		else if (userData.Perms.Remove(permission))
		{
			HookCaller.CallStaticHook(1879829838u, Id, permission);
		}
	}

	public void Teleport(float x, float y, float z)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)BasePlayer == (Object)null || !((BaseCombatEntity)BasePlayer).IsAlive() || BasePlayer.IsSpectating())
		{
			return;
		}
		try
		{
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(x, y, z);
			BasePlayer.EnsureDismounted();
			((BaseEntity)BasePlayer).SetParent((BaseEntity)null, true, true);
			BasePlayer.SetServerFall(true);
			BasePlayer.MovePosition(val, true);
			((BaseEntity)BasePlayer).ClientRPC(RpcTarget.Player("ForcePositionTo", BasePlayer), val);
		}
		finally
		{
			BasePlayer.SetServerFall(false);
		}
	}

	public void Teleport(GenericPosition position)
	{
		Teleport(position.X, position.Y, position.Z);
	}

	public void Unban()
	{
		if (IsBanned)
		{
			ServerUsers.Remove(ulong.Parse(Id));
			ServerUsers.Save();
		}
	}
}
