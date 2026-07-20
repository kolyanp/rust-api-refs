using UnityEngine;

namespace Carbon.Components;

public class LuiOutlineComp : LuiCompBase
{
	public string color;

	public Vector2 distance;

	public bool useGraphicAlpha;

	public LuiOutlineComp()
	{
		type = LuiCompType.Outline;
	}
}
