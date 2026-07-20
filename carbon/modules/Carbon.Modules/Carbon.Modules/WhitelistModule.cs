using System;
using System.Collections.Generic;
using System.Linq;
using Carbon.Base;
using Carbon.Pooling;
using Network;
using Oxide.Core;
using Oxide.Core.Plugins;

namespace Carbon.Modules;

public class WhitelistModule : CarbonModule<WhitelistConfig, EmptyModuleData>
{
	internal static WhitelistModule Singleton { get; set; }

	public override string Name => "Whitelist";

	public override VersionNumber Version => new VersionNumber(1, 0, 0);

	public override Type Type => typeof(WhitelistModule);

	public override bool ForceModded => false;

	public WhitelistModule()
	{
		Singleton = this;
	}

	public override void OnServerInit(bool initial)
	{
		base.OnServerInit(initial);
		if (initial)
		{
			((CarbonModule<WhitelistConfig, EmptyModuleData>)this).OnEnabled(true);
		}
	}

	public override void OnEnabled(bool initialized)
	{
		base.OnEnabled(initialized);
		((BaseHookable)this).Subscribe("CanUserLogin");
		if (initialized)
		{
			base.Permissions.UnregisterPermissions((BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.BypassPermission, (BaseHookable)(object)this);
			if (!base.Permissions.GroupExists(base.ConfigInstance.BypassGroup))
			{
				base.Permissions.CreateGroup(base.ConfigInstance.BypassGroup, "Whitelisted", 0);
			}
		}
	}

	public override void OnDisabled(bool initialized)
	{
		base.OnDisabled(initialized);
		((BaseHookable)this).Unsubscribe("CanUserLogin");
	}

	public override Dictionary<string, Dictionary<string, string>> GetDefaultPhrases()
	{
		return new Dictionary<string, Dictionary<string, string>> { ["en"] = new Dictionary<string, string> { ["denied"] = "Not whitelisted" } };
	}

	private object CanUserLogin(string name, string id, string ipAddress)
	{
		Connection connection = Net.sv.connections.FirstOrDefault((Connection x) => x.userid.ToString() == id);
		if (connection.authLevel >= 2 || CanBypass(id))
		{
			return null;
		}
		ConsoleNetwork.SendClientCommand(connection, "echo " + ((CarbonModule<WhitelistConfig, EmptyModuleData>)this).GetPhrase("denied", id), Array.Empty<object>());
		((Plugin)Community.Runtime.Core).NextTick((Action)delegate
		{
			ConnectionAuth.Reject(connection, ((CarbonModule<WhitelistConfig, EmptyModuleData>)this).GetPhrase("denied", id), (string)null);
		});
		return null;
	}

	public bool CanBypass(string playerId)
	{
		if (!base.Permissions.UserExists(playerId))
		{
			base.Permissions.GetUserData(playerId, true);
			if (Community.Runtime.Config.Permissions.AutoGrantPlayerGroup && !string.IsNullOrEmpty(Community.Runtime.Config.Permissions.PlayerDefaultGroup))
			{
				base.Permissions.AddUserGroup(playerId, Community.Runtime.Config.Permissions.PlayerDefaultGroup, false);
			}
		}
		if (!base.Permissions.UserHasPermission(playerId, base.ConfigInstance.BypassPermission))
		{
			if (!string.IsNullOrEmpty(base.ConfigInstance.BypassGroup))
			{
				return base.Permissions.UserHasGroup(playerId, base.ConfigInstance.BypassGroup);
			}
			return false;
		}
		return true;
	}

	public override object InternalCallHook(uint hook, object[] args)
	{
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		int? num = args?.Length;
		object obj = ((num > 0) ? args[0] : null);
		object obj2 = ((num > 1) ? args[1] : null);
		object obj3 = ((num > 2) ? args[2] : null);
		try
		{
			if (hook == 1045800646)
			{
				bool flag = ((obj is string || obj == null) ? true : false);
				bool flag2 = flag;
				string name = (flag2 ? ((string)(obj ?? null)) : null);
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag3 = flag;
				string id = (flag3 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is string || obj3 == null) ? true : false);
				bool flag4 = flag;
				string ipAddress = (flag4 ? ((string)(obj3 ?? null)) : null);
				if (flag2 && flag3 && flag4)
				{
					return CanUserLogin(name, id, ipAddress);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error((object)string.Format("Failed to call internal hook '{0}' on module '{1} v{2}' [{3}]", new object[4]
			{
				HookStringPool.GetOrAdd(hook),
				((CarbonModule<WhitelistConfig, EmptyModuleData>)this).Name,
				((BaseHookable)this).Version,
				hook
			}), ex);
			((BaseHookable)this).OnException(hook);
		}
		return null;
	}
}
