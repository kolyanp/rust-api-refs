using System.Collections.Generic;
using Development.Attributes;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public static class ClanInfoExtensions
{
	[PoolAnalyzerGetWrapper]
	public static ClanInfo ToProto(this IClan clan)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		if (clan == null)
		{
			return null;
		}
		ClanInfo val = Pool.Get<ClanInfo>();
		val.clanId = clan.ClanId;
		val.name = clan.Name;
		val.created = clan.Created;
		val.creator = clan.Creator;
		val.motd = clan.Motd;
		val.motdTimestamp = clan.MotdTimestamp;
		val.motdAuthor = clan.MotdAuthor;
		val.logo = clan.Logo;
		val.color = clan.Color.ToInt32();
		val.maxMemberCount = clan.MaxMemberCount;
		val.score = clan.Score;
		val.roles = Pool.Get<List<Role>>();
		foreach (ClanRole role in clan.Roles)
		{
			val.roles.Add(role.ToProto());
		}
		val.members = Pool.Get<List<Member>>();
		foreach (ClanMember member in clan.Members)
		{
			val.members.Add(member.ToProto());
		}
		val.invites = Pool.Get<List<Invite>>();
		foreach (ClanInvite invite in clan.Invites)
		{
			val.invites.Add(invite.ToProto());
		}
		return val;
	}

	[PoolAnalyzerGetWrapper]
	private static Role ToProto(this ClanRole role)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		bool flag = role.Rank == 1;
		Role val = Pool.Get<Role>();
		val.roleId = role.RoleId;
		val.rank = role.Rank;
		val.name = role.Name;
		val.canSetMotd = flag || role.CanSetMotd;
		val.canSetLogo = flag || role.CanSetLogo;
		val.canInvite = flag || role.CanInvite;
		val.canKick = flag || role.CanKick;
		val.canPromote = flag || role.CanPromote;
		val.canDemote = flag || role.CanDemote;
		val.canSetPlayerNotes = flag || role.CanSetPlayerNotes;
		val.canAccessLogs = flag || role.CanAccessLogs;
		val.canAccessScoreEvents = flag || role.CanAccessScoreEvents;
		return val;
	}

	public static ClanRole FromProto(this Role proto)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		return new ClanRole
		{
			RoleId = proto.roleId,
			Rank = proto.rank,
			Name = proto.name,
			CanSetMotd = proto.canSetMotd,
			CanSetLogo = proto.canSetLogo,
			CanInvite = proto.canInvite,
			CanKick = proto.canKick,
			CanPromote = proto.canPromote,
			CanDemote = proto.canDemote,
			CanSetPlayerNotes = proto.canSetPlayerNotes,
			CanAccessLogs = proto.canAccessLogs,
			CanAccessScoreEvents = proto.canAccessScoreEvents
		};
	}

	[PoolAnalyzerGetWrapper]
	private static Member ToProto(this ClanMember member)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Member val = Pool.Get<Member>();
		val.steamId = member.SteamId;
		val.roleId = member.RoleId;
		val.joined = member.Joined;
		val.lastSeen = member.LastSeen;
		val.notes = member.Notes;
		val.online = (NexusServer.Started ? NexusServer.IsOnline(member.SteamId) : ServerPlayers.IsOnline(member.SteamId));
		return val;
	}

	[PoolAnalyzerGetWrapper]
	private static Invite ToProto(this ClanInvite invite)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Invite val = Pool.Get<Invite>();
		val.steamId = invite.SteamId;
		val.recruiter = invite.Recruiter;
		val.timestamp = invite.Timestamp;
		return val;
	}
}
