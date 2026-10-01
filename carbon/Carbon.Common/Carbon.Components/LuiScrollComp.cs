using UnityEngine;

namespace Carbon.Components;

public class LuiScrollComp : LuiCompBase
{
	public LuiPosition anchor = LuiPosition.Full;

	public LuiOffset offset = LuiOffset.None;

	public Vector2 pivot = LUI.defaultPivot;

	public bool horizontal;

	public bool vertical;

	public string movementType;

	public float elasticity = -1f;

	public bool inertia;

	public float decelerationRate = -1f;

	public float scrollSensitivity = -1f;

	public LuiScrollbar horizontalScrollbar;

	public LuiScrollbar verticalScrollbar;

	public float horizontalNormalizedPosition;

	public float verticalNormalizedPosition;

	public LuiScrollComp()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		type = LuiCompType.ScrollView;
	}
}
