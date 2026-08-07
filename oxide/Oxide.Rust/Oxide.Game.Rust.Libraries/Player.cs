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
		return (player?.net?.connection != null) ? Address(player.net.connection) : null;
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
		return ServerUsers.Is(id, ServerUsers.UserGroup.Owner) || DeveloperList.Contains(id);
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
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = Players.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!nameOrIdOrIp.Equals(current.displayName, StringComparison.OrdinalIgnoreCase) && !nameOrIdOrIp.Equals(current.UserIDString) && !nameOrIdOrIp.Equals(current.net.connection.ipaddress))
				{
					continue;
				}
				return current;
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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = Players.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!id.Equals(current.UserIDString))
				{
					continue;
				}
				return current;
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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = Players.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!id.Equals(current.userID))
				{
					continue;
				}
				return current;
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
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)player).transform.position;
		PlayerInventory playerInventory = Inventory(player);
		for (int i = 0; i < playerInventory.containerMain.capacity; i++)
		{
			global::Item slot = playerInventory.containerMain.GetSlot(i);
			if (slot.info.itemid == itemId)
			{
				slot.Drop(position + new Vector3(0f, 1f, 0f) + position / 2f, (position + new Vector3(0f, 0.2f, 0f)) * 8f);
			}
		}
		for (int j = 0; j < playerInventory.containerBelt.capacity; j++)
		{
			global::Item slot2 = playerInventory.containerBelt.GetSlot(j);
			if (slot2.info.itemid == itemId)
			{
				slot2.Drop(position + new Vector3(0f, 1f, 0f) + position / 2f, (position + new Vector3(0f, 0.2f, 0f)) * 8f);
			}
		}
		for (int k = 0; k < playerInventory.containerWear.capacity; k++)
		{
			global::Item slot3 = playerInventory.containerWear.GetSlot(k);
			if (slot3.info.itemid == itemId)
			{
				slot3.Drop(position + new Vector3(0f, 1f, 0f) + position / 2f, (position + new Vector3(0f, 0.2f, 0f)) * 8f);
			}
		}
	}

	public void DropItem(BasePlayer player, global::Item item)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)player).transform.position;
		PlayerInventory playerInventory = Inventory(player);
		for (int i = 0; i < playerInventory.containerMain.capacity; i++)
		{
			global::Item slot = playerInventory.containerMain.GetSlot(i);
			if (slot == item)
			{
				slot.Drop(position + new Vector3(0f, 1f, 0f) + position / 2f, (position + new Vector3(0f, 0.2f, 0f)) * 8f);
			}
		}
		for (int j = 0; j < playerInventory.containerBelt.capacity; j++)
		{
			global::Item slot2 = playerInventory.containerBelt.GetSlot(j);
			if (slot2 == item)
			{
				slot2.Drop(position + new Vector3(0f, 1f, 0f) + position / 2f, (position + new Vector3(0f, 0.2f, 0f)) * 8f);
			}
		}
		for (int k = 0; k < playerInventory.containerWear.capacity; k++)
		{
			global::Item slot3 = playerInventory.containerWear.GetSlot(k);
			if (slot3 == item)
			{
				slot3.Drop(position + new Vector3(0f, 1f, 0f) + position / 2f, (position + new Vector3(0f, 0.2f, 0f)) * 8f);
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
