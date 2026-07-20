using System.Collections.Generic;
using Oxide.Core.Libraries;
using Oxide.Game.Rust.Libraries.Covalence;
using UnityEngine;

public static class CovalenceEx
{
	public static RustPlayer AsIPlayer(this BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return null;
		}
		RustPlayer rustPlayer = Permission.iPlayerField.GetValue(player) as RustPlayer;
		if (rustPlayer == null)
		{
			Permission.iPlayerField.SetValue(player, rustPlayer = new RustPlayer(player));
		}
		rustPlayer.Object = player;
		return rustPlayer;
	}

	public static RustPlayer AsIPlayer(this KeyValuePair<string, UserData> user)
	{
		if (user.Value == null)
		{
			return null;
		}
		if (user.Value.Player == null)
		{
			user.Value.Player = new RustPlayer(user.Key, user.Value);
		}
		return user.Value.Player;
	}
}
