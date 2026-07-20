using System.Collections.Generic;
using UnityEngine;

public class TriggerAnalytic : TriggerBase, IServerComponent
{
	private struct RecentPlayerEntrance
	{
		public BasePlayer Player;

		public TimeSince Time;
	}

	public string AnalyticMessage;

	public float Timeout = 120f;

	private List<RecentPlayerEntrance> recentEntrances = new List<RecentPlayerEntrance>();

	internal override GameObject InterestedInObject(GameObject obj)
	{
		if (GameObjectEx.ToBaseEntity(obj) is BasePlayer { IsNpc: false, isServer: not false } basePlayer)
		{
			return ((Component)basePlayer).gameObject;
		}
		return null;
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base.OnEntityEnter(ent);
		BasePlayer basePlayer = ent.ToPlayer();
		if ((Object)(object)basePlayer != (Object)null && !basePlayer.IsNpc)
		{
			CheckTimeouts();
			if (IsPlayerValid(basePlayer))
			{
				recentEntrances.Add(new RecentPlayerEntrance
				{
					Player = basePlayer,
					Time = TimeSince.op_Implicit(0f)
				});
			}
		}
	}

	private void CheckTimeouts()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		for (int num = recentEntrances.Count - 1; num >= 0; num--)
		{
			if (TimeSince.op_Implicit(recentEntrances[num].Time) > Timeout)
			{
				recentEntrances.RemoveAt(num);
			}
		}
	}

	private bool IsPlayerValid(BasePlayer p)
	{
		for (int i = 0; i < recentEntrances.Count; i++)
		{
			if ((Object)(object)recentEntrances[i].Player == (Object)(object)p)
			{
				return false;
			}
		}
		return true;
	}
}
