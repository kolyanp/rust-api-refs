using UnityEngine;

public class LodLevelDisplay : MonoBehaviour, IEditorComponent
{
	public Color TextColor;

	[Range(1f, 6f)]
	public float TextScaleMultiplier;

	public LodLevelDisplay()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		TextColor = Color.green;
		TextScaleMultiplier = 1f;
		((MonoBehaviour)this)._002Ector();
	}
}
