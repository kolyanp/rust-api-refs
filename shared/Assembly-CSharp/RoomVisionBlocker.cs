using System;

public class RoomVisionBlocker : PrefabAttribute
{
	protected override Type GetIndexedType()
	{
		return typeof(RoomVisionBlocker);
	}
}
