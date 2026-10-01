using UnityEngine;

public class MapMarkerMLRSRocket : MapMarker
{
	public RectTransform uiMarkerTransform;

	public override void ServerInit()
	{
		base.ServerInit();
		limitNetworking = true;
	}

	public override bool ShouldNetworkTo(BasePlayer player)
	{
		return (ulong)player.userID == OwnerID;
	}
}
