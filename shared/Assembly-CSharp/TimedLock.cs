using System.Collections.Generic;
using Facepunch;
using ProtoBuf;

public class TimedLock : BaseLock
{
	private Dictionary<ulong, TimeSince> authedUsers = new Dictionary<ulong, TimeSince>();

	public override void Save(SaveInfo info)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		TimedAccess val = Pool.Get<TimedAccess>();
		info.msg.timedAccess = val;
		val.authedUsers = Pool.Get<List<TimedUserAccess>>();
		foreach (KeyValuePair<ulong, TimeSince> authedUser in authedUsers)
		{
			TimedUserAccess val2 = Pool.Get<TimedUserAccess>();
			val2.steamid = authedUser.Key;
			val2.secondsLeft = TimeSince.op_Implicit(authedUser.Value);
			val.authedUsers.Add(val2);
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.timedAccess == null)
		{
			return;
		}
		authedUsers.Clear();
		foreach (TimedUserAccess authedUser in info.msg.timedAccess.authedUsers)
		{
			if (!(authedUser.secondsLeft <= 0f))
			{
				authedUsers[authedUser.steamid] = TimeSince.op_Implicit(authedUser.secondsLeft);
			}
		}
	}

	public void AuthUser(ulong steamId, float duration)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		authedUsers[steamId] = TimeSince.op_Implicit(duration);
	}

	private bool HasAccess(ulong steamId)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (authedUsers.TryGetValue(steamId, out var value))
		{
			return TimeSince.op_Implicit(value) > 0f;
		}
		return false;
	}

	public override bool OnTryToOpen(BasePlayer player)
	{
		return HasAccess(player.userID);
	}

	public override bool OnTryToClose(BasePlayer player)
	{
		return HasAccess(player.userID);
	}

	public override bool HasLockPermission(BasePlayer player)
	{
		return HasAccess(player.userID);
	}

	public override bool GetPlayerLockPermission(BasePlayer player)
	{
		return HasAccess(player.userID);
	}
}
