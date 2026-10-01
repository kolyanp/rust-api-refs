using UnityEngine;

namespace Carbon.Components;

public class LuiCanvasGroupComp : LuiCompBase
{
	public float alpha = -1f;

	public bool blocksRaycasts = true;

	public bool interactable = true;

	public Vector2 fade = LUI.defaultFade;

	public LuiCanvasGroupComp()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		type = LuiCompType.CanvasGroup;
	}
}
