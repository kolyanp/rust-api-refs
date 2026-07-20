public class SocketMod_EnvironmentVolume : SocketMod
{
	[InspectorFlags]
	public EnvironmentType environmentNone;

	public override bool DoCheck(ref Construction.Placement place)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		EnvironmentType environmentType = EnvironmentManager.Get(place.position + place.rotation * worldPosition);
		if (environmentNone != 0 && (environmentType & environmentNone) != 0)
		{
			EnvironmentType blockedTypes = environmentType & environmentNone;
			Construction.lastPlacementError = Phrase.op_Implicit(GetErrorMessage(blockedTypes));
			return false;
		}
		return true;
	}

	private string GetErrorMessage(EnvironmentType blockedTypes)
	{
		string text = "Can't be placed ";
		if ((blockedTypes & EnvironmentType.TrainTunnels) == EnvironmentType.TrainTunnels)
		{
			return text + "In Train Tunnels";
		}
		if ((blockedTypes & EnvironmentType.Underground) == EnvironmentType.Underground)
		{
			return text + "Underground";
		}
		if ((blockedTypes & EnvironmentType.NoSunlight) == EnvironmentType.NoSunlight)
		{
			return text + "In The Dark";
		}
		if ((blockedTypes & EnvironmentType.Submarine) == EnvironmentType.Submarine)
		{
			return text + "In A Submarine";
		}
		if ((blockedTypes & EnvironmentType.Outdoor) == EnvironmentType.Outdoor)
		{
			return text + "Outdoors";
		}
		if ((blockedTypes & EnvironmentType.PlayerConstruction) == EnvironmentType.PlayerConstruction)
		{
			return text + "Near Player Construction";
		}
		if ((blockedTypes & EnvironmentType.UnderwaterLab) == EnvironmentType.UnderwaterLab)
		{
			return text + "In Underwater Labs";
		}
		if ((blockedTypes & EnvironmentType.Elevator) == EnvironmentType.Elevator)
		{
			return text + "Near Elevators";
		}
		if ((blockedTypes & EnvironmentType.Building) == EnvironmentType.Building || (blockedTypes & EnvironmentType.BuildingDark) == EnvironmentType.BuildingDark || (blockedTypes & EnvironmentType.BuildingVeryDark) == EnvironmentType.BuildingVeryDark)
		{
			return text + "In A Building";
		}
		return text + "(Unknown Environment)";
	}
}
