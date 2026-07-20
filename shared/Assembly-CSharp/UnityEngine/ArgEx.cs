using System.Collections.Generic;

namespace UnityEngine;

public static class ArgEx
{
	public static BasePlayer Player(this ConsoleSystem.Arg arg)
	{
		if (arg == null || arg.Connection == null)
		{
			return null;
		}
		return arg.Connection.player as BasePlayer;
	}

	public static BasePlayer GetPlayer(this ConsoleSystem.Arg arg, int iArgNum)
	{
		string text = arg.GetString(iArgNum, null);
		if (text == null)
		{
			return null;
		}
		return BasePlayer.Find(text);
	}

	public static List<BasePlayer> GetPlayerArgs(this ConsoleSystem.Arg arg, int startArgIndex)
	{
		List<BasePlayer> list = new List<BasePlayer>();
		int num = arg.Args.Length - startArgIndex;
		if (num <= 0)
		{
			return list;
		}
		for (int i = 0; i < num; i++)
		{
			BasePlayer player = GetPlayer(arg, startArgIndex + i);
			if ((Object)(object)player != (Object)null)
			{
				list.Add(player);
				Debug.Log((object)("Added player " + player.displayName));
			}
		}
		return list;
	}

	public static BasePlayer GetSleeper(this ConsoleSystem.Arg arg, int iArgNum)
	{
		string text = arg.GetString(iArgNum);
		if (text == null)
		{
			return null;
		}
		return BasePlayer.FindSleeping(text);
	}

	public static BasePlayer GetPlayerOrSleeper(this ConsoleSystem.Arg arg, int iArgNum)
	{
		string text = arg.GetString(iArgNum);
		if (text == null)
		{
			return null;
		}
		return BasePlayer.FindAwakeOrSleeping(text);
	}

	public static BasePlayer GetPlayerOrSleeperOrBot(this ConsoleSystem.Arg arg, int iArgNum)
	{
		if (arg.TryGetUInt(iArgNum, out var value))
		{
			return BasePlayer.FindBot(value);
		}
		return GetPlayerOrSleeper(arg, iArgNum);
	}

	public static NetworkableId GetEntityID(this ConsoleSystem.Arg arg, int iArg, NetworkableId def = default(NetworkableId))
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return new NetworkableId(arg.GetUInt64(iArg, def.Value));
	}

	public static ItemId GetItemID(this ConsoleSystem.Arg arg, int iArg, ItemId def = default(ItemId))
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return new ItemId(arg.GetUInt64(iArg, def.Value));
	}
}
