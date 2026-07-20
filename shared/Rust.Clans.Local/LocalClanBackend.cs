using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facepunch.Sqlite;
using Oxide.Core;

public class LocalClanBackend : IClanBackend, IDisposable, IClanChangeSink
{
	private readonly string _rootFolder;

	public readonly int MaxMemberCount;

	private readonly Dictionary<long, LocalClan> _clans;

	private IClanChangeSink _changeSink;

	public readonly LocalClanDatabase Database;

	public LocalClanBackend(string rootFolder, int version, int maxMemberCount)
	{
		_rootFolder = rootFolder;
		MaxMemberCount = Math.Max(maxMemberCount, 0);
		_clans = new Dictionary<long, LocalClan>();
		Database = new LocalClanDatabase(version);
	}

	public async ValueTask Initialize(IClanChangeSink changeSink)
	{
		_changeSink = changeSink ?? throw new ArgumentNullException("changeSink");
		Database.Open(_rootFolder);
	}

	public void Dispose()
	{
		Database.Close();
	}

	public async ValueTask<ClanValueResult<IClan>> Get(long clanId)
	{
		if (clanId <= 0)
		{
			return ClanValueResult<IClan>.op_Implicit((ClanResult)4);
		}
		LocalClan clan;
		return TryGetClan(clanId, out clan) ? new ClanValueResult<IClan>((IClan)(object)clan) : ClanValueResult<IClan>.op_Implicit((ClanResult)4);
	}

	public bool TryGet(long clanId, out IClan clan)
	{
		if (_clans.TryGetValue(clanId, out var value))
		{
			clan = (IClan)(object)value;
			return true;
		}
		clan = null;
		return false;
	}

	public async ValueTask<ClanValueResult<IClan>> GetByMember(ulong steamId)
	{
		long? num = Database.FindClanByMember(steamId);
		if (!num.HasValue)
		{
			return ClanValueResult<IClan>.op_Implicit((ClanResult)3);
		}
		LocalClan clan;
		return TryGetClan(num.Value, out clan) ? new ClanValueResult<IClan>((IClan)(object)clan) : ClanValueResult<IClan>.op_Implicit((ClanResult)4);
	}

	public async ValueTask<ClanValueResult<IClan>> Create(ulong leaderSteamId, string name)
	{
		Database.BeginTransaction();
		long clanId;
		try
		{
			try
			{
				clanId = Database.CreateClan(name, leaderSteamId) ?? throw new Exception("Failed to create clan");
			}
			catch (SqliteException ex) when (ex.Result == 2067)
			{
				Database.Rollback();
				return ClanValueResult<IClan>.op_Implicit((ClanResult)13);
			}
			int num = Database.CreateRole(clanId, new ClanRole
			{
				Name = "Leader",
				CanSetMotd = true,
				CanSetLogo = true,
				CanInvite = true,
				CanKick = true,
				CanPromote = true,
				CanDemote = true,
				CanSetPlayerNotes = true,
				CanAccessScoreEvents = true,
				CanAccessLogs = true
			}) ?? throw new Exception("Failed to create leader role");
			if (num != 1)
			{
				throw new Exception("Owner role does not have rank 1!");
			}
			_ = Database.CreateRole(clanId, new ClanRole
			{
				Name = "Member",
				CanSetMotd = false,
				CanSetLogo = false,
				CanInvite = false,
				CanKick = false,
				CanPromote = false,
				CanDemote = false,
				CanSetPlayerNotes = false,
				CanAccessScoreEvents = true,
				CanAccessLogs = false
			}) ?? throw new Exception("Failed to create member role");
			try
			{
				if (!Database.CreateMember(clanId, leaderSteamId, num))
				{
					throw new Exception("Failed to add leader to new clan");
				}
			}
			catch (SqliteException ex2) when (ex2.Result == 2067)
			{
				Database.Rollback();
				return ClanValueResult<IClan>.op_Implicit((ClanResult)19);
			}
			Database.AppendLog(clanId, "founded", leaderSteamId);
			Database.Commit();
		}
		catch
		{
			Database.Rollback();
			throw;
		}
		if (!TryGetClan(clanId, out var clan))
		{
			throw new Exception("Couldn't find the clan we just created?");
		}
		MembershipChanged(leaderSteamId, clan.ClanId);
		ClanValueResult<IClan> result = ClanValueResult<IClan>.op_Implicit((IClan)(object)clan);
		Interface.CallHook("OnClanCreated", clan, leaderSteamId);
		return result;
	}

	public async ValueTask<ClanValueResult<List<ClanInvitation>>> ListInvitations(ulong steamId)
	{
		return ClanValueResult<List<ClanInvitation>>.op_Implicit(Database.ListInvitationsForPlayer(steamId));
	}

	public async ValueTask<ClanValueResult<List<ClanLeaderboardEntry>>> GetLeaderboard(int limit)
	{
		return ClanValueResult<List<ClanLeaderboardEntry>>.op_Implicit(Database.ListTopClans(limit));
	}

	private bool TryGetClan(long clanId, out LocalClan clan)
	{
		if (_clans.TryGetValue(clanId, out var value))
		{
			clan = value;
			return value != null;
		}
		LocalClan localClan = new LocalClan(this, clanId);
		if (!localClan.Refresh((ClanDataSource)(-1)))
		{
			_clans.Add(clanId, null);
			clan = null;
			return false;
		}
		_clans.Add(clanId, localClan);
		clan = localClan;
		return true;
	}

	public void ClanChanged(long clanId, ClanDataSource dataSources)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_changeSink.ClanChanged(clanId, dataSources);
	}

	public void ClanDisbanded(long clanId)
	{
		_clans.Remove(clanId);
		_changeSink.ClanDisbanded(clanId);
	}

	public void InvitationCreated(ulong steamId, long clanId)
	{
		_changeSink.InvitationCreated(steamId, clanId);
	}

	public void MembershipChanged(ulong steamId, long? clanId)
	{
		_changeSink.MembershipChanged(steamId, clanId);
	}

	public void ClanChatMessage(long clanId, ClanChatEntry entry)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_changeSink.ClanChatMessage(clanId, entry);
	}
}
