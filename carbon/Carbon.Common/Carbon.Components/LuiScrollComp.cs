using UnityEngine;

namespace Carbon.Components;

public class LuiScrollComp : LuiCompBase
{
	public LuiPosition anchor;

	public LuiOffset offset;

	public Vector2 pivot;

	public bool horizontal;

	public bool vertical;

	public string movementType;

	public float elasticity;

	public bool inertia;

	public float decelerationRate;

	public float scrollSensitivity;

	public LuiScrollbar horizontalScrollbar;

	public LuiScrollbar verticalScrollbar;

	public float horizontalNormalizedPosition;

	public float verticalNormalizedPosition;

	public LuiScrollComp()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		anchor = LuiPosition.Full;
		offset = LuiOffset.None;
		pivot = LUI.defaultPivot;
		elasticity = -1f;
		decelerationRate = -1f;
		scrollSensitivity = -1f;
		base._002Ector();
		type = LuiCompType.ScrollView;
	}
}
