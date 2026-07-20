namespace API.Permissions;

public interface IPermissionManager
{
	IUserManagement Users { get; }

	IGroupManagement Groups { get; }
}
