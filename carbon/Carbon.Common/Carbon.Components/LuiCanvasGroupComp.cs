using UnityEngine;

namespace Carbon.Components;

public class LuiCanvasGroupComp : LuiCompBase
{
	public float alpha;

	public bool blocksRaycasts;

	public bool interactable;

	public Vector2 fade;

	public LuiCanvasGroupComp()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		alpha = -1f;
		blocksRaycasts = true;
		interactable = true;
		fade = LUI.defaultFade;
		base._002Ector();
		type = LuiCompType.CanvasGroup;
	}
}
