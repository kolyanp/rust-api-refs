using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;

public static class Auth_Steam
{
	internal static List<Connection> waitingList = new List<Connection>();

	public static IEnumerator Run(Connection connection)
	{
		connection.authStatusSteam = string.Empty;
		if (!connection.active || connection.rejected)
		{
			yield break;
		}
		if (!PlatformService.Instance.BeginPlayerSession(connection.userid, connection.token))
		{
			ConnectionAuth.Reject(connection, "Steam Auth Failed");
			yield break;
		}
		waitingList.Add(connection);
		Stopwatch timeout = Stopwatch.StartNew();
		while (timeout.Elapsed.TotalSeconds < 30.0 && connection.active && !(connection.authStatusSteam != string.Empty))
		{
			yield return null;
		}
		waitingList.Remove(connection);
		if (connection.active)
		{
			if (connection.authStatusSteam.Length == 0)
			{
				ConnectionAuth.Reject(connection, "Steam Auth Timeout: No auth response");
				PlatformService.Instance.EndPlayerSession(connection.userid);
			}
			else if (connection.authStatusSteam == "banned")
			{
				ConnectionAuth.Reject(connection, "Steam Auth: " + connection.authStatusSteam);
				PlatformService.Instance.EndPlayerSession(connection.userid);
			}
			else if (connection.authStatusSteam == "gamebanned")
			{
				ConnectionAuth.Reject(connection, "Steam Auth: " + connection.authStatusSteam);
				PlatformService.Instance.EndPlayerSession(connection.userid);
			}
			else if (connection.authStatusSteam == "vacbanned")
			{
				ConnectionAuth.Reject(connection, "Steam Auth: " + connection.authStatusSteam);
				PlatformService.Instance.EndPlayerSession(connection.userid);
			}
			else if (connection.authStatusSteam != "ok")
			{
				ConnectionAuth.Reject(connection, "Steam Auth Failed", "Steam Auth Error: " + connection.authStatusSteam);
				PlatformService.Instance.EndPlayerSession(connection.userid);
			}
			else
			{
				string text = (ConVar.Server.censorplayerlist ? RandomUsernames.Get(connection.userid + (ulong)Random.Range(0, 100000)) : connection.username);
				PlatformService.Instance.UpdatePlayerSession(connection.userid, text);
			}
		}
	}

	public unsafe static bool ValidateConnecting(ulong steamid, ulong ownerSteamID, AuthResponse response)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Invalid comparison between Unknown and I4
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Invalid comparison between Unknown and I4
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Invalid comparison between Unknown and I4
		Connection connection = waitingList.Find((Connection x) => x.userid == steamid);
		if (connection == null)
		{
			return false;
		}
		connection.ownerid = ownerSteamID;
		if (ServerUsers.Is(ownerSteamID, ServerUsers.UserGroup.Banned) || ServerUsers.Is(steamid, ServerUsers.UserGroup.Banned))
		{
			connection.authStatusSteam = "banned";
			return true;
		}
		if ((int)response == 2)
		{
			connection.authStatusSteam = "ok";
			return true;
		}
		if ((int)response == 3)
		{
			connection.authStatusSteam = "vacbanned";
			return true;
		}
		if ((int)response == 4)
		{
			connection.authStatusSteam = "gamebanned";
			return true;
		}
		if ((int)response == 1 && !ConVar.Server.strictauth_steam && connection.authLevel == 0 && connection.os != "editor")
		{
			Debug.LogWarning((object)("Steam Auth Timeout: AuthResponse.TimedOut for " + steamid + " / " + ownerSteamID + " - bypassing since strictauth_steam is false"));
			connection.authStatusSteam = "ok";
			return true;
		}
		connection.authStatusSteam = ((object)(*(AuthResponse*)(&response))/*cast due to constrained. prefix*/).ToString();
		return true;
	}
}
