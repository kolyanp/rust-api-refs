using System;

public class RoomVisionBlocker : PrefabAttribute
{
	public bool AlwaysClosed;

	protected override Type GetIndexedType()
	{
		return typeof(RoomVisionBlocker);
	}
}
