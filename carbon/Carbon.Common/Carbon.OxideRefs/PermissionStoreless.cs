using System.Collections.Generic;
using Carbon.Extensions;
using Oxide.Core.Libraries;

namespace Carbon.OxideRefs;

public class PermissionStoreless : Permission
{
	public override void SaveData()
	{
	}

	public override void LoadFromDatafile()
	{
		userdata = new Dictionary<string, UserData>();
		groupdata = new Dictionary<string, GroupData>();
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
		base.IsLoaded = true;
	}
}
