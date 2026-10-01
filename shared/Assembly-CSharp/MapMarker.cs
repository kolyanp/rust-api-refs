using System.Collections.Generic;
using CompanionServer;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class MapMarker : BaseEntity
{
	public enum ClusterType
	{
		None,
		Vending
	}

	public AppMarkerType appType;

	public GameObjectRef markerObj;

	public static readonly List<MapMarker> serverMapMarkers = new List<MapMarker>();

	public override void InitShared()
	{
		if (isServer && !serverMapMarkers.Contains(this))
		{
			serverMapMarkers.Add(this);
		}
		base.InitShared();
	}

	public override void DestroyShared()
	{
		if (isServer)
		{
			serverMapMarkers.Remove(this);
		}
		base.DestroyShared();
	}

	public virtual AppMarker GetAppMarkerData()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		AppMarker val = Pool.Get<AppMarker>();
		Vector2 val2 = CompanionServer.Util.WorldToMap(((Component)this).transform.position);
		val.id = net.ID;
		val.type = appType;
		val.x = val2.x;
		val.y = val2.y;
		return val;
	}
}
