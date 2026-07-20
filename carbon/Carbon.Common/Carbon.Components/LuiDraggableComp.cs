using UnityEngine;

namespace Carbon.Components;

public class LuiDraggableComp : LuiCompBase
{
	public bool limitToParent;

	public float maxDistance;

	public bool allowSwapping;

	public bool dropAnywhere = true;

	public float dragAlpha = -1f;

	public int parentLimitIndex = -1;

	public string filter;

	public Vector2 parentPadding;

	public Vector2 anchorOffset;

	public bool keepOnTop;

	public string positionRPC;

	public bool moveToAnchor;

	public bool rebuildAnchor;

	public LuiDraggableComp()
	{
		type = LuiCompType.Draggable;
	}
}
