using UnityEngine;

namespace Carbon.Components;

public class LuiGridLayoutGroupComp : LuiCompBase
{
	public Vector2 cellSize = LUI.defaultCellSize;

	public Vector2 spacing;

	public string startCorner;

	public string startAxis;

	public string childAlignment;

	public string constraint;

	public int constraintCount;

	public string padding;

	public LuiGridLayoutGroupComp()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		type = LuiCompType.GridLayoutGroup;
	}
}
