public class MapMarkerPet : MapMarker
{
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
