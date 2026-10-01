using UnityEngine;

public class LodLevelDisplay : MonoBehaviour, IEditorComponent
{
	public Color TextColor = Color.green;

	[Range(1f, 6f)]
	public float TextScaleMultiplier = 1f;

	public LodLevelDisplay()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
