namespace API.Permissions;

public interface IGroupManagement
{
	void Insert(string groupID, string title, int rank, string parent);

	void Remove(string steamID);

	string GetGroupTitle(string groupID);

	int GetGroupRank(string groupID);

	string GetGroupParent(string groupID);

	void AddGroupPermission(string groupID, string permission);

	void RemoveGroupPermission(string groupID, string permission);

	void ResetPermissions(string groupID);
}
