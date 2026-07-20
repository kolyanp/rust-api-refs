using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Carbon.Base;
using Carbon.Extensions;
using ConVar;
using Facepunch;
using Facepunch.Sqlite;
using Oxide.Core.Libraries;
using UnityEngine;

namespace Carbon.OxideRefs;

public class PermissionSql : Permission
{
	public class PermissionDatabase : Database
	{
		public IEnumerable<(string groupName, GroupData data)> QueryAllGroups()
		{
			return ((Database)this).ExecuteAndReadQueryResults<(string, GroupData)>(((Database)this).Prepare("SELECT groupName, title, rank, parentGroup FROM groups"), (Func<IntPtr, (string, GroupData)>)ReadGroupRow);
		}

		public (string groupName, GroupData data) ReadGroupRow(IntPtr stmHandle)
		{
			GroupData groupData = new GroupData();
			string columnValue = Database.GetColumnValue<string>(stmHandle, 0);
			groupData.Title = Database.GetColumnValue<string>(stmHandle, 1);
			groupData.Rank = Database.GetColumnValue<int>(stmHandle, 2);
			groupData.ParentGroup = Database.GetColumnValue<string>(stmHandle, 3);
			IEnumerable<string> enumerable = QueryGroupPermissions(columnValue);
			foreach (string item in enumerable)
			{
				groupData.Perms.Add(item);
			}
			return (groupName: columnValue, data: groupData);
		}

		public IEnumerable<string> QueryGroupPermissions(string groupName)
		{
			IntPtr intPtr = ((Database)this).Prepare("SELECT groupName, permission FROM groupsPerms WHERE groupName = ?");
			Database.Bind<string>(intPtr, 1, groupName);
			return ((Database)this).ExecuteAndReadQueryResults<string>(intPtr, (Func<IntPtr, string>)ReadStringRow);
		}

		public string ReadStringRow(IntPtr stmHandle)
		{
			return Database.GetColumnValue<string>(stmHandle, 1);
		}

		public IEnumerable<(string userId, UserData data)> QueryUsers()
		{
			return ((Database)this).ExecuteAndReadQueryResults<(string, UserData)>(((Database)this).Prepare("SELECT * FROM users"), (Func<IntPtr, (string, UserData)>)ReadUserRow);
		}

		public (string userId, UserData data) QueryUser(string id)
		{
			IntPtr intPtr = ((Database)this).Prepare("SELECT * FROM users WHERE userId = ? OR LOWER(lastSeenNickname) = LOWER(?)");
			Database.Bind<string>(intPtr, 1, id);
			Database.Bind<string>(intPtr, 2, id);
			return ((Database)this).ExecuteAndReadQueryResults<(string, UserData)>(intPtr, (Func<IntPtr, (string, UserData)>)ReadUserRow).FirstOrDefault();
		}

		public (string userId, UserData data) ReadUserRow(IntPtr stmHandle)
		{
			UserData userData = new UserData();
			string columnValue = Database.GetColumnValue<string>(stmHandle, 0);
			userData.LastSeenNickname = Database.GetColumnValue<string>(stmHandle, 1);
			userData.Language = Database.GetColumnValue<string>(stmHandle, 2);
			IEnumerable<string> enumerable = QueryUserPermissions(columnValue);
			foreach (string item in enumerable)
			{
				userData.Perms.Add(item);
			}
			IEnumerable<string> enumerable2 = QueryUserGroups(columnValue);
			foreach (string item2 in enumerable2)
			{
				userData.Groups.Add(item2);
			}
			return (userId: columnValue, data: userData);
		}

		public IEnumerable<string> QueryUserPermissions(string userId)
		{
			IntPtr intPtr = ((Database)this).Prepare("SELECT userId, permission FROM userPerms WHERE userId = ?");
			Database.Bind<string>(intPtr, 1, userId);
			return ((Database)this).ExecuteAndReadQueryResults<string>(intPtr, (Func<IntPtr, string>)ReadStringRow);
		}

		public IEnumerable<string> QueryUserGroups(string userId)
		{
			IntPtr intPtr = ((Database)this).Prepare("SELECT userId, groupName FROM userGroups WHERE userId = ?");
			Database.Bind<string>(intPtr, 1, userId);
			return ((Database)this).ExecuteAndReadQueryResults<string>(intPtr, (Func<IntPtr, string>)ReadStringRow);
		}
	}

	public PermissionDatabase db;

	private int _transactionDepth;

	public void InitializeDb()
	{
		if (db == null)
		{
			string sQLPermissionsDatabase = Switches.GetSQLPermissionsDatabase(Path.Combine(Server.filesStorageFolder, "carbon.perms.db"));
			db = new PermissionDatabase();
			((Database)db).Open(sQLPermissionsDatabase, true);
			((Database)db).Execute("PRAGMA foreign_keys = ON");
			((Database)db).Execute("PRAGMA wal_autocheckpoint = 1000");
			((Database)db).Execute("PRAGMA busy_timeout = 5000");
			((Database)db).Execute("CREATE TABLE IF NOT EXISTS users ( userId TEXT PRIMARY KEY, lastSeenNickname TEXT, language TEXT )");
			((Database)db).Execute("CREATE TABLE IF NOT EXISTS groups ( groupName TEXT COLLATE NOCASE PRIMARY KEY, title TEXT, rank INTEGER, parentGroup TEXT )");
			((Database)db).Execute("CREATE TABLE IF NOT EXISTS userPerms (userId TEXT, permission TEXT, PRIMARY KEY (userId, permission), FOREIGN KEY (userId) REFERENCES users(userId) ON DELETE CASCADE)");
			((Database)db).Execute("CREATE TABLE IF NOT EXISTS userGroups (userId TEXT, groupName TEXT COLLATE NOCASE, PRIMARY KEY (userId, groupName), FOREIGN KEY (userId) REFERENCES users(userId) ON DELETE CASCADE, FOREIGN KEY (groupName) REFERENCES groups(groupName) ON DELETE CASCADE)");
			((Database)db).Execute("CREATE TABLE IF NOT EXISTS groupsPerms (groupName TEXT COLLATE NOCASE, permission TEXT, PRIMARY KEY (groupName, permission), FOREIGN KEY (groupName) REFERENCES groups(groupName) ON DELETE CASCADE)");
		}
	}

	public override void SaveData()
	{
		PermissionDatabase permissionDatabase = db;
		if (permissionDatabase != null)
		{
			((Database)permissionDatabase).Execute("PRAGMA wal_checkpoint(FULL)");
		}
	}

	public override void SaveUsers()
	{
	}

	public override void SaveGroups()
	{
	}

	public void MigrateFromProto(Permission database)
	{
		Logger.Log("Migrating database..");
		bool ownsTransaction = BeginImmediateTransaction();
		try
		{
			int num = 0;
			foreach (KeyValuePair<BaseHookable, HashSet<string>> item in database.permset)
			{
				permset[item.Key] = item.Value;
				num += item.Value.Count;
			}
			Logger.Log($" Migrating {database.permset.Count:n0} plugins with {num:n0} perms..");
			foreach (KeyValuePair<string, GroupData> groupdatum in database.groupdata)
			{
				CreateGroup(groupdatum.Key, groupdatum.Value.Title, groupdatum.Value.Rank);
				SetGroupParent(groupdatum.Key, groupdatum.Value.ParentGroup);
				foreach (string perm in groupdatum.Value.Perms)
				{
					GrantGroupPermission(groupdatum.Key, perm, null);
					PermissionDatabase permissionDatabase = db;
					if (permissionDatabase != null)
					{
						((Database)permissionDatabase).Execute<string, string>("INSERT OR IGNORE INTO groupsPerms ( groupName, permission ) VALUES ( ?, ? )", groupdatum.Key, perm);
					}
				}
				Logger.Log($" Group {groupdatum.Key} with {groupdatum.Value.Perms.Count} perms");
			}
			Logger.Log($" Migrating {database.userdata.Count:n0} users..");
			foreach (KeyValuePair<string, UserData> userdatum in database.userdata)
			{
				userdata[userdatum.Key] = userdatum.Value;
				CommitUser(userdatum.Key, userdatum.Value);
				foreach (string group in userdatum.Value.Groups)
				{
					string text = group;
					if (!StringEx.IsLower(group))
					{
						text = group.ToLowerInvariant();
					}
					AddUserGroup(userdatum.Key, text);
					PermissionDatabase permissionDatabase2 = db;
					if (permissionDatabase2 != null)
					{
						((Database)permissionDatabase2).Execute<string, string>("INSERT OR IGNORE INTO userGroups ( userId, groupName ) VALUES ( ?, ? )", userdatum.Key, text);
					}
				}
				foreach (string perm2 in userdatum.Value.Perms)
				{
					GrantUserPermission(userdatum.Key, perm2, null);
					PermissionDatabase permissionDatabase3 = db;
					if (permissionDatabase3 != null)
					{
						((Database)permissionDatabase3).Execute<string, string>("INSERT OR IGNORE INTO userPerms ( userId, permission ) VALUES ( ?, ? )", userdatum.Key, perm2);
					}
				}
			}
			CommitTransaction(ownsTransaction);
		}
		catch
		{
			RollbackTransaction(ownsTransaction);
			throw;
		}
		Logger.Log("Successfully migrated database!");
	}

	public void MigrateToProto(Permission database)
	{
		Logger.Log("Migrating database..");
		database.groupdata = new Dictionary<string, GroupData>(database.groupdata, StringComparer.OrdinalIgnoreCase);
		database.userdata = new Dictionary<string, UserData>(database.userdata, StringComparer.OrdinalIgnoreCase);
		int num = 0;
		foreach (KeyValuePair<BaseHookable, HashSet<string>> item in permset)
		{
			database.permset[item.Key] = item.Value;
			num += item.Value.Count;
		}
		Logger.Log($" Migrating {database.permset.Count:n0} plugins with {num:n0} perms..");
		IEnumerable<(string, GroupData)> enumerable = db.QueryAllGroups();
		foreach (var item2 in enumerable)
		{
			if (!string.IsNullOrEmpty(item2.Item2.ParentGroup))
			{
				item2.Item2.ParentGroup = item2.Item2.ParentGroup.ToLowerInvariant();
			}
			database.groupdata[item2.Item1] = item2.Item2;
			Logger.Log($" Group {item2.Item1} with {item2.Item2.Perms.Count} perms");
		}
		List<(string, UserData)> list = db.QueryUsers().ToList();
		Logger.Log($" Migrating {list.Count:n0} users..");
		foreach (var item3 in list)
		{
			HashSet<string> groups = item3.Item2.Groups;
			if (groups != null && groups.Count > 0)
			{
				item3.Item2.Groups = new HashSet<string>(item3.Item2.Groups.Select((string g) => g.ToLowerInvariant()), StringComparer.OrdinalIgnoreCase);
			}
			database.userdata[item3.Item1] = item3.Item2;
		}
		Logger.Log("Successfully migrated database!");
	}

	public override bool CreateGroup(string group, string title, int rank)
	{
		if (base.CreateGroup(group, title, rank))
		{
			if (!StringEx.IsLower(group))
			{
				group = group.ToLower();
			}
			PermissionDatabase permissionDatabase = db;
			if (permissionDatabase != null)
			{
				((Database)permissionDatabase).Execute<string, string, int>("INSERT OR REPLACE INTO groups ( groupName, title, rank ) VALUES ( ?, ?, ? )", group, title, rank);
			}
			return true;
		}
		return false;
	}

	public override bool RemoveGroup(string group)
	{
		if (base.RemoveGroup(group))
		{
			if (!StringEx.IsLower(group))
			{
				group = group.ToLower();
			}
			PermissionDatabase permissionDatabase = db;
			if (permissionDatabase != null)
			{
				((Database)permissionDatabase).Execute<string>("DELETE FROM groups WHERE groupName = ?", group);
			}
			return true;
		}
		return false;
	}

	public override bool SetGroupTitle(string group, string title)
	{
		if (base.SetGroupTitle(group, title))
		{
			if (!StringEx.IsLower(group))
			{
				group = group.ToLower();
			}
			PermissionDatabase permissionDatabase = db;
			if (permissionDatabase != null)
			{
				((Database)permissionDatabase).Execute<string, string>("UPDATE groups SET title = ? WHERE groupName = ?", title, group);
			}
			return true;
		}
		return false;
	}

	public override bool SetGroupRank(string group, int rank)
	{
		if (base.SetGroupRank(group, rank))
		{
			if (!StringEx.IsLower(group))
			{
				group = group.ToLower();
			}
			PermissionDatabase permissionDatabase = db;
			if (permissionDatabase != null)
			{
				((Database)permissionDatabase).Execute<int, string>("UPDATE groups SET rank = ? WHERE groupName = ?", rank, group);
			}
			return true;
		}
		return false;
	}

	public override bool SetGroupParent(string group, string parent)
	{
		if (base.SetGroupParent(group, parent))
		{
			if (!StringEx.IsLower(group))
			{
				group = group.ToLower();
			}
			PermissionDatabase permissionDatabase = db;
			if (permissionDatabase != null)
			{
				((Database)permissionDatabase).Execute<string, string>("UPDATE groups SET parentGroup = ? WHERE groupName = ?", parent, group);
			}
			return true;
		}
		return false;
	}

	public override bool GrantGroupPermission(string name, string perm, BaseHookable owner)
	{
		if (!base.GrantGroupPermission(name, perm, owner))
		{
			return false;
		}
		if (!StringEx.IsLower(name))
		{
			name = name.ToLower();
		}
		if (!StringEx.IsLower(perm))
		{
			perm = perm.ToLower();
		}
		if (perm.Length > 0 && perm[perm.Length - 1] == '*')
		{
			SqlInsertWildcardGroupPerms(name, perm, owner);
			return true;
		}
		PermissionDatabase permissionDatabase = db;
		if (permissionDatabase != null)
		{
			((Database)permissionDatabase).Execute<string, string>("INSERT OR IGNORE INTO groupsPerms ( groupName, permission ) VALUES ( ?, ? )", name, perm);
		}
		return true;
	}

	private void SqlInsertWildcardGroupPerms(string name, string perm, BaseHookable owner)
	{
		if (db == null)
		{
			return;
		}
		int num = ((perm.Length != 1) ? (perm.Length - 1) : 0);
		bool ownsTransaction = BeginImmediateTransaction();
		try
		{
			HashSet<string> value2;
			if (owner == null)
			{
				foreach (KeyValuePair<BaseHookable, HashSet<string>> item in permset)
				{
					HashSet<string> value = item.Value;
					if (value.Count == 0)
					{
						continue;
					}
					foreach (string item2 in value)
					{
						if (num <= 0 || Permission.StartsWithPrefix(item2, perm, num))
						{
							((Database)db).Execute<string, string>("INSERT OR IGNORE INTO groupsPerms ( groupName, permission ) VALUES ( ?, ? )", name, item2);
						}
					}
				}
			}
			else if (permset.TryGetValue(owner, out value2) && value2.Count > 0)
			{
				foreach (string item3 in value2)
				{
					if (num <= 0 || Permission.StartsWithPrefix(item3, perm, num))
					{
						((Database)db).Execute<string, string>("INSERT OR IGNORE INTO groupsPerms ( groupName, permission ) VALUES ( ?, ? )", name, item3);
					}
				}
			}
			CommitTransaction(ownsTransaction);
		}
		catch
		{
			RollbackTransaction(ownsTransaction);
			throw;
		}
	}

	public override bool RevokeGroupPermission(string name, string perm)
	{
		if (!base.RevokeGroupPermission(name, perm))
		{
			return false;
		}
		if (!StringEx.IsLower(name))
		{
			name = name.ToLower();
		}
		if (!StringEx.IsLower(perm))
		{
			perm = perm.ToLower();
		}
		if (perm.Length > 0 && perm[perm.Length - 1] == '*')
		{
			if (perm.Length == 1)
			{
				PermissionDatabase permissionDatabase = db;
				if (permissionDatabase != null)
				{
					((Database)permissionDatabase).Execute<string>("DELETE FROM groupsPerms WHERE groupName = ?", name);
				}
				return true;
			}
			PermissionDatabase permissionDatabase2 = db;
			if (permissionDatabase2 != null)
			{
				((Database)permissionDatabase2).Execute<string, string>("DELETE FROM groupsPerms WHERE groupName = ? AND permission LIKE ?", name, perm.Substring(0, perm.Length - 1) + "%");
			}
			return true;
		}
		PermissionDatabase permissionDatabase3 = db;
		if (permissionDatabase3 != null)
		{
			((Database)permissionDatabase3).Execute<string, string>("DELETE FROM groupsPerms WHERE groupName = ? AND permission = ?", name, perm);
		}
		return true;
	}

	public override UserData GetUserData(string id, bool addIfNotExisting = false)
	{
		if (!base.UserExists(id))
		{
			var (text, userData) = db.QueryUser(id);
			if (!string.IsNullOrEmpty(text) && userData != null)
			{
				userdata.Add(text, userData);
			}
			else if (addIfNotExisting && id.IsSteamId())
			{
				UserData userData2 = new UserData();
				userdata[id] = userData2;
				CommitUser(id, userData2);
			}
		}
		return base.GetUserData(id, addIfNotExisting);
	}

	public override bool UserExists(string id)
	{
		if (base.UserExists(id))
		{
			return true;
		}
		if (string.IsNullOrEmpty(id))
		{
			return false;
		}
		var (text, userData) = db.QueryUser(id);
		if (!string.IsNullOrEmpty(text) && userData != null)
		{
			userdata[text] = userData;
			return true;
		}
		return false;
	}

	public override bool UserExists(string id, out UserData data)
	{
		if (userdata.TryGetValue(id, out data))
		{
			return true;
		}
		if (string.IsNullOrEmpty(id))
		{
			return false;
		}
		var (text, userData) = db.QueryUser(id);
		if (!string.IsNullOrEmpty(text) && userData != null)
		{
			userdata[text] = userData;
			data = userData;
			return true;
		}
		return false;
	}

	public override void CommitUser(string userId, UserData data)
	{
		if (db != null)
		{
			((Database)db).Execute<string, string, string>("INSERT INTO users ( userId, lastSeenNickname, language ) VALUES ( ?, ?, ? ) ON CONFLICT(userId) DO UPDATE SET lastSeenNickname = excluded.lastSeenNickname, language = excluded.language", userId, data.LastSeenNickname, data.Language);
		}
	}

	public override bool GrantUserPermission(string id, string perm, BaseHookable owner)
	{
		if (!PermissionExists(perm, owner))
		{
			return false;
		}
		UserData userData = GetUserData(id);
		if (!StringEx.IsLower(perm))
		{
			perm = perm.ToLower();
		}
		if (perm.Length > 0 && perm[perm.Length - 1] == '*')
		{
			return SqlGrantWildcardUser(userData.Perms, perm, owner, id);
		}
		if (!userData.Perms.Add(perm))
		{
			return false;
		}
		PermissionDatabase permissionDatabase = db;
		if (permissionDatabase != null)
		{
			((Database)permissionDatabase).Execute<string, string>("INSERT OR IGNORE INTO userPerms ( userId, permission ) VALUES ( ?, ? )", id, perm);
		}
		HookCaller.CallStaticHook(4054877424u, id, perm);
		return true;
	}

	private bool SqlGrantWildcardUser(HashSet<string> target, string perm, BaseHookable owner, string id)
	{
		int num = ((perm.Length != 1) ? (perm.Length - 1) : 0);
		PooledList<string> val = Pool.Get<PooledList<string>>();
		try
		{
			if (owner == null)
			{
				foreach (KeyValuePair<BaseHookable, HashSet<string>> item in permset)
				{
					HashSet<string> value = item.Value;
					if (value.Count == 0)
					{
						continue;
					}
					foreach (string item2 in value)
					{
						if ((num <= 0 || Permission.StartsWithPrefix(item2, perm, num)) && target.Add(item2))
						{
							((List<string>)(object)val).Add(item2);
						}
					}
				}
			}
			else
			{
				if (!permset.TryGetValue(owner, out var value2) || value2.Count == 0)
				{
					return false;
				}
				foreach (string item3 in value2)
				{
					if ((num <= 0 || Permission.StartsWithPrefix(item3, perm, num)) && target.Add(item3))
					{
						((List<string>)(object)val).Add(item3);
					}
				}
			}
			if (((List<string>)(object)val).Count == 0)
			{
				return false;
			}
			if (db != null)
			{
				bool ownsTransaction = BeginImmediateTransaction();
				try
				{
					for (int i = 0; i < ((List<string>)(object)val).Count; i++)
					{
						((Database)db).Execute<string, string>("INSERT OR IGNORE INTO userPerms ( userId, permission ) VALUES ( ?, ? )", id, ((List<string>)(object)val)[i]);
					}
					CommitTransaction(ownsTransaction);
				}
				catch
				{
					RollbackTransaction(ownsTransaction);
					throw;
				}
			}
			for (int j = 0; j < ((List<string>)(object)val).Count; j++)
			{
				HookCaller.CallStaticHook(4054877424u, id, ((List<string>)(object)val)[j]);
			}
			return true;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override bool RevokeUserPermission(string id, string perm)
	{
		if (string.IsNullOrEmpty(perm))
		{
			return false;
		}
		UserData userData = GetUserData(id);
		if (userData.Perms.Count == 0)
		{
			return false;
		}
		if (!StringEx.IsLower(perm))
		{
			perm = perm.ToLower();
		}
		if (perm.Length > 0 && perm[perm.Length - 1] == '*')
		{
			if (perm.Length == 1)
			{
				PooledList<string> val = Pool.Get<PooledList<string>>();
				try
				{
					((List<string>)(object)val).AddRange((IEnumerable<string>)userData.Perms);
					PermissionDatabase permissionDatabase = db;
					if (permissionDatabase != null)
					{
						((Database)permissionDatabase).Execute<string>("DELETE FROM userPerms WHERE userId = ?", id);
					}
					userData.Perms.Clear();
					for (int i = 0; i < ((List<string>)(object)val).Count; i++)
					{
						HookCaller.CallStaticHook(1879829838u, id, ((List<string>)(object)val)[i]);
					}
					return true;
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			return SqlRevokeWildcardUser(userData.Perms, perm, id);
		}
		if (!userData.Perms.Remove(perm))
		{
			return false;
		}
		PermissionDatabase permissionDatabase2 = db;
		if (permissionDatabase2 != null)
		{
			((Database)permissionDatabase2).Execute<string, string>("DELETE FROM userPerms WHERE userId = ? AND permission = ?", id, perm);
		}
		HookCaller.CallStaticHook(1879829838u, id, perm);
		return true;
	}

	private bool SqlRevokeWildcardUser(HashSet<string> perms, string perm, string id)
	{
		int prefixLen = perm.Length - 1;
		PooledList<string> val = Pool.Get<PooledList<string>>();
		try
		{
			foreach (string perm2 in perms)
			{
				if (Permission.StartsWithPrefix(perm2, perm, prefixLen))
				{
					((List<string>)(object)val).Add(perm2);
				}
			}
			if (((List<string>)(object)val).Count == 0)
			{
				return false;
			}
			if (db != null)
			{
				bool ownsTransaction = BeginImmediateTransaction();
				try
				{
					for (int i = 0; i < ((List<string>)(object)val).Count; i++)
					{
						((Database)db).Execute<string, string>("DELETE FROM userPerms WHERE userId = ? AND permission = ?", id, ((List<string>)(object)val)[i]);
					}
					CommitTransaction(ownsTransaction);
				}
				catch
				{
					RollbackTransaction(ownsTransaction);
					throw;
				}
			}
			for (int j = 0; j < ((List<string>)(object)val).Count; j++)
			{
				string text = ((List<string>)(object)val)[j];
				if (perms.Remove(text))
				{
					HookCaller.CallStaticHook(1879829838u, id, text);
				}
			}
			return true;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override void AddUserGroup(string id, string name, bool addIfNotExisting = false)
	{
		if (!StringEx.IsLower(name))
		{
			name = name.ToLower();
		}
		if (GroupExists(name) && GetUserData(id, addIfNotExisting).Groups.Add(name))
		{
			PermissionDatabase permissionDatabase = db;
			if (permissionDatabase != null)
			{
				((Database)permissionDatabase).Execute<string, string>("INSERT OR IGNORE INTO userGroups ( userId, groupName ) VALUES ( ?, ? )", id, name);
			}
			HookCaller.CallStaticHook(3116013984u, id, name);
		}
	}

	public override void RemoveUserGroup(string id, string name)
	{
		if (!StringEx.IsLower(name))
		{
			name = name.ToLower();
		}
		if (name.Length == 1 && name[0] == '*')
		{
			UserData userData = GetUserData(id);
			if (userData.Groups.Count == 0)
			{
				return;
			}
			PooledList<string> val = Pool.Get<PooledList<string>>();
			try
			{
				((List<string>)(object)val).AddRange((IEnumerable<string>)userData.Groups);
				userData.Groups.Clear();
				PermissionDatabase permissionDatabase = db;
				if (permissionDatabase != null)
				{
					((Database)permissionDatabase).Execute<string>("DELETE FROM userGroups WHERE userId = ?", id);
				}
				for (int i = 0; i < ((List<string>)(object)val).Count; i++)
				{
					HookCaller.CallStaticHook(1018697706u, id, ((List<string>)(object)val)[i]);
				}
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if (!groupdata.ContainsKey(name))
		{
			return;
		}
		UserData userData2 = GetUserData(id);
		if (userData2.Groups.Count != 0 && userData2.Groups.Remove(name))
		{
			PermissionDatabase permissionDatabase2 = db;
			if (permissionDatabase2 != null)
			{
				((Database)permissionDatabase2).Execute<string, string>("DELETE FROM userGroups WHERE userId = ? AND groupName = ?", id, name);
			}
			HookCaller.CallStaticHook(1018697706u, id, name);
		}
	}

	public override KeyValuePair<string, UserData> FindUser(string id)
	{
		GetUserData(id);
		return base.FindUser(id);
	}

	private bool BeginImmediateTransaction()
	{
		if (db == null)
		{
			return false;
		}
		if (_transactionDepth == 0)
		{
			_transactionDepth = 1;
			((Database)db).Execute("BEGIN IMMEDIATE TRANSACTION");
			return true;
		}
		return false;
	}

	private void CommitTransaction(bool ownsTransaction)
	{
		if (db != null && ownsTransaction)
		{
			_transactionDepth = 0;
			((Database)db).Execute("COMMIT");
		}
	}

	private void RollbackTransaction(bool ownsTransaction)
	{
		if (db != null && ownsTransaction)
		{
			_transactionDepth = 0;
			((Database)db).Execute("ROLLBACK");
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		((Database)db).Close();
		db = null;
	}

	public override void LoadFromDatafile()
	{
		InitializeDb();
		LoadGroups();
		string playerDefaultGroup = Community.Runtime.Config.Permissions.PlayerDefaultGroup;
		string adminDefaultGroup = Community.Runtime.Config.Permissions.AdminDefaultGroup;
		string moderatorDefaultGroup = Community.Runtime.Config.Permissions.ModeratorDefaultGroup;
		if (!string.IsNullOrEmpty(playerDefaultGroup) && !GroupExists(playerDefaultGroup))
		{
			CreateGroup(playerDefaultGroup, playerDefaultGroup.ToCamelCase(), 0);
		}
		if (!string.IsNullOrEmpty(adminDefaultGroup) && !GroupExists(adminDefaultGroup))
		{
			CreateGroup(adminDefaultGroup, adminDefaultGroup.ToCamelCase(), 1);
		}
		if (!string.IsNullOrEmpty(moderatorDefaultGroup) && !GroupExists(moderatorDefaultGroup))
		{
			CreateGroup(moderatorDefaultGroup, moderatorDefaultGroup.ToCamelCase(), 1);
		}
		if (Community.Runtime.Config.Permissions.SqlPermissionUserPreload)
		{
			IEnumerable<(string, UserData)> enumerable = db.QueryUsers();
			foreach (var item in enumerable)
			{
				userdata[item.Item1] = item.Item2;
			}
		}
		base.IsLoaded = true;
	}

	public void LoadGroups()
	{
		IEnumerable<(string, GroupData)> enumerable = db.QueryAllGroups();
		foreach (var item in enumerable)
		{
			groupdata[item.Item1] = item.Item2;
		}
	}
}
