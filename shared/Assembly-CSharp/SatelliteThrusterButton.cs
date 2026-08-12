using Rust.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SatelliteThrusterButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public RustText labelText;

	public Button button;

	[Header("Colours")]
	public Color normalColor;

	public Color firedColor;

	public Color disabledColor;

	[Header("Hover")]
	public GameObject hoverFill;

	public Color hoverLabelColor;

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	public SatelliteThrusterButton()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		normalColor = Color.white;
		firedColor = new Color(0.1f, 1f, 0.2f, 1f);
		disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
		hoverLabelColor = new Color(0.12668449f, 0.12668449f, 0.12668449f, 1f);
		((MonoBehaviour)this)._002Ector();
	}
}
