namespace API.Permissions;

public interface IUserManagement
{
	void Insert(string steamID, string nickname, string language);

	void Remove(string steamID);

	string GetPlayerName(string steamID);

	string GetPlayerName(ulong steamID);

	string GetPlayerLanguage(string steamID);

	string GetPlayerLanguage(ulong steamID);

	void AddUserPermission(string steamID, string permission);

	void RemoveUserPermission(string steamID, string permission);

	void ResetPermissions(string steamID);

	void AddToGroup(string steamID, string groupID);

	void RemoveFromGroup(string steamID, string groupID);
}
