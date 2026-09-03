using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Facepunch;
using Facepunch.Sqlite;
using Oxide.Core;
using UnityEngine;

public class LocalClanDatabase : Database
{
	private readonly int _version;

	private void CreateClansTable()
	{
		Execute("\r\n            CREATE TABLE IF NOT EXISTS clans\r\n            (\r\n                clan_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,\r\n                name TEXT NOT NULL,\r\n                name_normalized TEXT NOT NULL,\r\n                created INTEGER NOT NULL,\r\n                deleted INTEGER NULL,\r\n                creator INTEGER NOT NULL,\r\n                motd TEXT,\r\n                motd_timestamp INTEGER,\r\n                motd_author INTEGER,\r\n                logo BLOB,\r\n                logo_timestamp INTEGER,\r\n                color INTEGER,\r\n                score INTEGER NOT NULL DEFAULT 0\r\n            );\r\n        ");
		Execute("CREATE UNIQUE INDEX IF NOT EXISTS clans_name_normalized ON clans (name_normalized) WHERE deleted IS NULL;");
		Execute("CREATE INDEX IF NOT EXISTS clans_score ON clans (score DESC) WHERE deleted IS NULL;");
	}

	public long? CreateClan(string name, ulong creatorSteamId)
	{
		string value = name.ToLowerInvariant().Normalize(NormalizationForm.FormKC);
		IntPtr stmHandle = Prepare("INSERT INTO clans (name, name_normalized, created, creator) VALUES (?, ?, ?, ?) RETURNING clan_id");
		Database.Bind(stmHandle, 1, name);
		Database.Bind(stmHandle, 2, value);
		Database.Bind(stmHandle, 3, ClanUtility.Timestamp());
		Database.Bind(stmHandle, 4, creatorSteamId);
		long num = ExecuteAndReadQueryResult<long>(stmHandle);
		if (num <= 0)
		{
			return null;
		}
		return num;
	}

	public ClanData? ReadClan(long clanId)
	{
		IntPtr stmHandle = Prepare("SELECT name, created, creator, motd, motd_timestamp, motd_author, logo, logo_timestamp, color, score FROM clans WHERE clan_id = ? AND deleted IS NULL");
		Database.Bind(stmHandle, 1, clanId);
		return ExecuteAndReadQueryResult(stmHandle, delegate(IntPtr stm)
		{
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			return new ClanData
			{
				Name = Database.GetColumnValue<string>(stm, 0),
				Created = Database.GetColumnValue<long>(stm, 1),
				Creator = Database.GetColumnValue<ulong>(stm, 2),
				Motd = Database.GetColumnValue<string>(stm, 3),
				MotdTimestamp = Database.GetColumnValue<long>(stm, 4),
				MotdAuthor = Database.GetColumnValue<ulong>(stm, 5),
				Logo = Database.GetColumnValue<byte[]>(stm, 6),
				LogoTimestamp = Database.GetColumnValue<long>(stm, 7),
				Color = ColorEx.FromInt32(Database.GetColumnValue<int>(stm, 8)),
				Score = Database.GetColumnValue<long>(stm, 9)
			};
		});
	}

	public bool UpdateClanMotd(long clanId, string newMotd, ulong authorSteamId)
	{
		Execute("UPDATE clans SET motd = ?, motd_timestamp = ?, motd_author = ? WHERE clan_id = ? AND deleted IS NULL", newMotd, ClanUtility.Timestamp(), authorSteamId, clanId);
		return base.AffectedRows > 0;
	}

	public bool UpdateClanLogo(long clanId, byte[] newLogo)
	{
		Execute("UPDATE clans SET logo = ?, logo_timestamp = ? WHERE clan_id = ? AND deleted IS NULL", newLogo, ClanUtility.Timestamp(), clanId);
		return base.AffectedRows > 0;
	}

	public bool UpdateClanColor(long clanId, Color32 newColor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Execute("UPDATE clans SET color = ? WHERE clan_id = ? AND deleted IS NULL", newColor.ToInt32(), clanId);
		return base.AffectedRows > 0;
	}

	public bool DeleteClan(long clanId)
	{
		BeginTransaction();
		Execute("UPDATE clans SET deleted = ? WHERE clan_id = ? AND deleted IS NULL", ClanUtility.Timestamp(), clanId);
		if (base.AffectedRows == 0)
		{
			Rollback();
			return false;
		}
		Execute("DELETE FROM members WHERE clan_id = ?", clanId);
		Execute("DELETE FROM invites WHERE clan_id = ?", clanId);
		Commit();
		return true;
	}

	public long? FindClanByMember(ulong memberSteamId)
	{
		long num = Query<long, ulong>("SELECT clan_id FROM members WHERE user_id = ?", memberSteamId);
		if (num <= 0)
		{
			return null;
		}
		return num;
	}

	public List<ClanLeaderboardEntry> ListTopClans(int limit)
	{
		IntPtr stmHandle = Prepare("SELECT clan_id, name, score FROM clans WHERE deleted IS NULL ORDER BY score DESC LIMIT ?");
		Database.Bind(stmHandle, 1, Mathf.Clamp(limit, 10, 100));
		List<ClanLeaderboardEntry> list = Pool.Get<List<ClanLeaderboardEntry>>();
		ExecuteAndReadQueryResults<ClanLeaderboardEntry>(stmHandle, list, delegate(IntPtr stm)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			return new ClanLeaderboardEntry
			{
				ClanId = Database.GetColumnValue<long>(stm, 0),
				Name = Database.GetColumnValue<string>(stm, 1),
				Score = Database.GetColumnValue<long>(stm, 2)
			};
		});
		return list;
	}

	private void CreateInvitesTable()
	{
		Execute("\r\n            CREATE TABLE IF NOT EXISTS invites\r\n            (\r\n                clan_id INTEGER NOT NULL,\r\n                user_id INTEGER NOT NULL,\r\n                recruiter INTEGER NOT NULL,\r\n                timestamp INTEGER NOT NULL,\r\n                PRIMARY KEY (clan_id, user_id),\r\n                FOREIGN KEY (clan_id) REFERENCES clans (clan_id) ON DELETE CASCADE\r\n            ) WITHOUT ROWID;\r\n        ");
		Execute("\r\n            CREATE INDEX IF NOT EXISTS invites_player ON invites (user_id);\r\n        ");
	}

	public bool CreateInvite(long clanId, ulong steamId, ulong recruiterSteamId)
	{
		Execute("\r\n            INSERT OR IGNORE INTO invites (clan_id, user_id, recruiter, timestamp)\r\n            SELECT ?1, ?2, ?3, ?4\r\n            FROM (SELECT 1)\r\n            WHERE NOT EXISTS (SELECT * FROM members m WHERE m.user_id = ?2);\r\n        ", clanId, steamId, recruiterSteamId, ClanUtility.Timestamp());
		return base.AffectedRows > 0;
	}

	public bool AcceptInvite(long clanId, ulong steamId)
	{
		BeginTransaction();
		try
		{
			if (DeleteInvite(clanId, steamId) && CreateMember(clanId, steamId))
			{
				Commit();
				Interface.CallHook("OnClanMemberAdded", clanId, steamId);
				return true;
			}
			Rollback();
			return false;
		}
		catch
		{
			Rollback();
			throw;
		}
	}

	public List<ClanInvite> ListInvites(long clanId)
	{
		IntPtr stmHandle = Prepare("SELECT user_id, recruiter, timestamp FROM invites WHERE clan_id = ? ORDER BY timestamp ASC");
		Database.Bind(stmHandle, 1, clanId);
		List<ClanInvite> list = Pool.Get<List<ClanInvite>>();
		ExecuteAndReadQueryResults<ClanInvite>(stmHandle, list, delegate(IntPtr stm)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			return new ClanInvite
			{
				SteamId = Database.GetColumnValue<ulong>(stm, 0),
				Recruiter = Database.GetColumnValue<ulong>(stm, 1),
				Timestamp = Database.GetColumnValue<long>(stm, 2)
			};
		});
		return list;
	}

	public List<ClanInvitation> ListInvitationsForPlayer(ulong steamId)
	{
		IntPtr stmHandle = Prepare("SELECT clan_id, recruiter, timestamp FROM invites WHERE user_id = ? ORDER BY timestamp ASC");
		Database.Bind(stmHandle, 1, steamId);
		List<ClanInvitation> list = Pool.Get<List<ClanInvitation>>();
		ExecuteAndReadQueryResults<ClanInvitation>(stmHandle, list, delegate(IntPtr stm)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			return new ClanInvitation
			{
				ClanId = Database.GetColumnValue<long>(stm, 0),
				Recruiter = Database.GetColumnValue<ulong>(stm, 1),
				Timestamp = Database.GetColumnValue<long>(stm, 2)
			};
		});
		return list;
	}

	public bool DeleteInvite(long clanId, ulong steamId)
	{
		Execute("DELETE FROM invites WHERE clan_id = ? AND user_id = ?", clanId, steamId);
		return base.AffectedRows > 0;
	}

	public void DeleteAllInvites(ulong steamId)
	{
		Execute("DELETE FROM invites WHERE user_id = ?", steamId);
	}

	private void CreateLogsTable()
	{
		Execute("\r\n            CREATE TABLE IF NOT EXISTS logs\r\n            (\r\n                clan_id INTEGER NOT NULL,\r\n                timestamp INTEGER NOT NULL,\r\n                event TEXT NOT NULL,\r\n                arg1 TEXT,\r\n                arg2 TEXT,\r\n                arg3 TEXT,\r\n                arg4 TEXT,\r\n                FOREIGN KEY (clan_id) REFERENCES clans (clan_id) ON DELETE CASCADE\r\n            );\r\n        ");
		Execute("\r\n            CREATE INDEX IF NOT EXISTS logs_ordered ON logs (clan_id, timestamp DESC);\r\n        ");
	}

	public void AppendLog(long clanId, string eventKey)
	{
		Execute("INSERT INTO logs (clan_id, timestamp, event) VALUES (?, ?, ?)", clanId, ClanUtility.Timestamp(), eventKey);
	}

	public void AppendLog<T1>(long clanId, string eventKey, T1 arg1)
	{
		Execute("INSERT INTO logs (clan_id, timestamp, event, arg1) VALUES (?, ?, ?, ?)", clanId, ClanUtility.Timestamp(), eventKey, arg1);
	}

	public void AppendLog<T1, T2>(long clanId, string eventKey, T1 arg1, T2 arg2)
	{
		Execute("INSERT INTO logs (clan_id, timestamp, event, arg1, arg2) VALUES (?, ?, ?, ?, ?)", clanId, ClanUtility.Timestamp(), eventKey, arg1, arg2);
	}

	public void AppendLog<T1, T2, T3>(long clanId, string eventKey, T1 arg1, T2 arg2, T3 arg3)
	{
		Execute("INSERT INTO logs (clan_id, timestamp, event, arg1, arg2, arg3) VALUES (?, ?, ?, ?, ?, ?)", clanId, ClanUtility.Timestamp(), eventKey, arg1, arg2, arg3);
	}

	public void AppendLog<T1, T2, T3, T4>(long clanId, string eventKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		Execute("INSERT INTO logs (clan_id, timestamp, event, arg1, arg2, arg3, arg4) VALUES (?, ?, ?, ?, ?, ?, ?)", clanId, ClanUtility.Timestamp(), eventKey, arg1, arg2, arg3, arg4);
	}

	public List<ClanLogEntry> ReadLogs(long clanId, int limit)
	{
		IntPtr stmHandle = Prepare("SELECT timestamp, event, arg1, arg2, arg3, arg4 FROM logs WHERE clan_id = ? ORDER BY timestamp DESC LIMIT ?");
		Database.Bind(stmHandle, 1, clanId);
		Database.Bind(stmHandle, 2, Mathf.Clamp(limit, 10, 1000));
		List<ClanLogEntry> list = Pool.Get<List<ClanLogEntry>>();
		ExecuteAndReadQueryResults<ClanLogEntry>(stmHandle, list, delegate(IntPtr stm)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			return new ClanLogEntry
			{
				Timestamp = Database.GetColumnValue<long>(stm, 0),
				EventKey = Database.GetColumnValue<string>(stm, 1),
				Arg1 = Database.GetColumnValue<string>(stm, 2),
				Arg2 = Database.GetColumnValue<string>(stm, 3),
				Arg3 = Database.GetColumnValue<string>(stm, 4),
				Arg4 = Database.GetColumnValue<string>(stm, 5)
			};
		});
		return list;
	}

	private void CreateMembersTable()
	{
		Execute("\r\n            CREATE TABLE IF NOT EXISTS members\r\n            (\r\n                clan_id INTEGER NOT NULL,\r\n                user_id INTEGER NOT NULL,\r\n                role_id INTEGER NOT NULL,\r\n                joined INTEGER NOT NULL,\r\n                seen INTEGER NOT NULL,\r\n                notes TEXT,\r\n                notes_timestamp INTEGER,\r\n                PRIMARY KEY (clan_id, user_id),\r\n                UNIQUE (user_id),\r\n                FOREIGN KEY (clan_id) REFERENCES clans (clan_id) ON DELETE CASCADE,\r\n                FOREIGN KEY (clan_id, role_id) REFERENCES roles (clan_id, role_id) ON UPDATE CASCADE ON DELETE RESTRICT\r\n            ) WITHOUT ROWID;\r\n        ");
	}

	public bool CreateMember(long clanId, ulong steamId)
	{
		Execute("\r\n            INSERT OR IGNORE INTO members (clan_id, user_id, role_id, joined, seen)\r\n            SELECT ?1, ?2, MAX(r.role_id), ?3, ?3\r\n            FROM (SELECT role_id FROM roles WHERE clan_id = ?1 ORDER BY rank DESC LIMIT 1) r\r\n        ", clanId, steamId, ClanUtility.Timestamp());
		return base.AffectedRows > 0;
	}

	public bool CreateMember(long clanId, ulong steamId, int roleId)
	{
		Execute("INSERT INTO members (clan_id, user_id, role_id, joined, seen) VALUES (?1, ?2, ?3, ?4, ?4)", clanId, steamId, roleId, ClanUtility.Timestamp());
		return base.AffectedRows > 0;
	}

	public List<ClanMember> ListMembers(long clanId)
	{
		IntPtr stmHandle = Prepare("\r\n            SELECT m.user_id, m.role_id, m.joined, m.seen, m.notes, m.notes_timestamp\r\n            FROM members m\r\n            LEFT JOIN roles r ON r.clan_id = ?1 AND r.role_id = m.role_id\r\n            WHERE m.clan_id = ?1\r\n            ORDER BY r.rank ASC, joined ASC\r\n        ");
		Database.Bind(stmHandle, 1, clanId);
		List<ClanMember> list = Pool.Get<List<ClanMember>>();
		ExecuteAndReadQueryResults<ClanMember>(stmHandle, list, delegate(IntPtr stm)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			return new ClanMember
			{
				SteamId = Database.GetColumnValue<ulong>(stm, 0),
				RoleId = Database.GetColumnValue<int>(stm, 1),
				Joined = Database.GetColumnValue<long>(stm, 2),
				LastSeen = Database.GetColumnValue<long>(stm, 3),
				Notes = Database.GetColumnValue<string>(stm, 4),
				NotesTimestamp = Database.GetColumnValue<long>(stm, 5)
			};
		});
		return list;
	}

	public bool UpdateMemberLastSeen(long clanId, ulong steamId)
	{
		Execute("UPDATE members SET seen = ? WHERE clan_id = ? AND user_id = ?", ClanUtility.Timestamp(), clanId, steamId);
		return base.AffectedRows > 0;
	}

	public bool UpdateMemberRole(long clanId, ulong steamId, int newRoleId)
	{
		Execute("UPDATE members SET role_id = ? WHERE clan_id = ? AND user_id = ?", newRoleId, clanId, steamId);
		return base.AffectedRows > 0;
	}

	public bool UpdateMemberNotes(long clanId, ulong steamId, string newNotes)
	{
		Execute("UPDATE members SET notes = ?, notes_timestamp = ? WHERE clan_id = ? AND user_id = ?", newNotes, ClanUtility.Timestamp(), clanId, steamId);
		return base.AffectedRows > 0;
	}

	public bool DeleteMember(long clanId, ulong steamId)
	{
		Execute("DELETE FROM members WHERE clan_id = ? AND user_id = ?", clanId, steamId);
		return base.AffectedRows > 0;
	}

	private void CreateRolesTable()
	{
		Execute("\r\n            CREATE TABLE IF NOT EXISTS roles\r\n            (\r\n                clan_id INTEGER NOT NULL,\r\n                role_id INTEGER NOT NULL,\r\n                rank INTEGER NOT NULL,\r\n                name TEXT NOT NULL,\r\n                can_set_motd BOOLEAN NOT NULL DEFAULT FALSE,\r\n                can_set_logo BOOLEAN NOT NULL DEFAULT FALSE,\r\n                can_invite BOOLEAN NOT NULL DEFAULT FALSE,\r\n                can_kick BOOLEAN NOT NULL DEFAULT FALSE,\r\n                can_promote BOOLEAN NOT NULL DEFAULT FALSE,\r\n                can_demote BOOLEAN NOT NULL DEFAULT FALSE,\r\n                can_set_player_notes BOOLEAN NOT NULL DEFAULT FALSE,\r\n                can_access_logs BOOLEAN NOT NULL DEFAULT FALSE,\r\n                can_access_score_events BOOLEAN NOT NULL DEFAULT FALSE,\r\n                PRIMARY KEY (clan_id, role_id),\r\n                FOREIGN KEY (clan_id) REFERENCES clans (clan_id) ON DELETE CASCADE,\r\n                UNIQUE (clan_id, name)\r\n            ) WITHOUT ROWID;\r\n        ");
	}

	public int? CreateRole(long clanId, ClanRole role)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		IntPtr stmHandle = Prepare("\r\n            WITH next AS (\r\n\t            SELECT\r\n                    COALESCE(MAX(role_id), 0) + 1 AS role_id,\r\n                    COALESCE(MAX(rank), 0) + 1 AS rank\r\n                FROM roles r\r\n                WHERE r.clan_id = ?1\r\n            )\r\n            INSERT INTO roles (clan_id, role_id, rank, name, can_set_motd, can_set_logo, can_invite, can_kick, can_promote, can_demote, can_set_player_notes, can_access_logs, can_access_score_events)\r\n            SELECT ?1, next.role_id, next.rank, ?2, ?3, ?4, ?5, ?6, ?7, ?8, ?9, ?10, ?11\r\n            FROM next\r\n            WHERE 1\r\n            RETURNING role_id\r\n        ");
		Database.Bind(stmHandle, 1, clanId);
		Database.Bind(stmHandle, 2, role.Name);
		Database.Bind(stmHandle, 3, role.CanSetMotd);
		Database.Bind(stmHandle, 4, role.CanSetLogo);
		Database.Bind(stmHandle, 5, role.CanInvite);
		Database.Bind(stmHandle, 6, role.CanKick);
		Database.Bind(stmHandle, 7, role.CanPromote);
		Database.Bind(stmHandle, 8, role.CanDemote);
		Database.Bind(stmHandle, 9, role.CanSetPlayerNotes);
		Database.Bind(stmHandle, 10, role.CanAccessLogs);
		Database.Bind(stmHandle, 11, role.CanAccessScoreEvents);
		int num = ExecuteAndReadQueryResult<int>(stmHandle);
		if (num <= 0)
		{
			return null;
		}
		return num;
	}

	public List<ClanRole> ListRoles(long clanId)
	{
		IntPtr stmHandle = Prepare("\r\n            SELECT role_id, rank, name, can_set_motd, can_set_logo, can_invite, can_kick, can_promote, can_demote, can_set_player_notes, can_access_logs, can_access_score_events\r\n            FROM roles\r\n            WHERE clan_id = ?\r\n            ORDER BY rank ASC, role_id ASC\r\n        ");
		Database.Bind(stmHandle, 1, clanId);
		List<ClanRole> list = Pool.Get<List<ClanRole>>();
		ExecuteAndReadQueryResults<ClanRole>(stmHandle, list, delegate(IntPtr stm)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			return new ClanRole
			{
				RoleId = Database.GetColumnValue<int>(stm, 0),
				Rank = Database.GetColumnValue<int>(stm, 1),
				Name = Database.GetColumnValue<string>(stm, 2),
				CanSetMotd = Database.GetColumnValue<bool>(stm, 3),
				CanSetLogo = Database.GetColumnValue<bool>(stm, 4),
				CanInvite = Database.GetColumnValue<bool>(stm, 5),
				CanKick = Database.GetColumnValue<bool>(stm, 6),
				CanPromote = Database.GetColumnValue<bool>(stm, 7),
				CanDemote = Database.GetColumnValue<bool>(stm, 8),
				CanSetPlayerNotes = Database.GetColumnValue<bool>(stm, 9),
				CanAccessLogs = Database.GetColumnValue<bool>(stm, 10),
				CanAccessScoreEvents = Database.GetColumnValue<bool>(stm, 11)
			};
		});
		return list;
	}

	public bool UpdateRole(long clanId, ClanRole role)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		IntPtr stmHandle = Prepare("\r\n            UPDATE roles\r\n            SET name = ?3, can_set_motd = ?4, can_set_logo = ?5, can_invite = ?6, can_kick = ?7, can_promote = ?8, can_demote = ?9, can_set_player_notes = ?10, can_access_logs = ?11, can_access_score_events = ?12\r\n            WHERE clan_id = ?1 AND role_id = ?2\r\n        ");
		Database.Bind(stmHandle, 1, clanId);
		Database.Bind(stmHandle, 2, role.RoleId);
		Database.Bind(stmHandle, 3, role.Name);
		Database.Bind(stmHandle, 4, role.CanSetMotd);
		Database.Bind(stmHandle, 5, role.CanSetLogo);
		Database.Bind(stmHandle, 6, role.CanInvite);
		Database.Bind(stmHandle, 7, role.CanKick);
		Database.Bind(stmHandle, 8, role.CanPromote);
		Database.Bind(stmHandle, 9, role.CanDemote);
		Database.Bind(stmHandle, 10, role.CanSetPlayerNotes);
		Database.Bind(stmHandle, 11, role.CanAccessLogs);
		Database.Bind(stmHandle, 12, role.CanAccessScoreEvents);
		ExecuteQuery(stmHandle);
		return base.AffectedRows > 0;
	}

	public bool UpdateRoleName(long clanId, int roleId, string newRoleName)
	{
		Execute("UPDATE OR IGNORE roles SET name = ?3 WHERE clan_id = ?1 AND role_id = ?2", clanId, roleId, newRoleName);
		return base.AffectedRows > 0;
	}

	public bool SwapRoleRanks(long clanId, int roleIdA, int roleIdB)
	{
		BeginTransaction();
		try
		{
			int num = Query<int, long, int>("SELECT rank FROM roles WHERE clan_id = ?1 AND role_id = ?2", clanId, roleIdA);
			int num2 = Query<int, long, int>("SELECT rank FROM roles WHERE clan_id = ?1 AND role_id = ?2", clanId, roleIdB);
			if (num <= 0 || num2 <= 0)
			{
				Rollback();
				return false;
			}
			Execute("UPDATE OR IGNORE roles SET rank = ?3 WHERE clan_id = ?1 AND role_id = ?2", clanId, roleIdA, num2);
			if (base.AffectedRows != 1)
			{
				Rollback();
				return false;
			}
			Execute("UPDATE OR IGNORE roles SET rank = ?3 WHERE clan_id = ?1 AND role_id = ?2", clanId, roleIdB, num);
			if (base.AffectedRows != 1)
			{
				Rollback();
				return false;
			}
			Commit();
			return true;
		}
		catch
		{
			Rollback();
			throw;
		}
	}

	public bool DeleteRole(long clanId, int roleId)
	{
		Execute("DELETE FROM roles WHERE clan_id = ? AND role_id = ?", clanId, roleId);
		return base.AffectedRows > 0;
	}

	private void CreateScoreEventsTable()
	{
		Execute("\r\n            CREATE TABLE IF NOT EXISTS score_events\r\n            (\r\n                clan_id INTEGER NOT NULL,\r\n                timestamp INTEGER NOT NULL,\r\n                type INTEGER NOT NULL,\r\n                score INTEGER NOT NULL,\r\n                multiplier INTEGER NOT NULL,\r\n                user_id INTEGER,\r\n                other_user_id INTEGER,\r\n                other_clan_id INTEGER,\r\n                arg1 TEXT,\r\n                arg2 TEXT,\r\n                FOREIGN KEY (clan_id) REFERENCES clans (clan_id) ON DELETE CASCADE\r\n            );\r\n        ");
		Execute("\r\n            CREATE INDEX IF NOT EXISTS score_events_ordered ON logs (clan_id, timestamp DESC);\r\n        ");
	}

	public void AppendScoreEvent(long clanId, ClanScoreEvent e)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected I4, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		long arg = ((e.Timestamp > 0) ? e.Timestamp : ClanUtility.Timestamp());
		BeginTransaction();
		Execute("INSERT INTO score_events (clan_id, timestamp, type, score, multiplier, user_id, other_user_id, other_clan_id, arg1, arg2) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", clanId, arg, (int)e.Type, e.Score, e.Multiplier, e.SteamId, e.OtherSteamId, e.OtherClanId, e.Arg1, e.Arg2);
		Execute("UPDATE clans SET score = score + ? WHERE clan_id = ?", e.Score * e.Multiplier, clanId);
		Commit();
	}

	public long ReadClanScore(long clanId)
	{
		IntPtr stmHandle = Prepare("SELECT score FROM clans WHERE clan_id = ? AND deleted IS NULL");
		Database.Bind(stmHandle, 1, clanId);
		return ExecuteAndReadQueryResult<long>(stmHandle);
	}

	public List<ClanScoreEvent> ReadScoreEvents(long clanId, int limit)
	{
		IntPtr stmHandle = Prepare("SELECT timestamp, type, score, multiplier, user_id, other_user_id, other_clan_id, arg1, arg2 FROM score_events WHERE clan_id = ? ORDER BY timestamp DESC LIMIT ?");
		Database.Bind(stmHandle, 1, clanId);
		Database.Bind(stmHandle, 2, Mathf.Clamp(limit, 10, 1000));
		List<ClanScoreEvent> list = Pool.Get<List<ClanScoreEvent>>();
		ExecuteAndReadQueryResults<ClanScoreEvent>(stmHandle, list, delegate(IntPtr stm)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			return new ClanScoreEvent
			{
				Timestamp = Database.GetColumnValue<long>(stm, 0),
				Type = (ClanScoreEventType)Database.GetColumnValue<int>(stm, 1),
				Score = Database.GetColumnValue<int>(stm, 2),
				Multiplier = Database.GetColumnValue<int>(stm, 3),
				SteamId = Database.GetColumnValue<ulong>(stm, 4),
				OtherSteamId = Database.GetColumnValue<ulong?>(stm, 5),
				OtherClanId = Database.GetColumnValue<long?>(stm, 6),
				Arg1 = Database.GetColumnValue<string>(stm, 7),
				Arg2 = Database.GetColumnValue<string>(stm, 8)
			};
		});
		return list;
	}

	public LocalClanDatabase(int version)
	{
		_version = version;
	}

	public void Open(string rootFolder)
	{
		Open(Path.Combine(rootFolder, $"clans.{_version}.db"), true);
		Execute("PRAGMA foreign_keys = ON");
		CreateClansTable();
		CreateRolesTable();
		CreateMembersTable();
		CreateInvitesTable();
		CreateLogsTable();
		CreateScoreEventsTable();
	}
}
