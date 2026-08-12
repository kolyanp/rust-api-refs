using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISatelliteListEntry : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Labels")]
	public TextMeshProUGUI textName;

	public TextMeshProUGUI textPayload;

	public TextMeshProUGUI textMass;

	public TextMeshProUGUI textFuel;

	public TextMeshProUGUI textSize;

	[Header("Interaction")]
	public Button button;

	[Header("Hover")]
	public Color panelColor;

	public Color textColor;

	public Image rowFill;

	public Image selectFrame;

	public void Init(SatelliteData sat, int index, SatelliteMenuUI menu)
	{
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	public UISatelliteListEntry()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		panelColor = new Color(0.85490197f, 10f / 51f, 0.05490196f);
		textColor = new Color(11f / 51f, 0.08627451f, 1f / 17f);
		((MonoBehaviour)this)._002Ector();
	}
}
