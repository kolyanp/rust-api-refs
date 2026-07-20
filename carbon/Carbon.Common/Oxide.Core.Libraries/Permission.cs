using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Carbon;
using Carbon.Base;
using Carbon.Core;
using Carbon.Extensions;
using Facepunch;
using Network;
using Oxide.Game.Rust.Libraries.Covalence;
using Oxide.Plugins;
using UnityEngine;

namespace Oxide.Core.Libraries;

public class Permission : Library
{
	public enum SerializationMode
	{
		Storeless = -1,
		Protobuf,
		SQL
	}

	public static readonly char[] Star = new char[1] { '*' };

	public static readonly string StarStr = "*";

	public Dictionary<string, UserData> userdata = new Dictionary<string, UserData>(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, GroupData> groupdata = new Dictionary<string, GroupData>(StringComparer.OrdinalIgnoreCase);

	public readonly Dictionary<BaseHookable, HashSet<string>> permset;

	private Func<string, bool> validate;

	internal static readonly UserData _blankUser = new UserData();

	private static FieldInfo _iPlayerFieldCache;

	private const int ParentGroupDepthLimit = 32;

	public override bool IsGlobal => false;

	public bool IsLoaded { get; set; }

	public static FieldInfo iPlayerField => _iPlayerFieldCache ?? (_iPlayerFieldCache = typeof(BasePlayer).GetField("IPlayer", BindingFlags.Instance | BindingFlags.Public));

	public Permission()
	{
		permset = new Dictionary<BaseHookable, HashSet<string>>();
		RegisterValidate((string value) => ulong.TryParse(value, out var result) && ((result == 0L) ? 1 : ((int)Math.Floor(Math.Log10(result) + 1.0))) >= 17);
		LoadFromDatafile();
		CleanUp();
	}

	public virtual void LoadFromDatafile()
	{
		Utility.DatafileToProto<Dictionary<string, UserData>>("oxide.users");
		Utility.DatafileToProto<Dictionary<string, GroupData>>("oxide.groups");
		bool flag = false;
		bool flag2 = false;
		userdata = ProtoStorage.Load<Dictionary<string, UserData>>(new string[1] { "oxide.users" }) ?? new Dictionary<string, UserData>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, UserData> dictionary = new Dictionary<string, UserData>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, UserData> userdatum in userdata)
		{
			UserData value = userdatum.Value;
			hashSet2.Clear();
			hashSet.Clear();
			foreach (string perm in value.Perms)
			{
				hashSet2.Add(perm);
			}
			value.Perms = new HashSet<string>(hashSet2, StringComparer.OrdinalIgnoreCase);
			foreach (string group in value.Groups)
			{
				hashSet.Add(group);
			}
			value.Groups = new HashSet<string>(hashSet, StringComparer.OrdinalIgnoreCase);
			if (dictionary.TryGetValue(userdatum.Key, out var value2))
			{
				value2.Perms.UnionWith(value.Perms);
				value2.Groups.UnionWith(value.Groups);
				flag = true;
			}
			else
			{
				dictionary.Add(userdatum.Key, value);
			}
		}
		hashSet2.Clear();
		hashSet.Clear();
		userdata.Clear();
		userdata = null;
		userdata = dictionary;
		CovalencePlugin.PlayerManager.RefreshDatabase(userdata);
		groupdata = ProtoStorage.Load<Dictionary<string, GroupData>>(new string[1] { "oxide.groups" }) ?? new Dictionary<string, GroupData>();
		Dictionary<string, GroupData> dictionary2 = new Dictionary<string, GroupData>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet3 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, GroupData> groupdatum in groupdata)
		{
			GroupData value3 = groupdatum.Value;
			hashSet3.Clear();
			foreach (string perm2 in value3.Perms)
			{
				hashSet3.Add(perm2);
			}
			value3.Perms = new HashSet<string>(hashSet3, StringComparer.OrdinalIgnoreCase);
			if (dictionary2.ContainsKey(groupdatum.Key))
			{
				dictionary2[groupdatum.Key].Perms.UnionWith(value3.Perms);
				flag2 = true;
			}
			else
			{
				dictionary2.Add(groupdatum.Key, value3);
			}
		}
		foreach (KeyValuePair<string, GroupData> groupdatum2 in groupdata)
		{
			if (!string.IsNullOrEmpty(groupdatum2.Value.ParentGroup) && HasCircularParent(groupdatum2.Key, groupdatum2.Value.ParentGroup))
			{
				Logger.Warn("Detected circular parent group for '{keyValuePair.Key}'! Removing parent '{keyValuePair.Value.ParentGroup}'");
				groupdatum2.Value.ParentGroup = null;
				flag2 = true;
			}
		}
		hashSet3.Clear();
		groupdata.Clear();
		groupdata = null;
		groupdata = dictionary2;
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
		IsLoaded = true;
		if (flag)
		{
			SaveUsers();
		}
		if (flag2)
		{
			SaveGroups();
		}
	}

	public virtual void Export(string prefix = "auth")
	{
		if (IsLoaded)
		{
			Interface.Oxide.DataFileSystem.WriteObject(prefix + ".groups", groupdata);
			Interface.Oxide.DataFileSystem.WriteObject(prefix + ".users", userdata);
		}
	}

	public virtual void SaveData()
	{
		SaveUsers();
		SaveGroups();
	}

	public virtual void SaveUsers()
	{
		ProtoStorage.Save(userdata, "oxide.users");
	}

	public virtual void SaveGroups()
	{
		ProtoStorage.Save(groupdata, "oxide.groups");
	}

	public virtual void RegisterValidate(Func<string, bool> val)
	{
		validate = val;
	}

	public virtual void CleanUp()
	{
		if (!IsLoaded || validate == null)
		{
			return;
		}
		PooledList<string> val = Pool.Get<PooledList<string>>();
		try
		{
			foreach (string key in userdata.Keys)
			{
				if (!validate(key))
				{
					((List<string>)(object)val).Add(key);
				}
			}
			if (((List<string>)(object)val).Count != 0)
			{
				for (int i = 0; i < ((List<string>)(object)val).Count; i++)
				{
					userdata.Remove(((List<string>)(object)val)[i]);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual void MigrateGroup(string oldGroup, string newGroup)
	{
		if (IsLoaded && GroupExists(oldGroup))
		{
			string fileDataPath = ProtoStorage.GetFileDataPath("oxide.groups.data");
			File.Copy(fileDataPath, fileDataPath + ".old", overwrite: true);
			string[] groupPermissions = GetGroupPermissions(oldGroup);
			foreach (string perm in groupPermissions)
			{
				GrantGroupPermission(newGroup, perm, null);
			}
			if (GetUsersInGroup(oldGroup).Length == 0)
			{
				RemoveGroup(oldGroup);
			}
		}
	}

	public virtual void RegisterPermission(string name, BaseHookable owner)
	{
		if (string.IsNullOrEmpty(name))
		{
			return;
		}
		if (!StringEx.IsLower(name))
		{
			name = name.ToLower();
		}
		if (PermissionExists(name, owner))
		{
			return;
		}
		if (PermissionExists(name))
		{
			Logger.Warn("Trying to register permission '" + name + "' but already used by another plugin. (Requestee plugin '" + owner.Name + "')");
		}
		else
		{
			if (!permset.TryGetValue(owner, out var value))
			{
				value = new HashSet<string>();
				permset.Add(owner, value);
			}
			value.Add(name);
			HookCaller.CallStaticHook(4257240972u, name, owner);
		}
	}

	public virtual void UnregisterPermissions(BaseHookable owner)
	{
		if (owner != null && permset.TryGetValue(owner, out var value))
		{
			value.Clear();
			permset.Remove(owner);
			HookCaller.CallStaticHook(2952085131u, owner);
		}
	}

	public virtual bool PermissionExists(string name, BaseHookable owner = null)
	{
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		if (!StringEx.IsLower(name))
		{
			name = name.ToLower();
		}
		bool flag = name[name.Length - 1] == '*';
		bool flag2 = flag && name.Length == 1;
		int prefixLen = (flag ? (name.Length - 1) : 0);
		if (owner == null)
		{
			if (permset.Count == 0)
			{
				return false;
			}
			if (flag)
			{
				if (flag2)
				{
					foreach (KeyValuePair<BaseHookable, HashSet<string>> item in permset)
					{
						if (item.Value.Count > 0)
						{
							return true;
						}
					}
					return false;
				}
				foreach (KeyValuePair<BaseHookable, HashSet<string>> item2 in permset)
				{
					HashSet<string> value = item2.Value;
					if (value.Count == 0)
					{
						continue;
					}
					foreach (string item3 in value)
					{
						if (StartsWithPrefix(item3, name, prefixLen))
						{
							return true;
						}
					}
				}
				return false;
			}
			foreach (KeyValuePair<BaseHookable, HashSet<string>> item4 in permset)
			{
				if (item4.Value.Contains(name))
				{
					return true;
				}
			}
			return false;
		}
		if (!permset.TryGetValue(owner, out var value2) || value2.Count == 0)
		{
			return false;
		}
		if (flag)
		{
			if (flag2)
			{
				return true;
			}
			foreach (string item5 in value2)
			{
				if (StartsWithPrefix(item5, name, prefixLen))
				{
					return true;
				}
			}
			return false;
		}
		return value2.Contains(name);
	}

	public virtual bool UserIdValid(string id)
	{
		if (validate != null)
		{
			return validate(id);
		}
		return true;
	}

	public virtual bool UserExists(string id)
	{
		if (userdata.ContainsKey(id))
		{
			return true;
		}
		foreach (KeyValuePair<string, UserData> userdatum in userdata)
		{
			if (userdatum.Value.LastSeenNickname.Equals(id, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool UserExists(string id, out UserData data)
	{
		return userdata.TryGetValue(id, out data);
	}

	public virtual UserData GetUserData(string id, bool addIfNotExisting = false)
	{
		if (!userdata.TryGetValue(id, out var value))
		{
			if (!addIfNotExisting)
			{
				return _blankUser;
			}
			userdata.Add(id, value = new UserData());
		}
		return value;
	}

	public virtual GroupData GetGroupData(string id)
	{
		if (groupdata.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual KeyValuePair<string, UserData> FindUser(string id)
	{
		if (id.IsSteamId())
		{
			return new KeyValuePair<string, UserData>(id, GetUserData(id, addIfNotExisting: true));
		}
		PooledList<KeyValuePair<string, UserData>> val = Pool.Get<PooledList<KeyValuePair<string, UserData>>>();
		try
		{
			((List<KeyValuePair<string, UserData>>)(object)val).Clear();
			foreach (KeyValuePair<string, UserData> userdatum in userdata)
			{
				if (userdatum.Value != null)
				{
					if (userdatum.Key == id)
					{
						return new KeyValuePair<string, UserData>(userdatum.Key, userdatum.Value);
					}
					if (!string.IsNullOrEmpty(userdatum.Value.LastSeenNickname) && userdatum.Value.LastSeenNickname.IndexOf(id, StringComparison.InvariantCultureIgnoreCase) >= 0)
					{
						((List<KeyValuePair<string, UserData>>)(object)val).Add(userdatum);
					}
				}
			}
			if (((List<KeyValuePair<string, UserData>>)(object)val).Count >= 1)
			{
				if (((List<KeyValuePair<string, UserData>>)(object)val).Count > 1)
				{
					Logger.Warn($"Found {((List<KeyValuePair<string, UserData>>)(object)val).Count} users with '{id}' in nickname:");
					foreach (KeyValuePair<string, UserData> item in (List<KeyValuePair<string, UserData>>)(object)val)
					{
						Logger.Warn("  - " + item.Key + " (" + item.Value.LastSeenNickname + ")");
					}
					Logger.Warn("Using first (" + ((List<KeyValuePair<string, UserData>>)(object)val)[0].Key + ") as an result...");
				}
				return new KeyValuePair<string, UserData>(((List<KeyValuePair<string, UserData>>)(object)val)[0].Key, ((List<KeyValuePair<string, UserData>>)(object)val)[0].Value);
			}
			return default(KeyValuePair<string, UserData>);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual void RefreshUser(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		Config.PermissionsConfig permissions = Community.Runtime.Config.Permissions;
		string userIDString = player.UserIDString;
		UserData userData = GetUserData(userIDString, addIfNotExisting: true);
		userData.Player = player.AsIPlayer();
		userData.LastSeenNickname = player.displayName;
		if (((BaseNetworkable)player).net != null && ((BaseNetworkable)player).net.connection != null && ((BaseNetworkable)player).net.connection.info != null)
		{
			userData.Language = ((BaseNetworkable)player).net.connection.info.GetString("global.language", Community.Runtime.Config.Language);
		}
		else
		{
			userData.Language = Community.Runtime.Config.Language;
		}
		CommitUser(userIDString, userData);
		string playerDefaultGroup = permissions.PlayerDefaultGroup;
		string adminDefaultGroup = permissions.AdminDefaultGroup;
		string moderatorDefaultGroup = permissions.ModeratorDefaultGroup;
		if (permissions.AutoGrantPlayerGroup && !string.IsNullOrEmpty(playerDefaultGroup))
		{
			AddUserGroup(userIDString, playerDefaultGroup);
		}
		if (permissions.AutoGrantAdminGroup && !string.IsNullOrEmpty(adminDefaultGroup))
		{
			Networkable net = ((BaseNetworkable)player).net;
			if (net != null)
			{
				Connection connection = net.connection;
				if (connection != null && connection.authLevel == 2)
				{
					AddUserGroup(userIDString, adminDefaultGroup);
					goto IL_013b;
				}
			}
			if (UserHasGroup(userIDString, adminDefaultGroup))
			{
				RemoveUserGroup(userIDString, adminDefaultGroup);
			}
		}
		goto IL_013b;
		IL_018f:
		if (!string.IsNullOrEmpty(adminDefaultGroup))
		{
			Networkable net = ((BaseNetworkable)player).net;
			if (net != null)
			{
				Connection connection = net.connection;
				if (connection != null && connection.authLevel == 3)
				{
					AddUserGroup(userIDString, adminDefaultGroup);
				}
			}
		}
		object value = iPlayerField.GetValue(player);
		RustPlayer rustPlayer;
		if (value == null)
		{
			rustPlayer = new RustPlayer(player);
			iPlayerField.SetValue(player, rustPlayer);
		}
		else
		{
			rustPlayer = (RustPlayer)value;
		}
		rustPlayer.Object = player;
		rustPlayer.Name = player.displayName.Sanitize();
		return;
		IL_013b:
		if (permissions.AutoGrantModeratorGroup && !string.IsNullOrEmpty(moderatorDefaultGroup))
		{
			Networkable net = ((BaseNetworkable)player).net;
			if (net != null)
			{
				Connection connection = net.connection;
				if (connection != null && connection.authLevel == 1)
				{
					AddUserGroup(userIDString, moderatorDefaultGroup);
					goto IL_018f;
				}
			}
			if (UserHasGroup(userIDString, moderatorDefaultGroup))
			{
				RemoveUserGroup(userIDString, moderatorDefaultGroup);
			}
		}
		goto IL_018f;
	}

	public virtual void UpdateNickname(string id, string nickname)
	{
		if (UserExists(id))
		{
			UserData userData = GetUserData(id);
			string lastSeenNickname = userData.LastSeenNickname;
			userData.LastSeenNickname = nickname.Sanitize();
			CommitUser(id, userData);
			HookCaller.CallStaticHook(4255507790u, id, lastSeenNickname, userData.LastSeenNickname);
		}
	}

	public virtual void CommitUser(string userId, UserData data)
	{
	}

	public virtual bool UserHasAnyGroup(string id)
	{
		if (UserExists(id))
		{
			return GetUserData(id).Groups.Count > 0;
		}
		return false;
	}

	public virtual bool GroupsHavePermission(HashSet<string> groups, string perm)
	{
		if (groups != null && groups.Count != 0 && !string.IsNullOrEmpty(perm))
		{
			foreach (string group in groups)
			{
				if (GroupHasPermission(group, perm))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool GroupHasPermission(string name, string perm)
	{
		if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(perm))
		{
			return false;
		}
		string text = name;
		for (int i = 0; i < 32; i++)
		{
			if (!groupdata.TryGetValue(text, out var value))
			{
				return false;
			}
			if (value.Perms.Count > 0 && value.Perms.Contains(perm))
			{
				return true;
			}
			string parentGroup = value.ParentGroup;
			if (string.IsNullOrEmpty(parentGroup) || string.Equals(parentGroup, text, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			text = parentGroup;
		}
		return false;
	}

	public virtual bool UserHasPermission(string id, string perm)
	{
		if (string.IsNullOrEmpty(perm) || string.IsNullOrEmpty(id) || perm.Equals(StarStr))
		{
			return false;
		}
		if (id.Equals("server_console"))
		{
			return true;
		}
		UserData userData = GetUserData(id);
		if (userData.Perms.Count > 0 && userData.Perms.Contains(perm))
		{
			return true;
		}
		HashSet<string> groups = userData.Groups;
		if (groups.Count > 0)
		{
			foreach (string item in groups)
			{
				if (GroupHasPermission(item, perm))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual string[] GetUserGroups(string id)
	{
		HashSet<string> groups = GetUserData(id).Groups;
		if (groups.Count != 0)
		{
			return groups.ToArray();
		}
		return Array.Empty<string>();
	}

	public virtual string[] GetUserPermissions(string id)
	{
		UserData userData = GetUserData(id);
		HashSet<string> perms = userData.Perms;
		HashSet<string> groups = userData.Groups;
		if (perms.Count == 0 && groups.Count == 0)
		{
			return Array.Empty<string>();
		}
		PooledHashSet<string> val = Pool.Get<PooledHashSet<string>>();
		try
		{
			foreach (string item in perms)
			{
				((HashSet<string>)(object)val).Add(item);
			}
			foreach (string item2 in groups)
			{
				CollectGroupPermissions(item2, (HashSet<string>)(object)val);
			}
			if (((HashSet<string>)(object)val).Count == 0)
			{
				return Array.Empty<string>();
			}
			string[] array = new string[((HashSet<string>)(object)val).Count];
			int num = 0;
			foreach (string item3 in (HashSet<string>)(object)val)
			{
				array[num++] = item3;
			}
			return array;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual string[] GetGroupPermissions(string name, bool parents = false)
	{
		if (string.IsNullOrEmpty(name) || !groupdata.TryGetValue(name, out var value))
		{
			return Array.Empty<string>();
		}
		PooledHashSet<string> val = Pool.Get<PooledHashSet<string>>();
		try
		{
			GroupData value2 = value;
			string b = name;
			for (int i = 0; i < 32; i++)
			{
				foreach (string perm in value2.Perms)
				{
					((HashSet<string>)(object)val).Add(perm);
				}
				string parentGroup = value2.ParentGroup;
				if (string.IsNullOrEmpty(parentGroup) || string.Equals(parentGroup, b, StringComparison.OrdinalIgnoreCase) || !groupdata.TryGetValue(parentGroup, out value2))
				{
					break;
				}
				b = parentGroup;
			}
			if (((HashSet<string>)(object)val).Count == 0)
			{
				return Array.Empty<string>();
			}
			string[] array = new string[((HashSet<string>)(object)val).Count];
			int num = 0;
			foreach (string item in (HashSet<string>)(object)val)
			{
				array[num++] = item;
			}
			return array;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual string[] GetPermissions()
	{
		if (permset.Count == 0)
		{
			return Array.Empty<string>();
		}
		PooledHashSet<string> val = Pool.Get<PooledHashSet<string>>();
		try
		{
			foreach (KeyValuePair<BaseHookable, HashSet<string>> item in permset)
			{
				foreach (string item2 in item.Value)
				{
					((HashSet<string>)(object)val).Add(item2);
				}
			}
			if (((HashSet<string>)(object)val).Count == 0)
			{
				return Array.Empty<string>();
			}
			string[] array = new string[((HashSet<string>)(object)val).Count];
			int num = 0;
			foreach (string item3 in (HashSet<string>)(object)val)
			{
				array[num++] = item3;
			}
			return array;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual string[] GetPermissions(BaseHookable hookable)
	{
		if (hookable == null || !permset.TryGetValue(hookable, out var value) || value.Count == 0)
		{
			return Array.Empty<string>();
		}
		string[] array = new string[value.Count];
		int num = 0;
		foreach (string item in value)
		{
			array[num++] = item;
		}
		return array;
	}

	public virtual string[] GetPermissionUsers(string perm)
	{
		if (string.IsNullOrEmpty(perm))
		{
			return Array.Empty<string>();
		}
		if (!StringEx.IsLower(perm))
		{
			perm = perm.ToLower();
		}
		PooledHashSet<string> val = Pool.Get<PooledHashSet<string>>();
		try
		{
			foreach (KeyValuePair<string, UserData> userdatum in userdata)
			{
				UserData value = userdatum.Value;
				if (value.Perms.Count > 0 && value.Perms.Contains(perm))
				{
					((HashSet<string>)(object)val).Add(userdatum.Key + "(" + value.LastSeenNickname + ")");
				}
			}
			if (((HashSet<string>)(object)val).Count == 0)
			{
				return Array.Empty<string>();
			}
			string[] array = new string[((HashSet<string>)(object)val).Count];
			int num = 0;
			foreach (string item in (HashSet<string>)(object)val)
			{
				array[num++] = item;
			}
			return array;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual string[] GetPermissionGroups(string perm)
	{
		if (string.IsNullOrEmpty(perm))
		{
			return Array.Empty<string>();
		}
		if (!StringEx.IsLower(perm))
		{
			perm = perm.ToLower();
		}
		PooledHashSet<string> val = Pool.Get<PooledHashSet<string>>();
		try
		{
			foreach (KeyValuePair<string, GroupData> groupdatum in groupdata)
			{
				GroupData value = groupdatum.Value;
				if (value.Perms.Count > 0 && value.Perms.Contains(perm))
				{
					((HashSet<string>)(object)val).Add(groupdatum.Key);
				}
			}
			if (((HashSet<string>)(object)val).Count == 0)
			{
				return Array.Empty<string>();
			}
			string[] array = new string[((HashSet<string>)(object)val).Count];
			int num = 0;
			foreach (string item in (HashSet<string>)(object)val)
			{
				array[num++] = item;
			}
			return array;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual void AddUserGroup(string id, string name, bool addIfNotExisting = false)
	{
		if (GroupExists(name) && GetUserData(id, addIfNotExisting).Groups.Add(name.ToLower()))
		{
			HookCaller.CallStaticHook(3116013984u, id, name);
		}
	}

	public virtual void RemoveUserGroup(string id, string name)
	{
		if (!StringEx.IsLower(name))
		{
			name = name.ToLower();
		}
		if (!GroupExists(name))
		{
			return;
		}
		UserData userData = GetUserData(id);
		if (name.Equals(StarStr))
		{
			if (userData.Groups.Count <= 0)
			{
				return;
			}
			foreach (string group in userData.Groups)
			{
				HookCaller.CallStaticHook(1018697706u, id, group);
			}
			userData.Groups.Clear();
		}
		else if (userData.Groups.Remove(name))
		{
			HookCaller.CallStaticHook(1018697706u, id, name);
		}
	}

	public virtual bool UserHasGroup(string id, string name)
	{
		if (string.IsNullOrEmpty(name) || !groupdata.ContainsKey(name))
		{
			return false;
		}
		HashSet<string> groups = GetUserData(id).Groups;
		if (groups.Count > 0)
		{
			return groups.Contains(name);
		}
		return false;
	}

	public virtual bool GroupExists(string groupName)
	{
		if (string.IsNullOrEmpty(groupName))
		{
			return false;
		}
		if (groupName.Length == 1 && groupName[0] == '*')
		{
			return true;
		}
		return groupdata.ContainsKey(groupName);
	}

	public virtual string[] GetGroups()
	{
		if (groupdata.Count != 0)
		{
			return groupdata.Keys.ToArray();
		}
		return Array.Empty<string>();
	}

	public virtual string[] GetUsersInGroup(string group)
	{
		if (!GroupExists(group))
		{
			return Array.Empty<string>();
		}
		if (!StringEx.IsLower(group))
		{
			group = group.ToLower();
		}
		PooledList<string> val = Pool.Get<PooledList<string>>();
		try
		{
			foreach (KeyValuePair<string, UserData> userdatum in userdata)
			{
				UserData value = userdatum.Value;
				if (value.Groups.Count > 0 && value.Groups.Contains(group))
				{
					((List<string>)(object)val).Add(userdatum.Key + " (" + value.LastSeenNickname + ")");
				}
			}
			if (((List<string>)(object)val).Count == 0)
			{
				return Array.Empty<string>();
			}
			string[] array = new string[((List<string>)(object)val).Count];
			for (int i = 0; i < ((List<string>)(object)val).Count; i++)
			{
				array[i] = ((List<string>)(object)val)[i];
			}
			return array;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual string GetGroupTitle(string group)
	{
		if (string.IsNullOrEmpty(group) || !groupdata.TryGetValue(group, out var value))
		{
			return string.Empty;
		}
		return value.Title;
	}

	public virtual int GetGroupRank(string group)
	{
		if (string.IsNullOrEmpty(group) || !groupdata.TryGetValue(group, out var value))
		{
			return 0;
		}
		return value.Rank;
	}

	public virtual string GetGroupParent(string group)
	{
		if (string.IsNullOrEmpty(group) || !groupdata.TryGetValue(group, out var value))
		{
			return string.Empty;
		}
		return value.ParentGroup;
	}

	public virtual bool GrantUserPermission(string id, string perm, BaseHookable owner)
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
			return GrantWildcard(userData.Perms, perm, owner, id, isUser: true);
		}
		if (!userData.Perms.Add(perm))
		{
			return false;
		}
		HookCaller.CallStaticHook(4054877424u, id, perm);
		return true;
	}

	public virtual bool RevokeUserPermission(string id, string perm)
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
			return RevokeWildcardUser(userData.Perms, perm, id);
		}
		if (!userData.Perms.Remove(perm))
		{
			return false;
		}
		HookCaller.CallStaticHook(1879829838u, id, perm);
		return true;
	}

	public virtual bool GrantGroupPermission(string name, string perm, BaseHookable owner)
	{
		if (!PermissionExists(perm, owner) || !GroupExists(name))
		{
			return false;
		}
		if (!StringEx.IsLower(name))
		{
			name = name.ToLower();
		}
		if (!groupdata.TryGetValue(name, out var value))
		{
			return false;
		}
		if (!StringEx.IsLower(perm))
		{
			perm = perm.ToLower();
		}
		if (perm.Length > 0 && perm[perm.Length - 1] == '*')
		{
			return GrantWildcard(value.Perms, perm, owner, name, isUser: false);
		}
		if (!value.Perms.Add(perm))
		{
			return false;
		}
		HookCaller.CallStaticHook(2479711677u, name, perm);
		return true;
	}

	public virtual bool RevokeGroupPermission(string name, string perm)
	{
		if (string.IsNullOrEmpty(perm) || string.IsNullOrEmpty(name))
		{
			return false;
		}
		if (!StringEx.IsLower(name))
		{
			name = name.ToLower();
		}
		if (!groupdata.TryGetValue(name, out var value))
		{
			return false;
		}
		if (value.Perms.Count == 0)
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
				foreach (string perm2 in value.Perms)
				{
					HookCaller.CallStaticHook(3443835039u, name, perm2);
				}
				value.Perms.Clear();
				return true;
			}
			return RevokeWildcardGroup(value.Perms, perm, name);
		}
		if (!value.Perms.Remove(perm))
		{
			return false;
		}
		HookCaller.CallStaticHook(3443835039u, name, perm);
		return true;
	}

	public virtual bool CreateGroup(string group, string title, int rank)
	{
		if (string.IsNullOrEmpty(group) || GroupExists(group))
		{
			return false;
		}
		GroupData value = new GroupData
		{
			Title = title,
			Rank = rank
		};
		if (!StringEx.IsLower(group))
		{
			group = group.ToLower();
		}
		groupdata.Add(group, value);
		HookCaller.CallStaticHook(1889097028u, group, title, rank);
		return true;
	}

	public virtual bool RemoveGroup(string group)
	{
		if (string.IsNullOrEmpty(group))
		{
			return false;
		}
		if (!StringEx.IsLower(group))
		{
			group = group.ToLower();
		}
		bool flag = groupdata.Remove(group);
		if (flag)
		{
			foreach (GroupData value in groupdata.Values)
			{
				if (value.ParentGroup == group)
				{
					value.ParentGroup = string.Empty;
				}
			}
		}
		bool flag2 = false;
		foreach (UserData value2 in userdata.Values)
		{
			if (value2.Groups.Count > 0 && value2.Groups.Remove(group))
			{
				flag2 = true;
			}
		}
		if (flag2)
		{
			SaveUsers();
		}
		if (flag)
		{
			HookCaller.CallStaticHook(3702696305u, group);
		}
		return flag;
	}

	public virtual bool SetGroupTitle(string group, string title)
	{
		if (string.IsNullOrEmpty(group))
		{
			return false;
		}
		if (!StringEx.IsLower(group))
		{
			group = group.ToLower();
		}
		if (!groupdata.TryGetValue(group, out var value))
		{
			return false;
		}
		if (value.Title == title)
		{
			return true;
		}
		value.Title = title;
		HookCaller.CallStaticHook(1035562059u, group, title);
		return true;
	}

	public virtual bool SetGroupRank(string group, int rank)
	{
		if (string.IsNullOrEmpty(group))
		{
			return false;
		}
		if (!StringEx.IsLower(group))
		{
			group = group.ToLower();
		}
		if (!groupdata.TryGetValue(group, out var value))
		{
			return false;
		}
		if (value.Rank == rank)
		{
			return true;
		}
		value.Rank = rank;
		HookCaller.CallStaticHook(407332709u, group, rank);
		return true;
	}

	public virtual bool SetGroupParent(string group, string parent)
	{
		if (string.IsNullOrEmpty(group))
		{
			return false;
		}
		if (!StringEx.IsLower(group))
		{
			group = group.ToLower();
		}
		if (!groupdata.TryGetValue(group, out var value))
		{
			return false;
		}
		if (string.IsNullOrEmpty(parent))
		{
			value.ParentGroup = null;
			return true;
		}
		if (!StringEx.IsLower(parent))
		{
			parent = parent.ToLower();
		}
		if (!groupdata.ContainsKey(parent) || group.Equals(parent))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(value.ParentGroup) && value.ParentGroup.Equals(parent))
		{
			return true;
		}
		if (HasCircularParent(group, parent))
		{
			return false;
		}
		value.ParentGroup = parent;
		HookCaller.CallStaticHook(3763369361u, group, parent);
		return true;
	}

	public virtual bool HasCircularParent(string group, string parent)
	{
		if (!groupdata.TryGetValue(parent, out var value))
		{
			return false;
		}
		PooledHashSet<string> val = Pool.Get<PooledHashSet<string>>();
		try
		{
			((HashSet<string>)(object)val).Add(group);
			((HashSet<string>)(object)val).Add(parent);
			while (!string.IsNullOrEmpty(value.ParentGroup))
			{
				if (!((HashSet<string>)(object)val).Add(value.ParentGroup))
				{
					return true;
				}
				if (!groupdata.TryGetValue(value.ParentGroup, out value))
				{
					return false;
				}
			}
			return false;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected internal static bool StartsWithPrefix(string stored, string permWithStar, int prefixLen)
	{
		if (prefixLen == 0)
		{
			return true;
		}
		if (stored.Length < prefixLen)
		{
			return false;
		}
		return string.CompareOrdinal(stored, 0, permWithStar, 0, prefixLen) == 0;
	}

	private bool RevokeWildcardUser(HashSet<string> perms, string perm, string id)
	{
		int prefixLen = perm.Length - 1;
		PooledList<string> val = Pool.Get<PooledList<string>>();
		try
		{
			foreach (string perm2 in perms)
			{
				if (StartsWithPrefix(perm2, perm, prefixLen))
				{
					((List<string>)(object)val).Add(perm2);
				}
			}
			if (((List<string>)(object)val).Count == 0)
			{
				return false;
			}
			for (int i = 0; i < ((List<string>)(object)val).Count; i++)
			{
				string text = ((List<string>)(object)val)[i];
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

	private bool RevokeWildcardGroup(HashSet<string> perms, string perm, string name)
	{
		int prefixLen = perm.Length - 1;
		PooledList<string> val = Pool.Get<PooledList<string>>();
		try
		{
			foreach (string perm2 in perms)
			{
				if (StartsWithPrefix(perm2, perm, prefixLen))
				{
					((List<string>)(object)val).Add(perm2);
				}
			}
			if (((List<string>)(object)val).Count == 0)
			{
				return false;
			}
			for (int i = 0; i < ((List<string>)(object)val).Count; i++)
			{
				string text = ((List<string>)(object)val)[i];
				if (perms.Remove(text))
				{
					HookCaller.CallStaticHook(3443835039u, name, text);
				}
			}
			return true;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void CollectGroupPermissions(string name, HashSet<string> output)
	{
		if (string.IsNullOrEmpty(name))
		{
			return;
		}
		string text = name;
		for (int i = 0; i < 32; i++)
		{
			if (!groupdata.TryGetValue(text, out var value))
			{
				break;
			}
			foreach (string perm in value.Perms)
			{
				output.Add(perm);
			}
			string parentGroup = value.ParentGroup;
			if (string.IsNullOrEmpty(parentGroup) || string.Equals(parentGroup, text, StringComparison.OrdinalIgnoreCase))
			{
				break;
			}
			text = parentGroup;
		}
	}

	private bool GrantWildcard(HashSet<string> target, string perm, BaseHookable owner, string subjectKey, bool isUser)
	{
		uint hookId = (isUser ? 4054877424u : 2479711677u);
		bool result = false;
		if (perm.Length == 1)
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
						if (target.Add(item2))
						{
							result = true;
							HookCaller.CallStaticHook(hookId, subjectKey, item2);
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
					if (target.Add(item3))
					{
						result = true;
						HookCaller.CallStaticHook(hookId, subjectKey, item3);
					}
				}
			}
			return result;
		}
		int prefixLen = perm.Length - 1;
		if (owner == null)
		{
			foreach (KeyValuePair<BaseHookable, HashSet<string>> item4 in permset)
			{
				HashSet<string> value3 = item4.Value;
				if (value3.Count == 0)
				{
					continue;
				}
				foreach (string item5 in value3)
				{
					if (StartsWithPrefix(item5, perm, prefixLen) && target.Add(item5))
					{
						result = true;
						HookCaller.CallStaticHook(hookId, subjectKey, item5);
					}
				}
			}
		}
		else
		{
			if (!permset.TryGetValue(owner, out var value4) || value4.Count == 0)
			{
				return false;
			}
			foreach (string item6 in value4)
			{
				if (StartsWithPrefix(item6, perm, prefixLen) && target.Add(item6))
				{
					result = true;
					HookCaller.CallStaticHook(hookId, subjectKey, item6);
				}
			}
		}
		return result;
	}
}
