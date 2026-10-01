using System;
using System.Net;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Rust;
using Rust.Platform.Common;
using UnityEngine;

public class RustPlatformHooks : IPlatformHooks
{
	public static readonly RustPlatformHooks Instance = new RustPlatformHooks();

	public uint SteamAppId => Rust.Defines.appID;

	public ServerParameters? ServerParameters
	{
		get
		{
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			if (Net.sv == null)
			{
				return null;
			}
			if ((Object)(object)SingletonComponent<ServerMgr>.Instance == (Object)null || !SingletonComponent<ServerMgr>.Instance.NetworkPortsConfigured)
			{
				return null;
			}
			IPAddress iPAddress = null;
			if (!string.IsNullOrEmpty(ConVar.Server.ip))
			{
				iPAddress = IPAddress.Parse(ConVar.Server.ip);
			}
			return new ServerParameters("rust", "Rust", 2634.ToString(), Net.sv.secure, CommandLine.HasSwitch("-sdrnet"), iPAddress, (ushort)Net.sv.port, (ushort)ResolveQueryPort());
		}
	}

	public void Abort()
	{
		Application.Quit();
	}

	public void OnItemDefinitionsChanged()
	{
		SteamInventory.InvalidateWorkshopSkinCaches();
		ItemManager.InvalidateWorkshopSkinCache();
	}

	private static int ResolveQueryPort()
	{
		if (ConVar.Server.queryport > 0 && ConVar.Server.queryport != ConVar.Server.port)
		{
			return ConVar.Server.queryport;
		}
		return Math.Max(ConVar.Server.port, RCon.Port) + 1;
	}

	public void AuthSessionValidated(ulong userId, ulong ownerUserId, AuthResponse response, string rawResponse)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Analytics.Azure.OnSteamAuth(userId, ownerUserId, rawResponse);
		SingletonComponent<ServerMgr>.Instance.OnValidateAuthTicketResponse(userId, ownerUserId, response);
	}
}
