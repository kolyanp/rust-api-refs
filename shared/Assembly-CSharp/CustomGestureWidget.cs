using UnityEngine;
using UnityEngine.UI;

public class CustomGestureWidget : MonoBehaviour, IClientComponent
{
	public PieShape Shape;

	public Image GestureIcon;

	public Color HighlightedColor;

	public Color NeutralColor;

	public CustomGestureWidget()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		HighlightedColor = Color.red;
		NeutralColor = Color.white;
		((MonoBehaviour)this)._002Ector();
	}
}
