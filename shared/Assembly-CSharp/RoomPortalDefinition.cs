using System;

public class RoomPortalDefinition : PrefabAttribute
{
	public string[] FillerSocketNames;

	protected override Type GetIndexedType()
	{
		return typeof(RoomPortalDefinition);
	}
}
