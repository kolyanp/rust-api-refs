using System.Collections.Generic;
using System.Threading.Tasks;
using ConVar;
using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer.Handlers;

public class MapMarkers : BasePlayerHandler<AppEmpty>
{
	public override ValueTask Execute()
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		if (!ConVar.Server.mapenabled || ConVar.Server.fogofwar)
		{
			SendError("no_map");
			return default(ValueTask);
		}
		AppMapMarkers val = Pool.Get<AppMapMarkers>();
		val.markers = Pool.Get<List<AppMarker>>();
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(base.UserId);
		if (playerTeam != null)
		{
			foreach (ulong member in playerTeam.members)
			{
				BasePlayer basePlayer = RelationshipManager.FindByID(member);
				if (!((Object)(object)basePlayer == (Object)null))
				{
					val.markers.Add(GetPlayerMarker(basePlayer));
				}
			}
		}
		else if ((Object)(object)base.Player != (Object)null)
		{
			val.markers.Add(GetPlayerMarker(base.Player));
		}
		foreach (MapMarker serverMapMarker in MapMarker.serverMapMarkers)
		{
			if ((int)serverMapMarker.appType != 0)
			{
				val.markers.Add(serverMapMarker.GetAppMarkerData());
			}
		}
		AppResponse val2 = Pool.Get<AppResponse>();
		val2.mapMarkers = val;
		Send(val2);
		return default(ValueTask);
	}

	private static AppMarker GetPlayerMarker(BasePlayer player)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		AppMarker obj = Pool.Get<AppMarker>();
		Vector2 val = Util.WorldToMap(((Component)player).transform.position);
		obj.id = player.net.ID;
		obj.type = (AppMarkerType)1;
		obj.x = val.x;
		obj.y = val.y;
		obj.steamId = player.userID;
		return obj;
	}
}
