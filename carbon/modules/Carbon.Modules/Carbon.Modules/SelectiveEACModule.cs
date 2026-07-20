using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Carbon.Base;
using ConVar;
using HarmonyLib;
using JetBrains.Annotations;
using Network;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;

namespace Carbon.Modules;

public class SelectiveEACModule : CarbonModule<SelectiveEACConfig, EmptyModuleData>
{
	[AutoPatch]
	[UsedImplicitly]
	[HarmonyPatch(typeof(EACServer), "OnJoinGame")]
	private class EACServer_OnJoinGame
	{
		[UsedImplicitly]
		private static bool Prefix(Connection connection)
		{
			try
			{
				if (CanBypass(connection))
				{
					EACServer.OnAuthenticatedLocal(connection);
					EACServer.OnAuthenticatedRemote(connection);
					return false;
				}
			}
			catch (Exception ex)
			{
				Logger.Error((object)"EACServer.OnJoinGame CanBypass failure", ex);
			}
			return true;
		}
	}

	[AutoPatch]
	[UsedImplicitly]
	[HarmonyPatch(typeof(ServerMgr), "JoinGame")]
	private class ServerMgr_JoinGame
	{
		[UsedImplicitly]
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> op)
		{
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Expected O, but got Unknown
			List<CodeInstruction> list = new List<CodeInstruction>(op);
			for (int i = 0; i < list.Count; i++)
			{
				CodeInstruction val = list[i];
				if (val.opcode == OpCodes.Ldfld && val.operand is FieldInfo { Name: "encryption" } fieldInfo)
				{
					Type declaringType = fieldInfo.DeclaringType;
					if ((object)declaringType != null && declaringType.Name == "Server")
					{
						val.opcode = OpCodes.Ldarg_1;
						val.operand = null;
						list.Insert(i + 1, new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(SelectiveEACModule), "UserEncryptionOverride", (Type[])null, (Type[])null)));
						i++;
					}
				}
			}
			return list;
		}
	}

	public static readonly int DefaultEncryption = 1;

	internal static SelectiveEACModule Singleton { get; set; }

	public override string Name => "SelectiveEAC";

	public override VersionNumber Version => new VersionNumber(1, 0, 0);

	public override Type Type => typeof(SelectiveEACModule);

	public override bool ForceModded => false;

	public SelectiveEACModule()
	{
		Singleton = this;
	}

	public override void OnServerInit(bool initial)
	{
		base.OnServerInit(initial);
		if (initial)
		{
			((CarbonModule<SelectiveEACConfig, EmptyModuleData>)this).OnEnabled(true);
		}
	}

	public override void OnEnabled(bool initialized)
	{
		base.OnEnabled(initialized);
		if (initialized)
		{
			base.Permissions.UnregisterPermissions((BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.UsePermission, (BaseHookable)(object)this);
			if (!base.Permissions.GroupExists(base.ConfigInstance.UseGroup))
			{
				base.Permissions.CreateGroup(base.ConfigInstance.UseGroup, "Selective EAC", 0);
			}
		}
	}

	private static bool CanBypass(Connection connection)
	{
		string text = connection.userid.ToString();
		Permission permission = ((Plugin)Community.Runtime.Core).permission;
		if (!permission.UserExists(text))
		{
			permission.GetUserData(text, true);
			if (Community.Runtime.Config.Permissions.AutoGrantPlayerGroup && !string.IsNullOrEmpty(Community.Runtime.Config.Permissions.PlayerDefaultGroup))
			{
				permission.AddUserGroup(text, Community.Runtime.Config.Permissions.PlayerDefaultGroup, false);
			}
		}
		if (!((Plugin)Community.Runtime.Core).permission.UserHasPermission(text, ((CarbonModule<SelectiveEACConfig, EmptyModuleData>)Singleton).ConfigInstance.UsePermission))
		{
			return ((Plugin)Community.Runtime.Core).permission.UserHasGroup(text, ((CarbonModule<SelectiveEACConfig, EmptyModuleData>)Singleton).ConfigInstance.UseGroup);
		}
		return true;
	}

	private static int UserEncryptionOverride(Server sv, Connection connection)
	{
		try
		{
			if (CanBypass(connection))
			{
				return DefaultEncryption;
			}
		}
		catch (Exception ex)
		{
			Logger.Error((object)"Failed getting UserEncryptionOverride", ex);
		}
		return Server.encryption;
	}
}
