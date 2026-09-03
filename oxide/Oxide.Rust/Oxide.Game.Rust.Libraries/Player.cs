using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Facepunch;
using Network;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;

namespace Oxide.Game.Rust.Libraries;

public class Player : Library
{
	private static readonly string ipPattern = ":{1}[0-9]{1}\\d*";

	internal readonly Permission permission = Interface.Oxide.GetLibrary<Permission>();

	public ListHashSet<BasePlayer> Players => BasePlayer.activePlayerList;

	public ListHashSet<BasePlayer> Sleepers => BasePlayer.sleepingPlayerList;

	public CultureInfo Language(BasePlayer player)
	{
		try
		{
			return CultureInfo.GetCultureInfo(player.net.connection.language ?? "en");
		}
		catch (CultureNotFoundException)
		{
			return CultureInfo.GetCultureInfo("en");
		}
	}

	public string Address(Connection connection)
	{
		return Regex.Replace(connection.ipaddress, ipPattern, "");
	}

	public string Address(BasePlayer player)
	{
		if (player?.net?.connection == null)
		{
			return null;
		}
		return Address(player.net.connection);
	}

	public int Ping(Connection connection)
	{
		return Net.sv.GetAveragePing(connection);
	}

	public int Ping(BasePlayer player)
	{
		return Ping(player.net.connection);
	}

	public bool IsAdmin(ulong id)
	{
		if (!ServerUsers.Is(id, ServerUsers.UserGroup.Owner))
		{
			return DeveloperList.Contains(id);
		}
		return true;
	}

	public bool IsAdmin(string id)
	{
		return IsAdmin(Convert.ToUInt64(id));
	}

	public bool IsAdmin(BasePlayer player)
	{
		return IsAdmin(player.userID);
	}

	public bool IsBanned(ulong id)
	{
		return ServerUsers.Is(id, ServerUsers.UserGroup.Banned);
	}

	public bool IsBanned(string id)
	{
		return IsBanned(Convert.ToUInt64(id));
	}

	public bool IsBanned(BasePlayer player)
	{
		return IsBanned(player.userID);
	}

	public bool IsConnected(BasePlayer player)
	{
		return player.IsConnected;
	}

	public bool IsSleeping(ulong id)
	{
		return Object.op_Implicit((Object)(object)BasePlayer.FindSleeping(id));
	}

	public bool IsSleeping(string id)
	{
		return IsSleeping(Convert.ToUInt64(id));
	}

	public bool IsSleeping(BasePlayer player)
	{
		return IsSleeping(player.userID);
	}

	public void Ban(ulong id, string reason = "", long expiry = -1L)
	{
		if (!IsBanned(id))
		{
			BasePlayer basePlayer = FindById(id);
			ServerUsers.Set(id, ServerUsers.UserGroup.Banned, basePlayer?.displayName ?? "Unknown", reason, expiry);
			ServerUsers.Save();
			if ((Object)(object)basePlayer != (Object)null && IsConnected(basePlayer))
			{
				Kick(basePlayer, reason);
			}
		}
	}

	public void Ban(string id, string reason = "", long expiry = -1L)
	{
		Ban(Convert.ToUInt64(id), reason, expiry);
	}

	public void Ban(BasePlayer player, string reason = "", long expiry = -1L)
	{
		Ban(player.UserIDString, reason, expiry);
	}

	public void Heal(BasePlayer player, float amount)
	{
		player.Heal(amount);
	}

	public void Hurt(BasePlayer player, float amount)
	{
		player.Hurt(amount);
	}

	public void Kick(BasePlayer player, string reason = "")
	{
		player.Kick(reason);
	}

	public void Kill(BasePlayer player)
	{
		player.Die();
	}

	public void Rename(BasePlayer player, string name)
	{
		name = (string.IsNullOrEmpty(name.Trim()) ? player.displayName : name);
		SingletonComponent<ServerMgr>.Instance.persistance.SetPlayerName(player.userID, name);
		player.net.connection.username = name;
		player.displayName = name;
		player._name = name;
		player.SendNetworkUpdateImmediate();
		player.IPlayer.Name = name;
		permission.UpdateNickname(player.UserIDString, name);
		if (player.net.group == BaseNetworkable.LimboNetworkGroup)
		{
			return;
		}
		List<Connection> list = Pool.Get<List<Connection>>();
		for (int i = 0; i < Net.sv.connections.Count; i++)
		{
			Connection connection = Net.sv.connections[i];
			if (connection.connected && connection.isAuthenticated && connection.player is BasePlayer && (Object)(object)connection.player != (Object)(object)player)
			{
				list.Add(connection);
			}
		}
		player.OnNetworkSubscribersLeave(list);
		Pool.FreeUnmanaged<Connection>(ref list);
		if (!player.limitNetworking)
		{
			player.syncPosition = false;
			player._limitedNetworking = true;
			Interface.Oxide.NextTick(delegate
			{
				player.syncPosition = true;
				player._limitedNetworking = false;
				player.UpdateNetworkGroup();
				player.SendNetworkUpdate();
			});
		}
	}

	public void Teleport(BasePlayer player, Vector3 destination)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (player.IsAlive() && !player.IsSpectating())
		{
			try
			{
				player.EnsureDismounted();
				player.SetParent(null, worldPositionStays: true, sendImmediate: true);
				player.SetServerFall(wantsOn: true);
				player.MovePosition(destination);
				player.ClientRPC(RpcTarget.Player("ForcePositionTo", player), destination);
			}
			finally
			{
				player.SetServerFall(wantsOn: false);
			}
		}
	}

	public void Teleport(BasePlayer player, BasePlayer target)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		Teleport(player, Position(target));
	}

	public void Teleport(BasePlayer player, float x, float y, float z)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Teleport(player, new Vector3(x, y, z));
	}

	public void Unban(ulong id)
	{
		if (IsBanned(id))
		{
			ServerUsers.Remove(id);
			ServerUsers.Save();
		}
	}

	public void Unban(string id)
	{
		Unban(Convert.ToUInt64(id));
	}

	public void Unban(BasePlayer player)
	{
		Unban(player.userID);
	}

	public Vector3 Position(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)player).transform.position;
	}

	public BasePlayer Find(string nameOrIdOrIp)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = Players.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (nameOrIdOrIp.Equals(current.displayName, StringComparison.OrdinalIgnoreCase) || nameOrIdOrIp.Equals(current.UserIDString) || nameOrIdOrIp.Equals(current.net.connection.ipaddress))
				{
					return current;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return null;
	}

	public BasePlayer FindById(string id)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = Players.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (id.Equals(current.UserIDString))
				{
					return current;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return null;
	}

	public BasePlayer FindById(ulong id)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = Players.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (id.Equals(current.userID))
				{
					return current;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return null;
	}

	public void Message(BasePlayer player, string message, string prefix, ulong userId = 0uL, params object[] args)
	{
		if (!string.IsNullOrEmpty(message))
		{
			message = ((args.Length != 0) ? string.Format(Formatter.ToUnity(message), args) : Formatter.ToUnity(message));
			string text = ((prefix != null) ? (prefix + " " + message) : message);
			if (Interface.CallHook("OnMessagePlayer", text, player, userId) == null)
			{
				player.SendConsoleCommand("chat.add", 2, userId, text);
			}
		}
	}

	public void Message(BasePlayer player, string message, ulong userId = 0uL)
	{
		Message(player, message, null, userId);
	}

	public void Reply(BasePlayer player, string message, string prefix, ulong userId = 0uL, params object[] args)
	{
		Message(player, message, prefix, userId, args);
	}

	public void Reply(BasePlayer player, string message, ulong userId = 0uL)
	{
		Message(player, message, null, userId);
	}

	public void Command(BasePlayer player, string command, params object[] args)
	{
		player.SendConsoleCommand(command, args);
	}

	public void DropItem(BasePlayer player, int itemId)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		PlayerInventory playerInventory = Inventory(player);
		PooledList<global::Item> val = Pool.Get<PooledList<global::Item>>();
		try
		{
			if (playerInventory.containerMain != null)
			{
				playerInventory.containerMain.FindItemsByItemID((List<global::Item>)(object)val, itemId);
			}
			if (playerInventory.containerBelt != null)
			{
				playerInventory.containerBelt.FindItemsByItemID((List<global::Item>)(object)val, itemId);
			}
			if (playerInventory.containerWear != null)
			{
				playerInventory.containerWear.FindItemsByItemID((List<global::Item>)(object)val, itemId);
			}
			for (int i = 0; i < ((List<global::Item>)(object)val).Count; i++)
			{
				((List<global::Item>)(object)val)[i].Drop(player.GetDropPosition(), player.GetDropVelocity());
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void DropItem(BasePlayer player, global::Item item)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (item == null)
		{
			return;
		}
		ItemContainer parent = item.parent;
		if (parent != null)
		{
			PlayerInventory playerInventory = Inventory(player);
			if (parent == playerInventory.containerMain || parent == playerInventory.containerBelt || parent == playerInventory.containerWear)
			{
				item.Drop(player.GetDropPosition(), player.GetDropVelocity());
			}
		}
	}

	public void GiveItem(BasePlayer player, int itemId, int quantity = 1)
	{
		GiveItem(player, Item.GetItem(itemId), quantity);
	}

	public void GiveItem(BasePlayer player, global::Item item, int quantity = 1)
	{
		player.inventory.GiveItem(ItemManager.CreateByItemID(item.info.itemid, quantity, 0uL, 0uL));
	}

	public PlayerInventory Inventory(BasePlayer player)
	{
		return player.inventory;
	}

	public void ClearInventory(BasePlayer player)
	{
		Inventory(player)?.Strip();
	}

	public void ResetInventory(BasePlayer player)
	{
		PlayerInventory playerInventory = Inventory(player);
		if ((Object)(object)playerInventory != (Object)null)
		{
			playerInventory.DoDestroy();
			playerInventory.ServerInit(player);
		}
	}
}
