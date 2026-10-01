using System.Collections.Generic;
using Development.Attributes;
using Facepunch;
using ProtoBuf;

public static class ClanLeaderboardExtensions
{
	[PoolAnalyzerGetWrapper]
	public static ClanLeaderboard ToProto(this List<ClanLeaderboardEntry> leaderboard)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		List<Entry> list = Pool.Get<List<Entry>>();
		foreach (ClanLeaderboardEntry item in leaderboard)
		{
			list.Add(ToProto(item));
		}
		ClanLeaderboard val = Pool.Get<ClanLeaderboard>();
		val.entries = list;
		return val;
	}

	[PoolAnalyzerGetWrapper]
	public static Entry ToProto(this ClanLeaderboardEntry entry)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Entry val = Pool.Get<Entry>();
		val.clanId = entry.ClanId;
		val.name = entry.Name;
		val.score = entry.Score;
		return val;
	}
}
